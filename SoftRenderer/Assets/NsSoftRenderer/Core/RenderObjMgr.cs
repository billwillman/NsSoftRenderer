using System;
using System.Collections;
using System.Collections.Generic;
using Utils;

namespace NsSoftRenderer {

    // 渲染对象管理器(Mesh单位)，用于做摄影机裁剪用处以及排序
    internal class RenderObjMgr: DisposeObject {
        // 在视野范围的
        private NativeList<int> m_Objs = new NativeList<int>();

        protected override void OnFree(bool isManual) {
            if (m_Objs != null) {
                m_Objs.Dispose();
                m_Objs = null;
            }
        }

        // 摄影机剔除
        // visiableList: 可见列表
        public void CameraCull(SoftCamera camera, Dictionary<int, SoftRenderObject> objs, out NativeList<int> visiableList) {
            visiableList = null;
            if (camera == null)
                return;
            if (objs == null || objs.Count <= 0) {
                m_Objs.Clear(false);
                return;
            }

            m_Objs.Clear(false);
            // 简单处理，直接干
            var iter = objs.GetEnumerator();
            while (iter.MoveNext()) {
                SoftRenderObject obj = iter.Current.Value;
                if (obj != null && obj.CanRenderer) {
                    switch (obj.ObjType) {
                        case SoftRenderObjType.MeshRender:
                            SoftSpere spere = (obj as SoftMeshRenderer).WorldBoundSpere;
                            // if (!camera.IsOpenCameraSpereCull || SoftMath.BoundSpereInCamera(spere, camera)) {
                            if (!camera.IsOpenCameraSpereCull || SoftMath.BoundSpereInCamera_UseMVP(spere, camera)) {
                                m_Objs.Add(obj.InstanceId);
                            }
                            break;
                    }
                }
            }
            iter.Dispose();
            if (m_Objs.Count > 0)
                visiableList = m_Objs;
        }
    }
}
