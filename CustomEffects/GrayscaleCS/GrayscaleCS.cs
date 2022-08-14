using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/GrayscaleCS")]
public class GrayscaleCS : VolumeComponent, IPostProcessComponent
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
[VolumeComponentEditor(typeof(GrayscaleCS))]
class GrayscaleCSEditor : VolumeComponentEditor
{
	SerializedDataParameter blendRate;

	public override void OnEnable()
	{
		var grayscale = (GrayscaleCS)target;
		grayscale.enable.value = true;
		grayscale.enable.overrideState = true;

		var o = new PropertyFetcher<GrayscaleCS>(serializedObject);
		blendRate = Unpack(o.Find(x => x.blendRate));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(blendRate);
	}
}
#endif

