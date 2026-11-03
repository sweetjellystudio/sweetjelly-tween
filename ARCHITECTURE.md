#   Tween architecture

Tween provides reusable interpolation, scheduling, execution control and Unity-facing presentation players. DDD responsibility boundaries and Clean Architecture dependency direction guide the package; these folders are not separate bounded contexts.

##  Responsibilities and dependency direction

Contract owns playable, service, handle, completion and easing ports plus update/easing values. Domain owns pure easing and TweenTiming calculations: finite input validation, minimum duration/delay, nonnegative delta and normalized progress/delay advancement. App owns lane scheduling, handle/group orchestration and Task adaptation. Their asmdefs have noEngineReferences=true; Domain references Contract and App references Domain/Contract.

Framework.Unity references App/Domain/Contract and Unity.TextMeshPro. It owns target validation/value application, Update/FixedUpdate/LateUpdate delivery, concrete cases/extensions, curve configuration and designer-facing property/Group/Sequence/Animator players. Framework.Bootstrapper composes the concrete service and Unity driver through the supported Bootstrapper contracts. Samples own their demonstration UI and injection, not the reusable player implementations.

TweenCase keeps its existing type, assembly, fluent returns and protected state fields as a compatibility boundary for derived cases. Pure timing calculations are delegated to Domain without keeping a second mutable copy of playback state. Target invocation, public completion hooks and legacy state fields stay with this bridge. This is a deliberate boundary, not a claim that every state transition is already a standalone Domain model. See the [accepted decision][adr].

The manifest declares Bootstrapper 2.0.0, uGUI 2.0.0 and the Audio/Animation built-in modules actually used by runtime code. TMP uses the uGUI package and retains the Unity.TextMeshPro assembly reference. No Utils source or asmdef reference remains necessary. Public Unity target types are intentional presentation adapters, not technology leaked into the pure assemblies.

##  Execution and callbacks

The caller serializes service calls, configuration, notifications and cancellation. Unity outputs/players require the Unity main thread; there is no worker queue or automatic marshalling. Scaled and unscaled deltas are supplied to the service. Nonfinite time inputs are rejected before state mutation; negative deltas become zero. With scaled-zero skipping enabled, scaled progress and callbacks stop while unscaled playback continues. Target validation still permits destroyed-target cleanup.

An active playable cannot be scheduled twice. Playback added during a callback starts on a later Tick. Control operations act on the entries present when called, including pending entries for KillAll. Same-lane Tick reentry is rejected. Execution callback failure stops that Tick, kills the failing execution, settles its cancellation and restores scheduler state; unrelated entries remain for later Ticks. Cleanup must preserve the original execution exception.

Progress reaching the end is distinct from successful value application and the terminal notification. A handle records one execution's terminal result and detaches its notifier; replaying the underlying case does not re-open that handle. Completed and Killed are mutually exclusive until explicit reactivation/reset. Terminal observers are cleanup notifications: settle all waiters before propagating the first observer exception, without rewriting an outcome that was already published. Operational callbacks still fail the Tick immediately after required cleanup.

ToTask succeeds for normal completion and cancels for Kill/token cancellation, including an already killed handle. It releases both event subscriptions and token registration. Custom playables that need live completion notification implement INotifyTweenCompletion, and custom handles must publish their terminal events consistently. A non-notifying playable can be controlled/polled, but has no live event stream for an outstanding Task adapter.

##  Ownership and Unity players

The Bootstrapper asset is reusable configuration, not an independent global runtime scope. Existing service registrations or bound drivers are rejected. Registration/binding failure rolls back owned state. Shutdown disposes the owned concrete service, cancels its runs, removes only a matching registry entry/binding and destroys only a driver created by that bootstrap. A borrowed unbound driver survives. Disposed services reject new playback.

Players own their active handles and child subscriptions. Generic property players retain capture-on-forward/configured endpoints, direction, Snap and loop behavior. Group/Sequence track their own child playback, cancel on child Kill, and ignore stale callbacks from replaced playback. They clean up subscriptions and only stop work they started. Animator players use controller triggers/states; they are not property interpolation or reverse-time clip playback.

The game owns page/view sequencing and game-specific animation policy. Its repeated player-event-to-Task adapters motivate a [deferred package follow-up][todo]; no new TweenPlayer asynchronous API is part of this transition.

[adr]: docs/adr/0001-playback-rules-and-owned-lifetime.md
[todo]: Documentation~/TODO.md
