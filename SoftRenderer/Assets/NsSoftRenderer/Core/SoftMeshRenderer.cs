﻿using UnityEngine;

namespace NsSoftRenderer {

    public class SoftMeshRenderer : SoftRenderObject {

        private SoftMesh m_Mesh = null;

        public SoftMeshRenderer(Vector3 pos, Vector3 up, Vector3 lookAt, Mesh mesh) {
            this.Position = pos;
            this.Up = up;
            this.LookAt = lookAt;
            if (mesh != null) {
                m_Mesh = new SoftMesh(mesh);
            }
        }

        // 世界坐标系包围球
        public SoftSpere WorldBoundSpere {
            get {
                if (m_Mesh != null) {
                    SoftSpere ret = m_Mesh.LocalBoundSpere;
                    ret.position = this.LocalToGlobalMatrix * ret.position;
                    return ret;
                } else {
                    SoftSpere ret = new SoftSpere();
                    ret.position = Vector3.zero;
                    ret.radius = 0f;
                    return ret;
                }
            }
        }

        protected override void OnFree(bool isManual) {
            if (m_Mesh != null) {
                m_Mesh.Dispose();
                m_Mesh = null;
            }
        }

        public SoftMesh sharedMesh {
            get {
                return m_Mesh;
            }
            set {
                m_Mesh = value;
            }
        }

        // 提交到渲染队列中
        public override void Render(SoftCamera camera) {
            SoftDevice device = SoftDevice.StaticDevice;
            if (device != null) {
                
            }
        }
    }

}