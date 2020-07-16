Shader "SimplePBR"
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		[NoScaleOffset]_BumpTex("Normal Map",2D) = "bump"{}
		[NoScaleOffset]_Metallic("Metallic",2D) = "metallic"{}
		_Color("Color",Color) = (1,1,1,1)
		_BumpSacle("Bump Sacle",Range(-1,1)) = 1
		_Roughness("Roughness",Range(0,1)) = 0.1
		_AO("AO",Range(0,1)) = 0.1
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase"}
			LOD 100

			CGINCLUDE
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			float3 FresnelSchlick(float cosTheta,float3 F0)
			{
				return F0 + (1.0 - F0)*pow(1.0 - cosTheta,5.0);
			}

			float DistributionGGX(float3 N,float3 H,float roughness)
			{
				float a2 = roughness * roughness;
				a2 = a2 * a2;
				float NdotH = saturate(dot(N,H));
				float NdotH2 = NdotH * NdotH;

				float denom = (NdotH2*(a2 - 1.0) + 1.0);
				denom = UNITY_PI * denom*denom;
				return a2 / denom;
			}

			float3 GeometrySchlickGGX(float NdotV,float roughness)
			{
				float r = roughness + 1.0;
				float k = r * r / 8.0;

				float denom = NdotV * (1.0 - k) + k;
				return NdotV / denom;
			}

			float GeometrySmith(float3 N,float3 V,float3 L,float roughness)
			{
				float NdotV = saturate(dot(N,V));
				float NdotL = saturate(dot(N,L));
				float ggx1 = GeometrySchlickGGX(NdotV,roughness);
				float ggx2 = GeometrySchlickGGX(NdotL,roughness);

				return ggx1 * ggx2;
			}
			ENDCG

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal:NORMAL;
					float4 tangent:TANGENT;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float3 lightDir:TEXCOORD1;
					float3 viewDir:TEXCOORD2;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _BumpTex;
				float _BumpSacle;
				float _Roughness;
				sampler2D _Metallic;
				float _AO;
				fixed4 _Color;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					//用法线要用的，基本操作了
					TANGENT_SPACE_ROTATION;
					o.lightDir = mul(rotation,ObjSpaceLightDir(v.vertex).xyz);
					o.viewDir = mul(rotation,ObjSpaceViewDir(v.vertex).xyz);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{

					float4 packNormal = tex2D(_BumpTex,i.uv);
					float3 normal = UnpackNormal(packNormal);
					normal.xy *= _BumpSacle;
					normal.z = sqrt(1.0 - saturate(dot(normal.xy,normal.xy)));
					float3 lightDir = normalize(i.lightDir);
					float3 viewDir = normalize(i.viewDir);
					float3 halfDir = normalize(lightDir + viewDir);

					//把色彩转到线性空间，我不太确定Unity的光照颜色是线性的还是Gamma的......
					fixed4 lightColor = pow(_LightColor0,2.2);
					fixed4 color = pow(_Color,2.2);

					//fixed4 albedo = pow(tex2D(_MainTex, i.uv),2.2)*_LightColor0*_Color;
					fixed4 albedo = pow(tex2D(_MainTex, i.uv),2.2)*lightColor*color;
					float3 F0 = float3(0.04,0.04,0.04);

					float metallic = tex2D(_Metallic,i.uv).r;
					F0 = lerp(F0,albedo,metallic);
					float3 Lo = float3(0,0,0);

					float NDF = DistributionGGX(normal,halfDir,_Roughness);
					float G = GeometrySmith(normal,viewDir,lightDir,_Roughness);
					float3 F = FresnelSchlick(clamp(dot(halfDir,viewDir),0,1),F0);

					float3 nom = NDF * G*F;
					float3 denom = 4 * max(dot(normal,viewDir),0)*saturate(dot(normal,lightDir));
					float3 specular = nom / max(denom,0.001);//max避免denom为零

					float3 Ks = F;
					float3 Kd = 1 - Ks;
					Kd *= 1.0 - metallic;
					float NdotL = max(dot(normal,lightDir),0.0);
					Lo += (Kd*albedo / UNITY_PI + specular)*NdotL;//
					float3 ambient = 0.03*albedo*_AO;
					float4 finalColor = float4(ambient + Lo,1.0);

					//转回Gamma空间
					finalColor = finalColor / (finalColor + 0.1);
					finalColor = pow(finalColor,1.0 / 2.2);
					finalColor.a = 1;

					return finalColor;
				}

				ENDCG
			}
		}
}