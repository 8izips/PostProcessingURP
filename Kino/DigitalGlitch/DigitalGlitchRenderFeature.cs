using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class DigitalGlitchRenderFeature : CustomRenderFeature<DigitalGlitch>
{
    protected override CustomPass<DigitalGlitch> CreatePass(RenderPassEvent renderPassEvent) => new DigitalGlitchPass(renderPassEvent);
}

public class DigitalGlitchPass : CustomPass<DigitalGlitch>
{
    public override string GetShaderPath() => "Hidden/PostProcessing/DigitalGlitch";
    public override string renderTag => "Render DigitalGlitch Effects";
    public DigitalGlitchPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

    sealed class History
    {
        public Camera camera;
        public RTHandle first, second;
        public Texture2D noise;
        public RenderTextureDescriptor descriptor;
        public bool valid;
        public int lastFrame = -1;
        public bool chooseFirst;
        public System.Random random;
        public readonly Color32[] pixels = new Color32[64 * 32];
        public void Dispose()
        {
            first?.Release(); second?.Release();
            first = second = null;
            CoreUtils.Destroy(noise); noise = null;
            valid = false;
        }
    }
    readonly Dictionary<int, History> histories = new Dictionary<int, History>();
    readonly List<int> stale = new List<int>();

    protected override TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters p, DigitalGlitch component)
    {
        var cameraData = frameData.Get<UniversalCameraData>();
        var camera = cameraData.camera;
        int id = camera.GetInstanceID();
        stale.Clear();
        foreach (var item in histories)
            if (item.Value.camera == null || Time.frameCount - item.Value.lastFrame > 300) stale.Add(item.Key);
        foreach (int key in stale) { histories[key].Dispose(); histories.Remove(key); }
        if (!histories.TryGetValue(id, out var history) || history.camera != camera)
        {
            history?.Dispose();
            history = new History { camera = camera, random = new System.Random(id) };
            histories[id] = history;
        }

        var inputDesc = graph.GetTextureDesc(source);
        var descriptor = cameraData.cameraTargetDescriptor;
        descriptor.width = Mathf.Max(1, inputDesc.width > 0 ? inputDesc.width : descriptor.width);
        descriptor.height = Mathf.Max(1, inputDesc.height > 0 ? inputDesc.height : descriptor.height);
        descriptor.graphicsFormat = inputDesc.colorFormat;
        descriptor.dimension = inputDesc.dimension;
        descriptor.volumeDepth = Mathf.Max(1, inputDesc.slices);
        descriptor.depthBufferBits = 0;
        descriptor.msaaSamples = 1;
        descriptor.bindMS = false;
        descriptor.useDynamicScale = false;
        descriptor.useMipMap = false;
        descriptor.autoGenerateMips = false;
        descriptor.enableRandomWrite = false;
        descriptor.memoryless = RenderTextureMemoryless.None;
        if (history.first == null || !history.descriptor.Equals(descriptor))
        {
            history.Dispose();
            history.descriptor = descriptor;
            history.first = Allocate(descriptor, "DigitalGlitch history A / " + id);
            history.second = Allocate(descriptor, "DigitalGlitch history B / " + id);
            history.noise = new Texture2D(64, 32, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            UpdateNoise(history);
        }
        var first = graph.ImportTexture(history.first);
        var second = graph.ImportTexture(history.second);
        int frame = Time.frameCount;
        bool firstUse = !history.valid || frame - history.lastFrame > 1;
        if (firstUse || (frame != history.lastFrame && frame % 13 == 0))
            PostProcessingGraph.Copy(graph, source, first, renderTag + " / Save A", true);
        if (firstUse || (frame != history.lastFrame && frame % 73 == 0))
            PostProcessingGraph.Copy(graph, source, second, renderTag + " / Save B", true);
        if (frame != history.lastFrame)
        {
            if (history.random.NextDouble() > Mathf.Lerp(0.9f, 0.5f, component.intensity.value)) UpdateNoise(history);
            history.chooseFirst = history.random.NextDouble() > 0.5;
        }
        history.lastFrame = frame;
        history.valid = true;
        p.Float("_Intensity", component.intensity.value);
        p.Texture("_NoiseTex", history.noise);
        var output = PostProcessingGraph.CreateColor(graph, source, renderTag + " / Output");
        PostProcessingGraph.Draw(graph, source, output, material, p, 0, renderTag,
            history.chooseFirst ? first : second, PostProcessingParameters.Id("_TrashTex"));
        return output;
    }

    static RTHandle Allocate(RenderTextureDescriptor descriptor, string name) => RTHandles.Alloc(
        descriptor.width, descriptor.height, slices: descriptor.volumeDepth, depthBufferBits: DepthBits.None,
        colorFormat: descriptor.graphicsFormat, filterMode: FilterMode.Point, wrapMode: TextureWrapMode.Clamp,
        dimension: descriptor.dimension, useDynamicScale: false, name: name);

    static void UpdateNoise(History history)
    {
        Color32 color = default;
        for (int i = 0; i < history.pixels.Length; i++)
        {
            if (i == 0 || history.random.NextDouble() > 0.89)
                color = new Color32((byte)history.random.Next(256), (byte)history.random.Next(256),
                    (byte)history.random.Next(256), (byte)history.random.Next(256));
            history.pixels[i] = color;
        }
        history.noise.SetPixels32(history.pixels);
        history.noise.Apply(false, false);
    }

    public override void Release()
    {
        foreach (var history in histories.Values) history.Dispose();
        histories.Clear(); stale.Clear();
        base.Release();
    }
}
