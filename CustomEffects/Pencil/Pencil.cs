using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Pencil")]
public class Pencil : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Tooltip("Gradient Threshold")]
	public ClampedFloatParameter gradThreshold = new ClampedFloatParameter(0.01f, 0.00001f, 0.01f);
	[Tooltip("Color Threshold")]
	public ClampedFloatParameter colorThreshold = new ClampedFloatParameter(0.8f, 0f, 1f);
	public ClampedFloatParameter sensivity = new ClampedFloatParameter(10f, 0f, 100f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Pencil))]
class PencilEditor : VolumeComponentEditor
{
	SerializedDataParameter gradThreshold;
	SerializedDataParameter colorThreshold;
	SerializedDataParameter sensivity;

	public override void OnEnable()
	{
		var pencil = (Pencil)target;
		pencil.enable.value = true;
		pencil.enable.overrideState = true;

		var o = new PropertyFetcher<Pencil>(serializedObject);
		gradThreshold = Unpack(o.Find(x => x.gradThreshold));
		colorThreshold = Unpack(o.Find(x => x.colorThreshold));
		sensivity = Unpack(o.Find(x => x.sensivity));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(gradThreshold);
		PropertyField(colorThreshold);
		PropertyField(sensivity);
	}
}
#endif

