using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Clarity2")]
public class Clarity2 : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	public enum BlendMode
	{
		SoftLight,
		Overlay,
		HardLight,
		Multiply,
		VividLight,
		LinearLight,
		Addition
	}
	[Serializable]
	public sealed class BlendModeParameter : VolumeParameter<BlendMode> { }

	public ClampedIntParameter radius = new ClampedIntParameter(3, 0, 4);
	public ClampedIntParameter offset = new ClampedIntParameter(2, 1, 5);
	public BlendModeParameter blendMode = new BlendModeParameter { value = BlendMode.HardLight};
	public ClampedIntParameter blendIfDark = new ClampedIntParameter(50, 0, 255);
	public ClampedIntParameter blendIfLight = new ClampedIntParameter(205, 0, 255);
	public ClampedFloatParameter blendIfRange = new ClampedFloatParameter(0.2f, 0f, 1f);
	public ClampedFloatParameter strength = new ClampedFloatParameter(0.4f, 0f, 1f);
	public ClampedFloatParameter maskContrast = new ClampedFloatParameter(0f, 0f, 1f);
	public ClampedFloatParameter darkIntensity = new ClampedFloatParameter(0.4f, 0f, 10f);
	public ClampedFloatParameter lightIntensity = new ClampedFloatParameter(0f, 0f, 10f);
	public ClampedFloatParameter ditherStrength = new ClampedFloatParameter(1f, 0f, 10f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Clarity2))]
class Clarity2Editor : VolumeComponentEditor
{
	SerializedDataParameter radius;
	SerializedDataParameter offset;
	SerializedDataParameter blendMode;
	SerializedDataParameter blendIfDark;
	SerializedDataParameter blendIfLight;
	SerializedDataParameter blendIfRange;
	SerializedDataParameter strength;
	SerializedDataParameter maskContrast;
	SerializedDataParameter darkIntensity;
	SerializedDataParameter lightIntensity;
	SerializedDataParameter ditherStrength;

	public override void OnEnable()
	{
		var component = (Clarity2)target;
		component.enable.value = true;
		component.enable.overrideState = true;

		var o = new PropertyFetcher<Clarity2>(serializedObject);
		radius = Unpack(o.Find(x => x.radius));
		offset = Unpack(o.Find(x => x.offset));
		blendMode = Unpack(o.Find(x => x.blendMode));
		blendIfDark = Unpack(o.Find(x => x.blendIfDark));
		blendIfLight = Unpack(o.Find(x => x.blendIfLight));
		blendIfRange = Unpack(o.Find(x => x.blendIfRange));
		strength = Unpack(o.Find(x => x.strength));
		maskContrast = Unpack(o.Find(x => x.maskContrast));
		darkIntensity = Unpack(o.Find(x => x.darkIntensity));
		lightIntensity = Unpack(o.Find(x => x.lightIntensity));
		ditherStrength = Unpack(o.Find(x => x.ditherStrength));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(radius);
		PropertyField(offset);
		PropertyField(blendMode);
		PropertyField(blendIfDark);
		PropertyField(blendIfLight);
		PropertyField(blendIfRange);
		PropertyField(strength);
		PropertyField(maskContrast);
		PropertyField(darkIntensity);
		PropertyField(lightIntensity);
		PropertyField(ditherStrength);
	}
}
#endif

