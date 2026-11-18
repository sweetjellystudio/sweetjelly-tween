---
status: accepted
date: 2026-11-18
---

#   Saved UITK definitions and independently bound Players

The user confirmed Q1–Q9 and the final implementation/verification agreement. Production UXML element selection, saved motion and runtime/preview parity are required together. Feature/Show/Hide are consumer examples, not Tween domain types or scheduling scopes.

Existing component players store settings, component targets, capture and execution together. Their child references, public/protected surface and prepared timeline owner are component-specific. Replacing their serialized graph would impose migration and change event/observed-boundary semantics. A UIDocument component adapter alone also fails to identify independent repeated roots in one document.

We add reusable definition assets and an explicitly root-bound runtime Player, plus dedicated Editor authoring. Assets retain no live elements, services, handles, tokens or captures. A preparation freezes values/curves, calculated sequential starts and original inline declarations. Runtime and preview use that same trajectory and existing pure timing/easing and scheduler/handle engine. Separate UITK timeline/control types preserve the existing sealed component timeline's ownership and terminal contract without a broad compatibility refactor.

New backward playback retraces the forward trajectory, including nonlinear easing and delay placement. Legacy authored backward remains unchanged. Same-Player replay replaces its execution; other Players may run concurrently on disjoint properties. Conflicting property acquisition fails before canceling or writing. Feature-wide single-execution policy was explicitly rejected as a package responsibility.

Restore cancels the owner's execution and restores original inline declarations, including their absence, while retaining prepared numeric trajectories. Dispose retains the pose. Detach invalidates the binding/preparation and settles outstanding waits; explicit rebind prepares fresh targets. This requires a panel-detach-aware Editor teardown path, rather than relying only on window OnDisable.

The cost is an additional authoring/runtime surface and explicit consumer binding/disposal. It avoids shared mutable assets, automatic global document queries, game-specific package coupling and legacy prefab migration. Initial authored properties are opacity, XY pixel translate and XY scale with finite Repeat/Yoyo/Increment. Percent translation, new Animator/custom nodes and infinite authored graphs remain outside this scope; existing APIs retain their support.
