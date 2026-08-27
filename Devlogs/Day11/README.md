# 프로젝트 ε 개발 일지 — Day11

## 개발 목표

Day10에서 완성한 XP → Level Up → Body 성장 → Pause 흐름에
실제 무기 보상 선택을 연결한다.

이번 일차의 핵심 목표:

- 레벨업 무기 후보 Pool
- 해금 여부 필터 구조
- 중복 없는 무기 후보 3개 생성
- ★1 기본 등장
- Lv.10 이후 ★2 자연 등장
- 보유 ★5 무기 일반 후보 제외
- LevelUpPanel의 Continue 방식 제거
- 무기 후보 3개 선택 UI
- 선택한 무기의 실제 Body 슬롯 장착
- 새 무기 Head 뒤 우선 배치
- 빈 슬롯이 없을 때 꼬리 쪽 무기 밀어내기
- 테스트용 무기 Pool 확장
- Day12 합성 시스템을 위한 등급 기반 준비

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `e8abf0b7e606b6b09a5e72ddc6e90a39ba2d6966` |
| 기준 커밋 제목 | `11` |
| 이전 커밋 | `9e3bc42266f44cc16a48115d4d3f54b5aef512b6` |

---

## 진행 내용

### 1. WeaponRewardCandidate 추가

레벨업 화면에 표시되는 무기 후보 한 개를 표현하기 위해
`WeaponRewardCandidate` 구조를 추가했다.

후보는 다음 두 값을 가진다.

```text
WeaponData
Grade
```

따라서 같은 WeaponData라도
후보가 ★1인지 ★2인지 별도로 표현할 수 있다.

### 2. WeaponRewardRules 추가

무기 후보 등급 결정 규칙을
`WeaponRewardRules`로 분리했다.

★2 자연 등장 확률:

```text
Lv.1~9
→ 0%

Lv.10~14
→ 10%

Lv.15~19
→ 15%

Lv.20+
→ 20%
```

WeaponData의 MaxGrade가 1이면
항상 ★1 후보만 생성된다.

Day11에서는 후보 등급 결정까지만 담당하며
실제 ★등급 피해 배율은 Day12에서 연결한다.

### 3. WeaponRewardPool 추가

레벨업 무기 후보의 원본 목록을 관리하기 위해
`WeaponRewardPool` ScriptableObject를 추가했다.

구조:

```text
WeaponRewardPool
└─ WeaponRewardEntry
   ├─ WeaponData
   └─ Unlocked
```

현재 Debug Pool의 무기는 모두 해금 상태로 구성한다.

향후 메타 해금 시스템이 구현되면
`Unlocked` 판단을 실제 진행도 데이터와 연결할 수 있다.

### 4. 후보 3개 생성

레벨업마다 Pool에서 최대 3개의 후보를 생성한다.

```text
전체 사용 가능 Pool
↓
무기 1개 Random 선택
↓
선택 항목 임시 제거
↓
두 번째 선택
↓
임시 제거
↓
세 번째 선택
```

따라서 같은 레벨업 화면에서는
동일 WeaponData가 중복해서 등장하지 않는다.

예:

```text
Debug Blade ★1
Debug Needle ★1
Debug Nova ★1
```

정상

```text
Debug Blade ★1
Debug Blade ★1
Debug Nova ★1
```

불가

### 5. 해금 여부 필터

후보 Pool은 `WeaponRewardEntry.Unlocked`가
true인 무기만 사용한다.

```text
Unlocked = true
→ 후보 가능

Unlocked = false
→ 후보 제외
```

현재는 Debug Weapon 6종을 모두 true로 구성한다.

### 6. ★5 완성 무기 후보 제외

`SnakeWeaponManager.HasCompletedGradeFive()`를 추가했다.

현재 Body Weapon Slot을 검사하여:

```text
동일 WeaponData
+
Grade >= 5
```

인 슬롯이 존재하면
해당 WeaponData는 일반 레벨업 후보 Pool에서 제외된다.

이 기능은 Day12 합성 시스템에서 ★5 무기가 실제로 만들어지면
즉시 활용할 수 있다.

### 7. 테스트 무기 Pool 6종 구성

후보 3개를 안정적으로 테스트할 수 있도록
기존 테스트 무기 3종에 3종을 추가했다.

기존:

```text
Debug Blade
Debug Blaster
Debug Pulse
```

추가:

```text
Debug Cutter
Debug Needle
Debug Nova
```

총:

```text
6종
```

으로 Debug Weapon Reward Pool을 구성했다.

### 8. Debug Cutter

공격 타입:

```text
Melee
```

기본 수치:

```text
Damage 18
Attack Interval 1.05
Range 1.1
```

기존 Debug Blade와 다른 근접 공격 성향을 테스트한다.

