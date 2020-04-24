using UnityEngine;

namespace NsSoftRenderer {

    // 平面描述
    public struct SoftPlane {
        public Vector3 normal;
        public float d;

        public SoftPlane(Vector3 n, float d) {
            this.normal = n;
            this.d = d;
        }

        public override string ToString() {
            string ret = string.Format("【normal】{0},{1},{2}【d】{3}", normal.x.ToString(), normal.y.ToString(), normal.z.ToString(), d.ToString());
            return ret;
        }
    }

    public struct SoftSpere {
        public Vector3 position; // 位置
        public float radius; // 半径

        public SoftSpere(Vector3 pos, float r) {
            position = pos;
            radius = r;
        }
    }

    public static class SoftMath {

        private static readonly float EPS = 1e-6f;

        // 判断点是否在三角形上（三维坐标）
        public static bool PtInTriangle(ref Vector3 pt, ref Triangle trangle, out float h) {
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

        public static float PtInPlane(ref Vector3 pt, SoftPlane plane) {
            float ret = plane.normal.x * pt.x + plane.normal.y * pt.y + plane.normal.z * pt.z + plane.d;
            return ret;
        }

        public static bool PtInCamera(ref Vector3 pt, SoftCamera camera) {
            SoftPlane[] planes = camera.WorldPlanes;
            if (planes != null && planes.Length >= 6) {
                float ret = PtInPlane(ref pt, planes[SoftCameraPlanes.NearPlane]) * PtInPlane(ref pt, planes[SoftCameraPlanes.FarPlane]);
                if (ret < 0)
                    return false;

                ret = PtInPlane(ref pt, planes[SoftCameraPlanes.LeftPlane]) * PtInPlane(ref pt, planes[SoftCameraPlanes.RightPlane]);
                if (ret < 0)
                    return false;

                ret = PtInPlane(ref pt, planes[SoftCameraPlanes.UpPlane]) * PtInPlane(ref pt, planes[SoftCameraPlanes.DownPlane]);
                if (ret < 0)
                    return false;

                return true;
            }
            return false;
        }

        // 包围球是否在摄影机内
        public static bool BoundSpereInCamera(SoftSpere spere, SoftCamera camera) {
            if (camera == null)
                return false;

            

            return false;
        }
    }
}