# TweenPlayer async playback and UI Toolkit

Available from package version 2.1.0. Tag publication and consumer migration are separate steps.

## Player playback

```csharp
await player.PlayAsync(ct);          // The player's configured default direction.
await player.PlayAsync(false, ct);   // Explicit backward command.
```

These entry points start and await one command, including every finite loop. They return Task, not a public playback handle. Infinite loops wait until stopped; looping Animator states retain the controller's existing completion rules. All calls and token cancellation must occur on Unity's main thread. There is no automatic thread marshalling.

Normal completion succeeds. Kill, disable/destroy, replacement by another synchronous or async Play, child cancellation/replacement and either Snap direction cancel the old Task. A previous token or delayed callback cannot stop or complete the replacement. Group/Sequence cancel when an owned child run is replaced and only stop their other owned runs. Existing synchronous methods, serialized fields, GUIDs and UnityEvents remain; in particular Group/Sequence Snap still emits its existing events, without completing the interrupted Task.

An already canceled token returns a canceled Task without starting or replacing anything. An inactive, uninjected or invalidly configured player fails immediately with InvalidOperationException rather than queuing an async start. Unsupported custom subclasses fail with NotSupportedException. Async composition requires children that support the execution hooks; legacy children remain usable through synchronous composition.

Start exceptions are thrown synchronously after owned cleanup. Execution exceptions still propagate from Tick and cancel the affected await, matching handle awaiting. Terminal observer failures do not rewrite an already published outcome. Synchronous completion is supported, including empty compositions and backward capture with no origin. Continuations are asynchronous; use the Unity synchronization context when awaiting before accessing Unity objects.

Animator Kill cancels observation, not the controller's actual animation. The consumer controls pose restoration and controller transitions. This API does not introduce a universal force-complete command.

## Custom player opt-in

Existing subclasses compile without new abstract members. To opt in, override SupportsAsyncPlayback to true, validate readiness through ValidateAsyncPlayback(bool? forward), and call BeginPlayback once per command after ending the previous owned run. Retain its opaque PlaybackRun in delayed callbacks. IsCurrentPlayback(run) rejects stale callbacks. Apply final state and clear owned runtime state, then call CompletePlayback(run) or CancelPlayback(run), before the existing NotifyCompleted/NotifyKilled and UnityEvents. Do not create another run per loop.

Kill and OnDisable must stop owned work and cancel its token. Releasing a terminal run must never mutate a run started by an observer. OnDestroy provides a base fallback; subclasses overriding it must call base.OnDestroy(). These hooks do not infer identity from the legacy Played event, which property players still emit once per loop.

## VisualElement targets

```csharp
// After attaching the element and resolving any USS-dependent starting values:
var fade = element.DOFade(0f, .3f, unscaledTime: true);
var scale = element.DOScale(new Vector3(.4f, 1f, 1f), .4f);
await Task.WhenAll(service.Play(fade).ToTask(ct), service.Play(scale).ToTask(ct));
```

Import System.Threading.Tasks, UnityEngine, SWeetJelly.Tween.Contract and SWeetJelly.Tween.Framework.Unity for this example. Use your injected ITweenService; the TweenCase targets work independently of TweenPlayer async support.

VisualElementFadeTweenCase and VisualElementScaleTweenCase capture their starting values at construction, not after delay. Concrete inline values take precedence over resolvedStyle. Attach the element and allow required style resolution before construction. Scale is a Vector3 factor (zero and negative factors are allowed), not a percentage. Nonfinite targets are rejected; opacity output is clamped to 0..1. Existing easing, delay, duration, scaled/unscaled and update-lane controls apply.

Only the exact element is owned as a target. Its detach, including a panel move, cancels immediately and releases the event subscription. Descendant detach does not cancel a parent target. Reattachment does not revive the old run, and a recreated UIDocument is never queried or rebound automatically. Create a new case for its new element. Starting on a detached target is rejected. Completion, Kill and failures release detach subscriptions.

Values are written as inline styles and retained after completion/cancellation; USS defaults are not restored automatically. The consumer owns transform-origin, multiple tweens of the same property, and conflicts with USS transitions. Use distinct properties or explicitly end previous work. This package does not change UI Builder authoring visibility, display, picking, layout, UIDocument activation or element queries. Inspector binding and translate/color targets remain separate follow-ups.

The runtime uses the built-in com.unity.modules.uielements 1.0.0 dependency. Unity-facing types remain in Framework.Unity; the Contract/Domain/App assemblies remain engine-independent. See Samples~/Basic/UITK for a thin consumer example.
