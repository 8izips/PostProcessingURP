using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ADOFRenderFeature : CustomRenderFeature<ADOF>
{

    protected override CustomPass<ADOF> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new ADOFPass(renderPassEvent);
    }
}

public class ADOFPass : CustomPass<ADOF>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/ADOF";
    public override string renderTag => "Render ADOF Effects";
    public ADOFPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, ADOF c, UniversalCameraData camera)
    {
        // ADOF was an unfinished placeholder, not a depth-of-field implementation.
    }
    public override bool IsValid() => false;
}
