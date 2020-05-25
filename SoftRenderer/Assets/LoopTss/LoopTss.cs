using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace TssLoop {

    // Loop细分算法
    public class LoopTss : MonoBehaviour {

        // 暂定只支持4次细分
        [Range(0, 3)]
        public int TssLevel = 0;
        // 运行时的细分级别
        private int m_RunTssLevel = 0;

        // 开始处理
        void Process() {
            if (m_RunTssLevel == TssLevel)
                return;

        }
    }
}
