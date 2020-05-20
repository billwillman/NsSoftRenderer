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

        internal IndexBuffer Indexes {
            get {
                return m_IndexBuffer;
            }
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

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
        private UVBuffer m_UV1Buffer = null; // UV1坐标
        private List<SoftSubMesh> m_SubList = null;
        // 采用的是模型坐标系
        private SoftSpere m_BoundSpere;

        public SoftMesh(Mesh mesh) {
            BuildFromMesh(mesh);
        }

        internal List<SoftSubMesh> SubMeshes {
            get {
                return m_SubList;
            }
        }

        internal VertexBuffer Vertexs {
            get {
                return m_VertexBuffer;
            }
        }

        internal VertexColorBuffer Colors {
            get {
                return m_ColorBuffer;
            }
        }

        internal VertexNormalBuffer Normals {
            get {
                return m_NormalBuffer;
            }
        }

        internal UVBuffer UV1s {
            get {
                return m_UV1Buffer;
            }
        }

        // 局部坐标系的
        public SoftSpere LocalBoundSpere {
            get {
                return m_BoundSpere;
            }
        }

        public void BuildFromMesh(Mesh mesh) {
            Clear();
            if (mesh != null) {

                Vector3 minVec = Vector3.zero;
                Vector3 maxVec = Vector3.zero;
                bool isInitMinMax = false;
                List<Vector3> vertexs = new List<Vector3>();
                mesh.GetVertices(vertexs);
                if (vertexs.Count > 0) {
                    m_VertexBuffer = new VertexBuffer();
                    m_VertexBuffer.Capacity = vertexs.Count;
                    for (int i = 0; i < vertexs.Count; ++i) {
                        Vector3 v = vertexs[i];
                        m_VertexBuffer.Add(v);
                        if (!isInitMinMax) {
                            minVec = v;
                            maxVec = v;
                            isInitMinMax = true;
                        } else {
                            if (minVec.x > v.x)
                                minVec.x = v.x;
                            if (minVec.y > v.y)
                                minVec.y = v.y;
                            if (minVec.z > v.z)
                                minVec.z = v.z;
                            if (maxVec.x < v.x)
                                maxVec.x = v.x;
                            if (maxVec.y < v.y)
                                maxVec.y = v.y;
                            if (maxVec.z < v.z)
                                maxVec.z = v.z;
                        }
                    }
                }

                if (isInitMinMax) {
                    m_BoundSpere.position = (maxVec + minVec) / 2.0f;
                    m_BoundSpere.radius = (maxVec - minVec).magnitude / 2.0f; 
                } else {
                    m_BoundSpere.position = Vector3.zero;
                    m_BoundSpere.radius = 0f;
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

                // UV 1坐标
                List<Vector4> uvs = new List<Vector4>();
                mesh.GetUVs(0, uvs);
                if (uvs.Count > 0) {
                    if (m_UV1Buffer == null)
                        m_UV1Buffer = new UVBuffer();
                    m_UV1Buffer.Capacity = uvs.Count;
                    for (int i = 0; i < uvs.Count; ++i) {
                        m_UV1Buffer.Add(uvs[i]);
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

            if (m_UV1Buffer != null) {
                m_UV1Buffer.Dispose();
                m_UV1Buffer = null;
            }
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            Clear();
        }
    }
}
