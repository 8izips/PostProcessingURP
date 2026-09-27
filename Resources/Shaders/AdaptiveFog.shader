Shader "Hidden/PostProcessing/AdaptiveFog"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    float _BloomThreshold, _BloomPower, _BloomWidth, _MaxFogFactor, _FogCurve, _FogStart;
    float4 _FogColor;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 color=SampleMain(i.texcoord);
        float depth=PPEyeDepth(SampleSceneDepth(i.texcoord));
        float fog=clamp(saturate(depth-_FogStart)*_FogCurve,0,max(0,_MaxFogFactor));
        if (fog<=0) return color;
        float3 blur=0;
        float width=max(_BloomWidth,1e-4);
        // Pixel offsets must not depend on the screen position (UV).
        float2 stepUV=_MainTex_TexelSize.xy*width;
        [unroll] for (int x=0;x<5;x++)
            [unroll] for (int y=0;y<5;y++)
            {
                float2 offset=float2(2.5-x,-2.5+y);
                float distanceWeight=saturate(sqrt(dot(offset,offset)/8.0));
                blur+=lerp(SampleMain(i.texcoord+offset*stepUV).rgb,color.rgb,distanceWeight);
            }
        blur/=max(1e-4,25-(_BloomPower-_BloomThreshold*5));
        float amount=saturate(dot(color.rgb,float3(0.299,0.587,0.114)));
        float3 bloom=lerp(color.rgb,blur*(_BloomPower+4),amount);
        color.rgb=lerp(color.rgb,lerp(bloom,_FogColor.rgb,fog),fog);
        return color; // Preserve input alpha and HDR values; no whole-screen saturate.
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
