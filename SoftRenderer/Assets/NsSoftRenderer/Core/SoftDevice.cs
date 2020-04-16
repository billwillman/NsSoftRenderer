using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;


using RenderTargetClearFlags = System.Int32;

namespace NsSoftRenderer {

    // 软渲染设备
    public class SoftDevice: DisposeObject {

        private RenderTarget m_RenerTarget = null;

        public SoftDevice(int deviceWidth, int deviceHeight) {
            m_RenerTarget = new RenderTarget(deviceWidth, deviceHeight);
        }

        public Color ClearColor {
            get {
                if (m_RenerTarget != null)
                    return m_RenerTarget.CleanColor;
                return Color.clear;
            }

            set {
                if (m_RenerTarget.CleanColor != value) {
                    m_RenerTarget.CleanColor = value;
                }
            }
        }


        public int DeviceWidth {
            get {
                if (m_RenerTarget == null)
                    return 0;
                return m_RenerTarget.Width;
            }
        }


        public int DeviceHeight {
            get {
                if (m_RenerTarget == null)
                    return 0;
                return m_RenerTarget.Height;
            }
        }

        public void Update(float delta, IRenderTargetNotify notify) {
            // 1.先清理Target
            if (m_RenerTarget != null) {
                m_RenerTarget.Prepare();
                m_RenerTarget.FlipToScreen(notify);
            }
        }

        public RenderTargetClearFlags ClearFlags {
            get {
                if (m_RenerTarget != null)
                    return m_RenerTarget.ClearFlags;
                return 0;
            }
            set {
                if (m_RenerTarget != null)
                    m_RenerTarget.ClearFlags = value;
            }
        }

        protected override void OnFree(bool isManual) {
            if (m_RenerTarget != null) {
                m_RenerTarget.Dispose();
                m_RenerTarget = null;
            }
        }
    }
}
