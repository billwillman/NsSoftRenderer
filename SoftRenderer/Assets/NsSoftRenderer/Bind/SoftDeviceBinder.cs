using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using NsSoftRenderer;

public class SoftDeviceBinder : MonoBehaviour
{
    public int DeviceWidth = 800;
    public int DeviceHeight = 600;
    private SoftDevice m_Device = null;

    private void Awake() {
        m_Device = new SoftDevice(DeviceWidth, DeviceHeight);
    }

    private void Update() {
        if (m_Device != null)
            m_Device.Update(Time.deltaTime);
    }

    private void OnDestroy() {
        if (m_Device != null) {
            m_Device.Dispose();
            m_Device = null;
        }
    }
}
