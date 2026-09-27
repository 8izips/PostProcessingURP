using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AdaptiveFogRenderFeature : CustomRenderFeature<AdaptiveFog>
{

    protected override CustomPass<AdaptiveFog> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new AdaptiveFogPass(renderPassEvent);
    }
}

public class AdaptiveFogPass : CustomPass<AdaptiveFog>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/AdaptiveFog";
    public override string renderTag => "Render AdaptiveFog Effects";
    public AdaptiveFogPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, AdaptiveFog c, UniversalCameraData camera)
    {
        p.Float("_MaxFogFactor", c.maxFogFactor.value);
        p.Float("_FogCurve", c.fogCurve.value);
        p.Float("_FogStart", c.fogStart.value);
        p.Float("_BloomThreshold", c.bloomThreshold.value);
        p.Float("_BloomPower", c.bloomPower.value);
        p.Float("_BloomWidth", Mathf.Max(0.0001f, c.bloomWidth.value));
        p.Color("_FogColor", c.fogColor.value);
    }

    public override ScriptableRenderPassInput GetInputRequirements(AdaptiveFog c) => ScriptableRenderPassInput.Depth;

}
