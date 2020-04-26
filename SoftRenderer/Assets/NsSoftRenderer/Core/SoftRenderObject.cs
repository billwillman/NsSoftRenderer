using UnityEngine;

namespace NsSoftRenderer {

    // 所有3D物件类基
    public class SoftRenderObject {
        private static int m_GlobalInstanceId = 0;
        private int m_InstanceId = 0;
        protected Vector3 m_Position;

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

        protected virtual void PositionChanged()
        { }

        public Vector3 Position
        {
            get
            {
                return m_Position;
            }

            set
            {
                if (m_Position != value)
                {
                    m_Position = value;
                    PositionChanged();
                }
            }
        }
    }

}
