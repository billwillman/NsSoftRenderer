using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace NsSoftRenderer {
    // 软渲染设备
    public class SoftDevice: DisposeObject {
        private ColorBuffer m_FrontColorBuffer = null;
        private Depth32Buffer m_FrontDepthBuffer = null;

        public SoftDevice(int deviceWidth, int deviceHeight) {
            m_FrontColorBuffer = new ColorBuffer(deviceWidth, deviceHeight);
            m_FrontDepthBuffer = new Depth32Buffer(deviceWidth, deviceHeight);
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
