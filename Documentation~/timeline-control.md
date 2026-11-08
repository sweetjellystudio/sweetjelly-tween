#   Prepared TweenPlayer timelines

Available in 2.2.0. A prepared timeline captures a value trajectory; a control owns one execution of it. All calls, disposal and token cancellation must occur on Unity's main thread.

##  Transfer the current playback

```csharp
using SWeetJelly.Tween.Framework.Unity;

// player is the existing serialized property, Sequence or Group player.
if (!player.CanPrepareTimeline(out string reason))
    throw new System.NotSupportedException(reason);

TweenPlayerTimeline timeline = player.PrepareTimeline();
float currentSeconds = timeline.Position;
float totalSeconds = timeline.Duration;
using TweenPlayerControl control = timeline.TakeControl();
// Transfer does not apply a pose. The previous PlayAsync is now canceled.
control.Sample(.6f);
control.Sample(.2f);
control.SampleProgress(.4f);
await control.ResumeAsync(towardEnd: true, ct: cancellationToken);
```

PrepareTimeline reads values/configuration without applying poses, dispatching events or starting children. During a supported ordinary command it returns that command's recorded trajectory; omit forward because its direction is already captured. Duration and Position can be inspected before transfer. TakeControl validates the live pose and ownership before stopping the observed run. A callback that starts replacement playback during transfer causes acquisition to fail without stopping that replacement.

Position on the prepared object reports the current owned execution, or zero when idle; an ended control retains its own last Position. Live pose validation allows float accumulation error of 0.0001 plus 0.00001 times the current value magnitude. Transfer itself never writes a pose. Re-evaluating the current time can differ from the preceding ordinary Tick by float rounding; the fractional-duration/large-coordinate regression bounds the observed difference below 0.005 pixels. Repeated samples of the same prepared time are deterministic.

For idle playback, PrepareTimeline(true) prepares the authored forward command and PrepareTimeline(false) prepares the authored backward command. Capture-on-forward starts at the prepare-time value; configured endpoints use the serialized start and destination. Backward capture without an existing captured origin is the same no-op as ordinary backward playback. Seeking backward on a forward timeline retraces that forward curve; preparing an authored backward command applies its own easing and Sequence order. These are distinct for nonlinear curves.

##  Sampling and execution lifetime

Sample takes seconds; SampleProgress takes a normalized position. Finite out-of-range values clamp to the timeline bounds; NaN/infinity throw before stopping the current automatic clock. Sampling stops the control's automatic writer and applies only values. It does not execute Played/Completed/Killed, UnityEvents, time callbacks or child Play commands. Sampling either endpoint leaves Completion pending and the control active.

ResumeAsync continues at one timeline second per clock second toward the end or beginning. It uses the captured service, clock and update lane. It does not restart child playback or replay child events. Regrab by calling Sample(control.Position); the same execution and its pending waiters remain active. Reaching the chosen endpoint through ResumeAsync completes the new execution and invokes the root Completed/serialized completed event once. An already-selected endpoint completes synchronously. Task continuations are asynchronous.

TakeControl cancels the old command and its await, including its ordinary terminal cancellation notifications. Those transfer notifications are separate from silent sampling. The new control does not emit Played when acquired. Control.Completion observes the entire new execution. ResumeAsync(ct) also observes that execution; canceling any attached token cancels it, including while regrabbed. Old tokens from the replaced ordinary command cannot affect it.

Dispose, root/member Kill, replacement Play/Snap, disable/destroy and target loss cancel the control and its await, release ownership and stop its clock. Dispose retains the displayed pose and never automatically resumes or restores. Ended controls reject Sample/ResumeAsync; repeated Dispose is harmless. Player IsPlaying remains true while a control owns it, including while parked. Runtime failures cancel the execution and retain the original synchronous/Tick exception; terminal observers cannot leave waiters unsettled.

