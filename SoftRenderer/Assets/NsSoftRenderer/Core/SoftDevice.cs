using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;


using RenderTargetClearFlags = System.Int32;

namespace NsSoftRenderer {

    class CameraSort: IComparer<SoftCamera> {
        public int Compare(SoftCamera x, SoftCamera y) {
            int ret = x.Depth - y.Depth;
            return ret;
        }
    }

    // 软渲染设备
    public class SoftDevice: DisposeObject, ISoftCameraLinker {
        private static SoftDevice m_StaticDevice = null;
        private RenderTarget m_RenerTarget = null;
        // 软渲染摄像机列表（按照深度排序）
        private List<SoftCamera> m_CamList = null;
        private Dictionary<int, SoftRenderObject> m_RenderObjMap = null;
        private bool m_CamListChanged = true;
        private IComparer<SoftCamera> m_CamSortFunc = new CameraSort();
        // 默认渲染管线
        private IRenderPipline m_RenderPipline = new DefaultRenderPipline();

        public RenderTarget Target {
            get {
                return m_RenerTarget;
            }
        }

        public SoftRenderObject GetRenderObject(int instanceId) {
            if (m_RenderObjMap != null) {
                SoftRenderObject ret;
                if (!m_RenderObjMap.TryGetValue(instanceId, out ret))
                    ret = null;
                return ret;
            }
            return null;
        }

        public static SoftDevice StaticDevice {
            get {
                return m_StaticDevice;
            }
        }

        public void RemoveRenderObject(SoftRenderObject obj) {
            if (obj != null && m_RenderObjMap != null) {
                int instanceId = obj.InstanceId;
                m_RenderObjMap.Remove(instanceId);

                SoftCamera cam = obj as SoftCamera;
                if (cam != null) {
                    m_CamList.Remove(cam);
                }

                obj.Dispose();
            }
        }

        private void OnCamListChanged() {
            m_CamListChanged = true;
        }

        protected int RegisterRenderObject(SoftRenderObject obj) {
            if (obj == null)
                return -1;
            if (m_RenderObjMap == null)
                m_RenderObjMap = new Dictionary<RenderTargetClearFlags, SoftRenderObject>();
            if (m_RenderObjMap.ContainsKey(obj.InstanceId))
                return 0;
            m_RenderObjMap.Add(obj.InstanceId, obj);
            return 1;
        }

        protected bool AddCamera(SoftCamera cam) {
            if (cam == null)
                return false;
            if (m_CamList == null)
                m_CamList = new List<SoftCamera>();
            if (RegisterRenderObject(cam) > 0)
                m_CamList.Add(cam);
            return true;
        }

        public SoftCamera AddPCamera(PCameraInfo info, Vector3 pos, Vector3 up, Vector3 lookAt, int depth, bool isMainCamera = false) {
            SoftCamera cam = new SoftCamera(this);
            cam.Position = pos;
            cam.Up = up;
            cam.LookAt = lookAt;
            cam.Depth = depth;
            cam.SetPCamera(info);
            if (AddCamera(cam)) {
                cam.IsMainCamera = isMainCamera;
                return cam;
            }

            return null;
        }

        // 添加UNITY摄影机
        public SoftCamera AddCamera(UnityEngine.Camera cam, bool isMainCamera) {
            if (cam != null) {
                if (cam.orthographic) {
                    OCameraInfo info = OCameraInfo.Create();
                    info.Size = cam.orthographicSize;
                    info.nearPlane = cam.nearClipPlane;
                    info.farPlane = cam.farClipPlane;
                    var trans = cam.transform;
                    return AddOCamera(info, trans.position, trans.up, trans.forward, (int)cam.depth, isMainCamera);
                } else {
                    PCameraInfo info = PCameraInfo.Create();
                    info.nearPlane = cam.nearClipPlane;
                    info.farPlane = cam.farClipPlane;
                    info.fieldOfView = cam.fieldOfView;
                    var trans = cam.transform;
                    return AddPCamera(info, trans.position, trans.up, trans.forward, (int)cam.depth, isMainCamera);
                }
            }
            return null;
        }

