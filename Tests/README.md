# Validation

`python Tests/validate_sources.py` runs offline source-contract and mathematical regression checks.
They do **not** compile C#/HLSL, execute Unity, validate images, or measure GPU time.

In Unity 6, first run **Tools > PostProcessingURP > Check Imported Shaders**.
This reports missing assets and currently imported shader errors; it does not compile every variant.

## Visual smoke checks (required before calling a platform verified)

1. Use a Universal Renderer with Compatibility Mode disabled and the camera's Post Processing enabled.
   Add one matching Renderer Feature and Volume override. Check each of the 16 advertised effects independently.
   GrayscaleCS includes its raster fallback. ADOF is explicitly an unfinished, disabled placeholder.
2. Compare Grayscale and GrayscaleCS at 1919x1079 and 1920x1080, with blend 0 and 1, in Linear and Gamma projects.
   Include RGB values above 1 in an HDR RenderTexture and inspect them before tone mapping. Test a device without compute support.
3. For Clarity2, use a uniform gray/HDR image and a checkerboard. Change every radius and blend mode in both directions.
   The blur stages must preserve constant input and must not stretch/collapse image coordinates.
4. For Contour and EdgeDetection, use Forward and Deferred (including accurate G-buffer normals), perspective and orthographic cameras.
   Test depth-only, normal-only and combined input, including the first frame after enabling the feature.
5. For Streak, test 1x1, 15x7, 1919x1079 and the normal camera target. Check the RenderGraph Viewer for separate mip reads/writes.
6. For DigitalGlitch, enable it at an arbitrary frame; resize and toggle HDR/render scale. Display two cameras with visibly different colors.
   Check the first frame, re-enable after a gap, remove a camera, and reload/disable the Renderer Feature. Histories must not cross cameras.
7. Test Base + Overlay stacks, MSAA 1x/4x and Render Scale 0.5/1.0. Existing early injection events preserve the original attachment via a copy;
   the usual final-camera post-processing path instead hands off a new camera color.
8. Test CameraColor -> Custom, Custom -> Custom, Custom -> CameraColor and a missing custom input.
   Named sources must have a producer earlier in this same camera graph. Missing input should warn and skip without changing the image.
9. Test a Player build on the target graphics APIs. For XR test both eyes and compute texture-array kernels on hardware;
   hardware dynamic resolution, foveated rendering, and platform-specific behavior are not proven by source checks.
10. Profile OilPaint and WaterColor on the actual target. Their high sampling cost is not eliminated by this migration.

Do not treat a successful import/static test as evidence of correct rendering, shader variant coverage, XR support, or performance.
