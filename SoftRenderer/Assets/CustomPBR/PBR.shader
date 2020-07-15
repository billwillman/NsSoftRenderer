// 采用：金属工作流
Shader "Unlit/PBR"
{
	

    Properties
    {
		// Base Color
        _MainTex ("基础贴图", 2D) = "white" {}
		_BaseColor ("基础颜色", COLOR) = (1.0, 1.0, 1.0, 1.0)
	    //----- 后面可以考虑把 金属度，粗糙度 合并贴图
		
		[Toggle(Use_MetallicSmooth)] _UseMetallicSmooth("使用金属度平滑度贴图", Int) = 0
		// 金属度
		_MetallicMap("金属度贴图", 2D) = "black" {}
		// 粗糙度 1表示粗糙，0表示光滑
		//_RoughnessMap("粗糙度贴图", 2D) = "white" {}
		_Smoothness("平滑度", Range(0, 1)) = 0.5
		// 环境吸收
		_EnvMap("环境吸收", CUBE) = "None" {}

		// 法线贴图
		_NormalMap("法线贴图", 2D) = "None" {}

		// 高度贴图
		_HeightMap("高度贴图", 2D) = "None" {}

		// 次表面
	    subsurface("次表面值", Range(0, 1)) = 0
		// 各异向
		anisotropic("各异向值", Range(0, 1)) = 0
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
			#include "Lighting.cginc"
			#include "../CgIncludes/brdf.cginc"
		//	#include "AutoLight.cginc"

			#pragma shader_feature Use_MetallicSmooth

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				// 法线
				float3 normal: NORMAL;
				// 切线
				float4 tangent: TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
               // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				// 三个坐标系
				float4 NormalToWorld_1: TEXCOORD2;
				float4 NormalToWorld_2: TEXCOORD3;
				float4 NormalToWorld_3: TEXCOORD4;
            };

            sampler2D _MainTex;
			sampler2D _MetallicMap;
			sampler2D _NormalMap;

            float4 _MainTex_ST;
			fixed4 _BaseColor;
			float _Smoothness;
			half subsurface;
			half anisotropic;

			// 获得DiffuseColor 金属度metallic
			fixed4 DiffuseColor(fixed4 baseColor, float metallic)
			{
				fixed4 ret = (1.0 - metallic) * baseColor;
				return ret;
			}


			// 迪士尼 漫反射
			fixed3 Disny_Diffuse(fixed4 diffColor, half3 nl, half3 nv, half3 f0, half f90)
			{
				fixed3 ret = diffColor / 3.141592653 * F_Schlick(f0, f90, nl) * F_Schlick(f0, f90, nv);
				return ret;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 bTangent = cross(v.normal, v.tangent) * v.tangent.w;
				float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
				worldVertex /= worldVertex.w;

				float3x3 mat;
				mat[0] = float3(v.tangent.x, v.tangent.y, v.tangent.z);
				mat[1] = float3(bTangent.x, bTangent.y, bTangent.z);
				mat[2] = float3(v.normal.x, v.normal.y, v.normal.z);
				mat = mul(mat, unity_WorldToObject); // 切线空间到模型坐标空间再到世界空间
				

				// 给转置
				o.NormalToWorld_1 = float4(mat[0][0], mat[1][0], mat[2][0], worldVertex.x);
				o.NormalToWorld_2 = float4(mat[0][1], mat[1][1], mat[2][1], worldVertex.y);
				o.NormalToWorld_3 = float4(mat[0][2], mat[1][2], mat[2][2], worldVertex.z);

			

               // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			// 光源0的方向
			half3 Light0_Dir(half3 worldVector)
			{

			}

			// 平行光方向
			half3 WorldDirectLightDir()
			{
				half3 ret = normalize(_WorldSpaceLightPos0);
				return ret;
			}

			// 观察者方向
			half3 WorldViewDir(half3 worldVector)
			{
				half3 ret = normalize(_WorldSpaceCameraPos.xyz - worldVector);
				return ret;
			}

			half WorldHDir(half3 worldViewDir, half3 lightDir)
			{
				half3 ret = worldViewDir + lightDir;
				return ret;
			}

			// 计算漫反射(兰伯特模型)
			half3 CalcLightDiffuse_Lambert(half3 lightColor, half3 diffColor, half3 worldNomral, half3 lightDir)
			{
				half lcolor = max(0, dot(worldNomral, lightDir));
				half3 ret = lightColor * diffColor * lcolor;
				return ret;
			}

			// 法綫分佈函數
			half D_GGX(float nh, float roughness)
			{
				half a2 = roughness * roughness;
				half f = (nh * a2 - nh) * nh + 1.0;
				half ret = a2 / (3.141592653 * f * f);
				return ret;
			}

			// 几何遮蔽函数
			half G_GGX(float roughness, half nv, half nl)
			{
				half f = lerp(2 * nl * nv, nl + nv, roughness);
				half ret = 0.5 / f;
				return ret;
			}

			// 鏡面
			half3 Disney_Spec(half f0, half vh, half nh, half nv, half nl, half roughness)
			{
				// float F_Schlick(float f0, float f90, float cos)
				half3 f = F_Schlick(f0, 1.0, vh);

				half d = D_GGX(nh, roughness);
				half g = G_GGX(roughness, nv, nl);

				half3 ret = f * d * g / (4 * nv * nl);

				return ret;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                // 基础颜色
				fixed4 metallic = tex2D(_MetallicMap, i.uv);
				fixed4 baseColor = tex2D(_MainTex, i.uv);
				//baseColor.rgb = pow(baseColor.rgb, 2.2);

				half metal = metallic.r;
                fixed4 diffuseColor = DiffuseColor(baseColor, metal);
				

				half3 normal = UnpackNormal(tex2D(_NormalMap, i.uv));

			//	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));

				half3 worldNormal = normalize(half3(dot(i.NormalToWorld_1.xyz, normal), dot(i.NormalToWorld_2.xyz, normal), dot(i.NormalToWorld_3.xyz, normal)));
			//	half3 worldNormal = UnityObjectToWorldNormal(normal);
				half3 worldVector = half3(i.NormalToWorld_1.w, i.NormalToWorld_2.w, i.NormalToWorld_3.w);
				half3 worldTangent = half3(i.NormalToWorld_1.x, i.NormalToWorld_2.x, i.NormalToWorld_3.x);
				half3 worldbTagent = half3(i.NormalToWorld_1.y, i.NormalToWorld_2.y, i.NormalToWorld_3.y);

				half3 worldDirectLightDir = WorldDirectLightDir();
				half3 worldViewDir = WorldViewDir(worldVector);

				half smoothness = _Smoothness;
#ifdef Use_MetallicSmooth
				smoothness *= metallic.g;
#endif
				float3 f0 = lerp(unity_ColorSpaceDielectricSpec.rgb, baseColor, metal);

				half roughness = 1.0 - smoothness;

				fixed4 col;
				// 必须是normal因为计算是COSA,是角度值
				/*
				half3 h = normalize(WorldHDir(worldViewDir, worldDirectLightDir));
				half3 lh = saturate(normalize(dot(worldDirectLightDir, h)));
				half3 nl = saturate(normalize(dot(worldNormal, worldDirectLightDir)));
				half3 nv = saturate(normalize(dot(worldNormal, worldViewDir)));
				half3 vh = saturate(normalize(dot(h, worldViewDir)));
				half3 nh = saturate(normalize(dot(h, worldNormal)));


				half f90 = F90(roughness, lh);
				fixed3 diffuse = Disny_Diffuse(_LightColor0, nl, nv, 1.0 - f0, f90) * diffuseColor;

				fixed3 spec = Disney_Spec(f0, vh, nh, nv, nl, roughness) * diffuseColor;
				col = fixed4(diffuse, 1.0);

				col *= _BaseColor;
				*/
				/*
				col.rgb = f_cooktorrance(worldDirectLightDir, worldViewDir, worldNormal) * 
						g_cooktorrance(worldDirectLightDir, worldViewDir, worldNormal) *
						d_beckmann(worldDirectLightDir, worldViewDir, worldNormal) /
						(4.0 * dot(worldNormal, worldDirectLightDir) * dot(worldNormal, worldViewDir));
				col.a = 1.0;
				col *= baseColor * smoothness;
				*/
				col.rgb = Disney_Brdf(worldDirectLightDir, worldViewDir, worldNormal, baseColor, roughness, metal, worldTangent, worldbTagent, subsurface, anisotropic);
				//col.rgb = baseColor.rgb;
				//col = half4(f_cooktorrance( worldDirectLightDir, worldViewDir, worldNormal, 2.0), 1.0);

                return col;
            }
            ENDCG
        }
    }
}
