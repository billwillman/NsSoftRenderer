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

    public Shader[] m_Shaders = null;

    // Start is called before the first frame update
    void Start()
    {
        InitShaders();
    }

    private void Update() {
        CheckShaderEnum();
    }

    private void ProcessMaterial(Material target, PixelShaderEnum selctor) {
        switch (selctor) {
            case PixelShaderEnum.MipMapShow: {
                    List<Vector4> colorLst = new List<Vector4>(MipMapShowPixelShader.m_MipColor.Length);
                    for (int i = 0; i < MipMapShowPixelShader.m_MipColor.Length; ++i) {
                        Color color = MipMapShowPixelShader.m_MipColor[i];
                        Vector4 v = new Vector4(color.r, color.g, color.b, color.a);
                        colorLst.Add(v);
                    }
                    target.SetVectorArray("_MipmapColor", colorLst);
                    break;
                }
        }
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

                        Shader targetShader = null;
                        if ((m_Shaders != null) && (idx < m_Shaders.Length)) {
                            targetShader = m_Shaders[idx];
                        }

                        if (targetShader != null) {
                            SoftMeshBinder[] binders = GameObject.FindObjectsOfType<SoftMeshBinder>();
                            if (binders != null) {
                                for (int i = 0; i < binders.Length; ++i) {
                                    var binder = binders[i];
                                    if (binder != null) {
                                        var renderer = binder.GetComponent<MeshRenderer>();
                                        if (renderer != null) {
                                            renderer.material.shader = targetShader;
                                            ProcessMaterial(renderer.material, ShaderEnum);
                                        }
                                    }
                                }
                            }
                        }

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
