using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderFeature<T> : ScriptableRendererFeature where T : VolumeComponent, IPostProcessComponent
{
	public enum BufferType
	{
		CameraColor,
		Custom
	}

	[System.Serializable]
	public class Settings
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		public BufferType sourceType = BufferType.CameraColor;
		public BufferType destinationType = BufferType.CameraColor;
		public string sourceTexture = "_SourceTexture";
		public string destinationTexture = "_DestinationTexture";
	}
	public Settings settings = new Settings();

	CustomPass<T> pass;
	public override void Create()
	{
		pass = CreatePass(settings.renderPassEvent);
		pass.Init(pass.GetShaderPath());
	}

	protected override void Dispose(bool disposing)
	{
		pass?.Release();
	}

	protected virtual CustomPass<T> CreatePass(RenderPassEvent renderPassEvent) { return null; }

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		pass.Setup(settings);
		renderer.EnqueuePass(pass);
	}
}
