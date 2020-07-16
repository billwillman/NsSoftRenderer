#ifndef _BRDF

#define _BRDF


half Fresnel(half f0, half f90, half cos)
{
	return f0 + (f90 - f0) * pow(1 - cos, 5);
}

float SchlickFresnel(float u)
{
	float m = clamp(1 - u, 0, 1);
	float m2 = m * m;
	return m2 * m2*m; // pow(m,5)
}

// Schlick简化公式，菲涅尔中的F
// 原来简化公式里f90项是1，这里漫反射有一个修正值f90，高光模型这个f90就是1
float3 F_Schlick(float3 f0, float f90, float cos)
{
	float3 ret = f0 + (float3(f90, f90, f90) - f0) * pow((1.0 - cos), 5.0);
	return ret;
}

half Fresnel_Ior(half cos, half ior)
{
	half g = sqrt(ior * ior + cos * cos - 1);
	return 0.5 * pow(g - cos, 2.0) / pow(g + cos, 2.0) * (1.0 + pow(cos * (g + cos) - 1, 2) / pow(cos * (g - cos) + 1, 2.0));
}

half3 f_schlick_f0(half f0, half3 l, half3 v, half3 n)
{
	half3 h = normalize(l + v);
	half ret = Fresnel(f0, 1.0, dot(h, l));
	return half3(ret, ret, ret);
}

// 当标准曲面着色器中的 IOR 大于 1 时，物体的反射将取决于视角，并依照菲涅尔方程发生变化。
// 标准曲面着色器使用 Schlick 的菲涅尔方程近似法，并可使用材质的 IOR 来控制。
half3 f_schlick_ior(half3 l, half3 v, half3 n, half ior)
{
	half3 h = normalize(l + v);
	half f0 = pow((ior - 1) / (ior + 1), 2);
	half  ret = Fresnel(f0, 1.0, dot(l, h));
	return half3(ret, ret, ret);
}

half3 f_cooktorrance(half3 l, half3 v, half3 n, half ior = 3)
{
	half3 h = normalize(l + v);
	half ret = Fresnel_Ior(dot(l, h), ior);
	return half3(ret, ret, ret);
}

half3 g_cooktorrance(half3 l, half3 v, half3 n, bool includeInvNdotLNdotV = false)
{
	half3 h = normalize(l + v);
	half nh = dot(n, h);
	half vh = dot(v, h);
	half nl = dot(n, l);
	half nv = dot(n, v);

	half g = min(1.0, min(2 * nh * nv / vh, 2 * nh * nl / vh));
	if (includeInvNdotLNdotV)
		g *= 1 / (nl * nv);
	return half3(g, g, g);
}

float Beckmann(float m, float t)
{
	float M = m * m;
	float T = t * t;
	return exp((T - 1) / (M*T)) / (3.14159265358979323846 * M*T*T);
}

half3 d_beckmann(half3 l, half3 v, half3 n, half m = 0.001)
{
	half3 h = normalize(l + v);
	half nh = dot(n, h);
	half d = Beckmann(m, nh);
	return half3(d, d, d);
}

// GAMMA转线性
half3 mon2lin(half3 x)
{
	half3 ret = pow(x, 2.2);
	return ret;
}

half F90(half roughness, half lh)
{
	half ret = 0.5 + 2.0 * roughness * lh * lh;
	return ret;
}

half GTR1(half NdotH, half a)
{
	if (a >= 1) return 1 / 3.14159265358979323846;
	half a2 = a * a;
	half t = 1 + (a2 - 1)*NdotH*NdotH;
	return (a2 - 1) / (3.14159265358979323846*log(a2)*t);
}

half GTR2_aniso(half NdotH, half HdotX, half HdotY, float ax, float ay)
{
	half s1 = HdotX / ax;
	half s11 = s1 * s1;
	half s2 = HdotY / ay;
	half s22 = s2 * s2;
	half s3 = s11 + s22 + NdotH * NdotH;
	half s33 = s3 * s3;
	return 1.0 / (3.14159265358979323846 * ax*ay * s33);
}


//-----------------------------------------

half D_GGX(half Roughness, half NoH)
{
	half a = Roughness * Roughness;
	half a2 = a * a;
	half d = (NoH * a2 - NoH) * NoH + 1;	// 2 mad
	return a2 / (3.14159265358979323846*d*d);					// 4 mul, 1 rcp
}

half D_Beckmann(half Roughness, half NoH)
{
	half a = Roughness * Roughness;
	half a2 = a * a;
	half NoH2 = NoH * NoH;
	return exp((NoH2 - 1) / (a2 * NoH2)) / (3.14159265358979323846 * a2 * NoH2 * NoH2);
}

