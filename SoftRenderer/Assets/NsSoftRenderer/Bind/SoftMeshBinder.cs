using System.Collections.Generic;
using UnityEngine;

namespace NsSoftRenderer {

    [RequireComponent(typeof(MeshFilter))]
    public class SoftMeshBinder: MonoBehaviour {
        // 材质
        public Material sharedMaterial = null;

        private Mesh m_CustomMesh = null;
        private MeshFilter m_MeshFilter = null;
        private SoftMeshRenderer m_SoftMeshRenderer = null;
        private void Start() {
            m_MeshFilter = GetComponent<MeshFilter>();
            CheckMesh();
            var trans = this.transform;
            m_SoftMeshRenderer = SoftMeshRenderer.Create(trans.position, trans.up, trans.forward, m_MeshFilter.sharedMesh);
            UpdatePos();
        }

        private void CheckMesh()
        {
            if (m_MeshFilter != null && m_MeshFilter.sharedMesh == null)
            {
                // 默认用一个三角形
                if (m_CustomMesh == null)
                {
                    m_CustomMesh = new Mesh();
                    List<Vector3> vs = new List<Vector3>();
                    vs.Add(new Vector3(-1, 0, 0));
                    vs.Add(new Vector3(1, 0, 0));
                    vs.Add(new Vector3(1, 1, 0));
                    m_CustomMesh.SetVertices(vs);

                    m_CustomMesh.subMeshCount = 1;
                    int[] idxs = new int[3];
                    idxs[0] = 0; idxs[1] = 2; idxs[2] = 1;
                    m_CustomMesh.SetIndices(idxs, MeshTopology.Triangles, 0);

                    List<Color> colors = new List<Color>();
                    colors.Add(Color.red);
                    colors.Add(Color.green);
                    colors.Add(Color.blue);
                    m_CustomMesh.SetColors(colors);
                }

                m_MeshFilter.sharedMesh = m_CustomMesh;
            }
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
            if (m_CustomMesh != null)
            {
                GameObject.Destroy(m_CustomMesh);
                m_CustomMesh = null;
            }
        }
    }
}