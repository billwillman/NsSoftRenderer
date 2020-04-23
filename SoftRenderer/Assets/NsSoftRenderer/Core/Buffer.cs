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
}