half D_GGXaniso(half RoughnessX, half RoughnessY, half NoH, half3 H, half3 X, half3 Y)
{
	float ax = RoughnessX * RoughnessX;
	float ay = RoughnessY * RoughnessY;
	float XoH = dot(X, H);
	float YoH = dot(Y, H);
	float d = XoH * XoH / (ax*ax) + YoH * YoH / (ay*ay) + NoH * NoH;
	return 1 / (3.14159265358979323846 * ax*ay * d*d);
}

// 几何遮蔽函数

//Compact metallic reflectance models
half Vis_Neumann(half NoV, half NoL)
{
	return 1 / (4 * max(NoL, NoV));
}

half3 Diffuse_Lambert(half3 baseColor, half metallic)
{
	
	return baseColor * (1 / 3.14159265358979323846) * (1.0 - metallic);
}

half3 Diffuse_Burley(half3 lh, half3 nl, half3 nv, half3 baseColor, half roughness, half metallic)
{
	half f90 = F90(roughness, lh);
	half lightScatter = F_Schlick(nl, 1.0, f90);
	half viewScatter = F_Schlick(nv, 1.0, f90);
	half diffuse = 1.0 / 3.14159265358979323846 * lightScatter * viewScatter;
	half ret = diffuse * (1.0 - metallic) * baseColor;
	return ret;
}

half3 Diffuse_OrenNayar(half3 baseColor, half Roughness, half NoV, half NoL, half VoH, half metallic)
{
	half a = Roughness * Roughness;
	half s = a;// / ( 1.29 + 0.5 * a );
	half s2 = s * s;
	half VoL = 2 * VoH * VoH - 1;		// double angle identity
	half Cosri = VoL - NoV * NoL;
	half C1 = 1 - 0.5 * s2 / (s2 + 0.33);
	half C2 = 0.45 * s2 / (s2 + 0.09) * Cosri * (Cosri >= 0 ? rcp(max(NoL, NoV)) : 1); // rcp為计算每个分量的快速近似倒数。
	half3 ret = baseColor / 3.14159265358979323846 * (C1 + C2) * (1 + Roughness * 0.5) * (1.0 - metallic);
	return ret;
}

half3 Diffuse_Gotanda(half3 baseColor, half Roughness, half NoV, half NoL, half VoH, half metallic)
{
	half a = Roughness * Roughness;
	half a2 = a * a;
	half F0 = 0.04;
	half VoL = 2 * VoH * VoH - 1;		// double angle identity
	half Cosri = VoL - NoV * NoL;
#if 1
	half a2_13 = a2 + 1.36053;
	half Fr = (1 - (0.542026*a2 + 0.303573*a) / a2_13) * (1 - pow(1 - NoV, 5 - 4 * a2) / a2_13) * ((-0.733996*a2*a + 1.50912*a2 - 1.16402*a) * pow(1 - NoV, 1 + rcp(39 * a2*a2 + 1)) + 1);
	//float Fr = ( 1 - 0.36 * a ) * ( 1 - pow( 1 - NoV, 5 - 4*a2 ) / a2_13 ) * ( -2.5 * Roughness * ( 1 - NoV ) + 1 );
	half Lm = (max(1 - 2 * a, 0) * (1 - Pow5(1 - NoL)) + min(2 * a, 1)) * (1 - 0.5*a * (NoL - 1)) * NoL;
	half Vd = (a2 / ((a2 + 0.09) * (1.31072 + 0.995584 * NoV))) * (1 - pow(1 - NoL, (1 - 0.3726732 * NoV * NoV) / (0.188566 + 0.38841 * NoV)));
	half Bp = Cosri < 0 ? 1.4 * NoV * NoL * Cosri : Cosri;
	half Lr = (21.0 / 20.0) * (1 - F0) * (Fr * Lm + Vd + Bp);
	half3 ret = baseColor / 3.14159265358979323846 * Lr * (1 - metallic);
	return ret;
#else
	half a2_13 = a2 + 1.36053;
	half Fr = (1 - (0.542026*a2 + 0.303573*a) / a2_13) * (1 - pow(1 - NoV, 5 - 4 * a2) / a2_13) * ((-0.733996*a2*a + 1.50912*a2 - 1.16402*a) * pow(1 - NoV, 1 + rcp(39 * a2*a2 + 1)) + 1);
	half Lm = (max(1 - 2 * a, 0) * (1 - Pow5(1 - NoL)) + min(2 * a, 1)) * (1 - 0.5*a + 0.5*a * NoL);
	half Vd = (a2 / ((a2 + 0.09) * (1.31072 + 0.995584 * NoV))) * (1 - pow(1 - NoL, (1 - 0.3726732 * NoV * NoV) / (0.188566 + 0.38841 * NoV)));
	half Bp = Cosri < 0 ? 1.4 * NoV * Cosri : Cosri / max(NoL, 1e-8);
	half Lr = (21.0 / 20.0) * (1 - F0) * (Fr * Lm + Vd + Bp);
	half3 ret = DiffuseColor / 3.14159265358979323846 * Lr * (1 - metallic);
	return ret;
#endif
}

