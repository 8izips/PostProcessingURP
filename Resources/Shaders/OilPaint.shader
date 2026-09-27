Shader "Hidden/PostProcessing/OilPaint"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    int _Radius;
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        int radius = clamp(_Radius,1,16);
        float n = (radius+1)*(radius+1);
        float minVariance = 3.402823466e+38;
        float4 color = SampleMain(i.texcoord);
        [unroll] for (int region=0; region<4; region++)
        {
            float2 start = float2((region<2)?-radius:0, ((region&1)==0)?-radius:0);
            float3 sum=0, sumSquared=0;
            [loop] for (int x=0; x<=radius; x++)
                [loop] for (int y=0; y<=radius; y++)
                {
                    float3 sampleColor = SampleMain(i.texcoord+(start+float2(x,y))*_MainTex_TexelSize.xy).rgb;
                    sum += sampleColor;
                    sumSquared += sampleColor*sampleColor;
                }
            float3 mean = sum/n;
            float variance = dot(abs(sumSquared/n-mean*mean),float3(1,1,1));
            if (variance<minVariance) { minVariance=variance; color.rgb=mean; }
        }
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
