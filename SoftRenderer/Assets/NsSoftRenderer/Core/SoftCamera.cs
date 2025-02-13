﻿#define _Use_FlipTrangle2

using System.Collections.Generic;
using UnityEngine;
using Utils;

//using RenderTargetClearFlags = System.Int32;

namespace NsSoftRenderer {

    // 摄影机类型
    public enum SoftCameraType {
        O, // 正交摄影机
        P  // 透视摄影机
    }

    public static class SoftCameraPlanes {
        public static readonly byte NearPlane = 0; // 近平面
        public static readonly byte FarPlane = 1; // 远平面
        public static readonly byte LeftPlane = 2;
        public static readonly byte RightPlane = 3;
        public static readonly byte UpPlane =  4;
        public static readonly byte DownPlane = 5;
    }

    // 透视摄影机数据
    public struct PCameraInfo {
        public float nearPlane, farPlane;
        public float fieldOfView;  // 角度制

        public void ResetDefault() {
            nearPlane = 0.3f;
            farPlane = 1000f;
            fieldOfView = 60.0f;
        }

        public static PCameraInfo Create() {
            PCameraInfo ret = new PCameraInfo();
            ret.ResetDefault();
            return ret;
        }

        public void GetFarWidthAndHeight(int deviceWidth, int deviceHeight, out float width, out float height) {
            height = this.farHeight;
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            width = aspect * height;
        }

        public void GetNearWidthAndHeight(int deviceWidth, int deviceHeight, out float width, out float height) {
            height = this.nearHeight;
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            width = aspect * height;
        }

        // 近平面高度
        public float nearHeight {
            get {
                float halfAngle = fieldOfView / 2.0f * Mathf.PI/180.0f; // 弧度制
                float ret = 2.0f * Mathf.Tan(halfAngle) * nearPlane;
                return ret;
            }
        }

        // 远平面高度
        public float farHeight {
            get {
                float halfAngle = fieldOfView / 2.0f * Mathf.PI / 180.0f; // 弧度制
                float ret = 2.0f * Mathf.Tan(halfAngle) * farPlane;
                return ret;
            }
        }

        // 获得近平面宽度
        public float GetNearWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            float ret = aspect * nearHeight;
            return ret;
        }

