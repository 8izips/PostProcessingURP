Shader "Hidden/PostProcessing/FakeHDR"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float _HDRPower, _Radius1, _Radius2;
    float3 BloomRing(float2 uv, float radius)
    {
        const float2 offsets[8] = { float2(1.5,-1.5), float2(-1.5,-1.5), float2(1.5,1.5), float2(-1.5,1.5),
            float2(0,-2.5), float2(0,2.5), float2(-2.5,0), float2(2.5,0) };
        float3 result = 0;
        [unroll] for (int j=0; j<8; j++) result += SampleMain(uv + offsets[j]*radius*_MainTex_TexelSize.xy).rgb;
        return result;
    }
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 color = SampleMain(i.texcoord);
        float3 first = BloomRing(i.texcoord,_Radius1)*0.005;
        float3 second = BloomRing(i.texcoord,_Radius2)*0.010;
        float3 hdr = (color.rgb+second-first)*(_Radius2-_Radius1);
        color.rgb = pow(abs(hdr+color.rgb),abs(_HDRPower))+hdr;
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
