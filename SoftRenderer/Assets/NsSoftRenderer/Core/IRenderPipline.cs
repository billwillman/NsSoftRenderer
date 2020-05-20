using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace NsSoftRenderer {

    public enum RenderType {
        None,
        // 非透明
        Opaque,
        // 透明
        Transparent
    }

    // 剔除模式
    public enum CullMode {
        none,
        front,
        back
    }

    // ZTest操作
    public enum ZTestOp {
        // <=
        LessEqual = 0,
        // <
        Less,
        // ==
        Equal,
        // >
        Greate,
        // >=
        GreateEqual
    }

    // 渲染PASS模式
    public class RenderPassMode {
        // 是否写深度
        public bool ZWrite = true;
        // 剔除模式
        public CullMode Cull = CullMode.back;
        // ZTest模式
        public ZTestOp ZTest = ZTestOp.LessEqual;
        // 当前的VertexShader
        public VertexShader vertexShader = null;
        // 当前的PixelShader
        public int pixelShaderHandle = 0;
        // 暂时放这里
        public int mainTex = 0;

        public PixelShader pixelShader {
            get {
                if (pixelShaderHandle == 0)
                    return null;

                var device = SoftDevice.StaticDevice;
                if (device == null || device.ResMgr == null)
                    return null;
                return device.ResMgr.GetSoftRes<PixelShader>(pixelShaderHandle);
            }
        }

        public T CreateVertexShader<T>() where T : VertexShader, new() {
            T ret = new T();
            ret.m_Owner = this;
            vertexShader = ret;
            return ret;
        }

        public void AttachVertexShader(VertexShader vert) {
            if (vert != null) {
                vert.m_Owner = this;
            }
            vertexShader = vert;
        }

        public void AttachPixelShader(int pixel) {
            this.pixelShaderHandle = pixel;
        }

        public Matrix4x4 MVPMatrix = Matrix4x4.identity;
        // 参与相关光照
        public NativeList<int> LightHandles = null; // 光源列表

        public int LightCount {
            get {
                if (LightHandles == null)
                    return 0;
                return LightHandles.Count;
            }
        }

        public SoftLight GetLight(int index) {
            if (LightHandles == null)
                return null;
            int handle = LightHandles[index];
            if (handle == 0)
                return null;
            var device = SoftDevice.StaticDevice;
            if (device == null)
                return null;
            SoftLight ret = device.GetRenderObject(handle) as SoftLight;
            return ret;
        }
    }

    // 渲染Pass
    public abstract class IRenderPass {
        // 渲染模式
        public RenderPassMode PassMode;

        public virtual RenderType Type {
            get {
                return RenderType.None;
            }
        }

        // 渲染准备(里面可以排序)
      //  protected abstract void DoRenderPrepare();
      //  protected abstract void DoVertexShader();
      //  protected abstract void DoPixelShader();
    }

    public static class RenderQueue {
        public static readonly int Geometry = 2000;
    }

    // 渲染队列
    public abstract class IRenderQueue {
        
    }

    // 渲染管线
    public abstract class IRenderPipline {
        protected SortedDictionary<int, IRenderQueue> m_RenderQueue = null;
        private static RenderQueueSort m_RenderQueueSort = new RenderQueueSort();

        private class RenderQueueSort: IComparer<int> {
            public int Compare(int x, int y) {
                return (x - y);
            }
        }

        public virtual void AttachPixelShader(int pixel) { }

        internal virtual bool DoCameraRender(SoftCamera camera, Dictionary<int, SoftRenderObject> objMap) { return false; }

        internal bool RegisterRenderQueue(int renderQueue, IRenderQueue queue) {
            if (queue == null)
                return false;

            if (m_RenderQueue == null) {
                m_RenderQueue = new SortedDictionary<int, IRenderQueue>(m_RenderQueueSort);
            }
            m_RenderQueue[renderQueue] = queue;
            return true;
        }
    }
}
