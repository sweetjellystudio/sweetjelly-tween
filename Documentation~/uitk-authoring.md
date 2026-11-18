#   Saved UI Toolkit Tween authoring

Available in local development version 2.3.0, requiring Unity 6.3 LTS. Git publication and game migration are separate. Existing Component Players, custom subclasses, serialized fields/GUIDs and Snap events keep their existing contracts.

##  Designer workflow

Create an asset with **Assets > Create > SWeetJelly > Tween > UI Toolkit Tween**. Double-click it, use its **Open UITK Tween Editor** Inspector button, or open **Window > SWeetJelly > Tween UI Toolkit**. Assign the production UXML document. The reference is remembered for editing; runtime uses the caller's actual root.

Select/add a property node and pick its target in the element tree or preview. An empty target selects the supplied root. Descendants need names unique inside that root. Repeated roots may use identical internal names. Missing, renamed or duplicate names fail validation; the package never searches the whole UIDocument for the first matching element or silently rewrites production UXML names.

Property controls provide opacity, XY translate in local UI pixels, and XY scale factors. Scale/translate retain Z; transform-origin stays in UXML/USS. Parents own layout while internal presentation elements own their tween transform. Percentage translate capture is unsupported, including percentage starting styles; use a pixel-valued inner presentation element. Values must be finite. Opacity output clamps to 0–1; scale factors may be zero or negative. Avoid external writers and USS transitions on the same animated property.

Choose configured start/end or capture-current start/end, duration, initial delay, easing or custom curve, total repetitions and Repeat/Yoyo/Increment. Nonpositive duration retains the existing 0.0001-second minimum and negative delay becomes zero; repetitions must be positive. Add nested Sequence/Group nodes, select children and use Up/Down/Remove. Composite nodes place their children; timing/repetition controls belong to property leaves. Infinite, Animator/custom nodes, graph cycles/shared node instances and parallel writers to one property are rejected.

Typed numeric values commit on Enter or focus loss; intermediate decimal text stays editable. Curve controls and selection/reorder are immediate authoring actions. Save persists the definition, not preview inline values. Undo/Redo restores edits. Change assets and reopen to reuse the saved definition and document.

Play, Pause, Reverse and the seconds slider use the runtime's prepared evaluator on a private clone and private service. Editor preview advances wall-clock seconds while runtime uses its selected clock. At matching timeline seconds the values match. Reset, document reload/change and window close restore original inline declarations and release preview work. Production UXML/USS and game trees are never preview outputs.

##  Consumer binding and playback

```csharp
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;
using UnityEngine.UIElements;

// Inject ITweenService through the consumer's existing composition.
// The consumer creates/attaches the root and allows USS/layout to resolve first.
UITKTweenPlayer player = motionAsset.Bind(actualRoot, service,
    updateMethod: UpdateMethod.Update, unscaledTime: true);
try
{
    UITKTweenTimeline timeline = player.PrepareTimeline(); // Read/capture only.
    await player.PlayAsync(cancellationToken);            // One identified forward execution.
    await player.PlayAsync(false, cancellationToken);     // Retrace the same prepared curve.
    player.ApplyStart();                                 // Silent, cancels this owner's run.
    player.ApplyEnd();
    player.Restore();                                    // Original inline declarations, capture retained.
}
finally { player.Dispose(); }                            // Cancel work; retain displayed pose.
```

Definitions are reusable data. Binding retains the supplied root and injected service, while each Player independently owns its prepared values, execution identity, subscriptions and tokens. No MonoBehaviour subclass, separate UIDocument, Feature type or named game command is required. A MonoBehaviour consumer calls Dispose at its disable/destroy boundary; a C# consumer does so at its own lifetime boundary. The package does not decide visibility, focus, input, layout, navigation or document activation.

The default graph clock is scaled Update. Bind can override it to unscaled and/or FixedUpdate/LateUpdate for the whole graph. Every required service lane must be ticked by the consumer's driver. Scaled progress stops at timeScale zero; select unscaled for paused UI. The sample owns a local service/driver only to stay independently runnable; production consumers can inject the existing service.