half3 UE4_Brdf(half3 l, half3 v, half3 n, half3 baseColor, half roughness, half metallic, half f0 = 0.99)
{
	
	half nl = dot(n, l);
	half nv = dot(n, v);
	if (nl < 0 || nv < 0)
		return half3(0, 0, 0);
	half3 h = normalize(l + v);
	half lh = dot(l, h);
	half vh = dot(v, h);
	half nh = dot(n, h);

	baseColor = mon2lin(baseColor);

	half  oneMinusReflectivity = unity_ColorSpaceDielectricSpec.a - metallic * unity_ColorSpaceDielectricSpec.a;
	half3 diffColor = baseColor * oneMinusReflectivity;

//	half3 diffuse = Diffuse_Gotanda(baseColor, roughness, nv, nl, vh, metallic);
//	half3 diffuse = Diffuse_OrenNayar(baseColor, roughness, nv, nl, vh, metallic);
	half3 diffuse = Diffuse_Burley(lh, nl, nv, diffColor, roughness, metallic);
//	half3 diffuse = Diffuse_Lambert(baseColor, metallic);
	
	//half3 sepcColor = (baseColor + half3(0.56, 0.56, 0.56)) * (metallic);
	//half3 sepcColor = half3(0.56, 0.56, 0.56);
	// 鏡面部分 / (4.0 * nl * nv) * nl * metallic
	half3 specColor = lerp(unity_ColorSpaceDielectricSpec.rgb, baseColor, metallic);
	half3 spec = D_GGX(nh, roughness) * F_Schlick(f0, 1.0, vh) * Vis_Neumann(nv, nl)  * specColor;

	//-----------------


	half3 ret = diffuse + spec;
	return ret;
}

//----------------------------------------------

// subsurface是次表面项
/*
half3 Disney_Brdf(half3 l, half3 v, half3 n, half3 baseColor, half roughness, half metallic, 
	half3 x, half3 y,
	half subsurface, 
	half anisotropic)
{
	half nl = dot(n, l);
	half nv = dot(n, v);
	if (nl < 0 || nv < 0)
		return half3(0, 0, 0);

	half3 h = normalize(l + v);
	half nh = dot(n, h);
	half lh = dot(l, h);

	half3 cdlin = mon2lin(baseColor);
	//half3 cdlin = baseColor;

	// 亮度公式是 Brightness = 0.3 * R + 0.6 * G + 0.1 * B，
	//half cdlum = 0.3 * cdlin.r + 0.6 * cdlin.g + 0.1 * cdlin.b;

    half PI = 3.14159265358979323846;

	half f90 = F90(roughness, lh);
	//half fd = F_Schlick(1.0, f90, nl) * F_Schlick(1.0, f90, nv);
	half fl = SchlickFresnel(nl);
	half fv = SchlickFresnel(nv);
	half fd = lerp(1.0, f90, fl) * lerp(1.0, f90, fv);

	// Based on Hanrahan-Krueger brdf approximation of isotropic bssrdf
	half fss90 = lh * lh * roughness;
	half fss = lerp(.0, fss90, fl) * lerp(1.0, fss90, fv);
	half ss = 1.25 * (fss * (1.0 / (nl + nv) - 0.5) + 0.5);

	// 镜面
	half aspect = sqrt(1.0 - anisotropic * 0.9);
	half ax = max(0.001, (roughness * roughness) / aspect);
	half ay = max(0.001, (roughness * roughness) * aspect);
	half ds = GTR2_aniso(nh, dot(l, x), dot(h, y), ax, ay);
	half fh = SchlickFresnel(lh);
	half3 fs = lerp(Cspec0, vec3(1), fh);

	half3 diffuse = 1.0 / PI * lerp(fd, ss, subsurface) * cdlin * (1.0 - metallic);

	// 普通镜面

	half3 ret = diffuse;
	return ret;
}*/

#endif
