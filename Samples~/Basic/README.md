# Tween Dev Samples

These samples show consumer-side flows for `com.sweetjelly.tween`.

- `Tween Package Bootstrapper.asset` installs `TweenPackageBootstrapperSo` through Bootstrapper.
- `Sample Package Bootstrapper Profile.asset` is assigned to each sample scene's `BootstrapperBehaviour`.
- `Default` demonstrates service-driven transform tweens, groups, and handle controls.
- `UI` demonstrates CanvasGroup, Graphic, Image, and TMP text tweens.
- `TweenPlayer` demonstrates inspector-style tween player components with service injection.

## Setup

Use Unity 6.3 LTS, Bootstrapper 2.0.0 and uGUI 2.0.0. Configure custom Git dependencies in the consuming project's manifest as described in the package README, available through Package Manager. Import this sample through Package Manager. In a fresh project import TMP Essential Resources before opening the UI scene.

Open `Default/Sample Scene.unity`, `UI/Sample Scene.unity` or `TweenPlayer/Sample Scene.unity` and enter Play Mode. All three scenes have the same file name, so select their full paths when configuring a build. The saved profile uses `packageBootstrappers` and the module uses `bootstrapOrder`.

The sample runs with a built-in renderer and does not require URP. Default creates its target/material and preserves a borrowed camera. UI creates its panel and, only if needed, a fallback Canvas; disabling stops its own work and hides its panel, destroying releases generated assets while preserving a borrowed Canvas. Re-enabling resolves the current service again.

TweenPlayer injection uses the typed base class, including Group/Sequence and inactive children. Replay calls Play on active root players, so the button also works when Play On Enable is off. Use the existing designer guides for endpoint, Snap, loop and Animator authoring.

Use the on-screen buttons to start tweens and control the active handle.
