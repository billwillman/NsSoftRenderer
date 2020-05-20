using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Utils;

namespace NsSoftRenderer {

    public class Buffer<T>: NativeList<T> where T: struct {
        private int m_Width;
        private int m_Height;

        public int Width {
            get {
                return m_Width;
            }
        }

        public int Height {
            get {
                return m_Height;
            }
        }

        public Buffer(int width, int height): base(width * height) {
            m_Width = width;
            m_Height = height;
            this.Count = width * height;
        }

        public T GetItem(int col, int row) {
            int idx = col + row * m_Width;
            T ret = this[idx];
            return ret;
        }

        public void SetItem(int col, int row, T item) {
            int idx = col + row * m_Width;
            this[idx] = item;
        }
    }

    // 颜色Buffer
    public class ColorBuffer: Buffer<Color32> {
        public ColorBuffer(int width, int height): base(width, height) { }
    }

    public class PixelBuffer : Buffer<PixelInfo> {
        // 这里有内存数据冗余，不太合理。先不管暂时这样，只是图形学效果
        private ColorBuffer m_ColorBuffer = null;

        public ColorBuffer colorBuffer {
            get {
                return m_ColorBuffer;
            }
        }

        public Vector4 GetUV1(int col, int row) {
            PixelInfo info = GetItem(col, row);
            return info.uv1;
        }

        public PixelBuffer(int width, int height) : base(width, height) {
            // 循环标注出索引
            PixelInfo info = new PixelInfo();
            for (int row = 0; row < height; ++row) {
                for (int col = 0; col < width; ++col) {
                    info.u = col;
                    info.v = row;
                    SetItem(col, row, info);
                }
            }

            m_ColorBuffer = new ColorBuffer(width, height);
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_ColorBuffer != null) {
                m_ColorBuffer.Dispose();
                m_ColorBuffer = null;
            }
        }


        public void SetPixel(int col, int row, Color color) {
            PixelInfo info = this.GetItem(col, row);
            info.color = color;
            info.uv1 = Vector4.zero;
            this.SetItem(col, row, info);
            m_ColorBuffer.SetItem(col, row, color);
        }
        public void SetPixel(int col, int row, Color color, Vector4 uv1, byte isFill = 1) {
            PixelInfo info = this.GetItem(col, row);
            info.color = color;
            info.uv1 = uv1;
            info.isFill = isFill;
            this.SetItem(col, row, info);
            m_ColorBuffer.SetItem(col, row, color);
        }
    }


    public class VertexColorBuffer: NativeList<Color> { }

    // 32位深度
    public class Depth32Buffer : Buffer<float> {
        public Depth32Buffer(int width, int height) : base(width, height) { }
    }

    // 16位深度
    public class Depth16Buffer : Buffer<short> {
        public Depth16Buffer(int width, int height) : base(width, height) { }
    }

    // 三角形Buffer
    public class TriangleList: NativeList<TriangleVertex> {
    }

    // 索引缓冲
    public class IndexBuffer: NativeList<int> {

    }

    // 顶点缓冲
    public class VertexBuffer: NativeList<Vector3> {

    }

    // 法线缓冲
    public class VertexNormalBuffer: NativeList<Vector3> {

    }

    // UV坐标
    public class UVBuffer: NativeList<Vector4> {

    }
}
