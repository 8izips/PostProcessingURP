Shader "Hidden/PostProcessing/EdgeDetection"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
    float _Intensity, _Thickness;
    float4 _Threshold;
    float3 _Color;
    float4 DepthNormal(float2 uv)
    {
        uv=saturate(uv);
        // URP owns normal encoding, including _GBUFFER_NORMALS_OCT.
        return float4(SampleSceneNormals(uv), max(PPEyeDepth(SampleSceneDepth(uv)),1e-5));
    }
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        const float2 offsets[8]={float2(-1,-1),float2(-1,0),float2(-1,1),float2(0,-1),
            float2(0,1),float2(1,-1),float2(1,0),float2(1,1)};
        float4 center=DepthNormal(i.texcoord);
        float4 sum=0; float weight=0;
        [unroll] for (int j=0;j<8;j++)
        {
            float4 sampleValue=DepthNormal(i.texcoord+offsets[j]*_MainTex_TexelSize.xy*_Thickness);
            sum+=sampleValue/sampleValue.w;
            weight+=rcp(sampleValue.w);
        }
        sum/=max(weight,1e-5);
        sum.xyz*=rsqrt(max(dot(sum.xyz,sum.xyz),1e-12));
        float normalSame=saturate((dot(center.xyz,sum.xyz)-_Threshold.x)/max(_Threshold.y-_Threshold.x,1e-5));
        normalSame=normalSame*normalSame*(3-2*normalSame);
        float depthEdge=saturate((abs(center.w-sum.w)-_Threshold.z*center.w)/max((_Threshold.w-_Threshold.z)*center.w,1e-5));
        depthEdge=depthEdge*depthEdge*(3-2*depthEdge);
        float4 color=SampleMain(i.texcoord);
        color.rgb=lerp(color.rgb,_Color,(1-normalSame*(1-depthEdge))*_Intensity);
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
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
