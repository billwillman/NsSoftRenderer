

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/PlaneRelect"
{
    Properties
    {
        _MainTex ("Texture", CUBE) = "black" {}
		_CubeSize ("CubeSize", float) = 1.0
		_Delta("_Delta", float) = 0.00001
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

            samplerCUBE _MainTex;
            float4 _MainTex_ST;
			float _CubeSize;
			float _Delta;

            v2f vert (appdata v)
            {
                v2f o;
				// 这里不应该在模型空间而是应该在local viewSpace下，就是模型节点在世界里的局部坐标系空间，这里偷懒，所以这里必须保证模型的GameObject固定在0的原点
				o.model_vertex = v.vertex.xyz/v.vertex.w;
				o.model_camPos = mul(unity_WorldToObject, _WorldSpaceCameraPos.xyz);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			bool CheckPt(float3 pt)
			{
				float halfSize = _CubeSize / 2.0;
				return (pt.z >= 0 - _Delta) && (pt.z <= _CubeSize + _Delta) && (pt.x >= -halfSize - _Delta) && (pt.x <= halfSize + _Delta) && 
					(pt.y >= -halfSize - _Delta) && (pt.y <= halfSize + _Delta);
			}

			bool CheckPlanePt(float3 dir, float3 org, float3 pnlNormal, float3 pnlPt, out float3 pt)
			{
				float3 p = pnlPt - org;
				float div = (dir.x * pnlNormal.x + dir.y * pnlNormal.y + dir.z * pnlNormal.z);
				if (div == 0)
					return false;
				float t = (p.x * pnlNormal.x + p.y * pnlNormal.y + p.z * pnlNormal.z) / div;
				if (t <= 0)
					return false;
				pt = org + dir * t;
				return CheckPt(pt);
				//return true;
			}

			

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col;

				float3 org = i.model_camPos;
				float3 dir = i.model_vertex - org;

				float halfSize = _CubeSize / 2.0;
				float3 reflectCenter = float3(0, 0, halfSize);

				// left panel
				float3 pnlNormal = float3(1.0, 0, 0);
				float3 pnlPt = float3(-halfSize, 0, 0);
				float3 pt;
				bool isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, pt);
				if (isInPnl)
				{
					float3 reflectDir = normalize(pt - reflectCenter);
					col = texCUBE(_MainTex, reflectDir);
				}
				else
				{
					// back panel
					
					pnlNormal = float3(0, 0, -1);
					pnlPt = float3(-halfSize, 0, _CubeSize);
					isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, pt);
					if (isInPnl)
					{
						float3 reflectDir = normalize(pt - reflectCenter);
						col = texCUBE(_MainTex, reflectDir);
					}
					else
					{
						

						// right panel
						pnlNormal = float3(-1, 0, 0);
						pnlPt = float3(halfSize, 0, 0);
						isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, pt);
						if (isInPnl)
						{
							float3 reflectDir = normalize(pt - reflectCenter);
							col = texCUBE(_MainTex, reflectDir);
						}
						else
						{
							// top panel
							pnlNormal = float3(0, -1, 0);
							pnlPt = float3(0, halfSize, 0);
							isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, pt);
							if (isInPnl)
							{
								float3 reflectDir = normalize(pt - reflectCenter);
								col = texCUBE(_MainTex, reflectDir);
							}
							else
							{
								// bottom panel
								pnlNormal = float3(0, 1, 0);
								pnlPt = float3(0, -halfSize, 0);
								isInPnl = CheckPlanePt(dir, org, pnlNormal, pnlPt, pt);
								if (isInPnl)
								{
									float3 reflectDir = normalize(pt - reflectCenter);
									col = texCUBE(_MainTex, reflectDir);
								}
							}
						}

						
					}
					
				}
				
				if (!isInPnl)
					col = fixed4(0, 0, 0, 0);

		
				//col = fixed4(normalize(float3(0.5, 0.5, 0) + i.model_vertex), 0);

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
