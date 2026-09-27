using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionRenderFeature : CustomRenderFeature<EdgeDetection>
{

    protected override CustomPass<EdgeDetection> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new EdgeDetectionPass(renderPassEvent);
    }
}

public class EdgeDetectionPass : CustomPass<EdgeDetection>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/EdgeDetection";
    public override string renderTag => "Render EdgeDetection Effects";
    public EdgeDetectionPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, EdgeDetection c, UniversalCameraData camera)
    {
        p.Float("_Intensity", c.intensity.value);
        p.Float("_Thickness", Mathf.Max(0, c.thickness.value));
        var normal = c.normalThreshold.value;
        var depth = c.depthThreshold.value;
        p.Vector("_Threshold", new Vector4(Mathf.Cos(normal.y * Mathf.Deg2Rad),
            Mathf.Cos(normal.x * Mathf.Deg2Rad), depth.x, depth.y));
        p.Color("_Color", c.color.value);
    }

    public override ScriptableRenderPassInput GetInputRequirements(EdgeDetection c) =>
        ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;

}
