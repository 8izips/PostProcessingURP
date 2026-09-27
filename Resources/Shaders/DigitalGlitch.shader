Shader "Hidden/PostProcessing/DigitalGlitch"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    TEXTURE2D(_NoiseTex);
    SAMPLER(sampler_NoiseTex);
    PP_DECLARE_TEXTURE(_TrashTex);
    float _Intensity;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 glitch = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, i.texcoord, 0);
        float threshold = 1.001 - _Intensity * 1.001;
        float wd = step(threshold, pow(abs(glitch.z), 2.5));
        float wf = step(threshold, pow(abs(glitch.w), 2.5));
        float wc = step(threshold, pow(abs(glitch.z), 3.5));
        float2 uv = frac(i.texcoord + glitch.xy * wd);
        float4 source = SampleMain(uv);
        float3 color = lerp(source.rgb, PP_SAMPLE_TEXTURE(_TrashTex, uv).rgb, wf);
        float3 negative = saturate(color.grb + (1 - dot(color, float3(1,1,1))) * 0.5);
        return float4(lerp(color, negative, wc), source.a);
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
