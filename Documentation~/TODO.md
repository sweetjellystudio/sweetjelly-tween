# Tween follow-up work

The player async and code-based UITK opacity/scale contracts requested on 2026-11-07 are accepted and implemented for version 2.1.0. See [the contract](async-and-uitk.md) and [ADR 0002](../docs/adr/0002-player-runs-and-uitk-lifetime.md). Verification evidence belongs to the development host; this status is not a release or public-tag installation claim. The historical 2.0 transition deferred this work; its already completed handle correctness fixes remain unchanged.

Remaining candidates require their own scope agreement:

* UIDocument/element-query Inspector authoring and automatic document binding.
* UITK translate/color and other property targets.
* Public execution result/control types beyond Task, if real consumers require them.
* Game view-transition simplification, actual Logo restoration and game dependency migration after a suitable package release.

Game page/view sequencing and screen policy remain consumer responsibilities. Do not infer publication, tag, Git operations or game source changes from this list.
