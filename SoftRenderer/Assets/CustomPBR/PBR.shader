Shader "Unlit/PBR"
{
    Properties
    {
        _BaseColor ("基本颜色", 2D) = "white" {}
		_Subsurface("次表面影响程度", float) = 0

		_Metallic("金属度", 2D) = "black" {}
		_Specular("高光度", 2D) = "black" {}
		_SpecularTint("高光影响程度", float) = 0
		_Roughness("粗糙度", 2D) = "black" {}
		
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
           // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
               // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _BaseColor;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
               // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_BaseColor, i.uv);
                // apply fog
               // UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
