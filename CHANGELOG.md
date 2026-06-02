# CHANGELOG

## v1.0.0

* [Added] Added `SWeetJelly.Tween` as an independent Unity package extracted from the previous Core module.
* [Added] Added tween service contract through `ITweenService`.
* [Added] Added tween lane support for `Update`, `FixedUpdate`, and `LateUpdate` execution.
* [Added] Added Unity tween driver support through `TweenBehaviour`.
* [Added] Added tween case and handle workflow for running and controlling tween operations.
* [Added] Added easing support through `EaseType` and easing function abstractions.
* [Added] Added Initialiser integration for registering tween services during project bootstrap.
* [Changed] Moved tween logic from the unclassified Core module into dedicated Tween assemblies.
* [Changed] Separated Tween into `Contract`, `App`, `Framework`, and `InitialiserIntegration` layers.