        public SoftMeshRenderer AddMeshRenderer(Vector3 pos, Vector3 up, Vector3 lookAt, Mesh mesh) {
            SoftMeshRenderer ret = new SoftMeshRenderer(pos, up, lookAt, mesh);
            if (RegisterRenderObject(ret) > 0)
                return ret;
            return null;
        }

        // 添加
        public SoftCamera AddOCamera(OCameraInfo info, Vector3 pos, Vector3 up, Vector3 lookAt, int depth, bool isMainCamera = false) {
            SoftCamera cam = new SoftCamera(this);
            cam.Position = pos;
            cam.Up = up;
            cam.LookAt = lookAt;
            cam.Depth = depth;
            cam.SetOCamera(info);
            if (AddCamera(cam)) {
                cam.IsMainCamera = isMainCamera;
                return cam;
            }
            return null;
        }

        public void OnCameraDepthChanged() {
            OnCamListChanged();
        }

        private void SortCamList() {
            if (m_CamListChanged && m_CamList != null) {
                // 排序
                m_CamListChanged = false;
                m_CamList.Sort(m_CamSortFunc);
            }
        }

        public SoftDevice(int deviceWidth, int deviceHeight) {
            m_RenerTarget = new RenderTarget(deviceWidth, deviceHeight);
            m_StaticDevice = this;
        }

        public Color ClearColor {
            get {
                if (m_RenerTarget != null)
                    return m_RenerTarget.CleanColor;
                return Color.clear;
            }

            set {
                if (m_RenerTarget.CleanColor != value) {
                    m_RenerTarget.CleanColor = value;
                }
            }
        }


        public int DeviceWidth {
            get {
                if (m_RenerTarget == null)
                    return 0;
                return m_RenerTarget.Width;
            }
        }


        public int DeviceHeight {
            get {
                if (m_RenerTarget == null)
                    return 0;
                return m_RenerTarget.Height;
            }
        }

        private void CameraRender(SoftCamera cam) {
            if (cam == null)
                return;
            var target = cam.Target;
            if (target != null) {
                target.Prepare();
                if (m_RenderPipline != null) {
                    m_RenderPipline.DoCameraRender(cam, m_RenderObjMap);
                }
            }
        }

        public void Update(float delta, IRenderTargetNotify notify) {

            // 排序CameraList列表
            SortCamList();

            // 1.先清理Target
            if (m_RenerTarget != null) {
                // 通过Camera接口处理了, 这里注释
            //    m_RenerTarget.Prepare();

                // 多摄像机渲染，根据摄影机深度排序后顺序渲染
                for (int i = 0; i < m_CamList.Count; ++i) {
                    var cam = m_CamList[i];
                    CameraRender(cam);
                }


                m_RenerTarget.FlipToNotify(notify);
            }
        }

        public RenderTargetClearFlags ClearFlags {
            get {
                if (m_RenerTarget != null)
                    return m_RenerTarget.ClearFlags;
                return 0;
            }
            set {
                if (m_RenerTarget != null)
                    m_RenerTarget.ClearFlags = value;
            }
        }

        /*
        private void DestroyRenderObjects() {
            if (m_RenderObjMap != null) {
                var iter = m_RenderObjMap.GetEnumerator();
                while (iter.MoveNext()) {
                    if (iter.Current.Value != null)
                        iter.Current.Value.Dispose();
                }
                iter.Dispose();
                m_RenderObjMap.Clear();
            }
        }*/

        protected override void OnFree(bool isManual) {
           // DestroyRenderObjects();

            if (m_RenerTarget != null) {
                m_RenerTarget.Dispose();
                m_RenerTarget = null;
            }
            m_StaticDevice = null;
        }
    }
}