        // 获得远平面宽度
        public float GetFarWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            float ret = aspect * farHeight;
            return ret;
        }

        /*
        // 摄影机朝向Z轴负方向(Unity系)
        public Matrix4x4 PMatrix2 {
            get {
                //
                // -n, 0, 0, 0
                // 0, -n, 0, 0
                // 0, 0, -(f + n), -nf
                // 0, 0, 1, 0
                
                Matrix4x4 mat = Matrix4x4.zero;
                mat.m00 = -nearPlane;
                mat.m11 = -nearPlane;
                mat.m22 = -(farPlane + nearPlane);
                mat.m23 = -nearPlane * farPlane;
                mat.m32 = 1.0f;
                return mat;
            }
        }*/

        // 只有透视的矩阵，坐标系是：摄影机朝向Z轴正方向
        public Matrix4x4 PMatrix {
            get {
                /*
                 * n, 0, 0, 0
                 * 0, n, 0, 0
                 * 0, 0, f + n, -nf
                 * 0, 0, 1, 0
                 * 
                 * 推到过程：
                 * 1）普通点：视锥体【任意点】(x0, y0, z0) 到最终变换到近平面点(x1, y1, z1(Unknown))，根据相似三角形推出
                 *    y1 = near/z0 * y0;    x1 = near/z0 * x0;  Z1为UNKOWN，因为是一个矩形盒子里。
                 *    最终变换坐标为：(near/z0 * x0, near/z0 * y0, z1), 根据齐次坐标定义，都乘以Z得到齐次坐标：(near * x0, near * y0, z1(unknow(?)), z0),这样仍然描述同一个点。
                 *    这样构造出一个矩阵结论：
                 *    near, 0, 0, 0
                 *    0, near, 0, 0
                 *    A(?), B(?), C(?), D(?)
                 *    0，0，1, 0
                 *   【关键，现在就是求第三行了】
                 * 2）【近平面任意点】转换后仍然是相同的点。
                 *    即：M * (x0, y0, near, 1) = (x0 * near, y0 * near, near^2, near)【变换后Z值不变】
                 *    来推到出矩阵第三行条件：
                 *    A * x0 + B * y0 + C * near + D = near^2
                 *    任意点不受X,Y影响，所以得到，A = 0, B = 0
                 *    【最终得到】 
                 *          C * near + D  = near^2 ------------- 式1
                 * 3）对于【远平面中心原点】坐标为：(0, 0, 0, far, 1)最终变换到的齐次坐标为：(0, 0, far^2, far)【变换后Z值不变】
                 *    【推算出】
                 *          C * far + D = far^2    ---------------- 式2
                 * 最后，根据 式1 和 式2，求出C和D,得到：
                 *      C = far + near; D = -near * far
                 *  最终求出视锥体变换矩阵的第一步
                 */
                Matrix4x4 mat = Matrix4x4.zero;
                mat.m00 = nearPlane;
                mat.m11 = nearPlane;
                mat.m22 = farPlane + nearPlane;
                mat.m23 = -nearPlane * farPlane;
                mat.m32 = 1.0f; 
                return mat;
            }
        }

        public void GetPlanePoints(int deviceWidth, int deviceHeight, SoftCamera camera, 
            out Vector3 a, out Vector3 b, out Vector3 c, out Vector3 d, out Vector3 e, out Vector3 f, out Vector3 g, out Vector3 h) {
            // 8个顶点
            float halfNearHeight;
            float halfNearWidth;
            GetNearWidthAndHeight(deviceWidth, deviceHeight, out halfNearWidth, out halfNearHeight);
            halfNearWidth = halfNearWidth / 2.0f;
            halfNearHeight = halfNearHeight / 2.0f;

            Vector3 forward = camera.LookAt;
            Vector3 nearCenter = camera.Position + forward * nearPlane;
            /* e  h
             * f  g
             * |  |
             * a d
             * b c
             */
            // 近平面4个点
            // a, b, c, d;
            // 远平面4个点
            // e, f, g, h;
            a = nearCenter + new Vector3(-halfNearWidth, halfNearHeight, 0f);
            b = nearCenter + new Vector3(-halfNearWidth, -halfNearHeight, 0f);
            c = nearCenter + new Vector3(halfNearWidth, -halfNearHeight, 0f);
            d = nearCenter + new Vector3(halfNearWidth, halfNearHeight, 0f);

            float halfFarWidth, halfFarHeight;
            GetFarWidthAndHeight(deviceWidth, deviceHeight, out halfFarWidth, out halfFarHeight);
            halfFarWidth = halfFarWidth / 2.0f;
            halfFarHeight = halfFarHeight / 2.0f;
            Vector3 farCenter = camera.Position + forward * farPlane;
            e = farCenter + new Vector3(-halfFarWidth, halfFarHeight, 0f);
            f = farCenter + new Vector3(-halfFarWidth, -halfFarHeight, 0f);
            g = farCenter + new Vector3(halfFarWidth, -halfFarHeight, 0f);
            h = farCenter + new Vector3(halfFarWidth, halfFarHeight, 0f);
        }

        // 六大平面
        public void InitPlanes(SoftPlane[] planes, int deviceWidth, int deviceHeight, SoftCamera camera) {
            Vector3 a, b, c, d;
            Vector3 e, f, g, h;
            GetPlanePoints(deviceWidth, deviceHeight, camera, out a, out b, out c, out d, out e, out f, out g, out h);

            Vector3 camPos = camera.Position;

            // near plane
            Vector3 n = camera.LookAt;
            float dd = -n.x * a.x - n.y * a.y - n.z * a.z;
            planes[SoftCameraPlanes.NearPlane] = new SoftPlane(n, dd);
            // far plane
            n = -camera.LookAt;
            dd = -n.x * e.x - n.y * e.y - n.z * e.z;
            planes[SoftCameraPlanes.FarPlane] = new SoftPlane(n, dd);
            // left plane
            Vector3 v1 = a - camPos;
            Vector3 v2 = b - camPos;
            n = Vector3.Cross(v1, v2).normalized;
            dd = -n.x * a.x - n.y * a.y - n.z * a.z;
            planes[SoftCameraPlanes.LeftPlane] = new SoftPlane(n, dd);
            // right plane
            v1 = d - camPos;
            v2 = c - camPos;
            n = Vector3.Cross(v2, v1).normalized;
            planes[SoftCameraPlanes.RightPlane] = new SoftPlane(n, dd);
            // up plane
            v1 = e - a;
            v2 = d - a;
            n = Vector3.Cross(v2, v1).normalized;
            planes[SoftCameraPlanes.UpPlane] = new SoftPlane(n, dd);
            // down plane
            v1 = g - c;
            v2 = d - c;
            n = Vector3.Cross(v1, v2).normalized;
            planes[SoftCameraPlanes.DownPlane] = new SoftPlane(n, dd);
        }

        private static bool Compare(PCameraInfo f1, PCameraInfo f2) {
            bool ret = SoftMath.FloatEqual(f1.fieldOfView, f2.fieldOfView) && SoftMath.FloatEqual(f1.nearPlane, f2.nearPlane) && 
                SoftMath.FloatEqual(f1.farPlane, f2.farPlane);
            return ret;
        }

        public static bool operator ==(PCameraInfo f1, PCameraInfo f2) {
            return Compare(f1, f2);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator !=(PCameraInfo i1, PCameraInfo i2) {
            return !(i1 == i2);
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            if (!(obj is PCameraInfo))
                return false;
            return Compare(this, (PCameraInfo)obj);
        }
    }

    // 正交摄影机数据
    public struct OCameraInfo {
        public float Size;
        public float nearPlane;
        public float farPlane;

        public static bool operator==(OCameraInfo f1, OCameraInfo f2) {
            return Compare(f1, f2);
        }

        private static bool Compare(OCameraInfo f1, OCameraInfo f2) {
            bool ret = SoftMath.FloatEqual(f1.Size, f2.Size) && SoftMath.FloatEqual(f1.nearPlane, f2.nearPlane) && SoftMath.FloatEqual(f1.farPlane, f2.farPlane);
            return ret;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            if (!(obj is OCameraInfo))
                return false;
            return Compare(this, (OCameraInfo)obj);
        }

        public static bool operator !=(OCameraInfo i1, OCameraInfo i2) {
            return !(i1 == i2);
        }

        public void ResetDefault() {
            Size = 5.0f;
            nearPlane = 0.3f;
            farPlane = 1000f;
        }

        public static OCameraInfo Create() {
            OCameraInfo ret = new OCameraInfo();
            ret.ResetDefault();
            return ret;
        }

        public float CameraHeight {
            get {
                return Size * 2.0f;
            }
        }

        public float GetCameraWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float ret = w / h * CameraHeight;
            return ret;
        }

        public void InitPlanes(SoftPlane[] planes, int deviceWidth, int deviceHeight, SoftCamera camera) {
            if (camera == null || planes == null || planes.Length < 6)
                return;
            Vector3 forward = camera.LookAt;
            Vector3 right = camera.Right;
            Vector3 up = camera.Up;
            float cameraHeight = this.CameraHeight;
            float cameraWidth = GetCameraWidth(deviceWidth, deviceHeight);
            Vector3 nearCenter = camera.Position + forward * nearPlane;

            // near plane
            Vector3 n = forward;
            float d = -n.x * nearCenter.x - n.y * nearCenter.y - n.z * nearCenter.z;
            planes[SoftCameraPlanes.NearPlane] = new SoftPlane(n, d);
            // far plane
            Vector3 pos = camera.Position + forward * farPlane;
            n = -forward;
            d = -n.x * pos.x - n.y * pos.y - n.z * pos.z;
            planes[SoftCameraPlanes.FarPlane] = new SoftPlane(n, d);
            // Left plane
            n = right;
            pos = nearCenter + (-right * cameraWidth / 2.0f);
            d = -n.x * pos.x - n.y * pos.y - n.z * pos.z;
            planes[SoftCameraPlanes.LeftPlane] = new SoftPlane(n, d);
            // right plane
            n = -right;
            pos = nearCenter + (right * cameraWidth / 2.0f);
            d = -n.x * pos.x - n.y * pos.y - n.z * pos.z;
            planes[SoftCameraPlanes.RightPlane] = new SoftPlane(n, d);
            // up plane
            n = -up;
            pos = nearCenter + (up * cameraHeight / 2.0f);
            d = -n.x * pos.x - n.y * pos.y - n.z * pos.z;
            planes[SoftCameraPlanes.UpPlane] = new SoftPlane(n, d);
            // down plane
            n = up;
            pos = nearCenter - (up * cameraHeight / 2.0f);
            d = -n.x * pos.x - n.y * pos.y - n.z * pos.z;
            planes[SoftCameraPlanes.DownPlane] = new SoftPlane(n, d);
        }
    }

    // 相机通知
    public interface ISoftCameraLinker {
        void OnCameraDepthChanged();

        int DeviceWidth {
            get;
        }

        int DeviceHeight {
            get;
        }
    }

    // 软渲染摄影机
    public class SoftCamera: SoftRenderObject {
        private static SoftCamera m_MainCamera = null;

        private SoftCameraType m_CamType = SoftCameraType.O;
        // 是否是主摄像机
        private bool m_IsMainCamera = false;
        
        private bool m_IsMustChgMatrix = true;
        // 透视摄影机
        private PCameraInfo m_PCameraInfo;
        // 正交摄影机
        private OCameraInfo m_OCameraInfo;

        private ISoftCameraLinker m_Linker = null;

        private int m_Depth = 0;
        // 观测和投影矩阵
        private Matrix4x4 m_ViewProjMatrix = Matrix4x4.identity;
        private Matrix4x4 m_ViewProjInvMatrix = Matrix4x4.identity;
        private Matrix4x4 m_ProjMatrix = Matrix4x4.identity;
        private Matrix4x4 m_ProjInvMatrix = Matrix4x4.identity;
        private Matrix4x4 m_LinkerScreenMatrix = Matrix4x4.identity;
        private Matrix4x4 m_LinkerScreenInvMatrix = Matrix4x4.identity;
        // 世界坐标系转屏幕坐标系
       // private Matrix4x4 m_ViewProjLinkerScreenMatrix = Matrix4x4.identity;
        // 渲染目标
        private RenderTarget m_RenderTarget = null;
        private bool m_IsMustUpdatePlanes = true;
        private SoftPlane[] m_Planes = new SoftPlane[6];

        // 提交的三角形
        private RenderTrianglesMgr m_TrianglesMgr = new RenderTrianglesMgr();
        // 用于渲染各种排序管理,做过剔除的都会在里面，只存ID索引
        private RenderObjMgr m_RenderObjMgr = new RenderObjMgr();
        

        public bool IsOpenCameraSpereCull {
            get;
            set;
        }

        // 采用1f - z的方式，解决一定浮点的精度问题
        public bool ZBuffer_RevertZ {
            get {
                var target = this.Target;
                if (target == null)
                    return false;
                return target.isOneRevertZ;
            }
            set {
                var target = this.Target;
                if (target == null)
                    return;
                target.isOneRevertZ = value;
            }
        }

       public Matrix4x4 PorjInvMatrix {
            get {
                UpdateMatrix();
                return m_ProjInvMatrix;
            }
        }

       public void Cull(Dictionary<int, SoftRenderObject> objMap, out NativeList<int> visibleList) {
            m_RenderObjMgr.CameraCull(this, objMap, out visibleList);
        }

        public void UpdateCamera(Camera camera) {
            if (camera == null)
                return;
            bool isMainCam = camera.CompareTag("MainCamera");
            var trans = camera.transform;
            this.Position = trans.position;
            this.LookAt = trans.forward;
            this.Up = trans.up;
            this.IsMainCamera = isMainCam;
            this.Depth = (int)camera.depth;
            if (camera.orthographic) {
                OCameraInfo info = OCameraInfo.Create();
                info.Size = camera.orthographicSize;
                info.nearPlane = camera.nearClipPlane;
                info.farPlane = camera.farClipPlane;
                this.SetOCamera(info);
            } else {
                PCameraInfo info = PCameraInfo.Create();
                info.nearPlane = camera.nearClipPlane;
                info.farPlane = camera.farClipPlane;
                info.fieldOfView = camera.fieldOfView;
                this.SetPCamera(info);
            }
        }

        // 渲染提前调用
        internal virtual void DoCameraPreRender() {
            m_TrianglesMgr.Clear();
            // 更新矩阵
            UpdateMatrix();
        }

        public bool IsShowVertexLog = false;

        private void DebugVertexLog(TriangleVertex vertex) {
            var camera = Camera.main;
            if (camera != null) {
                var p1 = camera.WorldToScreenPoint(vertex.triangle.p1);
                var p2 = camera.WorldToScreenPoint(vertex.triangle.p2);
                var p3 = camera.WorldToScreenPoint(vertex.triangle.p3);
                string ss1 = SoftCameraTest.GetVectorStr(p1);
                string ss2 = SoftCameraTest.GetVectorStr(p2);
                string ss3 = SoftCameraTest.GetVectorStr(p3);
                string ss4 = string.Format("【Camera】p1={0} p2={1} p3={2}", ss1, ss2, ss3);

                vertex.triangle.Trans(this.WorldToScreenPointEvt);

               // Debug.LogError(this.ViewProjLinkerScreenMatrix.ToString());

                string s1 = SoftCameraTest.GetVectorStr(vertex.triangle.p1);
                string s2 = SoftCameraTest.GetVectorStr(vertex.triangle.p2);
                string s3 = SoftCameraTest.GetVectorStr(vertex.triangle.p3);
                string s4 = string.Format("【SoftCamera】p1={0} p2={1} p3={2}", s1, s2, s3);

                Debug.Log(ss4 + s4);
            }
        }

        private void InitPassMode(RenderPassMode passMode) {
            passMode.MVPMatrix = this.Shader_MVP_Matrix;
        }

        private void FlipTriangle(TriangleVertex vertex, RenderPassMode passMode) {
            // 三角形转到屏幕坐标系
            RenderTarget target = this.Target;
            if (target != null) {
                if (IsShowVertexLog) {
                    DebugVertexLog(vertex);
                }

                // 这里是VertexShader的部分
                // 世界坐标系到屏幕坐标系
                if (passMode.vertexShader == null) {
                    // 默认的一个处理
                    //vertex.triangle.Trans(this.WorldToScreenPointEvt2, false);
                    vertex.triangle.MulMatrix(this.Shader_MVP_Matrix);
                } else {
                    InitPassMode(passMode);
                    passMode.vertexShader.Main(ref vertex); 
                }

                // 做个三角形的判断
                if (vertex.IsAllZGreateOne)
                    return;

                // 这里做背面剔除
                if (SoftMath.Is_MVP_Culled(passMode.Cull, vertex.triangle))
                    return;

                vertex.triangle.MulMatrix(this.LinkerScreenMatrix);
#if _Use_FlipTrangle2
                target.FlipScreenTriangle2(this, vertex, passMode);
#else
                target.FlipScreenTriangle(this, vertex, passMode);
#endif
            }
        }

        private void FlipTraiangles(RenderPassMode passMode) {
            TriangleVertex tri;
            for (int i = 0; i < m_TrianglesMgr.Count; ++i) {
                if (m_TrianglesMgr.GetTrangle(i, out tri)) {
                    // tri已经是世界坐标系的
                    FlipTriangle(tri, passMode);
                } else
                    break;
            }
            m_TrianglesMgr.Clear();
        }

        internal virtual void DoCameraPostRender(RenderPassMode passMode) {
            // 提交渲染结果
            FlipTraiangles(passMode);
        }

        private bool RenderSubMesh(SoftMesh mesh, SoftSubMesh subMesh, Matrix4x4 objToWorld, RenderPassMode passMode) {
            if (subMesh == null || passMode == null)
                return false;
            var indexes = subMesh.Indexes;
            var vertexs = mesh.Vertexs;
            var colors = mesh.Colors;
            var uv1s = mesh.UV1s;
            bool ret = false;

            bool isColorEmpty = colors  == null || colors.Count <= 0;
            bool isUV1Empty = uv1s == null || uv1s.Count <= 0;
            Color c1 = Color.white;
            Color c2 = Color.white;
            Color c3 = Color.white;
            Vector4 uv1_1 = Vector4.zero;
            Vector4 uv1_2 = Vector4.zero;
            Vector4 uv1_3 = Vector4.zero;
            if (vertexs != null && (isColorEmpty || vertexs.Count == colors.Count)
                && indexes != null && indexes.Count > 0) {

                int triangleCnt = ((int)indexes.Count / 3);
                for (int i = 0; i < triangleCnt; ++i) {
                    int idx = i * 3;
                    int index = indexes[idx];
                    Vector3 p1 = vertexs[index];
                    if (!isColorEmpty)
                        c1 = colors[index];
                    if (!isUV1Empty)
                        uv1_1 = uv1s[index];
                    index = indexes[idx + 1];
                    Vector3 p2 = vertexs[index];
                    if (!isColorEmpty)
                        c2 = colors[index];
                    if (!isUV1Empty)
                        uv1_2 = uv1s[index];
                    index = indexes[idx + 2];
                    Vector3 p3 = vertexs[index];
                    if (!isColorEmpty)
                        c3 = colors[index];
                    if (!isUV1Empty)
                        uv1_3 = uv1s[index];
                    Triangle tri = new Triangle(p1, p2, p3);

                    // 三角形转到世界坐标系
                    tri.MulMatrix(objToWorld);
                    // 过CullMode 【注意】根据渲染管线VertexShader中可以任意改变三角形，所以要放到VS后面才行，也就是到MVP坐标系里判断
                    // 不在这里做摄影机剔除，移到VS后面
                //   if (SoftMath.IsCulled(this, passMode.Cull, tri)) {
                 //       continue;
                 //   }
                    //----

                    TriangleVertex triV = new TriangleVertex(tri, c1, c2, c3, passMode.mainTex);
                    if (!isUV1Empty) {
                        triV.uv1_1 = uv1_1;
                        triV.uv1_2 = uv1_2;
                        triV.uv1_3 = uv1_3;
                    }

                    // 进入VertexShader了， 做顶点变换等
                    m_TrianglesMgr.AddTriangle(triV);
                    ret = true;
                }
            }
            return ret;
        }

        public bool RenderMesh(SoftMesh mesh, Matrix4x4 objToWorld, RenderPassMode passMode) {
            if (mesh == null || passMode == null)
                return false;
            var subMeshes = mesh.SubMeshes;
            bool ret = false;
            if (subMeshes != null) {
                for (int i = 0; i < subMeshes.Count; ++i) {
                    var subMesh = subMeshes[i];
                    if (RenderSubMesh(mesh, subMesh, objToWorld, passMode))
                        ret = true;
                }
            }
            return ret;
        }

        public SoftRenderObject GetRenderObject(int instanceId) {
            SoftDevice device = SoftDevice.StaticDevice;
            if (device != null)
                return device.GetRenderObject(instanceId);
            return null;
        }

        public RenderTarget Target {
            get {
                if (m_RenderTarget != null)
                    return m_RenderTarget;
                var device = SoftDevice.StaticDevice;
                if (device != null)
                    return device.Target;
                return null;
            }
        }

        public RenderTrianglesMgr TrianglesMgr {
            get {
                return m_TrianglesMgr;
            }
        }

        public SoftPlane[] WorldPlanes {
            get {
                UpdatePlanes();
                return m_Planes;
            }
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_RenderObjMgr != null) {
                m_RenderObjMgr.Dispose();
                m_RenderObjMgr = null;
            }

            if (m_TrianglesMgr != null)
            {
                m_TrianglesMgr.Dispose();
                m_TrianglesMgr = null;
            }

            base.OnFree(isManual);
        }

        // 更新Plane
        private void UpdatePlanes() {
            if (m_IsMustUpdatePlanes) {
                m_IsMustUpdatePlanes = false;
                // 生成Plane
                switch (m_CamType) {
                    case SoftCameraType.O:
                        m_OCameraInfo.InitPlanes(m_Planes, m_Linker.DeviceWidth, m_Linker.DeviceHeight, this);
                        break;
                    case SoftCameraType.P:
                        m_PCameraInfo.InitPlanes(m_Planes, m_Linker.DeviceWidth, m_Linker.DeviceHeight, this);
                        break;
                }
            }
        }

        private void DoMustUpdatePlanes() {
            m_IsMustUpdatePlanes = true;
        }

        private void UpdateLinkerScreenMatrix() {
            if (m_Linker != null) {
                // X: 0-DeviceWidth Y:0`DeviceHeight Z: near~far
                m_LinkerScreenMatrix = Matrix4x4.Scale(new Vector3(m_Linker.DeviceWidth, m_Linker.DeviceHeight, 1f));
                m_LinkerScreenInvMatrix = m_LinkerScreenMatrix.inverse;
            } else {
                m_LinkerScreenMatrix = Matrix4x4.identity;
                m_LinkerScreenInvMatrix = Matrix4x4.identity;
            }
        }

        /*
        public Matrix4x4 ViewProjLinkerScreenMatrix {
            get {
                UpdateMatrix();
                return m_ViewProjLinkerScreenMatrix;
            }
        }*/

        public Matrix4x4 LinkerScreenMatrix {
            get {
                UpdateMatrix();
                return m_LinkerScreenMatrix;
            }
        } 

        public Matrix4x4 LinkercreenInvSMatrix {
            get {
                UpdateMatrix();
                return m_LinkerScreenInvMatrix;
            }
        }

        public static SoftCamera MainCamera {
            get {
                return m_MainCamera;
            }
        }

        public bool IsMainCamera {
            get {
                return m_IsMainCamera;
            }

            set {
                if (m_IsMainCamera != value) {
                    m_IsMainCamera = value;
                   
                   if (value) {
                        if (m_MainCamera != null)
                            m_MainCamera.IsMainCamera = false;
                        m_MainCamera = this;
                    } else {
                        if (m_MainCamera == this)
                            m_MainCamera = null;
                    }
                }
            }
        }

        protected override void DoMustGlobalToLocalMatrixChg() {
            base.DoMustGlobalToLocalMatrixChg();
            DoMatrixChange();
        }

        public void SetPCamera(PCameraInfo info) {
            if (m_PCameraInfo != info) {
                m_PCameraInfo = info;
                this.CameraType = SoftCameraType.P;
                DoMustUpdatePlanes();
                DoMustGlobalToLocalMatrixChg();
            }
        }

        public void SetOCamera(OCameraInfo info) {
            if (m_OCameraInfo != info) {
                m_OCameraInfo = info;
                this.CameraType = SoftCameraType.O;
                DoMustUpdatePlanes();
                DoMustGlobalToLocalMatrixChg();
            }
        }

        public void RefreshLinker() {
            UpdateLinkerScreenMatrix();
        }

        public SoftCamera(ISoftCameraLinker linker): base() {
            InitEvents();
            m_Linker = linker;
            m_Type = SoftRenderObjType.Camera;
            UpdateLinkerScreenMatrix();
        }

        public int Depth {
            get {
                return m_Depth;
            }
            set {
                if (m_Depth != value) {
                    m_Depth = value;
                    OnDepthChanged();
                }
            }
        }

        private void OnDepthChanged() {
            if (m_Linker != null)
                m_Linker.OnCameraDepthChanged();
        }

        public Matrix4x4 ViewMatrix {
            get {
                UpdateMatrix();
                return m_GlobalToLocalMatrix;
            }
        }

        public Matrix4x4 ViewInvMatrix {
            get {
                UpdateMatrix();
                return m_LocalToGlobalMatrix;
            }
        }

        public Matrix4x4 ViewProjMatrix {
            get {
                UpdateMatrix();
                return m_ViewProjMatrix;
            }
        }

        public Matrix4x4 ViewProjInvMatrix {
            get {
                UpdateMatrix();
                return m_ViewProjInvMatrix;
            }
        }

        public Matrix4x4 ProjMatrix {
            get {
                UpdateMatrix();
                return m_ProjMatrix;
            }
        }

        public Matrix4x4 ProjInvMatrix {
            get {
                UpdateMatrix();
                return m_ProjInvMatrix;
            }
        }

        private void DoMatrixChange() {
            m_IsMustChgMatrix = true;
            DoMustUpdatePlanes();
        }

        protected override void DoLookAtUpChange() {
            base.DoLookAtUpChange();
            DoMatrixChange();
        }

        public float nearPlane {
            get {
                switch (this.CameraType) {
                    case SoftCameraType.O:
                        return m_OCameraInfo.nearPlane;
                    case SoftCameraType.P:
                        return m_PCameraInfo.nearPlane;
                }
                return 0f;
            }
        }

        public float farPlane {
            get {
                switch (this.CameraType) {
                    case SoftCameraType.O:
                        return m_OCameraInfo.farPlane;
                    case SoftCameraType.P:
                        return m_PCameraInfo.farPlane;
                }
                return 0f;
            }
        }

        public Vector3 ViewportToWorldPoint(Vector3 position, bool isUseViewZ) {

            if (position == Vector3.zero) {
                return this.Position;
            }

            
            if (isUseViewZ) {
                float z = position.z;
                // 反求真实Z，这个要注意，投影变换（正交和透视摄影机坐标系中Z值一样，投影转换后Z也一样）
                // 因为开启了isUseViewZ position里的Z并不是真实在ProjMatrix转换后的Z是ViewMatrix转换后的Z
                // 如果不去获取真实的Z，因为在透视矩阵里是Z不同哪怕X,Y一样映射到近平面的值也会不一样,则需要转换一次获得真实的Z
                // 为什么乘以-1是因为，在WorldToViewport函数里Z取反了。
               
                Vector3 tmp = new Vector3(0f, 0f, -z);
                tmp = this.ProjMatrix.MultiplyPoint(tmp);
                position.z = tmp.z;
                
            }

     
            Matrix4x4 transMat = Matrix4x4.Translate(new Vector3(-0.5f, -0.5f, 0f));
            Matrix4x4 scaleMat = Matrix4x4.Scale(new Vector3(2f, 2f, 1f));
            
            Matrix4x4 mat = this.ViewInvMatrix * this.ProjInvMatrix * scaleMat * transMat;
            Vector3 ret = mat.MultiplyPoint(position);

            return ret;
        }

        public Vector3 ViewportToWorldPoint(Vector3 position) {
            return ViewportToWorldPoint(position, true);
        }

        //   private Matrix4x4 testInvMat;

        private Matrix4x4 m_Shader_MVP_Matrix;
        private Matrix4x4 Shader_MVP_Matrix {
            get {
                UpdateMatrix();
                return m_Shader_MVP_Matrix;
            }
        }

        private void Update_Shader_MVP_Matrix() {
            
            Matrix4x4 transMat = Matrix4x4.Translate(new Vector3(1f, 1f, 1f));
            Matrix4x4 scaleMat = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f));
            m_Shader_MVP_Matrix = scaleMat * transMat * m_ViewProjMatrix;
            //Matrix4x4 scaleMat = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1f));
           // Matrix4x4 transMat = Matrix4x4.Translate(new Vector3(0.5f, 0.5f, 0f));
           // m_Shader_MVP_Matrix = transMat * scaleMat * m_ViewProjMatrix;
        }

        // 摄影机左下角为0,0， 右上角为1,1, 注意：ViewProjMatrix是-1~1,但转换后要是是0~1范围（Unity的规则）
        // 最终UNITY的效果是 X：0~1 Y: 0~1,  Z is in world units from the camera.
        // 來自UNITY幫助：isUseViewZ 设置为TRUE的时候
        //    Viewport space is normalized and relative to the camera. The bottom-left of the camera is (0,0); the top-right is (1,1). 
        //    The z position is in world units from the camera.
        public Vector3 WorldToViewportPoint(Vector3 position, bool isUseViewZ) {

            /*

            Matrix4x4 mat = this.ViewProjMatrix;//X: -1~1 Y: -1~1 Z:-1~1
            Matrix4x4 transMat = Matrix4x4.Translate(new Vector3(1f, 1f, 1f)); //X: 0~2, Y: 0~2, Z: 0~2 原点移到摄影机左下角
            Matrix4x4 scaleMat = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f * (farPlane - nearPlane)));// x: 0~1, y:0~1 Z:: 0~farPlane - nearPlane
            Matrix4x4 nearTransMat = Matrix4x4.Translate(new Vector3(0f, 0f, nearPlane));
            Vector3 ret = (nearTransMat * scaleMat * transMat * mat).MultiplyPoint(position);
            */

            if (position == this.Position)
                return Vector3.zero;

            
            Matrix4x4 viewMat = this.ViewMatrix;
            Vector3 ret = viewMat.MultiplyPoint(position);
            float z = 0f;
            if (isUseViewZ)
                z = -ret.z; // 要取反方向
            Matrix4x4 transMat = Matrix4x4.Translate(new Vector3(1f, 1f, 0f));
            Matrix4x4 scaleMat = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 1f));
            Matrix4x4 mat = (scaleMat * transMat * this.ProjMatrix);

            //  Debug.LogErrorFormat("[inv1]{0}", mat.inverse);

            ret = mat.MultiplyPoint(ret);
            if (isUseViewZ)
                ret.z = z;

          //  testInvMat = (mat * viewMat).inverse;

            return ret;
        }

        public Vector3 WorldToViewportPoint(Vector3 position) {
            return WorldToViewportPoint(position, true);
        }

        private System.Func<Vector3, Vector3> m_WorldToViewPointEvt = null;
        private System.Func<Vector3, Vector3> m_WorldToScreenPointEvt = null;
        private System.Func<Vector3, bool, Vector3> m_WorldToViewPointEvt2 = null;
        private System.Func<Vector3, bool, Vector3> m_WorldToScreenPointEvt2 = null;

        private void InitEvents() {
            if (m_WorldToViewPointEvt == null)
                m_WorldToViewPointEvt = new System.Func<Vector3, Vector3>(this.WorldToViewportPoint);
            if (m_WorldToScreenPointEvt == null)
                m_WorldToScreenPointEvt = new System.Func<Vector3, Vector3>(this.WorldToScreenPoint);
            if (m_WorldToViewPointEvt2 == null)
                m_WorldToViewPointEvt2 = new System.Func<Vector3, bool, Vector3>(this.WorldToViewportPoint);
            if (m_WorldToScreenPointEvt2 == null)
                m_WorldToScreenPointEvt2 = new System.Func<Vector3, bool, Vector3>(this.WorldToScreenPoint);
        }

        public System.Func<Vector3, Vector3> WorldToViewPointEvt {
            get {
                return m_WorldToViewPointEvt;
            }
        }

        public System.Func<Vector3, Vector3> WorldToScreenPointEvt {
            get {
                return m_WorldToScreenPointEvt;
            }
        }

        public System.Func<Vector3, bool, Vector3> WorldToViewPointEvt2 {
            get {
                return m_WorldToViewPointEvt2;
            }
        }

        public System.Func<Vector3, bool, Vector3> WorldToScreenPointEvt2 {
            get {
                return m_WorldToScreenPointEvt2;
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position) {
            return WorldToScreenPoint(position, true);
        }

        public Vector3 WorldToScreenPoint(Vector3 position, bool isUseViewZ) {
            /*
            var mat = this.ViewProjLinkerScreenMatrix; 
            Vector3 ret = mat.MultiplyPoint(position);
            Triangle.CheckPtIntf(ref ret);
            return ret;
            */

            Vector3 ret = WorldToViewportPoint(position, isUseViewZ);
            ret = this.LinkerScreenMatrix.MultiplyPoint(ret);

           // Triangle.CheckPtIntf(ref ret);
            return ret;
        }

        public Vector3 ScreenToWorldPoint(Vector3 position) {
            return ScreenToWorldPoint(position, true);
        }

        public Vector3 ScreenToWorldPoint(Vector3 position, bool isUseViewZ) {
            Vector3 ret = this.LinkercreenInvSMatrix.MultiplyPoint(position);
            ret = ViewportToWorldPoint(ret, isUseViewZ);
            return ret;
        }

        private void UpdateViewMatrix() {
            UpdateGlobalToLocalMatrix();
        }

        private void UpdateOProjMatrix() {
            if (m_Linker != null) {
                int deviceWidth = m_Linker.DeviceWidth;
                int deviceHeight = m_Linker.DeviceHeight;
                float w = m_OCameraInfo.GetCameraWidth(deviceWidth, deviceHeight);
                float h = m_OCameraInfo.CameraHeight;

                // 先平移到 Z 正方形中心点
                Vector3 offset = new Vector3(0f, 0f, m_OCameraInfo.nearPlane + (m_OCameraInfo.farPlane - m_OCameraInfo.nearPlane) / 2.0f);
                Matrix4x4 offsetMat = Matrix4x4.Translate(offset);

                // 缩放
                // - 2.0f/(m_OCameraInfo.farPlane - m_OCameraInfo.nearPlane) 为什么前面加负号因为摄影机的坐标系是看向[0, 0, -1]方向, 
                // 需要将方向变反，这样保证相机看到的Z缩放后，是从小变大)
                    Vector3 scale = new Vector3(2.0f / w, 2.0f / h, -2.0f / (m_OCameraInfo.farPlane - m_OCameraInfo.nearPlane));
               // Vector3 scale = new Vector3(2.0f / w, 2.0f / h, 2.0f / (m_OCameraInfo.farPlane - m_OCameraInfo.nearPlane));

                // 最终变换到的结果 X:-1~1 Y:-1~1 Z: -1~1
                // 在相机空间是Z：-near~-far，因为缩放的时候取反了，就变成了z:-1~1。
                m_ProjMatrix = Matrix4x4.Scale(scale) * offsetMat;

                m_ProjInvMatrix = m_ProjMatrix.inverse;
            } else {
                m_ProjMatrix = Matrix4x4.identity;
                m_ProjInvMatrix = Matrix4x4.identity;
            }
        }

        private void UpdatePProjMatrix() {
            if (m_Linker != null) {
                int deviceWidth = m_Linker.DeviceWidth;
                int deviceHeight = m_Linker.DeviceHeight;

                float nearW, nearH;
                m_PCameraInfo.GetNearWidthAndHeight(deviceWidth, deviceHeight, out nearW, out nearH);

                // 1.从视锥体转到正方体
                // 因为PMatrix是根据正向NEAR~FAR（都是正数摄影机朝向Z轴正方向）来推到的，而UNITY的摄影机Z视反向摄影机看向方向, 最后还要转回去
                Matrix4x4 pMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, -1f))  * m_PCameraInfo.PMatrix * Matrix4x4.Scale(new Vector3(1f, 1f, -1f));

                // Vector3 v = new Vector3(0, 0, -m_PCameraInfo.nearPlane);
                //  v = pMatrix.MultiplyPoint(v);


                // 先平移到 Z 正方形中心点
                Vector3 offset = new Vector3(0f, 0f, (m_PCameraInfo.nearPlane + (m_PCameraInfo.farPlane - m_PCameraInfo.nearPlane) / 2.0f));
                Matrix4x4 offsetMat = Matrix4x4.Translate(offset);
                // 3.缩放矩阵，缩放到-1~1 -1~1，因为UNITY的摄影机Z视反向摄影机看向方向，所以这里再反转一次，所以Z的变换乘了-1
                Vector3 scale = new Vector3(2.0f / nearW, 2.0f / nearH, -2.0f / (m_PCameraInfo.farPlane - m_PCameraInfo.nearPlane));
                Matrix4x4 scaleMat = Matrix4x4.Scale(scale);
                // 根据步骤求出ProjMatrix
                m_ProjMatrix = scaleMat * offsetMat * pMatrix;

                m_ProjInvMatrix = m_ProjMatrix.inverse;
            } else {
                m_ProjMatrix = Matrix4x4.identity;
                m_ProjInvMatrix = Matrix4x4.identity;
            }
        }

        // 投影矩阵
        private void UpdateProjMatrix() {
            switch (m_CamType) {
                // 正交
                case SoftCameraType.O:
                    UpdateOProjMatrix();
                    break;
                // 透视
                case SoftCameraType.P:
                    UpdatePProjMatrix();
                    break;
            }
        }

        private void UpdateViewProjMatrix() {
            m_ViewProjMatrix = m_ProjMatrix * m_GlobalToLocalMatrix;
            m_ViewProjInvMatrix = m_ViewProjMatrix.inverse;
        }

        /*
        private void UpdateViewProjLinerScreenMatrix() {
            // 1. X: -1~1 Y: -1~1 Z:-1~1
            // m_ViewProjMatrix
            // 2. X: 0-2 Y: 0-2 Z: 0-2
            
            Matrix4x4 trans = Matrix4x4.Translate(new Vector3(1f, 1f, 1f));
            // 3. X:0~1 Y: 0~1 Z: 0~farPlane - nearPlane
            Matrix4x4 scales = Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f * (farPlane - nearPlane)));
            // 4. X: 0~1 Y:0~1 Z: nearPlane ~ farPlane
            Matrix4x4 nearTrans = Matrix4x4.Translate(new Vector3(0f, 0f, nearPlane));
            Matrix4x4 mat = nearTrans * scales * trans * m_ViewProjMatrix; 
            // 5. X: 0~DeviceWidth Y: 0~DeviceHeight Z: nearPlane~farPlane
            m_ViewProjLinkerScreenMatrix = m_LinkerScreenMatrix * mat;
        }*/

        private void UpdateMatrix() {
            if (m_IsMustChgMatrix) {
                m_IsMustChgMatrix = false;
                // 更新坐标
                UpdateAxis();
                // 更新观察矩阵
                UpdateViewMatrix();
                // 更新投影矩阵
                UpdateProjMatrix();
                // 更新观察投影矩阵
                UpdateViewProjMatrix();
                // 更新世界坐标到屏幕
                //UpdateViewProjLinerScreenMatrix();
                // Shader相关
                Update_Shader_MVP_Matrix();
            }
        }

        protected override void PositionChanged()
        {
            DoMatrixChange();
        }

        public override void Update(float delta) {
            UpdateMatrix();
        }

        // 摄影机类型
        public SoftCameraType CameraType {
            get {
                return m_CamType;
            }
            set {
                if (m_CamType != value) {
                    m_CamType = value;
                    DoMatrixChange();
                }
            }
        }


    }
}
