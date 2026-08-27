# 프로젝트 ε 개발 일지 — Day10

## 개발 목표

Day09에서 구현한 적 처치 → XP Gem → Head 흡수 → XP 누적 흐름을
실제 레벨업과 Snake 성장으로 연결한다.

이번 일차의 핵심 목표:

- 레벨 시스템
- XP 요구량 곡선
- 초과 XP 이월
- 연속 레벨업 순차 처리
- 레벨업 이벤트
- 레벨업 시 게임 일시정지
- Body +1 성장
- Body 최대 20 상태 보상
- 최대 Body 상태에서 공유 HP 완전 회복
- Level / XP HUD 실제 동기화
- LevelUpPanel
- Continue 후 게임 재개
- Day11 무기 후보 UI 확장을 위한 기반 구성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `fbccd0d4dfdecc066bbe82132ef09230999c0006` |
| 기준 커밋 제목 | `10` |
| 이전 커밋 | `d5bfe08353b873831136d0a9c550ffbc94d30028` |

---

## 진행 내용

### 1. SnakeExperience를 실제 레벨 시스템으로 확장

Day09의 `SnakeExperience`는 XP 누적과 HUD 표시까지만 담당했다.

Day10에서는 다음 상태를 추가했다.

```text
CurrentLevel
CurrentExperience
RequiredExperience
LevelUpPending
```

이제 XP가 요구량에 도달하면 단순 누적에서 끝나지 않고
실제 레벨업 요청으로 이어진다.

### 2. XP 요구량 곡선

기본 요구 XP:

```text
10
```

성장 배율:

```text
1.12
```

계산식:

```text
Required XP
= Base XP × 1.12^(Level - 1)
```

현재 계산은 올림 방식으로 적용한다.

예:

```text
Lv.1 → 약 10 XP
Lv.2 → 약 12 XP
Lv.3 → 약 13 XP
Lv.4 → 약 15 XP
```

최종 밸런스 수치는 후반 조정 단계에서 변경할 수 있다.

### 3. Level 관련 이벤트 추가

`SnakeExperience`에 다음 이벤트를 추가했다.

```text
ExperienceChanged
LevelChanged
LevelUpRequested
```

역할:

```text
ExperienceChanged
→ XP HUD 갱신

LevelChanged
→ Level HUD 갱신

LevelUpRequested
→ 성장 / UI / Pause 처리 요청
```

XP 데이터와 UI / 성장 로직을 직접 결합하지 않고
이벤트로 연결하도록 구성했다.

### 4. 초과 XP 이월

레벨업 시 요구 XP만 차감하고 남은 XP는 그대로 유지한다.

예:

```text
XP 8 / 10
+5 XP

↓

총 13 XP

↓

10 XP 사용
Lv.2

↓

XP 3 / 12
```

따라서 큰 XP Gem을 획득해도 초과 XP가 사라지지 않는다.

### 5. 연속 레벨업 순차 처리

한 번에 많은 XP를 얻어 여러 레벨 요구량을 만족하더라도
모든 레벨업을 즉시 처리하지 않는다.

```text
대량 XP 획득
↓
첫 Level Up 요청
↓
LevelUpPanel
↓
Continue
↓
남은 XP 확인
↓
다음 Level Up 요청
```

이 방식으로 레벨업 UI를 하나씩 처리한다.

Day11에서 무기 후보 3개를 각 레벨업마다 보여주기 위한 기반이 된다.

### 6. SnakeLevelUpController 추가

레벨업 처리 전용 `SnakeLevelUpController`를 추가했다.

주요 역할:

- `LevelUpRequested` 구독
- Body 성장
- 최대 Body 보상
- GameManager Pause
- LevelUpPanel 표시
- Continue 처리
- 연속 레벨업 연결
- 모든 레벨업 완료 후 Resume

구조:

