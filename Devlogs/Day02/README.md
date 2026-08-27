# 프로젝트 ε 개발 일지 — Day02

## 개발 목표

플레이어 입력, 테스트 이동, 카메라 추적, 기본 HUD를 구축하여
이후 Snake 이동 시스템을 구현할 수 있는 플레이 테스트 환경을 완성한다.

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `fbdc40bdea2a8528a8ee464029a1ed4bba711ac3` |
| 기준 커밋 제목 | `2` |

---

## 진행 내용

### 1. 플레이어 입력 시스템 구축

Unity Input System을 기반으로 키보드 입력을 읽는 `PlayerInputReader`를 구현했다.

현재 테스트 입력:

- `A / ←` : 왼쪽
- `D / →` : 오른쪽
- `W / ↑` : 위쪽 테스트 이동
- `S / ↓` : 아래쪽 테스트 이동
- `Shift` : 테스트 가속

입력 처리와 실제 이동 로직을 분리하여
추후 Snake 전용 이동 시스템으로 교체하기 쉽도록 구성했다.

### 2. 테스트 플레이어 이동 구현

`PlayerDebugMovement`를 추가하여
Input System과 카메라 추적 기능을 확인할 수 있는 임시 이동 환경을 만들었다.

현재 이동은 Day02 테스트를 위한 기능이며,
추후 Snake 자동 전진과 곡선 회전 시스템이 구현되면 제거할 예정이다.

### 3. 카메라 추적 시스템 구현

`CameraFollow2D`를 추가하고 Main Camera가 Player를 부드럽게 따라가도록 구성했다.

주요 특징:

- Player 위치 추적
- 카메라 Z 위치 유지
- `SmoothDamp` 기반 부드러운 이동
- Player 회전에 따른 카메라 회전 없음

### 4. 기본 HUD 구축

`HUDCanvas`를 생성하고 게임 플레이에 필요한 기본 정보를 표시하도록 구성했다.

표시 항목:

- HP
- 현재 몸 길이 / 최대 몸 길이
- Level
- XP
- Stamina
- 게임 진행 시간

현재 기본 표시값:

```text
HP 100 / 100
몸 3 / 20
Lv. 1
XP 0 / 10
Stamina 100 / 100
00:00
```

HP, 몸 길이, XP, Level, Stamina는 아직 실제 게임 시스템과 연결하기 전의 테스트 값이며,
게임 진행 시간은 실제로 증가하도록 구현했다.

### 5. HUD 표시 로직 분리

HUD 문자열 구성을 `HUDTextFormatter`로 분리하고,
`HUDController`는 전달받은 값을 화면에 표시하는 역할을 담당하도록 구성했다.

이를 통해 이후 실제 HP, 경험치, 몸통, 스태미나 시스템이 구현될 때
UI 구조를 크게 변경하지 않고 연결할 수 있도록 준비했다.

### 6. Day02 자동 Scene 세팅

`ProjectEpsilonDay2Setup`을 추가하여
Unity Editor에서 Day02 구성 요소를 자동 생성 및 연결하도록 만들었다.

자동 구성 대상:

```text
===Gameplay===
└─ Player
   ├─ SpriteRenderer
   ├─ PlayerInputReader
   └─ PlayerDebugMovement

===UI===
└─ HUDCanvas
   ├─ BodyHP
   ├─ BodyCount
   ├─ Level
   ├─ Experience
   ├─ GameTimer
   └─ Stamina

Main Camera
└─ CameraFollow2D
```

테스트 플레이어용 Sprite도 자동 생성하도록 구성했다.

### 7. GameManager 루트 구조 문제 수정

Day02 테스트 과정에서 다음 Unity 경고를 확인했다.

```text
DontDestroyOnLoad only works for root GameObjects
or components on root GameObjects.
```

기존 구조에서는 `GameManager`가 `===Managers===`의 자식이었기 때문에
`DontDestroyOnLoad(gameObject)`를 정상적으로 사용할 수 없었다.

기존:

```text
===Managers===
└─ GameManager
```

수정:

```text
===Managers===  ← GameManager 컴포넌트
```

`===Managers===` 자체를 Root GameObject로 유지하고
그 오브젝트에 `GameManager` 컴포넌트를 직접 연결하도록 변경했다.

또한 Day01 자동 Scene 생성 코드도 동일한 구조를 사용하도록 수정했으며,
기존 Scene을 자동 보정하는 `ProjectEpsilonManagerRootFix`를 추가했다.

---

## 확인 결과

최신 저장소 상태를 기준으로 다음 항목을 확인했다.

- Player 입력 및 테스트 이동 스크립트 존재
- Main Camera에 `CameraFollow2D` 연결
- Camera Follow의 Target이 Player로 연결
- HUDCanvas 및 HUD 항목 구성
- `GameTimerUI` 및 `HUDController` 연결
- `===Managers===`가 Scene Root에 존재
- `===Managers===` 자체에 `GameManager` 컴포넌트 연결
- 기존 자식 `GameManager` 제거
- Day01 자동 생성 코드의 GameManager 구조 수정
- 기존 Scene용 GameManager Root 보정 코드 추가

GitHub Actions / CI는 현재 구성되어 있지 않으므로,
Unity Editor의 실제 컴파일 및 Console 상태는 로컬 실행 결과를 기준으로 최종 확인한다.

---

## 2일차 완료 기준

- [x] Player 입력 처리 구조 구현
- [x] 좌우 입력 감지
- [x] Boost 입력 감지
- [x] 테스트 Player 생성
- [x] 테스트 이동 구현
- [x] Camera Follow 구현
- [x] Player와 Camera 연결
- [x] 기본 HUD 생성
- [x] 게임 시간 표시
- [x] HUD 표시 로직 분리
- [x] Day02 자동 Scene 세팅 구현
- [x] GameManager Root 구조 수정
- [x] Day01 자동 생성 구조 수정
- [x] 기존 Scene GameManager 구조 보정

---

## 다음 개발 방향

다음 단계부터 프로젝트 ε의 핵심인 Snake 이동 시스템을 개발한다.

주요 목표:

- 자동 전진
- 좌우 곡선 회전
- 즉시 방향 전환 방지
- 회전 속도 제한
- 이후 몸통 Follow 시스템을 연결할 수 있는 이동 경로 기반 준비

Day02에서 사용한 `PlayerDebugMovement`는
Snake 이동 시스템이 안정적으로 동작하는 시점에 제거한다.
