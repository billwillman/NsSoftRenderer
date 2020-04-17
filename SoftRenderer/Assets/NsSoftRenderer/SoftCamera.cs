
namespace NsSoftRenderer {

    // 摄影机类型
    public enum SoftCameraType {
        O, // 正交摄影机
        P  // 透视摄影机
    }

    // 软渲染摄影机
    public class SoftCamera {
        private SoftCameraType m_CamType = SoftCameraType.O;

        // 摄影机类型
        public SoftCameraType CameraType {
            get {
                return m_CamType;
            }
        }
    }
}
