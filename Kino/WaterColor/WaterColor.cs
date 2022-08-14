using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Post-processing Custom/WaterColor")]
public class WaterColor : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enable = new BoolParameter(false);

	public ColorParameter fillColor = new ColorParameter(new Color(0.9f, 0.8f, 1f, 0.8f));
	public ColorParameter edgeColor = new ColorParameter(new Color(0.1f, 0.1f, 0.3f));	
	public FloatParameter edgeContrast = new FloatParameter(1f);
	public FloatParameter blurWidth = new FloatParameter(0.1f);
	public FloatParameter blurFrequency = new FloatParameter(0.2f);
	public FloatParameter hueShift = new FloatParameter(0.2f);
	public FloatParameter interval = new FloatParameter(0.5f);
	public IntParameter iteration = new IntParameter(20);

	#region IPostProcessComponent
	public bool IsActive() => enable.value;
	public bool IsTileCompatible() => false;
	#endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(WaterColor))]
class WaterColorEditor : VolumeComponentEditor
{
	SerializedDataParameter fillColor;
	SerializedDataParameter edgeColor;
	SerializedDataParameter edgeContrast;
	SerializedDataParameter blurWidth;
	SerializedDataParameter blurFrequency;
	SerializedDataParameter hueShift;
	SerializedDataParameter interval;
	SerializedDataParameter iteration;

	public override void OnEnable()
	{
		var component = (WaterColor)target;
		component.enable.value = true;
		component.enable.overrideState = true;

		var o = new PropertyFetcher<WaterColor>(serializedObject);
		fillColor = Unpack(o.Find(x => x.fillColor));
		edgeColor = Unpack(o.Find(x => x.edgeColor));
		edgeContrast = Unpack(o.Find(x => x.edgeContrast));
		blurWidth = Unpack(o.Find(x => x.blurWidth));
		blurFrequency = Unpack(o.Find(x => x.blurFrequency));
		hueShift = Unpack(o.Find(x => x.hueShift));
		interval = Unpack(o.Find(x => x.interval));
		iteration = Unpack(o.Find(x => x.iteration));
		iteration = Unpack(o.Find(x => x.iteration));
	}

	public override void OnInspectorGUI()
	{		
		PropertyField(fillColor);
		PropertyField(edgeColor);
		PropertyField(edgeContrast);
		PropertyField(blurWidth);
		PropertyField(blurFrequency);
		PropertyField(hueShift);
		PropertyField(interval);
		PropertyField(iteration);
	}
}
#endif

