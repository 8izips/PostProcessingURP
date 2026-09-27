using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AdaptiveSharpenRenderFeature : CustomRenderFeature<AdaptiveSharpen>
{

    protected override CustomPass<AdaptiveSharpen> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new AdaptiveSharpenPass(renderPassEvent);
    }
}

public class AdaptiveSharpenPass : CustomPass<AdaptiveSharpen>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/AdaptiveSharpen";
    public override string renderTag => "Render AdaptiveSharpen Effects";
    public AdaptiveSharpenPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, AdaptiveSharpen c, UniversalCameraData camera)
    {
        p.Float("_CurveHeight", c.curveHeight.value);
        p.Float("_CurveSlope", c.curveSlope.value);
        p.Float("_LightOvershoot", c.lightOvershoot.value);
        p.Float("_LightCompressionLow", c.lightCompressionLow.value);
        p.Float("_LightCompressionHigh", c.lightCompressionHigh.value);
        p.Float("_DarkOvershoot", c.darkOvershoot.value);
        p.Float("_DarkCompressionLow", c.darkCompressionLow.value);
        p.Float("_DarkCompressionHigh", c.darkCompressionHigh.value);
        p.Float("_ScaleLim", c.scaleLim.value);
        p.Float("_ScaleCompressionSlope", c.scaleCompressionSlope.value);
        p.Float("_PowerMeanP", Mathf.Max(0.0001f, c.powerMeanP.value));
        p.Keyword("FastOps", c.fastOperation.value);
    }

    protected override TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters p, AdaptiveSharpen component)
    {
        // Edge/luma data must not be clamped to an 8-bit camera format.
        var edges = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Edge-Luma",
            format: UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat);
        var output = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Output");
        PostProcessingGraph.Draw(graph, source, edges, material, p, 0, renderTag + " / Analyze");
        PostProcessingGraph.Draw(graph, source, output, material, p, 1, renderTag + " / Sharpen",
            edges, PostProcessingParameters.Id("_TempTex"));
        return output;
    }

}
