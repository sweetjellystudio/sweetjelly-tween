# TweenPlayer Designer Guide

이 문서는 디자이너가 Unity Inspector와 UnityEvent를 통해 tween을 구성할 때 사용하는 가이드입니다.

`TweenPlayerBehaviour`는 코드에서 tween을 직접 작성하지 않고, GameObject에 컴포넌트를 붙여 tween을 재생하기 위한 도구입니다.

## 기본 개념

TweenPlayer는 하나의 property를 현재 값에서 지정한 값으로 보간합니다.

```text
Backward 값 -> Forward 값
```

예를 들어 Handle이 스크롤 중 작아지고, 멈추면 원래 크기로 돌아가야 한다면 다음처럼 생각합니다.

```text
Backward: 원래 크기
Forward: 작아진 크기
```

UnityEvent에서는 보통 다음 메서드를 연결합니다.

- `PlayForward()`: Forward 값으로 tween
- `PlayBackward()`: Backward 값으로 tween
- `Play(bool)`: true면 Forward, false면 Backward
- `SnapForward()`: Forward 값으로 즉시 변경
- `SnapBackward()`: Backward 값으로 즉시 변경
- `Kill()`: 재생 중인 tween 정지

## 가장 흔한 사용 방식

1. Tween을 적용할 GameObject를 선택합니다.
2. 필요한 TweenPlayer 컴포넌트를 추가합니다.
3. `Target Component`를 지정합니다.
4. `Forward Value`를 설정합니다.
5. 이벤트를 발행하는 View의 UnityEvent에 TweenPlayer를 연결합니다.
6. UnityEvent에서 `PlayForward`, `PlayBackward`, 또는 `Play(bool)`을 선택합니다.

`Target Component`를 비워두면 TweenPlayer가 붙어 있는 GameObject에서 필요한 컴포넌트를 찾습니다.

## UnityEvent bool 연결 주의

UnityEvent에서 bool 값을 직접 고르는 항목을 선택하면, 이벤트가 전달한 bool이 아니라 Inspector에 고정한 bool 값이 사용될 수 있습니다.

상태 이벤트의 bool 값을 그대로 받으려면 dynamic bool 항목의 `Play(bool)`을 선택해야 합니다.

예:

```text
IndexIndicatorView.HandleMovingChanged(bool)
-> TweenPlayerGroupBehaviour.Play(bool)
```

이렇게 연결하면 moving이 true일 때 Forward, false일 때 Backward로 재생됩니다.

## Single Player

단일 TweenPlayer는 하나의 property만 제어합니다.

예:

- `TransformScaleTweenPlayer`: scale 전체 변경
- `CanvasGroupFadeTweenPlayer`: CanvasGroup alpha 변경
- `GraphicFadeTweenPlayer`: Graphic alpha 변경
- `RectTransformAnchoredPositionYTweenPlayer`: anchoredPosition y만 변경

하나의 값만 바꾸고 싶으면 축 전용 Player를 우선 사용합니다.

예:

```text
Y 위치만 움직이기:
RectTransformAnchoredPositionYTweenPlayer

X scale만 바꾸기:
TransformScaleXTweenPlayer

높이만 바꾸기:
RectTransformHeightTweenPlayer
```

전체 Vector Player를 사용하면 다른 축도 함께 변경됩니다.

## Group Player

여러 TweenPlayer를 동시에 재생하려면 `TweenPlayerGroupBehaviour`를 사용합니다.

예:

```text
HandleMovingChanged(bool)
-> TweenPlayerGroupBehaviour.Play(bool)
```

Group 안에는 여러 Player를 넣을 수 있습니다.

```text
Scale Player
Fade Player
Anchored Position Y Player
```

이렇게 하면 이벤트에는 Group 하나만 연결하고, Group 내부에서 여러 tween을 동시에 실행할 수 있습니다.

Group에 들어가는 child TweenPlayer의 `Play On Enable`은 보통 꺼둡니다.

