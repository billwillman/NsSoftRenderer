using UnityEngine;

namespace NsSoftRenderer {

    [RequireComponent(typeof(MeshFilter))]
    public class SoftMeshBinder: MonoBehaviour {
        // 材质
        public Material sharedMaterial = null;

        private MeshFilter m_MeshFilter = null;
        private SoftMeshRenderer m_SoftMeshRenderer = null;
        private void Start() {
            m_MeshFilter = GetComponent<MeshFilter>();
            var trans = this.transform;
            m_SoftMeshRenderer = SoftMeshRenderer.Create(trans.position, trans.up, trans.forward, m_MeshFilter.sharedMesh);
            UpdatePos();
        }

        private void UpdatePos() {
            if (m_SoftMeshRenderer != null) {
                var trans = this.transform;
                m_SoftMeshRenderer.Position = trans.position;
                m_SoftMeshRenderer.LookAt = trans.forward;
                m_SoftMeshRenderer.Up = trans.up;
                //m_SoftMeshRenderer.sharedMesh = m_MeshFilter.sharedMesh;
            }
        }

        private void Update() {
            UpdatePos();
        }

        private void OnDestroy() {
            if (m_SoftMeshRenderer != null) {
                m_SoftMeshRenderer.Dispose();
                m_SoftMeshRenderer = null;
            }
        }
    }
}