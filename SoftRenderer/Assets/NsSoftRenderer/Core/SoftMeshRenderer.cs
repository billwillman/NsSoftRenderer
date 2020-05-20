using UnityEngine;

namespace NsSoftRenderer {

    public class SoftMeshRenderer : SoftRenderObject {

        private SoftMesh m_Mesh = null;
        private int m_MainTex = 0;

        internal SoftMeshRenderer(Vector3 pos, Vector3 up, Vector3 lookAt, Mesh mesh): base() {
            this.Position = pos;
            this.Up = up;
            this.LookAt = lookAt;
            this.m_Type = SoftRenderObjType.MeshRender;
            if (mesh != null) {
                m_Mesh = new SoftMesh(mesh);
            }
        }

        public static SoftMeshRenderer Create(Vector3 pos, Vector3 up, Vector3 lookAt, Mesh mesh) {
            var device = SoftDevice.StaticDevice;
            if (device !=  null) {
                SoftMeshRenderer ret = device.CreateMeshRenderer(pos, up, lookAt, mesh);
                return ret;
            }
            return null;
        }

        public int MainTex {
            get {
                return m_MainTex;
            }
            set {
                m_MainTex = value;
            }
        }

        // 世界坐标系包围球
        public SoftSpere WorldBoundSpere {
            get {
                if (m_Mesh != null) {
                    SoftSpere ret = m_Mesh.LocalBoundSpere;
                    ret.position = this.LocalToGlobalMatrix.MultiplyPoint(ret.position);
                    Triangle.CheckPtIntf(ref ret.position);
                    return ret;
                } else {
                    SoftSpere ret = new SoftSpere();
                    ret.position = Vector3.zero;
                    ret.radius = 0f;
                    return ret;
                }
            }
        }

        // 暂时这样
        private CullMode m_CullMode = CullMode.back;
        public CullMode cullMode
        {
            get
            {
                return m_CullMode;
            }
            set
            {
                m_CullMode = value;
            }
        }

        protected override void OnFree(bool isManual) {
            if (m_Mesh != null) {
                m_Mesh.Dispose();
                m_Mesh = null;
            }
        }

        public SoftMesh sharedMesh {
            get {
                return m_Mesh;
            }
            set {
                m_Mesh = value;
            }
        }

        // 模型空间转世界空间
        private Matrix4x4 m_MeshAxisToGlobalMatrix = Matrix4x4.identity;

        protected override void DoUpdateGlobalToLocalMatrix() {
            // 模型空间转到局部坐标系空间
            // 这里注意要做Z取反，是因为模型工具导出的模型坐标系和UNITY不一致
            Matrix4x4 modelViewToLocalView = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
            m_MeshAxisToGlobalMatrix = m_LocalToGlobalMatrix * modelViewToLocalView;
        }

        // 提交到渲染队列中
        public override bool Render(SoftCamera camera, RenderPassMode passMode) {
            if (camera == null || passMode == null || m_Mesh == null)
                return false;
            UpdateGlobalToLocalMatrix();
            CullMode old = passMode.Cull;
            passMode.Cull = this.cullMode;
            int oldMainTex = passMode.mainTex;
            passMode.mainTex = m_MainTex;
            bool ret = camera.RenderMesh(m_Mesh, m_MeshAxisToGlobalMatrix, passMode);
            passMode.Cull = old;
            passMode.mainTex = oldMainTex;
            return ret;
        }
    }

}