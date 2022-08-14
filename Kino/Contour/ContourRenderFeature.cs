using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ContourRenderFeature : CustomRenderFeature<Contour>
{
	protected override CustomPass<Contour> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new ContourPass(renderPassEvent);
	}
}

public class ContourPass : CustomPass<Contour>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Contour";
	public override string renderTag => "Render Contour Effects";

	public ContourPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		
		internal static readonly int Color = Shader.PropertyToID("_Color");
		internal static readonly int Background = Shader.PropertyToID("_Background");
		internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
		internal static readonly int InvRange = Shader.PropertyToID("_InvRange");
		internal static readonly int ColorSensitivity = Shader.PropertyToID("_ColorSensitivity");
		internal static readonly int DepthSensitivity = Shader.PropertyToID("_DepthSensitivity");
		internal static readonly int NormalSensitivity = Shader.PropertyToID("_NormalSensitivity");
		internal static readonly int InvFallOff = Shader.PropertyToID("_InvFallOff");
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		base.OnCameraSetup(cmd, ref renderingData);

		if (volumeComponent != null) {
			if (volumeComponent.normalSensitivity.value > 0f) {
				ConfigureInput(ScriptableRenderPassInput.Normal);
			}
			else if (volumeComponent.depthSensitivity.value > 0f) {
				ConfigureInput(ScriptableRenderPassInput.Depth);
			}
		}
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetColor(ShaderPropertyID.Color, volumeComponent.lineColor.value);
		material.SetColor(ShaderPropertyID.Background, volumeComponent.bgColor.value);
		material.SetFloat(ShaderPropertyID.Threshold, volumeComponent.lowerThreshold.value);
		material.SetFloat(ShaderPropertyID.InvRange, 1 / (volumeComponent.upperThreshold.value - volumeComponent.lowerThreshold.value));
		material.SetFloat(ShaderPropertyID.ColorSensitivity, volumeComponent.colorSensitivity.value);
		material.SetFloat(ShaderPropertyID.DepthSensitivity, volumeComponent.depthSensitivity.value * 2f);
		material.SetFloat(ShaderPropertyID.NormalSensitivity, volumeComponent.normalSensitivity.value);
		material.SetFloat(ShaderPropertyID.InvFallOff, 1 / volumeComponent.falloffDepth.value);

		if (volumeComponent.colorSensitivity.value > 0f)
			material.EnableKeyword("_CONTOUR_COLOR");
		else
			material.DisableKeyword("_CONTOUR_COLOR");

		if (volumeComponent.depthSensitivity.value > 0f)
			material.EnableKeyword("_CONTOUR_DEPTH");
		else
			material.DisableKeyword("_CONTOUR_DEPTH");

		if (volumeComponent.normalSensitivity.value > 0f)
			material.EnableKeyword("_CONTOUR_NORMAL");
		else
			material.DisableKeyword("_CONTOUR_NORMAL");

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}