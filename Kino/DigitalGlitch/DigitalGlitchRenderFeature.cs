using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DigitalGlitchRenderFeature : CustomRenderFeature<DigitalGlitch>
{
	protected override CustomPass<DigitalGlitch> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new DigitalGlitchPass(renderPassEvent);
	}
}

public class DigitalGlitchPass : CustomPass<DigitalGlitch>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/DigitalGlitch";
	public override string renderTag => "Render DigitalGlitch Effects";

	Texture2D noiseTexture;
	RenderTexture trashFrame1;
	RenderTexture trashFrame2;
	public DigitalGlitchPass(RenderPassEvent renderPassEvent) : base(renderPassEvent) {
		noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
		noiseTexture.hideFlags = HideFlags.DontSave;
		noiseTexture.wrapMode = TextureWrapMode.Clamp;
		noiseTexture.filterMode = FilterMode.Point;

		trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
		trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
		trashFrame1.hideFlags = HideFlags.DontSave;
		trashFrame2.hideFlags = HideFlags.DontSave;
	}

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
		internal static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
		internal static readonly int TrashTex = Shader.PropertyToID("_TrashTex");
	}
	
	Color GetRandomColor()
	{
		return new Color(Random.value, Random.value, Random.value, Random.value);
	}

	void UpdateNoiseTexture()
	{
		var color = GetRandomColor();
		for (int y = 0; y < noiseTexture.height; y++) {
			for (int x = 0; x < noiseTexture.width; x++) {
				if (Random.value > 0.89f)
					color = GetRandomColor();
				noiseTexture.SetPixel(x, y, color);
			}
		}

		noiseTexture.Apply();
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (Random.value > Mathf.Lerp(0.9f, 0.5f, volumeComponent.intensity.value))
			UpdateNoiseTexture();

		var frameCount = Time.frameCount;
		if (frameCount % 13 == 0)
			cmd.Blit(source, trashFrame1);
		if (frameCount % 73 == 0)
			cmd.Blit(source, trashFrame2);
		var trashFrame = Random.value > 0.5f ? trashFrame1 : trashFrame2;

		material.SetFloat(ShaderPropertyID.Intensity, volumeComponent.intensity.value);
		material.SetTexture(ShaderPropertyID.NoiseTex, noiseTexture);
		material.SetTexture(ShaderPropertyID.TrashTex, trashFrame);
		material.EnableKeyword("APPLY_FORWARD_FOG");

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.Blit(source, destination, material);
	}

	public override void Release()
	{
		if (noiseTexture != null) {
#if UNITY_EDITOR
			Object.DestroyImmediate(noiseTexture);
#else
			Object.Destroy(noiseTexture);
#endif
		}
		
		if (trashFrame1 != null)
			trashFrame1.Release();
		if (trashFrame2 != null)
			trashFrame2.Release();
	}
}