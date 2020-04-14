using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Utils;

namespace NsSoftRenderer {

    // 颜色Buffer
    public class ColorBuffer: NativeList<Color> { }

    // 32位深度
    public class Depth32Buffer : NativeList<int> { }

    // 16位深度
    public class Depth16Buffer : NativeList<short> { }
}
