namespace NsSoftRenderer {

    public class GeometryQueue: IRenderQueue {

    }

    // 默认的RenderPipline
    public class DefaultRenderPipline: IRenderPipline {

        public DefaultRenderPipline() {
            RegisterRenderQueue(RenderQueue.Geometry, new GeometryQueue());
        }
    }
}