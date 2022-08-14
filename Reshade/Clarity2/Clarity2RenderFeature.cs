using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Clarity2RenderFeature : CustomRenderFeature<Clarity2>
{
	protected override CustomPass<Clarity2> CreatePass(RenderPassEvent renderPassEvent)
	{
		return new Clarity2Pass(renderPassEvent);
	}
}

public class Clarity2Pass : CustomPass<Clarity2>
{
	public override string GetShaderPath() => "Hidden/PostProcessing/Clarity2";
	public override string renderTag => "Render Clarity2 Effects";

	public Clarity2Pass(RenderPassEvent renderPassEvent) : base(renderPassEvent) { }

	static class ShaderPropertyID
	{
		internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
		internal static readonly int Temp1Tex = Shader.PropertyToID("_Temp1Tex");
		internal static readonly int Temp2Tex = Shader.PropertyToID("_Temp2Tex");

		internal static readonly int Offset = Shader.PropertyToID("_Offset");
		internal static readonly int BlendIfDark = Shader.PropertyToID("_BlendIfDark");
		internal static readonly int BlendIfLight = Shader.PropertyToID("_BlendIfLight");
		internal static readonly int BlendIfRange = Shader.PropertyToID("_BlendIfRange");
		internal static readonly int Strength = Shader.PropertyToID("_Strength");
		internal static readonly int MaskContrast = Shader.PropertyToID("_MaskContrast");
		internal static readonly int DarkIntensity = Shader.PropertyToID("_DarkIntensity");
		internal static readonly int LightIntensity = Shader.PropertyToID("_LightIntensity");
		internal static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
	}

	protected override void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		var descriptor = renderingData.cameraData.cameraTargetDescriptor;

		material.SetInt(ShaderPropertyID.Offset, volumeComponent.offset.value);
		material.SetInt(ShaderPropertyID.BlendIfDark, volumeComponent.blendIfDark.value);
		material.SetInt(ShaderPropertyID.BlendIfLight, volumeComponent.blendIfLight.value);
		material.SetFloat(ShaderPropertyID.BlendIfRange, volumeComponent.blendIfRange.value);
		material.SetFloat(ShaderPropertyID.Strength, volumeComponent.strength.value);
		material.SetFloat(ShaderPropertyID.MaskContrast, volumeComponent.maskContrast.value);
		material.SetFloat(ShaderPropertyID.DarkIntensity, volumeComponent.darkIntensity.value);
		material.SetFloat(ShaderPropertyID.LightIntensity, volumeComponent.lightIntensity.value);
		material.SetFloat(ShaderPropertyID.DitherStrength, volumeComponent.ditherStrength.value);

		switch (volumeComponent.radius.value) {
			case 0: material.EnableKeyword("RADIUS_ZERO"); break;
			case 1: material.EnableKeyword("RADIUS_ONE"); break;
			case 2: material.EnableKeyword("RADIUS_TWO"); break;
			case 3: material.EnableKeyword("RADIUS_THEREE"); break;
			case 4: material.EnableKeyword("RADIUS_FOUR"); break;
		}

		switch (volumeComponent.blendMode.value) {
			case Clarity2.BlendMode.SoftLight: material.EnableKeyword("BLENDMODE_SOFTLIGHT"); break;
			case Clarity2.BlendMode.Overlay: material.EnableKeyword("BLENDMODE_OVERLAY"); break;
			case Clarity2.BlendMode.HardLight: material.EnableKeyword("BLENDMODE_HARDLIGHT"); break;
			case Clarity2.BlendMode.Multiply: material.EnableKeyword("BLENDMODE_MULTIPLY"); break;
			case Clarity2.BlendMode.VividLight: material.EnableKeyword("BLENDMODE_VIVIDLIGHT"); break;
			case Clarity2.BlendMode.LinearLight: material.EnableKeyword("BLENDMODE_LINEARLIGHT"); break;
			case Clarity2.BlendMode.Addition: material.EnableKeyword("BLENDMODE_ADDITION"); break;
		}

		cmd.SetGlobalTexture(ShaderPropertyID.MainTex, source);
		cmd.GetTemporaryRT(ShaderPropertyID.Temp1Tex, descriptor.width / 2, descriptor.height / 2, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
		cmd.GetTemporaryRT(ShaderPropertyID.Temp2Tex, descriptor.width / 2, descriptor.height / 2, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
		var temp1 = new RenderTargetIdentifier(ShaderPropertyID.Temp1Tex);
		var temp2 = new RenderTargetIdentifier(ShaderPropertyID.Temp2Tex);
		cmd.SetGlobalTexture(ShaderPropertyID.Temp1Tex, temp1);
		cmd.SetGlobalTexture(ShaderPropertyID.Temp2Tex, temp2);
		cmd.Blit(source, temp1, material, 0);
		cmd.Blit(temp1, temp2, material, 1);
		cmd.Blit(temp2, temp1, material, 2);
		cmd.Blit(source, destination, material, 3);

		cmd.ReleaseTemporaryRT(ShaderPropertyID.Temp1Tex);
		cmd.ReleaseTemporaryRT(ShaderPropertyID.Temp2Tex);
	}
}