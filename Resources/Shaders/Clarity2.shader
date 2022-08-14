Shader "Hidden/PostProcessing/Clarity2"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	Texture2D _Temp1Tex;
	SamplerState sampler_Temp1Tex;
	Texture2D _Temp2Tex;
	SamplerState sampler_Temp2Tex;
	
	float4 _MainTex_TexelSize;

	int _Offset;
	int _BlendIfDark;
	int _BlendIfLight;
	float _BlendIfRange;
	float _Strength;
	float _MaskContrast;
	float _DarkIntensity;
	float _LightIntensity;
	float _DitherStrength;

	#define Luma float3(0.32786885,0.655737705,0.0163934436)

	//Offset Values
	#define Offset02y _MainTex_TexelSize.y*1.1824255238
	#define Offset03y _MainTex_TexelSize.y*3.0293122308
	#define Offset04y _MainTex_TexelSize.y*5.0040701377

	#define Offset02x _MainTex_TexelSize.x*1.1824255238
	#define Offset03x _MainTex_TexelSize.x*3.0293122308
	#define Offset04x _MainTex_TexelSize.x*5.0040701377

	#define OffsetA2y _MainTex_TexelSize.y*1.4584295168
	#define OffsetA3y _MainTex_TexelSize.y*3.40398480678
	#define OffsetA4y _MainTex_TexelSize.y*5.3518057801
	#define OffsetA5y _MainTex_TexelSize.y*7.302940716
	#define OffsetA6y _MainTex_TexelSize.y*9.2581597095

	#define OffsetA2x _MainTex_TexelSize.x*1.4584295168
	#define OffsetA3x _MainTex_TexelSize.x*3.40398480678
	#define OffsetA4x _MainTex_TexelSize.x*5.3518057801
	#define OffsetA5x _MainTex_TexelSize.x*7.302940716
	#define OffsetA6x _MainTex_TexelSize.x*9.2581597095

	#define OffsetB2y 1.4895848401*_MainTex_TexelSize.y
	#define OffsetB3y 3.4757135714*_MainTex_TexelSize.y
	#define OffsetB4y 5.4618796741*_MainTex_TexelSize.y
	#define OffsetB5y 7.4481042327*_MainTex_TexelSize.y
	#define OffsetB6y 9.4344079746*_MainTex_TexelSize.y
	#define OffsetB7y 11.420811147*_MainTex_TexelSize.y
	#define OffsetB8y 13.4073334*_MainTex_TexelSize.y
	#define OffsetB9y 15.3939936778*_MainTex_TexelSize.y
	#define OffsetB10y 17.3808101174*_MainTex_TexelSize.y
	#define OffsetB11y 19.3677999584*_MainTex_TexelSize.y

	#define OffsetB2x 1.4895848401*_MainTex_TexelSize.x
	#define OffsetB3x 3.4757135714*_MainTex_TexelSize.x
	#define OffsetB4x 5.4618796741*_MainTex_TexelSize.x
	#define OffsetB5x 7.4481042327*_MainTex_TexelSize.x
	#define OffsetB6x 9.4344079746*_MainTex_TexelSize.x
	#define OffsetB7x 11.420811147*_MainTex_TexelSize.x
	#define OffsetB8x 13.4073334*_MainTex_TexelSize.x
	#define OffsetB9x 15.3939936778*_MainTex_TexelSize.x
	#define OffsetB10x 17.3808101174*_MainTex_TexelSize.x
	#define OffsetB11x 19.3677999584*_MainTex_TexelSize.x

	#define OffsetC2y _MainTex_TexelSize.y*1.4953705027
	#define OffsetC3y _MainTex_TexelSize.y*3.4891992113
	#define OffsetC4y _MainTex_TexelSize.y*5.4830312105
	#define OffsetC5y _MainTex_TexelSize.y*7.4768683759
	#define OffsetC6y _MainTex_TexelSize.y*9.4707125766
	#define OffsetC7y _MainTex_TexelSize.y*11.4645656736
	#define OffsetC8y _MainTex_TexelSize.y*13.4584295168
	#define OffsetC9y _MainTex_TexelSize.y*15.4523059431
	#define OffsetC10y _MainTex_TexelSize.y*17.4461967743
	#define OffsetC11y _MainTex_TexelSize.y*19.4401038149
	#define OffsetC12y _MainTex_TexelSize.y*21.43402885
	#define OffsetC13y _MainTex_TexelSize.y*23.4279736431
	#define OffsetC14y _MainTex_TexelSize.y*25.4219399344
	#define OffsetC15y _MainTex_TexelSize.y*27.4159294386

	#define OffsetC2x _MainTex_TexelSize.x*1.4953705027
	#define OffsetC3x _MainTex_TexelSize.x*3.4891992113
	#define OffsetC4x _MainTex_TexelSize.x*5.4830312105
	#define OffsetC5x _MainTex_TexelSize.x*7.4768683759
	#define OffsetC6x _MainTex_TexelSize.x*9.4707125766
	#define OffsetC7x _MainTex_TexelSize.x*11.4645656736
	#define OffsetC8x _MainTex_TexelSize.x*13.4584295168
	#define OffsetC9x _MainTex_TexelSize.x*15.4523059431
	#define OffsetC10x _MainTex_TexelSize.x*17.4461967743
	#define OffsetC11x _MainTex_TexelSize.x*19.4401038149
	#define OffsetC12x _MainTex_TexelSize.x*21.43402885
	#define OffsetC13x _MainTex_TexelSize.x*23.4279736431
	#define OffsetC14x _MainTex_TexelSize.x*25.4219399344
	#define OffsetC15x _MainTex_TexelSize.x*27.4159294386

	#define OffsetD2y _MainTex_TexelSize.y*1.4953705027
	#define OffsetD3y _MainTex_TexelSize.y*3.4891992113
	#define OffsetD4y _MainTex_TexelSize.y*5.4830312105
	#define OffsetD5y _MainTex_TexelSize.y*7.4768683759
	#define OffsetD6y _MainTex_TexelSize.y*9.4707125766
	#define OffsetD7y _MainTex_TexelSize.y*11.4645656736
	#define OffsetD8y _MainTex_TexelSize.y*13.4584295168
	#define OffsetD9y _MainTex_TexelSize.y*15.4523059431
	#define OffsetD10y _MainTex_TexelSize.y*17.4461967743
	#define OffsetD11y _MainTex_TexelSize.y*19.4661974725
	#define OffsetD12y _MainTex_TexelSize.y*21.4627427973
	#define OffsetD13y _MainTex_TexelSize.y*23.4592916956
	#define OffsetD14y _MainTex_TexelSize.y*25.455844494
	#define OffsetD15y _MainTex_TexelSize.y*27.4524015179
	#define OffsetD16y _MainTex_TexelSize.y*29.4489630909
	#define OffsetD17y _MainTex_TexelSize.y*31.445529535
	#define OffsetD18y _MainTex_TexelSize.y*33.4421011704

	#define OffsetD2x _MainTex_TexelSize.x*1.4953705027
	#define OffsetD3x _MainTex_TexelSize.x*3.4891992113
	#define OffsetD4x _MainTex_TexelSize.x*5.4830312105
	#define OffsetD5x _MainTex_TexelSize.x*7.4768683759
	#define OffsetD6x _MainTex_TexelSize.x*9.4707125766
	#define OffsetD7x _MainTex_TexelSize.x*11.4645656736
	#define OffsetD8x _MainTex_TexelSize.x*13.4584295168
	#define OffsetD9x _MainTex_TexelSize.x*15.4523059431
	#define OffsetD10x _MainTex_TexelSize.x*17.4461967743
	#define OffsetD11x _MainTex_TexelSize.x*19.4661974725
	#define OffsetD12x _MainTex_TexelSize.x*21.4627427973
	#define OffsetD13x _MainTex_TexelSize.x*23.4592916956
	#define OffsetD14x _MainTex_TexelSize.x*25.455844494
	#define OffsetD15x _MainTex_TexelSize.x*27.4524015179
	#define OffsetD16x _MainTex_TexelSize.x*29.4489630909
	#define OffsetD17x _MainTex_TexelSize.x*31.445529535
	#define OffsetD18x _MainTex_TexelSize.x*33.4421011704

	//Dithering Noise
	#define noiseR float(frac((cos(dot(texcoord, float2(12.9898,78.233)))) * 43758.5453 + texcoord.x)*0.015873)-0.0079365
	#define noiseG float(frac((cos(dot(texcoord, float2(-12.9898,78.233)))) * 43758.5453 + texcoord.x)*0.015873)-0.0079365
	#define noiseB float(frac((sin(dot(texcoord, float2(12.9898,-78.233)))) * 43758.5453 + texcoord.x)*0.015873)-0.0079365

	#include "Common.hlsl"

	float4 Clarity0(v2f i) : SV_Target
	{
		float4 color = _MainTex.Sample(sampler_MainTex, i.texcoord * _Offset);

		return color;
	}

	float4 Clarity1(v2f i) : SV_Target
	{
		float4 blur = _Temp1Tex.Sample(sampler_Temp1Tex, i.texcoord);

		float2 texcoord = i.texcoord;
		float2 coordOffset;

#ifdef RADIUS_TWO
		static const float offset[11] = { 0.0, OffsetB2y, OffsetB3y, OffsetB4y, OffsetB5y, OffsetB6y, OffsetB7y, OffsetB8y, OffsetB9y, OffsetB10y, OffsetB11y };
		static const float weight[11] = { 0.06649, 0.1284697563, 0.111918249, 0.0873132676, 0.0610011113, 0.0381655709, 0.0213835661, 0.0107290241, 0.0048206869, 0.0019396469, 0.0006988718 };
		static const int loopCount = 11;
#elif RADIUS_THEREE
		static const float offset[15] = { 0.0, OffsetC2y, OffsetC3y, OffsetC4y, OffsetC5y, OffsetC6y, OffsetC7y, OffsetC8y, OffsetC9y, OffsetC10y, OffsetC11y, OffsetC12y, OffsetC13y, OffsetC14y, OffsetC15y };
		static const float weight[15] = { 0.0443266667, 0.0872994708, 0.0820892038, 0.0734818355, 0.0626171681, 0.0507956191, 0.0392263968, 0.0288369812, 0.0201808877, 0.0134446557, 0.0085266392, 0.0051478359, 0.0029586248, 0.0016187257, 0.0008430913 };
		static const int loopCount = 15;		
#elif RADIUS_FOUR
		static const float offset[18] = { 0.0, OffsetD2y, OffsetD3y, OffsetD4y, OffsetD5y, OffsetD6y, OffsetD7y, OffsetD8y, OffsetD9y, OffsetD10y, OffsetD11y, OffsetD12y, OffsetD13y, OffsetD14y, OffsetD15y, OffsetD16y, OffsetD17y, OffsetD18y };
		static const float weight[18] = { 0.033245, 0.0659162217, 0.0636705814, 0.0598194658, 0.0546642566, 0.0485871646, 0.0420045997, 0.0353207015, 0.0288880982, 0.0229808311, 0.0177815511, 0.013382297, 0.0097960001, 0.0069746748, 0.0048301008, 0.0032534598, 0.0021315311, 0.0013582974 };
		static const int loopCount = 18;
#elif RADIUS_ONE
		static const float offset[6] = { 0.0, OffsetA2y, OffsetA3y, OffsetA4y, OffsetA5y, OffsetA6y };
		static const float weight[6] = { 0.13298, 0.23227575, 0.1353261595, 0.0511557427, 0.01253922, 0.0019913644 };
		static const int loopCount = 6;
#else
		static const float offset[4] = { 0.0, Offset02y, Offset03y, Offset04y };
		static const float weight[4] = { 0.39894, 0.2959599993, 0.0045656525, 0.00000149278686458842 };
		static const int loopCount = 4;
#endif
		blur *= weight[0];

		[loop]
		for (int i = 1; i < loopCount; ++i) {
			coordOffset = float2(0.0, offset[i]);
			blur += _Temp1Tex.SampleLevel(sampler_Temp1Tex, (texcoord + coordOffset) * weight[i], 0);
			blur += _Temp1Tex.SampleLevel(sampler_Temp1Tex, (texcoord - coordOffset) * weight[i], 0);
		}

		return blur;
	}

	float4 Clarity2(v2f i) : SV_Target
	{
		float4 blur = _Temp2Tex.Sample(sampler_Temp2Tex, i.texcoord);

		float2 texcoord = i.texcoord;
		float2 coordOffset;

#ifdef RADIUS_TWO
		static const float offset[11] = { 0.0, OffsetB2x, OffsetB3x, OffsetB4x, OffsetB5x, OffsetB6x, OffsetB7x, OffsetB8x, OffsetB9x, OffsetB10x, OffsetB11x };
		static const float weight[11] = { 0.06649, 0.1284697563, 0.111918249, 0.0873132676, 0.0610011113, 0.0381655709, 0.0213835661, 0.0107290241, 0.0048206869, 0.0019396469, 0.0006988718 };
		static const int loopCount = 11;
#elif RADIUS_THEREE
		static const float offset[15] = { 0.0, OffsetC2x, OffsetC3x, OffsetC4x, OffsetC5x, OffsetC6x, OffsetC7x, OffsetC8x, OffsetC9x, OffsetC10x, OffsetC11x, OffsetC12x, OffsetC13x, OffsetC14x, OffsetC15x };
		static const float weight[15] = { 0.0443266667, 0.0872994708, 0.0820892038, 0.0734818355, 0.0626171681, 0.0507956191, 0.0392263968, 0.0288369812, 0.0201808877, 0.0134446557, 0.0085266392, 0.0051478359, 0.0029586248, 0.0016187257, 0.0008430913 };
		static const int loopCount = 15;
#elif RADIUS_FOUR
		static const float offset[18] = { 0.0, OffsetD2x, OffsetD3x, OffsetD4x, OffsetD5x, OffsetD6x, OffsetD7x, OffsetD8x, OffsetD9x, OffsetD10x, OffsetD11x, OffsetD12x, OffsetD13x, OffsetD14x, OffsetD15x, OffsetD16x, OffsetD17x, OffsetD18x };
		static const float weight[18] = { 0.033245, 0.0659162217, 0.0636705814, 0.0598194658, 0.0546642566, 0.0485871646, 0.0420045997, 0.0353207015, 0.0288880982, 0.0229808311, 0.0177815511, 0.013382297, 0.0097960001, 0.0069746748, 0.0048301008, 0.0032534598, 0.0021315311, 0.0013582974 };
		static const int loopCount = 18;
#elif RADIUS_ONE
		static const float offset[6] = { 0.0, OffsetA2x, OffsetA3x, OffsetA4x, OffsetA5x, OffsetA6x };
		static const float weight[6] = { 0.13298, 0.23227575, 0.1353261595, 0.0511557427, 0.01253922, 0.0019913644 };
		static const int loopCount = 6;
#else
		static const float offset[4] = { 0.0, Offset02x, Offset03x, Offset04x };
		static const float weight[4] = { 0.39894, 0.2959599993, 0.0045656525, 0.00000149278686458842 };
		static const int loopCount = 4;
#endif
		blur *= weight[0];

		[loop]
		for (int i = 1; i < loopCount; ++i) {
			coordOffset = float2(offset[i], 0.0);
			blur += _Temp2Tex.SampleLevel(sampler_Temp2Tex, (texcoord + coordOffset) * weight[i], 0);
			blur += _Temp2Tex.SampleLevel(sampler_Temp2Tex, (texcoord - coordOffset) * weight[i], 0);
		}

		return blur;
	}

	float4 Clarity3(v2f i) : SV_Target
	{
		float4 orig = _MainTex.Sample(sampler_MainTex, i.texcoord);
		float3 blur = _Temp1Tex.Sample(sampler_Temp1Tex, i.texcoord / (float)_Offset).rgb;
		
		float2 texcoord = i.texcoord;
		float3 sharp = 1.0 - blur;

		if (_MaskContrast) {
			const float3 vivid = saturate((1 - (1 - orig.rgb) / (2 * sharp) + orig.rgb / (2 * (1 - sharp))) * 0.5);
			sharp = (orig.rgb + sharp) * 0.5;
			sharp = lerp(sharp, vivid, _MaskContrast);
		}
		else {
			sharp = (orig.rgb + sharp) * 0.5;
		}

		if (_DarkIntensity || _LightIntensity)
		{
			float3 curve = sharp * sharp * sharp * (sharp * (sharp * 6.0 - 15.0) + 10.0);
			float3 sharpMin = lerp(sharp, curve, _DarkIntensity);
			float3 sharpMax = lerp(sharp, curve, _LightIntensity);
			float3 sharpStep = step(0.5, sharp);
			sharp = (sharpMin * (1 - sharpStep)) + (sharpMax * sharpStep);
		}

		sharp = lerp(sharp, sharp - float3(noiseR, noiseG, noiseB), _DitherStrength);

#ifdef BLENDMODE_SOFTLIGHT
		const float3 A = (2 * orig.rgb * sharp) + (orig.rgb * orig.rgb * (1.0 - 2 * sharp));
		const float3 B = (2 * orig.rgb * (1.0 - sharp)) + (sqrt(orig.rgb) * (2 * sharp - 1.0));
		const float3 C = step(0.49, sharp);
		sharp = (A * (1.0 - C)) + (B * C);
#elif BLENDMODE_OVERLAY
		const float3 A = (2 * orig.rgb * sharp);
		const float3 B = (1.0 - 2 * (1.0 - orig.rgb) * (1.0 - sharp));
		const float3 C = step(0.5, orig.rgb);
		sharp = lerp(A, B, C);
#elif BLENDMODE_HARDLIGHT
		const float3 A = 2 * orig.rgb * sharp;
		const float3 B = 1.0 - 2 * (1.0 - orig.rgb) * (1.0 - sharp);
		const float3 C = step(0.5, sharp);
		sharp = lerp(A, B, C);
#elif BLENDMODE_MULTIPLY
		sharp = saturate(2 * orig.rgb * sharp);
#elif BLENDMODE_VIVIDLIGHT
		const float3 A = 2 * orig.rgb * sharp;
		const float3 B = orig.rgb / (2 * (1 - sharp));
		const float3 C = step(0.50, sharp);
		sharp = lerp(A, B, C);
#elif BLENDMODE_LINEARLIGHT
		sharp = orig.rgb + 2.0 * sharp - 1.0;
#else // BLENDMODE_ADDITION
		sharp = saturate(orig.rgb + (sharp - 0.5));
#endif

		if (_BlendIfDark || _BlendIfLight < 255) {
			const float blendIfD = ((255 - _BlendIfDark) / 255.0);
			const float blendIfL = (_BlendIfLight / 255.0);
			float3 mask = 1.0;
			float range;

			if (_BlendIfDark) {
				range = blendIfD * _BlendIfRange;
				float3 cmix = 1 - orig.rgb;
				mask -= smoothstep(blendIfD - (range), blendIfD + (range), cmix);
			}

			if (_BlendIfLight) {
				range = blendIfL * _BlendIfRange;
				const float3 cmix = orig.rgb;
				mask = lerp(mask, 0.0, smoothstep(blendIfL - range, blendIfL + range, cmix));
			}

			sharp = lerp(orig.rgb, sharp, mask);
		}

		orig.rgb = lerp(orig.rgb, sharp, _Strength);

		return orig;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Clarity0
			ENDHLSL
		}		
		Pass
		{
			HLSLPROGRAM
			#pragma multi_compile RADIUS_ZERO RADIUS_ONE RADIUS_TWO RADIUS_THEREE RADIUS_FOUR
			#pragma vertex Vert
			#pragma fragment Clarity1
			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM
			#pragma multi_compile RADIUS_ZERO RADIUS_ONE RADIUS_TWO RADIUS_THEREE RADIUS_FOUR
			#pragma vertex Vert
			#pragma fragment Clarity2
			ENDHLSL
		}
		Pass
		{
			HLSLPROGRAM
			#pragma multi_compile BLENDMODE_SOFTLIGHT BLENDMODE_OVERLAY BLENDMODE_HARDLIGHT BLENDMODE_MULTIPLY BLENDMODE_VIVIDLIGHT BLENDMODE_LINEARLIGHT BLENDMODE_ADDITION
			#pragma vertex Vert
			#pragma fragment Clarity3
			ENDHLSL
		}
	}
}