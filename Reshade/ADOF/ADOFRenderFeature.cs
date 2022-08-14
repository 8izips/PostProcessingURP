using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ADOFRenderFeature : CustomRenderFeature<ADOF>
{
	protected override CustomPass<ADOF> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new ADOFPass(renderPassEvent);
	}
}

public class ADOFPass : CustomPass<ADOF>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/ADOF";
	public override string renderTag => "Render ADOF Effects";

	public ADOFPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int Blend = Shader.PropertyToID("_Blend");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//material.SetFloat(ShaderPropertyID.Blend, volumeComponent.blendRate.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}