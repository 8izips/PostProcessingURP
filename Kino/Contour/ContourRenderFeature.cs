using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ContourRenderFeature : CustomRenderFeature<Contour>
{

    protected override CustomPass<Contour> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new ContourPass(renderPassEvent);
    }
}

public class ContourPass : CustomPass<Contour>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Contour";
    public override string renderTag => "Render Contour Effects";
    public ContourPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Contour c, UniversalCameraData camera)
    {
        p.Color("_Color", c.lineColor.value);
        p.Color("_Background", c.bgColor.value);
        p.Float("_Threshold", c.lowerThreshold.value);
        p.Float("_InvRange", 1f / Mathf.Max(0.0001f, c.upperThreshold.value - c.lowerThreshold.value));
        p.Float("_ColorSensitivity", c.colorSensitivity.value);
        p.Float("_DepthSensitivity", c.depthSensitivity.value * 2f);
        p.Float("_NormalSensitivity", c.normalSensitivity.value);
        p.Float("_InvFallOff", 1f / Mathf.Max(0.0001f, c.falloffDepth.value));
        p.Keyword("_CONTOUR_COLOR", c.colorSensitivity.value > 0);
        p.Keyword("_CONTOUR_DEPTH", c.depthSensitivity.value > 0);
        p.Keyword("_CONTOUR_NORMAL", c.normalSensitivity.value > 0);
    }

    public override ScriptableRenderPassInput GetInputRequirements(Contour c)
    {
        var input = ScriptableRenderPassInput.None;
        if (c.depthSensitivity.value > 0) input |= ScriptableRenderPassInput.Depth;
        if (c.normalSensitivity.value > 0) input |= ScriptableRenderPassInput.Normal;
        return input;
    }

}
