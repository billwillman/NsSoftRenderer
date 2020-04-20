namespace NsSoftRenderer {

    // 所有类基
    public class SoftRenderObject {
        private static int m_GlobalInstanceId = 0;
        private int m_InstanceId = 0;

        private static int GenInstanceId() {
            return ++m_GlobalInstanceId;
        }

        public SoftRenderObject() {
            m_InstanceId = GenInstanceId();
        }

        public int InstanceId {
            get {
                return m_InstanceId;
            }
        }
    }

}
