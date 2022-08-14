using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/ADOF")]
public class ADOF : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	public BoolParameter enableAutoFocus = new BoolParameter(true);
	public Vector2Parameter autoFocusCenter = new Vector2Parameter(new Vector2(0.5f, 0.5f));
	public ClampedFloatParameter autoFocusRadius = new ClampedFloatParameter(0.6f, 0f, 1f);
	public ClampedFloatParameter autoFocusSpeed = new ClampedFloatParameter(0.1f, 0.05f, 1f);
	public ClampedFloatParameter manualFocusDepth = new ClampedFloatParameter(0.001f, 0f, 1f);
	public ClampedFloatParameter nearBlurCurve = new ClampedFloatParameter(6f, 0.5f, 6f);
	public ClampedFloatParameter farBlurCurve = new ClampedFloatParameter(1.5f, 0.5f, 6f);
	public ClampedFloatParameter hyperFocus = new ClampedFloatParameter(0.1f, 0f, 1f);
	public ClampedFloatParameter renderResolutionMult = new ClampedFloatParameter(0.5f, 0.5f, 1f);
	public ClampedFloatParameter shapeRadius = new ClampedFloatParameter(20.5f, 0f, 100f);
	public ClampedFloatParameter smootheningAmount = new ClampedFloatParameter(4.0f, 0f, 200f);
	public ClampedFloatParameter bokehIntensity = new ClampedFloatParameter(0.3f, 0f, 1f);
	public ClampedIntParameter bokehMode = new ClampedIntParameter(2, 0, 3);
	public ClampedIntParameter shapeVertices = new ClampedIntParameter(6, 3, 9);
	public ClampedIntParameter shapeQuality = new ClampedIntParameter(5, 2, 25);
	public ClampedFloatParameter shapeCurvatureAmount = new ClampedFloatParameter(1f, -1f, 1f);
	public ClampedFloatParameter shapeRotation = new ClampedFloatParameter(0f, 0f, 360f);
	public ClampedFloatParameter shapeAnamorphRatio = new ClampedFloatParameter(1f, 0f, 1f);
	public BoolParameter enableChromaticAberration = new BoolParameter(true);
	public ClampedFloatParameter shapeChromaAmount = new ClampedFloatParameter(-0.1f, -1f, 1f);
	public ClampedIntParameter shapeChromaMode = new ClampedIntParameter(2, 0, 2);
	
	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(ADOF))]
class ADOFEditor : VolumeComponentEditor
{
	SerializedDataParameter enableAutoFocus;
	SerializedDataParameter autoFocusCenter;
	SerializedDataParameter autoFocusRadius;
	SerializedDataParameter autoFocusSpeed;
	SerializedDataParameter manualFocusDepth;
	SerializedDataParameter nearBlurCurve;
	SerializedDataParameter farBlurCurve;
	SerializedDataParameter hyperFocus;
	SerializedDataParameter renderResolutionMult;
	SerializedDataParameter shapeRadius;
	SerializedDataParameter smootheningAmount;
	SerializedDataParameter bokehIntensity;
	SerializedDataParameter bokehMode;
	SerializedDataParameter shapeVertices;
	SerializedDataParameter shapeQuality;
	SerializedDataParameter shapeCurvatureAmount;
	SerializedDataParameter shapeRotation;
	SerializedDataParameter shapeAnamorphRatio;
	SerializedDataParameter enableChromaticAberration;
	SerializedDataParameter shapeChromaAmount;
	SerializedDataParameter shapeChromaMode;

	ADOF _instance;
	public override void OnEnable()
	{
		_instance = (ADOF)target;
		_instance.enable.value = true;
		_instance.enable.overrideState = true;

		var o = new PropertyFetcher<ADOF>(serializedObject);
		enableAutoFocus = Unpack(o.Find(x => x.enableAutoFocus));
		autoFocusCenter = Unpack(o.Find(x => x.autoFocusCenter));
		autoFocusRadius = Unpack(o.Find(x => x.autoFocusRadius));
		autoFocusSpeed = Unpack(o.Find(x => x.autoFocusSpeed));
		manualFocusDepth = Unpack(o.Find(x => x.manualFocusDepth));
		nearBlurCurve = Unpack(o.Find(x => x.nearBlurCurve));
		farBlurCurve = Unpack(o.Find(x => x.farBlurCurve));
		hyperFocus = Unpack(o.Find(x => x.hyperFocus));
		renderResolutionMult = Unpack(o.Find(x => x.renderResolutionMult));
		shapeRadius = Unpack(o.Find(x => x.shapeRadius));
		smootheningAmount = Unpack(o.Find(x => x.smootheningAmount));
		bokehIntensity = Unpack(o.Find(x => x.bokehIntensity));
		bokehMode = Unpack(o.Find(x => x.bokehMode));
		shapeVertices = Unpack(o.Find(x => x.shapeVertices));
		shapeQuality = Unpack(o.Find(x => x.shapeQuality));
		shapeCurvatureAmount = Unpack(o.Find(x => x.shapeCurvatureAmount));
		shapeRotation = Unpack(o.Find(x => x.shapeRotation));
		shapeAnamorphRatio = Unpack(o.Find(x => x.shapeAnamorphRatio));
		enableChromaticAberration = Unpack(o.Find(x => x.enableChromaticAberration));
		shapeChromaAmount = Unpack(o.Find(x => x.shapeChromaAmount));
		shapeChromaMode = Unpack(o.Find(x => x.shapeChromaMode));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(enableAutoFocus);
		if (_instance.enableAutoFocus.value) {
			PropertyField(autoFocusCenter);
			PropertyField(autoFocusRadius);
			PropertyField(autoFocusSpeed);
		}

		PropertyField(manualFocusDepth);
		PropertyField(nearBlurCurve);
		PropertyField(farBlurCurve);
		PropertyField(hyperFocus);
		PropertyField(renderResolutionMult);
		PropertyField(shapeRadius);
		PropertyField(smootheningAmount);
		PropertyField(bokehIntensity);
		PropertyField(bokehMode);
		PropertyField(shapeVertices);
		PropertyField(shapeQuality);
		PropertyField(shapeCurvatureAmount);
		PropertyField(shapeRotation);
		PropertyField(shapeAnamorphRatio);

		PropertyField(enableChromaticAberration);
		if (_instance.enableChromaticAberration.value) {
			PropertyField(shapeChromaAmount);
			PropertyField(shapeChromaMode);
		}
	}
}
#endif

