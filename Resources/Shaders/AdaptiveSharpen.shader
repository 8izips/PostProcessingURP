Shader "Hidden/PostProcessing/AdaptiveSharpen"
{
    HLSLINCLUDE
    #include "Common.hlsl"

    PP_DECLARE_TEXTURE(_TempTex);
    float _CurveHeight, _CurveSlope, _LightOvershoot, _LightCompressionLow, _LightCompressionHigh;
    float _DarkOvershoot, _DarkCompressionLow, _DarkCompressionHigh, _ScaleLim, _ScaleCompressionSlope, _PowerMeanP;
    #define sqr(a) ((a)*(a))
    #define max4(a,b,c,d) max(max(a,b),max(c,d))
    #define texc(x,y) (_MainTex_TexelSize.xy*float2(x,y)+i.texcoord)
    #define getB(x,y) (SampleMain(texc(x,y)).rgb)
    #define getT(x,y) (PP_SAMPLE_TEXTURE(_TempTex,texc(x,y)).xy)
    #define b_diff(pix) abs(blur-c[pix])
    #define wpmean(a,b,w) pow(abs(w)*pow(abs(a),_PowerMeanP)+abs(1-w)*pow(abs(b),_PowerMeanP),rcp(_PowerMeanP))
    #define mdiff(a,b,c,d,e,f,g) max(1e-6,(abs(luma[g]-luma[a])+abs(luma[g]-luma[b])+abs(luma[g]-luma[c])+abs(luma[g]-luma[d])+0.5*(abs(luma[g]-luma[e])+abs(luma[g]-luma[f]))))
    #define soft_if(a,b,c) saturate((a+b+c+0.056)*rcp(abs(maxedge)+0.03)-0.85)
    float soft_lim(float v,float s)
    {
        s=max(abs(s),1e-5);
#ifdef FastOps
        float vs=v/s, vs2=vs*vs;
        return saturate(abs(vs)*(27+vs2)/(27+9*vs2))*s;
#else
        float sv=exp(2*min(abs(v),s*24)/s);
        return (sv-1)/(sv+1)*s;
#endif
    }
#ifdef FastOps
    #define fskip_th (0.114*pow(min(abs(_LightOvershoot),abs(_DarkOvershoot)),0.676)+3.20e-4)
#else
    #define fskip_th 0.000110882
#endif
    float2 Frag0(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float3 c[13]={getB(0,0),getB(-1,-1),getB(0,-1),getB(1,-1),getB(-1,0),getB(1,0),
            getB(-1,1),getB(0,1),getB(1,1),getB(0,-2),getB(-2,0),getB(2,0),getB(0,2)};
        float luma=sqrt(dot(float3(0.2558,0.6511,0.0931),sqr(c[0])));
        float3 blur=(2*(c[2]+c[4]+c[5]+c[7])+c[1]+c[3]+c[6]+c[8]+4*c[0])/16;
        float compression=saturate(4.0/15.0+0.9*exp2(dot(blur,float3(-37.0/15.0,-37.0/15.0,-37.0/15.0))));
        float edge=length(1.38*b_diff(0)+1.15*(b_diff(2)+b_diff(4)+b_diff(5)+b_diff(7))+
            0.92*(b_diff(1)+b_diff(3)+b_diff(6)+b_diff(8))+0.23*(b_diff(9)+b_diff(10)+b_diff(11)+b_diff(12)));
        return float2(edge*compression,luma);
    }
    float4 Frag1(v2f i) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
        float3 original=getB(0,0);
        float2 d[25]={getT(0,0),getT(-1,-1),getT(0,-1),getT(1,-1),getT(-1,0),getT(1,0),getT(-1,1),getT(0,1),getT(1,1),
            getT(0,-2),getT(-2,0),getT(2,0),getT(0,2),getT(0,3),getT(1,2),getT(-1,2),getT(3,0),getT(2,1),getT(2,-1),
            getT(-3,0),getT(-2,1),getT(-2,-1),getT(0,-3),getT(1,-2),getT(-1,-2)};
        float maxedge=max4(max4(d[1].x,d[2].x,d[3].x,d[4].x),max4(d[5].x,d[6].x,d[7].x,d[8].x),max4(d[9].x,d[10].x,d[11].x,d[12].x),d[0].x);
        float sbe=soft_if(d[2].x,d[9].x,d[22].x)*soft_if(d[7].x,d[12].x,d[13].x)+
            soft_if(d[4].x,d[10].x,d[19].x)*soft_if(d[5].x,d[11].x,d[16].x)+
            soft_if(d[1].x,d[24].x,d[21].x)*soft_if(d[8].x,d[14].x,d[17].x)+
            soft_if(d[3].x,d[23].x,d[18].x)*soft_if(d[6].x,d[20].x,d[15].x);
#ifdef FastOps
        float2 cs=lerp(float2(_LightCompressionLow,_DarkCompressionLow),float2(_LightCompressionHigh,_DarkCompressionHigh),saturate(1.091*sbe-2.282));
#else
        float2 cs=lerp(float2(_LightCompressionLow,_DarkCompressionLow),float2(_LightCompressionHigh,_DarkCompressionHigh),smoothstep(2,3.1,sbe));
#endif
        float luma[25];
        [unroll] for (int pixel=0;pixel<25;pixel++) luma[pixel]=d[pixel].y;
        const float3 W1=float3(0.5,1,1.41421356237), W2=float3(0.86602540378,1,0.54772255751);
#ifdef FastOps
        float3 dW=sqr(lerp(W1,W2,saturate(2.4*d[0].x-0.82)));
#else
        float3 dW=sqr(lerp(W1,W2,smoothstep(0.3,0.8,d[0].x)));
#endif
        float centerDiff=0.02+3*(abs(luma[0]-luma[2])+abs(luma[0]-luma[4])+abs(luma[0]-luma[5])+abs(luma[0]-luma[7])+
            0.25*(abs(luma[0]-luma[1])+abs(luma[0]-luma[3])+abs(luma[0]-luma[6])+abs(luma[0]-luma[8])));
        float weights[12]={min(centerDiff/mdiff(24,21,2,4,9,10,1),dW.y),dW.x,min(centerDiff/mdiff(23,18,5,2,9,11,3),dW.y),dW.x,dW.x,
            min(centerDiff/mdiff(4,20,15,7,10,12,6),dW.y),dW.x,min(centerDiff/mdiff(5,7,17,14,12,11,8),dW.y),
            min(centerDiff/mdiff(2,24,23,22,1,3,9),dW.z),min(centerDiff/mdiff(20,19,21,4,1,6,10),dW.z),
            min(centerDiff/mdiff(17,5,18,16,3,8,11),dW.z),min(centerDiff/mdiff(13,15,7,14,6,8,12),dW.z)};
        weights[0]=(max(max((weights[8]+weights[9])/4,weights[0]),0.25)+weights[0])/2;
        weights[2]=(max(max((weights[8]+weights[10])/4,weights[2]),0.25)+weights[2])/2;
        weights[5]=(max(max((weights[9]+weights[11])/4,weights[5]),0.25)+weights[5])/2;
        weights[7]=(max(max((weights[10]+weights[11])/4,weights[7]),0.25)+weights[7])/2;
        float lowThresholdSum=0, weightSum=0, negativeLaplacian=0;
        [unroll] for (int pixel=0;pixel<12;pixel++)
        {
#ifdef FastOps
            float low=clamp(13.2*d[pixel+1].x-0.221,0.01,1);
            float weighted=weights[pixel]*low;
            negativeLaplacian+=sqr(luma[pixel+1])*weighted;
#else
            float t=saturate((d[pixel+1].x-0.01)/0.09);
            float low=t*t*(2.97-1.98*t)+0.01;
            float weighted=weights[pixel]*low;
            negativeLaplacian+=pow(abs(luma[pixel+1])+0.06,2.4)*weighted;
#endif
            weightSum+=weighted;
            lowThresholdSum+=low/12;
        }
#ifdef FastOps
        negativeLaplacian=sqrt(max(negativeLaplacian/max(weightSum,1e-6),0));
#else
        negativeLaplacian=pow(abs(negativeLaplacian/max(weightSum,1e-6)),1.0/2.4)-0.06;
#endif
        float sharpen=_CurveHeight/max(_CurveHeight*_CurveSlope*pow(abs(d[0].x),3.5)+0.625,1e-5);
        float difference=(d[0].y-negativeLaplacian)*(lowThresholdSum*sharpen+0.01);
        [branch] if (abs(difference)>fskip_th)
        {
            // The original partial-sort network, with FastOps consistently selecting 2 or 3 rounds.
#ifdef FastOps
            const int rounds=2;
#else
            const int rounds=3;
#endif
            [unroll] for (int round=0;round<rounds;round++)
            {
                [unroll] for (int a=round;a<24-round;a+=2)
                {
                    float temporary=luma[a];
                    luma[a]=min(luma[a],luma[a+1]);
                    luma[a+1]=max(temporary,luma[a+1]);
                }
                [unroll] for (int a=24-round;a>round;a-=2)
                {
                    float temporary=luma[round];
                    luma[round]=min(luma[round],luma[a]);
                    luma[a]=max(temporary,luma[a]);
                    temporary=luma[24-round];
                    luma[24-round]=max(luma[24-round],luma[a-1]);
                    luma[a-1]=min(temporary,luma[a-1]);
                }
            }
#ifdef FastOps
            float nearMax=(max(luma[23],d[0].y)*2+luma[24])/3;
            float nearMin=(min(luma[1],d[0].y)*2+luma[0])/3;
            float minDistance=min(abs(nearMax-d[0].y),abs(d[0].y-nearMin));
            float positiveScale=minDistance+_LightOvershoot;
            float negativeScale=minDistance+_DarkOvershoot;
#else
            float nearMax=(max(luma[22]+luma[23]*2,d[0].y*3)+luma[24])/4;
            float nearMin=(min(luma[2]+luma[1]*2,d[0].y*3)+luma[0])/4;
            float minDistance=min(abs(nearMax-d[0].y),abs(d[0].y-nearMin));
            float positiveScale=minDistance+min(_LightOvershoot,1.0001-minDistance-d[0].y);
            float negativeScale=minDistance+min(_DarkOvershoot,0.0001+d[0].y-minDistance);
#endif
            float limit=_ScaleLim*(1-_ScaleCompressionSlope);
            positiveScale=min(positiveScale,limit+positiveScale*_ScaleCompressionSlope);
            negativeScale=min(negativeScale,limit+negativeScale*_ScaleCompressionSlope);
            float positive=max(difference,0),negative=min(difference,0);
            difference=wpmean(positive,soft_lim(positive,positiveScale),cs.x)-wpmean(negative,soft_lim(negative,negativeScale),cs.y);
        }
        float3 result=d[0].y+difference+(original-d[0].y)*((d[0].y+max(difference*0.9,difference)*1.03+0.03)/(d[0].y+0.03));
        return float4(result,SampleMain(i.texcoord).a);
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
            #pragma fragment Frag0
            #pragma multi_compile_local_fragment _ FastOps
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #pragma vertex Vert
            #pragma fragment Frag1
            #pragma multi_compile_local_fragment _ FastOps
            ENDHLSL
        }
    }
}
