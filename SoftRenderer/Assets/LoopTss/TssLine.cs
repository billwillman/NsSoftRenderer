using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace TssLoop {


    using TssLineMap = Dictionary<long, TssLine>;

    // 三角形边
    public struct TssLine {
        // 保证一点pt1的值小于pt2的值，这样解决边的顶点前后关系，好做HASH
        public int pt1;   // 第一个顶点索引
        public int pt2;   // 第二个顶点索引
    }


    // 三角形由三条边组成
    public struct TssTriangle {
        public int[] vertIdxs;
        public long line1, line2, line3; // 全是索引而且也是从小到大排序

        public static long MakeLineKey(int pt1, int pt2) {
            if (pt1 > pt2) {
                int tmp = pt1;
                pt1 = pt2;
                pt2 = tmp;
            }

            long ret = ((long)pt1 << 32) | ((long)pt2);
            return ret;
        }

        private static void CheckAddLineMap(TssLineMap lineMap, int pt1, int pt2) {
            if (pt1 > pt2) {
                int tmp = pt1;
                pt1 = pt2;
                pt2 = tmp;
            }

            long key = MakeLineKey(pt1, pt2);

            if (!lineMap.ContainsKey(key)) {
                TssLine line = new TssLine();
                line.pt1 = pt1; line.pt2 = pt2;
                lineMap.Add(key, line);
            }
        }


        public TssTriangle(int idx1, int idx2, int idx3, TssLineMap lineMap) {
            vertIdxs = new int[3];
            vertIdxs[0] = idx1;
            vertIdxs[1] = idx2;
            vertIdxs[2] = idx3;

            line1 = MakeLineKey(vertIdxs[0], vertIdxs[1]);
            CheckAddLineMap(lineMap, vertIdxs[0], vertIdxs[1]);
            line2 = MakeLineKey(vertIdxs[1], vertIdxs[2]);
            CheckAddLineMap(lineMap, vertIdxs[1], vertIdxs[2]);
            line3 = MakeLineKey(vertIdxs[2], vertIdxs[0]);
            CheckAddLineMap(lineMap, vertIdxs[2], vertIdxs[0]);
        }
    }

    public class TssLineBuffer : NativeList<TssLine> { }

    // 顶点数据
    public class TssVertexBuffer : NativeList<Vector3> { }

    // 顶点索引数据
    public class TssTriangleBuffer : NativeList<TssTriangle> { }

    // 细分过后，法线得重新计算
    public class TssMesh: DisposeObject {

        private TssVertexBuffer m_VertexBuffer = new TssVertexBuffer();
        private TssTriangleBuffer m_TriangleBuffer = new TssTriangleBuffer();
        private TssLineMap m_LinesMap = new TssLineMap();

        // 从Mesh加载
        public void LoadFromMesh(Mesh mesh, int subMesh = 0) {

            Clear();

            List<Vector3> vertList = new List<Vector3>();
            mesh.GetVertices(vertList);
            m_VertexBuffer.Capacity = vertList.Count;
            for (int i = 0; i < vertList.Count; ++i) {
                m_VertexBuffer.Add(vertList[i]);
            }
            int[] tris = mesh.GetTriangles(subMesh);
            m_TriangleBuffer.Capacity = tris.Length;
            for (int i = 0; i < (int)(tris.Length/3); ++i) {
                int idx = i * 3;
                int idx1 = tris[idx++];
                int idx2 = tris[idx++];
                int idx3 = tris[idx++];
                TssTriangle tri = new TssTriangle(idx1, idx2, idx3, m_LinesMap);
                m_TriangleBuffer.Add(tri);
            }
        }

        private void Clear() {
            m_LinesMap.Clear();

            if (m_VertexBuffer != null) {
                m_VertexBuffer.Clear();
            }

            if (m_TriangleBuffer != null) {
                m_TriangleBuffer.Clear();
            }
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_VertexBuffer != null) {
                m_VertexBuffer.Dispose();
                m_VertexBuffer = null;
            }

            if (m_TriangleBuffer != null) {
                m_TriangleBuffer.Dispose();
                m_TriangleBuffer = null;
            }
        }
    }
}