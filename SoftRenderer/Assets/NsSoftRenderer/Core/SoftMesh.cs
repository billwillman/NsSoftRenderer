using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace NsSoftRenderer {

    public class SoftSubMesh: DisposeObject {
        private IndexBuffer m_IndexBuffer = null;

        public SoftSubMesh(Mesh mesh, int[] indexes) {
            if (indexes != null && indexes.Length > 0) {
                m_IndexBuffer = new IndexBuffer();
                m_IndexBuffer.Capacity = indexes.Length;
                for (int i = 0; i < indexes.Length; ++i) {
                    m_IndexBuffer.Add(indexes[i]);
                }
            }
        }

        protected override void OnFree(bool isManual) {
            if (m_IndexBuffer != null) {
                m_IndexBuffer.Dispose();
                m_IndexBuffer = null;
            }
        }
    }

    // 模型
    public class SoftMesh: DisposeObject {
        private VertexBuffer m_VertexBuffer = null;
        private VertexColorBuffer m_ColorBuffer = null;
        private VertexNormalBuffer m_NormalBuffer = null;
        private List<SoftSubMesh> m_SubList = null;

        public SoftMesh(Mesh mesh) {
            BuildFromMesh(mesh);
        }

        public void BuildFromMesh(Mesh mesh) {
            Clear();
            if (mesh != null) {
                
                List<Vector3> vertexs = new List<Vector3>();
                mesh.GetVertices(vertexs);
                if (vertexs.Count > 0) {
                    m_VertexBuffer = new VertexBuffer();
                    m_VertexBuffer.Capacity = vertexs.Count;
                    for (int i = 0; i < vertexs.Count; ++i) {
                        m_VertexBuffer.Add(vertexs[i]);
                    }
                }
                List<Color> colors = new List<Color>();
                mesh.GetColors(colors);
                if (colors.Count > 0) {
                    m_ColorBuffer = new VertexColorBuffer();
                    m_ColorBuffer.Capacity = colors.Count;
                    for (int i = 0; i < colors.Count; ++i) {
                        m_ColorBuffer.Add(colors[i]);
                    }
                }

                for (int i = 0; i < mesh.subMeshCount; ++i) {
                    if (m_SubList == null)
                        m_SubList = new List<SoftSubMesh>();  
                    var triangles = mesh.GetTriangles(i);
                    if (triangles != null && triangles.Length > 0) {
                        SoftSubMesh subMesh = new SoftSubMesh(mesh, triangles);
                        m_SubList.Add(subMesh);
                    }
                }
            }
        }

        public void Clear() {

            if (m_SubList != null) {
                for (int i = 0; i < m_SubList.Count; ++i) {
                    var subMesh = m_SubList[i];
                    if (subMesh != null)
                        subMesh.Dispose();
                }
                m_SubList.Clear();
            }

            if (m_VertexBuffer != null) {
                m_VertexBuffer.Dispose();
                m_VertexBuffer = null;
            }

            if (m_ColorBuffer != null) {
                m_ColorBuffer.Dispose();
                m_ColorBuffer = null;
            }

            if (m_NormalBuffer != null) {
                m_NormalBuffer.Dispose();
                m_NormalBuffer = null;
            }
        }

        protected override void OnFree(bool isManual) {
            Clear();
        }
    }
}
