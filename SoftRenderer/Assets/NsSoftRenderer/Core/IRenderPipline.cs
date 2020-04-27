using System;
using System.Collections.Generic;

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
        public ZTestOp ZTest = ZTestOp.Less;
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
        protected abstract void DoRenderPrepare();
        protected abstract void DoVertexShader();
        protected abstract void DoPixelShader();
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

        internal virtual void DoCameraRender(SoftCamera camera, Dictionary<int, SoftRenderObject> objMap) { }

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
