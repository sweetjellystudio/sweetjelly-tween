# TweenPlayer Designer Guide

이 문서는 디자이너가 Unity Inspector와 UnityEvent를 통해 TweenPlayer를 구성하는 방법을 설명합니다.

`TweenPlayerBehaviour`는 코드를 직접 작성하지 않고 GameObject에 컴포넌트를 추가해 특정 프로퍼티를 tween하기 위한 도구입니다.

## 기본 개념

Tween은 하나의 값을 시작값에서 목표값까지 시간에 따라 변화시키는 동작입니다.

```text
시작값 -> 목표값
```

TweenPlayer는 두 가지 방식으로 이 값을 정할 수 있습니다.

### 현재값에서 목표값으로 이동

`Endpoint Mode`를 `Capture On Forward`로 사용합니다.

```text
PlayForward()
현재값을 Origin으로 캡처
현재값 -> Forward Value

PlayBackward()
현재값 -> 캡처한 Origin
```

Inspector에서는 목표값인 `Forward Value`만 정하면 됩니다. `Backward Value`는 사용하지 않습니다.

예를 들어 현재 scale이 `0.8`이고 `Forward Value`가 `1.1`이라면 다음과 같이 동작합니다.

```text
PlayForward():  0.8 -> 1.1
PlayBackward(): 1.1 -> 0.8
```

Origin은 `Awake`가 아니라 첫 `PlayForward()` 호출 시 캡처됩니다. Forward를 재생 중에 다시 호출하거나 중간에 방향을 바꿔도 같은 Origin을 유지하며, Backward 재생이 완료되면 해제됩니다.

### 시작값과 목표값을 모두 지정

`Endpoint Mode`를 `Configured Endpoints`로 사용합니다.

```text
PlayForward():  Backward Value -> Forward Value
PlayBackward(): Forward Value  -> Backward Value
```

매번 정확히 정해진 구간을 재생해야 하는 Page Fade, 등장 및 퇴장 연출 등에 적합합니다.

예를 들어 실제 `CanvasGroup.alpha`가 1이어도 다음 설정으로 항상 `0 -> 1` Fade를 시작할 수 있습니다.

```text
Endpoint Mode  = Configured Endpoints
Backward Value = 0
Forward Value  = 1
```

## 재생 메서드

UnityEvent에서는 보통 다음 메서드를 연결합니다.

- `PlayForward()`: Forward 방향으로 tween
- `PlayBackward()`: Backward 방향으로 tween
- `Play(bool)`: true면 Forward, false면 Backward
- `SnapForward()`: Forward 결과를 즉시 적용
- `SnapBackward()`: Backward 결과를 즉시 적용
- `Kill()`: 재생 중인 tween 정지

`Capture On Forward` 상태에서 아직 Forward를 실행하지 않고 Backward를 먼저 호출하면 복귀할 Origin이 없습니다. 이 경우 값은 변경하지 않고 `Played`, `Completed`가 즉시 발생합니다.

## TweenPlayer Inspector

다음 항목은 일반 TweenPlayer Inspector에 표시되는 순서입니다.

### Timing

#### Duration

Tween이 시작값에서 목표값까지 이동하는 시간입니다.

#### Delay

재생 요청 후 tween이 시작되기 전까지 기다리는 시간입니다.

#### Unscaled Time

활성화하면 `Time.timeScale`의 영향을 받지 않습니다. 일시정지 화면이나 독립적인 UI 연출에 적합합니다.

#### Update Method

Tween이 갱신되는 Unity 실행 시점을 선택합니다.

- `Update`: 일반적인 UI와 오브젝트 연출
- `FixedUpdate`: 물리 갱신 주기에 맞춰야 하는 경우
- `LateUpdate`: 다른 Update 처리 이후 값을 반영해야 하는 경우

특별한 이유가 없다면 `Update`를 사용합니다.

### Playback

#### Play On Enable

GameObject 또는 컴포넌트가 활성화될 때 자동으로 재생합니다. 기본값은 꺼져 있습니다.

View의 UnityEvent에서 재생을 제어한다면 보통 끄는 것을 권장합니다. 다음과 같이 독립적으로 실행되는 연출에만 사용하는 것이 좋습니다.

- GameObject가 활성화될 때 항상 등장 연출을 재생하는 경우
- 별도의 이벤트 연결 없이 실행되는 장식용 tween

