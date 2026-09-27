using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Keep the original class and serialized setting names so existing renderer assets migrate.
public class CustomRenderFeature<T> : ScriptableRendererFeature where T : VolumeComponent, IPostProcessComponent
{
    public enum BufferType { CameraColor, Custom }

    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public BufferType sourceType = BufferType.CameraColor;
        public BufferType destinationType = BufferType.CameraColor;
        [Tooltip("Name of an output produced earlier in the same camera's RenderGraph.")]
        public string sourceTexture = "_SourceTexture";
        public string destinationTexture = "_DestinationTexture";
        public bool showInSceneView = true;
        [Tooltip("Off by default to avoid applying an effect again on overlay cameras.")]
        public bool affectOverlayCameras = false;
    }

    public Settings settings = new Settings();
    CustomPass<T> pass;
    bool compatibilityWarning;

    public override void Create()
    {
        pass?.Release();
        pass = CreatePass(settings.renderPassEvent);
        pass?.Init(pass.GetShaderPath());
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Release();
        pass = null;
    }

    protected virtual CustomPass<T> CreatePass(RenderPassEvent renderPassEvent) => null;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var graphSettings = GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>();
        if (graphSettings != null && graphSettings.enableRenderCompatibilityMode)
        {
            if (!compatibilityWarning)
                Debug.LogWarning("PostProcessingURP requires RenderGraph. Disable Compatibility Mode in URP graphics settings.", this);
            compatibilityWarning = true;
            return;
        }
        compatibilityWarning = false;
        var camera = renderingData.cameraData;
        if (pass == null || !camera.postProcessEnabled || camera.isPreviewCamera ||
            (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView) ||
            (!settings.showInSceneView && camera.isSceneViewCamera) ||
            (!settings.affectOverlayCameras && camera.renderType == CameraRenderType.Overlay))
            return;

        var component = VolumeManager.instance.stack.GetComponent<T>();
        if (component == null || !component.active || !component.IsActive() || !pass.IsValid())
            return;

        pass.Setup(settings);
        // Declare inputs before URP decides which depth/normal passes to schedule.
        // The intermediate-color requirement is separate: requesting Color would also
        // create an unnecessary opaque texture for effects that need the active color.
        pass.ConfigureInput(pass.GetInputRequirements(component));
        renderer.EnqueuePass(pass);
    }
}
