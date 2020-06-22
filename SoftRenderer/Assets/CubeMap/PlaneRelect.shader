

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/PlaneRelect"
{
    Properties
    {
        _MainTex ("Texture", CUBE) = "black" {}
		_CubeSize ("CubeSize", float) = 1.0
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
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
			
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				// 模型空间顶点
				float3 model_vertex: TEXCOORD2;
				float3 model_camPos: TEXCOORD3;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _CubeSize;

            v2f vert (appdata v)
            {
                v2f o;
				o.model_vertex = v.vertex.xyz/v.vertex.w;
				o.model_camPos = mul(unity_WorldToObject, _WorldSpaceCameraPos.xyz);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			bool CheckPlanePt(float3 dir, float3 normal, float d, out float3 pt)
			{
				return false;
			}

            fixed4 frag (v2f i) : SV_Target
            {
				float3 dir = i.model_camPos - i.model_vertex;
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
		
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