```text
SnakeExperience
↓
LevelUpRequested
↓
SnakeLevelUpController
├─ SnakeBodyManager
├─ SnakeHealth
├─ GameManager
└─ LevelUpPanelController
```

### 7. 레벨업 시 Body +1

현재 Body가 최대치보다 작으면
기존 `SnakeBodyManager.TryAddBody()`를 사용해 Body를 한 칸 추가한다.

```text
Body 3
↓
Level Up
↓
Body 4
```

Body Count 변경 이벤트가 기존 Weapon Slot 시스템으로 전달되므로
새 Body에는 Empty Weapon Slot도 함께 추가된다.

```text
Body +1
↓
Weapon Slot +1
↓
새 Slot = Empty
```

### 8. 최대 Body 20 처리

Body가 이미 최대치인 경우 추가 Body를 만들지 않는다.

```text
Body 20 / 20
↓
Level Up
↓
Body 증가 없음
```

대신 공유 HP를 완전 회복한다.

```text
Body 20
HP 35 / 100

↓

Level Up

↓

Body 20
HP 100 / 100
```

따라서 최대 Body 상태에서도 레벨업 보상이 사라지지 않는다.

### 9. GameManager Pause 연동

레벨업 발생 시 기존 `GameManager.PauseGame()`을 사용한다.

```text
Level Up
↓
GameManager.PauseGame()
↓
GameState.Paused
↓
Time.timeScale = 0
```

별도 TimeScale 관리 코드를 만들지 않고
기존 게임 상태 관리 시스템을 재사용했다.

### 10. Continue 후 Resume

레벨업 UI에서 Continue를 누르면
현재 레벨업을 완료 처리한다.

추가 레벨업 조건이 없다면:

```text
LevelUpPanel Hide
↓
GameManager.ResumeGame()
↓
Time.timeScale = 1
```

로 정상 플레이 상태로 돌아간다.

초과 XP로 다음 레벨업이 필요한 경우에는
게임을 재개하지 않고 다음 LevelUpPanel을 이어서 표시한다.

### 11. LevelUpPanelController 추가

레벨업 UI 전용 `LevelUpPanelController`를 추가했다.

구조:

```text
LevelUpPanel
├─ Title
├─ LevelText
├─ GrowthText
└─ ContinueButton
```

표시 정보:

```text
LEVEL UP!
Lv. X
Body 성장 정보
Continue
```

Body가 증가한 경우:

```text
Body 3 → 4
Empty Weapon Slot +1
```

최대 Body 상태에서는:

```text
Body MAX 20
HP FULL RESTORE
```

를 표시한다.

### 12. Day11 확장을 고려한 UI 구조

Day10의 LevelUpPanel은 단순 Continue 방식이지만
다음 Day11에서 무기 후보 3개를 연결할 수 있도록
레벨업 처리와 UI를 별도 클래스로 분리했다.

현재:

```text
LevelUpPanel
└─ Continue
```

Day11 확장 예정:

```text
LevelUpPanel
├─ Weapon Candidate 01
├─ Weapon Candidate 02
└─ Weapon Candidate 03
```

### 13. XP / Level HUD 통합

기존 `SnakeExperienceHUDPresenter`를 확장했다.

이제 다음 두 이벤트를 모두 구독한다.

```text
ExperienceChanged
LevelChanged
```

HUD 흐름:

```text
SnakeExperience
↓
SnakeExperienceHUDPresenter
↓
HUDController
├─ SetExperience()
└─ SetLevel()
```

예:

```text
Lv. 1
XP 8 / 10

↓

Level Up

↓

Lv. 2
XP 3 / 12
```

### 14. Progression Debug 입력 추가

레벨업 기능을 빠르게 확인할 수 있도록
`SnakeProgressionDebugControls`를 추가했다.

```text
L
→ XP +10
```

이를 이용해 실제 적을 반복 처치하지 않고도
레벨업 흐름을 바로 테스트할 수 있다.

### 15. Day10 자동 Scene 구성

