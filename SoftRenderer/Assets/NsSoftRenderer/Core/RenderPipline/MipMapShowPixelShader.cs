// 显示MIPMAP的PixelShader

using UnityEngine;

namespace NsSoftRenderer {

    public class MipMapShowPixelShader: PixelShader {
        public override bool Main(PixelData data, out Color frag) {
            var target = this.param.target;
            var tex = data.mainTex;
            if (tex != null && target != null) {
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
                        ddx = ((uv - uv1).magnitude) / 1f * tex.Width;
                    }
                }

                // 垂直变化率
                float ddy = 0f;
                if (v > 0) {
                    var info2 = target.FrontColorBuffer.GetItem(u, data.info.v);
                    if (info2.isFill != 0) {
                        Vector2 uv2 = info2.uv1;
                        ddy = ((uv - uv2).magnitude) / 1f * tex.Height;
                    }
                }

                float maxDD = Mathf.Max(ddx, ddy);
                float n = Mathf.Log(maxDD, 2);
                float nn = 1f/n;
                frag = new Color(nn, nn, nn);
            } else
                frag = Color.black;
            
            return true;
        }
    }
}