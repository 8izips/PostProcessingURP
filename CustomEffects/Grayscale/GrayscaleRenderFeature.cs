using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderFeature : CustomRenderFeature<Grayscale>
{

    protected override CustomPass<Grayscale> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new GrayscalePass(renderPassEvent);
    }
}

public class GrayscalePass : CustomPass<Grayscale>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Grayscale";
    public override string renderTag => "Render Grayscale Effects";
    public GrayscalePass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Grayscale c, UniversalCameraData camera)
    {
        p.Float("_Blend", c.blendRate.value);
    }

}
