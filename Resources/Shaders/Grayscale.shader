Shader "Hidden/PostProcessing/Grayscale"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float _Blend;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 color = SampleMain(i.texcoord);
        float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750));
        color.rgb = lerp(color.rgb, luminance.xxx, _Blend);
        return color;
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
