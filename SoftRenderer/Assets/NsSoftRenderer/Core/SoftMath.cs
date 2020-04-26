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

        // 點到平面的距離
        public static float PtToPlaneDistance(ref Vector3 pt, SoftPlane panel) {
            float d = panel.normal.magnitude;
            float a = Mathf.Abs(pt.x * panel.normal.x + pt.y * panel.normal.y + pt.z * panel.normal.z + panel.d);
            float ret = a / d;
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

            float r = spere.radius;
            SoftPlane[] panels = camera.WorldPlanes;
            for (int i = 0; i < panels.Length; ++i) {
                var panel = panels[i];
                float distance = PtToPlaneDistance(ref spere.position, panel);
                if (distance < r)
                    return true;
            }

            return false;
        }

        /* 根据三角形三个点获得三角形中一个点的插值
         * 在重心坐标系中，三角形三点为A, B, C。有一个点P在三角形内，则P = a * A + b * B * c * C
         * 系数a, b, c就是P点的重心坐标。
         * 对应的特性：
         * 1.A点对面的三角形面积 Sa就是BPC的三角形面积，同理Sb,Sc。有如下性质
         *   a = Sa/(Sa + Sb + Sc) 同理，b = Sb/(Sa + Sb + Sc) c = Sc/(Sa + Sb + Sc)
         * 2. a + b + c = 1
         * 3. a >= 0, b >= 0, c >=0。如果有一个不满足，虽然满足2条件，说明P点在属于三角形的平面的三角形外面，而非
         *   在三角形里面。
         *   
         * 【推到如何求出 a, b, c】   
         *      A
         *   B  P  C
         *   可知：
         *   AP = u * AB + v * AC
         *   => P - A = u (B - A) + v * (C - A)
         *   => P = u * B - u * A + v * C - v * A + A
         *   => P = (1 - u - v) * A + u * B  + v * C
         *   令：r = 1 - u - v, 則 P = r * A + u * B  + v * C
         *   【结论】说明存在一个 P = r * A + u * B  + v * C 即重心坐标系
         *
         *   因为，AP = u * AB + v * AC
         *   =》u * AB + v * AC - AP = 0
         *   更改为向量乘法为：
         *   [u, v, 1] * [AB, AC, -AP]
         *   => [u, v, 1] * [AB, AC, PA]
         *   拆分得到：
         *   => [u, v, 1] * [ABx, ACx, PAx] = 0
         *      [u, v, 1] * [ABy, ACy, PAy] = 0
         *   【结论】也就是求同时垂直 [ABx, ACx, PAx]和[ABy, ACy, PAy]的向量，也就是这两个向量的叉乘。
        */
        public static void GetBarycentricCoordinate(ref Vector3 A, ref Vector3 B, ref Vector3 C, ref Vector3 P, out float u, out float v, out float r) {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 PA = A - P;

            Vector3 v1 = new Vector3(AB.x, AC.x, PA.x);
            Vector3 v2 = new Vector3(AB.y, AC.y, PA.y);
            Vector3 vv = Vector3.Cross(v1, v2);
            if (vv.x < 0)
                vv = -vv;
            u = vv.x;
            v = vv.y;
            r = 1 - u - v;
        }
    }
}