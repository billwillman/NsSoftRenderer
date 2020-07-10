#ifndef _BRDF

#define _BRDF

const float PI = 3.14159265358979323846;

half Fresnel(half f0, half f90, half cos)
{
	return f0 + (f90 - f0) * pow(1 - cos, 5);
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
	return exp((T - 1) / (M*T)) / (PI*M*T*T);
}

half3 d_beckmann(half3 l, half3 v, half3 n, half m = 0.001)
{
	half3 h = normalize(l + v);
	half nh = dot(n, h);
	half d = Beckmann(m, nh);
	return half3(d, d, d);
}

#endif
