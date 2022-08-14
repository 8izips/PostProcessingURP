Shader "Hidden/PostProcessing/RadialBlur"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	
	float _BlurRadius;
	float _Iteration;
	float _RadialCenterX;
	float _RadialCenterY;

	#include "Common.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float2 center = float2(_RadialCenterX, _RadialCenterY);
		float2 blurVector = (center - i.texcoord.xy) * _BlurRadius;
		float4 stackedColor = float4(0, 0, 0, 0);

		[unroll(30)]
		for (int c = 0; c < _Iteration; c++)
		{
			stackedColor += _MainTex.Sample(sampler_MainTex, i.texcoord);
			i.texcoord.xy += blurVector;
		}

		return stackedColor / _Iteration;
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