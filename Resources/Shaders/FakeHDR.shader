Shader "Hidden/PostProcessing/FakeHDR"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	float4 _MainTex_TexelSize;
	float _HDRPower;
	float _Radius1;
	float _Radius2;

	#include "Common.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float3 color = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb;

		float3 bloom_sum1 = _MainTex.Sample(sampler_MainTex, i.texcoord + float2(1.5, -1.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-1.5, -1.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(1.5, 1.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-1.5, 1.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(0.0, -2.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(0.0, 2.5) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-2.5, 0.0) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(2.5, 0.0) * _Radius1 * _MainTex_TexelSize.xy).rgb;
		bloom_sum1 *= 0.005;

		float3 bloom_sum2 = _MainTex.Sample(sampler_MainTex, i.texcoord + float2(1.5, -1.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-1.5, -1.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(1.5, 1.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-1.5, 1.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(0.0, -2.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(0.0, 2.5) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(-2.5, 0.0) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 += _MainTex.Sample(sampler_MainTex, i.texcoord + float2(2.5, 0.0) * _Radius2 * _MainTex_TexelSize.xy).rgb;
		bloom_sum2 *= 0.010;

		float dist = _Radius2 - _Radius1;
		float3 hdr = (color + (bloom_sum2 - bloom_sum1)) * dist;
		float3 blend = hdr + color;
		color = pow(abs(blend), abs(_HDRPower)) + hdr;

		return float4(color, 1.0);
	}
	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			ENDHLSL
		}
	}
}
