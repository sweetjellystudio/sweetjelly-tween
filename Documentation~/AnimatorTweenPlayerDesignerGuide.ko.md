# AnimatorTweenPlayer Designer Guide

이 문서는 디자이너가 `AnimatorTweenPlayerBehaviour`와 Animator Controller를 연결하는 방법을 설명합니다.

공통 TweenPlayer 설정과 Group, Sequence 사용법은 [TweenPlayer Designer Guide](TweenPlayerDesignerGuide.ko.md)를 참고합니다.

## 기본 개념

`AnimatorTweenPlayerBehaviour`는 프로퍼티 값을 직접 보간하지 않습니다. Animator Controller에 Trigger를 전달해 Transition을 시작하고, 지정한 State의 재생을 관찰해 TweenPlayer의 완료 시점을 판단합니다.

```text
일반 TweenPlayer
현재값 -> Forward Value

Animator TweenPlayer
Trigger 전달 -> Animator Transition -> 지정 State 재생 및 완료 추적
```

따라서 Animator Player의 Forward와 Backward는 같은 AnimationClip의 정방향 및 역방향 재생을 뜻하지 않습니다. 디자이너가 각 방향에 사용할 Trigger와 State를 따로 정합니다.

```text
PlayForward()
-> Show Trigger
-> Showing State 추적

PlayBackward()
-> Hide Trigger
-> Hiding State 추적
```

## 권장 Animator 구성

일반적인 Page Animator Controller는 다음처럼 구성할 수 있습니다.

```text
Hidden
  -- Show Trigger --> Showing
  -- 재생 완료 ----> Shown

Shown
  -- Hide Trigger --> Hiding
  -- 재생 완료 ----> Hidden
```

Player에는 다음처럼 설정합니다.

```text
Forward Trigger    = Show
Forward State      = Showing
Forward Snap State = Shown

Backward Trigger    = Hide
Backward State      = Hiding
Backward Snap State = Hidden
```

Trigger와 State 이름이 같아도 되지만 서로 다른 대상입니다. Trigger는 Animator의 `Parameters`에 등록된 Trigger Parameter이고, State는 Animator 그래프 안의 Animation State입니다.

## Inspector

### Target

#### Target Animator

재생할 `Animator` 컴포넌트입니다. 비워두면 같은 GameObject의 Animator를 사용합니다.

#### Layer

State를 재생하고 추적할 Animator Layer의 인덱스입니다. `Base Layer`는 일반적으로 0입니다.

Show와 Hide는 보통 별도 Layer가 아니라 같은 Layer 안의 State로 만듭니다.

```text
Layers
└─ Base Layer: 0
   ├─ Showing State
   ├─ Shown State
   ├─ Hiding State
   └─ Hidden State
```

### Forward

#### Forward Trigger

`PlayForward()`가 Animator에 전달하는 Trigger Parameter입니다. Forward 재생 전에 반대쪽 Backward Trigger는 Reset됩니다.

#### Forward State

Forward Trigger가 실행된 뒤 Player가 진입과 완료를 추적할 State입니다. 등장 AnimationClip이 연결된 `Showing` 같은 State를 지정합니다.

#### Forward Snap State

`SnapForward()`가 즉시 적용할 결과 State입니다. `Shown`처럼 Forward 연출이 끝난 상태를 지정합니다. 비워두면 `Forward State`의 마지막 시점을 사용합니다.

### Backward

#### Backward Trigger

`PlayBackward()`가 Animator에 전달하는 Trigger Parameter입니다. Backward 재생 전에 반대쪽 Forward Trigger는 Reset됩니다.

#### Backward State

Backward Trigger가 실행된 뒤 Player가 진입과 완료를 추적할 State입니다. 퇴장 AnimationClip이 연결된 `Hiding` 같은 State를 지정합니다.

#### Backward Snap State

`SnapBackward()`가 즉시 적용할 결과 State입니다. `Hidden`처럼 Backward 연출이 끝난 상태를 지정합니다. 비워두면 `Backward State`의 마지막 시점을 사용합니다.

### Playback

#### Play On Enable

