using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleRenderFeature : CustomRenderFeature<Grayscale>
{
	protected override CustomPass<Grayscale> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new GrayscalePass(renderPassEvent);
	}
}

public class GrayscalePass : CustomPass<Grayscale>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Grayscale";
	public override string renderTag => "Render Grayscale Effects";

	public GrayscalePass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int Blend = Shader.PropertyToID("_Blend");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.Blend, volumeComponent.blendRate.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}