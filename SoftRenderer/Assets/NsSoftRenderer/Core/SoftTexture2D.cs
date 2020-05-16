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

    public enum TextureClampType {
        Clamp = 0,
        Repeat = 1,
        RepeatSwap = 2
    }

    public enum TextureFliter {
        Nearst,// 取整
        Biller // 线性
    }

    // 纹理
    public class SoftTexture2D: Buffer<Color32>, SoftTexture {

        public TextureClampType ClampType {
            get;
            set;
        }

        public TextureFliter TexFilter {
            get;
            set;
        }

        public SoftTexture2D(int width, int height): base(width, height) {

        }

        public int uuid {
            get;
            set;
        }

        private void TransUV(ref Vector2 uv) {
            switch (ClampType) {
                case TextureClampType.Clamp:
                    uv.x = Mathf.Clamp01(uv.x);
                    uv.y = Mathf.Clamp01(uv.y);
                    break;
                case TextureClampType.Repeat:
                    uv.x = Mathf.Repeat(uv.x, 1.0f);
                    uv.y = Mathf.Repeat(uv.y, 1.0f);
                    break;
                case TextureClampType.RepeatSwap:
                    uv.x = Mathf.PingPong(uv.x, 1.0f);
                    uv.y = Mathf.PingPong(uv.y, 1.0f);
                    break;
                default:
                    uv.x = Mathf.Clamp01(uv.x);
                    uv.y = Mathf.Clamp01(uv.y);
                    break;
            }
        }

        public Color GetColor(Vector2 uv) {

            TransUV(ref uv);

            switch (TexFilter) {
                case TextureFliter.Nearst:
                    return GetItem(Mathf.FloorToInt(uv.x), Mathf.FloorToInt(uv.y));
                case TextureFliter.Biller: {
                        int u0 = Mathf.FloorToInt(uv.x);
                        int u1 = Mathf.RoundToInt(uv.x);
                        int v0 = Mathf.FloorToInt(uv.y);
                        int v1 = Mathf.RoundToInt(uv.y);

                        Color c00 = GetItem(u0, v0);
                        Color c01 = GetItem(u1, v0);
                        Color c10 = GetItem(u0, v1);
                        Color c11 = GetItem(u1, v1);

                        Color c0;
                        Color c1;

                        if (u1 != u0) {
                            float t0 = (uv.x - (float)u0) / (float)(u1 - u0);
                            c0 = (1f - t0) * c00 + t0 * c01;
                            c1 = (1f - t0) * c10 + t0 * c11;
                        } else {
                            c0 = c00;
                            c1 = c10;
                        }

                        Color c;

                        if (v1 != v0) {
                            float t1 = (uv.y - (float)v0) / (float)(v1 - v0);
                            c = (1f - t1) * c0 + t1 * c1;
                        } else {
                            c = c0;
                        }

                        return c;
                    }
                default:
                    return Color.clear;
            }

            
        }
    }
}