### 9. Debug Needle

공격 타입:

```text
StraightProjectile
```

기본 수치:

```text
Damage 7
Attack Interval 0.48
Range 7
Projectile Speed 11
Projectile Lifetime 2.5
```

빠른 연사형 투사체 테스트 무기다.

### 10. Debug Nova

공격 타입:

```text
Area
```

기본 수치:

```text
Damage 12
Attack Interval 1.9
Range 2.25
```

기존 Debug Pulse보다 넓고 강한 범위형 테스트 무기다.

### 11. DebugWeaponRewardPool.asset 생성

Day11 Setup은 다음 Asset을 자동 생성한다.

```text
Assets/Data/Progression/
└─ DebugWeaponRewardPool.asset
```

등록 무기:

```text
Debug Blade
Debug Blaster
Debug Pulse
Debug Cutter
Debug Needle
Debug Nova
```

현재 모두 Unlocked 상태다.

### 12. LevelUpPanel 후보 선택 UI 전환

Day10의 LevelUpPanel은:

```text
LevelUpPanel
├─ Title
├─ LevelText
├─ GrowthText
└─ ContinueButton
```

구조였다.

Day11에서는 ContinueButton을 제거하고:

```text
LevelUpPanel
├─ Title
├─ LevelText
├─ GrowthText
├─ CandidateButton_01
├─ CandidateButton_02
└─ CandidateButton_03
```

구조로 변경했다.

### 13. 후보 표시 정보

각 후보 버튼은 최소한 다음 정보를 표시한다.

```text
Weapon Name
★ Grade
Attack Type
Damage
```

예:

```text
Debug Needle ★
StraightProjectile
DMG 7
```

현재는 Prototype용 텍스트 UI이며
최종 아이콘과 아트는 후반 UI 단계에서 적용한다.

### 14. 후보 선택 이벤트

`LevelUpPanelController`는
기존 `ContinueRequested` 대신:

```text
CandidateSelected(index)
```

이벤트를 제공한다.

선택 가능한 인덱스:

```text
0
1
2
```

따라서 레벨업 로직은 UI Button 자체를 직접 알 필요 없이
선택된 후보 번호만 전달받는다.

### 15. SnakeLevelUpController 무기 보상 연결

Day10의 `SnakeLevelUpController`에 다음 참조를 추가했다.

```text
SnakeWeaponManager
WeaponRewardPool
```

Level Up 흐름:

```text
LevelUpRequested
↓
Body 성장
↓
후보 3개 생성
↓
게임 Pause
↓
LevelUpPanel 표시
↓
후보 선택
↓
AcquireWeapon()
↓
현재 레벨업 완료
↓
게임 Resume
```

초과 XP로 다음 레벨업이 필요한 경우
기존 Day10 순차 레벨업 구조를 그대로 유지한다.

### 16. AcquireWeapon 추가

기존 `SnakeWeaponManager`에는
빈 Slot 장착과 특정 Slot 장착 기능이 있었다.

Day11에서는 실제 무기 획득 규칙을 담당하는:

```text
AcquireWeapon()
```

을 추가했다.

### 17. 새 무기 Head 뒤 우선 배치

새 무기를 획득하면
첫 번째 Body Weapon Slot에 배치한다.

예:

```text
기존
[A] [B] [C] [Empty]

새 무기 X 획득

↓

[X] [A] [B] [C]
```

즉 새 무기는 항상 Head에 가장 가까운 Body로 들어온다.

### 18. 빈 Slot이 없는 경우

모든 Body에 무기가 장착되어 있는 경우에도
동일한 Head 우선 배치 규칙을 사용한다.

```text
기존
[A] [B] [C] [D]

새 무기 X

↓

[X] [A] [B] [C]
```

가장 꼬리 쪽의 기존 D 무기는 제거된다.

이 구조는 이후 합성 결과를
Head 뒤에 재배치하는 Day12 규칙에도 재사용할 수 있다.

### 19. Body 성장과 무기 획득 연결

Day10에서 Level Up 시 Body가 먼저 +1 된다.

예:

```text
Body 3
↓
Level Up
↓
Body 4
```

새 Body가 생성되면
기존 Weapon Slot 동기화 시스템이 Empty Slot을 만든다.

그 후 Day11 무기 선택이 진행된다.

```text
Body +1
↓
Empty Weapon Slot +1
↓
후보 선택
↓
AcquireWeapon
↓
새 무기 Head 뒤 배치
```

### 20. 최대 Body 상태 유지

Body가 이미 20인 경우에는
Day10 규칙대로 공유 HP를 완전 회복한다.

그 후에도 무기 후보 선택은 정상적으로 진행된다.

