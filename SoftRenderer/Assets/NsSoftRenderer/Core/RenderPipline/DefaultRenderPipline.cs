using System.Collections.Generic;
using Utils;

namespace NsSoftRenderer {

    public class GeometryQueue: IRenderQueue {

    }

    // 默认的RenderPipline
    public class DefaultRenderPipline: IRenderPipline {
        private RenderPassMode m_DefaultPassMode = new RenderPassMode();

        public DefaultRenderPipline() {
            RegisterRenderQueue(RenderQueue.Geometry, new GeometryQueue());
        }

        // 排序
        internal override void DoCameraRender(SoftCamera camera, Dictionary<int, SoftRenderObject> objMap) {
            if (camera == null || objMap == null)
                return;
            NativeList<int> visibleList;
            camera.DoCameraPreRender();
            camera.Cull(objMap, out visibleList);
            if (visibleList != null && visibleList.Count > 0) {
                for (int i = 0; i < visibleList.Count; ++i) {
                    SoftRenderObject obj = camera.GetRenderObject(visibleList[i]);
                    if (obj != null) {
                        // 后面考虑排序和后面模拟可编程。。。
                        obj.Render(camera, m_DefaultPassMode);
                    }
                }
            }
            camera.DoCameraPostRender(m_DefaultPassMode);
        }
    }
}