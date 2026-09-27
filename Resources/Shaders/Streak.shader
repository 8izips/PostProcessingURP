Shader "Hidden/PostProcessing/Streak"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    PP_DECLARE_TEXTURE(_HighTex);
    float _Threshold, _Stretch, _Intensity;
    float3 _Color;
    float4 FragPrefilter(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float dy=_MainTex_TexelSize.y*0.75;
        float3 color=(SampleMain(i.texcoord+float2(0,-dy)).rgb+SampleMain(i.texcoord+float2(0,dy)).rgb)*0.5;
        float brightness=max(color.r,max(color.g,color.b));
        color*=max(0,brightness-_Threshold)/max(brightness,1e-5);
        return float4(color,1);
    }
    float4 FragDownsample(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float dx=_MainTex_TexelSize.x*1.25;
        float3 sum=0;
        [unroll] for (int j=-5;j<=5;j+=2) sum+=SampleMain(i.texcoord+float2(dx*j,0)).rgb;
        return float4(sum/6,1);
    }
    float4 FragUpsample(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        return float4(lerp(PP_SAMPLE_TEXTURE(_HighTex,i.texcoord).rgb,SampleMain(i.texcoord).rgb,_Stretch),1);
    }
    float4 FragComposition(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 original=PP_SAMPLE_TEXTURE(_HighTex,i.texcoord);
        original.rgb+=SampleMain(i.texcoord).rgb*_Color*_Intensity*5;
        return original;
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
            #pragma fragment FragPrefilter

            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment FragDownsample

            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment FragUpsample

            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment FragComposition

            ENDHLSL
        }
    }
}
