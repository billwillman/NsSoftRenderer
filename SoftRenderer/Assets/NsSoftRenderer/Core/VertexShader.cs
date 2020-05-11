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
        public virtual Color Main(PixelData data) {
            return data.color;
        }
    }
}