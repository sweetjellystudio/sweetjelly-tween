# Tween follow-up work

The player async and code-based UITK opacity/scale contracts requested on 2026-11-07 are accepted and implemented for version 2.1.0. See [the contract](async-and-uitk.md) and [ADR 0002](../docs/adr/0002-player-runs-and-uitk-lifetime.md). Verification evidence belongs to the development host; this status is not a release or public-tag installation claim. The historical 2.0 transition deferred this work; its already completed handle correctness fixes remain unchanged.

Remaining candidates require their own scope agreement:

* UITK rotation/color/layout and percentage translation, beyond the saved authoring path's opacity/pixel translate/scale.
* Infinite or Animator/custom nodes in saved definitions, if real finite-sampling and lifecycle contracts can be defined.
* Additional public execution result types beyond the existing Task and timeline/control surfaces, if real consumers require them.
* Game view-transition simplification, actual Logo restoration and game dependency migration after a suitable package release.

Saved UITK assets, a production-UXML dedicated editor, explicit root binding and runtime/preview evaluation are included in 2.3.0; see [UITK authoring](uitk-authoring.md) and [ADR 0004](../docs/adr/0004-uitk-authored-definitions-and-bound-players.md). This supersedes the previous authoring/translate deferral. The caller supplies the actual runtime root; automatic global document lookup is not part of that contract. Existing Component Inspector graphs remain supported without migration.

Game page/view sequencing and screen policy remain consumer responsibilities. Do not infer publication, tag, Git operations or game source changes from this list.
