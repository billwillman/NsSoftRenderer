using UnityEngine;
using NsSoftRenderer;

[RequireComponent(typeof(Camera))]
public class SoftCameraBinder: MonoBehaviour {

    // 是否开启视锥体球面剪裁
    public bool IsOpenCameraSpereCull = true;
    public bool ZBuffer_RevertZ = true;

    private Camera m_Cam = null;
    private SoftCamera m_SoftCam = null;

    private void Start() {

        m_Cam = GetComponent<Camera>();

        if (m_Cam != null) {
            var device = SoftDevice.StaticDevice;
            if (device != null) {
                m_SoftCam = device.AddCamera(m_Cam);
            }
        }
    }

    private void Update() {
        if (m_Cam != null && m_SoftCam != null) {
            m_SoftCam.UpdateCamera(m_Cam);
            m_SoftCam.IsOpenCameraSpereCull = this.IsOpenCameraSpereCull;
            m_SoftCam.ZBuffer_RevertZ = ZBuffer_RevertZ;
        }
    }

    private void OnDestroy() {
        if (m_SoftCam != null) {
            m_SoftCam.Dispose();
            m_SoftCam = null;
        }
    }
}