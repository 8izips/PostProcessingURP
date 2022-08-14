Shader "Hidden/PostProcessing/Grayscale"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	float _Blend;

	#include "Common.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float4 color = _MainTex.Sample(sampler_MainTex, i.texcoord);
		float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
		color.rgb = lerp(color.rgb, luminance.xxx, _Blend.xxx);
		return color;
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