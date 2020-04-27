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

    public struct Triangle {
       public Vector3 p1, p2, p3;
       public Triangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public void MulMatrix(ref Matrix4x4 mat) {
            p1 = mat * p1;
            p2 = mat * p2;
            p3 = mat * p3;
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

        private void CheckClipPt(ref Vector2 pt) {
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
    }
}
