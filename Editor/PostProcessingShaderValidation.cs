#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Lightweight import diagnostics, not a substitute for rendering in the Editor and a Player.
public static class PostProcessingShaderValidation
{
    static readonly string[] effects = {
        "AnalogGlitch", "Contour", "DigitalGlitch", "Streak", "WaterColor",
        "AdaptiveFog", "AdaptiveSharpen", "Cartoon", "Clarity2", "FakeHDR",
        "EdgeDetection", "Grayscale", "OilPaint", "Pencil", "RadialBlur"
    };

    [MenuItem("Tools/PostProcessingURP/Check Imported Shaders")]
    public static void CheckImportedShaders()
    {
        int errors = 0;
        if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
        {
            Debug.LogError("PostProcessingURP: the active pipeline is not URP.");
            errors++;
        }
        var settings = GraphicsSettings.GetRenderPipelineSettings<RenderGraphSettings>();
        if (settings != null && settings.enableRenderCompatibilityMode)
        {
            Debug.LogError("PostProcessingURP: disable RenderGraph Compatibility Mode.");
            errors++;
        }
        foreach (var effect in effects)
        {
            var shader = Shader.Find("Hidden/PostProcessing/" + effect);
            if (shader == null)
            {
                Debug.LogError("PostProcessingURP: missing shader " + effect);
                errors++;
                continue;
            }
            if (!shader.isSupported)
            {
                Debug.LogError("PostProcessingURP: unsupported shader " + effect, shader);
                errors++;
            }
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
            {
                if (message.severity.ToString() != "Error") continue;
                Debug.LogError(effect + ": " + message.message + " (" + message.file + ":" + message.line + ")", shader);
                errors++;
            }
        }
        var compute = Resources.Load<ComputeShader>("Shaders/GrayscaleCS");
        if (SystemInfo.supportsComputeShaders && (compute == null ||
            !compute.HasKernel("GrayscaleCS") || !compute.HasKernel("GrayscaleCSArray")))
        {
            Debug.LogError("PostProcessingURP: the compute asset or one of its kernels is missing.");
            errors++;
        }
        Debug.Log("PostProcessingURP import check: " + errors + " error(s). " +
            "This checks currently imported shader messages only. Render each effect and keyword variant; " +
            "check a Player build, HDR, odd resolutions, camera stacks and XR on the target device.");
    }
}
#endif