## Sequence Player

여러 TweenPlayer를 순서대로 재생하려면 `TweenPlayerSequenceBehaviour`를 사용합니다.

Sequence는 다음 상황에 적합합니다.

- 먼저 scale을 키운 뒤 fade를 실행
- 위치 이동이 끝난 뒤 색상 변경
- 짧은 UI 연출을 순서대로 구성

단순히 동시에 실행하고 싶다면 Group을 사용합니다.

## Play On Enable

`Play On Enable`은 GameObject가 활성화될 때 자동으로 tween을 실행합니다.

기본값은 꺼져 있습니다.

디자이너용 이벤트 연결에서는 보통 `Play On Enable`을 끄고, View의 UnityEvent에서 재생하는 방식을 권장합니다.

다음 상황에서만 켜는 것이 좋습니다.

- GameObject가 켜질 때 항상 등장 애니메이션을 재생해야 하는 경우
- 이벤트 연결 없이 독립적으로 재생되는 장식 tween인 경우

Group이나 Sequence로 제어되는 child Player는 `Play On Enable`을 끄는 것이 안전합니다.

## Backward 값은 언제 정해지는가

Backward 값은 TweenPlayer의 `Awake` 시점에 현재 property 값으로 캡처됩니다.

예를 들어 scale이 `(1, 1, 1)`인 상태에서 시작하면 Backward 값은 `(1, 1, 1)`입니다.

주의할 점:

- 다른 스크립트가 Awake 전에 값을 바꾸면 Backward 값도 바뀐 값으로 캡처됩니다.
- TweenPlayer가 제어하는 property는 다른 런타임 코드가 직접 계속 수정하지 않는 것이 좋습니다.
- 같은 property를 여러 TweenPlayer가 동시에 제어하지 않는 것이 안전합니다.

## 반복 입력과 상태 이벤트

이벤트를 발행하는 쪽은 상태가 바뀔 때만 이벤트를 보내는 것이 좋습니다.

```text
좋음:
false -> true 로 바뀔 때 한 번 호출
true -> false 로 바뀔 때 한 번 호출

피하기:
매 프레임 true 반복 호출
```

TweenPlayer는 이미 재생 중이어도 새 호출이 오면 현재 값 기준으로 다시 재생합니다.

## 서비스 주입

TweenPlayer가 재생되려면 `ITweenService`가 주입되어 있어야 합니다.

서비스가 주입되지 않은 상태에서 Play가 호출되면 warning 로그가 출력됩니다.

```text
[TransformScaleTweenPlayer]: Tween service is not injected. Call Initialise before playing this tween.
```

테스트 씬에서는 Bootstrapper나 별도 injector가 TweenPlayer, Group, Sequence에 `Initialise(ITweenService)`를 호출해야 합니다.

Group이나 Sequence에 서비스를 주입하면 내부 child Player에도 서비스가 전달됩니다.

## 문제 해결

### Play Mode 시작 시 alpha가 0이 됩니다

Fade Player의 `Play On Enable`이 켜져 있고 `Forward Value`가 0인지 확인합니다.

이벤트로만 제어하려면 `Play On Enable`을 끕니다.

### Y만 움직이고 싶은데 X도 바뀝니다

전체 position Player 대신 축 전용 Player를 사용합니다.

```text
RectTransformAnchoredPositionTweenPlayer
-> RectTransformAnchoredPositionYTweenPlayer
```

### UnityEvent에서 bool이 항상 같은 값으로 동작합니다

Inspector에서 고정 bool 값을 선택한 연결일 수 있습니다.

dynamic bool의 `Play(bool)` 연결을 사용합니다.

### Tween이 실행되지 않습니다

다음을 확인합니다.

- TweenPlayer가 활성화되어 있는지
- Target Component가 올바른지
- ITweenService가 주입되었는지
- UnityEvent가 올바른 메서드에 연결되어 있는지
- Group이나 Sequence에 child Player가 등록되어 있는지

