using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class PencilRenderFeature : CustomRenderFeature<Pencil>
{

    protected override CustomPass<Pencil> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new PencilPass(renderPassEvent);
    }
}

public class PencilPass : CustomPass<Pencil>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Pencil";
    public override string renderTag => "Render Pencil Effects";
    public PencilPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Pencil c, UniversalCameraData camera)
    {
        p.Float("_GradThreshold", c.gradThreshold.value);
        p.Float("_ColorThreshold", c.colorThreshold.value);
        p.Float("_Sensivity", c.sensivity.value);
    }

}
