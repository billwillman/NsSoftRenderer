using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

using RenderTargetUseFlags = System.Int32;

namespace NsSoftRenderer {

    public enum RenderTargetClearFlag {
        None = 0,
        Color = 1,
        Depth = 2
    };


    public interface IRenderTargetNotify {
        // IAntiAliasing 抗锯齿接口
        void OnFillColor(ColorBuffer buffer, RectInt fillRect, RectInt clearRect);
    }

    public struct Triangle2D {
        public Vector2 p1, p2, p3;
        public Color cP1, cP2, cP3;

        public void Trans(System.Func<Vector3, Vector3> onTrans) {
            if (onTrans == null)
                return;
            p1 = onTrans(p1);
            p2 = onTrans(p2);
            p3 = onTrans(p3);
        }

        public void Trans(System.Func<Vector3, bool, Vector3> onTrans, bool isUseViewZ) {
            if (onTrans == null)
                return;
            p1 = onTrans(p1, isUseViewZ);
            p2 = onTrans(p2, isUseViewZ);
            p3 = onTrans(p3, isUseViewZ);
        }

        public void MulMatrix(Matrix4x4 mat) {
            p1 = mat.MultiplyPoint(p1);
            p2 = mat.MultiplyPoint(p2);
            p3 = mat.MultiplyPoint(p3);
        }
    }

    public static class Vector3Helper {
        public static string ToString2(this Vector3 v) {
            return Triangle.ToVecStr(v);
        }
    }

    public struct Triangle {
       public Vector3 p1, p2, p3;
       public Triangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public static Vector3 Trans(Vector4 v) {
            Vector3 ret = new Vector3(v.x / v.w, v.y / v.w, v.z / v.w);
            return ret;
        }

        public static Vector4 Trans(Vector3 v) {
            Vector4 ret = new Vector4(v.x, v.y, v.z, 1f);
            return ret;
        }

        public static void CheckPtIntf(ref Vector3 v) {
            return;
            /*
            v.x = float.IsInfinity(v.x) || float.IsNaN(v.x) ? 0 : v.x;
            v.y = float.IsInfinity(v.y) || float.IsNaN(v.y) ? 0 : v.y;
            v.z = float.IsInfinity(v.z) || float.IsNaN(v.z) ? 0 : v.z;
            */
        }

        public void MulMatrix(Matrix4x4 mat) {
            p1 = mat.MultiplyPoint(p1);
            p2 = mat.MultiplyPoint(p2);
            p3 = mat.MultiplyPoint(p3);
            CheckPtIntf(ref p1);
            CheckPtIntf(ref p2);
            CheckPtIntf(ref p3);
        }


        public void Trans(System.Func<Vector3, Vector3> onTrans) {
            if (onTrans == null)
                return;
            p1 = onTrans(p1);
            p2 = onTrans(p2);
            p3 = onTrans(p3);
            CheckPtIntf(ref p1);
            CheckPtIntf(ref p2);
            CheckPtIntf(ref p3);
        }

        public void Trans(System.Func<Vector3, bool, Vector3> onTrans, bool isUseViewZ) {
            if (onTrans == null)
                return;
            p1 = onTrans(p1, isUseViewZ);
            p2 = onTrans(p2, isUseViewZ);
            p3 = onTrans(p3, isUseViewZ);
            CheckPtIntf(ref p1);
            CheckPtIntf(ref p2);
            CheckPtIntf(ref p3);
        }

        public static string ToVecStr(Vector3 v) {

            v.x = float.IsInfinity(v.x) || float.IsNaN(v.x) ? 0 : v.x;
            v.y = float.IsInfinity(v.y) || float.IsNaN(v.y) ? 0 : v.y;
            v.z = float.IsInfinity(v.z) || float.IsNaN(v.z) ? 0 : v.z;

            string ret = string.Format("x: {0} y: {1} z: {2}", v.x, v.y, v.z);
            return ret;
        }

