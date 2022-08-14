﻿Shader "Hidden/PostProcessing/OilPaint"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	float4 _MainTex_TexelSize;

	float _Radius;

	#include "Common.hlsl"

	float4 Frag(v2f i) : SV_Target
	{
		float2 texcoord = i.texcoord;
		float3 mean[4] = {
			{ 0, 0, 0 },
			{ 0, 0, 0 },
			{ 0, 0, 0 },
			{ 0, 0, 0 }
		};

		float3 sigma[4] = {
			{ 0, 0, 0 },
			{ 0, 0, 0 },
			{ 0, 0, 0 },
			{ 0, 0, 0 }
		};

		float2 start[4] = { { -_Radius, -_Radius },{ -_Radius, 0 },{ 0, -_Radius },{ 0, 0 } };

		[unroll]
		for (int k = 0; k < 4; k++) {
			for (int i = 0; i <= _Radius; i++) {
				for (int j = 0; j <= _Radius; j++) {
					float2 pos = float2(i, j) + start[k];
					float3 col = _MainTex.Sample(sampler_MainTex, texcoord + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y)).rgb;
					mean[k] += col;
					sigma[k] += col * col;
				}
			}
		}

		float sigma2;

		float n = pow(_Radius + 1, 2);
		float4 color = _MainTex.Sample(sampler_MainTex, i.texcoord);
		float min = 1;

		for (int l = 0; l < 4; l++) {
			mean[l] /= n;
			sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
			sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;

			if (sigma2 < min) {
				min = sigma2;
				color.rgb = mean[l].rgb;
			}
		}
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
