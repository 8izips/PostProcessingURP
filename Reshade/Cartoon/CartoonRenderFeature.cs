using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CartoonRenderFeature : CustomRenderFeature<Cartoon>
{

    protected override CustomPass<Cartoon> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new CartoonPass(renderPassEvent);
    }
}

public class CartoonPass : CustomPass<Cartoon>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Cartoon";
    public override string renderTag => "Render Cartoon Effects";
    public CartoonPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Cartoon c, UniversalCameraData camera)
    {
        p.Float("_Power", c.power.value);
        p.Float("_EdgeSlope", c.edgeSlope.value);
    }

}
