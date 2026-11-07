#   Tween package instructions

Use the installed unity-package coordinator. Read [architecture][architecture], [timing and ownership decisions][adr] and [migration][migration] before changing playback rules, callbacks, handles, player lifecycle, assembly references or public/protected APIs.

*   Preserve existing public/protected names, named arguments, enum values, script/asset GUIDs and serialized player fields. Keep existing TMP, uGUI, Transform, Animator and custom TweenPlayer subclass consumption supported.
*   Contract/Domain/App remain Unity-independent. Domain owns timing/easing calculations; App owns scheduling and execution handles. Keep Unity targets and lifecycle integration in Framework.Unity.
*   Use a serialized caller context. Unity target access and token cancellation occur on the Unity main thread. Keep synchronous failure propagation, explicit same-lane Tick reentry rejection, and cleanup of failed executions.
*   Snapshot control-operation membership; new playback from callbacks waits for a later Tick. Protect bootstrap registrations and driver bindings by identity, cancel owned work, and preserve borrowed objects.
*   A handle observes one execution. Keep terminal notifications, Task settlement and event/token cleanup consistent. Guard player composition against stale child callbacks and duplicate terminal outcomes.
*   Run package EditMode/PlayMode tests, actual sample/Inspector/Animator checks, isolated declared-dependency consumption, and the selected ordinary Player backends on Unity 6.3. Store environment-specific evidence in the development host.
*   For player async hooks, replacement/Snap settlement or VisualElement target changes, read [the run and UITK contract](Documentation~/async-and-uitk.md) and [ADR 0002](docs/adr/0002-player-runs-and-uitk-lifetime.md). Preserve existing designer guides. [Deferred work][todo] tracks remaining authoring/consumer scope.

[architecture]: ARCHITECTURE.md
[adr]: docs/adr/0001-playback-rules-and-owned-lifetime.md
[migration]: Documentation~/migration-2.0.md
[todo]: Documentation~/TODO.md
