using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using NsSoftRenderer;

public class SoftDeviceBinder : MonoBehaviour, IRenderTargetNotify {
    public int DeviceWidth = 800;
    public int DeviceHeight = 600;
    private SoftDevice m_Device = null;
    private Texture2D m_TargetTexture = null;

    public Color m_ClearColor = Color.clear;
    public Camera[] m_StartCameras = null;

    public virtual void OnFillColor(ColorBuffer buffer, RectInt fillRect, RectInt clearRect) {
        if (buffer != null && m_TargetTexture != null && buffer.IsVaild) {
            m_TargetTexture.LoadRawTextureData<Color32>(buffer.OriginArray);
            m_TargetTexture.Apply();
        }
        /*
        bool isTexChg = false;
        if (m_Device != null && buffer != null && m_TargetTexture != null) {
            if ((clearRect.width > 0) && (clearRect.height > 0)) {
                for (int r = clearRect.yMin; r < clearRect.yMax; ++r) {
                    for (int c = clearRect.xMin; c < clearRect.xMax; ++c) {
                        m_TargetTexture.SetPixel(c, r, m_Device.ClearColor);
                    }
                }
                isTexChg = true;
            }


            if ((fillRect.width > 0) && (fillRect.height > 0)) {
                for (int r = fillRect.yMin; r < fillRect.yMax; ++r) {
                    for (int c = fillRect.xMin; c < fillRect.xMax; ++c) {
                        m_TargetTexture.SetPixel(c, r, buffer.GetItem(c, r));
                    }
                }
                isTexChg = true;
            }

            if (isTexChg)
                m_TargetTexture.Apply();
        }*/
    }

    // 显示的渲染目标
    public Renderer m_ShowRenderer = null;

    public bool m_IsShowRenderer = true;

    private void Awake() {
        DeviceWidth = Screen.width;
        DeviceHeight = Screen.height;
        m_Device = new SoftDevice(DeviceWidth, DeviceHeight);

        m_ClearColor = m_Device.ClearColor;

        InitStartCameras();
    }

    private void InitStartCameras() {
        if (m_StartCameras != null) {
            Camera mainCam = Camera.main;
            for (int i = 0; i < m_StartCameras.Length; ++i) {
                Camera cam = m_StartCameras[i];
                if (cam != null) {
                    if (cam.orthographic) {
                        OCameraInfo info = OCameraInfo.Create();
                        info.Size = cam.orthographicSize;
                        info.nearPlane = cam.nearClipPlane;
                        info.farPlane = cam.farClipPlane;
                        var trans = cam.transform;
                        bool isMainCamera = cam == mainCam;
                        m_Device.AddOCamera(info, trans.position, trans.up, trans.forward, (int)cam.depth, isMainCamera);
                    } else {
                        PCameraInfo info = PCameraInfo.Create();
                        info.nearPlane = cam.nearClipPlane;
                        info.farPlane = cam.farClipPlane;
                        info.fieldOfView = cam.fieldOfView;
                        var trans = cam.transform;
                        bool isMainCamera = cam == mainCam;
                        m_Device.AddPCamera(info, trans.position, trans.up, trans.forward, (int)cam.depth, isMainCamera);
                    }
                }
            }
        }
    }

    private void Update() {
        if (m_Device != null) {
            InitTargetTexture();
            m_Device.ClearColor = m_ClearColor;
            IRenderTargetNotify notify = null;
            if (m_IsShowRenderer)
                notify = this;

            m_Device.Update(Time.deltaTime, notify);
        }
    }

    private void InitTargetTexture() {
       if (m_Device != null) {
            if (m_TargetTexture != null) {
                if ((m_Device.DeviceWidth != m_TargetTexture.width) || (m_Device.DeviceHeight != m_TargetTexture.height)) {
                    DestroyTargetTexture();
                } else
                    return;
            }

            m_TargetTexture = new Texture2D(m_Device.DeviceWidth, m_Device.DeviceHeight, TextureFormat.RGBA32, false, false);
            m_TargetTexture.filterMode = FilterMode.Point;

            if (m_ShowRenderer != null)
                m_ShowRenderer.sharedMaterial.mainTexture = m_TargetTexture;
        } else {
            DestroyTargetTexture();
        }
    }


    private void DestroyTargetTexture() {
        if (m_TargetTexture != null) {
            GameObject.Destroy(m_TargetTexture);
            m_TargetTexture = null;
        }
    }

    private void OnDestroy() {
        if (m_Device != null) {
            m_Device.Dispose();
            m_Device = null;
        }

        DestroyTargetTexture();
    }
}
