using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class CustomPass<T> : ScriptableRenderPass where T : VolumeComponent, IPostProcessComponent
{
    protected Material material;
    CustomRenderFeature<T>.Settings settings;
    readonly HashSet<string> warnings = new HashSet<string>();

    public virtual string GetShaderPath() => null;
    public virtual string renderTag => "Render Custom Effects";
    public FilterMode filterMode { get; set; } = FilterMode.Bilinear;

    public CustomPass(RenderPassEvent renderPassEvent)
    {
        this.renderPassEvent = renderPassEvent;
        requiresIntermediateTexture = true;
    }

    public void Setup(CustomRenderFeature<T>.Settings featureSettings)
    {
        settings = featureSettings;
        renderPassEvent = settings.renderPassEvent;
    }

    public virtual ScriptableRenderPassInput GetInputRequirements(T component) => ScriptableRenderPassInput.None;
    public virtual bool IsValid() => material != null;

    public virtual void Init(string shaderPath)
    {
        CoreUtils.Destroy(material);
        material = null;
        if (string.IsNullOrEmpty(shaderPath)) return;
        var shader = Shader.Find(shaderPath);
        if (shader == null || !shader.isSupported)
        {
            Warn("shader", "Shader not found or unsupported: " + shaderPath);
            return;
        }
        material = CoreUtils.CreateEngineMaterial(shader);
    }

    public virtual void Release()
    {
        CoreUtils.Destroy(material);
        material = null;
        warnings.Clear();
    }

    protected void Warn(string key, string message)
    {
        if (warnings.Add(key)) Debug.LogWarning("PostProcessingURP/" + typeof(T).Name + ": " + message);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        var camera = frameData.Get<UniversalCameraData>();
        var resources = frameData.Get<UniversalResourceData>();
        var component = VolumeManager.instance.stack.GetComponent<T>();
        if (settings == null || !IsValid() || !camera.postProcessEnabled ||
            component == null || !component.active || !component.IsActive()) return;

        // Late injection can still make the active target the backbuffer. Never sample it.
        if (resources.isActiveTargetBackBuffer)
        {
            Warn("backbuffer", "The active target is the backbuffer. Use BeforeRenderingPostProcessing (recommended) or an earlier intermediate-color event.");
            return;
        }

        var textures = frameData.GetOrCreate<PostProcessingFrameTextures>();
        TextureHandle source;
        if (settings.sourceType == CustomRenderFeature<T>.BufferType.CameraColor)
            source = resources.activeColorTexture;
        else if (!textures.TryGet(settings.sourceTexture, out source))
        {
            Warn("source:" + settings.sourceTexture, "No earlier RenderGraph producer for custom input '" + settings.sourceTexture + "'. The effect was skipped; no uninitialized input was allocated.");
            return;
        }
        if (!source.IsValid()) return;
        if (settings.destinationType == CustomRenderFeature<T>.BufferType.Custom && string.IsNullOrEmpty(settings.destinationTexture))
        {
            Warn("destination", "A custom destination needs a non-empty name.");
            return;
        }

        var parameters = new PostProcessingParameters();
        ConfigureParameters(parameters, component, camera);
        var result = RecordEffect(renderGraph, frameData, source, parameters, component);
        if (!result.IsValid()) return;

        if (settings.destinationType == CustomRenderFeature<T>.BufferType.Custom)
        {
            // Handles are scoped to THIS camera graph; Reset clears them every frame.
            textures.Set(settings.destinationTexture, result);
        }
        else if (renderPassEvent < RenderPassEvent.BeforeRenderingPostProcessing || !camera.resolveFinalTarget)
        {
            // Preserve the shared stack/MSAA target for subsequent geometry or cameras.
            // This copy is safe: the sampled result and attachment are distinct textures.
            PostProcessingGraph.Copy(renderGraph, result, resources.activeColorTexture, renderTag + " / Camera resolve");
        }
        else
        {
            // Normal post-processing path: hand off the new color without copying back.
            resources.cameraColor = result;
        }
    }

    protected virtual void ConfigureParameters(PostProcessingParameters parameters, T component, UniversalCameraData camera) { }

    protected virtual TextureHandle RecordEffect(RenderGraph graph, ContextContainer frameData,
        TextureHandle source, PostProcessingParameters parameters, T component)
    {
        var destination = PostProcessingGraph.CreateColor(graph, source, renderTag);
        var resources = frameData.Get<UniversalResourceData>();
        var requirements = GetInputRequirements(component);
        var depth = (requirements & ScriptableRenderPassInput.Depth) != 0 ? resources.cameraDepthTexture : default;
        var normals = (requirements & ScriptableRenderPassInput.Normal) != 0 ? resources.cameraNormalsTexture : default;
        if (((requirements & ScriptableRenderPassInput.Depth) != 0 && !depth.IsValid()) ||
            ((requirements & ScriptableRenderPassInput.Normal) != 0 && !normals.IsValid()))
        {
            Warn("inputs", "Required depth/normal texture is unavailable at this injection point. Use BeforeRenderingPostProcessing.");
            return source;
        }
        PostProcessingGraph.Draw(graph, source, destination, material, parameters, 0, renderTag,
            depth: depth, normals: normals);
        return destination;
    }
}
