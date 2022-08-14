using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OilPaintRenderFeature : CustomRenderFeature<OilPaint>
{
	protected override CustomPass<OilPaint> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new OilPaintPass(renderPassEvent);
	}
}

public class OilPaintPass : CustomPass<OilPaint>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/OilPaint";
	public override string renderTag => "Render OilPaint Effects";

	public OilPaintPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int Radius = Shader.PropertyToID("_Radius");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.Radius, volumeComponent.radius.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}