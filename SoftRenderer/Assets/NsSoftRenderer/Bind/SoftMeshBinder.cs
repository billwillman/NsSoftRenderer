using System.Collections.Generic;
using UnityEngine;

namespace NsSoftRenderer {

    [RequireComponent(typeof(MeshFilter))]
    public class SoftMeshBinder: MonoBehaviour {
        // 材质
        public CullMode cullMode = CullMode.back;
        //public bool isMeshRevert_Z = false;

        private Mesh m_CustomMesh = null;
        private MeshFilter m_MeshFilter = null;
        private SoftMeshRenderer m_SoftMeshRenderer = null;

        private void CheckTex() {
            Texture2D mainTex = null;
            MeshRenderer renderer = this.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.sharedMaterial != null) {
                mainTex = renderer.sharedMaterial.mainTexture as Texture2D;
            }

            if (mainTex != null) {
                var device = SoftDevice.StaticDevice;
                if (device != null && m_SoftMeshRenderer != null) {
                    var resMgr = device.ResMgr;
                    if (resMgr != null) {
                        int mainTexId = resMgr.LoadFromTexture2D(mainTex);
                        m_SoftMeshRenderer.MainTex = mainTexId;
                        var tex = resMgr.GetSoftRes<SoftTexture2D>(mainTexId);
                        tex.TexFilter = TextureFliter.Biller;
                    }
                }
            }
        }

        private void Start() {
            m_MeshFilter = GetComponent<MeshFilter>();
            CheckMesh();
            var trans = this.transform;

            Mesh mesh = this.editorMesh;

            m_SoftMeshRenderer = SoftMeshRenderer.Create(trans.position, trans.up, trans.forward, mesh);
            UpdatePos();

            CheckTex();
        }

        protected Mesh editorMesh {
            get {
                Mesh mesh;
                if (m_CustomMesh != null)
                    mesh = m_CustomMesh;
                else
                    mesh = m_MeshFilter.sharedMesh;
                return mesh;
            }
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
                    vs.Add(new Vector3(0, 1, 0));
                    vs.Add(new Vector3(1, 0, 0));
                    vs.Add(new Vector3(0, -1, 0));
                    
                    m_CustomMesh.SetVertices(vs);

                    m_CustomMesh.subMeshCount = 1;
                    int[] idxs = new int[6];
                    idxs[0] = 0; idxs[1] = 1; idxs[2] = 2;
                    idxs[3] = 0; idxs[4] = 2; idxs[5] = 3;
                    m_CustomMesh.SetIndices(idxs, MeshTopology.Triangles, 0);

                    List<Color> colors = new List<Color>();
                    colors.Add(Color.red);
                    colors.Add(Color.green);
                    colors.Add(Color.blue);
                    colors.Add(Color.white);
                    m_CustomMesh.SetColors(colors);

                    List<Vector4> uvs = new List<Vector4>();
                    uvs.Add(new Vector4(0, 0, 0, 0));
                    uvs.Add(new Vector4(1, 0, 0, 0));
                    uvs.Add(new Vector4(1, 1, 0, 0));
                    uvs.Add(new Vector4(0, 1, 0, 0));
                    m_CustomMesh.SetUVs(0, uvs);
                }

                m_MeshFilter.sharedMesh = m_CustomMesh;
            } else if (m_MeshFilter != null && m_MeshFilter.sharedMesh != null) {
                //Matrix4x4 mat = m_MeshFilter.GetComponent<MeshRenderer>().localToWorldMatrix * this.transform.worldToLocalMatrix;
                /*Mesh mesh;
                
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
                }*/

                Mesh mesh = mesh = m_MeshFilter.sharedMesh;

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
                m_SoftMeshRenderer.Scale = trans.lossyScale;
                m_SoftMeshRenderer.cullMode = this.cullMode;
                //m_SoftMeshRenderer.sharedMesh = m_MeshFilter.sharedMesh;

            //    Debug.LogErrorFormat("[Unity] {0} [Soft] {1}", 
            //        this.transform.localToWorldMatrix *, m_SoftMeshRenderer.LocalToGlobalMatrix.ToString());
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
 