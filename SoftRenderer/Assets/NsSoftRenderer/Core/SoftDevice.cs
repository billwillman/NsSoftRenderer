using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;




namespace NsSoftRenderer {

    // 软渲染设备
    public class SoftDevice: DisposeObject {

        private RenderTarget m_RenerTarget = null;

        public SoftDevice(int deviceWidth, int deviceHeight) {
            m_RenerTarget = new RenderTarget(deviceWidth, deviceHeight);
        }

        public void Update(float delta) {
            // 绘制
        }

        protected override void OnFree(bool isManual) {
            if (m_RenerTarget != null) {
                m_RenerTarget.Dispose();
                m_RenerTarget = null;
            }
        }
    }
}