        public override string ToString() {
            string ret = string.Format("p1={0} p2={1} p3={2}", ToVecStr(p1), ToVecStr(p2), ToVecStr(p3));
            return ret;
        }
    }

    // p1, p2, p3必须按照一定顺序，逆时针或者顺时针,坐标系是屏幕坐标系0~width, 0~height，类型：浮点
    public struct TriangleVertex {
        // 顶点位置
        public Triangle triangle;
        // 顶点颜色
        public Color cP1, cP2, cP3;

        public TriangleVertex(Triangle tri, Color p1, Color p2, Color p3) {
            triangle = tri;
            cP1 = p1;
            cP2 = p2;
            cP3 = p3;
        }

        // 获得拆分上下三角形的点
        /*
         *        Top
         *        /|
         *  Middle-- P
         *        \|     
                  Bottom
         *     
         *     
         */
        // top:选Y最大，如果有一样的Y，选X最大。middle：选次之Y最大，如果Y中有一样，则选X大者
        internal void GetScreenSpaceTopMiddleBottom(out Vector3 top, out Vector3 middle, out Vector3 bottom,
                                                    out Color topC, out Color middleC, out Color bottomC) {
            // 此处完全不考虑Z, 因为这里是屏幕空间
            top = triangle.p1 ;
            topC = cP1;
            if (top.y < triangle.p2.y || (Mathf.Abs(top.y - triangle.p2.y) <= float.Epsilon && triangle.p2.x > top.x)) {
                middle = top;
                middleC = topC;
                top = triangle.p2;
                topC = cP2;
            } else {
                middle = triangle.p2;
                middleC = cP2;
            }

            if (top.y < triangle.p3.y || (Mathf.Abs(top.y - triangle.p2.y) <= float.Epsilon && triangle.p3.x > top.x)) {
                bottom = top;
                bottomC = topC;
                top = triangle.p3;
                topC = cP3;
            } else {
                bottom = triangle.p3;
                bottomC = cP3;
            }

            if (middle.y < bottom.y || (Mathf.Abs(middle.y - bottom.y) <= float.Epsilon && bottom.x > middle.x)) {
                Vector3 tmp = middle;
                Color tmpC = middleC;
                middle = bottom;
                middleC = bottomC;
                bottom = tmp;
                bottomC = tmpC;
            }

        }

        internal enum ScreenSpaceTopBottomType {
            topBottom = 0,
            top = 1,
            bottom = 2
        };

        private float GetScreenSpacePointZ(Triangle tri, float screenX, float screenY) {
            Vector3 p = new Vector3(screenX, screenY);
            Vector3 AB = tri.p2 - tri.p1;
            Vector3 AC = tri.p3 - tri.p1;
            Vector3 PA = tri.p1 - p;
            float c = ((PA.y / AB.y) - (PA.x / AB.x)) / ((AC.x/AB.x) - (AC.y/AB.y));
            float b = ((PA.y / AC.y) - (PA.x / AC.x)) / ((AB.x / AC.x) - (AB.y - AC.y));
            float a = 1 - b - c;
            p = tri.p1 * a + tri.p2 * b + tri.p3 * c;
            return p.z;
        }

