using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class StreakRenderFeature : CustomRenderFeature<Streak>
{

    protected override CustomPass<Streak> CreatePass(RenderPassEvent renderPassEvent)
    {
        return new StreakPass(renderPassEvent);
    }
}

public class StreakPass : CustomPass<Streak>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/Streak";
    public override string renderTag => "Render Streak Effects";
    public StreakPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    protected override void ConfigureParameters(PostProcessingParameters p, Streak c, UniversalCameraData camera)
    {
        p.Float("_Threshold", c.threshold.value);
        p.Float("_Stretch", c.stretch.value);
        p.Float("_Intensity", c.intensity.value);
        p.Color("_Color", c.tint.value);
    }

    protected override TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters p, Streak component)
    {
        var desc = graph.GetTextureDesc(source);
        var cameraDesc = frameData.Get<UniversalCameraData>().cameraTargetDescriptor;
        int width = Mathf.Max(1, desc.width > 0 ? desc.width : cameraDesc.width);
        int height = Mathf.Max(1, (desc.height > 0 ? desc.height : cameraDesc.height) / 2);
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        var levels = new TextureHandle[16];
        var widths = new int[16];
        widths[0] = width;
        levels[0] = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Prefilter", width, height, format);
        PostProcessingGraph.Draw(graph, source, levels[0], material, p, 0, renderTag + " / Prefilter");
        int count = 1;
        while (count < levels.Length && width / 2 > 7)
        {
            width /= 2;
            widths[count] = width;
            levels[count] = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Down " + count, width, height, format);
            PostProcessingGraph.Draw(graph, levels[count - 1], levels[count], material, p, 1, renderTag + " / Down " + count);
            count++;
        }
        var last = levels[count - 1];
        for (int i = count - 2; i >= 1; i--)
        {
            var up = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Up " + i, widths[i], height, format);
            PostProcessingGraph.Draw(graph, last, up, material, p, 2, renderTag + " / Up " + i,
                levels[i], PostProcessingParameters.Id("_HighTex"));
            last = up;
        }
        var output = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Output");
        PostProcessingGraph.Draw(graph, last, output, material, p, 3, renderTag + " / Composite",
            source, PostProcessingParameters.Id("_HighTex"));
        // RenderGraph owns every transient level and frees it after its last reader.
        return output;
    }

}
