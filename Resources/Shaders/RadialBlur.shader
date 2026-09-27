Shader "Hidden/PostProcessing/RadialBlur"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float _BlurRadius, _RadialCenterX, _RadialCenterY;
    int _Iteration;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float2 delta = (float2(_RadialCenterX,_RadialCenterY)-i.texcoord)*_BlurRadius;
        float4 color = 0;
        int count = max(1,_Iteration);
        [loop] for (int j=0; j<count; j++) color += SampleMain(i.texcoord+delta*j);
        return color/count;
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
