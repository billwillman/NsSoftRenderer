using UnityEngine;

namespace NsSoftRenderer {
    public class NormalShowPixelShader: PixelShader {
        public override bool Main(PixelData data, out Color frag) {
            var target = this.param.target;
            if (target != null) {
                int u = data.info.u - 1;
                int v = data.info.v - 1;

                if (u >= 0 && v >= 0) {
                    Vector3 ddx = Vector3.zero;

                    var info1 = target.FrontColorBuffer.GetItem(u, data.info.v);
                    if (info1.isFill != 0) {
                        ddx = data.info.pos - info1.pos;
                    }

                    Vector3 ddy = Vector3.zero;
                    var info2 = target.FrontColorBuffer.GetItem(data.info.u, v);
                    if (info2.isFill != 0) {
                        ddy = data.info.pos - info2.pos;
                    }

                    Vector3 normal = Vector3.Cross(ddx, ddy).normalized;

                    frag = new Color(normal.x, normal.y, normal.z);
                } else {
                    frag = Color.black;
                }
            } else
                frag = Color.black;

            return true;
        }
    }
}
