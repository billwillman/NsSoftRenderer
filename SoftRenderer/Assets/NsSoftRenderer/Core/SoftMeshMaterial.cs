using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace NsSoftRenderer {

    // 材质系统
    public class SoftMaterial: SoftRes {
        // 主贴图句柄
        public int _MainTex = 0;

        public int uuid {
            get;
            set;
        }

        public virtual void Dispose() { }
    }

    // 材质管理器
    public class SoftResourceManager: DisposeObject {
        private static int m_GlobalResInstanceId = 0;

        protected static int GenInstanceId() {
            return ++m_GlobalResInstanceId;
        }

        private Dictionary<int, SoftRes> m_ResMap = null;

        // 从贴图加载, 获得句柄
        public int LoadFromTexture2D(Texture2D tex) {
            if (tex == null)
                return 0;

            // 不考虑性能问题，直接转COLOR
            Color32[] colors = tex.GetPixels32();

            SoftTexture2D softTex = new SoftTexture2D(tex.width, tex.height);
            for (int i = 0; i < colors.Length; ++i) {
                softTex[i] = colors[i];
            }

            AddRes(softTex);

            return softTex.uuid;
        }

        public int CreatePixelShader<T>() where T: PixelShader, new() {
            T ret = new T();
            AddRes(ret);
            return ret.uuid;
        }

        protected void AddRes(SoftRes res) {
            if (res == null)
                return;
            res.uuid = GenInstanceId();
            if (m_ResMap == null)
                m_ResMap = new Dictionary<int, SoftRes>();
            m_ResMap.Add(res.uuid, res);
        }

        protected SoftRes GetSoftRes(int resId) {
            if (m_ResMap == null)
                return null;
            SoftRes res;
            if (!m_ResMap.TryGetValue(resId, out res))
                return null;
            return res;
        }

        public T GetSoftRes<T>(int resId) where T: class, SoftRes {
            T ret = GetSoftRes(resId) as T;
            return ret;
        }

        protected override void OnFree(bool isManual) {
            base.OnFree(isManual);

            if (m_ResMap != null) {
                var iter = m_ResMap.GetEnumerator();
                while (iter.MoveNext()) {
                    if (iter.Current.Value != null)
                        iter.Current.Value.Dispose();
                }
                iter.Dispose();
                m_ResMap.Clear();
            }
        }
    }
}
