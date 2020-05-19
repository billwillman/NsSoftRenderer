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

    public struct PixelData {
        public Color color;
        public SoftTexture2D mainTex;
        public Vector4 uv1;
    }

    public class PixelShader {
        // 是否开启了Clip
        public bool isUseClip = false;
        public virtual bool Main(PixelData data, out Color frag) {


            /*
             * 处理纹理
             */

           
            if (data.mainTex != null) {
                var texColor = data.mainTex.GetColor(data.uv1);
                texColor.a = 1f;
                frag = texColor * data.color;
            } else {
                frag = data.color;
            }
           
            return true; // 这里返回值模拟clip操作
        }

        public RenderPassMode m_Owner = null;
    }
}