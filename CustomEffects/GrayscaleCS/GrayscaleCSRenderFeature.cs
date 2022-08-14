using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrayscaleCSRenderFeature : CustomRenderFeature<GrayscaleCS>
{
	[SerializeField]
	ComputeShader shader;

	protected override CustomPass<GrayscaleCS> CreatePass(RenderPassEvent renderPassEvent)
	{
		settings.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

		var pass = new GrayscaleCSPass(settings.renderPassEvent);
		pass.computeShader = shader;
		return pass;
	}
}

public class GrayscaleCSPass : CustomPass<GrayscaleCS>
{
	public override string GetShaderPath() => "GrayscaleCS";
	public override string renderTag => "Render Grayscale Effects";

	public GrayscaleCSPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		internal static readonly int Source = Shader.PropertyToID("_SourceTex");
		internal static readonly int Result = Shader.PropertyToID("_ResultTex");

		internal static readonly int Blend = Shader.PropertyToID("_Blend");
	}

	public ComputeShader computeShader;
	string kernelName = "GrayscaleCS";
	int kernelIndex = -1;
	public override void Init(string shaderPath)
	{
		if (computeShader != null && computeShader.HasKernel(kernelName)) {
			kernelIndex = computeShader.FindKernel(kernelName);
		}	
	}

	public override bool IsValid()
	{
		return kernelIndex != -1;
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		var descriptor = renderingData.cameraData.cameraTargetDescriptor;
		var width = descriptor.width;
		var height = descriptor.height;

		cmd.GetTemporaryRT(ShaderPropertyID.Result, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, true);
		var rtResult = new RenderTargetIdentifier(ShaderPropertyID.Result);

		cmd.SetComputeTextureParam(computeShader, kernelIndex, ShaderPropertyID.MainTex, source);
		cmd.SetComputeTextureParam(computeShader, kernelIndex, ShaderPropertyID.Result, rtResult);
		cmd.SetComputeFloatParam(computeShader, ShaderPropertyID.Blend, volumeComponent.blendRate.value);
		
		cmd.DispatchCompute(computeShader, kernelIndex, width, height, 1);
		cmd.Blit(rtResult, destination);

		cmd.ReleaseTemporaryRT(ShaderPropertyID.Result);
	}
}