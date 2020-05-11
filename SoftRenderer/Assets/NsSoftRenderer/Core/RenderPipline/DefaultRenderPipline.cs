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
            m_DefaultPassMode.vertexShader = new VertexShader();
            m_DefaultPassMode.pixelShader = new PixelShader();
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