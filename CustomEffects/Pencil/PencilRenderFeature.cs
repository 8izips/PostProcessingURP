using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PencilRenderFeature : CustomRenderFeature<Pencil>
{
	protected override CustomPass<Pencil> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new PencilPass(renderPassEvent);
	}
}

public class PencilPass : CustomPass<Pencil>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Pencil";
	public override string renderTag => "Render Pencil Effects";

	public PencilPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int GradThreshold = Shader.PropertyToID("_GradThreshold");
		internal static readonly int ColorThreshold = Shader.PropertyToID("_ColorThreshold");
		internal static readonly int Sensivity = Shader.PropertyToID("_Sensivity");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.GradThreshold, volumeComponent.gradThreshold.value);
		material.SetFloat(ShaderPropertyID.ColorThreshold, volumeComponent.colorThreshold.value);
		material.SetFloat(ShaderPropertyID.Sensivity, volumeComponent.sensivity.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}