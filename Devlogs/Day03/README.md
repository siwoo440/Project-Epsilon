# 프로젝트 ε 개발 일지 — Day03

## 개발 목표

Day01~02에서 구축한 프로젝트 기반, 입력, 테스트 Player, Camera, HUD를 통합 점검하고
게임 설정에서 조작 키를 직접 변경하고 저장할 수 있는 키 리바인딩 시스템을 추가한다.

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `2d423cd68119237249ed7186232afd61d2b3c6a5` |
| 기준 커밋 제목 | `3` |

---

## 진행 내용

### 1. Day01~02 기반 통합 점검

기존 프로젝트 구조와 Day02 플레이 테스트 환경을 기준으로
입력, Player, Camera Follow, HUD, GameManager 구조가 함께 동작할 수 있도록 유지했다.

주요 확인 대상:

- Player 입력 구조
- 테스트 Player 이동
- Camera Follow
- HUD / Timer
- GameManager 상태 관리
- `===Managers===` Root 구조

### 2. 입력 관리 구조 중앙화

기존 `PlayerInputReader`의 하드코딩 입력을
`InputBindingManager`를 통해 읽도록 변경했다.

게임에서 사용하는 핵심 입력은 다음 세 가지로 분리했다.

- 좌회전
- 우회전
- Boost

기본 키:

```text
좌회전 = A
우회전 = D
Boost = Left Shift
```

Day02 테스트용 W/S 상하 이동은 Debug 이동 확인용으로 유지했다.

### 3. 키 리바인딩 시스템 구현

Unity Input System의 Interactive Rebinding을 사용하여
플레이 중 설정 화면에서 키를 직접 변경할 수 있도록 구현했다.

지원 기능:

- 좌회전 키 변경
- 우회전 키 변경
- Boost 키 변경
- 새 키 입력 대기
- ESC로 변경 취소
- 변경 즉시 적용

키 변경은 키보드 입력을 대상으로 한다.

### 4. 키 설정 저장 / 불러오기

변경한 키 설정을 `PlayerPrefs`에 저장하도록 구현했다.

저장 구조:

```text
ProjectEpsilon.InputBindings
```

Input System의 Binding Override 데이터를 JSON으로 저장하고,
게임 실행 시 다시 불러오도록 구성했다.

따라서 게임을 종료했다가 다시 실행해도
사용자가 변경한 키 설정이 유지된다.

저장 데이터가 정상적으로 읽히지 않을 경우
기본 키 설정을 사용하도록 예외 처리를 추가했다.

### 5. 기본 키 설정 복원

Settings 화면에서 `기본값 복원` 기능을 사용할 수 있도록 구현했다.

복원 시:

```text
좌회전 = A
우회전 = D
Boost = Left Shift
```

로 되돌아가며 저장된 Binding Override 정보도 제거한다.

### 6. Settings UI 구축

HUD에 Settings 버튼과 설정 패널을 추가했다.

구성:

```text
HUDCanvas
├─ 기존 HUD
├─ SettingsButton
└─ SettingsPanel
   ├─ Title
   ├─ TurnLeftLabel
   ├─ TurnLeftButton
   ├─ TurnRightLabel
   ├─ TurnRightButton
   ├─ BoostLabel
   ├─ BoostButton
   ├─ Status
   ├─ ResetButton
   └─ CloseButton
```

각 입력 버튼에는 현재 설정된 키가 표시된다.

키 변경 중에는:

```text
새 키를 누르세요. ESC로 취소합니다.
```

상태 메시지를 표시하도록 구성했다.

### 7. Settings와 게임 일시정지 연결

Settings 화면을 열면 현재 게임이 `Playing` 상태인지 확인하고
게임을 `Paused` 상태로 전환하도록 구성했다.

Settings 화면을 닫을 때
설정을 열기 전 게임이 진행 중이었다면 다시 `Playing` 상태로 복귀한다.

이를 통해 키 변경 중 Player와 게임 시간이 계속 진행되는 것을 방지했다.

### 8. Input System UI EventSystem 구성

Settings 버튼과 UI 입력을 Unity Input System으로 처리할 수 있도록
`EventSystem`과 `InputSystemUIInputModule`을 Scene에 구성했다.

기존 `StandaloneInputModule`이 존재하는 경우 제거하고
새 Input System UI 모듈을 사용하도록 자동 보정한다.

### 9. Day03 자동 Scene 세팅

`ProjectEpsilonDay3Setup`을 추가하여
필요한 관리자와 UI를 자동 생성 및 연결하도록 구성했다.

자동 구성:

```text
===Managers===
├─ GameManager
└─ InputBindingManager

===UI===
├─ HUDCanvas
│  ├─ 기존 HUD
│  ├─ SettingsButton
│  └─ SettingsPanel
└─ EventSystem
   └─ InputSystemUIInputModule
```

---

## 확인 결과

최신 저장소 상태를 기준으로 다음 구조를 확인했다.

- `InputBindingManager` 구현
- 좌회전 / 우회전 / Boost InputAction 생성
- Interactive Rebinding 구현
- ESC 리바인딩 취소 구현
- Binding Override 저장 구현
- Binding Override 불러오기 구현
- 기본 키 복원 구현
- `PlayerInputReader`와 InputBindingManager 연결
- Settings UI Controller 구현
- Settings Open 시 Pause 처리
- Settings Close 시 Resume 처리
- Day03 자동 Scene 구성 코드 추가

현재 GitHub Actions / CI는 구성되어 있지 않으므로
Unity Editor 실제 컴파일 및 런타임 Console 상태는 로컬 실행으로 최종 확인한다.

---

## 3일차 완료 기준

- [x] 기존 입력 시스템 통합
- [x] 핵심 입력 중앙 관리
- [x] 좌회전 키 변경 기능
- [x] 우회전 키 변경 기능
- [x] Boost 키 변경 기능
- [x] ESC 키 변경 취소
- [x] 키 설정 저장
- [x] 키 설정 자동 불러오기
- [x] 기본값 복원
- [x] Settings UI 구성
- [x] Settings와 Pause / Resume 연결
- [x] Input System UI EventSystem 구성
- [x] Day03 자동 Scene 세팅 추가

---

## 다음 개발 방향

다음 단계에서는 프로젝트 ε의 핵심 조작인 Snake 이동 시스템을 구현한다.

주요 목표:

- Player 자동 전진
- 좌우 곡선 회전
- 즉시 방향 전환 방지
- 회전 속도 제한
- 현재 리바인딩된 좌우 조작키를 Snake 이동에 그대로 사용
- 기존 `PlayerDebugMovement`를 Snake 이동 검증 이후 제거

Day03까지 구축한 입력 관리 구조를 재사용하여
Day04부터 실제 게임 플레이 감각을 만드는 단계로 진행한다.
