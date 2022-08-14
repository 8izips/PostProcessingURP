#ifndef POSTPROCESSURP_COMMON_INCLUDED
#define POSTPROCESSURP_COMMON_INCLUDED

struct appdata
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2f
{
	float2 texcoord : TEXCOORD0;
	float4 vertex : SV_POSITION;
};

v2f Vert(appdata v)
{
	v2f o;
	o.vertex = float4((v.vertex.xy - 0.5) * 2, 0.0, 1.0);
	o.texcoord = v.vertex.xy;

	return o;
}

#endif