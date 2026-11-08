# Tween 2.0 validation

Run package tests through a Unity 6.3 LTS consumer with `com.sweetjelly.tween` in the project's `testables`. Use the declared dependencies and import the sample through Package Manager. The development host records the exact Editor patch, executed counts, source fingerprint and separate consumer/Player results.

EditMode covers pure timing/easing, finite inputs, delay, negative delta, scaled-zero rules, scheduler mutation/reentry, failed execution cleanup, duplicate/replayed cases, terminal observers, handle/Task cleanup, mixed handle groups, partial-start rollback, bootstrap/driver ownership, and property/Group/Sequence lifetime. The tests use public APIs; composition arrays use Unity's serialized authoring boundary.

PlayMode covers actual driver Update, stopped scaled time with continuing unscaled playback, destroyed-target cancellation, and the existing capture/configured endpoints and Snap regressions. Actual Default/UI/TweenPlayer samples, TMP, Inspector and Animator controller checks complement these tests. Windows ordinary Mono and IL2CPP Players are separate checks; an Editor pass does not establish another backend or platform.

The original test descriptions follow. They describe baseline coverage and are not the complete 2.0 verification matrix.

# Tween Tests

이 폴더는 `com.sweetjelly.tween` 패키지를 현재 Unity 테스트 프로젝트에서 검증하기 위한 테스트를 담는다.

## EditMode

`EditMode` 테스트는 Unity 프레임 실행에 의존하지 않고 `TweenService.Tick(...)`으로 시간을 직접 주입해 tween 실행 엔진과 easing 계산을 검증한다.

- `TweenDomainTest`
  - `EasingMath.Clamp01(...)`이 범위를 clamp하는지 검증한다.
  - `EasingFunctionFactory`가 대표 easing function을 생성하고 계산하는지 검증한다.
  - `CustomEasingFunction`이 입력값을 clamp해서 evaluator에 전달하는지 검증한다.
  - null evaluator가 `ArgumentNullException`을 발생시키는지 검증한다.

- `TweenServiceTest`
  - float tween이 tick에 따라 값을 갱신하고 완료 이벤트를 발생시키는지 검증한다.
  - delay가 끝나기 전에는 tween callback이 실행되지 않는지 검증한다.
  - pause/resume이 tween 진행을 멈추고 다시 시작하는지 검증한다.
  - kill이 active tween을 종료하고 killed 이벤트를 발생시키는지 검증한다.
  - tween group이 모든 handle 완료 후 completed 이벤트를 발생시키는지 검증한다.
  - explicit update method로 재생한 tween이 해당 lane에서만 tick되는지 검증한다.
  - null playable 입력이 예외를 발생시키는지 검증한다.

- `TweenPackageIntegrationTest`
  - `TweenPackageBootstrapperSo`이 holder object에 `TweenBehaviour`를 추가하는지 검증한다.
  - `TweenPackageBootstrapperSo`이 `ITweenService`를 `ServiceRegistry`에 등록하는지 검증한다.
  - `TweenPackageBootstrapperSo.Shutdown(...)` 후 service 등록이 해제되는지 검증한다.

## PlayMode

`PlayMode` 테스트는 Unity `MonoBehaviour` driver인 `TweenBehaviour`가 실제 프레임에서 `TweenService`를 tick하는지 검증한다.

- `TweenBehaviourFlowTest`
  - `TweenBehaviour.Bind(...)`로 연결된 `TweenService`가 `Update()`에서 tick되어 tween을 완료하는지 검증한다.

현재 테스트는 대표 tween case인 `FloatTweenCase`와 `TweenCases.DOFloat(...)`를 중심으로 실행 엔진을 검증한다. Transform, UI, Renderer 등 구체 extension은 대상 Unity component별 integration test가 필요할 때 추가한다.


## Player async and UITK follow-up

TweenPlayerAsyncTest covers execution replacement, token ownership, loop-wide completion, synchronous completion, Snap, exception/observer behavior, composition child replacement and custom opt-in. VisualElementTweenTest uses actual runtime UIDocument panels for interpolation, delay, style capture, detach, cancellation and validation. TweenPlayerAsyncLifetimeTest checks actual disable/destroy callbacks in PlayMode. Keep EditMode lifecycle assumptions separate from runtime evidence. Results and isolated consumer observations are recorded by the host, not asserted by this document.

## Prepared timeline control

TweenPlayerTimelineTest covers pose-preserving live transfer, silent endpoint sampling, regrab/resume, captured endpoints/easing/delay, sequential same-property restoration, nested composition, finite loops and observed frame-boundary holds, capability rejection, stale controls/tokens and terminal observer replacement. TweenPlayerTimelineLifetimeTest checks actual frame progression, disable/destroy and released targets while the manual clock is parked. Run them with the ordinary package regressions; fresh NUnit XML, sample/Inspector observations and separate declared-dependency/Player results belong to the host evidence.
