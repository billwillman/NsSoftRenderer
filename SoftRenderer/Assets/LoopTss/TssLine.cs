using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace TssLoop {


    using TssLineMap = Dictionary<long, TssLine>;
    // 顶点的邻点MAP KEY=>当前点索引 VALUE=>邻点索引
    using TssVertexRefMap = Dictionary<int, List<int>>;

    public enum TssLineType {
        None,
        TwoTri, // 两个三角形共边的线
        OneTri // 一个三角形共边
    }

    // 三角形边
    public struct TssLine {
        // 保证一点pt1的值小于pt2的值，这样解决边的顶点前后关系，好做HASH
        public int pt1;   // 第一个顶点索引
        public int pt2;   // 第二个顶点索引

        public int tri1, tri2;

        public TssLineType LineType {
            get {
                if (tri1 < 0)
                    return TssLineType.None;
                if (tri2 < 0)
                    return TssLineType.OneTri;
                return TssLineType.TwoTri;
            }
        }

        public Vector3 GetPt1Vec(TssVertexBuffer verts) {
            return verts[pt1];
        }

        public Vector3 GetPt2Vec(TssVertexBuffer verts) {
            return verts[pt2];
        }
    }

    // 三角形由三条边组成
    public struct TssTriangle {
        public int vertIdx1, vertIdx2, vertIdx3;
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

        private static void CheckAddLineMap(TssLineMap lineMap, TssTriangleBuffer triBuf, int pt1, int pt2) {
            if (pt1 > pt2) {
                int tmp = pt1;
                pt1 = pt2;
                pt2 = tmp;
            }

            long key = MakeLineKey(pt1, pt2);

            TssLine line;
            if (!lineMap.TryGetValue(key, out line)) {
                line = new TssLine();
                line.pt1 = pt1; line.pt2 = pt2;
                line.tri1 = triBuf.Count;
                line.tri2 = -1;
                lineMap.Add(key, line);
            } else {
                line.tri2 = triBuf.Count;
                lineMap[key] = line;
            }
        }

        private static void AddVertRef(int key, int value, TssVertexRefMap refMap) {
            List<int> refLst;
            if (refMap.TryGetValue(key, out refLst)) {
                refLst.Add(value);
            } else {
                refLst = new List<int>();
                refLst.Add(value);
                refMap.Add(key, refLst);
            }
        }


        public TssTriangle(int idx1, int idx2, int idx3, TssLineMap lineMap, TssTriangleBuffer triBuf, TssVertexRefMap refMap) {
            vertIdx1 = idx1;
            vertIdx2 = idx2;
            vertIdx3 = idx3;

            AddVertRef(vertIdx1, vertIdx2, refMap);
            AddVertRef(vertIdx1, vertIdx3, refMap);
            AddVertRef(vertIdx2, vertIdx1, refMap);
            AddVertRef(vertIdx2, vertIdx3, refMap);
            AddVertRef(vertIdx3, vertIdx1, refMap);
            AddVertRef(vertIdx3, vertIdx2, refMap);

            line1 = MakeLineKey(vertIdx1, vertIdx2);
            CheckAddLineMap(lineMap, triBuf, vertIdx1, vertIdx2);
            line2 = MakeLineKey(vertIdx2, vertIdx3);
            CheckAddLineMap(lineMap, triBuf, vertIdx2, vertIdx3);
            line3 = MakeLineKey(vertIdx3, vertIdx1);
            CheckAddLineMap(lineMap, triBuf, vertIdx3, vertIdx1);
        }
    }

    public class TssLineBuffer : NativeList<TssLine> { }

    // 顶点数据
    public class TssVertexBuffer : NativeList<Vector3> { }

    // 顶点索引数据
    public class TssTriangleBuffer : NativeList<TssTriangle> {

    }

    // 新的顶点三角形
    public struct TssExtrTriangle {
        public Vector3 p1, p2, p3; // 新的点
        public long line1, line2, line3; // 初始生成在的边
    }

    public class TssExtrTringleBuffer: NativeList<TssExtrTriangle> { }

    // 细分过后，法线得重新计算
    public class TssMesh: DisposeObject {

        private TssVertexBuffer m_VertexBuffer = new TssVertexBuffer();
        private TssVertexRefMap m_VertexRefMap = new TssVertexRefMap();
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
                TssTriangle tri = new TssTriangle(idx1, idx2, idx3, m_LinesMap, m_TriangleBuffer, m_VertexRefMap);
                m_TriangleBuffer.Add(tri);
            }
        }

        // 生成下一級的細分
        public void TssNextLevel() {
            TssExtrTringleBuffer extrTris = new TssExtrTringleBuffer();

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