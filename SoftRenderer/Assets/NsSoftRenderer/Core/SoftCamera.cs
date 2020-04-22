using UnityEngine;


using RenderTargetClearFlags = System.Int32;

namespace NsSoftRenderer {

    // 摄影机类型
    public enum SoftCameraType {
        O, // 正交摄影机
        P  // 透视摄影机
    }

    // 透视摄影机数据
    public struct PCameraInfo {
        public float nearPlane, farPlane;
        public float fieldOfView;  // 角度制

        public void ResetDefault() {
            nearPlane = 0.3f;
            farPlane = 1000f;
            fieldOfView = 60.0f;
        }

        public static PCameraInfo Create() {
            PCameraInfo ret = new PCameraInfo();
            ret.ResetDefault();
            return ret;
        }

        // 近平面高度
        public float nearHeight {
            get {
                float halfAngle = fieldOfView / 2.0f * Mathf.PI/180.0f; // 弧度制
                float ret = 2.0f * Mathf.Tan(halfAngle) * nearPlane;
                return ret;
            }
        }

        // 远平面高度
        public float farHeight {
            get {
                float halfAngle = fieldOfView / 2.0f * Mathf.PI / 180.0f; // 弧度制
                float ret = 2.0f * Mathf.Tan(halfAngle) * farPlane;
                return ret;
            }
        }

        // 获得近平面宽度
        public float GetNearWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            float ret = aspect * nearHeight;
            return ret;
        }

