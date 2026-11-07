#   Migrating to Tween 2.0

Requires Unity 6.3 LTS, Bootstrapper 2.0.0 and uGUI 2.0.0. Keep Unity.TextMeshPro references for TMP APIs; the standalone TMP package declaration and unused Utils dependency are removed. Runtime Audio and Animation modules are declared explicitly. Read Bootstrapper's migration guidance before upgrading old profiles; this package does not rewrite game assets.

Public/protected type and member names, named parameters, enum values, script GUIDs and serialized player fields remain. Existing custom TweenPlayerBehaviour subclasses, fluent extensions, TMP font-size tweens, capture-on-forward/configured endpoints, Snap and loop modes remain supported. Existing designer guides describe those authoring workflows.

Behavior changes:

*   A case killed outside Tick can be replayed without duplicate lane entries. Reuse during its old Tick must wait until the lane has released it. Reset cancels an active run before resetting it. Frame waits reset their frame count for each activation.
*   Duplicate active Play and same-lane Tick reentry fail explicitly. Playback added by a callback waits until a later Tick. KillAll includes pending entries present at that call.
*   Execution callbacks propagate failures after canceling that run and restoring scheduler state. Other runs can continue on a later Tick. Normal terminal observer errors do not undo a completed result; all terminal waiters are notified before the first observer failure propagates.
*   Each handle remains terminal after its own run. ToTask after Kill cancels immediately; completion/cancellation releases event/token registrations. Use a new handle for another playback.
*   Nonfinite time values are rejected. Duration retains its 0.0001-second minimum, delay its zero minimum, and negative deltas become zero. Scaled-zero skipping also pauses scaled frame/condition callbacks; use unscaled mode to continue them.
*   Bootstrap rejects an existing service or bound driver. Failure/shutdown releases only owned state and cancels owned runs. A disposed concrete TweenService rejects new playback.
*   Handle groups cancel on any child Kill and settle their Tasks; an empty handle group is already completed. Failed PlayGroup rolls back only handles started by that call.
*   Group/Sequence cancel their operation when a child is killed, clean up their own children and subscriptions, and ignore stale completion callbacks from replacement playback.

Serialize calls and cancellation in the same execution context. For Unity targets use the main thread, including token cancellation. There is no automatic thread marshalling. Custom live playables need INotifyTweenCompletion for event-based awaiting; custom handles must publish terminal events.

The proposed broader TweenPlayer await adapter and consumer simplification are [deferred][todo]. Current handle correctness fixes are included here. Git publication and actual public-tag installation verification are separate tasks.

[todo]: TODO.md


## Migrating from 2.0 to 2.1

Version 2.1.0 includes the separately approved player async and UITK contracts. No serialized fields or GUIDs are migrated. Existing synchronous custom subclasses remain supported; safe async consumption requires explicit protected run hooks. See [async and UITK](async-and-uitk.md) and CHANGELOG 2.1.0. The earlier deferral above describes the historical 2.0 transition, not the current working tree. The package version is 2.1.0; actual tag publication and game dependency updates remain separate steps.
