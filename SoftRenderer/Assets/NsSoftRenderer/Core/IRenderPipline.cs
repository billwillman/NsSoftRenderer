﻿using System;
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
    public struct RenderPassMode {
        // 是否写深度
        public bool ZWrite;
        // 剔除模式
        public CullMode Cull;
        // ZTest模式
        public ZTestOp ZTest;
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

    // 渲染管线
    public class IRenderPipline {
        private List<IRenderPass> m_PassList = null;
    }
}