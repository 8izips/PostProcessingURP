using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/AnalogGlitch")]
public class AnalogGlitch : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0, 1), Tooltip("Scan line jitter")]
	public FloatParameter scanLineJitter = new FloatParameter(0.5f);

	[Range(0, 1), Tooltip("Vertical jump")]
	public FloatParameter verticalJump = new FloatParameter(0.1f);

	[Range(0, 1), Tooltip("Vertical jump")]
	public FloatParameter horizontalShake = new FloatParameter(0.1f);

	[Range(0, 1), Tooltip("Color Drift")]
	public FloatParameter colorDrift = new FloatParameter(0.2f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(AnalogGlitch))]
class AnalogGlitchEditor : VolumeComponentEditor
{
	SerializedDataParameter scanLineJitter;
	SerializedDataParameter verticalJump;
	SerializedDataParameter horizontalShake;
	SerializedDataParameter colorDrift;

	public override void OnEnable()
	{
		var glitch = (AnalogGlitch)target;
		glitch.enable.value = true;
		glitch.enable.overrideState = true;

		var o = new PropertyFetcher<AnalogGlitch>(serializedObject);
		scanLineJitter = Unpack(o.Find(x => x.scanLineJitter));
		verticalJump = Unpack(o.Find(x => x.verticalJump));
		horizontalShake = Unpack(o.Find(x => x.horizontalShake));
		colorDrift = Unpack(o.Find(x => x.colorDrift));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(scanLineJitter);
		PropertyField(verticalJump);
		PropertyField(horizontalShake);
		PropertyField(colorDrift);
	}
}
#endif

