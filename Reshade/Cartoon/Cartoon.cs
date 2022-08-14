using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Cartoon")]
public class Cartoon : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0.1f, 10f), Tooltip("Amount of effect you want")]
	public ClampedFloatParameter power = new ClampedFloatParameter(1.5f, 0.1f, 10f);
	[Range(0.1f, 6f), Tooltip("Raise this to filter out fainter edges. You might need to increase the power to compensate. Whole numbers are faster")]
	public ClampedFloatParameter edgeSlope = new ClampedFloatParameter(1.5f, 0.1f, 6f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Cartoon))]
class CartoonEditor : VolumeComponentEditor
{
	SerializedDataParameter power;
	SerializedDataParameter edgeSlope;

	public override void OnEnable()
	{
		var cartoon = (Cartoon)target;
		cartoon.enable.value = true;
		cartoon.enable.overrideState = true;

		var o = new PropertyFetcher<Cartoon>(serializedObject);
		power = Unpack(o.Find(x => x.power));
		edgeSlope = Unpack(o.Find(x => x.edgeSlope));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(power);
		PropertyField(edgeSlope);
	}
}
#endif

