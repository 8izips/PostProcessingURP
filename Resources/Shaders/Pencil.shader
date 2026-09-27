Shader "Hidden/PostProcessing/Pencil"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    float _GradThreshold, _ColorThreshold, _Sensivity;
    float LuminanceAt(float2 pixel)
    {
        return dot(SampleMain(pixel*_MainTex_TexelSize.xy).rgb,float3(0.2126,0.7152,0.0722));
    }
    float2 GradientAt(float2 pixel)
    {
        return float2(LuminanceAt(pixel+float2(1,0))-LuminanceAt(pixel-float2(1,0)),
                      LuminanceAt(pixel+float2(0,1))-LuminanceAt(pixel-float2(0,1)))*0.5;
    }
    float4 Frag(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float2 pixel = i.texcoord*_MainTex_TexelSize.zw;
        float weight=1;
        [unroll] for (int angle=0; angle<4; angle++)
        {
            float a=angle*PI/4;
            float2 direction=float2(cos(a),-sin(a));
            float2 perpendicular=float2(-direction.y,direction.x);
            [unroll] for (int offset=-16; offset<=16; offset+=2)
            {
                float2 position=pixel+direction*offset;
                if (any(position<0) || any(position>=_MainTex_TexelSize.zw)) continue;
                float2 gradient=GradientAt(position);
                if (length(gradient)<max(_GradThreshold,1e-6)) continue;
                weight -= pow(abs(dot(perpendicular,PPSafeNormalize(gradient))),_Sensivity)/(17.0*4.0);
            }
        }
        float4 color=SampleMain(i.texcoord);
        color.rgb=lerp(color.rgb,1,_ColorThreshold)*saturate(weight);
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