        // 返回值：0:共两个三角形，分上下。1：只有上三角形。2.只有下三角形
        // topTri和bottomTri， p1.Y >= P2.y>= P3.y 如果其中Y相等，則P1.X>=p2.X>=p3.X
        internal ScreenSpaceTopBottomType GetScreenSpaceTopBottomTriangle(SoftCamera camera, out TriangleVertex topTri, out TriangleVertex bottomTri) {
            ScreenSpaceTopBottomType ret;

            Vector3 top, middle, bottom;
            Color topC, middleC, bottomC;
            GetScreenSpaceTopMiddleBottom(out top, out middle, out bottom, out topC, out middleC, out bottomC);
            if (Mathf.Abs(top.y - middle.y) <= float.Epsilon) {
                // 说明只有下三角形
                ret = ScreenSpaceTopBottomType.bottom;
                topTri = new TriangleVertex();
                bottomTri = new TriangleVertex();
                bottomTri.triangle.p1 = top;
                bottomTri.cP1 = topC;
                bottomTri.triangle.p2 = middle;
                bottomTri.cP2 = middleC;
                bottomTri.triangle.p3 = bottom;
                bottomTri.cP3 = bottomC;
            } else if (Mathf.Abs(middle.y - bottom.y) <= float.Epsilon) {
                // 只有上三角形
                ret = ScreenSpaceTopBottomType.top;
                bottomTri = new TriangleVertex();
                topTri = new TriangleVertex();
                topTri.triangle.p1 = top;
                topTri.cP1 = topC;
                topTri.triangle.p2 = middle;
                topTri.cP2 = middleC;
                topTri.triangle.p3 = bottom;
                topTri.cP3 = bottomC;
            } else {
                ret = ScreenSpaceTopBottomType.topBottom;
                // 计算重心坐标，找到P点切割点
                // middle的Y必然大于bottom.Y
                // 这里是屏幕空间的X,Y
                /*
                Vector3 AB = middle - top;
                Vector3 AC = bottom - top;
                Vector3 p;
                p.y = middle.y;
                float v = (top.y - p.y) / AC.y;
                p.x = top.x - v * AC.x;
                p.z = top.z - v * AC.z;*/
                Vector3 p;
                p.y = middle.y;
                float t;
                p.x = SoftMath.GetScreenSpaceXFromScreenY(top, bottom, p.y, out t);

                topTri = new TriangleVertex();
                bottomTri = new TriangleVertex();
                topTri.triangle.p1 = top;
                topTri.cP1 = topC;
                bottomTri.triangle.p3 = bottom;
                bottomTri.cP3 = bottomC;
                // 下面这个不对，要转到世界坐标系里算重心坐标
                //Color pC = bottomC * v + (1 - v) * topC;
                // 这里转到世界坐标系去算P点的重心坐标，再算出P点颜色插值
                /*
                Vector3 A = camera.ScreenToWorldPoint(top, false);
                Vector3 B = camera.ScreenToWorldPoint(middle, false);
                Vector3 C = camera.ScreenToWorldPoint(bottom, false);
                Vector3 PP = camera.ScreenToWorldPoint(p, false);
                float a, b, c;
                SoftMath.GetBarycentricCoordinate(A, B, C, PP, out a, out b, out c);
                Color pC = topC * a + middleC * b + bottomC * c;*/
                //----------------------------------

                /*
                * 在摄影机VIEW空间 1/Zp = t * 1/Za + (1-t) * 1/Zc => 透视校正插值（一定要在摄影机ViewSpace[局部坐标系里]）
                */
                p.z = SoftMath.GetPerspectZFromLerp(top, bottom, t);
                // 颜色UV的插值方式: Pcolor * 1/Zp = Acolor * t * 1/Za + (1 - t) * /Zc * Ccolor
                Color pC = (topC * t * 1f / top.z + (1f - t) * bottomC / bottom.z) * p.z;

                if (p.x > middle.x) {
                    topTri.triangle.p2 = p;
                    topTri.cP2 = pC;
                    topTri.triangle.p3 = middle;
                    topTri.cP3 = middleC;

                    bottomTri.triangle.p1 = p;
                    bottomTri.cP1 = pC;
                    bottomTri.triangle.p2 = middle;
                    bottomTri.cP2 = middleC;
                } else {
                    topTri.triangle.p2 = middle;
                    topTri.cP2 = middleC;
                    topTri.triangle.p3 = p;
                    topTri.cP3 = pC;

                    bottomTri.triangle.p1 = middle;
                    bottomTri.cP1 = middleC;
                    bottomTri.triangle.p2 = p;
                    bottomTri.cP2 = pC;
                }



            }

            return ret;
        }
    }