활성화하면 GameObject가 켜질 때 지정된 방향으로 Animator 재생을 요청합니다. Group이나 Sequence에서 제어한다면 끕니다.

#### Play On Enable Direction

`Play()`와 `Play On Enable`에서 사용할 Forward 또는 Backward 방향입니다.

#### State Entry Timeout

Trigger를 전달한 뒤 지정 State에 진입할 때까지 기다리는 최대 시간입니다. 이 시간 안에 진입하지 못하면 warning을 출력하고 Player를 Kill 처리합니다. 0으로 설정하면 Timeout을 사용하지 않습니다.

Trigger 조건이나 Transition 설정이 잘못됐을 때 Sequence가 영원히 기다리는 것을 막기 위해 기본값을 유지하는 것을 권장합니다.

### Events

- `Played`: 유효한 Trigger를 Animator에 전달한 직후 발생
- `Completed`: 지정 State가 정상적으로 완료되거나 해당 State에서 나간 뒤 발생
- `Killed`: 명시적으로 Kill하거나, State 진입 Timeout 또는 Animator 비활성화로 추적이 중단될 때 발생

## 완료 판단

Player는 Trigger 호출 자체를 완료로 판단하지 않습니다.

```text
Trigger 전달
-> 지정 State 진입 확인
-> Transition 종료 확인
-> State 재생 시간 완료
-> Completed
```

추적 State에 진입한 뒤 다른 State로 나가도 해당 재생이 끝난 것으로 판단합니다. 따라서 Forward State와 Backward State에는 실제로 재생 완료를 기다릴 Animation State를 지정해야 합니다.

Loop State는 재생 시간이 한 바퀴를 넘어도 자동 완료되지 않습니다. Animator가 다른 State로 전환하거나 Player가 Kill될 때까지 기다리며, 실행 중 warning이 출력됩니다. Sequence의 한 단계로 사용할 State는 Loop를 끄는 것이 안전합니다.

State 이름에는 `Showing` 같은 짧은 이름을 사용할 수 있습니다. 하위 State Machine에 들어 있는 State이거나 이름이 겹칠 가능성이 있다면 다음처럼 전체 경로를 사용합니다.

```text
Base Layer.UI.Showing
```

## Snap

Snap은 Trigger와 Transition을 실행하지 않고 지정한 Snap State의 마지막 시점을 즉시 평가합니다.

```text
SnapForward()
-> Forward Snap State의 마지막 포즈 즉시 적용

SnapBackward()
-> Backward Snap State의 마지막 포즈 즉시 적용
```

Snap State가 비어 있으면 해당 방향 State의 마지막 포즈를 사용합니다. Snap은 재생이 아니므로 Player의 `Played`, `Completed` 이벤트를 발생시키지 않습니다. Group이나 Sequence를 Snap하면 상위 Group 또는 Sequence가 자체 `Played`, `Completed`를 발생시킵니다.

## Kill과 재호출

Animator 전체를 멈추기 위해 `Animator.speed`를 변경하면 다른 State와 Layer도 영향을 받습니다. 이 때문에 Animator Player의 `Kill()`은 Animator를 정지하거나 이전 포즈로 되돌리지 않고 현재 완료 추적만 중단한 뒤 `Killed`를 발생시킵니다.

```text
Kill()
-> Animator는 Controller 설정대로 계속 동작
-> Player의 완료 추적 종료
-> Killed
```

재생 중 반대 방향이나 같은 방향을 다시 호출하면 기존 추적은 `Killed` 없이 교체되고 새 Trigger와 State를 추적합니다.

## 일반 TweenPlayer와 다른 점

Animator Player는 Animator를 직접 사용하므로 일반 값 TweenPlayer처럼 Endpoint, Duration, Easing, Loop를 설정하지 않습니다.

- 시작과 목표 상태: Animator State와 Transition으로 구성
- 재생 시간: AnimationClip 길이로 구성
- 전환 시간: Transition Duration으로 구성
- 완료 후 전환: Exit Time과 Animator Controller로 구성
- 변화 곡선: AnimationClip의 Curve로 구성

`ITweenService` 주입도 필요하지 않지만 Group과 Sequence의 child로는 동일하게 사용할 수 있습니다.
