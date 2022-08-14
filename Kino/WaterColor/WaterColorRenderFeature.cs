using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaterColorRenderFeature : CustomRenderFeature<WaterColor>
{
	[SerializeField]
	public Texture2D noiseTex;

	protected override CustomPass<WaterColor> CreatePass(RenderPassEvent renderPassEvent)
	{
		var pass = new WaterColorPass(renderPassEvent);
		pass.noiseTex = noiseTex;
		return pass;
	}
}

public class WaterColorPass : CustomPass<WaterColor>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/WaterColor";
	public override string renderTag => "Render WaterColor Effects";

	public Texture2D noiseTex;

	public WaterColorPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		internal static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");

		internal static readonly int FillColor = Shader.PropertyToID("_FillColor");
		internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
		internal static readonly int EffectParams = Shader.PropertyToID("_EffectParams");
		internal static readonly int Interval = Shader.PropertyToID("_Interval");
		internal static readonly int Iteration = Shader.PropertyToID("_Iteration");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetColor(ShaderPropertyID.FillColor, volumeComponent.fillColor.value);
		material.SetColor(ShaderPropertyID.EdgeColor, volumeComponent.edgeColor.value);
		material.SetVector(ShaderPropertyID.EffectParams, new Vector4(
			volumeComponent.edgeContrast.value, 
			volumeComponent.blurWidth.value, 
			Mathf.Exp(volumeComponent.blurFrequency.value - 0.5f) * 6f, 
			volumeComponent.hueShift.value));
		material.SetFloat(ShaderPropertyID.Interval, volumeComponent.interval.value);
		material.SetInt(ShaderPropertyID.Iteration, volumeComponent.iteration.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.SetGlobalTexture(ShaderPropertyID.NoiseTex, noiseTex);
		cmd.Blit(source, destination, material);
	}
}