    // 填充策略(包围盒扫描策略或三角形扫描线策略)
    internal interface IRenderTargetFillProxy {
        // 颜色填充三角形
        void FillTriangleColor(TriangleVertex triangleVertexs, ColorBuffer colorBuffer);
    }

    public class RenderTarget: DisposeObject {
        private ColorBuffer m_FrontColorBuffer = null;
        private Depth32Buffer m_FrontDepthBuffer = null;
        private RenderTargetUseFlags m_ClearFlags = 0;
        private Color m_CleanColor = Color.black;
        // 脏矩形
        private RectInt m_ClearColorDirtRect = new RectInt(0, 0, 0, 0);
        private RectInt m_ColorDirthRect = new RectInt(0, 0, 0, 0);
        private RectInt m_DepthDirthRect = new RectInt(0, 0, 0, 0);
        private bool m_IsCleanedColor = true;
        private bool m_IsCleanedDepth = true;
        private bool m_IsAllCleanedColor = false;
        private bool m_IsAllCleanedDepth = false;

        // 无限远定义
        private static readonly int _cFarFarZ = -9999999;
        private static readonly RectInt _cZeroRect = new RectInt(0, 0, 0, 0);

        public RenderTarget(int deviceWidth, int deviceHeight) {
            m_FrontColorBuffer = new ColorBuffer(deviceWidth, deviceHeight);
            m_FrontDepthBuffer = new Depth32Buffer(deviceWidth, deviceHeight);
            unchecked {
                m_ClearFlags = (RenderTargetUseFlags)0xFFFFFFFF;
            }
        }

        // ZTest检测，暂时占位，后面完善
        public bool ZTest(ZTestOp op, float z) {
            return true;
        }

        private bool InitClearAllColor() {
            if (m_FrontColorBuffer != null && (!m_IsAllCleanedColor)) {
                m_IsAllCleanedColor = true;
                for (int r = 0; r < m_FrontColorBuffer.Height; ++r) {
                    for (int c = 0; c < m_FrontColorBuffer.Width; ++c) {
                        m_FrontColorBuffer.SetItem(c, r, m_CleanColor);
                    }
                }
                m_IsCleanedColor = true;
                return true;
            }
            return false;
        }

        private bool InitClearAllDepth() {
            if (m_FrontDepthBuffer != null && (!m_IsAllCleanedDepth)) {
                m_IsAllCleanedDepth = true;
                for (int r = 0; r < m_FrontDepthBuffer.Height; ++r) {
                    for (int c = 0; c < m_FrontDepthBuffer.Width; ++c) {
                        m_FrontDepthBuffer.SetItem(c, r, _cFarFarZ);
                    }
                }
                m_IsCleanedDepth = true;
                return true;
            }
            return false;
        }

        private void Clear() {
            if (m_FrontColorBuffer != null && (!m_IsCleanedColor) && (RenderTarget.IncludeUseFlag(m_ClearFlags, RenderTargetClearFlag.Color))) {
                m_IsCleanedColor = true;
                if (m_ColorDirthRect.width > 0 && m_ColorDirthRect.height > 0) {
                    for (int r = m_ColorDirthRect.yMin; r < m_ColorDirthRect.yMax; ++r) {
                        for (int c = m_ColorDirthRect.xMin; c < m_ColorDirthRect.xMax; ++c) {
                            m_FrontColorBuffer.SetItem(c, r, m_CleanColor);
                        }
                    }

                    m_ClearColorDirtRect = m_ColorDirthRect;

                    m_ColorDirthRect.x = 0;
                    m_ColorDirthRect.y = 0;
                    m_ColorDirthRect.width = 0;
                    m_ColorDirthRect.height = 0;
                }
            }

            if (m_FrontDepthBuffer != null && (!m_IsCleanedDepth) && (RenderTarget.IncludeUseFlag(m_ClearFlags, RenderTargetClearFlag.Depth))) {
                m_IsCleanedDepth = true;
                if (m_DepthDirthRect.width > 0 && m_DepthDirthRect.height > 0) {
                    for (int r = m_DepthDirthRect.yMin; r < m_DepthDirthRect.yMax; ++r) {
                        for (int c = m_DepthDirthRect.xMin; c < m_DepthDirthRect.xMax; ++c) {
                            m_FrontDepthBuffer.SetItem(c, r, _cFarFarZ);
                        }
                    }
                }
            }
        }

