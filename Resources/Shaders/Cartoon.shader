Shader "Hidden/PostProcessing/Cartoon"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float _Power, _EdgeSlope;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 color = SampleMain(i.texcoord);
        const float3 luma = float3(0.2126,0.7152,0.0722);
        float2 stepUV = _MainTex_TexelSize.xy;
        float d1 = dot(luma, SampleMain(i.texcoord-stepUV).rgb-SampleMain(i.texcoord+stepUV).rgb);
        float2 diagonal = stepUV * float2(1,-1);
        float d2 = dot(luma, SampleMain(i.texcoord-diagonal).rgb-SampleMain(i.texcoord+diagonal).rgb);
        color.rgb = max(0, color.rgb - pow(abs(d1*d1+d2*d2), _EdgeSlope) * _Power);
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
