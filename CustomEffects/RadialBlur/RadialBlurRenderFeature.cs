using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RadialBlurRenderFeature : CustomRenderFeature<RadialBlur>
{

    protected override CustomPass<RadialBlur> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new RadialBlurPass(renderPassEvent);
    }
}

public class RadialBlurPass : CustomPass<RadialBlur>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/RadialBlur";
    public override string renderTag => "Render RadialBlur Effects";
    public RadialBlurPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, RadialBlur c, UniversalCameraData camera)
    {
        p.Float("_BlurRadius", c.blurRadius.value);
        p.Float("_RadialCenterX", c.radialCenterX.value);
        p.Float("_RadialCenterY", c.radialCenterY.value);
        p.Int("_Iteration", Mathf.Clamp(c.iteration.value, 1, 128));
    }

}
