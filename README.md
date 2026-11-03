#   SWeetJelly Tween

Reusable easing, fluent Unity target tweens, execution handles/Task adapters and designer-facing TweenPlayers. Requires Unity 6.3 LTS.

##  Installation

After publishing the version tag, add the custom Git dependencies to the consuming project's Packages/manifest.json:

```json
{
  "dependencies": {
    "com.sweetjelly.tween": "https://github.com/sweetjellystudio/sweetjelly-tween.git#v2.0.0",
    "com.sweetjelly.bootstrapper": "https://github.com/sweetjellystudio/sweetjelly-bootstrapper.git#v2.0.0"
  }
}
```

The package declares Bootstrapper 2.0.0, uGUI 2.0.0 and the Audio/Animation built-in modules it uses. TMP APIs retain Unity.TextMeshPro references; no standalone TMP or Utils package is required. The example does not claim an unpublished tag is available. License: [MIT][license].

##  Code consumption

Add TweenPackageBootstrapperSo to a Bootstrapper profile and resolve ITweenService, or manually compose a concrete service and driver:

```csharp
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Framework.Unity;

var service = new TweenService();
driver.Bind(service); // TweenBehaviour supplies Unity frame deltas.
ITweenHandle handle = service.Play(
    transform.DOMove(new UnityEngine.Vector3(0f, 2f, 0f), 0.3f)
        .SetEasing(EaseType.SineOut));
await handle.ToTask(cancellationToken);
// At the owner's shutdown:
driver.Unbind(service);
service.Dispose();
```

The owner must put shutdown in its normal lifecycle/finally path, including cancellation or errors. Bootstrapper performs that ownership automatically. Manual simulation can call TweenService.Tick with an UpdateMethod and explicit scaled/unscaled deltas instead of binding a Unity driver.

ITweenService provides Play/PlayGroup and Pause/Resume/Kill controls. Handles observe one execution, expose Completed/Killed and support ToTask. The existing service.PlayAsync extensions remain available. Kill/token cancellation produces a canceled Task; normal completion succeeds. Use a new handle when replaying a case.

Use the App namespace for the fluent Play extension with an ITweenService. The Unity driver is an adapter, not the old runner described by historical TweenBucket diagrams.

##  Supported presentation adapters

*   Transform position/rotation/scale, path/follow/shake and axis variants.
*   RectTransform position/size, CanvasGroup, Graphic, Image and LayoutElement.
*   Renderer property-block values, SpriteRenderer, Camera and AudioSource.
*   Legacy Text/TextMesh and TMP font-size output.
*   Delay, condition/frame waits and AsyncOperation/legacy Animation observation.
*   Property TweenPlayers, Group, Sequence and Animator trigger/state players.

TweenCase keeps its existing fluent/protected consumption surface. Unity adapters validate targets and apply values, while pure easing/timing calculations and scheduling remain independent of Unity. Read [architecture][architecture] and [migration][migration] for the precise rules and intentional behavior changes.

Serialize service/configuration/cancellation calls; Unity targets and token cancellation use the main thread. Execution exceptions propagate after required cleanup; there is no automatic worker queue. An active playable cannot be scheduled twice, and same-lane Tick reentry is rejected. Newly scheduled callback work waits for a later Tick.

##  Designer workflows and samples

The [TweenPlayer designer guide][designer] covers capture-on-forward/configured endpoints, direction, Snap, looping, injection and composition. The [Animator guide][animator] covers trigger/state authoring; forward/backward commands do not mean reverse-time clip playback.

Import Tween Sample through Package Manager. Open a scene under Default, UI or TweenPlayer. The UI sample uses TMP; import TMP Essential Resources in a fresh consumer if its default font/settings are absent. The sample profile uses current Bootstrapper fields. The sample owns its demonstration controls and generated visuals; core presentation players belong to this package.

Tests and environment-specific verification are described in [Tests/README][tests]. See the [accepted decision][adr] and [glossary][glossary] when changing semantics. A broader TweenPlayer await surface is a [deferred follow-up][todo], not part of this version's public API. Publishing and public-tag installation verification are separate tasks.

[license]: LICENSE.md
[architecture]: ARCHITECTURE.md
[migration]: Documentation~/migration-2.0.md
[designer]: Documentation~/TweenPlayerDesignerGuide.ko.md
[animator]: Documentation~/AnimatorTweenPlayerDesignerGuide.ko.md
[tests]: Tests/README.md
[adr]: docs/adr/0001-playback-rules-and-owned-lifetime.md
[glossary]: GLOSSARY.md
[todo]: Documentation~/TODO.md
