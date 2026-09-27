using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GrayscaleCSRenderFeature : CustomRenderFeature<GrayscaleCS>
{
    [SerializeField] ComputeShader shader;
    protected override CustomPass<GrayscaleCS> CreatePass(RenderPassEvent renderPassEvent) =>
        new GrayscaleCSPass(renderPassEvent) { computeShader = shader };
}

public class GrayscaleCSPass : CustomPass<GrayscaleCS>
{
    public ComputeShader computeShader;
    int kernel2D = -1;
    int kernelArray = -1;
    public override string GetShaderPath() => "Hidden/PostProcessing/Grayscale";
    public override string renderTag => "Render Grayscale Compute Effects";
    public GrayscaleCSPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    public override void Init(string shaderPath)
    {
        base.Init(shaderPath); // Raster fallback also covers missing assets / unsupported devices.
        if (computeShader == null) computeShader = Resources.Load<ComputeShader>("Shaders/GrayscaleCS");
        if (computeShader == null || !SystemInfo.supportsComputeShaders) return;
        if (computeShader.HasKernel("GrayscaleCS")) kernel2D = computeShader.FindKernel("GrayscaleCS");
        if (computeShader.HasKernel("GrayscaleCSArray")) kernelArray = computeShader.FindKernel("GrayscaleCSArray");
    }

    protected override void ConfigureParameters(PostProcessingParameters p, GrayscaleCS c, UniversalCameraData camera) =>
        p.Float("_Blend", c.blendRate.value);

    class ComputeData
    {
        public TextureHandle source, destination;
        public ComputeShader shader;
        public int kernel, groupsX, groupsY, groupsZ, width, height, slices;
        public bool array;
        public float blend;
    }

    protected override TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters p, GrayscaleCS component)
    {
        var descriptor = graph.GetTextureDesc(source);
        bool array = descriptor.dimension == TextureDimension.Tex2DArray;
        int kernel = array ? kernelArray : kernel2D;
        var format = GraphicsFormat.R16G16B16A16_SFloat;
        if (computeShader == null || kernel < 0 || !SystemInfo.supportsComputeShaders ||
            !SystemInfo.IsFormatSupported(format, FormatUsage.LoadStore) ||
            (descriptor.dimension != TextureDimension.Tex2D && !array))
        {
            Warn("fallback", "Compute/UAV support is unavailable; using the raster Grayscale implementation.");
            return base.RecordEffect(graph, frameData, source, p, component);
        }

        var camera = frameData.Get<UniversalCameraData>().cameraTargetDescriptor;
        int width = Mathf.Max(1, descriptor.width > 0 ? descriptor.width : camera.width);
        int height = Mathf.Max(1, descriptor.height > 0 ? descriptor.height : camera.height);
        int slices = array ? Mathf.Max(1, descriptor.slices) : 1;
        // Resolve MSAA and normalize the viewport into a fixed-size, linear input.
        // Compute performs integer loads, unlike raster sampling with RTHandle scaling.
        var input = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Input", width, height, format);
        var output = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Output", width, height, format, true);
        PostProcessingGraph.Copy(graph, source, input, renderTag + " / Resolve");
        computeShader.GetKernelThreadGroupSizes(kernel, out uint tx, out uint ty, out uint tz);
        using (var builder = graph.AddComputePass<ComputeData>(renderTag, out var data))
        {
            data.source = input; data.destination = output; data.shader = computeShader;
            data.kernel = kernel; data.array = array; data.blend = component.blendRate.value;
            data.width = width; data.height = height; data.slices = slices;
            data.groupsX = (width + (int)tx - 1) / (int)tx;
            data.groupsY = (height + (int)ty - 1) / (int)ty;
            data.groupsZ = (slices + (int)tz - 1) / (int)tz;
            builder.UseTexture(input, AccessFlags.Read);
            builder.UseTexture(output, AccessFlags.Write);
            builder.SetRenderFunc((ComputeData pass, ComputeGraphContext context) =>
            {
                context.cmd.SetComputeTextureParam(pass.shader, pass.kernel, pass.array ? SourceArray : Source, pass.source);
                context.cmd.SetComputeTextureParam(pass.shader, pass.kernel, pass.array ? ResultArray : Result, pass.destination);
                context.cmd.SetComputeFloatParam(pass.shader, Blend, pass.blend);
                context.cmd.SetComputeVectorParam(pass.shader, Size, new Vector4(pass.width, pass.height, pass.slices, 0));
                context.cmd.DispatchCompute(pass.shader, pass.kernel, pass.groupsX, pass.groupsY, pass.groupsZ);
            });
        }
        return output;
    }
    static readonly int Source = Shader.PropertyToID("_SourceTex");
    static readonly int Result = Shader.PropertyToID("_ResultTex");
    static readonly int SourceArray = Shader.PropertyToID("_SourceArray");
    static readonly int ResultArray = Shader.PropertyToID("_ResultArray");
    static readonly int Blend = Shader.PropertyToID("_Blend");
    static readonly int Size = Shader.PropertyToID("_TextureSize");
}
