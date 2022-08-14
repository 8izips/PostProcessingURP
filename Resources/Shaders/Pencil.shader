Shader "Hidden/PostProcessing/Pencil"
{
	HLSLINCLUDE
	Texture2D _MainTex;
	SamplerState sampler_MainTex;
	float4 _MainTex_TexelSize;

	float _GradThreshold;
	float _ColorThreshold;
	float _Sensivity;

	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float4 screenuv : TEXCOORD0;
	};

	v2f Vert(appdata v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, v.vertex));
		o.screenuv = ComputeScreenPos(o.pos);
		return o;
	}

	#define PI2 6.28318530717959
	#define RANGE 16
	#define STEP 2
	#define ANGLENUM 4
	#define GRADTHRESH 0.01

	float4 getCol(float2 pos)
	{
		return _MainTex.Sample(sampler_MainTex, pos / _ScreenParams.xy);
	}

	float getVal(float2 pos)
	{
		float4 c = getCol(pos);
		return dot(c.xyz, float3(0.2126, 0.7152, 0.0722));
	}

	float2 getGrad(float2 pos, float delta)
	{
		float2 d = float2(delta, 0.0);
		return float2(getVal(pos + d.xy) - getVal(pos - d.xy),
			getVal(pos + d.yx) - getVal(pos - d.yx)) / delta / 2.0;
	}

	void pR(inout float2 p, float a) {
		p = cos(a) * p + sin(a) * float2(p.y, -p.x);
	}

	float4 Frag(v2f i) : SV_Target
	{
		float2 screenuv = i.screenuv.xy / i.screenuv.w;
		float2 screenPos = float2(i.screenuv.x * _ScreenParams.x, i.screenuv.y * _ScreenParams.y);
		float weight = 1.0;

		[unroll]
		for (int j = 0; j < ANGLENUM; j++)
		{
			float2 dir = float2(1.0, 0.0);
			pR(dir, j * PI2 / (2.0 * ANGLENUM));

			float2 grad = float2(-dir.y, dir.x);

			[unroll]
			for (int i = -RANGE; i <= RANGE; i += STEP)
			{
				float2 b = normalize(dir);
				float2 pos2 = screenPos + float2(b.x, b.y) * i;

				if (pos2.y < 0.0 || pos2.x < 0.0 || pos2.x > _ScreenParams.x || pos2.y > _ScreenParams.y)
					continue;

				float2 g = getGrad(pos2, 1.0);

				if (sqrt(dot(g,g)) < _GradThreshold)
					continue;

				weight -= pow(abs(dot(normalize(grad), normalize(g))), _Sensivity) / floor((2.0 * RANGE + 1.0) / STEP) / ANGLENUM;
			}
		}

		float4 col = getCol(screenPos);
		float4 background = lerp(col, float4(1.0, 1.0, 1.0, 1.0), _ColorThreshold);

		return lerp(float4(0.0, 0.0, 0.0, 0.0), background, weight);
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