        // 获得远平面宽度
        public float GetFarWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float aspect = w / h;
            float ret = aspect * farHeight;
            return ret;
        }

        // 只有透视的矩阵，没有做0-2的范围的，那是缩放
        public Matrix4x4 PMatrix {
            get {
                Matrix4x4 mat = Matrix4x4.zero;
                mat.m00 = nearPlane;
                mat.m11 = nearPlane;
                mat.m22 = farPlane + nearPlane;
                mat.m23 = -nearPlane * farPlane;
                mat.m32 = 1.0f;
                return mat;
            }
        }
    }

    // 正交摄影机数据
    public struct OCameraInfo {
        public float Size;

        public void ResetDefault() {
            Size = 5.0f;
        }

        public static OCameraInfo Create() {
            OCameraInfo ret = new OCameraInfo();
            ret.ResetDefault();
            return ret;
        }

        public float CameraHeight {
            get {
                return Size * 2.0f;
            }
        }

        public float GetCameraWidth(int deviceWidth, int deviceHeight) {
            float w = (float)deviceWidth;
            float h = (float)deviceHeight;
            float ret = w / h * CameraHeight;
            return ret;
        }
    }

    // 相机通知
    public interface ISoftCameraLinker {
        void OnCameraDepthChanged();

        int DeviceWidth {
            get;
        }

        int DeviceHeight {
            get;
        }
    }

    // 软渲染摄影机
    public class SoftCamera: SoftRenderObject {
        private static SoftCamera m_MainCamera = null;

        private SoftCameraType m_CamType = SoftCameraType.O;
        // 是否是主摄像机
        private bool m_IsMainCamera = false;

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
        // 透视摄影机
        private PCameraInfo m_PCameraInfo;
        // 正交摄影机
        private OCameraInfo m_OCameraInfo;

        private ISoftCameraLinker m_Linker = null;

        private int m_Depth = 0;
        // 观测和投影矩阵
        private Matrix4x4 m_ViewProjMatrix = Matrix4x4.identity;
        private Matrix4x4 m_ViewMatrix = Matrix4x4.identity;
        private Matrix4x4 m_ProjMatrix = Matrix4x4.identity;
        private Matrix4x4 m_LinkerScreenMatrix = Matrix4x4.identity;
        // 世界坐标系转屏幕坐标系
        private Matrix4x4 m_ViewProjLinkerScreenMatrix = Matrix4x4.identity;
        // 渲染目标
        // private RenderTarget m_RenderTarget = null;

        private void UpdateLinkerScreenMatrix() {
            if (m_Linker != null) {
                float w = (float)m_Linker.DeviceWidth;
                float h = (float)m_Linker.DeviceHeight;
                Vector3 scale = new Vector3(w / 2.0f, h / 2.0f, 1.0f);
                m_LinkerScreenMatrix = Matrix4x4.Scale(scale);
            } else {
                m_LinkerScreenMatrix = Matrix4x4.identity;
            }
        }

        public Matrix4x4 ViewProjLinkerScreenMatrix {
            get {
                return m_ViewProjLinkerScreenMatrix;
            }
        }

        // 渲染到RenderTarget
        internal void FlipToRenderTarget(TriangleVertexColor trangleInfo, RenderTarget renderTarget) {

        }

        public Matrix4x4 LinkerScreenMatrix {
            get {
                return m_LinkerScreenMatrix;
            }
        } 

        public static SoftCamera MainCamera {
            get {
                return m_MainCamera;
            }
        }

        public bool IsMainCamera {
            get {
                return m_IsMainCamera;
            }

            set {
                if (m_IsMainCamera != value) {
                    m_IsMainCamera = value;
                   
                   if (value) {
                        if (m_MainCamera != null)
                            m_MainCamera.IsMainCamera = false;
                        m_MainCamera = this;
                    } else {
                        if (m_MainCamera == this)
                            m_MainCamera = null;
                    }
                }
            }
        }

        public void SetPCamera(PCameraInfo info) {
            m_PCameraInfo = info;
            this.CameraType = SoftCameraType.P;
        }

        public void SetOCamera(OCameraInfo info) {
            m_OCameraInfo = info;
            this.CameraType = SoftCameraType.O;
        }

        public SoftCamera(ISoftCameraLinker linker): base() {
            m_Linker = linker;
            UpdateLinkerScreenMatrix();
        }

        public int Depth {
            get {
                return m_Depth;
            }
            set {
                if (m_Depth != value) {
                    m_Depth = value;
                    OnDepthChanged();
                }
            }
        }

        private void OnDepthChanged() {
            if (m_Linker != null)
                m_Linker.OnCameraDepthChanged();
        }

        public Matrix4x4 ViewMatrix {
            get {
                UpdateMatrix();
                return m_ViewMatrix;
            }
        }

        public Matrix4x4 ViewProjMatrix {
            get {
                UpdateMatrix();
                return m_ViewProjMatrix;
            }
        }

        public Matrix4x4 ProjMatrix {
            get {
                UpdateMatrix();
                return m_ProjMatrix;
            }
        }

        private void DoMatrixChange() {
            m_IsMustChgMatrix = true;
        }

        private void DoLookAtUpChange() {
            m_IsLookAtAndUpChged = true;
        }

        // 更新轴
        private void UpdateAxis() {
            if (m_IsLookAtAndUpChged) {
                m_IsLookAtAndUpChged = false;
                m_LookAt = m_LookAt.normalized;
                m_Right = Vector3.Cross(m_LookAt, m_Up).normalized;
                m_Up = Vector3.Cross(m_Right, m_LookAt);
            }
        }

        private void UpdateViewMatrix() {
            Matrix4x4 invTranslate = Matrix4x4.Translate(-m_Position);
            Matrix4x4 axis = Matrix4x4.identity;
            axis.m00 = m_Right.x; axis.m01 = m_Right.y; axis.m02 = m_Right.z;
            axis.m10 = m_Up.x;  axis.m11 = m_Up.y; axis.m12 = m_Up.z;
            axis.m20 = m_LookAt.x; axis.m21 = m_LookAt.y; axis.m22 = m_LookAt.z;
            m_ViewMatrix = axis * invTranslate;
        }

        private void UpdateOProjMatrix() {
            if (m_Linker != null) {
                int deviceWidth = m_Linker.DeviceWidth;
                int deviceHeight = m_Linker.DeviceHeight;
                float w = m_OCameraInfo.GetCameraWidth(deviceWidth, deviceHeight);
                float h = m_OCameraInfo.CameraHeight;

                 Vector3 offset = new Vector3(-w / 2.0f, -h / 2.0f, 0f);
                 Matrix4x4 translate = Matrix4x4.Translate(offset);

                // 投影矩阵: 范围:X 0-2, Y 0-2, Z 0-W 
                Vector3 scale = new Vector3(2.0f / w, 2.0f / h, 1.0f);
                m_ProjMatrix = translate * Matrix4x4.Scale(scale);
            }
        }

        private void UpdatePProjMatrix() {
            
        }

        // 投影矩阵
        private void UpdateProjMatrix() {
            switch (m_CamType) {
                // 正交
                case SoftCameraType.O:
                    UpdateOProjMatrix();
                    break;
                // 透视
                case SoftCameraType.P:
                    UpdatePProjMatrix();
                    break;
            }
        }

        private void UpdateViewProjMatrix() {
            m_ViewProjMatrix = m_ProjMatrix * m_ViewMatrix;
        }

        private void UpdateViewProjLinerScreenMatrix() {
            m_ViewProjLinkerScreenMatrix = m_LinkerScreenMatrix * m_ViewProjMatrix;
        }

        private void UpdateMatrix() {
            if (m_IsMustChgMatrix) {
                m_IsMustChgMatrix = false;
                // 更新坐标
                UpdateAxis();
                // 更新观察矩阵
                UpdateViewMatrix();
                // 更新投影矩阵
                UpdateProjMatrix();
                // 更新观察投影矩阵
                UpdateViewProjMatrix();
                UpdateViewProjLinerScreenMatrix();
            }
        }

        public Vector3 Right {
            get {
                UpdateAxis();
                return m_Right;
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
                    DoMatrixChange();
                }
            }
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
                    DoMatrixChange();
                }
            }
        }

        public Vector3 Position {
            get {
                return m_Position;
            }

            set {
                if (m_Position != value) {
                    m_Position = value;
                    DoMatrixChange();
                }
            }
        }

        public void Update(float delta) {
            UpdateMatrix();
        }

        // 摄影机类型
        public SoftCameraType CameraType {
            get {
                return m_CamType;
            }
            set {
                if (m_CamType != value) {
                    m_CamType = value;
                    DoMatrixChange();
                }
            }
        }
    }
}
