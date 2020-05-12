
namespace NsSoftRenderer
{
    public enum SoftLightType {
        Direct = 0, // 平行光
        Point = 1   // 点光源
    }

    // 光源
    public class SoftLight: SoftRenderObject
    {
        // 默认：点光源
        private SoftLightType m_LightType = SoftLightType.Point;

        public SoftLight() : base() { }
    }
}