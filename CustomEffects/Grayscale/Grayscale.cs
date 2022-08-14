using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Grayscale")]
public class Grayscale : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0f, 1f)]
	public ClampedFloatParameter blendRate = new ClampedFloatParameter(0.5f, 0f, 1f);

	#region IPostProcessComponent
	public bool IsActive() => enable.value && blendRate.value > 0f;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Grayscale))]
class GrayscaleEditor : VolumeComponentEditor
{
	SerializedDataParameter blendRate;

	public override void OnEnable()
	{
		var component = (Grayscale)target;
		component.enable.value = true;
		component.enable.overrideState = true;

		var o = new PropertyFetcher<Grayscale>(serializedObject);
		blendRate = Unpack(o.Find(x => x.blendRate));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(blendRate);
	}
}
#endif