Group이나 Sequence가 제어하는 child Player의 `Play On Enable`은 꺼두는 것이 안전합니다.

#### Endpoint Mode

Tween의 시작값을 정하는 방식을 선택합니다.

- `Capture On Forward`: Forward 호출 시 현재값을 캡처하고 Backward에서 복구
- `Configured Endpoints`: Inspector에서 Backward와 Forward 값을 모두 지정

일반적인 상태 변화에는 `Capture On Forward`, 정확한 시작 포즈가 필요한 연출에는 `Configured Endpoints`를 사용합니다.

### Easing

#### Easing

Tween의 가속과 감속 형태를 선택합니다.

#### Has Custom Curve

활성화하면 기본 Easing 대신 `Custom Curve`를 사용합니다. 디자이너가 Animation Curve로 변화 속도를 직접 조절할 때 사용합니다.

### Loop

#### Loop Amount

전체 재생 횟수입니다.

```text
1     = 한 번 재생
2     = 두 번 재생
0 이하 = 무한 반복
```

무한 반복 Player는 직접 Kill하기 전까지 완료되지 않으므로 Sequence 안에서 사용할 때 주의합니다.

#### Loop Type

- `Repeat`: 같은 시작값과 목표값으로 반복
- `Yoyo`: 시작값과 목표값을 번갈아 재생
- `Increment`: 이전 변화량을 다음 반복 구간에 누적

### Target

#### Target Component

Tween할 프로퍼티를 가진 컴포넌트입니다. 비워두면 TweenPlayer가 붙은 GameObject에서 필요한 컴포넌트를 찾습니다.

#### Play On Enable Direction

`Play()` 또는 `Play On Enable`이 사용할 방향입니다.

- `Forward`: Forward 방향
- `Backward`: Backward 방향

`PlayForward()`와 `PlayBackward()`를 직접 호출할 때는 이 설정을 사용하지 않습니다.

#### Backward Value

`Configured Endpoints`에서 사용하는 시작 쪽 값입니다. `Capture On Forward`에서는 사용하지 않습니다.

#### Forward Value

Forward 방향의 목표값입니다. `Capture On Forward`에서는 캡처한 현재값에서 이 값으로 이동하고, `Configured Endpoints`에서는 Backward Value에서 이 값으로 이동합니다.

### Events

- `Played`: tween 재생이 시작될 때 발생
- `Completed`: tween이 정상적으로 끝날 때 발생
- `Killed`: 재생 도중 `Kill()`로 중단될 때 발생

재생 중 다시 Play를 호출하면 기존 tween은 `Killed` 이벤트 없이 교체됩니다. `Capture On Forward`는 현재 상태에서 이어서 재생하고, `Configured Endpoints`는 설정된 시작값을 다시 적용합니다.

## 기본 사용 순서

1. Tween을 적용할 GameObject를 선택합니다.
2. 필요한 TweenPlayer 컴포넌트를 추가합니다.
3. 필요하면 `Target Component`를 지정합니다.
4. `Endpoint Mode`를 선택합니다.
5. `Forward Value` 또는 양쪽 Endpoint 값을 설정합니다.
6. 이벤트를 발행하는 View의 UnityEvent에 TweenPlayer를 연결합니다.
7. UnityEvent에서 `PlayForward`, `PlayBackward`, `Play(bool)` 중 필요한 메서드를 선택합니다.

같은 프로퍼티를 여러 TweenPlayer가 동시에 제어하면 서로 값을 덮어쓸 수 있습니다. 가능하면 하나의 Player를 Forward와 Backward 양쪽에서 함께 사용합니다.

## UnityEvent bool 연결 주의

UnityEvent에서 bool 값을 직접 고르는 항목은 이벤트가 전달한 값이 아니라 Inspector에 지정한 고정값을 사용할 수 있습니다.

상태 이벤트의 bool 값을 그대로 받으려면 dynamic bool 항목의 `Play(bool)`을 선택합니다.

```text
IndexIndicatorView.HandleMovingChanged(bool)
-> TweenPlayerGroupBehaviour.Play(bool)
```

moving이 true일 때 Forward, false일 때 Backward로 재생됩니다.

## Group Player

여러 TweenPlayer를 동시에 재생하려면 `TweenPlayerGroupBehaviour`를 사용합니다.

