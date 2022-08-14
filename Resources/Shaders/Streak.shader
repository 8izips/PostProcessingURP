Shader "Hidden/PostProcessing/Streak"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	Texture2D _HighTex;
	SamplerState sampler_HighTex;

	float4 _MainTex_TexelSize;
	float _Threshold;
	float _Stretch;
	float _Intensity;
	half3 _Color;

	#include "Common.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	// Prefilter: Shrink horizontally and apply threshold.
	half4 FragPrefilter(v2f i) : SV_Target
	{
		// Actually this should be 1, but we assume you need more blur...
		const float vscale = 1.5;
		const float dy = _MainTex_TexelSize.y * vscale / 2;

		float2 uv = i.texcoord;
		half3 c0 = _MainTex.Sample(sampler_MainTex, float2(uv.x, uv.y - dy)).rgb;
		half3 c1 = _MainTex.Sample(sampler_MainTex, float2(uv.x, uv.y + dy)).rgb;
		half3 c = (c0 + c1) / 2;

		float br = max(c.r, max(c.g, c.b));
		c *= max(0, br - _Threshold) / max(br, 1e-5);

		return half4(c, 1);
	}

	// Downsampler
	half4 FragDownsample(v2f i) : SV_Target
	{
		// Actually this should be 1, but we assume you need more blur...
		const float hscale = 1.25;
		const float dx = _MainTex_TexelSize.x * hscale;

		float2 uv = i.texcoord;
		float u0 = uv.x - dx * 5;
		float u1 = uv.x - dx * 3;
		float u2 = uv.x - dx * 1;
		float u3 = uv.x + dx * 1;
		float u4 = uv.x + dx * 3;
		float u5 = uv.x + dx * 5;

		half3 c0 = _MainTex.Sample(sampler_MainTex, float2(u0, uv.y)).rgb;
		half3 c1 = _MainTex.Sample(sampler_MainTex, float2(u1, uv.y)).rgb;
		half3 c2 = _MainTex.Sample(sampler_MainTex, float2(u2, uv.y)).rgb;
		half3 c3 = _MainTex.Sample(sampler_MainTex, float2(u3, uv.y)).rgb;
		half3 c4 = _MainTex.Sample(sampler_MainTex, float2(u4, uv.y)).rgb;
		half3 c5 = _MainTex.Sample(sampler_MainTex, float2(u5, uv.y)).rgb;

		// Simple box filter
		half3 c = (c0 + c1 + c2 + c3 + c4 + c5) / 6;

		return half4(c, 1);
	}

	// Upsampler
	half4 FragUpsample(v2f i) : SV_Target
	{
		half3 c0 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 4;
		half3 c1 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 2;
		half3 c2 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 4;
		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		return half4(lerp(c3, c0 + c1 + c2, _Stretch), 1);
	}

	// Final composition
	half4 FragComposition(v2f i) : SV_Target
	{
		half3 c0 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 4;
		half3 c1 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 2;
		half3 c2 = _MainTex.Sample(sampler_MainTex, i.texcoord).rgb / 4;
		half3 c3 = _HighTex.Sample(sampler_HighTex, i.texcoord).rgb;
		half3 cf = (c0 + c1 + c2) * _Color * _Intensity * 5;
		return half4(cf + c3, 1);
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragPrefilter
			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragDownsample
			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragUpsample
			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment FragComposition
			ENDHLSL
		}
	}
}