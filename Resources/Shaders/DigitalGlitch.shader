Shader "Hidden/PostProcessing/DigitalGlitch"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	Texture2D _NoiseTex;
	SamplerState sampler_NoiseTex;
	Texture2D _TrashTex;
	SamplerState sampler_TrashTex;

	float _Intensity;

	#include "Common.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float4 glitch = _NoiseTex.Sample(sampler_NoiseTex, i.texcoord);

		float thresh = 1.001 - _Intensity * 1.001;
		float w_d = step(thresh, pow(abs(glitch.z), 2.5));
		float w_f = step(thresh, pow(abs(glitch.w), 2.5));
		float w_c = step(thresh, pow(abs(glitch.z), 3.5));

		float2 uv = frac(i.texcoord + glitch.xy * w_d);
		float4 source = _MainTex.Sample(sampler_MainTex, uv);
		float4 trash = _TrashTex.Sample(sampler_TrashTex, uv);
		float3 color = lerp(source, trash, w_f).rgb;
		float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.5);
		color = lerp(color, neg, w_c);

		return float4(color, source.a);
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