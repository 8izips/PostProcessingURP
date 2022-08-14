using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CartoonRenderFeature : CustomRenderFeature<Cartoon>
{
	protected override CustomPass<Cartoon> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new CartoonPass(renderPassEvent);
	}
}

public class CartoonPass : CustomPass<Cartoon>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Cartoon";
	public override string renderTag => "Render Cartoon Effects";

	public CartoonPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int Power = Shader.PropertyToID("_Power");
		internal static readonly int EdgeSlope = Shader.PropertyToID("_EdgeSlope");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.Power, volumeComponent.power.value);
		material.SetFloat(ShaderPropertyID.EdgeSlope, volumeComponent.edgeSlope.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}