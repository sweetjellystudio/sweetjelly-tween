# UITK tween consumer

Import the Tween Sample. In a runtime scene, add a UIDocument with PanelSettings and TweenUITKSampleBehaviour to a GameObject. No VisualTreeAsset is required: the thin consumer builds its preview controls at runtime and owns its service/driver. Enter Play Mode, click Replay to fade and compress the blue box, click Cancel midway to retain its current values, then Replay again. Disable/re-enable the object to check cleanup and reconstruction.

Optionally assign an execution-aware TweenPlayer to observe PlayAsync alongside the UITK cases. Its loop/direction/Animator settings remain its own. The sample injects its service; do not share that player with a second live service owner.

The UI defaults remain visible in authoring assets because the preview state is runtime-only. For real UXML/USS targets, resolve required styles after attachment before constructing cases. This sample has no automatic element query or Inspector binding. See Documentation~/async-and-uitk.md for lifecycle, conflict and cancellation responsibilities.
