using UnityEngine;

namespace NsSoftRenderer {

    // 所有3D物件类基
    public class SoftRenderObject {
        private static int m_GlobalInstanceId = 0;
        private int m_InstanceId = 0;
        protected Vector3 m_Position;
        // 观测方向
        protected Vector3 m_LookAt = new Vector3(0, 0, -1f);
        // UP方向
        protected Vector3 m_Up = new Vector3(0, 1f, 0);
        // Right方向
        protected Vector3 m_Right = Vector3.zero;
        protected bool m_IsLookAtAndUpChged = true;

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

        protected void UpdateAxis() {
            if (m_IsLookAtAndUpChged) {
                m_IsLookAtAndUpChged = false;
                m_LookAt = m_LookAt.normalized;
                m_Right = Vector3.Cross(m_LookAt, m_Up).normalized;
                m_Up = Vector3.Cross(m_Right, m_LookAt);
            }
        }

        protected virtual void DoLookAtUpChange() {
            m_IsLookAtAndUpChged = true;
        }

        public Vector3 LookAt {
            get {
                UpdateAxis();
                return m_LookAt;
            }
            set {
                if (m_LookAt != value) {
                    m_LookAt = value;
                    DoLookAtUpChange();
                }
            }
        }

        public Vector3 Up {
            get {
                UpdateAxis();
                return m_Up;
            }

            set {
                if (m_Up != value) {
                    m_Up = value;
                    DoLookAtUpChange();
                }
            }
        }

        public Vector3 Right {
            get {
                UpdateAxis();
                return m_Right;
            }
        }
    }

}
