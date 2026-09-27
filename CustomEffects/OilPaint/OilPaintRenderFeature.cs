using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OilPaintRenderFeature : CustomRenderFeature<OilPaint>
{

    protected override CustomPass<OilPaint> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new OilPaintPass(renderPassEvent);
    }
}

public class OilPaintPass : CustomPass<OilPaint>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/OilPaint";
    public override string renderTag => "Render OilPaint Effects";
    public OilPaintPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, OilPaint c, UniversalCameraData camera)
    {
        p.Int("_Radius", Mathf.Clamp(c.radius.value, 1, 16));
    }

}
