using UnityEngine;
using Utils;

namespace NsSoftRenderer {

    public enum SoftRenderObjType {
        None,
        Camera,
        MeshRender,
        SkinedMeshRender
    }

    // 所有3D物件类基
    public class SoftRenderObject: DisposeObject {
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
        protected Matrix4x4 m_LocalToGlobalMatrix = Matrix4x4.identity;
        protected bool m_MustGlobalToLocalMatrixChg = true;
        protected SoftRenderObjType m_Type = SoftRenderObjType.None;

        // 是否能渲染
        public bool CanRenderer {
            get {
                bool ret = (m_Type == SoftRenderObjType.MeshRender) || (m_Type == SoftRenderObjType.SkinedMeshRender);
                return ret;
            }
        }

        public SoftRenderObjType ObjType {
            get {
                return m_Type;
            }
        }

        public Matrix4x4 LocalToGlobalMatrix {
            get {
                UpdateGlobalToLocalMatrix();
                return m_LocalToGlobalMatrix;
            }
        }

        public Matrix4x4 GlobalToLocalMatrix {
            get {
                UpdateGlobalToLocalMatrix();
                return m_GlobalToLocalMatrix;
            }
        }

        protected virtual void DoMustGlobalToLocalMatrixChg() {
            m_MustGlobalToLocalMatrixChg = true;
        }

        protected override void OnFree(bool isManual) {
            var device = SoftDevice.StaticDevice;
            if (device != null) {
                device.RemoveRenderObject(this);
            }
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

        public virtual void Update(float delta) { }

        public virtual void Render(SoftCamera camera, RenderPassMode passMode) { }

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
                Matrix4x4 invTranslate = Matrix4x4.Translate(-m_Position);
                Matrix4x4 axis = Matrix4x4.identity;
                axis.m00 = m_Right.x; axis.m01 = m_Right.y; axis.m02 = m_Right.z;
                axis.m10 = m_Up.x; axis.m11 = m_Up.y; axis.m12 = m_Up.z;
                axis.m20 = m_LookAt.x; axis.m21 = m_LookAt.y; axis.m22 = m_LookAt.z;
                m_GlobalToLocalMatrix = axis * invTranslate;

                // 简单转置一下作为正交矩阵
                m_LocalToGlobalMatrix = m_GlobalToLocalMatrix.transpose;
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
    }

}
