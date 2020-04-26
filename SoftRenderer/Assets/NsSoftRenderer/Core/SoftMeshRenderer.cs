using UnityEngine;

namespace NsSoftRenderer {

    public class SoftMeshRenderer: SoftRenderObject {

        private Mesh m_Mesh = null;

        public SoftMeshRenderer(Vector3 pos, Vector3 up, Vector3 lookAt, Mesh mesh) {
            this.Position = pos;
            this.Up = up;
            this.LookAt = lookAt;
            m_Mesh = mesh;
        }

        public Mesh sharedMesh {
            get {
                return m_Mesh;
            }
            set {
                m_Mesh = value;
            }
    }

}