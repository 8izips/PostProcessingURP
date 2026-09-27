Shader "Hidden/PostProcessing/AnalogGlitch"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float2 _ScanLineJitter, _VerticalJump, _ColorDrift;
    float _HorizontalShake;
    float nrand(float x, float y) { return frac(sin(dot(float2(x,y), float2(12.9898,78.233))) * 43758.5453); }
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float u = i.texcoord.x, v = i.texcoord.y;
        float jitter = nrand(v, _Time.x) * 2 - 1;
        jitter *= step(_ScanLineJitter.y, abs(jitter)) * _ScanLineJitter.x;
        float jump = lerp(v, frac(v + _VerticalJump.y), _VerticalJump.x);
        float shake = (nrand(_Time.x, 2) - 0.5) * _HorizontalShake;
        float drift = sin(jump + _ColorDrift.y) * _ColorDrift.x;
        float4 a = SampleMain(frac(float2(u + jitter + shake, jump)));
        float4 b = SampleMain(frac(float2(u + jitter + shake + drift, jump)));
        return float4(a.r, b.g, a.b, a.a);
    }

    ENDHLSL
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment Frag

            ENDHLSL
        }
    }
}
