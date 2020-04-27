using System.Collections.Generic;
using Utils;

namespace NsSoftRenderer {

    public class GeometryQueue: IRenderQueue {

    }

    // 默认的RenderPipline
    public class DefaultRenderPipline: IRenderPipline {

        public DefaultRenderPipline() {
            RegisterRenderQueue(RenderQueue.Geometry, new GeometryQueue());
        }

        // 排序
        internal override void DoCameraRender(SoftCamera camera, Dictionary<int, SoftRenderObject> objMap) {
            if (camera == null || objMap == null)
                return;
            NativeList<int> visibleList;
            camera.Cull(objMap, out visibleList);
            if (visibleList != null && visibleList.Count > 0) {
                for (int i = 0; i < visibleList.Count; ++i) {
                    SoftRenderObject obj = camera.GetRenderObject(visibleList[i]);
                    if (obj != null) {

                    }
                }
            }
        }
    }
}