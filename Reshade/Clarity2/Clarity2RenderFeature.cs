using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class Clarity2RenderFeature : CustomRenderFeature<Clarity2>
{

    protected override CustomPass<Clarity2> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new Clarity2Pass(renderPassEvent);
    }
}

public class Clarity2Pass : CustomPass<Clarity2>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Clarity2";
    public override string renderTag => "Render Clarity2 Effects";
    public Clarity2Pass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Clarity2 c, UniversalCameraData camera)
    {
        p.Int("_Offset", Mathf.Max(1, c.offset.value));
        p.Int("_BlendIfDark", c.blendIfDark.value);
        p.Int("_BlendIfLight", c.blendIfLight.value);
        p.Float("_BlendIfRange", Mathf.Max(0.0001f, c.blendIfRange.value));
        p.Float("_Strength", c.strength.value);
        p.Float("_MaskContrast", c.maskContrast.value);
        p.Float("_DarkIntensity", c.darkIntensity.value);
        p.Float("_LightIntensity", c.lightIntensity.value);
        p.Float("_DitherStrength", c.ditherStrength.value);
        for (int i = 0; i < radiusKeywords.Length; i++) p.Keyword(radiusKeywords[i], i == Mathf.Clamp(c.radius.value, 0, 4));
        p.Keyword("BLENDMODE_SOFTLIGHT", c.blendMode.value == Clarity2.BlendMode.SoftLight);
        p.Keyword("BLENDMODE_OVERLAY", c.blendMode.value == Clarity2.BlendMode.Overlay);
        p.Keyword("BLENDMODE_HARDLIGHT", c.blendMode.value == Clarity2.BlendMode.HardLight);
        p.Keyword("BLENDMODE_MULTIPLY", c.blendMode.value == Clarity2.BlendMode.Multiply);
        p.Keyword("BLENDMODE_VIVIDLIGHT", c.blendMode.value == Clarity2.BlendMode.VividLight);
        p.Keyword("BLENDMODE_LINEARLIGHT", c.blendMode.value == Clarity2.BlendMode.LinearLight);
        p.Keyword("BLENDMODE_ADDITION", c.blendMode.value == Clarity2.BlendMode.Addition);
    }

    static readonly string[] radiusKeywords = { "RADIUS_ZERO", "RADIUS_ONE", "RADIUS_TWO", "RADIUS_THEREE", "RADIUS_FOUR" };
    protected override TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters p, Clarity2 component)
    {
        var desc = graph.GetTextureDesc(source);
        var cameraDesc = frameData.Get<UniversalCameraData>().cameraTargetDescriptor;
        int w = Mathf.Max(1, (desc.width > 0 ? desc.width : cameraDesc.width) / 2);
        int h = Mathf.Max(1, (desc.height > 0 ? desc.height : cameraDesc.height) / 2);
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        var down = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Downsample", w, h, format);
        var vertical = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Vertical", w, h, format);
        var horizontal = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Horizontal", w, h, format);
        var output = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Output");
        PostProcessingGraph.Draw(graph, source, down, material, p, 0, renderTag + " / Downsample");
        PostProcessingGraph.Draw(graph, down, vertical, material, p, 1, renderTag + " / Vertical");
        PostProcessingGraph.Draw(graph, vertical, horizontal, material, p, 2, renderTag + " / Horizontal");
        PostProcessingGraph.Draw(graph, source, output, material, p, 3, renderTag + " / Composite",
            horizontal, PostProcessingParameters.Id("_Temp1Tex"));
        return output;
    }

}
