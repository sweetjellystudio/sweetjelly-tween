# TweenPlayer Dev Sample

This sample shows the `TweenPlayerBehaviour` flow for `com.sweetjelly.tween`.

- `TweenPlayerSampleInjectorBehaviour` resolves `ITweenService` from Bootstrapper.
- The scene contains GameObjects with TweenPlayer components already attached.
- The injector sends `ITweenService` to every `TweenPlayerBehaviour` instance, then the players run from their `OnEnable` lifecycle.
- The sample includes Transform scale, CanvasGroup fade, Graphic color, and RectTransform anchored position players.
- For designer-facing setup rules and troubleshooting, see `Documentation~/TweenPlayerDesignerGuide.ko.md`.

## Setup

Open `Sample Scene` and enter Play Mode.

Select the `TweenPlayer - ...` scene objects to inspect their timing, easing, loop, target type, and target value settings.

Use `Replay Tween Players` to toggle the player GameObjects and replay their OnEnable tweens.
