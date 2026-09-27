using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class FakeHDRRenderFeature : CustomRenderFeature<FakeHDR>
{

    protected override CustomPass<FakeHDR> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new FakeHDRPass(renderPassEvent);
    }
}

public class FakeHDRPass : CustomPass<FakeHDR>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/FakeHDR";
    public override string renderTag => "Render FakeHDR Effects";
    public FakeHDRPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, FakeHDR c, UniversalCameraData camera)
    {
        p.Float("_HDRPower", c.power.value);
        p.Float("_Radius1", c.radius1.value);
        p.Float("_Radius2", c.radius2.value);
    }

}
