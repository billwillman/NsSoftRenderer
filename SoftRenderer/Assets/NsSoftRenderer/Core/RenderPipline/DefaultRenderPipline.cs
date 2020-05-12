using UnityEngine;
using System.Collections.Generic;
using Utils;

namespace NsSoftRenderer {

    public class GeometryQueue: IRenderQueue {

    }

    // 默认的RenderPipline
    public class DefaultRenderPipline: IRenderPipline {
        private RenderPassMode m_DefaultPassMode = new RenderPassMode();
        private VisiableListSort m_VisiableListSort = new VisiableListSort();

        public DefaultRenderPipline() {
            RegisterRenderQueue(RenderQueue.Geometry, new GeometryQueue());
            m_DefaultPassMode.CreateVertexShader<VertexShader>();
            m_DefaultPassMode.CreatePixelShader<PixelShader>();
        }

        private class VisiableListSort: IComparer<int> {

            public SoftCamera currentCamera = null;

            public int Compare(int handle1, int handle2) {
                if (currentCamera == null)
                    return 0;

                var device = SoftDevice.StaticDevice;
                if (device == null)
                    return 0;
                if (handle1 == handle2)
                    return 0;
                var obj1 = device.GetRenderObject(handle1);
                var obj2 = device.GetRenderObject(handle2);
                if (obj1 == null && obj2 == null)
                    return 0;
                if (obj1 == null)
                    return 1;
                if (obj2 == null)
                    return -1;
                var local1 = currentCamera.GlobalToLocalMatrix.MultiplyPoint(obj1.Position);
                var local2 = currentCamera.GlobalToLocalMatrix.MultiplyPoint(obj2.Position);
                if (Mathf.Abs(local1.z - local2.z) <= float.Epsilon)
                    return 0;
                if (local1.z < local2.z)
                    return -1;
                else
                    return 1;
            }
        }

        // 排序
        protected virtual void DoVisibleRenderObjectsSort(SoftCamera camera, NativeList<int> visibleList) {
            if (visibleList == null || visibleList.Count <= 0)
                return;
            m_VisiableListSort.currentCamera = camera;
            visibleList.Sort(m_VisiableListSort);
        }

        // 排序
        internal override bool DoCameraRender(SoftCamera camera, Dictionary<int, SoftRenderObject> objMap) {
            if (camera == null || objMap == null)
                return false;
            NativeList<int> visibleList;
            camera.DoCameraPreRender();
            camera.Cull(objMap, out visibleList);
            bool ret = false;
            if (visibleList != null && visibleList.Count > 0) {

                DoVisibleRenderObjectsSort(camera, visibleList);

                for (int i = 0; i < visibleList.Count; ++i) {
                    SoftRenderObject obj = camera.GetRenderObject(visibleList[i]);
                    if (obj != null) {
                        // 后面考虑排序和后面模拟可编程。。。
                        if (obj.Render(camera, m_DefaultPassMode))
                            ret = true;
                    }
                }
            }
            camera.DoCameraPostRender(m_DefaultPassMode);

            return ret;
        }
    }
}