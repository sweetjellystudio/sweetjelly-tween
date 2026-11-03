---
status: accepted
date: 2026-11-03
---

#   Preserve consumption while separating timing rules and playback ownership

Tween 2.0 requires Unity 6.3, the current Bootstrapper and uGUI/TMP dependency path. The real game consumes fluent case extensions, handles/ToTask, serialized players and a custom TweenPlayerBehaviour subclass. Moving or replacing the public base class, protected fields or serialized types would impose avoidable migration costs. We retain that bridge and move pure timing calculations into the engine-independent Domain assembly without duplicating mutable state. A wholesale new player hierarchy or UI Toolkit rewrite is outside this change.

Playback is synchronous in one serialized caller context, with Unity targets and cancellation on the main thread. Duplicate active scheduling and same-lane Tick reentry are rejected. Callback additions wait until a later Tick; KillAll covers the active/pending membership at its call. Execution failure cancels that run and preserves the first exception while restoring internal state. Silent swallowing, an automatic background queue and continuing other executions after a failure were rejected. Terminal observer notification is distinguished from value execution: release all waiters, preserve the published outcome, then propagate the first observer error.

Bootstrap configuration assets can be shared by profiles but cannot create conflicting active runtime owners. Failed registration/binding rolls back; shutdown cancels owned work and releases only matching registrations/bindings and created drivers. Borrowed objects and replacement owners survive. Each handle and player composition tracks its own playback lifetime so stale callbacks cannot finish a replacement operation.

The approved Q1–Q10 agreement preserves existing public/protected/serialized consumption and documents the intentional behavior changes in the migration guide. A broader TweenPlayer Task entry point is recorded as a separate TODO; game view-transition/Logo policy is not moved into this package.
