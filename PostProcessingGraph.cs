using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>Named outputs from earlier features in the same camera graph. Not persistent global textures.</summary>
public sealed class PostProcessingFrameTextures : ContextItem
{
    readonly Dictionary<string, TextureHandle> textures = new Dictionary<string, TextureHandle>();
    public bool TryGet(string name, out TextureHandle texture)
    {
        texture = default;
        return !string.IsNullOrEmpty(name) && textures.TryGetValue(name, out texture) && texture.IsValid();
    }
    public void Set(string name, TextureHandle texture)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("A texture name is required.", nameof(name));
        textures[name] = texture;
    }
    public override void Reset() => textures.Clear();
}

/// <summary>One camera-recording snapshot. Never store a live VolumeComponent in a render callback.</summary>
public sealed class PostProcessingParameters
{
    internal readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
    readonly List<KeyValuePair<string, bool>> keywords = new List<KeyValuePair<string, bool>>();
    static readonly Dictionary<string, int> ids = new Dictionary<string, int>();
    public static int Id(string name)
    {
        if (!ids.TryGetValue(name, out var id)) ids.Add(name, id = Shader.PropertyToID(name));
        return id;
    }
    public void Float(string name, float value) => block.SetFloat(Id(name), value);
    public void Int(string name, int value) => block.SetInteger(Id(name), value);
    public void Vector(string name, Vector4 value) => block.SetVector(Id(name), value);
    public void Color(string name, Color value) => block.SetColor(Id(name), value);
    public void Texture(string name, Texture value) => block.SetTexture(Id(name), value);
    public void Keyword(string name, bool enabled) => keywords.Add(new KeyValuePair<string, bool>(name, enabled));
    internal void ApplyKeywords(Material material)
    {
        foreach (var keyword in keywords) CoreUtils.SetKeyword(material, keyword.Key, keyword.Value);
    }
}

public static class PostProcessingGraph
{
    static readonly int MainTex = Shader.PropertyToID("_MainTex");
    static readonly int MainTexelSize = Shader.PropertyToID("_MainTex_TexelSize");
    static readonly int MainScale = Shader.PropertyToID("_MainTexScale");
    static readonly int BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");
    static readonly int DepthTex = Shader.PropertyToID("_CameraDepthTexture");
    static readonly int NormalTex = Shader.PropertyToID("_CameraNormalsTexture");

    public static TextureHandle CreateColor(RenderGraph graph, TextureHandle source, string name,
        int width = 0, int height = 0, GraphicsFormat format = GraphicsFormat.None, bool randomWrite = false)
    {
        var descriptor = graph.GetTextureDesc(source);
        descriptor.name = name;
        descriptor.depthBufferBits = DepthBits.None;
        descriptor.msaaSamples = MSAASamples.None;
        descriptor.bindTextureMS = false;
        descriptor.enableRandomWrite = randomWrite;
        descriptor.useMipMap = false;
        descriptor.autoGenerateMips = false;
        descriptor.memoryless = RenderTextureMemoryless.None;
        descriptor.clearBuffer = false;
        descriptor.filterMode = FilterMode.Bilinear;
        descriptor.wrapMode = TextureWrapMode.Clamp;
        if (format != GraphicsFormat.None) descriptor.colorFormat = format;
        if (width > 0 && height > 0)
        {
            descriptor.sizeMode = TextureSizeMode.Explicit;
            descriptor.width = Mathf.Max(1, width);
            descriptor.height = Mathf.Max(1, height);
            descriptor.useDynamicScale = false;
            descriptor.useDynamicScaleExplicit = false;
        }
        return graph.CreateTexture(descriptor);
    }

    class DrawData
    {
        public TextureHandle source, auxiliary, auxiliary2, depth, normals;
        public int auxiliaryId, auxiliary2Id;
        public Material material;
        public PostProcessingParameters parameters;
        public int shaderPass;
    }