First Play implicitly prepares if necessary. PrepareTimeline during an active execution returns its existing captured trajectory; idle PrepareTimeline explicitly captures again and invalidates older prepared objects. Captured values and copied curves stay fixed until that new preparation or rebinding. Delay never recaptures a start. Sequence current-start leaves use the calculated endpoint of earlier leaves; future leaves do not overwrite earlier intervals. Group duration is its maximum child duration; Sequence duration is its sum. New runtime composition follows this nominal continuous trajectory; legacy component frame-boundary holds and authored backward are unchanged.

##  Direct control and terminal outcomes

```csharp
var timeline = player.PrepareTimeline();
var previous = player.PlayAsync(cancellationToken);
using (UITKTweenControl control = timeline.TakeControl())
{
    // previous is now canceled. This is a distinct control execution.
    control.SampleProgress(.6f);
    control.Sample(.2f);             // Seconds. Repeating a time produces the same values.
    control.Pause();                 // Stop the automatic writer, retain this execution.
    await control.ResumeAsync(towardEnd: true, ct: newToken);
    await control.Completion;
}
timeline.Restore();
```

PlayAsync succeeds on normal endpoint application. Replacement, Kill, detach, Rebind, Dispose and cancellation cancel the owned await; old tokens cannot stop a replacement. A pre-canceled Play token starts/replaces nothing. Target/configuration/conflict failures throw synchronously before partial execution. Runtime application errors propagate through the existing Tick failure path and cancel that execution's await after cleanup. All calls, disposal and token cancellation use Unity's main thread; no worker marshalling is provided.

TakeControl preserves current timeline time without applying a pose and cancels the previous playback. Sample, including endpoints, never dispatches playback/child/game events or settles Completion. Sample also parks an automatic writer. ResumeAsync advances one timeline second per selected clock second and completes the control at the selected endpoint. Pause retains its pending wait and cancellation ownership. Dispose cancels while retaining pose. Finite out-of-range sample positions clamp; NaN/infinity throw before stopping the automatic clock.

ApplyStart/ApplyEnd and Restore cancel the owner's current execution, then apply silently. They do not report successful completion of an interrupted await. Restore reinstates the original inline values/keywords/absent declarations, allowing current USS defaults to apply again. The numeric trajectory stays captured, so it remains retraceable after restoration. These APIs do not alter legacy Snap events.

##  Ownership and reconnection

Same-Player Play replaces its previous execution. Different Players can run concurrently on disjoint element/property pairs, even within the same root. Conflicts with another UITK Player, including a parked control, fail before cancellation or writes; explicitly cancel the old owner first. Ownership covers these bound Players, not arbitrary external code/cases/animation libraries or USS transitions. Consumers coordinate those writers.

Detach of the root or a prepared target, including panel moves, cancels immediately and invalidates prepared capture. Reattachment does not revive it. Call Rebind(newRoot), allow attachment/style/layout readiness, and prepare again; every old timeline/control remains invalid. Rebind also cancels prior work and removes old subscriptions. Same-name elements in a recreated document never inherit old executions.

##  Sample and validation

Import **UI Toolkit Tween Authoring** in Package Manager and open UITKAuthoringSample.unity. Show/Hide/Focus are sample-owned slots calling public APIs, not built-in Tween concepts. Two clones share the same authored assets but own independent roots and execution state. Edit Show.asset in the dedicated window, save, then replay from the same sample code. The parent owns placement; title/art own motion. Use the slider for silent Show sampling and Restore to recover the prepared inline baseline.

Runtime regression tests cover real panels, copies, generation invalidation, writer conflicts, clocks, loops, endpoint/control semantics and stale tokens. Authoring tests cover Unity asset serialization, actual pointer/key input across Editor updates, delayed decimal commit, selection/reorder and preview restoration. Curve field value-path checks are distinct from physical curve-handle interaction. Compilation, automated tests, observed Editor/sample behavior, isolated declared-dependency consumption and ordinary Player backends are separate evidence categories recorded by the development host.
