using Utils;

namespace NsSoftRenderer {

    // 渲染的三角形管理器
    public class RenderTrianglesMgr: DisposeObject {
        private NativeList<TriangleVertex> m_List = null;

        protected override void OnFree(bool isManual) {
            if (m_List != null) {
                m_List.Dispose();
                m_List = null;
            }
        }

        public void AddTriangle(TriangleVertex vertex) {
            if (m_List == null)
                m_List = new NativeList<TriangleVertex>();
            m_List.Add(vertex);
        }

        public void Clear() {
            if (m_List != null) {
                m_List.Clear(false);
            }
        }
    }
}