```csharp
var timeline = player.PrepareTimeline(true);
using (var control = timeline.TakeControl()) control.SampleProgress(.6f);
timeline.Restore(); // All members must be idle; restores actual prepare-time values.
using (var control = timeline.TakeControl()) control.SampleProgress(.6f);
```

Restore does not replace the captured trajectory or dispatch events. It restores actual values at preparation, which may differ from configured time-zero endpoints. A subsequent Sample uses the same origins. Explicitly call PrepareTimeline again to capture a new trajectory. The prepared object owns no running clock or Task and needs no disposal. Keep using that timeline for retracing; a later ordinary Play is a separate command with its existing capture rules.

##  Time and composition

Idle prepared Sequence duration is the sum of child durations and Group duration is their maximum. A leaf includes its first delay and every finite repetition; later loops have no added delay, matching ordinary playback. Duration zero/negative retains the existing 0.0001-second minimum and negative delay retains the zero minimum. Repeat, Yoyo and Increment preserve their original endpoint arithmetic. Sampling each property starts from the saved baseline, applies preceding children, then evaluates the active interval. A future Sequence child cannot overwrite an earlier interval. Parallel children must have disjoint target/property keys; position and scale on one Transform are distinct keys.

An ordinary scheduler starts subsequent children/loops on a later Tick and can discard frame overshoot at a boundary. A live transfer freezes the **observed cycle spans**, including endpoint holds caused by that scheduling, before sampling. Consequently its captured Duration may be slightly longer than an idle prepared trajectory. Sequence still sums captured child spans and Group takes their maximum. This retains the actual displayed pose across nested parallel sequences without changing legacy scheduling. After transfer those spans and endpoints remain fixed; revisiting the same time gives the same pose. Future, not-yet-observed cycles use their nominal durations. At a loop boundary the next loop's start wins; the final endpoint wins at the full duration.

Every leaf in one controlled graph must share the same injected service, scaled/unscaled clock and update lane. Pure timing/loop calculations remain in Domain; target access, snapshots and lifecycle remain in Framework.Unity, and automatic clocks reuse the existing service/handle scheduler.

##  Supported capability

*   Supported leaves: exactly CanvasGroupFadeTweenPlayer, RectTransformAnchoredPositionTweenPlayer and TransformScaleTweenPlayer. Both endpoint modes, easing/AnimationCurve, finite Repeat/Yoyo/Increment and explicit/default direction are included.
*   Supported composites: nested TweenPlayerSequenceBehaviour and TweenPlayerGroupBehaviour; different durations, empty composites, and separate sequential children sharing one property are included. A shared player instance repeated in a graph is rejected; use separate authored players.
*   Rejected before acquisition: infinite loops, Animator and other/custom player types, cycles, disabled members, missing/replaced targets, mixed service/clock/lane, and parallel writes to the same property. Existing normal Play/PlayAsync and Snap continue supporting their previous player types and event meanings. No new abstract subclass hooks are required.
*   Capability and acquisition are distinct: CanPrepareTimeline validates configuration, while TakeControl also checks current live pose and run/property ownership. A live trajectory changed by external callbacks/writers can reject acquisition. Direct external property writes, Animator/other tween libraries and runtime configuration changes remain caller responsibilities; built-in sampled players guard control ownership against their own competing commands.

Sample code is in Samples~/Basic/Timeline/TweenTimelineSampleBehaviour.cs. This package does not choose gesture distances, release targets, Story progression, game effects, spring behavior or automatic navigation policy.

##  Migration from 2.1

Existing source, serialized fields, enum values, GUIDs and Snap events require no migration. Opt into the new API where intermediate state is needed. Handle canceled old PlayAsync calls during transfer; await Control.Completion or ResumeAsync for the new execution. Preserve one prepared timeline across restore/review operations instead of repeatedly capturing the current pose. Version 2.2.0 is a local working version until separately committed/published; this document does not claim a public tag exists.
