#define _USE_NEW_LERP_Z

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

        public void InvZ() {
            if (Mathf.Abs(p1.z) > float.Epsilon)
                p1.z = 1f / p1.z;
            if (Mathf.Abs(p2.z) > float.Epsilon)
                p2.z = 1f / p2.z;
            if (Mathf.Abs(p3.z) > float.Epsilon)
                p3.z = 1f / p3.z;
        }

        public Rect GetRect(out float minZ, out float maxZ) {
            Rect ret = new Rect();
            ret.xMin = Mathf.Min(Mathf.Min(p1.x, p2.x), p3.x);
            ret.xMax = Mathf.Max(Mathf.Max(p1.x, p2.x), p3.x);
            ret.yMax = Mathf.Max(Mathf.Max(p1.y, p2.y), p3.y);
            ret.yMin = Mathf.Min(Mathf.Min(p1.y, p2.y), p3.y);

            minZ = Mathf.Min(Mathf.Min(p1.z, p2.z), p3.z);
            maxZ = Mathf.Max(Mathf.Max(p1.z, p2.z), p3.z);

            return ret;
        }

        

        public bool IsAllZGreateOne {
            get {
                return (p1.z > 1f) && (p2.z > 1f) && (p3.z > 1f);
            }
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
        // 暂时这样
        public int mainTex;
        public Vector4 uv1_1, uv1_2, uv1_3;

        // 主纹理
        public SoftTexture2D MainTexture {
            get {
                if (mainTex == 0)
                    return null;
                var device = SoftDevice.StaticDevice;
                if (device == null)
                    return null;
                return device.ResMgr.GetSoftRes<SoftTexture2D>(mainTex);
            }
        }

        public TriangleVertex(Triangle tri, Color p1, Color p2, Color p3, int mainTex = 0) {
            triangle = tri;
            cP1 = p1;
            cP2 = p2;
            cP3 = p3;
            this.mainTex = mainTex;
            this.uv1_1 = Vector4.zero;
            this.uv1_2 = Vector4.zero;
            this.uv1_3 = Vector4.zero;
        }

        public bool IsAllZGreateOne {
            get {
                return triangle.IsAllZGreateOne;
            }
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

            if (top.y < triangle.p3.y || (Mathf.Abs(top.y - triangle.p3.y) <= float.Epsilon && triangle.p3.x > top.x)) {
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

        /*
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
        }*/

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
                float a, b, c;
                Vector2 PP = new Vector3(p.x, p.y, 0f);
                p.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(this, PP, out a, out b, out c);
                // 颜色UV的插值方式: Pcolor * 1/Zp = Acolor * t * 1/Za + (1 - t) * /Zc * Ccolor
                Color pC = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(this, p.z, a, b, c);

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
        private PixelBuffer m_FrontColorBuffer = null;
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
            m_FrontColorBuffer = new PixelBuffer(deviceWidth, deviceHeight);
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
                        m_FrontColorBuffer.SetPixel(c, r, m_CleanColor, Vector4.zero, 0);
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

        private bool m_IsCleanAllColorBuff = false;
        public bool IsCleanAllColor {
            get {
                return m_IsCleanAllColorBuff;
            }
            set {
                m_IsCleanAllColorBuff = value;
            }
        }

        private void Clear() {
            if (m_FrontColorBuffer != null && ((!m_IsCleanedColor) || (RenderTarget.IncludeUseFlag(m_ClearFlags, RenderTargetClearFlag.Color)))) {
                m_IsCleanedColor = true;
                if (m_ColorDirthRect.width > 0 && m_ColorDirthRect.height > 0) {
                    int yMin = Mathf.Max(m_ColorDirthRect.yMin, 0);
                    int yMax = Mathf.Min(m_ColorDirthRect.yMax, m_FrontColorBuffer.Height - 1);
                    int xMin = Mathf.Max(m_ColorDirthRect.xMin, 0);
                    int xMax = Mathf.Min(m_ColorDirthRect.xMax, m_FrontColorBuffer.Width - 1);
                    if (m_IsCleanAllColorBuff) {
                        yMin = 0;
                        xMin = 0;
                        yMax = m_FrontColorBuffer.Height - 1;
                        xMax = m_FrontColorBuffer.Width - 1;
                    }

                  //  Debug.LogErrorFormat("yMin: {0} yMax: {1} xMin: {2} xMax: {3}", yMin, yMax, xMin, xMax);
                    for (int r = yMin; r <= yMax; ++r) {
                        for (int c = xMin; c <= xMax; ++c) {
                            m_FrontColorBuffer.SetPixel(c, r, m_CleanColor, Vector4.zero);
                        }
                    }

                    m_ClearColorDirtRect = m_ColorDirthRect;

                    m_ColorDirthRect.x = 0;
                    m_ColorDirthRect.y = 0;
                    m_ColorDirthRect.width = 0;
                    m_ColorDirthRect.height = 0;
                }
            }

            if (m_FrontDepthBuffer != null && ((!m_IsCleanedDepth) || (RenderTarget.IncludeUseFlag(m_ClearFlags, RenderTargetClearFlag.Depth)))) {
                m_IsCleanedDepth = true;

                if (m_IsCleanAllColorBuff) {
                    m_DepthDirthRect.xMin = 0;
                    m_DepthDirthRect.yMin = 0;
                    m_DepthDirthRect.xMax = m_FrontDepthBuffer.Width - 1;
                    m_DepthDirthRect.yMax = m_FrontDepthBuffer.Height - 1;
                }

                if (m_DepthDirthRect.width > 0 && m_DepthDirthRect.height > 0) {
                    for (int r = m_DepthDirthRect.yMin; r <= m_DepthDirthRect.yMax; ++r) {
                        for (int c = m_DepthDirthRect.xMin; c <= m_DepthDirthRect.xMax; ++c) {
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
                    notify.OnFillColor(m_FrontColorBuffer.colorBuffer, fillRect, _cZeroRect);
                } else {
                    if ((m_ColorDirthRect.width > 0 && m_ColorDirthRect.height > 0) || (m_ClearColorDirtRect.width > 0 && m_ClearColorDirtRect.height > 0)) {
                        notify.OnFillColor(m_FrontColorBuffer.colorBuffer, m_ColorDirthRect, m_ClearColorDirtRect);
                    }
                }

                // 清理
              //  Clear();
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
                m_FrontColorBuffer.SetPixel(x, y, color);
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

        public PixelBuffer FrontColorBuffer {
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
            base.OnFree(isManual);

            if (m_FrontColorBuffer != null) {
                m_FrontColorBuffer.Dispose();
                m_FrontColorBuffer = null;
            }

            if (m_FrontDepthBuffer != null) {
                m_FrontDepthBuffer.Dispose();
                m_FrontDepthBuffer = null;
            }
        }

        private float TransZBuffer(float orgZ) {
            if (Mathf.Abs(orgZ) > float.Epsilon)
               // return orgZ * 1000f;
                return 1f/orgZ;
            return orgZ;
        }

        private int CompareZBuffer(float oldZ, float newZ) {
            if (Mathf.Abs(oldZ - newZ) <= float.Epsilon)
                return 0;
            if (oldZ < newZ)
                return -1;
            else
                return 1;
        }

        private bool CheckInvZTest(RenderPassMode passMode, int row, int col, float z) {
            float oldZ = m_FrontDepthBuffer.GetItem(col, row);
            if (oldZ < 0)
                return true;
            int cmp = CompareZBuffer(oldZ, z);
            switch (passMode.ZTest) {
                case ZTestOp.Equal:
                    return cmp == 0;
                case ZTestOp.Greate:
                    return cmp > 1;
                case ZTestOp.GreateEqual:
                    return cmp >= 0;
                case ZTestOp.Less:
                    return cmp < 0;
                case ZTestOp.LessEqual:
                    return cmp <= 0;
                default:
                    return false;
            }
        }

        // ZTest检查
        private bool CheckZTest(RenderPassMode passMode, int row, int col, Vector3 p) {
            return CheckZTest(passMode, row, col, p.z);
        }

        private bool CheckZTest(RenderPassMode passMode, int row, int col, float pz) {
            float z = TransZBuffer(pz);
            float oldZ = m_FrontDepthBuffer.GetItem(col, row);
            if (oldZ < 0)
                return true;
            int cmp = CompareZBuffer(oldZ, z);
            switch (passMode.ZTest) {
                case ZTestOp.Equal:
                    return cmp == 0;
                case ZTestOp.Greate:
                    return cmp > 1;
                case ZTestOp.GreateEqual:
                    return cmp >= 0;
                case ZTestOp.Less:
                    return cmp < 0;
                case ZTestOp.LessEqual:
                    return cmp <= 0;
                default:
                    return false;
            }
        }

        private void FillZBuffer(int row, int col, float z) {
            m_FrontDepthBuffer.SetItem(col, row, z);
            if (m_IsCleanedDepth) {
                m_IsCleanedDepth = false;

                m_DepthDirthRect.xMin = col;
                m_DepthDirthRect.xMax = col;
                m_DepthDirthRect.yMin = row;
                m_DepthDirthRect.yMax = row;
            } else {
                m_DepthDirthRect.xMin = Mathf.Min(col, m_DepthDirthRect.xMin);
                m_DepthDirthRect.xMax = Mathf.Max(col, m_DepthDirthRect.xMax);
                m_DepthDirthRect.yMin = Mathf.Min(row, m_DepthDirthRect.yMin);
                m_DepthDirthRect.yMax = Mathf.Max(row, m_DepthDirthRect.yMax);
            }
        }

        private void ScanlineFill(TriangleVertex tri, int row, Vector3 start, Vector3 end, 
              Color startColor, Color endColor,
              RenderPassMode passMode, out int minCol, out int maxCol) {
            float dx = end.x - start.x;
            minCol = -1;
            maxCol = -1;
            for (float x = start.x; x <= end.x; x += 1.0f) {
                int xIndex = (int)(x + 0.5f);
                if (xIndex >= m_FrontColorBuffer.Width)
                    break;

                if (xIndex >= 0 && xIndex < m_FrontColorBuffer.Width) {
                    float lerpFactor = 1f;
                    if (Mathf.Abs(dx) > float.Epsilon) {
                        lerpFactor = 1f - (x - start.x) / dx;
                    }

                    // //深度测试
                    //1/z’与x’和y'是线性关系的

                    float startInvZ = start.z;
                    if (Mathf.Abs(startInvZ) > float.Epsilon)
                        startInvZ = 1f / startInvZ;

                    float endInvZ = end.z;
                    if (Mathf.Abs(endInvZ) > float.Epsilon)
                        endInvZ = 1f / endInvZ;

                    float oneDivZ = SoftMath.GetFloatDeltaT(startInvZ, endInvZ, lerpFactor);

                    bool doFill = false;
                    bool isUseEarlyZ = (passMode.pixelShader == null) || (!passMode.pixelShader.isUseClip);

                    /*
                         * 传统Z-Test其实是发生在PS之后的，因此仅仅依靠Z-Test并不能加快多少渲染速度。而EZC则发生在光栅化之后，
                         * 调用PS之前。EZC会提前对深度进行比较，如果测试通过(Z-Func)，则执行PS，否则跳过此片段/像素(fragment/pixel)。
                         * 不过要注意的是，在PS中不能修改深度值，否则EZC会被禁用。
                         */

                    // 填充颜色 early-z culling
                    if ((!isUseEarlyZ) || CheckInvZTest(passMode, row, xIndex, oneDivZ)) {

                        float w = 1f / oneDivZ;
                        //插值顶点 原先需要插值的信息均乘以oneDivZ
                        //现在得到插值后的信息需要除以oneDivZ得到真实值
                        Color color = SoftMath.GetColorDeltaT(startColor, endColor, lerpFactor) * w;
                        // 这部分是PixelShader
                        if (passMode.pixelShader != null) {
                            PixelData data = new PixelData();
                            data.info.color = color;
                            data.info.u = xIndex;
                            data.info.v = row;
                            doFill = passMode.pixelShader.Main(data, out color);
                        }

                        if (doFill) {
                            // 如果不是Early-Z模式，需要再执行一次ZTEST检查
                            if (isUseEarlyZ || CheckInvZTest(passMode, row, xIndex, oneDivZ)) {
                                m_FrontColorBuffer.SetPixel(xIndex, row, color);
                                // 写入ZBUFFER
                                // Debug.LogErrorFormat("y: %d z: %s", row, P.z);
                                // 填充ZBUFFER
                                FillZBuffer(row, xIndex, oneDivZ);

                                if (minCol < 0 && maxCol < 0) {
                                    minCol = xIndex;
                                    maxCol = xIndex;
                                } else {
                                    minCol = minCol > xIndex? xIndex : minCol;
                                    maxCol = maxCol < xIndex ? xIndex : maxCol;
                                }
                            }
                        }

                    }
                }
            }
        }

        // 行填充
        private void ScreenSpaceScanLine(TriangleVertex tri, int row, Vector3 screenStart, Vector3 screenEnd,
                Color startColor, Color endColor,
                RenderPassMode passMode, out int minCol, out int maxCol) {
            // 扫描线
            int col = Mathf.Max(0, Mathf.FloorToInt(screenStart.x));
            minCol = col;
            float startX = col + 0.5f;
            float endX = Mathf.Min(m_FrontColorBuffer.Width - 1, Mathf.CeilToInt(screenEnd.x)) + 0.5f;
//            float e = -float.Epsilon;
            while (startX <= endX) {

                if (startX >= screenStart.x) {
                    Vector3 P = new Vector2(startX, screenStart.y);

                    float a, b, c;
                    P.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, P, out a, out b, out c);

                //   a = Mathf.Max(0, a);
                //    b = Mathf.Max(0, b);
                //    c = Mathf.Max(0, c);

                    bool isVaildP = (P.z <= 1f) && (a >= -float.Epsilon) && (b >= -float.Epsilon) && (c >= -float.Epsilon);
                    //  isVaildP = true;

                    if (isVaildP) {

                        // 1.判断是否在三角形中。有两种方法：1.使用向量叉乘，保证AP都在AB,BC,CA的同侧。2.使用重心坐标，求出a,b,c都是大于0的
                        //   if (a >= e && b >= e && c >= e) 
                        {

                            bool doFill = false;
                            bool isUseEarlyZ = (passMode.pixelShader == null) || (!passMode.pixelShader.isUseClip);

                            /*
                             * 传统Z-Test其实是发生在PS之后的，因此仅仅依靠Z-Test并不能加快多少渲染速度。而EZC则发生在光栅化之后，
                             * 调用PS之前。EZC会提前对深度进行比较，如果测试通过(Z-Func)，则执行PS，否则跳过此片段/像素(fragment/pixel)。
                             * 不过要注意的是，在PS中不能修改深度值，否则EZC会被禁用。
                             */

                            // 填充颜色 early-z culling
                            if ((!isUseEarlyZ) || CheckZTest(passMode, row, col, P)) {
                                //Color color = SoftMath.GetColorLerpFromScreenX(screenStart, screenEnd, P, startColor, endColor);

                                Color color = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, P.z, a, b, c);

                                //Color color = SoftMath.GetColorFromProjZ(screenStart.z, screenEnd.z, P.z, startColor, endColor);
                                // 这部分是PixelShader
                                if (passMode.pixelShader != null) {
                                    PixelData data = new PixelData();
                                    data.info.color = color;
                                    data.info.u = col;
                                    data.info.v = row;
                                    doFill = passMode.pixelShader.Main(data, out color);
                                }
                                // ----------------
                                if (doFill) {
                                    // 如果不是Early-Z模式，需要再执行一次ZTEST检查
                                    if (isUseEarlyZ || CheckZTest(passMode, row, col, P)) {
                                        m_FrontColorBuffer.SetPixel(col, row, color);
                                        // 写入ZBUFFER
                                        // Debug.LogErrorFormat("y: %d z: %s", row, P.z);
                                        float z = TransZBuffer(P.z);
                                        // 填充ZBUFFER
                                        FillZBuffer(row, col, z);
                                    }
                                }
                            }
                        }
                    }
                }

                startX += 1f; // 每次增加一个像素
                ++col;
            }
            maxCol = col;
        }

        // 在投影坐标系中的
        /*
        public static Vector3 GetProjSpaceVector3FromY(Vector3 v, Vector3 start, float y) {
            Vector3 ret;
            ret.y = y;
            
            bool isDY = Mathf.Abs(v.y) <= float.Epsilon; 
            if (isDY) {
                ret.x = 0f;
                ret.z = 1.1f; // 这个就是让它剔除
                return ret;
            }

            bool isDX = Mathf.Abs(v.x) <= float.Epsilon;
            
            bool isDZ = Mathf.Abs(v.z) <= float.Epsilon;
            if (isDX || (isDZ && isDY)) {
                ret.x = start.x;
            } else {
                ret.x = v.x * ((ret.y - start.y) / v.y) + start.x;
            }


            if (isDZ || (isDX && isDY)) {
                ret.z = start.z;
            } else {
                ret.z = v.z * ((ret.y - start.y) / v.y) + start.z;
            }

            return ret;
        }*/

            // v 是方向
        public static float GetZFromVector2(Vector3 v, Vector3 start, float y) {
            bool isZeroY = Mathf.Abs(v.y) <= float.Epsilon;
            if (isZeroY)
                return 1.1f; // 让它被剔除
            float ret = (y - start.y) * v.z / v.y + start.z;
            return ret;
        }

        // v是方向
        public static float GetVector2XFromY(Vector2 v, Vector2 start, float y) {
            bool isZeroX = Mathf.Abs(v.x) <= float.Epsilon;
            bool isZeroY = Mathf.Abs(v.y) <= float.Epsilon;
            if (isZeroX && isZeroY) {
                return 0;
            } else if (isZeroX) {
                return start.x;
            } else if (isZeroY) {
                return 0;
            } else {
                float ret = v.x / v.y * (y - start.y) + start.x;
                return ret;
            }
        }

        protected void UpdateColorBufferRect(int minRow, int maxRow, int minCol, int maxCol) {
            if (m_ColorDirthRect.size.x <= 0) {
                m_ColorDirthRect.xMin = minCol;
                m_ColorDirthRect.xMax = maxCol;
            } else {
                if (minCol < m_ColorDirthRect.xMin)
                    m_ColorDirthRect.xMin = minCol;
                if (maxCol > m_ColorDirthRect.xMax)
                    m_ColorDirthRect.xMax = maxCol;
            }
            if (m_ColorDirthRect.size.y <= 0) {
                m_ColorDirthRect.yMin = minRow;
                m_ColorDirthRect.yMax = maxRow;
            } else {
                if (minRow < m_ColorDirthRect.yMin)
                    m_ColorDirthRect.yMin = minRow;
                if (maxRow > m_ColorDirthRect.yMax)
                    m_ColorDirthRect.yMax = maxRow;
            }
        }

        protected float GetZFromBarycentricCoordinate(Triangle tri, Vector2 P) {
            float a, b, c;
            SoftMath.GetBarycentricCoordinate(tri, P, out a, out b, out c);
            if (a >= 0 && b >=0 && c >= 0) {
                float ret = tri.p1.z * a + tri.p2.z * b + tri.p3.z * c;
                return ret;
            }

            return 1.1f; // 让它被裁剪
        }

        protected void DrawTopTriangle(RenderPassMode passMode, TriangleVertex tri) {

            /*
             *    top(p1)
             * bottom(p3) middle(p2)
             */

            int minCol = -1;
            int maxCol = -1;
            int minRow = -1;
            int maxRow = -1;
            bool isSet = false;

            float dy = 0;
            float dd = tri.triangle.p1.y - tri.triangle.p3.y;
            for (float y = tri.triangle.p3.y; y <= tri.triangle.p1.y; y += 1.0f) {
                int yIndex = (int)(y + 0.5f);
                if (yIndex >= m_FrontColorBuffer.Height)
                    break;

                if (yIndex >= 0 && yIndex < m_FrontColorBuffer.Height) {

                    if (minRow < 0)
                        minRow = yIndex;
                    else if (minRow > yIndex)
                        minRow = yIndex;

                    if (maxRow < 0)
                        maxRow = yIndex;
                    else if (maxRow < yIndex)
                        maxRow = yIndex;

                    float t = 1f - dy / dd;
                    Vector3 start = SoftMath.GetVector3DeltaT(tri.triangle.p3, tri.triangle.p1, t);
                    Vector3 end = SoftMath.GetVector3DeltaT(tri.triangle.p2, tri.triangle.p1, t);
                    dy += 1.0f;

                    Color startColor = SoftMath.GetColorDeltaT(tri.cP3, tri.cP1, t);
                    Color endColor = SoftMath.GetColorDeltaT(tri.cP2, tri.cP1, t);

                    /*
                    float a, b, c;
                    SoftMath.GetBarycentricCoordinate(tri.triangle, start, out a, out b, out c);
                    Color startColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, start.z, a, b, c);

                    SoftMath.GetBarycentricCoordinate(tri.triangle, end, out a, out b, out c);
                    Color endColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, end.z, a, b, c);
                    */

                    int miC, maC;
                    ScanlineFill(tri, yIndex, start, end, startColor, endColor, passMode, out miC, out maC);

                    if (miC >= 0 && (minCol < 0 || minCol > miC)) {
                        minCol = miC;
                        isSet = true;
                    }
                    if (maC >= 0 && (maxCol < 0 || maxCol < maC)) {
                        maxCol = maC;
                        isSet = true;
                    }

                    
                }
            }

            // 更新填充Rect
            if (isSet) {
                UpdateColorBufferRect(minRow, maxRow, minCol, maxCol);
            }

        }

        protected void DrawBottomTriangle(RenderPassMode passMode, TriangleVertex tri) {

            // middle(p2)----top(p1)
            //  \       /
            //    bottom(p3)

            int minCol = -1;
            int maxCol = -1;
            int minRow = -1;
            int maxRow = -1;
            bool isSet = false;

            float dy = 0;
            float dd = tri.triangle.p1.y - tri.triangle.p3.y;
            for (float y = tri.triangle.p3.y; y <= tri.triangle.p1.y; y += 1.0f) {
                int yIndex = (int)(y + 0.5f);
                if (yIndex >= m_FrontColorBuffer.Height)
                    break;

                if (yIndex >= 0 && yIndex < m_FrontColorBuffer.Height) {

                    if (minRow < 0)
                        minRow = yIndex;
                    else if (minRow > yIndex)
                        minRow = yIndex;

                    if (maxRow < 0)
                        maxRow = yIndex;
                    else if (maxRow < yIndex)
                        maxRow = yIndex;

                    float t = 1f - dy / dd;
                    Vector3 start = SoftMath.GetVector3DeltaT(tri.triangle.p3, tri.triangle.p2, t);
                    Vector3 end = SoftMath.GetVector3DeltaT(tri.triangle.p3, tri.triangle.p1, t);
                    dy += 1.0f;

                    Color startColor = SoftMath.GetColorDeltaT(tri.cP3, tri.cP2, t);
                    Color endColor = SoftMath.GetColorDeltaT(tri.cP3, tri.cP1, t);

                    int miC, maC;
                    ScanlineFill(tri, yIndex, start, end, startColor, endColor, passMode, out miC, out maC);

                    if (miC >= 0 && (minCol < 0 || minCol > miC)) {
                        minCol = miC;
                        isSet = true;
                    }
                    if (maC >= 0 && (maxCol < 0 || maxCol < maC)) {
                        maxCol = maC;
                        isSet = true;
                    }

                    
                }
            }

            // 更新填充Rect
            if (isSet) {
                UpdateColorBufferRect(minRow, maxRow, minCol, maxCol);
            }

        }

        // 填充上三角形
        protected void FillScreenTopTriangle(RenderPassMode passMode, TriangleVertex tri) {
            //   top
            // bottom middle
            int yStart = Mathf.Max(Mathf.FloorToInt(tri.triangle.p3.y), 0);
            int yEnd = Mathf.Min(Mathf.CeilToInt(tri.triangle.p1.y), m_FrontColorBuffer.Height - 1);
            Vector2 bottomTop = tri.triangle.p1 - tri.triangle.p3;
            Vector2 middleTop = tri.triangle.p1 - tri.triangle.p2;
            int maxW = m_FrontColorBuffer.Width;
            int minCol = -1;
            int maxCol = -1;
            int minRow = -1;
            int maxRow = -1;
            bool isSet = false;
            for (int row = yStart; row <= yEnd; ++row) {
                float y = row + 0.5f;
                 if (y < tri.triangle.p3.y)
                     continue;
                  if (y > tri.triangle.p1.y)
                     break;

                if (minRow < 0)
                    minRow = row;
                else if (minRow > row)
                    minRow = row;

                if (maxRow < 0)
                    maxRow = row;
                else if (maxRow < row)
                    maxRow = row;

                Vector3 start = Vector3.zero;
                Vector3 end = Vector3.zero;
                start.y = y; end.y = y;

                start.x = GetVector2XFromY(bottomTop, tri.triangle.p3, y);
                end.x = GetVector2XFromY(middleTop, tri.triangle.p2, y);

                float a, b, c;
                start.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, start, out a, out b, out c);
           //     a = Mathf.Max(a, 0);
           //     b = Mathf.Max(b, 0);
           //     c = Mathf.Max(c, 0);

                bool isVaidDelta = (a >= -float.Epsilon) && (b >= -float.Epsilon) && (c >= -float.Epsilon);
                bool isStartVaild = (start.z <= 1f) && isVaidDelta;
                end.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, end, out a, out b, out c);
              //  a = Mathf.Max(a, 0);
            //    b = Mathf.Max(b, 0);
              //  c = Mathf.Max(c, 0);

                isVaidDelta = (a >= -float.Epsilon) && (b >= -float.Epsilon) && (c >= -float.Epsilon);
                bool isEndVaild = (end.z <= 1f) && isVaidDelta;

                   if (isStartVaild || isEndVaild) 
                {
                    Color startColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, start.z, a, b, c);
                    Color endColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, end.z, a, b, c);

                    int miC, maC;
                    ScreenSpaceScanLine(tri, row, start, end, startColor, endColor, passMode, out miC, out maC);
                    if (miC >= 0 && (minCol < 0 || minCol > miC)) {
                        minCol = miC;
                        isSet = true;
                    }
                    if (maC >= 0 && (maxCol < 0 || maxCol < maC)) {
                        maxCol = maC;
                        isSet = true;
                    }
                    
                } else {
                    Debug.LogErrorFormat("a: {0}, b: {1}, c: {2} row: {3}", a, b, c, row);
                }
            }

            if (isSet) {
                UpdateColorBufferRect(minRow, maxRow, minCol, maxCol);
            }
        }

        // 填充下三角形
        protected void FillScreenBottomTriangle(RenderPassMode passMode, TriangleVertex tri) {
            // middle(p2)----top(p1)
            //  \       /
            //    bottom(p3)
            int yStart = Mathf.Max(Mathf.FloorToInt(tri.triangle.p3.y), 0);
            int yEnd = Mathf.Min(Mathf.CeilToInt(tri.triangle.p2.y), m_FrontColorBuffer.Height - 1);

            Vector2 bottomMiddle = tri.triangle.p2 - tri.triangle.p3;
            Vector2 bottomTop = tri.triangle.p1 - tri.triangle.p3;
            int maxW = m_FrontColorBuffer.Width;
            int minCol = -1;
            int maxCol = -1;
            int minRow = -1;
            int maxRow = -1;
            bool isSet = false;
            for (int row = yStart; row <= yEnd; ++row) {
                
                float y = row + 0.5f;
                
                if (y < tri.triangle.p3.y)
                    continue;
                if (y > tri.triangle.p2.y)
                    break;
                    
                if (minRow < 0)
                    minRow = row;
                else if (minRow > row)
                    minRow = row;

                if (maxRow < 0)
                    maxRow = row;
                else if (maxRow < row)
                    maxRow = row;

                Vector3 start = Vector3.zero;
                Vector3 end = Vector3.zero;

                start.y = y; end.y = y;

                start.x = GetVector2XFromY(bottomMiddle, tri.triangle.p3, y);
                end.x = GetVector2XFromY(bottomTop, tri.triangle.p3, y);

                float a, b, c;
                start.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, start, out a, out b, out c);
                bool isVaidDelta = (a >= -float.Epsilon) && (b >= -float.Epsilon) && (c >= -float.Epsilon);
                bool isStartVaild = (start.z <= 1f) && isVaidDelta;
                end.z = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, end, out a, out b, out c);
                isVaidDelta = (a >= -float.Epsilon) && (b >= -float.Epsilon) && (c >= -float.Epsilon);
                bool isEndVaild = (end.z <= 1f) && isVaidDelta;

                if (isStartVaild || isEndVaild) {
                    Color startColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, start.z, a, b, c);
                    Color endColor = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, end.z, a, b, c);

                    // 扫描
                    int miC, maC;
                    ScreenSpaceScanLine(tri, row, start, end, startColor, endColor, passMode, out miC, out maC);
                    if ((miC >= 0) && (minCol < 0 || minCol > miC)) {
                        minCol = miC;
                        isSet = true;
                    }
                    if ((maC >= 0) && (maxCol < 0 || maxCol < maC)) {
                        maxCol = maC;
                        isSet = true;
                    }

                    
                }
            }

            // 更新填充Rect
            if (isSet) {
                UpdateColorBufferRect(minRow, maxRow, minCol, maxCol);
            }
        }

        // 采用矩形的方式，非三角形的方式
        internal void FlipScreenTriangle2(SoftCamera camera, TriangleVertex tri, RenderPassMode passMode) {
            if (passMode.pixelShader != null) {
                passMode.pixelShader.SetParam(this);
            }
            try {
                float minZ, maxZ;
                Rect r = tri.triangle.GetRect(out minZ, out maxZ);
                int yStart = Mathf.Clamp((int)r.yMin, 0, m_FrontColorBuffer.Height - 1);
                int yEnd = Mathf.Clamp((int)r.yMax, 0, m_FrontColorBuffer.Height - 1);
                int xStart = Mathf.Clamp((int)r.xMin, 0, m_FrontColorBuffer.Width - 1);
                int xEnd = Mathf.Clamp((int)r.xMax, 0, m_FrontColorBuffer.Width - 1);

                //if (yEnd - yStart <= 1)
                //    return;

                // if (xEnd - xStart <= 1)
                //    return;

                int minCol = -1;
                int maxCol = -1;
                int minRow = -1;
                int maxRow = -1;

                bool isSet = false;
                for (int row = yStart; row <= yEnd; ++row) {
                    float y = row + 0.5f;
                    if (y < r.yMin)
                        continue;
                    if (y > r.yMax)
                        break;

                    bool isSetRow = false;
                    for (int col = xStart; col <= xEnd; ++col) {
                        float x = col + 0.5f;
                        if (x < r.xMin)
                            continue;
                        if (x > r.xMax)
                            break;

                        Vector2 P = new Vector2(x, y);

                        // 使用叉乘判斷點是否在三角形上面
                        // 不要使用重心坐标判断是否在三角形内，有误差。这里采用屏幕上的三角形向量叉乘特性：三角形内的点，一定在边向量同侧。
                        bool isScreenVaild = SoftMath.ScreenSpacePtInTriangle(tri.triangle.p1, tri.triangle.p2, tri.triangle.p3, P);

                        if (isScreenVaild) {
                            float a, b, c;
                            float pz = SoftMath.GetProjSpaceBarycentricCoordinateZ(tri, P, out a, out b, out c);

                            // 下面的有注释，是因为开启有缝隙。。。
                            // 不要使用重心坐标判断是否在三角形内，有误差
                            bool isVaildP = (pz <= 1.0f) /*&& (a >= 0) && (b >= 0) && (c >= 0) && (pz >= minZ) && (pz <= maxZ)*/;
                            if (isVaildP) {

                                bool doFill = false;
                                bool isUseEarlyZ = (passMode.pixelShader == null) || (!passMode.pixelShader.isUseClip);

                                if ((!isUseEarlyZ) || CheckZTest(passMode, row, col, pz)) {



                                    Color color = SoftMath.GetColorFromProjSpaceBarycentricCoordinateAndZ(tri, pz, a, b, c);
                                    Vector4 uv1 = SoftMath.GetUV1FromProjSpaceBarycentricCoordinateAndZ(tri, pz, a, b, c);

                                    if (passMode.pixelShader != null) {
                                        PixelData data = new PixelData();
                                        data.info.color = color;
                                        data.mainTex = tri.MainTexture;
                                        data.info.uv1 = uv1;
                                        data.info.u = col;
                                        data.info.v = row;
                                        doFill = passMode.pixelShader.Main(data, out color);
                                    } else {
                                        doFill = true;
                                    }

                                    if (doFill) {
                                        // 如果不是Early-Z模式，需要再执行一次ZTEST检查
                                        if (isUseEarlyZ || CheckZTest(passMode, row, col, pz)) {
                                            m_FrontColorBuffer.SetPixel(col, row, color, uv1);
                                            // 写入ZBUFFER
                                            // Debug.LogErrorFormat("y: %d z: %s", row, P.z);
                                            // 填充ZBUFFER

                                            float z = TransZBuffer(pz);
                                            FillZBuffer(row, col, z);

                                            isSetRow = true;

                                            if (minCol < 0 && maxCol < 0) {
                                                minCol = col;
                                                maxCol = col;
                                            } else {
                                                minCol = minCol > col ? col : minCol;
                                                maxCol = maxCol < col ? col : maxCol;
                                            }

                                        }
                                    }

                                }
                            }

                        }
                    }

                    if (isSetRow) {
                        if (minRow < 0)
                            minRow = row;
                        else if (minRow > row)
                            minRow = row;

                        if (maxRow < 0)
                            maxRow = row;
                        else if (maxRow < row)
                            maxRow = row;

                        isSet = true;
                    }
                }

                // 更新填充Rect
                if (isSet) {
                    UpdateColorBufferRect(minRow, maxRow, minCol, maxCol);
                }
            } finally {
                if (passMode.pixelShader != null) {
                    passMode.pixelShader.ResetParam();
                }
            }
        }

        // tri已经是屏幕坐标系
        internal void FlipScreenTriangle(SoftCamera camera, TriangleVertex tri, RenderPassMode passMode) {
            if (passMode.pixelShader != null) {
                passMode.pixelShader.SetParam(this);
            }
            // 三角形
            try {
                TriangleVertex topTri, bottomTri;
                var triType = tri.GetScreenSpaceTopBottomTriangle(camera, out topTri, out bottomTri);
                switch (triType) {
                    case TriangleVertex.ScreenSpaceTopBottomType.top:

#if !_USE_NEW_LERP_Z
                     FillScreenTopTriangle(passMode, topTri);
#else
                        DrawTopTriangle(passMode, topTri);
#endif
                        break;
                    case TriangleVertex.ScreenSpaceTopBottomType.bottom:

#if !_USE_NEW_LERP_Z
                   FillScreenBottomTriangle(passMode, bottomTri);
#else
                        DrawBottomTriangle(passMode, bottomTri);
#endif
                        break;
                    case TriangleVertex.ScreenSpaceTopBottomType.topBottom:
#if !_USE_NEW_LERP_Z
                    FillScreenTopTriangle(passMode, topTri);
                     DrawBottomTriangle(passMode, bottomTri);
#else
                        DrawTopTriangle(passMode, topTri);
                        DrawBottomTriangle(passMode, bottomTri);
#endif
                        break;
                }
            } finally {
                if (passMode.pixelShader != null) {
                    passMode.pixelShader.ResetParam();
                }
            }

            }
    }
}
