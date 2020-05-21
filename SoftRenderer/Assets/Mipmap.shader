Shader "Unlit/Mipmap"
{
    Properties
    {
		 _MainTex("Texture", 2D) = "white" {}
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			uniform float4 _MipmapColor[12];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = fixed4(1.0, 1.0, 1.0, 1.0);
				
				float2 dx = ddx(i.uv) * _MainTex_TexelSize.zw;
				float2 dy = ddy(i.uv) * _MainTex_TexelSize.zw;
				float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));

				float lambda = log2(rho);
				int f = clamp(floor(lambda), 0, 11);
				int c = clamp(ceil(lambda), 0, 11);
				if (f == c)
				{
					col = _MipmapColor[f];
				}
				else
				{
					float t = 1.0 - ((lambda - (float)f) / ((float)c - (float)f));
					col = _MipmapColor[f] * t + (1.0 - t) * _MipmapColor[c];
				}

                return col;
            }
            ENDCG
        }
    }
}
