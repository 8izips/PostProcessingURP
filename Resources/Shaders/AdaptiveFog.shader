Shader "Hidden/PostProcessing/AdaptiveFog"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	float4 _MainTex_TexelSize;

	float _BloomThreshold;
	float _BloomPower;
	float _BloomWidth;
	float4 _FogColor;
	float _MaxFogFactor;
	float _FogCurve;
	float _FogStart;

	#include "Common.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float4 color = _MainTex.Sample(sampler_MainTex, i.texcoord);
		float depth = LinearEyeDepth(SampleSceneDepth(i.texcoord), _ZBufferParams);

		float3 blurColor2 = 0;
		float3 blurtemp = 0;
		const float maxDistance = 8 * _BloomWidth;
		float curDistance = 0;
		const float sampleCount = 25.0;
		const float2 blurtempvalue = i.texcoord * _MainTex_TexelSize.xy * _BloomWidth;
		float2 bloomSample = float2(2.5, -2.5);
		float2 bloomSampleValue = float2(0, 0);

		for (bloomSample.x = (2.5); bloomSample.x > -2.0; bloomSample.x = bloomSample.x - 1.0)
		{
			bloomSampleValue.x = bloomSample.x * blurtempvalue.x;
			float2 distancetemp = bloomSample.x * bloomSample.x * _BloomWidth;

			for (bloomSample.y = (-2.5); bloomSample.y < 2.0; bloomSample.y = bloomSample.y + 1.0)
			{
				distancetemp.y = bloomSample.y * bloomSample.y;
				curDistance = (distancetemp.y * _BloomWidth) + distancetemp.x;
				bloomSampleValue.y = bloomSample.y * blurtempvalue.y;
				blurtemp = _MainTex.Sample(sampler_MainTex, i.texcoord + bloomSampleValue).rgb;
				blurColor2.rgb += lerp(blurtemp.rgb, color.rgb, sqrt(curDistance / maxDistance));
			}
		}
		blurColor2.rgb = (blurColor2.rgb / (sampleCount - (_BloomPower - _BloomThreshold * 5)));

		float bloomAmount = (dot(color.rgb, float3(0.299f, 0.587f, 0.114f)));
		float4 blurColor = float4(blurColor2.rgb * (_BloomPower + 4.0), 1.0);
		blurColor = saturate(lerp(color, blurColor, bloomAmount));

		float fogFactor = clamp(saturate(depth - _FogStart) * _FogCurve, 0.0, _MaxFogFactor);

		return saturate(lerp(color, lerp(blurColor, _FogColor, fogFactor), fogFactor));
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