`ProjectEpsilonDay10Setup`을 추가했다.

자동 처리 항목:

- Game Scene 열기
- Player / SnakeBody 검색
- `SnakeExperience` 확장 설정
- XP 기본 요구량 10 설정
- XP 성장 배율 1.12 설정
- Level 1 / XP 0 초기화
- `SnakeExperienceHUDPresenter` 재연결
- `SnakeLevelUpController` 추가
- `SnakeProgressionDebugControls` 추가
- `LevelUpPanel` 자동 생성
- Continue Button 생성
- Scene 저장
- 이전 Day09 Setup 정리

### 16. Day09 Setup 제거

Day10 자동 구성과 이전 Setup이 동시에 Scene을 수정하는 상황을 막기 위해
Day09 Setup은 Day10 적용 과정에서 제거했다.

```text
ProjectEpsilonDay9Setup.cs
ProjectEpsilonDay9Setup.cs.meta
```

Day09의 실제 전투 / XP 시스템은 그대로 유지한다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeExperience` Level 시스템 확장
- CurrentLevel 추가
- RequiredExperience 계산
- 기본 XP 10
- XP 성장 배율 1.12
- 초과 XP 이월
- LevelUpPending 처리
- 연속 레벨업 순차 처리
- `LevelChanged` 이벤트
- `LevelUpRequested` 이벤트
- `SnakeLevelUpController` 추가
- 레벨업 시 Body +1
- 기존 Body 최대 20 제한 재사용
- 최대 Body 상태에서 공유 HP 완전 회복
- 기존 GameManager Pause / Resume 재사용
- `LevelUpPanelController` 추가
- LevelUpPanel 자동 생성
- Continue Button 처리
- 레벨업 완료 후 Resume
- 추가 레벨업이 있으면 Pause 유지
- XP HUD 실제 요구량 표시
- Level HUD 실제 값 연동
- `SnakeProgressionDebugControls` 추가
- L 키 XP +10 Debug
- Day10 자동 Scene Setup
- Day09 Setup 제거

현재 GitHub 커밋에는 CI / Actions 상태 체크가 등록되어 있지 않다.
따라서 Unity Editor 실제 컴파일 및 Play Mode 동작은
로컬 Unity 환경에서 최종 확인한다.

---

## 10일차 완료 기준

- [x] Level 시스템
- [x] XP 요구량 곡선
- [x] XP ×1.12 성장
- [x] 초과 XP 이월
- [x] 연속 Level Up 순차 처리
- [x] Level 이벤트
- [x] 레벨업 시 Pause
- [x] Body +1
- [x] Weapon Slot 자동 증가 기반 유지
- [x] 최대 Body 20 처리
- [x] 최대 Body에서 HP 완전 회복
- [x] LevelUpPanel
- [x] Continue 후 Resume
- [x] Level HUD 연동
- [x] XP HUD 연동
- [x] Debug XP 입력
- [x] Day10 자동 Scene 구성

---

## 다음 개발 방향

다음 Day11에서는 Day10의 LevelUpPanel을 실제 무기 선택 시스템으로 확장한다.

주요 목표:

- 무기 후보 Pool
- 해금 여부 필터
- Level Up 시 후보 3개 생성
- 같은 후보 내 동일 무기 중복 방지
- ★1 기본 등장
- Lv.10 이후 ★2 자연 등장 확률
- Lv.10~14 : ★2 10%
- Lv.15~19 : ★2 15%
- Lv.20+ : ★2 20%
- 완성된 ★5 무기 일반 후보에서 제외
- 후보 선택 후 Body 무기 슬롯에 장착
- Empty Body가 없는 경우 기존 무기 교체 흐름 준비
- Continue Button을 후보 선택 방식으로 전환

Day10에서 구축한
XP → Level Up → Pause → LevelUpPanel 흐름은 그대로 유지하고,
Day11부터 레벨업 보상이 실제 무기 선택으로 이어지게 만든다.
