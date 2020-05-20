using UnityEngine;

namespace NsSoftRenderer {

    // 一个VertexShader
    public class VertexShader {
        // 主函数,没有弄额外的结构，懒得弄，简单些
        public virtual void Main(ref TriangleVertex vertex) {
            vertex.triangle.MulMatrix(m_Owner.MVPMatrix);
        }

        
        public RenderPassMode m_Owner = null;
    }

    public struct PixelInfo {
        public int u; // x坐标（垂直坐标）
        public int v; // y坐标（水平坐标）
        public Color color;
        public Vector4 uv1;
        public byte isFill;
    }

    public struct PixelData {
        public PixelInfo info;
        public SoftTexture2D mainTex;
    }

    public struct PixelShaderParam {
        public RenderTarget target;
    }


    public class PixelShader: SoftRes {
        // 是否开启了Clip
        public bool isUseClip = false;
        public PixelShaderParam param = new PixelShaderParam();

        public int uuid {
            get;
            set;
        }

        public virtual void Dispose() { }

        public void SetParam(RenderTarget target) {
            param.target = target;
        }

        public void ResetParam() {
            param = new PixelShaderParam();
        }

        public virtual bool Main(PixelData data, out Color frag) {


            /*
             * 处理纹理
             */

           
            if (data.mainTex != null) {
                var texColor = data.mainTex.GetColor(data.info.uv1);
               // texColor.a = 1f;
                frag = texColor * data.info.color;
                //  frag = data.color;
                frag.a = 1f;
            } else {
                frag = data.info.color;
            }
           
            return true; // 这里返回值模拟clip操作
        }

        public RenderPassMode m_Owner = null;
    }
}