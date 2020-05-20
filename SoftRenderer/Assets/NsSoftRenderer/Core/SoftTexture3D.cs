using UnityEngine;
using Utils;

namespace NsSoftRenderer {
    // 立方体纹理
    public class CubeTexture: DisposeObject, SoftTexture {
        // 6个面
        private SoftTexture2D[] m_Texs = new SoftTexture2D[6];

        public int uuid {
            get;
            set;
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_Texs != null) {
                for (int i = 0; i < m_Texs.Length; ++i) {
                    var tex = m_Texs[i];
                    if (tex != null) {
                        tex.Dispose();
                        m_Texs[i] = null;
                    }
                }
            }
        }

    }
}
