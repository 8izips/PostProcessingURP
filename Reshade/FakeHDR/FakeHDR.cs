using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif

[System.Serializable, VolumeComponentMenu("Reshade/FakeHDR")]
public class FakeHDR : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);

    [Range(0f, 8f), Tooltip("Power")]
    public ClampedFloatParameter power = new ClampedFloatParameter(1.3f, 0f, 8f);

    [Range(0f, 8f), Tooltip("Radius 1")]
    public ClampedFloatParameter radius1 = new ClampedFloatParameter(0.793f, 0f, 8f);

    [Range(0f, 8f), Tooltip("Raising this seems to make the effect stronger and also brighter.")]
    public ClampedFloatParameter radius2 = new ClampedFloatParameter(0.87f, 0f, 8f);

    #region IPostProcessComponent
    public bool IsActive() => this.enable.value;
    public bool IsTileCompatible() => false;
    #endregion
}

#if UNITY_EDITOR
[VolumeComponentEditor(typeof(FakeHDR))]
class FakeHDREditor : VolumeComponentEditor
{
    SerializedDataParameter power;
    SerializedDataParameter radius1;
    SerializedDataParameter radius2;

    public override void OnEnable()
    {
        var grayscale = (FakeHDR)target;
        grayscale.enable.value = true;
        grayscale.enable.overrideState = true;

        var o = new PropertyFetcher<FakeHDR>(serializedObject);
        power = Unpack(o.Find(x => x.power));
        radius1 = Unpack(o.Find(x => x.radius1));
        radius2 = Unpack(o.Find(x => x.radius2));
    }

    public override void OnInspectorGUI()
    {
        PropertyField(power);
        PropertyField(radius1);
        PropertyField(radius2);
    }
}
#endif