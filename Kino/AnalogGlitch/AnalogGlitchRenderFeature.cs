using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class AnalogGlitchRenderFeature : CustomRenderFeature<AnalogGlitch>
{

    protected override CustomPass<AnalogGlitch> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new AnalogGlitchPass(renderPassEvent);
    }
}

public class AnalogGlitchPass : CustomPass<AnalogGlitch>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/AnalogGlitch";
    public override string renderTag => "Render AnalogGlitch Effects";
    public AnalogGlitchPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, AnalogGlitch c, UniversalCameraData camera)
    {
        int id = camera.camera.GetInstanceID();
        if (!phases.TryGetValue(id, out var phase))
        {
            phase = new Phase();
            phases.Add(id, phase);
        }
        if (phase.frame != Time.frameCount)
        {
            phase.time += Time.deltaTime * c.verticalJump.value * 11.3f;
            phase.frame = Time.frameCount;
        }
        float threshold = Mathf.Clamp01(1f - c.scanLineJitter.value * 1.2f);
        float displacement = 0.002f + Mathf.Pow(c.scanLineJitter.value, 3) * 0.05f;
        p.Vector("_ScanLineJitter", new Vector4(displacement, threshold, 0, 0));
        p.Vector("_VerticalJump", new Vector4(c.verticalJump.value, phase.time, 0, 0));
        p.Float("_HorizontalShake", c.horizontalShake.value * 0.2f);
        p.Vector("_ColorDrift", new Vector4(c.colorDrift.value * 0.04f, Time.time * 606.11f, 0, 0));
    }

    sealed class Phase { public float time; public int frame = -1; }
    readonly System.Collections.Generic.Dictionary<int, Phase> phases = new System.Collections.Generic.Dictionary<int, Phase>();
    public override void Release() { phases.Clear(); base.Release(); }

}
