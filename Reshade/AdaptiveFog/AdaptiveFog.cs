using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/AdaptiveFog")]
public class AdaptiveFog : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Tooltip("Color of the fog")]
	public ColorParameter fogColor = new ColorParameter(new Color(0.9f, 0.9f, 0.9f));
	[Range(0f, 1f), Tooltip("The maximum fog factor. 1.0 makes distant objects completely fogged out, a lower factor will shimmer them through the fog")]
	public FloatParameter maxFogFactor = new FloatParameter(1f);
	[Range(0f, 175f), Tooltip("The curve how quickly distant objects get fogged. A low value will make the fog appear just slightly. A high value will make the fog kick in rather quickly. The max value in the rage makes it very hard in general to view any objects outside fog")]
	public FloatParameter fogCurve = new FloatParameter(0.25f);
	[Range(0f, 1f), Tooltip("Start of the fog. 0.0 is at the camera, 1.0 is at the horizon, 0.5 is halfway towards the horizon. Before this point no fog will appear")]
	public FloatParameter fogStart = new FloatParameter(0.5f);
	[Range(0f, 50f), Tooltip("Threshold for what is a bright light (that causes bloom) and what isn't")]
	public FloatParameter bloomThreshold = new FloatParameter(10.25f);
	[Range(0f, 100f), Tooltip("Strength of the bloom")]
	public FloatParameter bloomPower = new FloatParameter(10.0f);
	[Range(0f, 1f), Tooltip("Width of the bloom")]
	public FloatParameter bloomWidth = new FloatParameter(0.2f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(AdaptiveFog))]
class AdaptiveFogEditor : VolumeComponentEditor
{
	SerializedDataParameter fogColor;
	SerializedDataParameter maxFogFactor;
	SerializedDataParameter fogCurve;
	SerializedDataParameter fogStart;
	SerializedDataParameter bloomThreshold;
	SerializedDataParameter bloomPower;
	SerializedDataParameter bloomWidth;

	public override void OnEnable()
	{
		var adaptiveFog = (AdaptiveFog)target;
		adaptiveFog.enable.value = true;
		adaptiveFog.enable.overrideState = true;

		var o = new PropertyFetcher<AdaptiveFog>(serializedObject);
		fogColor = Unpack(o.Find(x => x.fogColor));
		maxFogFactor = Unpack(o.Find(x => x.maxFogFactor));
		fogCurve = Unpack(o.Find(x => x.fogCurve));
		fogStart = Unpack(o.Find(x => x.fogStart));
		bloomThreshold = Unpack(o.Find(x => x.bloomThreshold));
		bloomPower = Unpack(o.Find(x => x.bloomPower));
		bloomWidth = Unpack(o.Find(x => x.bloomWidth));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(fogColor);
		PropertyField(maxFogFactor);
		PropertyField(fogCurve);
		PropertyField(fogStart);
		PropertyField(bloomThreshold);
		PropertyField(bloomPower);
		PropertyField(bloomWidth);
	}
}
#endif

