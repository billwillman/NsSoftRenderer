using UnityEngine;
using NsSoftRenderer;

[RequireComponent(typeof(Camera))]
public class SoftCameraBinder: MonoBehaviour {

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
        }
    }

    private void OnDestroy() {
        if (m_SoftCam != null) {
            m_SoftCam.Dispose();
            m_SoftCam = null;
        }
    }
}