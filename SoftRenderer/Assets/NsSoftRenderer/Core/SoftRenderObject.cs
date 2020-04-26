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
        protected Matrix4x4 m_GlobalToLocalMatrix = Matrix4x4.identity;
        protected bool m_MustGlobalToLocalMatrixChg = true;

        protected virtual void DoMustGlobalToLocalMatrixChg() {
            m_MustGlobalToLocalMatrixChg = true;
        }

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
                    DoMustGlobalToLocalMatrixChg();
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

        protected void UpdateGlobalToLocalMatrix() {
            if (m_MustGlobalToLocalMatrixChg) {
                m_MustGlobalToLocalMatrixChg = false;
                Matrix4x4 invTransMat = Matrix4x4.Translate(-this.m_Position);
                Matrix4x4 axisMat = Matrix4x4.zero;

                Vector3 xAxis = this.Right;
                Vector3 yAxis = this.Up;
                Vector3 zAxis = this.LookAt;

                axisMat.SetRow(0, xAxis);
                axisMat.SetRow(1, yAxis);
                axisMat.SetRow(2, zAxis);

                m_GlobalToLocalMatrix = axisMat * invTransMat;
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
                    DoMustGlobalToLocalMatrixChg();
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
                    DoMustGlobalToLocalMatrixChg();
                }
            }
        }

        public Vector3 Right {
            get {
                UpdateAxis();
                return m_Right;
            }
        }

        // 全局坐标转局部坐标
        public Matrix4x4 GlobalToLocalMatrix {
            get {
                UpdateGlobalToLocalMatrix();
                return m_GlobalToLocalMatrix;
            }
        }
    }

}
