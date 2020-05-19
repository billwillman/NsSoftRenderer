using System.Collections.Generic;
using UnityEngine;

namespace NsSoftRenderer {

    [RequireComponent(typeof(MeshFilter))]
    public class SoftMeshBinder: MonoBehaviour {
        // 材质
        public Material sharedMaterial = null;
        public CullMode cullMode = CullMode.back;
        public bool isMeshRevert_Z = false;

        private Mesh m_CustomMesh = null;
        private MeshFilter m_MeshFilter = null;
        private SoftMeshRenderer m_SoftMeshRenderer = null;
        private void Start() {
            m_MeshFilter = GetComponent<MeshFilter>();
            CheckMesh();
            var trans = this.transform;

            Mesh mesh;
            if (m_CustomMesh != null)
                mesh = m_CustomMesh;
            else
                mesh = m_MeshFilter.sharedMesh;

            m_SoftMeshRenderer = SoftMeshRenderer.Create(trans.position, trans.up, trans.forward, mesh);
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
                    vs.Add(new Vector3(0, 0, -1));
                    vs.Add(new Vector3(0, 0, 1));
                    vs.Add(new Vector3(0, 1, 1));
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
            } else if (m_MeshFilter != null && m_MeshFilter.sharedMesh != null) {
                //Matrix4x4 mat = m_MeshFilter.GetComponent<MeshRenderer>().localToWorldMatrix * this.transform.worldToLocalMatrix;
                Mesh mesh;
                if (isMeshRevert_Z) {
                    m_CustomMesh = GameObject.Instantiate(m_MeshFilter.sharedMesh);

                    List<Vector3> lst = new List<Vector3>();
                    Matrix4x4 mat = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                    m_CustomMesh.GetVertices(lst);
                    for (int i = 0; i < lst.Count; ++i) {
                        lst[i] = mat.MultiplyPoint3x4(lst[i]);
                    }
                    m_CustomMesh.SetVertices(lst);

                    mesh = m_CustomMesh;
                } else {
                    mesh = m_MeshFilter.sharedMesh;
                }

                List<Color> colors = new List<Color>();
                mesh.GetColors(colors);
                if (colors.Count <= 0 && mesh.vertexCount > 0) {
                    colors.Capacity = mesh.vertexCount;
                    for (int i = 0; i < mesh.vertexCount; ++i) {
                        colors.Add(new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), 1f));
                    }
                    mesh.SetColors(colors);
                    if (mesh != m_MeshFilter.sharedMesh)
                        m_MeshFilter.sharedMesh.SetColors(colors);
                }
            }
        }

        private void UpdatePos() {
            if (m_SoftMeshRenderer != null) {
                var trans = this.transform;
                m_SoftMeshRenderer.Position = trans.position;
                m_SoftMeshRenderer.LookAt = trans.forward;
                m_SoftMeshRenderer.Up = trans.up;
                m_SoftMeshRenderer.cullMode = this.cullMode;
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