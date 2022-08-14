using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionRenderFeature : CustomRenderFeature<EdgeDetection>
{
	protected override CustomPass<EdgeDetection> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new EdgeDetectionPass(renderPassEvent);
	}
}

public class EdgeDetectionPass : CustomPass<EdgeDetection>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/EdgeDetection";
	public override string renderTag => "Render EdgeDetection Effects";

	public EdgeDetectionPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal readonly static int Intensity = Shader.PropertyToID("_Intensity");
		internal readonly static int Threshold = Shader.PropertyToID("_Threshold");
		internal readonly static int Thickness = Shader.PropertyToID("_Thickness");
		internal readonly static int Color = Shader.PropertyToID("_Color");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		material.SetFloat(ShaderPropertyID.Intensity, volumeComponent.intensity.value);
		material.SetFloat(ShaderPropertyID.Thickness, volumeComponent.thickness.value);
		Vector2 normalThreshold = volumeComponent.normalThreshold.value;
		Vector2 depthThreshold = volumeComponent.depthThreshold.value;
		Vector4 threshold = new Vector4(Mathf.Cos(normalThreshold.y * Mathf.Deg2Rad), Mathf.Cos(normalThreshold.x * Mathf.Deg2Rad), depthThreshold.x, depthThreshold.y);
		material.SetVector(ShaderPropertyID.Threshold, threshold);
		material.SetColor(ShaderPropertyID.Color, volumeComponent.color.value);

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}
}