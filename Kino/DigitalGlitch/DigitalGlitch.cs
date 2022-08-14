using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/DigitalGlitch")]
public class DigitalGlitch : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0, 1), Tooltip("Intensity")]
	public FloatParameter intensity = new FloatParameter(0.02f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value && intensity.value > 0f;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(DigitalGlitch))]
class DigitalGlitchEditor : VolumeComponentEditor
{
	SerializedDataParameter intensity;

	public override void OnEnable()
	{
		var glitch = (DigitalGlitch)target;
		glitch.enable.value = true;
		glitch.enable.overrideState = true;

		var o = new PropertyFetcher<DigitalGlitch>(serializedObject);
		intensity = Unpack(o.Find(x => x.intensity));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(intensity);
	}
}
#endif

