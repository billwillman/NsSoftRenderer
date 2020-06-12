Shader "Unlit/Base"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Diffuse_FI("漫反射吸收参数", float) = 0.0 // 默认不吸收
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags {"LightMode" = "ForwardBase"}
			
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal: NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				// 世界顶点
				float3 worldVertex: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			fixed3 PointLight(float3 worldPos, float3 lightPos, fixed3 lightColor)
			{
				float3 v = worldPos - lightPos;
				float r2 = v.x * v.x + v.y * v.y + v.z * v.z;
				// 光源均匀发散
				fixed3 color = lightColor/(4 * 3.141592653) / r2;
				// 法线和方向的cos(未做)

				return color;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

				// 点光源
				half3 lightPos1 = half3(unity_4LightPosX0.x, unity_4LightPosY0.x, unity_4LightPosZ0.x);
				half3 lightPos2 = half3(unity_4LightPosX0.y, unity_4LightPosY0.y, unity_4LightPosZ0.y);
				half3 lightPos3 = half3(unity_4LightPosX0.z, unity_4LightPosY0.z, unity_4LightPosZ0.z);
				half3 lightPos4 = half3(unity_4LightPosX0.w, unity_4LightPosY0.w, unity_4LightPosZ0.w);

				fixed4 lightColor = fixed4(PointLight(i.worldVertex, lightPos1, unity_LightColor[0]), 1.0);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * lightColor;
                return col;
            }
            ENDCG
        }
    }
}
