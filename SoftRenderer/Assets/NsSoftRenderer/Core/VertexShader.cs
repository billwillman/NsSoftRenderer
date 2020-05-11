using UnityEngine;

namespace NsSoftRenderer {

    // 一个VertexShader
    public class VertexShader {
        // 主函数,没有弄额外的结构，懒得弄，简单些
        public virtual void Main(ref TriangleVertex vertex) {
            vertex.triangle.MulMatrix(MVPMatrix);
        }

        public Matrix4x4 MVPMatrix = Matrix4x4.identity;
    }

    public struct PixelData {
        public Color color;
    }

    public class PixelShader {
        // 是否开启了Clip
        public bool isUseClip = false;
        public virtual bool Main(PixelData data, out Color frag) {
            frag = data.color;
            return true; // 这里返回值模拟clip操作
        }
    }
}