```text
Body 20
↓
Level Up
↓
HP FULL RESTORE
↓
후보 3개
↓
새 무기 선택
↓
Head 뒤 삽입
↓
꼬리 무기 1개 제거
```

### 21. Day11 자동 Scene 구성

`ProjectEpsilonDay11Setup`은 다음 작업을 자동 처리한다.

- Game Scene 열기
- Day10 핵심 컴포넌트 검사
- 기존 테스트 무기 3종 갱신
- 신규 Debug Weapon 3종 생성
- DebugWeaponRewardPool 생성
- WeaponRewardPool에 6종 등록
- LevelUpPanel 크기 확장
- 기존 ContinueButton 제거
- 후보 버튼 3개 생성
- SnakeLevelUpController에 WeaponManager 연결
- SnakeLevelUpController에 RewardPool 연결
- Scene 저장
- 이전 Day10 Setup 제거

### 22. Day10 Setup 제거

Day11 자동 구성과 이전 Setup이 충돌하지 않도록:

```text
ProjectEpsilonDay10Setup.cs
ProjectEpsilonDay10Setup.cs.meta
```

를 Day11 적용 과정에서 제거했다.

Day10에서 만든 Level / XP / Body 성장 시스템 자체는 그대로 유지된다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `WeaponRewardCandidate` 추가
- `WeaponRewardRules` 추가
- `WeaponRewardPool` 추가
- WeaponRewardEntry 해금 상태 구조
- 후보 최대 3개 생성
- 동일 화면 후보 중복 제거
- Lv.1~9 ★2 확률 0%
- Lv.10~14 ★2 확률 10%
- Lv.15~19 ★2 확률 15%
- Lv.20+ ★2 확률 20%
- Weapon MaxGrade 제한 반영
- 보유 ★5 무기 일반 후보 제외
- Debug Weapon 6종 Pool
- Debug Cutter 추가
- Debug Needle 추가
- Debug Nova 추가
- DebugWeaponRewardPool.asset 생성
- ContinueButton 제거
- Candidate Button 3개 추가
- 후보 이름 / ★ / 공격 타입 / Damage 표시
- 후보 선택 이벤트
- `SnakeLevelUpController`와 Reward Pool 연결
- `SnakeWeaponManager.AcquireWeapon()` 추가
- 새 무기 Head 뒤 우선 배치
- 기존 무기 꼬리 방향 Shift
- Full Slot 시 가장 뒤 무기 제거
- Level Up Pause 유지
- 선택 후 Resume
- 초과 XP 연속 레벨업 구조 유지
- Day11 자동 Scene Setup
- Day10 Setup 제거

현재 GitHub 커밋에는 CI / Actions 상태 체크가 등록되어 있지 않다.
따라서 Unity Editor 실제 컴파일 및 Play Mode 동작은
로컬 Unity 환경에서 최종 확인한다.

---

## 11일차 완료 기준

- [x] Weapon Reward Pool
- [x] 해금 여부 필터 구조
- [x] 후보 3개 생성
- [x] 같은 화면 중복 후보 방지
- [x] ★1 기본 등장
- [x] Lv.10 이후 ★2 자연 등장
- [x] Lv.10~14 ★2 10%
- [x] Lv.15~19 ★2 15%
- [x] Lv.20+ ★2 20%
- [x] 보유 ★5 무기 후보 제외
- [x] 후보 선택 UI
- [x] Continue 방식 제거
- [x] 선택한 무기 실제 장착
- [x] Head 뒤 우선 배치
- [x] 기존 무기 꼬리 방향 Shift
- [x] Full Slot 교체 처리
- [x] Debug Weapon 6종
- [x] Day11 자동 Scene 구성

---

## 다음 개발 방향

다음 Day12에서는 현재 보유 무기를 대상으로
같은 무기 + 같은 ★등급의 2→1 합성 시스템을 구현한다.

주요 목표:

- 같은 WeaponData + 같은 Grade 탐색
- 동일 무기·등급 2개가 있을 때 합성 가능 판정
- ★1 + ★1 → ★2
- ★2 + ★2 → ★3
- ★3 + ★3 → ★4
- ★4 + ★4 → ★5
- 2개 Weapon Slot을 1개로 합성
- 합성 후 Empty Body 1칸 발생
- 합성 결과 무기를 Head 뒤로 재배치
- 실시간 Merge UI
- 합성 중 자동 직진
- 합성 중 이동 속도 70%
- ★1~★5 Damage 배율
- ★1 100%
- ★2 125%
- ★3 150%
- ★4 180%
- ★5 220%

Day11에서 완성한
Level Up → 후보 선택 → 새 무기 획득 구조와
현재 Slot의 WeaponData / Grade 정보가
Day12 합성 판정의 입력으로 사용된다.
