# 프로젝트 ε 개발 일지 — Day04

## 개발 목표

프로젝트 ε의 핵심 Snake 조작을 시작한다.
플레이어 Head가 자동 전진하고 좌우 입력으로 곡선 회전하며,
Body가 Head의 현재 위치가 아니라 실제 이동 경로를 일정 간격으로 따라오도록 구현한다.

또한 커밋 히스토리 정리 과정에서 누락된 Day02~03 플레이 기반 코드를 복구하여
Input, Camera, HUD, Settings와 Snake 이동을 다시 하나의 플레이 환경으로 연결한다.

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `be41f44101cd397e363c87eb21cb409a641a288a` |
| 기준 커밋 제목 | `4` |

---

## 진행 내용

### 1. Day02~03 플레이 기반 복구

커밋 히스토리를 정리한 뒤 현재 `main`에서 누락되어 있던 플레이 기반 스크립트를 다시 추가했다.

복구 대상의 주요 역할:

- Player 입력 읽기
- Camera Follow
- HUD / Timer
- InputBindingManager
- 키 리바인딩 저장 / 불러오기
- Settings UI
- Settings Open 시 Pause / Close 시 Resume
- Input System UI EventSystem
- GameManager Root 구조 보정

이후 Day04 Snake 시스템이 기존 키 설정과 HUD 환경을 그대로 사용할 수 있도록 연결했다.

### 2. Snake 자동 전진 구현

`SnakeMovement`를 추가하여 별도 전진 입력 없이 Head가 계속 현재 진행 방향으로 이동하도록 구현했다.

기본 이동 속도:

```text
Move Speed = 3
```

GameManager가 존재할 경우 `Playing` 상태에서만 이동하도록 구성하여
Settings와 Pause 상태에서 이동이 정지된다.

### 3. 좌우 곡선 회전 구현

`PlayerInputReader.TurnInput`을 Snake 이동에 연결했다.

기본 입력:

```text
A / ← = 좌회전
D / → = 우회전
```

Day03에서 변경한 키 설정이 존재할 경우 `InputBindingManager`의 현재 키를 사용한다.

기본 회전 속도:

```text
Turn Speed = 145 deg/s
```

위치 자체를 좌우로 이동시키는 방식이 아니라
현재 진행 방향을 시간에 따라 회전시키는 방식으로 구현했다.

### 4. Snake 이동 경로 기록 시스템

`SnakePathHistory`와 `SnakePathRecorder`를 추가했다.

Head가 이동할 때 일정 거리 이상 이동한 위치를 경로 데이터로 기록하고,
Body가 사용할 수 있도록 Head 뒤쪽의 특정 거리 위치를 샘플링한다.

기본값:

```text
Minimum Point Distance = 0.04
Maximum Path Length = 18
Initial Path Length = 14
```

초기에는 Head 뒤쪽으로 충분한 직선 경로를 미리 생성하여
게임 시작 직후 Body가 모두 Head 위치에 겹치지 않도록 구성했다.

### 5. 거리 기반 Body Follow 구현

`SnakeBodyFollower`를 추가하여 Body가 Head를 직접 추적하지 않고
기록된 이동 경로에서 각 Body의 거리만큼 뒤쪽 위치를 사용하도록 구현했다.

기본 Body 간격:

```text
Segment Spacing = 0.58
```

각 Body는 다음 구조로 배치된다.

```text
Head
↓ 0.58
Body_01
↓ 0.58
Body_02
↓ 0.58
Body_03
```

Body 위치뿐 아니라 해당 경로의 진행 방향도 계산하여
곡선 이동 시 각 Body의 회전도 경로에 맞게 변경된다.

### 6. 실행 순서 정리

Head 이동과 경로 기록, Body Follow가 같은 프레임에서 올바른 순서로 처리되도록 구성했다.

```text
SnakeMovement.Update
        ↓
Head 이동
        ↓
SnakePathRecorder.LateUpdate
        ↓
경로 기록
        ↓
SnakeBodyFollower.LateUpdate
        ↓
Body 위치 / 회전 갱신
```

