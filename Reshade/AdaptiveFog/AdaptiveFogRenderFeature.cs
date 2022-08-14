using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdaptiveFogRenderFeature : CustomRenderFeature<AdaptiveFog>
{
	protected override CustomPass<AdaptiveFog> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new AdaptiveFogPass(renderPassEvent);
	}
}

public class AdaptiveFogPass : CustomPass<AdaptiveFog>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/AdaptiveFog";
	public override string renderTag => "Render AdaptiveFog Effects";

	public AdaptiveFogPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int FogColor = Shader.PropertyToID("_FogColor");
		internal static readonly int MaxFogFactor = Shader.PropertyToID("_MaxFogFactor");
		internal static readonly int FogCurve = Shader.PropertyToID("_FogCurve");
		internal static readonly int FogStart = Shader.PropertyToID("_FogStart");
		internal static readonly int BloomThreshold = Shader.PropertyToID("_BloomThreshold");
		internal static readonly int BloomPower = Shader.PropertyToID("_BloomPower");
		internal static readonly int BloomWidth = Shader.PropertyToID("_BloomWidth");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetColor(ShaderPropertyID.FogColor, volumeComponent.fogColor.value);
		material.SetFloat(ShaderPropertyID.MaxFogFactor, volumeComponent.maxFogFactor.value);
		material.SetFloat(ShaderPropertyID.FogCurve, volumeComponent.fogCurve.value);
		material.SetFloat(ShaderPropertyID.FogStart, volumeComponent.fogStart.value);
		material.SetFloat(ShaderPropertyID.BloomThreshold, volumeComponent.bloomThreshold.value);
		material.SetFloat(ShaderPropertyID.BloomPower, volumeComponent.bloomPower.value);
		material.SetFloat(ShaderPropertyID.BloomWidth, volumeComponent.bloomWidth.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}