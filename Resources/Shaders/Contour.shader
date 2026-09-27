Shader "Hidden/PostProcessing/Contour"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
    float4 _Color, _Background;
    float _Threshold, _InvRange, _ColorSensitivity, _DepthSensitivity, _NormalSensitivity, _InvFallOff;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float2 uv0=i.texcoord, uv1=saturate(i.texcoord+_MainTex_TexelSize.xy);
        float2 uv2=saturate(i.texcoord+float2(_MainTex_TexelSize.x,0));
        float2 uv3=saturate(i.texcoord+float2(0,_MainTex_TexelSize.y));
        float4 color=SampleMain(uv0);
        float edge=0;
#ifdef _CONTOUR_COLOR
        float3 cg1=SampleMain(uv1).rgb-color.rgb;
        float3 cg2=SampleMain(uv3).rgb-SampleMain(uv2).rgb;
        edge=sqrt(dot(cg1,cg1)+dot(cg2,cg2))*_ColorSensitivity;
#endif
#ifdef _CONTOUR_DEPTH
        float z0=PPEyeDepth(SampleSceneDepth(uv0)), z1=PPEyeDepth(SampleSceneDepth(uv1));
        float z2=PPEyeDepth(SampleSceneDepth(uv2)), z3=PPEyeDepth(SampleSceneDepth(uv3));
        // Take the minimum AFTER linearizing; raw minimum is farthest on reversed-Z.
        float nearest=max(min(min(z0,z1),min(z2,z3)),1e-5);
        float falloff=1-saturate(nearest*_InvFallOff);
        edge=max(edge,length(float2(z1-z0,z3-z2))*falloff*_DepthSensitivity/nearest);
#endif
#ifdef _CONTOUR_NORMAL
        float3 ng1=SampleSceneNormals(uv1)-SampleSceneNormals(uv0);
        float3 ng2=SampleSceneNormals(uv3)-SampleSceneNormals(uv2);
        edge=max(edge,sqrt(dot(ng1,ng1)+dot(ng2,ng2))*_NormalSensitivity);
#endif
        edge=saturate((edge-_Threshold)*_InvRange);
        color.rgb=lerp(lerp(color.rgb,_Background.rgb,_Background.a),_Color.rgb,edge*_Color.a);
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
            #pragma multi_compile_local_fragment _ _CONTOUR_COLOR
            #pragma multi_compile_local_fragment _ _CONTOUR_DEPTH
            #pragma multi_compile_local_fragment _ _CONTOUR_NORMAL
            ENDHLSL
        }
    }
}
