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

        /*
         * 判断一个点是否在三角形内，有两种常用方法：
         *    1. 一种是P点1和三角形任意一点的向量，和三角形三个边向量都在同一侧，同时左侧还是有右侧（即采用向量叉乘，三条边的点乘符号+或者-都是一致的 ）
         *       1）点乘和叉乘的位置意义是，点乘是表示前后关系。
         *       2）叉乘是表示左右侧关系。
         *    2. 另一种是采用重心坐标系
         */

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

        public static float PtInPlane(Vector3 pt, SoftPlane plane) {
            float ret = plane.normal.x * pt.x + plane.normal.y * pt.y + plane.normal.z * pt.z + plane.d;
            return ret;
        }

        public static bool FloatEqual(float f1, float f2) {
            bool ret = Mathf.Abs(f1 - f2) <= float.Epsilon;
            return ret;
        }

        // 这里是在MVP坐标系空间做
        public static bool Is_MVP_Culled(CullMode mode, Triangle tri) {
            if (mode == CullMode.none)
                return false;

            Vector3 v1 = tri.p3 - tri.p2;
            Vector3 v2 = tri.p2 - tri.p1;
            Vector3 n = Vector3.Cross(v2, v1);
            Vector3 lookAt = new Vector3(0, 0, 1f);

            bool isFront = Vector3.Dot(lookAt, n) < 0;

            switch (mode) {
                case CullMode.front: {
                        return isFront;
                    }
                case CullMode.back: {
                        return !isFront;
                    }
            }
            return false;
        }

        // 判断是否需要Cull(这里是世界坐标剔除)
        public static bool IsCulled(SoftCamera camera, CullMode mode, Triangle tri) {
            if (mode == CullMode.none)
                return false;
            if (camera == null)
                return true;

            Vector3 v1 = tri.p3 - tri.p2;
            Vector3 v2 = tri.p2 - tri.p1;
            Vector3 n = Vector3.Cross(v1, v2);
            Vector3 lookAt = camera.LookAt;
            bool isFront = Vector3.Dot(lookAt, n) < 0;

            switch (mode) {
                case CullMode.front: {          
                        return isFront;
                    }
                case CullMode.back: {              
                        return !isFront;
                    }
            }
            return false;
        }

        // 點到平面的距離 collisionPt=>交点
        public static float PtToPlaneDistance(Vector3 pt, SoftPlane panel, out Vector3 collisionPt) {
            float d = panel.normal.magnitude;
            float aa = pt.x * panel.normal.x + pt.y * panel.normal.y + pt.z * panel.normal.z + panel.d;
            float a = Mathf.Abs(aa);
            float ret = a / d;

            // 获得交点
            Vector3 dir = panel.normal;
            if (aa < 0) {
                dir = -dir;
            }
            collisionPt = pt + dir * ret;

            return ret;
        }

        /*
         * 原理，点在平面上 AX + BY + CZ + D > 0，平面下 < 0。只要被包裹着，就是在视锥体里。
         */
        public static bool PtInCamera(Vector3 pt, SoftCamera camera) {

            SoftPlane[] planes = camera.WorldPlanes;
            if (planes != null && planes.Length >= 6) {
                float ret = PtInPlane(pt, planes[SoftCameraPlanes.NearPlane]) * PtInPlane(pt, planes[SoftCameraPlanes.FarPlane]);
                if (ret < 0)
                    return false;

                ret = PtInPlane(pt, planes[SoftCameraPlanes.LeftPlane]) * PtInPlane(pt, planes[SoftCameraPlanes.RightPlane]);
                if (ret < 0)
                    return false;

                ret = PtInPlane(pt, planes[SoftCameraPlanes.UpPlane]) * PtInPlane(pt, planes[SoftCameraPlanes.DownPlane]);
                if (ret < 0)
                    return false;

                return true;
            }
            return false;
        }

        public static bool BoundSpereInCamera_UseMVP(SoftSpere spere, SoftCamera camera) {
            Vector3 localPt = camera.WorldToViewportPoint(spere.position, false);

            float left = localPt.x - spere.radius;
            float right = localPt.x + spere.radius;
            if (right <= -1 || left >= 1)
                return false;

            float top = localPt.y + spere.radius;
            float bottom = localPt.y - spere.radius;
            if (bottom >= 1 || top <= -1)
                return false;

            float front = localPt.z + spere.radius;
            float back = localPt.z - spere.radius;
            if (back >= 1 || front <= -1)
                return false;

            return true;
        }

        /*
        // 包围球是否在摄影机内
        // 这里用世界坐标系，还可以放到投影坐标系里，X:-1~1, Y:-1~1, Z:-1~1
        public static bool BoundSpereInCamera(SoftSpere spere, SoftCamera camera) {
            if (camera == null)
                return false;

            float r = spere.radius;
            SoftPlane[] panels = camera.WorldPlanes;

            // 判断包围球中心是否在摄影机范围内，如果在，则直接返回TRUE
            if (PtInCamera(spere.position, camera))
                return true;

            Vector3 collisionPt;
            for (int i = 0; i < panels.Length; ++i) {
                var panel = panels[i];
                float distance = PtToPlaneDistance(spere.position, panel, out collisionPt);
                if ((distance < r) && PtInCamera(collisionPt, camera)) {
                    return true;
                }
            }

            return false;
        }*/

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
         *      [u, v, 1] * [ABz, ACz, PAz] = 0
         *    两个式子足够了，所以只取前两个用于计算。
         *   【结论】也就是求同时垂直 [ABx, ACx, PAx]和[ABy, ACy, PAy][ABz, ACz, PAz]的向量，也就是这两个向量的叉乘。
         *   【注意】矩阵变换后的三角形的重心和原来三角形的重心可能会不一致。。。
        */
        public static void GetBarycentricCoordinate(Vector3 A, Vector3 B,Vector3 C, Vector3 P, 
            out float a, out float b, out float c/*, bool isUseNormal = true*/) {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 PA = A - P;

            Vector3 v1 = new Vector3(AB.x, AC.x, PA.x);
            Vector3 v2 = new Vector3(AB.y, AC.y, PA.y);
            Vector3 vv = Vector3.Cross(v1, v2);
         //   if (isUseNormal)
         //       vv = vv.normalized;

            // 这里要除以Z，因为推到公式里是 u, v, 1
            if (Mathf.Abs(vv.z) > EPS)
                vv /= vv.z;

            vv.x = Mathf.Abs(vv.x) < EPS ? 0f : vv.x;
            vv.y = Mathf.Abs(vv.y) < EPS ? 0f : vv.y;

            if (vv.x < 0)
                vv = -vv;
            b = vv.x; //-->> b即是v
            c = vv.y; //-->> c即是u
            a = 1f - b - c; //-->>a即是 1- u - v = r
        }

        public static void GetBarycentricCoordinate(Triangle tri, Vector3 P, out float a, out float b, out float c/*, bool isUseNormal = true*/) {
            GetBarycentricCoordinate(tri.p1, tri.p2, tri.p3, P, out a, out b, out c/*, isUseNormal*/);
        }

        public static Color GetColorFromBarycentricCoordinate(TriangleVertex tri, Vector3 P) {
            float a, b, c;
            tri.triangle.InvZ();
            GetBarycentricCoordinate(tri.triangle, P, out a, out b, out c);
            Color ret = a * tri.cP1 + b * tri.cP2 + c * tri.cP3;
            return ret;
        }

        public static void GetScreenSpaceBarycentricCoordinate(Vector2 A, Vector2 B, Vector3 C, Vector2 P,
            out float a, out float b, out float c/*, bool isUseNormal = true*/) {
            Vector3 AA = new Vector3(A.x, A.y, 0f);
            Vector3 BB = new Vector3(B.x, B.y, 0f);
            Vector3 CC = new Vector3(C.x, C.y, 0f);
            Vector3 PP = new Vector3(P.x, P.y, 0f);
            GetBarycentricCoordinate(AA, BB, CC, PP, out a, out b, out c/*, isUseNormal*/);
        }

        public static float GetScreenSpaceBarycentricCoordinateZ(Vector3 A, Vector3 B, Vector3 C, Vector2 P) {
            float a, b, c;
            Vector3 AA = new Vector3(A.x, A.y, 0f);
            Vector3 BB = new Vector3(B.x, B.y, 0f);
            Vector3 CC = new Vector3(C.x, C.y, 0f);
            Vector3 PP = new Vector3(P.x, P.y, 0f);
            GetBarycentricCoordinate(AA, BB, CC, PP, out a, out b, out c);
            float invZ = a * 1f / A.z + b * 1f / B.z + c * 1f / C.z;
            float ret = 1f / invZ;
            return ret;
        }

        public static float GetScreenSpaceBarycentricCoordinateZ(Triangle tri, Vector2 P) {
            float ret = GetScreenSpaceBarycentricCoordinateZ(tri.p1, tri.p2, tri.p3, P);
            return ret;
        }

        public static float GetScreenSpaceBarycentricCoordinateZ(TriangleVertex tri, Vector2 P) {
            float ret = GetScreenSpaceBarycentricCoordinateZ(tri.triangle.p1, tri.triangle.p2, tri.triangle.p3, P);
            return ret;
        }

        /*
        public static float GetScreenSpacePointZ(Triangle tri, float screenX, float screenY) {
            Vector3 p = new Vector3(screenX, screenY);
            Vector3 AB = tri.p2 - tri.p1;
            Vector3 AC = tri.p3 - tri.p1;
            Vector3 PA = tri.p1 - p;
            float c = ((PA.y / AB.y) - (PA.x / AB.x)) / ((AC.x / AB.x) - (AC.y / AB.y));
            float b = ((PA.y / AC.y) - (PA.x / AC.x)) / ((AB.x / AC.x) - (AB.y - AC.y));
            float a = 1 - b - c;
            p = tri.p1 * a + tri.p2 * b + tri.p3 * c;
            return p.z;
        }*/

        public static float GetDeltaT(float a, float b, float p) {
            if (Mathf.Abs(a - b) <= float.Epsilon)
                return 1f;
            float ret = (p - b) / (a - b);
            return ret;
        }

        // 在屏幕坐标系里使用线段上的一个Y坐标算出插值T并算出X坐标
        public static float GetScreenSpaceXFromScreenY(Vector3 screenA, Vector3 screenB, float pY, out float t) {
            t = GetDeltaT(screenA.y, screenB.y, pY);
          //  t = (pY - screenB.y) / (screenA.y - screenB.y);
            float ret = t * screenA.x + (1f - t) * screenB.x;
            return ret;
        }

        public static float GetZFromVectorsX(Vector3 p1, Vector3 p2, Vector2 p)
        {
           // if (Mathf.Abs(p1.z) > float.Epsilon)
         //       p1.z = 1f / p1.z;
         //   if (Mathf.Abs(p2.z) > float.Epsilon)
         //       p2.z = 1f / p2.z;
            Vector3 l = p2 - p1;
            float dZ = Mathf.Abs(l.z);
            if (dZ <= float.Epsilon)
                return p1.z;
            float dA = Mathf.Abs(l.x);
            bool isDA = dA <= float.Epsilon;
            float dB = Mathf.Abs(l.y);
            bool isDB = dB <= float.Epsilon;
            if (isDA && isDB)
            {
                return Mathf.Max(p1.z, p2.z);
            }

            float ret;
            if (isDA)
            {
                ret = (l.z / l.y * (p.y - p1.y) + p1.z);
            }

         //   if (isDB)
          //  {
                ret = (l.z/l.x * (p.x - p1.x) + p1.z);
            //  } 
         //   if (Mathf.Abs(ret) > float.Epsilon)
        //        ret = 1f / ret;
            return ret;
        }

        public static Color GetColorLerpFromScreenY(Vector3 A, Vector3 B, Vector3 P, Color aColor, Color bColor) {
            float t = GetDeltaT(A.y, B.y, P.y);
            Color ret = (aColor * t * 1f / A.z + bColor * (1f - t) * 1f/B.z) * P.z;
            return ret;
        }

        public static Color GetColorLerpFromScreenX(Vector3 A, Vector3 B, Vector3 P, Color aColor, Color bColor) {
            float t = GetDeltaT(A.x, B.x, P.x);
            Color ret = (aColor * t * 1f / A.z + bColor * (1f - t) * 1f / B.z) * P.z;
            return ret;
        }

        public static Color GetColorFromProjZ(Vector3 A, Vector3 B, Vector3 P, Color c1, Color c2) {
            return GetColorFromProjZ(A.z, B.z, P.z, c1, c2);
        }

        public static Color GetColorFromProjZ(float z1, float z2, float pz, Color c1, Color c2)
        {
            float invZ1 = 0;
            if (Mathf.Abs(z1) > float.Epsilon)
                invZ1 = 1f / z1;
            float invZ2 = 0;
            if (Mathf.Abs(z2) > float.Epsilon)
                invZ2 = 1f / z2;
            float invPZ = 0;
            if (Mathf.Abs(pz) > float.Epsilon)
                invPZ = 1f / pz;
            float t = GetDeltaT(invZ1, invZ2, invPZ);
            Color ret = c1 * t + (1f - t) * c2;
            return ret;
        }

        // 透视校正插值获得Z
        // 在屏幕坐标系三点其中每个点的Z都是在ViewSpace里，可根据屏幕坐标系的X,Y的插值T算出在ViewSpace中的另外一个点的Z
        // 1/Zp = t * 1/Za + (1 - t) * 1/Zb 深度的倒数是线性的特性。
        public static float GetPerspectZFromLerp(Vector3 screenA, Vector3 screenB, float t) {
            float invZ = t * 1f / screenA.z + (1 - t) * 1f / screenB.z;
            float ret = 1f / invZ;
            return ret;
        }

        // 計算Diffuse 点光源.
        // diffuse从任何角度观察，模型上的光照效果是不变得，所以跟viewDir无关
        public static Color PointLight_DiffuseColor(Vector3 lightPos, Vector3 pointPos, Color lightColor, float lightI, Vector3 pointNormal) {
            Vector3 L = lightPos - pointPos;
            float r = L.magnitude;
            // lightI / (r * r) 模拟衰减
            Color ret = lightColor * lightI / (r * r) * Mathf.Max(0, Vector3.Dot(L, pointNormal));
            return ret;
        }
    }
}