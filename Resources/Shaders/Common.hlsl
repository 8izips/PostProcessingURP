#ifndef POSTPROCESSURP_COMMON_INCLUDED
#define POSTPROCESSURP_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#define v2f Varyings

// Logical viewport UV -> allocated RTHandle UV, clamped to the visible region.
float2 PPTextureUV(float2 uv, float4 scale, float4 texelSize)
{
    return clamp(uv, 0.5 * texelSize.xy, 1.0 - 0.5 * texelSize.xy) * scale.xy;
}
#define PP_DECLARE_TEXTURE(name) TEXTURE2D_X(name); float4 name##Scale; float4 name##_TexelSize
#define PP_SAMPLE_TEXTURE(name, uv) SAMPLE_TEXTURE2D_X_LOD(name, sampler_LinearClamp, PPTextureUV(uv, name##Scale, name##_TexelSize), 0)
PP_DECLARE_TEXTURE(_MainTex);
float4 SampleMain(float2 uv) { return PP_SAMPLE_TEXTURE(_MainTex, uv); }
float2 PPSafeNormalize(float2 value) { return value * rsqrt(max(dot(value, value), 1e-12)); }
float PPEyeDepth(float rawDepth)
{
    if (unity_OrthoParams.w > 0.5)
    {
#if UNITY_REVERSED_Z
        rawDepth = 1.0 - rawDepth;
#endif
        return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
    }
    return LinearEyeDepth(rawDepth, _ZBufferParams);
}
#endif
