# CHANGELOG

## v1.1.0

* [Added] Added `TweenPlayerBehaviour` components for designer-driven tween playback through Unity Events.
* [Added] Added `TweenPlayerGroupBehaviour` for playing, snapping, and killing multiple tween players together.
* [Added] Added `TweenPlayerSequenceBehaviour` for playing multiple tween players in order.
* [Added] Added forward and backward playback APIs through `PlayForward`, `PlayBackward`, `SnapForward`, and `SnapBackward`.
* [Added] Added explicit target component assignment support for tween players with self-component fallback.
* [Added] Added axis-specific tween players for `Transform` position, local position, scale, rotation, and local rotation.
* [Added] Added axis-specific tween players for `RectTransform` anchored position and size.
* [Added] Added a TweenPlayer designer guide.
* [Changed] Changed tween player terminology from target/initial playback to forward/backward playback.
* [Changed] Changed tween player `playOnEnable` default value to disabled.
* [Changed] Changed missing tween service handling to always log a warning when playback is requested before service injection.
* [Changed] Reorganized tween player scripts into type-based folders such as `Transform`, `RectTransform`, `Graphic`, `Image`, and `CanvasGroup`.
* [Removed] Removed legacy target/initial playback APIs from tween players.
* [Removed] Removed the `logMissingServiceWarning` toggle from tween players.

## v1.0.0

* [Added] Added `SWeetJelly.Tween` as an independent Unity package extracted from the previous Core module.
* [Added] Added tween service contract through `ITweenService`.
* [Added] Added tween lane support for `Update`, `FixedUpdate`, and `LateUpdate` execution.
* [Added] Added Unity tween driver support through `TweenBehaviour`.
* [Added] Added tween case and handle workflow for running and controlling tween operations.
* [Added] Added easing support through `EaseType` and easing function abstractions.
* [Added] Added Bootstrapper integration for registering tween services during project bootstrap.
* [Changed] Moved tween logic from the unclassified Core module into dedicated Tween assemblies.
* [Changed] Separated Tween into `Contract`, `App`, `Framework/Unity`, and `Framework/Bootstrapper` layers.