        // 填充到屏幕
        public void FlipToNotify(IRenderTargetNotify notify) {
            if (notify != null) {
                if (m_IsFillAllColor) {
                    RectInt fillRect = new RectInt(0, 0, this.Width, this.Height);
                    notify.OnFillColor(m_FrontColorBuffer, fillRect, _cZeroRect);
                } else {
                    if (m_ColorDirthRect.width > 0 && m_ColorDirthRect.height > 0) {
                        notify.OnFillColor(m_FrontColorBuffer, m_ColorDirthRect, m_ClearColorDirtRect);
                    }
                }
            }
        }

        private bool m_IsFillAllColor = false;
        public void Prepare() {
            m_IsFillAllColor = InitClearAllColor();
            InitClearAllDepth();
            Clear();
        }

        public int Width {
            get {
                if (m_FrontColorBuffer != null)
                    return m_FrontColorBuffer.Width;
                return 0;
            }
        }

        public int Height {
            get {
                if (m_FrontColorBuffer != null)
                    return m_FrontColorBuffer.Height;
                return 0;
            }
        }

        public Color CleanColor {
            get {
                return m_CleanColor;
            }

            set {
                m_CleanColor = value;
                m_IsAllCleanedColor = false;
            }
        }

        // 清理参数
        public RenderTargetUseFlags ClearFlags {
            get {
                return m_ClearFlags;
            }
            set {
                m_ClearFlags = value;
            }
        }

        /// <summary>
        /// 裁剪像素
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>是否触发剪裁</returns>
        protected bool ClipPixel(int x, int y) {
            if (m_FrontColorBuffer != null) {
                if (x < 0 || y < 0 || (x >= m_FrontColorBuffer.Width) || (y >= m_FrontColorBuffer.Height))
                    return true;
                return false;
            }
            return true;
        }

        public static RenderTargetUseFlags CombineUseFlag(RenderTargetUseFlags old, RenderTargetClearFlag flag) {
            RenderTargetUseFlags ret = old | (1 << ((int)flag - 1));
            return ret;
        }


        public static bool IncludeUseFlag(RenderTargetUseFlags flags, RenderTargetClearFlag flag) {
            bool ret = (flags & (1 << ((int)flag - 1))) != 0;
            return ret;
        }

        private void InitColorDirty(int x, int y) {
            if (m_IsCleanedColor) {
                m_ColorDirthRect.xMin = x;
                m_ColorDirthRect.xMax = x;
                m_ColorDirthRect.yMin = y;
                m_ColorDirthRect.yMax = y;

                m_IsCleanedColor = false;
            } else {
                if (x < m_ColorDirthRect.xMin)
                    m_ColorDirthRect.xMin = x;
                else if (x > m_ColorDirthRect.xMax)
                    m_ColorDirthRect.xMax = x;
                if (y < m_ColorDirthRect.yMin)
                    m_ColorDirthRect.yMin = y;
                else if (y > m_ColorDirthRect.yMax)
                    m_ColorDirthRect.yMax = y;
            }
        }

        private void InitDepthDirty(int x, int y) {
            if (m_IsCleanedDepth) {
                m_DepthDirthRect.xMin = x;
                m_DepthDirthRect.xMax = x;
                m_DepthDirthRect.yMin = y;
                m_DepthDirthRect.yMax = y;

                m_IsCleanedDepth = false;
            } else {
                if (x < m_DepthDirthRect.xMin)
                    m_DepthDirthRect.xMin = x;
                else if (x > m_DepthDirthRect.xMax)
                    m_DepthDirthRect.xMax = x;
                if (y < m_DepthDirthRect.yMin)
                    m_DepthDirthRect.yMin = y;
                else if (y > m_DepthDirthRect.yMax)
                    m_DepthDirthRect.yMax = y;
            }
        }

