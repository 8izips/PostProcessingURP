using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FakeHDRRenderFeature : CustomRenderFeature<FakeHDR>
{
	protected override CustomPass<FakeHDR> CreatePass(RenderPassEvent renderPassEvent)
	{	
		return new FakeHDRPass(renderPassEvent);
	}
}

public class FakeHDRPass : CustomPass<FakeHDR>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/FakeHDR";
	public override string renderTag => "Render FakeHDR Effects";

	public FakeHDRPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int HDRPower = Shader.PropertyToID("_HDRPower");
		internal static readonly int Radius1 = Shader.PropertyToID("_Radius1");
		internal static readonly int Radius2 = Shader.PropertyToID("_Radius2");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.HDRPower, volumeComponent.power.value);
		material.SetFloat(ShaderPropertyID.Radius1, volumeComponent.radius1.value);
		material.SetFloat(ShaderPropertyID.Radius2, volumeComponent.radius2.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}