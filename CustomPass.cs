using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPass<T> : ScriptableRenderPass where T : VolumeComponent, IPostProcessComponent
{
	protected T volumeComponent;
	CustomRenderFeature<T>.Settings settings;

	public virtual string GetShaderPath() { return null; }
	public virtual string renderTag => "Render Custom Effects";
	public FilterMode filterMode { get; set; }

	protected Material material;
	protected RenderTargetIdentifier source;
	protected RenderTargetIdentifier destination;

	public CustomPass(RenderPassEvent renderPassEvent)
	{
		this.renderPassEvent = renderPassEvent;		
	}

	public void Setup(CustomRenderFeature<T>.Settings featureSettings)
	{
		settings = featureSettings;
		renderPassEvent = featureSettings.renderPassEvent;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		blitTargetDescriptor.depthBufferBits = 0;

		var isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
			(settings.sourceType == CustomRenderFeature<T>.BufferType.CameraColor || settings.sourceTexture == settings.destinationTexture);

		var renderer = renderingData.cameraData.renderer;

		if (settings.sourceType == CustomRenderFeature<T>.BufferType.CameraColor) {
			source = renderer.cameraColorTargetHandle;
		}
		else {
			var sourceId = Shader.PropertyToID(settings.sourceTexture);
			cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode);
			source = new RenderTargetIdentifier(sourceId);
		}

		if (isSourceAndDestinationSameTarget) {
			destination = source;
		}
		else {
			var destinationId = Shader.PropertyToID(settings.destinationTexture);
			cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
			destination = new RenderTargetIdentifier(destinationId);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!renderingData.cameraData.postProcessEnabled || !IsValid())
			return;

		var stack = VolumeManager.instance.stack;
		volumeComponent = stack.GetComponent<T>();
		if (volumeComponent == null || !volumeComponent.IsActive()) return;

		var cmd = CommandBufferPool.Get(renderTag);
		Render(cmd, ref renderingData);
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	public virtual bool IsValid()
	{
		return material != null;
	}

	public virtual void Init(string shaderPath)
	{
		var shader = Shader.Find(shaderPath);
		if (shader == null)
			return;

		material = CoreUtils.CreateEngineMaterial(shader);
	}

	public virtual void Release() { if (material != null) CoreUtils.Destroy(material); }
	protected virtual void Render(CommandBuffer cmd, ref RenderingData renderingData) { }
}
