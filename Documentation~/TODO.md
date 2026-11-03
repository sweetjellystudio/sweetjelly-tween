#   Tween follow-up work

##  TweenPlayer await adapter

Status: deferred candidate, requested on 2026-11-03. Investigate after the Unity 6.3 / 2.0 transition. This is a work item, not an approved new API design.

The sweetjelly-2048 consumer adapts TweenPlayer Played/Completed/Killed events into Task completion and token cancellation in its view-transition adapter. Its custom Logo player also coordinates handles through ToTask, Task.WhenAll and a playback generation. Repeated event subscription, cancellation and stale-completion handling suggest a reusable package-level playback adapter. View/page sequencing and game-specific animation policy remain consumer responsibilities.

*   Evaluate a supported Task-based playback entry point for TweenPlayer, including forward/backward playback and custom subclasses. Preserve the existing synchronous and UnityEvent entry points.
*   Settle completion, Kill, cancellation, disable/destroy, replay/supersession, Snap, failed start and callback-exception behavior. Every started operation must reach a documented terminal result.
*   Bind completion to the correct playback generation, release event/token registrations, and specify Unity main-thread requirements. Avoid transferring cancellation from an old operation to a replacement playback.
*   Check equivalent behavior for property players, Group, Sequence and Animator players, including synchronous completion and infinite loops.
*   Compare a small extension adapter against a broader playback-operation contract before choosing an interface. Demonstrate reduced consumer boilerplate with acceptance fixtures and package-owned tests.
*   Plan game migration separately after the package contract and release are available. Do not move game view-transition abstractions or the Logo animation policy into this package.

The current 2.0 work already includes correctness fixes for handle ToTask termination and subscription cleanup. This follow-up concerns the broader TweenPlayer await surface and consumer simplification; it does not defer those agreed correctness fixes. No new asynchronous public API or sweetjelly-2048 source migration is authorized by this TODO.
