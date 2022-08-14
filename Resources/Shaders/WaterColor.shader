Shader "Hidden/PostProcessing/WaterColor"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	Texture2D _NoiseTex;
	SamplerState sampler_NoiseTex;
	float4 _EffectParams;
	float _Interval;
	int _Iteration;
	
	float4 _EdgeColor;
	float4 _FillColor;

	float edgeContrast;
	float blurWidth;
	float blurFrequency;
	float hueShift;
	float iterationRcp;

	#include "Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

	float2 Rotate90(float2 v)
	{
		return v.yx * float2(-1, 1);
	}

	// Texture sampling functions
	float3 SampleColor(float2 p)
	{
		return _MainTex.Sample(sampler_MainTex, p).rgb;
	}

	float SampleLuminance(float2 p)
	{
		return Luminance(SampleColor(p));
	}

	float3 SampleNoise(float2 p)
	{
		return _NoiseTex.Sample(sampler_NoiseTex, p).rgb;
	}

	float2 GetGradient(float2 p, float freq)
	{
		const float2 dx = float2(_Interval / 200, 0);
		float ldx = SampleLuminance(p + dx.xy) - SampleLuminance(p - dx.xy);
		float ldy = SampleLuminance(p + dx.yx) - SampleLuminance(p - dx.yx);
		float2 n = SampleNoise(p * 0.4 * freq).gb - 0.5;
		return float2(ldx, ldy) + n * 0.05;
	}

	// Edge / fill processing functions
	float ProcessEdge(inout float2 p, float stride)
	{
		float2 grad = GetGradient(p, 1);
		float edge = saturate(length(grad) * 10);
		float pattern = SampleNoise(p * 0.8).r;
		p += normalize(Rotate90(grad)) * stride;
		return pattern * edge;
	}

	float3 ProcessFill(inout float2 p, float stride)
	{
		float2 grad = GetGradient(p, blurFrequency);
		p += normalize(grad) * stride;
		float shift = SampleNoise(p * 0.1).r * 2;
		return SampleColor(p) * HsvToRgb(float3(shift, hueShift, 1));
	}

	float3 Process(float2 uv)
	{
		float2 p_e_n = uv;
		float2 p_e_p = uv;
		float2 p_c_n = uv;
		float2 p_c_p = uv;

		const float Stride = 0.04 * iterationRcp;

		float  acc_e = 0;
		float3 acc_c = 0;
		float  sum_e = 0;
		float  sum_c = 0;

		for (int i = 0; i < _Iteration; i++)
		{
			float w_e = 1.5 - i * iterationRcp;
			acc_e += ProcessEdge(p_e_n, -Stride) * w_e;
			acc_e += ProcessEdge(p_e_p, +Stride) * w_e;
			sum_e += w_e * 2;

			float w_c = 0.2 + i * iterationRcp;
			acc_c += ProcessFill(p_c_n, -Stride * blurWidth) * w_c;
			acc_c += ProcessFill(p_c_p, +Stride * blurWidth) * w_c * 0.3;
			sum_c += w_c * 1.3;
		}

		// Normalization and contrast
		acc_e /= sum_e;
		acc_c /= sum_c;		
		acc_e = saturate((acc_e - 0.5) * edgeContrast + 0.5);

		// Color blending
		float3 rgb_e = lerp(1, _EdgeColor.rgb, _EdgeColor.a * acc_e);
		float3 rgb_f = lerp(1, acc_c, _FillColor.a) * _FillColor.rgb;

		return rgb_e * rgb_f;
	}

	float4 Frag(v2f i) : SV_Target
	{
		edgeContrast = _EffectParams.x;
		blurWidth = _EffectParams.y;
		blurFrequency = _EffectParams.z;
		hueShift = _EffectParams.w;
		iterationRcp = 1.0 / _Iteration;

		return float4(Process(i.texcoord), 1);
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