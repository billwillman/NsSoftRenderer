using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace TssLoop {

    // Loop细分算法
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LoopTss : MonoBehaviour {

        private TssMesh m_TssMesh = null;
        public Mesh m_OrgMesh = null;

        private void Awake() {
            m_TssMesh = new TssMesh();
            BuildFromMesh();
        }

        void BuildFromMesh() {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter != null) {
                filter.sharedMesh = m_OrgMesh;
            }
            if (m_OrgMesh != null) {
                m_TssMesh = new TssMesh();
                m_TssMesh.LoadFromMesh(m_OrgMesh);
            }
        }

        private void OnDestroy() {
            if (m_TssMesh != null) {
                m_TssMesh.Dispose();
                m_TssMesh = null;
            }
        }

        private void OnGUI() {
            if (GUI.Button(new Rect(0, 0, 200, 100), "下一級細分")) {
                Process();
            }
        }

        // 开始处理
        void Process() {
            if (m_TssMesh != null) {
                m_TssMesh.TssNextLevel();
            }
        }
    }
}
