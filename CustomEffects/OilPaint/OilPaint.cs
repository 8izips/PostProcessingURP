using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/OilPaint")]
public class OilPaint : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0, 16), Tooltip("Brush Radius")]
	public IntParameter radius = new IntParameter(8);

	#region IPostProcessComponent
	public bool IsActive() => enable.value && radius.value > 0;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(OilPaint))]
class OilPaintEditor : VolumeComponentEditor
{
	SerializedDataParameter radius;

	public override void OnEnable()
	{
		var oilPaint = (OilPaint)target;
		oilPaint.enable.value = true;
		oilPaint.enable.overrideState = true;

		var o = new PropertyFetcher<OilPaint>(serializedObject);
		radius = Unpack(o.Find(x => x.radius));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(radius);
	}
}
#endif

