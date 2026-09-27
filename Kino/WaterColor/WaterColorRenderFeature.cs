using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class WaterColorRenderFeature : CustomRenderFeature<WaterColor>
{
    [SerializeField] public Texture2D noiseTex;
    protected override CustomPass<WaterColor> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new WaterColorPass(renderPassEvent) { noiseTex = noiseTex };
    }
}

public class WaterColorPass : CustomPass<WaterColor>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/WaterColor";
    public override string renderTag => "Render WaterColor Effects";
    public WaterColorPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, WaterColor c, UniversalCameraData camera)
    {
        p.Color("_FillColor", c.fillColor.value);
        p.Color("_EdgeColor", c.edgeColor.value);
        p.Vector("_EffectParams", new Vector4(c.edgeContrast.value, c.blurWidth.value,
            Mathf.Exp(Mathf.Clamp(c.blurFrequency.value - 0.5f, -20f, 20f)) * 6f, c.hueShift.value));
        p.Float("_Interval", c.interval.value);
        p.Int("_Iteration", Mathf.Clamp(c.iteration.value, 1, 128));
        p.Texture("_NoiseTex", noiseTex != null ? noiseTex : Texture2D.grayTexture);
    }
    public Texture2D noiseTex;
    public override void Init(string shaderPath)
    {
        base.Init(shaderPath);
        if (noiseTex == null) noiseTex = Resources.Load<Texture2D>("Textures/WaterColorNoise");
        if (noiseTex == null) Warn("noise", "WaterColorNoise is missing; a neutral texture will be used.");
    }

}
