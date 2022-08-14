using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdaptiveSharpenRenderFeature : CustomRenderFeature<AdaptiveSharpen>
{
	protected override CustomPass<AdaptiveSharpen> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new AdaptiveSharpenPass(renderPassEvent);
	}
}

public class AdaptiveSharpenPass : CustomPass<AdaptiveSharpen>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/AdaptiveSharpen";
	public override string renderTag => "Render AdaptiveSharpen Effects";

	public AdaptiveSharpenPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		internal static readonly int TempTex = Shader.PropertyToID("_TempTex");

		internal static readonly int CurveHeight = Shader.PropertyToID("_CurveHeight");
		internal static readonly int CurveSlope = Shader.PropertyToID("_CurveSlope");
		internal static readonly int LightOvershoot = Shader.PropertyToID("_LightOvershoot");
		internal static readonly int LightCompressionLow = Shader.PropertyToID("_LightCompressionLow");
		internal static readonly int LightCompressionHigh = Shader.PropertyToID("_LightCompressionHigh");
		internal static readonly int DarkOvershoot = Shader.PropertyToID("_DarkOvershoot");
		internal static readonly int DarkCompressionLow = Shader.PropertyToID("_DarkCompressionLow");
		internal static readonly int DarkCompressionHigh = Shader.PropertyToID("_DarkCompressionHigh");
		internal static readonly int ScaleLim = Shader.PropertyToID("_ScaleLim");
		internal static readonly int ScaleCompressionSlope = Shader.PropertyToID("_ScaleCompressionSlope");
		internal static readonly int PowerMeanP = Shader.PropertyToID("_PowerMeanP");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		var descriptor = renderingData.cameraData.cameraTargetDescriptor;

		material.SetFloat(ShaderPropertyID.CurveHeight, volumeComponent.curveHeight.value);
		material.SetFloat(ShaderPropertyID.CurveSlope, volumeComponent.curveSlope.value);
		material.SetFloat(ShaderPropertyID.LightOvershoot, volumeComponent.lightOvershoot.value);
		material.SetFloat(ShaderPropertyID.LightCompressionLow, volumeComponent.lightCompressionLow.value);
		material.SetFloat(ShaderPropertyID.LightCompressionHigh, volumeComponent.lightCompressionHigh.value);
		material.SetFloat(ShaderPropertyID.DarkOvershoot, volumeComponent.darkOvershoot.value);
		material.SetFloat(ShaderPropertyID.DarkCompressionLow, volumeComponent.darkCompressionLow.value);
		material.SetFloat(ShaderPropertyID.DarkCompressionHigh, volumeComponent.darkCompressionHigh.value);
		material.SetFloat(ShaderPropertyID.ScaleLim, volumeComponent.scaleLim.value);
		material.SetFloat(ShaderPropertyID.ScaleCompressionSlope, volumeComponent.scaleCompressionSlope.value);
		material.SetFloat(ShaderPropertyID.PowerMeanP, volumeComponent.powerMeanP.value);

		if (volumeComponent.fastOperation.value)
			material.EnableKeyword("FastOps");
		else
			material.DisableKeyword("FastOps");

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.GetTemporaryRT(ShaderPropertyID.TempTex, descriptor);
		var temp = new RenderTargetIdentifier(ShaderPropertyID.TempTex);
		cmd.Blit(source, temp, material, 0);

		cmd.SetGlobalTexture(ShaderPropertyID.TempTex, temp);
		cmd.Blit(source, destination, material, 1);

		cmd.ReleaseTemporaryRT(ShaderPropertyID.TempTex);
	}
}