```text
TweenPlayerGroupBehaviour
├─ Scale Player
├─ Fade Player
└─ Anchored Position Player
```

이벤트에는 Group 하나만 연결하고 여러 tween을 함께 실행할 수 있습니다. Group에 서비스를 주입하면 등록된 child Player에도 전달됩니다.

### 하나의 UnityEvent에 여러 Player를 연결하는 것과의 차이

하나의 UnityEvent에 여러 `PlayForward()`를 연결해도 모든 tween을 같은 시점에 시작할 수 있습니다. 시작만 필요하고 이후 완료 상태를 사용하지 않는 간단한 연출이라면 이 방식으로도 충분합니다.

```text
UnityEvent
├─ ScalePlayer.PlayForward()
├─ FadePlayer.PlayForward()
└─ PositionPlayer.PlayForward()
```

하지만 이 연결은 세 tween을 각각 호출할 뿐, 셋을 하나의 재생 단위로 표현하지는 않습니다. 호출한 쪽에서는 모든 tween이 끝난 시점을 알 수 없고, 방향 전환이나 Snap, Kill도 Player마다 따로 호출해야 합니다.

Group으로 묶으면 다음 동작을 하나의 Player처럼 다룰 수 있습니다.

- 한 번의 `PlayForward()`, `PlayBackward()`로 모든 child를 같은 방향으로 재생
- 한 번의 `SnapForward()`, `SnapBackward()`, `Kill()`을 모든 child에 전달
- 모든 child가 정상 완료된 뒤 Group의 `Completed` 발생
- Group 전체를 다른 Group이나 Sequence의 child로 재사용
- Group에 주입한 Tween 서비스를 모든 child에 전달

```text
UnityEvent
-> TweenPlayerGroupBehaviour.PlayForward()
   ├─ Scale
   ├─ Fade
   └─ Position
-> 모든 child 완료
-> Group.Completed
```

따라서 단순히 여러 효과를 한 번 실행하고 끝내면 UnityEvent에 직접 연결해도 되고, 여러 효과의 완료를 기다리거나 하나의 연출 단위로 재사용하려면 Group을 사용합니다.

## Sequence Player

여러 TweenPlayer를 순서대로 재생하려면 `TweenPlayerSequenceBehaviour`를 사용합니다.

```text
Scale 완료
-> Fade 완료
-> Position 완료
-> Sequence 완료
```

동시에 실행하려면 Group, 순서대로 실행하려면 Sequence를 사용합니다. Sequence 안의 무한 반복 Player는 완료되지 않으므로 다음 단계로 진행하지 않습니다.

### UnityEvent와 Completed를 직접 연결하는 것과의 차이

이전 Player의 `Completed`에 다음 Player의 `Play()`를 연결하면 직접 순서를 만들 수는 있습니다.

```text
ScalePlayer.Completed
-> FadePlayer.PlayForward()

FadePlayer.Completed
-> PositionPlayer.PlayForward()
```

한 번만 사용하는 짧은 연결이라면 이 방식도 가능합니다. 하지만 단계가 늘어나면 순서 정보가 여러 컴포넌트의 Events에 흩어지고, 중간 단계를 추가하거나 순서를 바꾸기 어려워집니다. 전체 연출의 완료와 중단을 외부에서 다루기도 어렵습니다.

Sequence는 순서를 한 배열에서 관리하고 각 child의 완료를 기다린 뒤 다음 Player를 실행합니다.

- `Players` 배열만 보고 전체 실행 순서를 확인하고 변경 가능
- 현재 단계가 완료된 후에만 다음 단계 재생
- Sequence 전체의 `Played`, `Completed`, `Killed` 사용
- `Kill()`로 현재 단계와 Sequence를 함께 중단
- 재생 중 다시 호출할 때 기존 단계를 정리하고 배열의 첫 단계부터 다시 시작
- `Reverse When Playing Backward`를 사용해 Backward 재생 시 배열 역순 실행
- Group을 한 단계로 넣어 동시 연출과 순차 연출을 조합

```text
Sequence
1. ScalePlayer
2. FadeGroup
   ├─ FadePlayer
   └─ PositionPlayer
3. AnimatorPlayer
```

따라서 child 간 연결 자체가 중요한 일회성 연출은 `Completed`를 직접 연결할 수 있고, 순서를 자주 수정하거나 전체 연출을 하나의 단위로 기다리고 중단해야 한다면 Sequence를 사용합니다.

