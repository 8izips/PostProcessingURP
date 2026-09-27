"""Offline source-contract and math regression checks. No Unity/GPU execution is implied."""
from pathlib import Path
import math
import re
import unittest

ROOT = Path(__file__).resolve().parents[1]

def read(path):
    return (ROOT / path).read_text(encoding="utf-8-sig")

class RenderGraphContracts(unittest.TestCase):
    def test_runtime_has_no_legacy_render_path(self):
        forbidden = ("cmd.Blit(", "GetTemporaryRT(", "ReleaseTemporaryRT(",
                     "OnCameraSetup(", "override void Execute(", "cameraColorTargetHandle")
        for path in ROOT.rglob("*.cs"):
            if "Editor" in path.parts:
                continue
            text = path.read_text(encoding="utf-8-sig")
            for pattern in forbidden:
                self.assertNotIn(pattern, text, f"{path}: {pattern}")

    def test_all_existing_feature_types_are_retained(self):
        expected = {
            "EdgeDetection", "Grayscale", "GrayscaleCS", "OilPaint", "Pencil", "RadialBlur",
            "AnalogGlitch", "Contour", "DigitalGlitch", "Streak", "WaterColor",
            "ADOF", "AdaptiveFog", "AdaptiveSharpen", "Cartoon", "Clarity2", "FakeHDR",
        }
        found = set()
        for path in ROOT.rglob("*RenderFeature.cs"):
            text = path.read_text(encoding="utf-8-sig")
            found.update(re.findall(r"public class (\w+)RenderFeature : CustomRenderFeature<", text))
        self.assertEqual(expected, found)

    def test_graph_inputs_are_explicit(self):
        text = read("PostProcessingGraph.cs")
        for name in ("source", "auxiliary", "auxiliary2", "depth", "normals"):
            self.assertIn(f"builder.UseTexture({name}, AccessFlags.Read)", text)
        self.assertIn("source.Equals(destination)", text)
        self.assertIn("auxiliary.Equals(destination)", text)
        self.assertIn("SetRenderAttachment(destination, 0, AccessFlags.Write)", text)
        self.assertNotIn("AllowGlobalStateModification", text)
        self.assertNotIn("AddUnsafePass", text)

    def test_no_live_volume_data_in_render_callbacks(self):
        text = read("PostProcessingGraph.cs")
        self.assertNotIn("VolumeManager", text)
        self.assertNotIn("public VolumeComponent", text)
        self.assertIn("MaterialPropertyBlock", text)
        self.assertIn("block.SetInteger(Id(name), value)", text)
        self.assertIn("PostProcessingParameters parameters", text)

    def test_named_textures_are_frame_scoped(self):
        text = read("PostProcessingGraph.cs")
        self.assertIn("PostProcessingFrameTextures : ContextItem", text)
        self.assertIn("override void Reset() => textures.Clear()", text)
        base = read("CustomPass.cs")
        self.assertIn("textures.TryGet(settings.sourceTexture, out source)", base)
        self.assertIn("textures.Set(settings.destinationTexture, result)", base)
        self.assertIn("resources.cameraColor = result", base)
        self.assertIn("!camera.resolveFinalTarget", base)

    def test_shader_entries_use_fullscreen_and_stereo(self):
        shaders = list((ROOT / "Resources/Shaders").glob("*.shader"))
        self.assertEqual(16, len(shaders))
        for path in shaders:
            text = path.read_text(encoding="utf-8-sig")
            entries = re.findall(r"#pragma fragment (\w+)", text)
            self.assertTrue(entries, path.name)
            self.assertEqual(len(entries), text.count("#pragma target 3.5"), path.name)
            self.assertEqual(len(entries), text.count("#pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON"), path.name)
            self.assertIn('"RenderPipeline" = "UniversalPipeline"', text)
            for entry in entries:
                self.assertRegex(text, rf"{entry}\(v2f i\)\s*:\s*SV_Target\s*\{{\s*UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX\(i\)")
            self.assertNotIn("UnpackNormalOctRectEncode", text)
            self.assertNotIn("unity_ObjectToWorld", text)
        common = read("Resources/Shaders/Common.hlsl")
        self.assertIn("Runtime/Utilities/Blit.hlsl", common)
        self.assertIn("TEXTURE2D_X(name)", common)

    def test_auxiliary_textures_have_scale_bindings(self):
        declared = set()
        for path in (ROOT / "Resources/Shaders").glob("*.shader"):
            declared.update(re.findall(r"PP_DECLARE_TEXTURE\((\w+)\)", path.read_text()))
        graph = read("PostProcessingGraph.cs")
        for name in declared:
            self.assertIn(f'"{name}"', graph)
        self.assertIn('name + "Scale"', graph)
        self.assertIn('name + "_TexelSize"', graph)

    def test_compute_binding_and_dispatch_contract(self):
        host = read("CustomEffects/GrayscaleCS/GrayscaleCSRenderFeature.cs")
        shader = read("Resources/Shaders/GrayscaleCS.compute")
        for name in ("_SourceTex", "_ResultTex", "_SourceArray", "_ResultArray", "_Blend", "_TextureSize"):
            self.assertIn(name, shader)
            self.assertIn(f'"{name}"', host)
        self.assertIn("GetKernelThreadGroupSizes", host)
        self.assertIn("(width + (int)tx - 1) / (int)tx", host)
        self.assertIn("(height + (int)ty - 1) / (int)ty", host)
        self.assertIn("AddComputePass<ComputeData>", host)
        self.assertIn("id.x >= width || id.y >= height", shader)
        self.assertNotIn("LinearToSRGB", shader)
        self.assertIn("R16G16B16A16_SFloat", host)

    def test_odd_resolution_dispatch_coverage(self):
        shader = read("Resources/Shaders/GrayscaleCS.compute")
        sizes = re.findall(r"numthreads\((\d+),\s*(\d+),\s*(\d+)\)", shader)
        self.assertEqual(2, len(sizes))
        for size in sizes:
            tx, ty, tz = map(int, size)
            for width, height, slices in ((1,1,1),(7,9,1),(1919,1079,1),(1920,1080,2),(3841,2161,2)):
                for length, group in ((width,tx),(height,ty),(slices,tz)):
                    groups = (length + group - 1) // group
                    self.assertGreaterEqual(groups * group, length)
                    self.assertLess((groups-1) * group, length)

    def test_clarity_blur_weights_and_hdr_constant_invariance(self):
        shader = read("Resources/Shaders/Clarity2.shader")
        kernels = re.findall(r"const float weights\[\d+\]=\{([^}]+)\}", shader)
        self.assertEqual(5, len(kernels))
        self.assertIn("(SampleMain(uv+delta)+SampleMain(uv-delta))*weights[j]", shader)
        self.assertIn("return color/normalization", shader)
        self.assertNotRegex(shader, r"SampleMain\([^\n]*delta\)\s*\*")
        for kernel in kernels:
            weights = [float(value) for value in kernel.split(",")]
            self.assertTrue(all(w > 0 and math.isfinite(w) for w in weights))
            norm = weights[0] + 2 * sum(weights[1:])
            for value in (0, 0.18, 1, 8, 100):
                result = (value*weights[0] + sum(2*value*w for w in weights[1:])) / norm
                self.assertAlmostEqual(value, result, places=10)

    def test_streak_pyramid_handles_tiny_and_odd_targets(self):
        host = read("Kino/Streak/StreakRenderFeature.cs")
        self.assertIn("width / 2 > 7", host)
        self.assertIn("levels[count - 1]", host)
        for original in (1,2,7,8,15,16,1919,1920,4096):
            widths = [original]
            while len(widths) < 16 and widths[-1] // 2 > 7:
                widths.append(widths[-1] // 2)
            self.assertTrue(all(value >= 1 for value in widths))
            self.assertLessEqual(len(widths),16)

    def test_history_ownership_and_first_frame_initialization(self):
        host = read("Kino/DigitalGlitch/DigitalGlitchRenderFeature.cs")
        self.assertIn("Dictionary<int, History>", host)
        self.assertIn("!history.valid", host)
        self.assertIn("history.descriptor.Equals(descriptor)", host)
        self.assertIn('" / Save A", true', host)
        self.assertIn('" / Save B", true', host)
        self.assertIn("base.Release()", host)
        self.assertIn("new System.Random(id)", host)
        self.assertNotIn("Screen.width", host)
        self.assertNotIn("Random.value", host)

    def test_simple_delimiter_sanity(self):
        # Deliberately just a lexical check, not a C# or HLSL compilation test.
        for path in ROOT.rglob("*"):
            if path.suffix not in (".cs", ".shader", ".hlsl", ".compute"):
                continue
            text = path.read_text(encoding="utf-8-sig")
            text = re.sub(r'"(?:\\.|[^"\\])*"', '""', text)
            text = re.sub(r"//[^\n]*|/\*[\s\S]*?\*/", "", text)
            for opening, closing in (("{","}"),("(",")"),("[","]")):
                count = 0
                for char in text:
                    count += (char == opening) - (char == closing)
                    self.assertGreaterEqual(count, 0, f"{path}: {opening}{closing}")
                self.assertEqual(0, count, f"{path}: {opening}{closing}")

if __name__ == "__main__":
    unittest.main(verbosity=2)