`SnakePathRecorder`와 `SnakeBodyFollower`는 실행 순서를 구분하여
Body가 갱신되기 전에 최신 Head 경로가 먼저 기록되도록 했다.

### 7. 테스트 Snake Sprite 구성

실제 캐릭터 에셋이 준비되기 전 Snake 동작을 확인할 수 있도록
Head와 Body용 Debug Sprite를 추가했다.

```text
DebugSnakeHead
DebugSnakeBody
```

현재 Sprite는 이동과 Body Follow 검증 목적의 임시 에셋이다.

### 8. Day04 자동 Scene 구성

`ProjectEpsilonDay4Setup`을 추가하여 필요한 Scene 구조를 자동 생성 및 연결하도록 구성했다.

기본 구조:

```text
===Managers===
  GameManager
  InputBindingManager

===Gameplay===
  Player
    PlayerInputReader
    SnakeMovement
    SnakePathRecorder

  SnakeBody
    SnakeBodyFollower
    Body_01
    Body_02
    Body_03

===Environment===

===UI===
  HUDCanvas
  EventSystem

Main Camera
  CameraFollow2D
```

### 9. 기존 Debug 이동 정리

Snake 이동과 동시에 실행될 수 있는 기존 자유 이동 테스트 코드를 제거 대상으로 처리했다.

정리 대상:

```text
PlayerDebugMovement
기존 Day02 / Day03 자동 Setup
기존 Manager Root Fix 보조 Setup
```

Day04 자동 Setup에서 현재 Snake 구조를 기준으로 Scene을 다시 정리하도록 구성했다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeMovement` 존재
- Head 자동 전진 코드 존재
- `TurnInput` 기반 좌우 회전 연결
- GameManager Playing 상태 확인
- `SnakePathHistory` 경로 저장 / 거리 샘플링 구현
- `SnakePathRecorder` 경로 기록 구현
- `SnakeBodyFollower` 거리 기반 Body Follow 구현
- Body 진행 방향 기반 회전 처리
- Day04 자동 Scene Setup 추가
- Debug Snake Head / Body Sprite 추가
- Day02~03 입력 / Camera / HUD / Settings 기반 코드 복구
- Day01 GameManager 생성 구조를 Root Component 방식으로 보정

현재 저장소에는 GitHub Actions / CI 상태 체크가 구성되어 있지 않다.
따라서 Unity Editor 실제 컴파일과 Play Mode 동작은 로컬 Unity Console에서 최종 확인한다.

---

## 4일차 완료 기준

- [x] Snake 자동 전진 구조 구현
- [x] 좌우 곡선 회전 구현
- [x] 기존 키 리바인딩 입력과 연결
- [x] Head 이동 경로 기록 구현
- [x] 거리 기반 경로 샘플링 구현
- [x] Body 3개 Follow 구조 구현
- [x] Body 방향 회전 구현
- [x] Camera Follow 연결 기반 복구
- [x] HUD / Settings 기반 복구
- [x] Debug Snake Sprite 추가
- [x] Day04 자동 Scene 구성
- [x] 기존 Debug 자유 이동 정리 기반 추가

---

## 다음 개발 방향

다음 5일차에서는 현재 테스트 Body Follow 구조를 실제 Snake 몸 관리 시스템으로 확장한다.

주요 목표:

- Head / Body / Tail 역할 구분
- 시작 Body 3칸 규칙 적용
- 런타임 Body 추가
- 런타임 Body 제거
- 최대 Body 20칸 제한
- Body 목록과 실제 Transform 동기화
- 이후 자기 충돌 / 공유 HP / 무기 슬롯 시스템이 사용할 공통 Body 관리 API 준비

Day04의 경로 추적 구조는 그대로 유지하고,
Day05부터 길이가 실제 게임 시스템에 의해 변하는 Snake 구조로 발전시킨다.