## 반복 입력과 상태 이벤트

이벤트를 발행하는 쪽에서는 상태가 실제로 바뀔 때만 호출하는 것이 좋습니다.

```text
권장:
false -> true로 바뀔 때 한 번 호출
true -> false로 바뀔 때 한 번 호출

피하기:
매 프레임 같은 값을 반복 호출
```

`Capture On Forward`는 한 번의 Forward/Backward 왕복 동안 처음 캡처한 Origin을 유지하므로, Forward 재호출이 중간값을 새로운 Origin으로 바꾸지는 않습니다.

## 서비스 주입

일반 TweenPlayer가 재생되려면 `ITweenService`가 주입되어 있어야 합니다.

서비스가 주입되기 전에 Play가 호출되면 요청한 방향을 보관했다가 주입 후 재생하고, warning 로그를 한 번 출력합니다.

```text
[TransformScaleTweenPlayer]: Tween service is not injected. Call Initialise before playing this tween.
```

테스트 씬에서는 Bootstrapper나 별도 injector가 TweenPlayer, Group, Sequence에 `Initialise(ITweenService)`를 호출해야 합니다.

## 지원 TweenPlayer

### 값 TweenPlayer

- Audio
  - `AudioVolumeTweenPlayer`: `AudioSource.volume`
- Camera
  - `CameraFieldOfViewTweenPlayer`: `Camera.fieldOfView`
  - `CameraOrthographicSizeTweenPlayer`: `Camera.orthographicSize`
- CanvasGroup
  - `CanvasGroupFadeTweenPlayer`: `CanvasGroup.alpha`
- Graphic
  - `GraphicColorTweenPlayer`: `Graphic.color`
  - `GraphicFadeTweenPlayer`: `Graphic.color`의 alpha
- Image
  - `ImageFillAmountTweenPlayer`: `Image.fillAmount`
- Layout
  - `LayoutElementPreferredHeightTweenPlayer`: `LayoutElement.preferredHeight`
- RectTransform
  - `RectTransformAnchoredPositionTweenPlayer`: `anchoredPosition` 전체
  - `RectTransformAnchoredPositionAxisTweenPlayer`: Inspector에서 선택한 X 또는 Y 축
  - `RectTransformAnchoredPositionXTweenPlayer`: `anchoredPosition.x`
  - `RectTransformAnchoredPositionYTweenPlayer`: `anchoredPosition.y`
  - `RectTransformSizeTweenPlayer`: `sizeDelta` 전체
  - `RectTransformSizeAxisTweenPlayer`: Inspector에서 선택한 Width 또는 Height
  - `RectTransformWidthTweenPlayer`: `sizeDelta.x`
  - `RectTransformHeightTweenPlayer`: `sizeDelta.y`
- SpriteRenderer
  - `SpriteRendererColorTweenPlayer`: `SpriteRenderer.color`
  - `SpriteRendererFadeTweenPlayer`: `SpriteRenderer.color`의 alpha
- Text
  - `TextMeshColorTweenPlayer`: `TextMesh.color`
  - `TextMeshFadeTweenPlayer`: `TextMesh.color`의 alpha
  - `TextMeshFontSizeTweenPlayer`: `TextMesh.fontSize`
  - `TMPTextFontSizeTweenPlayer`: `TMP_Text.fontSize`
  - `UnityTextFontSizeTweenPlayer`: `UnityEngine.UI.Text.fontSize`
