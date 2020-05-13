using UnityEngine;
using Utils;

namespace NsSoftRenderer {

    public interface SoftRes {
        int uuid {
            get;
            set;
        }

        void Dispose();
    }

    public interface SoftTexture: SoftRes {
    }

    // 纹理
    public class SoftTexture2D: Buffer<Color32>, SoftTexture {
        public SoftTexture2D(int width, int height): base(width, height) {

        }

        public int uuid {
            get;
            set;
        }
    }
}