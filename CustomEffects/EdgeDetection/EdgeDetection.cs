using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/EdgeDetection")]
public class EdgeDetection : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Tooltip("Controls the blending between the original and the edge color.")]
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0.5f, 0f, 1f);

	[Tooltip("Defines the edge thickness.")]
	public MinFloatParameter thickness = new MinFloatParameter(1, 0);

	[Tooltip("Define the threshold of the normal difference in degrees.")]
	public FloatRangeParameter normalThreshold = new FloatRangeParameter(new Vector2(1, 2), 0, 360);

	[Tooltip("Define the threshold of the depth difference in world units.")]
	public FloatRangeParameter depthThreshold = new FloatRangeParameter(new Vector2(0.1f, 0.11f), 0, 1);

	[Tooltip("Define the edge color.")]
	public ColorParameter color = new ColorParameter(Color.black, true, false, true);

	#region IPostProcessComponent
	public bool IsActive() => enable.value && intensity.value > 0f;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(EdgeDetection))]
class EdgeDetectionEditor : VolumeComponentEditor
{
	SerializedDataParameter intensity;
	SerializedDataParameter thickness;
	SerializedDataParameter normalThreshold;
	SerializedDataParameter depthThreshold;
	SerializedDataParameter color;

	public override void OnEnable()
	{
		var edgeDetection = (EdgeDetection)target;
		edgeDetection.enable.value = true;
		edgeDetection.enable.overrideState = true;

		var o = new PropertyFetcher<EdgeDetection>(serializedObject);
		intensity = Unpack(o.Find(x => x.intensity));
		thickness = Unpack(o.Find(x => x.thickness));
		normalThreshold = Unpack(o.Find(x => x.normalThreshold));
		depthThreshold = Unpack(o.Find(x => x.depthThreshold));
		color = Unpack(o.Find(x => x.color));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(intensity);
		PropertyField(thickness);
		PropertyField(normalThreshold);
		PropertyField(depthThreshold);
		PropertyField(color);
	}
}
#endif

