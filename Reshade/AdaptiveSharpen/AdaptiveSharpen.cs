using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/AdaptiveSharpen")]
public class AdaptiveSharpen : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	public ClampedFloatParameter curveHeight = new ClampedFloatParameter(1f, 0.01f, 2f);
	public ClampedFloatParameter curveSlope = new ClampedFloatParameter(0.5f, 0.01f, 2f);
	public ClampedFloatParameter lightOvershoot = new ClampedFloatParameter(0.003f, 0.001f, 0.1f);
	public ClampedFloatParameter lightCompressionLow = new ClampedFloatParameter(0.167f, 0f, 1f);
	public ClampedFloatParameter lightCompressionHigh = new ClampedFloatParameter(0.334f, 0f, 1f);
	public ClampedFloatParameter darkOvershoot = new ClampedFloatParameter(0.009f, 0.001f, 0.1f);
	public ClampedFloatParameter darkCompressionLow = new ClampedFloatParameter(0.250f, 0f, 1f);
	public ClampedFloatParameter darkCompressionHigh = new ClampedFloatParameter(0.5f, 0f, 1f);
	public ClampedFloatParameter scaleLim = new ClampedFloatParameter(0.1f, 0.01f, 1f);
	public ClampedFloatParameter scaleCompressionSlope = new ClampedFloatParameter(0.056f, 0f, 1f);
	public ClampedFloatParameter powerMeanP = new ClampedFloatParameter(0.7f, 0.01f, 1f);
	public BoolParameter fastOperation = new BoolParameter(false);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(AdaptiveSharpen))]
class AdaptiveSharpenEditor : VolumeComponentEditor
{
	SerializedDataParameter curveHeight;
	SerializedDataParameter curveSlope;
	SerializedDataParameter lightOvershoot;
	SerializedDataParameter lightCompressionLow;
	SerializedDataParameter lightCompressionHigh;
	SerializedDataParameter darkOvershoot;
	SerializedDataParameter darkCompressionLow;
	SerializedDataParameter darkCompressionHigh;
	SerializedDataParameter scaleLim;
	SerializedDataParameter scaleCompressionSlope;
	SerializedDataParameter powerMeanP;
	SerializedDataParameter fastOperation;

	public override void OnEnable()
	{
		var component = (AdaptiveSharpen)target;
		component.enable.value = true;
		component.enable.overrideState = true;

		var o = new PropertyFetcher<AdaptiveSharpen>(serializedObject);
		curveHeight = Unpack(o.Find(x => x.curveHeight));
		curveSlope = Unpack(o.Find(x => x.curveSlope));
		lightOvershoot = Unpack(o.Find(x => x.lightOvershoot));
		lightCompressionLow = Unpack(o.Find(x => x.lightCompressionLow));
		lightCompressionHigh = Unpack(o.Find(x => x.lightCompressionHigh));
		darkOvershoot = Unpack(o.Find(x => x.darkOvershoot));
		darkCompressionLow = Unpack(o.Find(x => x.darkCompressionLow));
		darkCompressionHigh = Unpack(o.Find(x => x.darkCompressionHigh));
		scaleLim = Unpack(o.Find(x => x.scaleLim));
		scaleCompressionSlope = Unpack(o.Find(x => x.scaleCompressionSlope));
		powerMeanP = Unpack(o.Find(x => x.powerMeanP));
		fastOperation = Unpack(o.Find(x => x.fastOperation));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(curveHeight);
		PropertyField(curveSlope);
		PropertyField(lightOvershoot);
		PropertyField(lightCompressionLow);
		PropertyField(lightCompressionHigh);
		PropertyField(darkOvershoot);
		PropertyField(darkCompressionLow);
		PropertyField(darkCompressionHigh);
		PropertyField(scaleLim);
		PropertyField(scaleCompressionSlope);
		PropertyField(powerMeanP);
		PropertyField(fastOperation);
	}
}
#endif

