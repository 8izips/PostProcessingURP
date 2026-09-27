Shader "Hidden/PostProcessing/WaterColor"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    TEXTURE2D(_NoiseTex);
    SAMPLER(sampler_NoiseTex);
    float4 _EffectParams, _EdgeColor, _FillColor;
    float _Interval;
    int _Iteration;
    float3 Noise(float2 uv) { return SAMPLE_TEXTURE2D_LOD(_NoiseTex,sampler_NoiseTex,uv,0).rgb; }
    float Luma(float2 uv) { return Luminance(SampleMain(uv).rgb); }
    float2 Gradient(float2 uv,float frequency)
    {
        float2 dx=float2(_Interval/200,0);
        return float2(Luma(uv+dx)-Luma(uv-dx),Luma(uv+dx.yx)-Luma(uv-dx.yx))+(Noise(uv*0.4*frequency).gb-0.5)*0.05;
    }
    float ProcessEdge(inout float2 uv,float stride)
    {
        float2 gradient=Gradient(uv,1);
        float edge=saturate(length(gradient)*10);
        float pattern=Noise(uv*0.8).r;
        uv+=PPSafeNormalize(gradient.yx*float2(-1,1))*stride;
        return pattern*edge;
    }
    float3 ProcessFill(inout float2 uv,float stride)
    {
        uv+=PPSafeNormalize(Gradient(uv,_EffectParams.z))*stride;
        float shift=Noise(uv*0.1).r*2;
        return SampleMain(uv).rgb*HsvToRgb(float3(shift,_EffectParams.w,1));
    }
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        int count=clamp(_Iteration,1,128);
        float reciprocal=rcp((float)count), stride=0.04*reciprocal;
        float2 en=i.texcoord, ep=i.texcoord, cn=i.texcoord, cp=i.texcoord;
        float edge=0, edgeWeight=0, colorWeight=0;
        float3 color=0;
        [loop] for (int j=0;j<count;j++)
        {
            float we=1.5-j*reciprocal, wc=0.2+j*reciprocal;
            edge+=(ProcessEdge(en,-stride)+ProcessEdge(ep,stride))*we;
            edgeWeight+=we*2;
            color+=(ProcessFill(cn,-stride*_EffectParams.y)+ProcessFill(cp,stride*_EffectParams.y)*0.3)*wc;
            colorWeight+=wc*1.3;
        }
        edge=saturate((edge/max(edgeWeight,1e-5)-0.5)*_EffectParams.x+0.5);
        color/=max(colorWeight,1e-5);
        float3 edges=lerp(1,_EdgeColor.rgb,_EdgeColor.a*edge);
        float3 fill=lerp(1,color,_FillColor.a)*_FillColor.rgb;
        return float4(edges*fill,SampleMain(i.texcoord).a);
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