        private void CheckClipPt(Vector2 pt) {
        }

        // 画2D线
        public bool Draw2DLine(Vector2 start, Vector2 end, RenderTargetUseFlags flags, Color color, int depth = 0) {

           if ((end - start).sqrMagnitude <= (float.Epsilon * float.Epsilon)) {
                // 画点
                return DrawPixel((int)start.x, (int)start.y, flags, color, depth);
            }

            return true;
        }

        public bool DrawPixel(int x, int y, RenderTargetUseFlags flags, Color color, int depth = 0) {
            if (flags == 0 || ClipPixel(x, y))
                return false;

            bool isUseColor = IncludeUseFlag(flags, RenderTargetClearFlag.Color);
            bool isUseDepth = IncludeUseFlag(flags, RenderTargetClearFlag.Depth);

            if (isUseColor && m_FrontColorBuffer != null) {
                m_FrontColorBuffer.SetItem(x, y, color);
                // 设置脏矩形
                InitColorDirty(x, y);
            }
            if (isUseDepth && m_FrontDepthBuffer != null) {
                m_FrontDepthBuffer.SetItem(x, y, depth);
                // 设置脏矩形
                InitDepthDirty(x, y);
            }

            return true;
        }

        public ColorBuffer FrontColorBuffer {
            get {
                return m_FrontColorBuffer;
            }
        }

        public Depth32Buffer FrontDepthBuffer {
            get {

                return m_FrontDepthBuffer;
            }
        }

        protected override void OnFree(bool isManual) {
            if (m_FrontColorBuffer != null) {
                m_FrontColorBuffer.Dispose();
                m_FrontColorBuffer = null;
            }

            if (m_FrontDepthBuffer != null) {
                m_FrontDepthBuffer.Dispose();
                m_FrontDepthBuffer = null;
            }
        }

        // 行填充
        private void ScanLine(Vector3 screenStart, Vector3 screenEnd) {

        }

        // 填充上三角形
        protected void FillScreenTopTriangle(SoftCamera camera, RenderPassMode passMode, TriangleVertex tri) {

        }
        
        // 填充下三角形
        protected void FillScreenBottomTriangle(SoftCamera camera, RenderPassMode passMode, TriangleVertex tri) {
            // middle(p2)----top(p1)
            //  \       /
            //    bottom(p3)
            int yStart = Mathf.Max(Mathf.FloorToInt(tri.triangle.p2.y), 0);
            int yEnd =  Mathf.Min(Mathf.CeilToInt(tri.triangle.p3.y), m_FrontColorBuffer.Height - 1);


            for (int row = yStart; row <= yEnd; ++row) {

            }

            // 更新包围盒
        }

        // tri已经是屏幕坐标系
        internal void FlipScreenTriangle(SoftCamera camera, TriangleVertex tri, RenderPassMode passMode) {
            // 三角形
            TriangleVertex topTri, bottomTri;
            var triType = tri.GetScreenSpaceTopBottomTriangle(camera, out topTri, out bottomTri);
             switch (triType) {
                case TriangleVertex.ScreenSpaceTopBottomType.top:
                    FillScreenTopTriangle(camera, passMode, topTri);
                    break;
                case TriangleVertex.ScreenSpaceTopBottomType.bottom:
                    FillScreenBottomTriangle(camera, passMode, bottomTri);
                    break;
                case TriangleVertex.ScreenSpaceTopBottomType.topBottom:
                    FillScreenTopTriangle(camera, passMode, topTri);
                    FillScreenBottomTriangle(camera, passMode, bottomTri);
                    break;
            }
        }
    }
}
