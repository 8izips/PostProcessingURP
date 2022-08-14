using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadialBlurRenderFeature : CustomRenderFeature<RadialBlur>
{
	protected override CustomPass<RadialBlur> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new RadialBlurPass(renderPassEvent);
	}
}

public class RadialBlurPass : CustomPass<RadialBlur>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/RadialBlur";
	public override string renderTag => "Render RadialBlur Effects";

	public RadialBlurPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int BlurRadius = Shader.PropertyToID("_BlurRadius");
		internal static readonly int Iteration = Shader.PropertyToID("_Iteration");
		internal static readonly int RadialCenterX = Shader.PropertyToID("_RadialCenterX");
		internal static readonly int RadialCenterY = Shader.PropertyToID("_RadialCenterY");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.BlurRadius, volumeComponent.blurRadius.value);
		material.SetFloat(ShaderPropertyID.Iteration, volumeComponent.iteration.value);
		material.SetFloat(ShaderPropertyID.RadialCenterX, volumeComponent.radialCenterX.value);
		material.SetFloat(ShaderPropertyID.RadialCenterY, volumeComponent.radialCenterY.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}