    // Auxiliary reads are declared here, not smuggled through global shader properties.
    public static void Draw(RenderGraph graph, TextureHandle source, TextureHandle destination,
        Material material, PostProcessingParameters parameters, int shaderPass, string name,
        TextureHandle auxiliary = default, int auxiliaryId = 0,
        TextureHandle auxiliary2 = default, int auxiliary2Id = 0,
        TextureHandle depth = default, TextureHandle normals = default)
    {
        if (source.Equals(destination) || (auxiliary.IsValid() && auxiliary.Equals(destination)) ||
            (auxiliary2.IsValid() && auxiliary2.Equals(destination)))
            throw new InvalidOperationException(name + ": a sampled texture cannot also be the render attachment.");

        using (var builder = graph.AddRasterRenderPass<DrawData>(name, out var data))
        {
            data.source = source; data.auxiliary = auxiliary; data.auxiliary2 = auxiliary2;
            data.depth = depth; data.normals = normals;
            data.auxiliaryId = auxiliaryId; data.auxiliary2Id = auxiliary2Id;
            data.material = material; data.parameters = parameters; data.shaderPass = shaderPass;
            builder.UseTexture(source, AccessFlags.Read);
            if (auxiliary.IsValid()) builder.UseTexture(auxiliary, AccessFlags.Read);
            if (auxiliary2.IsValid()) builder.UseTexture(auxiliary2, AccessFlags.Read);
            if (depth.IsValid()) builder.UseTexture(depth, AccessFlags.Read);
            if (normals.IsValid()) builder.UseTexture(normals, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            builder.SetRenderFunc((DrawData pass, RasterGraphContext context) =>
            {
                var block = pass.parameters.block;
                Bind(block, MainTex, pass.source);
                RTHandle sourceHandle = pass.source;
                var scale = Scale(sourceHandle);
                block.SetVector(MainScale, scale);
                var width = Mathf.Max(1, sourceHandle.rt.width * scale.x);
                var height = Mathf.Max(1, sourceHandle.rt.height * scale.y);
                block.SetVector(MainTexelSize, new Vector4(1f / width, 1f / height, width, height));
                block.SetVector(BlitScaleBias, new Vector4(1, 1, 0, 0));
                if (pass.auxiliary.IsValid()) Bind(block, pass.auxiliaryId, pass.auxiliary);
                if (pass.auxiliary2.IsValid()) Bind(block, pass.auxiliary2Id, pass.auxiliary2);
                if (pass.depth.IsValid()) Bind(block, DepthTex, pass.depth);
                if (pass.normals.IsValid()) Bind(block, NormalTex, pass.normals);
                pass.parameters.ApplyKeywords(pass.material);
                // Uses URP's Blit.hlsl full-screen triangle and an independent property snapshot.
                context.cmd.DrawProcedural(Matrix4x4.identity, pass.material, pass.shaderPass,
                    MeshTopology.Triangles, 3, 1, block);
            });
        }
    }

    static readonly string[] auxiliaryNames = { "_TempTex", "_Temp1Tex", "_Temp2Tex", "_HighTex", "_TrashTex" };
    static void Bind(MaterialPropertyBlock block, int id, TextureHandle texture)
    {
        block.SetTexture(id, (Texture)texture);
        foreach (var name in auxiliaryNames)
        {
            if (id != PostProcessingParameters.Id(name)) continue;
            RTHandle handle = texture;
            var scale = Scale(handle);
            float width = Mathf.Max(1, handle.rt.width * scale.x);
            float height = Mathf.Max(1, handle.rt.height * scale.y);
            block.SetVector(PostProcessingParameters.Id(name + "Scale"), scale);
            block.SetVector(PostProcessingParameters.Id(name + "_TexelSize"), new Vector4(1f / width, 1f / height, width, height));
            break;
        }
    }

    public static Vector4 Scale(RTHandle handle)
    {
        var scale = handle.useScaling ? handle.rtHandleProperties.rtHandleScale : Vector4.one;
        return new Vector4(scale.x, scale.y, 0, 0);
    }

    class CopyData { public TextureHandle source; }
    public static void Copy(RenderGraph graph, TextureHandle source, TextureHandle destination, string name, bool persistentWrite = false)
    {
        if (source.Equals(destination)) return;
        using (var builder = graph.AddRasterRenderPass<CopyData>(name, out var data))
        {
            data.source = source;
            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            if (persistentWrite) builder.AllowPassCulling(false);
            builder.SetRenderFunc((CopyData pass, RasterGraphContext context) =>
            {
                RTHandle handle = pass.source;
                Blitter.BlitTexture(context.cmd, handle, Scale(handle), 0f, true);
            });
        }
    }
}
