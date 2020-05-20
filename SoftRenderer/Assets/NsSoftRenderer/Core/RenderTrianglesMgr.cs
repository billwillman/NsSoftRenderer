using Utils;

namespace NsSoftRenderer {

    // 渲染的三角形管理器
    public class RenderTrianglesMgr: DisposeObject {
        private NativeList<TriangleVertex> m_List = null;

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_List != null) {
                m_List.Dispose();
                m_List = null;
            }
        }

        public bool GetTrangle(int index, out TriangleVertex vertex) {
            if (m_List == null || index < 0 || index >= m_List.Count) {
                vertex = new TriangleVertex();
                return false;
            }
            vertex = m_List[index];
            return true;
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

        public int Count {
            get {
                if (m_List != null)
                    return m_List.Count;
                return 0;
            }
        }
    }
}