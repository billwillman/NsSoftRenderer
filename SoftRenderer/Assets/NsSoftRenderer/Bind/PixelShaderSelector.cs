using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NsSoftRenderer;

public enum PixelShaderEnum {
    None = -1,
    Default = 0,
    MipMapShow = 1,
    NormalShow = 2
}

public class PixelShaderSelector : MonoBehaviour {

    public PixelShaderEnum ShaderEnum = PixelShaderEnum.None;
    private PixelShaderEnum m_Selctor = PixelShaderEnum.None;
    private List<int> m_PixelShaderHandles = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        InitShaders();
    }

    private void Update() {
        CheckShaderEnum();
    }

    void CheckShaderEnum() {
        if (m_Selctor != ShaderEnum) {
            var device = SoftDevice.StaticDevice;
            if (device != null && device.ResMgr != null && device.Pipline != null) {

                int idx = (int)ShaderEnum;
                if (idx >= 0 && idx < m_PixelShaderHandles.Count) {
                    int handle = m_PixelShaderHandles[idx];
                    if (handle != 0) {
                        device.Pipline.AttachPixelShader(handle);
                        m_Selctor = ShaderEnum;
                    }
                }
            }
            

        }
    }

    void InitShaders() {
        var device = SoftDevice.StaticDevice;
        if (device != null) {
            var resMgr = device.ResMgr;
            if (resMgr != null) {
                int handle = resMgr.CreatePixelShader<PixelShader>();
                m_PixelShaderHandles.Add(handle);

                handle = resMgr.CreatePixelShader<MipMapShowPixelShader>();
                m_PixelShaderHandles.Add(handle);

                handle = resMgr.CreatePixelShader<NormalShowPixelShader>();
                m_PixelShaderHandles.Add(handle);
            }
        }
    }
}
