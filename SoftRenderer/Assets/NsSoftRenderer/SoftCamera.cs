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
        private Vector3 m_Right = Vector3.zero;
        private bool m_IsLookAtAndUpChged = true;

        private void UpdateRightAxis() {
            if (m_IsLookAtAndUpChged) {
                m_IsLookAtAndUpChged = false;
                m_Right = Vector3.Cross(m_LookAt, m_Up);
            }
        }

        public Vector3 Right {
            get {
                UpdateRightAxis();
                return m_Right;
            }
        }

        public void Update(float delta) {
            UpdateRightAxis();
        }

        // 摄影机类型
        public SoftCameraType CameraType {
            get {
                return m_CamType;
            }
        }
    }
}
