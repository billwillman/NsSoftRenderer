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
         internal void GetScreenSpaceTopMiddleBottom(out Vector2 top, out Vector2 middle, out Vector2 bottom) {
            // 此处完全不考虑Z, 因为这里是屏幕空间
            top = p1;
            if (top.y < p2.y || (Mathf.Abs(top.y - p2.y) <= float.Epsilon && p2.x > top.x)) {
                middle = top;
                top = p2;
            } else
                middle = p2;

            if (top.y < p3.y || (Mathf.Abs(top.y - p2.y) <= float.Epsilon && p3.x > top.x)) {
                bottom = top;
                top = p3;
            } else {
                bottom = p3;
            }

            if (middle.y < bottom.y || (Mathf.Abs(middle.y - bottom.y) <= float.Epsilon && bottom.x > middle.x)) {
                Vector2 tmp = middle;
                middle = bottom;
                bottom = tmp;
            }

        }

        internal enum ScreenSpaceTopBottomType
         {
            topBottom = 0,
            top = 1,
            bottom = 2
        };

        // 返回值：0:共两个三角形，分上下。1：只有上三角形。2.只有下三角形
        // topTri和bottomTri， p1.Y >= P2.y>= P3.y 如果其中Y相等，則P1.X>=p2.X>=p3.X
        internal ScreenSpaceTopBottomType GetScreenSpaceTopBottomTriangle(out Triangle2D topTri, out Triangle2D bottomTri) {
            ScreenSpaceTopBottomType ret;

            Vector2 top, middle, bottom;
            GetScreenSpaceTopMiddleBottom(out top, out middle, out bottom);
            if (Mathf.Abs(top.y - middle.y) <= float.Epsilon) {
                // 说明只有下三角形
                ret = ScreenSpaceTopBottomType.bottom;
                topTri = new Triangle2D();
                bottomTri = new Triangle2D();
                bottomTri.p1 = top;
                bottomTri.p2 = middle;
                bottomTri.p3 = bottom;
            } else if (Mathf.Abs(middle.y - bottom.y) <= float.Epsilon) {
                // 只有上三角形
                ret = ScreenSpaceTopBottomType.top;
                bottomTri = new Triangle2D();
                topTri = new Triangle2D();
                topTri.p1 = top;
                topTri.p2 = middle;
                topTri.p3 = bottom;
            } else {
                ret = ScreenSpaceTopBottomType.topBottom;
                // 计算重心坐标，找到P点切割点
                // middle的Y必然大于bottom.Y
                Vector2 AB = middle - top;
                Vector2 AC = bottom - top;
                Vector2 p;
                p.y = middle.y;
                float v = (top.y - p.y) / AC.y;
                p.x = top.x - v * AC.x;

                topTri = new Triangle2D();
                bottomTri = new Triangle2D();
                topTri.p1 = top;
                bottomTri.p3 = bottom;
                if (p.x > middle.x) {
                    topTri.p2 = p;
                    topTri.p3 = middle;

                    bottomTri.p1 = p;
                    bottomTri.p2 = middle;
                } else {
                    topTri.p2 = middle;
                    topTri.p3 = p;

                    bottomTri.p1 = middle;
                    bottomTri.p2 = p;
                }

                

            }

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

        // 填充上三角形
        protected void FillScreenTopTriangle(Triangle2D tri, Color c1, Color c2, Color c3) {

        }
        
        // 填充下三角形
        protected void FillScreenBottomTriangle(Triangle2D tri, Color c1, Color c2, Color c3) {

        }

        // tri已经是屏幕坐标系
        internal void FlipScreenTriangle(SoftCamera camera, TriangleVertex tri, RenderPassMode passMode) {
            // 三角形
            Triangle2D topTri, bottomTri;
            var triType = tri.triangle.GetScreenSpaceTopBottomTriangle(out topTri, out bottomTri);
             switch (triType) {
                case Triangle.ScreenSpaceTopBottomType.top:
                    break;
                case Triangle.ScreenSpaceTopBottomType.bottom:
                    break;
                case Triangle.ScreenSpaceTopBottomType.topBottom:
                    break;
            }
        }
    }
}
