using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnalogGlitchRenderFeature : CustomRenderFeature<AnalogGlitch>
{
	protected override CustomPass<AnalogGlitch> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new AnalogGlitchPass(renderPassEvent);
	}
}

public class AnalogGlitchPass : CustomPass<AnalogGlitch>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/AnalogGlitch";
	public override string renderTag => "Render AnalogGlitch Effects";

	public AnalogGlitchPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int ScanLineJitter = Shader.PropertyToID("_ScanLineJitter");
		internal static readonly int VerticalJump = Shader.PropertyToID("_VerticalJump");
		internal static readonly int HorizontalShake = Shader.PropertyToID("_HorizontalShake");
		internal static readonly int ColorDrift = Shader.PropertyToID("_ColorDrift");
	}

	float verticalJumpTime;
	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		verticalJumpTime += Time.deltaTime * volumeComponent.verticalJump.value * 11.3f;

		var sl_thresh = Mathf.Clamp01(1.0f - volumeComponent.scanLineJitter.value * 1.2f);
		var sl_disp = 0.002f + Mathf.Pow(volumeComponent.scanLineJitter.value, 3) * 0.05f;
		var vj = new Vector2(volumeComponent.verticalJump.value, verticalJumpTime);
		var cd = new Vector2(volumeComponent.colorDrift.value * 0.04f, Time.time * 606.11f);

		material.SetVector(ShaderPropertyID.ScanLineJitter, new Vector2(sl_disp, sl_thresh));
		material.SetVector(ShaderPropertyID.VerticalJump, vj);
		material.SetFloat(ShaderPropertyID.HorizontalShake, volumeComponent.horizontalShake.value * 0.2f);
		material.SetVector(ShaderPropertyID.ColorDrift, cd);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}