using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Contour")]
public class Contour : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Tooltip("Line Color")]
	public ColorParameter lineColor = new ColorParameter(Color.black);
	[Tooltip("Background Color")]
	public ColorParameter bgColor = new ColorParameter(new Color(1, 1, 1, 0));
	[Tooltip("Lower Threshold")]
	public ClampedFloatParameter lowerThreshold = new ClampedFloatParameter(0.05f, 0f, 1f);
	[Tooltip("Upper Threshold")]
	public ClampedFloatParameter upperThreshold = new ClampedFloatParameter(0.5f, 0f, 1f);
	[Tooltip("Color Sensitivity")]
	public ClampedFloatParameter colorSensitivity = new ClampedFloatParameter(0f, 0f, 1f);
	[Tooltip("Depth Sensitivity")]
	public ClampedFloatParameter depthSensitivity = new ClampedFloatParameter(1f, 0f, 1f);
	[Tooltip("Normal Sensitivity")]
	public ClampedFloatParameter normalSensitivity = new ClampedFloatParameter(0f, 0f, 1f);
	[Tooltip("Falloff Depth")]
	public FloatParameter falloffDepth = new FloatParameter(40f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Contour))]
class ContourEditor : VolumeComponentEditor
{
	SerializedDataParameter lineColor;
	SerializedDataParameter bgColor;
	SerializedDataParameter lowerThreshold;
	SerializedDataParameter upperThreshold;
	SerializedDataParameter colorSensitivity;
	SerializedDataParameter depthSensitivity;
	SerializedDataParameter normalSensitivity;
	SerializedDataParameter falloffDepth;

	public override void OnEnable()
	{
		var contour = (Contour)target;
		contour.enable.value = true;
		contour.enable.overrideState = true;

		var o = new PropertyFetcher<Contour>(serializedObject);
		lineColor = Unpack(o.Find(x => x.lineColor));
		bgColor = Unpack(o.Find(x => x.bgColor));
		lowerThreshold = Unpack(o.Find(x => x.lowerThreshold));
		upperThreshold = Unpack(o.Find(x => x.upperThreshold));
		colorSensitivity = Unpack(o.Find(x => x.colorSensitivity));
		depthSensitivity = Unpack(o.Find(x => x.depthSensitivity));
		normalSensitivity = Unpack(o.Find(x => x.normalSensitivity));
		falloffDepth = Unpack(o.Find(x => x.falloffDepth));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(lineColor);
		PropertyField(bgColor);
		PropertyField(lowerThreshold);
		PropertyField(upperThreshold);
		PropertyField(colorSensitivity);
		PropertyField(depthSensitivity);
		PropertyField(normalSensitivity);
		PropertyField(falloffDepth);
	}
}
#endif

