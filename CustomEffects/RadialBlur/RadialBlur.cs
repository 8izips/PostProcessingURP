using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/RadialBlur")]
public class RadialBlur : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0f, 1f)]
	public ClampedFloatParameter blurRadius = new ClampedFloatParameter(0.025f, 0f, 1f);
	[Range(2, 30)]
	public ClampedIntParameter iteration = new ClampedIntParameter(10, 2, 30);
	[Range(0f, 1f)]
	public ClampedFloatParameter radialCenterX = new ClampedFloatParameter(0.5f, 0f, 1f);
	[Range(0f, 1f)]
	public ClampedFloatParameter radialCenterY = new ClampedFloatParameter(0.5f, 0f, 1f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(RadialBlur))]
class RadialBlurEditor : VolumeComponentEditor
{
	SerializedDataParameter blurRadius;
	SerializedDataParameter iteration;
	SerializedDataParameter radialCenterX;
	SerializedDataParameter radialCenterY;

	public override void OnEnable()
	{
		var radialBlur = (RadialBlur)target;
		radialBlur.enable.value = true;
		radialBlur.enable.overrideState = true;

		var o = new PropertyFetcher<RadialBlur>(serializedObject);
		blurRadius = Unpack(o.Find(x => x.blurRadius));
		iteration = Unpack(o.Find(x => x.iteration));
		radialCenterX = Unpack(o.Find(x => x.radialCenterX));
		radialCenterY = Unpack(o.Find(x => x.radialCenterY));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(blurRadius);
		PropertyField(iteration);
		PropertyField(radialCenterX);
		PropertyField(radialCenterY);
	}
}
#endif

