using UnityEngine;

namespace NsSoftRenderer {

    public static class SoftMath {

        private static readonly float EPS = 1e-6f;

        // 判断点是否在三角形上（三维坐标）
        public static bool PtInTriangle(Vector3 pt, Triangle trangle, out float h) {
            // 采用重心法
            Vector3 v2 = pt - trangle.p1;
            Vector3 v0 = trangle.p2 - trangle.p1;
            Vector3 v1 = trangle.p3 - trangle.p1;

            // v2 = u * v0 + v * v1 =>
            // (v2) • v0 = (u * v0 + v * v1) • v0
            // (v2) • v1 = (u * v0 + v * v1) • v1

            // Compute scaled barycentric coordinates
            float denom = v0[0] * v1[2] - v0[2] * v1[0];
            if (Mathf.Abs(denom) < EPS) {
                h = 0f;
                return false;
            }

            float u = v1[2] * v2[0] - v1[0] * v2[2];
            float v = v0[0] * v2[2] - v0[2] * v2[0];

            if (denom < 0) {
                denom = -denom;
                u = -u;
                v = -v;
            }

            // If point lies inside the triangle, return interpolated ycoord.
            if (u >= 0.0f && v >= 0.0f && (u + v) <= denom) {
                h = trangle.p1[1] + (v0[1] * u + v1[1] * v) / denom;
                return true;
            }

            h = 0f;
            return false;
        }
    }
}