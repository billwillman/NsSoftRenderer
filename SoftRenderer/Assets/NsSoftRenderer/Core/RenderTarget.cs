using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

using RenderTargetClearFlags = System.Int32;

namespace NsSoftRenderer {

    public enum RenderTargetClearFlag {
        None = 0,
        Color = 1,
        Depth = 2
    };

    public class RenderTarget: DisposeObject {
        private ColorBuffer m_FrontColorBuffer = null;
        private Depth32Buffer m_FrontDepthBuffer = null;
        private RenderTargetClearFlags m_ClearFlags = 0;
        // 脏矩形
        private RectInt m_ColorDirthRect = new RectInt(0, 0, 0, 0);
        private RectInt m_DepthDirthRect = new RectInt(0, 0, 0, 0);
        private bool m_IsCleanedColor = true;
        private bool m_IsCleanedDepth = true;

        public RenderTarget(int deviceWidth, int deviceHeight) {
            m_FrontColorBuffer = new ColorBuffer(deviceWidth, deviceHeight);
            m_FrontDepthBuffer = new Depth32Buffer(deviceWidth, deviceHeight);
        }

        // 清理参数
        public RenderTargetClearFlags ClearFlags {
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

        public static RenderTargetClearFlags CombineUseFlag(RenderTargetClearFlags old, RenderTargetClearFlag flag) {
            RenderTargetClearFlags ret = old | (1 << ((int)flag - 1));
            return ret;
        }


        public static bool IncludeUseFlag(RenderTargetClearFlags flags, RenderTargetClearFlag flag) {
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

        public bool DrawPixel(int x, int y, RenderTargetClearFlags flags, Color color, int depth = 0) {
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
