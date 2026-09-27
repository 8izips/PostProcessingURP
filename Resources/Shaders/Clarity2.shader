Shader "Hidden/PostProcessing/Clarity2"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    PP_DECLARE_TEXTURE(_Temp1Tex);
    int _Offset, _BlendIfDark, _BlendIfLight;
    float _BlendIfRange, _Strength, _MaskContrast, _DarkIntensity, _LightIntensity, _DitherStrength;

    float4 BlurAxis(float2 uv,float2 axis)
    {
#if defined(RADIUS_TWO)
        const int count=11;
        const float offsets[11]={0,1.4895848401,3.4757135714,5.4618796741,7.4481042327,9.4344079746,11.420811147,13.4073334,15.3939936778,17.3808101174,19.3677999584};
        const float weights[11]={0.06649,0.1284697563,0.111918249,0.0873132676,0.0610011113,0.0381655709,0.0213835661,0.0107290241,0.0048206869,0.0019396469,0.0006988718};
#elif defined(RADIUS_THEREE)
        const int count=15;
        const float offsets[15]={0,1.4953705027,3.4891992113,5.4830312105,7.4768683759,9.4707125766,11.4645656736,13.4584295168,15.4523059431,17.4461967743,19.4401038149,21.43402885,23.4279736431,25.4219399344,27.4159294386};
        const float weights[15]={0.0443266667,0.0872994708,0.0820892038,0.0734818355,0.0626171681,0.0507956191,0.0392263968,0.0288369812,0.0201808877,0.0134446557,0.0085266392,0.0051478359,0.0029586248,0.0016187257,0.0008430913};
#elif defined(RADIUS_FOUR)
        const int count=18;
        const float offsets[18]={0,1.4953705027,3.4891992113,5.4830312105,7.4768683759,9.4707125766,11.4645656736,13.4584295168,15.4523059431,17.4461967743,19.4661974725,21.4627427973,23.4592916956,25.455844494,27.4524015179,29.4489630909,31.445529535,33.4421011704};
        const float weights[18]={0.033245,0.0659162217,0.0636705814,0.0598194658,0.0546642566,0.0485871646,0.0420045997,0.0353207015,0.0288880982,0.0229808311,0.0177815511,0.013382297,0.0097960001,0.0069746748,0.0048301008,0.0032534598,0.0021315311,0.0013582974};
#elif defined(RADIUS_ONE)
        const int count=6;
        const float offsets[6]={0,1.4584295168,3.40398480678,5.3518057801,7.302940716,9.2581597095};
        const float weights[6]={0.13298,0.23227575,0.1353261595,0.0511557427,0.01253922,0.0019913644};
#else
        const int count=4;
        const float offsets[4]={0,1.1824255238,3.0293122308,5.0040701377};
        const float weights[4]={0.39894,0.2959599993,0.0045656525,0.00000149278686458842};
#endif
        float4 color=SampleMain(uv)*weights[0];
        float normalization=weights[0];
        [unroll] for (int j=1;j<count;j++)
        {
            float2 delta=axis*_MainTex_TexelSize.xy*offsets[j];
            // Weight the sampled COLOR, never the sampling coordinate.
            color+=(SampleMain(uv+delta)+SampleMain(uv-delta))*weights[j];
            normalization+=2*weights[j];
        }
        return color/normalization;
    }
    float4 Clarity0(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        return SampleMain(i.texcoord*max(1,_Offset));
    }
    float4 Clarity1(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        return BlurAxis(i.texcoord,float2(0,1));
    }
    float4 Clarity2(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        return BlurAxis(i.texcoord,float2(1,0));
    }
    float3 SafeDenominator(float3 value)
    {
        return lerp(-1.0,1.0,step(0,value))*max(abs(value),1e-5);
    }
    float4 Clarity3(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float4 original=SampleMain(i.texcoord);
        float3 blur=PP_SAMPLE_TEXTURE(_Temp1Tex,i.texcoord/(float)max(1,_Offset)).rgb;
        float3 sharp=1-blur;
        if (_MaskContrast!=0)
        {
            float3 vivid=saturate((1-(1-original.rgb)/SafeDenominator(2*sharp)+original.rgb/SafeDenominator(2*(1-sharp)))*0.5);
            sharp=lerp((original.rgb+sharp)*0.5,vivid,_MaskContrast);
        }
        else sharp=(original.rgb+sharp)*0.5;
        if (_DarkIntensity!=0 || _LightIntensity!=0)
        {
            float3 curve=sharp*sharp*sharp*(sharp*(sharp*6-15)+10);
            sharp=lerp(lerp(sharp,curve,_DarkIntensity),lerp(sharp,curve,_LightIntensity),step(0.5,sharp));
        }
        float2 uv=i.texcoord;
        float3 noise=float3(frac(cos(dot(uv,float2(12.9898,78.233)))*43758.5453+uv.x),
            frac(cos(dot(uv,float2(-12.9898,78.233)))*43758.5453+uv.x),
            frac(sin(dot(uv,float2(12.9898,-78.233)))*43758.5453+uv.x))*0.015873-0.0079365;
        sharp-=noise*_DitherStrength;
#ifdef BLENDMODE_SOFTLIGHT
        float3 a=2*original.rgb*sharp+original.rgb*original.rgb*(1-2*sharp);
        float3 b=2*original.rgb*(1-sharp)+sqrt(max(original.rgb,0))*(2*sharp-1);
        sharp=lerp(a,b,step(0.49,sharp));
#elif defined(BLENDMODE_OVERLAY)
        sharp=lerp(2*original.rgb*sharp,1-2*(1-original.rgb)*(1-sharp),step(0.5,original.rgb));
#elif defined(BLENDMODE_HARDLIGHT)
        sharp=lerp(2*original.rgb*sharp,1-2*(1-original.rgb)*(1-sharp),step(0.5,sharp));
#elif defined(BLENDMODE_MULTIPLY)
        sharp=saturate(2*original.rgb*sharp);
#elif defined(BLENDMODE_VIVIDLIGHT)
        sharp=lerp(2*original.rgb*sharp,original.rgb/SafeDenominator(2*(1-sharp)),step(0.5,sharp));
#elif defined(BLENDMODE_LINEARLIGHT)
        sharp=original.rgb+2*sharp-1;
#else
        sharp=saturate(original.rgb+sharp-0.5);
#endif
        if (_BlendIfDark!=0 || _BlendIfLight<255)
        {
            float3 mask=1;
            if (_BlendIfDark!=0)
            {
                float center=(255-_BlendIfDark)/255.0;
                float range=max(center*_BlendIfRange,1e-5);
                mask-=smoothstep(center-range,center+range,1-original.rgb);
            }
            if (_BlendIfLight!=0)
            {
                float center=_BlendIfLight/255.0;
                float range=max(center*_BlendIfRange,1e-5);
                mask=lerp(mask,0,smoothstep(center-range,center+range,original.rgb));
            }
            sharp=lerp(original.rgb,sharp,mask);
        }
        original.rgb=lerp(original.rgb,sharp,_Strength);
        return original;
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
            #pragma fragment Clarity0

            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment Clarity1
            #pragma multi_compile_local_fragment RADIUS_ZERO RADIUS_ONE RADIUS_TWO RADIUS_THEREE RADIUS_FOUR
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment Clarity2
            #pragma multi_compile_local_fragment RADIUS_ZERO RADIUS_ONE RADIUS_TWO RADIUS_THEREE RADIUS_FOUR
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment Clarity3
            #pragma multi_compile_local_fragment BLENDMODE_SOFTLIGHT BLENDMODE_OVERLAY BLENDMODE_HARDLIGHT BLENDMODE_MULTIPLY BLENDMODE_VIVIDLIGHT BLENDMODE_LINEARLIGHT BLENDMODE_ADDITION
            ENDHLSL
        }
    }
}