- Transform
  - `TransformPositionTweenPlayer`: 월드 `position` 전체
  - `TransformPositionAxisTweenPlayer`: Inspector에서 선택한 월드 position X, Y 또는 Z 축
  - `TransformPositionXTweenPlayer`: 월드 `position.x`만 tween
  - `TransformPositionYTweenPlayer`: 월드 `position.y`만 tween
  - `TransformPositionZTweenPlayer`: 월드 `position.z`만 tween
  - `TransformLocalPositionTweenPlayer`: `localPosition` 전체
  - `TransformLocalPositionAxisTweenPlayer`: Inspector에서 선택한 local position X, Y 또는 Z 축
  - `TransformLocalPositionXTweenPlayer`: `localPosition.x`만 tween
  - `TransformLocalPositionYTweenPlayer`: `localPosition.y`만 tween
  - `TransformLocalPositionZTweenPlayer`: `localPosition.z`만 tween
  - `TransformRotationEulerTweenPlayer`: 월드 Euler rotation 전체
  - `TransformRotationEulerAxisTweenPlayer`: Inspector에서 선택한 월드 Euler X, Y 또는 Z 축
  - `TransformRotationEulerXTweenPlayer`: 월드 `eulerAngles.x`만 tween
  - `TransformRotationEulerYTweenPlayer`: 월드 `eulerAngles.y`만 tween
  - `TransformRotationEulerZTweenPlayer`: 월드 `eulerAngles.z`만 tween
  - `TransformLocalRotationEulerTweenPlayer`: 로컬 Euler rotation 전체
  - `TransformLocalRotationEulerAxisTweenPlayer`: Inspector에서 선택한 로컬 Euler X, Y 또는 Z 축
  - `TransformLocalRotationEulerXTweenPlayer`: `localEulerAngles.x`만 tween
  - `TransformLocalRotationEulerYTweenPlayer`: `localEulerAngles.y`만 tween
  - `TransformLocalRotationEulerZTweenPlayer`: `localEulerAngles.z`만 tween
  - `TransformScaleTweenPlayer`: `localScale` 전체
  - `TransformScaleAxisTweenPlayer`: Inspector에서 선택한 scale X, Y 또는 Z 축
  - `TransformScaleXTweenPlayer`: `localScale.x`만 tween
  - `TransformScaleYTweenPlayer`: `localScale.y`만 tween
  - `TransformScaleZTweenPlayer`: `localScale.z`만 tween

축을 하나만 변경하려면 전체 Vector Player보다 Axis 선택형이나 축 전용 Player를 사용합니다. 여러 축 중 대상을 Inspector에서 바꾸려면 Axis 선택형을, 컴포넌트 이름만으로 변경할 축을 명확하게 구분하려면 X, Y, Z 또는 Width, Height 전용 Player를 사용합니다.

### Animator TweenPlayer

- `AnimatorTweenPlayerBehaviour`: Trigger를 실행하고 지정한 State의 완료를 추적

Animator Player는 프로퍼티 값을 보간하지 않고 Animator의 재생을 TweenPlayer 수명주기에 연결하는 특수 Player입니다. 자세한 사용법은 [AnimatorTweenPlayer Designer Guide](AnimatorTweenPlayerDesignerGuide.ko.md)를 참고합니다.

### 조합 Player

- `TweenPlayerGroupBehaviour`: 여러 Player 동시 재생
- `TweenPlayerSequenceBehaviour`: 여러 Player 순차 재생

## 문제 해결

### Forward를 실행했는데 값이 변하지 않습니다

현재값과 `Forward Value`가 같은지 확인합니다. 항상 정해진 시작값에서 재생하려면 `Configured Endpoints`를 선택하고 `Backward Value`와 `Forward Value`를 모두 설정합니다.

### Backward가 예상한 값으로 돌아가지 않습니다

`Capture On Forward`에서는 같은 Player의 `PlayForward()`가 먼저 호출되어야 합니다. Showing과 Hiding에 서로 다른 Player를 사용하면 캡처한 Origin을 공유하지 않습니다.

### Play Mode 시작 시 값이 갑자기 바뀝니다

`Play On Enable`이 켜져 있는지 확인합니다. 이벤트로만 제어하려면 끕니다. `Configured Endpoints`는 재생이 시작될 때 설정된 시작값을 즉시 적용합니다.

### Y만 움직이고 싶은데 X도 바뀝니다

전체 position Player 대신 축 전용 Player를 사용합니다.

```text
RectTransformAnchoredPositionTweenPlayer
-> RectTransformAnchoredPositionYTweenPlayer
```

### UnityEvent에서 bool이 항상 같은 값으로 동작합니다

Inspector에서 고정 bool 값을 선택했는지 확인하고 dynamic bool의 `Play(bool)`을 연결합니다.

### Tween이 실행되지 않습니다

다음을 확인합니다.

- TweenPlayer와 Target Component가 활성화되어 있는지
- Target Component가 올바른지
- `ITweenService`가 주입되었는지
- UnityEvent가 올바른 메서드에 연결되어 있는지
- Group이나 Sequence에 child Player가 등록되어 있는지
