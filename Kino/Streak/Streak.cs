using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/Streak")]
public class Streak : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	[Range(0, 5)] public FloatParameter threshold = new FloatParameter(0.75f);
	[Range(0, 1)] public FloatParameter stretch = new FloatParameter(0.5f);
	[Range(0, 1)] public FloatParameter intensity = new FloatParameter(0.25f);
	[ColorUsage(false)] public ColorParameter tint = new ColorParameter(new Color(0.55f, 0.55f, 1f));

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(Streak))]
class StreakEditor : VolumeComponentEditor
{
	SerializedDataParameter threshold;
	SerializedDataParameter stretch;
	SerializedDataParameter intensity;
	SerializedDataParameter tint;

	public override void OnEnable()
	{
		var streak = (Streak)target;
		streak.enable.value = true;
		streak.enable.overrideState = true;

		var o = new PropertyFetcher<Streak>(serializedObject);
		threshold = Unpack(o.Find(x => x.threshold));
		stretch = Unpack(o.Find(x => x.stretch));
		intensity = Unpack(o.Find(x => x.intensity));
		tint = Unpack(o.Find(x => x.tint));
	}

	public override void OnInspectorGUI()
	{
		PropertyField(threshold);
		PropertyField(stretch);
		PropertyField(intensity);
		PropertyField(tint);
	}
}
#endif

