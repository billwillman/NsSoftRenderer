// 显示MIPMAP的PixelShader

using UnityEngine;

namespace NsSoftRenderer {

    public class MipMapShowPixelShader: PixelShader {

        public static Color[] m_MipColor = {
            new Color(1, 0, 0),
            new Color(0, 0, 1),
            new Color(1, 0.5f, 0),
            new Color(1, 0, 0.5f),
            new Color(0, 0.5f, 0.5f),
            new Color(0, 0.25f, 0.5f),
            new Color(0.25f, 0.5f, 0),
            new Color(0.5f, 0, 1),
            new Color(1, 0.25f, 0.5f),
            new Color(0.5f, 0.5f, 0.5f),
            new Color(0.25f, 0.25f, 0.25f),
            new Color(0.125f, 0.125f, 0.125f)
        };

        private static int m_ColorCnt = m_MipColor.Length;

        public override bool Main(PixelData data, out Color frag) {
            var target = this.param.target;
            var tex = data.mainTex;
            if (tex != null && target != null) {
                float dw = 1f / tex.Width;
                float dh = 1f / tex.Height;

                Vector2 uv = data.info.uv1;
                int u = data.info.u - 1;
                int v = data.info.v - 1;
                // 水平变化率
                float ddx = 0f;
                if (u > 0) {
                    var info1 = target.FrontColorBuffer.GetItem(u, data.info.v);
                    if (info1.isFill != 0) {
                        // UV变化率 除以1是因为是1距离的变化
                        Vector2 uv1 = info1.uv1;
                        ddx = (((uv - uv1).magnitude) / 1f) / dw;
                    }
                }

                // 垂直变化率
                float ddy = 0f;
                if (v > 0) {
                    var info2 = target.FrontColorBuffer.GetItem(data.info.u, v);
                    if (info2.isFill != 0) {
                        Vector2 uv2 = info2.uv1;
                        ddy = (((uv - uv2).magnitude) / 1f) / dh;
                    }
                }

               
                float maxDD = Mathf.Max(ddx, ddy);

                if (maxDD >= 0) {
                    float n = Mathf.Log(maxDD, 2);

                    int s = Mathf.FloorToInt(n);
                    int e = Mathf.CeilToInt(n);
                    float t = SoftMath.GetDeltaT(s, e, n);

                    int idx = s - 1;

                    idx = Mathf.Clamp(idx, 0, m_ColorCnt - 1);
                    Color sC = m_MipColor[idx];

                    idx = e - 1;
                    idx = Mathf.Clamp(idx, 0, m_ColorCnt - 1);
                    Color eC = m_MipColor[idx];


                    frag = SoftMath.GetColorDeltaT(sC, eC, t);
                } else
                    frag = Color.black;
            } else
                frag = Color.black;
            
            return true;
        }
    }
}