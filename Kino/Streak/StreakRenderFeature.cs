using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StreakRenderFeature : CustomRenderFeature<Streak>
{
	protected override CustomPass<Streak> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new StreakPass(renderPassEvent);
	}
}

public class StreakPass : CustomPass<Streak>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Streak";
	public override string renderTag => "Render Streak Effects";

	const int MaxMipLevel = 16;
	int[] _mipWidth;
	int[] _rtMipDown;
	int[] _rtMipUp;
	public StreakPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) {
		_mipWidth = new int[MaxMipLevel];
		_rtMipDown = new int[MaxMipLevel];
		_rtMipUp = new int[MaxMipLevel];

		for (var i = 0; i < MaxMipLevel; i++) {
			_rtMipDown[i] = Shader.PropertyToID("_MipDown" + i);
			_rtMipUp[i] = Shader.PropertyToID("_MipUp" + i);
		}
	}

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		internal static readonly int HighTex = Shader.PropertyToID("_HighTex");

		internal static readonly int Threshold = Shader.PropertyToID("_Threshold");
		internal static readonly int Stretch = Shader.PropertyToID("_Stretch");
		internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
		internal static readonly int Color = Shader.PropertyToID("_Color");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref var cameraData = ref renderingData.cameraData;
		var w = cameraData.camera.scaledPixelWidth;
		var h = cameraData.camera.scaledPixelHeight;

		material.SetFloat(ShaderPropertyID.Threshold, volumeComponent.threshold.value);
		material.SetFloat(ShaderPropertyID.Stretch, volumeComponent.stretch.value);
		material.SetFloat(ShaderPropertyID.Intensity, volumeComponent.intensity.value);
		material.SetColor(ShaderPropertyID.Color, volumeComponent.tint.value);

		// Calculate the mip widths.
		_mipWidth[0] = w;
		for (var i = 1; i < MaxMipLevel; i++)
			_mipWidth[i] = _mipWidth[i - 1] / 2;

		// Apply the prefilter and store into MIP 0.
		var height = h / 2;
		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.GetTemporaryRT(_rtMipDown[0], _mipWidth[0], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
		cmd.Blit(source, _rtMipDown[0], material, 0);

		// Build the MIP pyramid.
		var level = 1;
		for (; level < MaxMipLevel && _mipWidth[level] > 7; level++) {
			cmd.GetTemporaryRT(_rtMipDown[level], _mipWidth[level], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
			cmd.SetGlobalTexture(ShaderPropertyID.MainTex, _rtMipDown[level - 1]);
			cmd.Blit(_rtMipDown[level - 1], _rtMipDown[level], material, 1);
		}
		// MIP 0 is not needed at this point.
		cmd.ReleaseTemporaryRT(_rtMipDown[level]);

		// Upsample and combine.
		var lastRT = _rtMipDown[--level];
		for (level--; level >= 1; level--) {
			cmd.GetTemporaryRT(_rtMipUp[level], _mipWidth[level], height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Default);
			cmd.SetGlobalTexture(ShaderPropertyID.MainTex, lastRT);
			cmd.SetGlobalTexture(ShaderPropertyID.HighTex, _rtMipDown[level]);
			cmd.Blit(lastRT, _rtMipUp[level], material, 2);

			cmd.ReleaseTemporaryRT(_rtMipDown[level]);
			cmd.ReleaseTemporaryRT(lastRT);

			lastRT = _rtMipUp[level];
		}

		// Final composition.
		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, lastRT);
		cmd.SetGlobalTexture(ShaderPropertyID.HighTex, source);		
		cmd.Blit(lastRT, destination, material, 3);

		cmd.ReleaseTemporaryRT(lastRT);
		cmd.EndSample("Streak");
	}
}