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

        public void Update(float delta) {
            // 1.先清理Target
            if (m_RenerTarget != null)
                m_RenerTarget.Prepare();
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
