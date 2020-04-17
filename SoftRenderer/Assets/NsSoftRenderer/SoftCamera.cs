using UnityEngine;


namespace NsSoftRenderer {

    // 摄影机类型
    public enum SoftCameraType {
        O, // 正交摄影机
        P  // 透视摄影机
    }

    // 软渲染摄影机
    public class SoftCamera {
        private SoftCameraType m_CamType = SoftCameraType.O;

        // 观测方向
        private Vector3 m_LookAt = new Vector3(0, 0, -1f);
        // UP方向
        private Vector3 m_Up = new Vector3(0, 1f, 0);
        // Right方向
        private Vector3 m_Right = Vector3.zero;
        // 位置
        private Vector3 m_Position = Vector3.zero;
        private bool m_IsLookAtAndUpChged = true;
        private bool m_IsMustChgMatrix = true;
        // 观测和投影矩阵
        private Matrix4x4 m_ViewProjMatrix = Matrix4x4.identity;

        // 更新轴
        private void UpdateAxis() {
            if (m_IsLookAtAndUpChged) {
                m_IsLookAtAndUpChged = false;
                m_LookAt = m_LookAt.normalized;
                m_Right = Vector3.Cross(m_LookAt, m_Up).normalized;
                m_Up = Vector3.Cross(m_Right, m_LookAt);
            }
        }

        private void UpdateMatrix() {
            if (m_IsMustChgMatrix) {
                m_IsMustChgMatrix = false;
                // 更新矩阵
            }
        }

        public Vector3 Right {
            get {
                UpdateAxis();
                return m_Right;
            }
        }

        public Vector3 Position {
            get {
                return m_Position;
            }

            set {
                if (m_Position != value) {
                    m_Position = value;
                    m_IsMustChgMatrix = true;
                }
            }
        }

        public void Update(float delta) {
            UpdateAxis();
            UpdateMatrix();
        }

        // 摄影机类型
        public SoftCameraType CameraType {
            get {
                return m_CamType;
            }
        }
    }
}
