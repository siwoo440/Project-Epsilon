# 프로젝트 ε 개발 일지 — Day13

## 개발 목표

Day12까지 구현한 이동, 전투, XP, 레벨업, Body 성장, 무기 선택, 무기 합성을
회복 및 Body 복구 시스템과 연결하여 첫 번째 Prototype 핵심 루프를 완성한다.

이번 일차의 핵심 목표:

- 획득한 Body 수와 현재 Body 수 분리
- Body Repair가 성장 수단으로 동작하지 않도록 제한
- HP 부분 회복 기능
- Heal Pickup
- Body Repair Pickup
- XP / Heal / Body Repair 공통 Enemy Drop
- ★3 고유 효과 Hook
- ★5 최종 효과 Hook
- 무기 획득 / 합성 / 공격 시 Grade Effect Hook 호출
- Level Up과 Merge UI 상태 충돌 방지
- Prototype 통합 테스트용 Debug 기능
- Day13 자동 Scene 구성
- Prototype 마일스톤 정리

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `a0438f7c4b3771d8e32c6f3105a9be70deeb4d08` |
| 기준 커밋 제목 | `13` |
| 이전 커밋 | `732d6b9da6fc0c80bf9bab30397977ed39e3e807` |

---

## 진행 내용

### 1. Body 성장과 복구 수치 분리

기존에는 현재 존재하는 Body 수만 관리했기 때문에
Body Repair 아이템이 단순 Body 추가 기능을 사용할 경우
레벨업 없이도 최대 길이가 증가할 수 있었다.

Day13에서는 다음 두 개념을 분리했다.

```text
EarnedBodyCount
→ 성장으로 확보한 Body 수

CurrentBodyCount
→ 현재 살아 있는 Body 수
```

예:

```text
Earned 5
Current 5

↓ Body 2개 손실

Earned 5
Current 3
```

Body를 잃어도 `EarnedBodyCount`는 유지된다.

### 2. TryGainBodyFromLevelUp 추가

레벨업을 통한 Body 성장은:

```text
TryGainBodyFromLevelUp()
```

으로 분리했다.

레벨업 시:

```text
EarnedBodyCount +1
CurrentBodyCount +1
```

을 동시에 적용한다.

기존 `TryAddBody()`는 이전 코드와의 호환을 위해
내부적으로 `TryGainBodyFromLevelUp()`을 호출하도록 유지했다.

### 3. TryRepairBody 추가

Body Repair는:

```text
TryRepairBody()
```

를 사용한다.

복구 조건:

```text
CurrentBodyCount < EarnedBodyCount
```

일 때만 Body를 한 칸 복구한다.

예:

```text
Earned 5
Current 3

↓ Repair

Earned 5
Current 4
```

현재 Body가 이미 Earned Body와 같으면
추가 성장을 허용하지 않는다.

```text
Earned 5
Current 5

↓ Repair

변화 없음
```

### 4. MissingRepairableBodyCount 추가

현재 복구할 수 있는 Body 개수를:

```text
MissingRepairableBodyCount
```

로 계산한다.

계산:

```text
EarnedBodyCount
-
CurrentBodyCount
```

따라서 Recovery Pickup이 실제로 필요한 상태인지
간단하게 판단할 수 있다.

### 5. Body Repair 시 Weapon 복구 없음

Body Repair는 Body Segment만 생성한다.

WeaponManager는 Body Count 변경을 감지하여
새 Weapon Slot을 생성하지만 해당 Slot은 Empty 상태다.

예:

```text
손실 전
[Blade]
[Pulse]
[Blaster]
[Needle]

↓ Body 손실

[Blade]
[Pulse]
[Blaster]

↓ Body Repair

[Blade]
[Pulse]
[Blaster]
[Empty]
```

즉 Body는 복구되지만 잃어버린 Weapon은 복원되지 않는다.

### 6. SnakeHealth.Heal 추가

기존 완전 회복 `ResetHealth()` 외에
일부 HP를 회복할 수 있는:

```text
Heal(int amount)
```

을 추가했다.

기본 Heal Pickup 값:

```text
+15 HP
```

예:

```text
HP 40 / 100
↓
Heal +15
↓
HP 55 / 100
```

최대 HP 이상으로는 증가하지 않는다.

```text
HP 94 / 100
↓
Heal +15
↓
HP 100 / 100
```

### 7. RecoveryPickup 추가

회복용 월드 Pickup을 공통 구조로 추가했다.

현재 타입:

```text
Heal
BodyRepair
```

두 가지다.

### 8. Heal Pickup

Heal Pickup은 플레이어가 회복 가능한 상태일 때만
Head 방향으로 흡수된다.

획득 효과:

```text
HP +15
```

현재 HP가 이미 최대라면 Pickup이 소모되지 않는다.

### 9. Body Repair Pickup

Body Repair Pickup은:

```text
MissingRepairableBodyCount > 0
```

인 경우에만 흡수된다.

획득 효과:

```text
CurrentBodyCount +1
```

단:

```text
CurrentBodyCount <= EarnedBodyCount
```

규칙을 넘지 않는다.

### 10. Recovery Pickup 시각 구분

Prototype 단계에서는 동일한 Debug Sprite를 사용하되
색상과 크기를 다르게 설정한다.

```text
Heal
→ 녹색 계열

Body Repair
→ 청색 계열
```

이를 통해 테스트 중 두 Pickup을 구분할 수 있다.

### 11. EnemyDropController 추가

기존 XP 전용 `ExperienceDropper` 구조를 확장하여
적 사망 보상을 한 곳에서 처리하는:

```text
EnemyDropController
```

를 추가했다.

처리 항목:

```text
Enemy Death
├─ XP
├─ Heal
└─ Body Repair
```

### 12. XP Drop 유지

적 사망 시 XP Pickup은 항상 생성된다.

기존 Day09 테스트 적의:

```text
XP 1
XP 5
XP 20
```

값을 Day13 Setup에서 읽어와
`EnemyDropController`로 이전한다.

### 13. Heal Drop 확률

Prototype 테스트용으로 적 XP 등급에 따라
Heal Drop 확률을 다르게 구성한다.

```text
XP 1 적
→ Heal 약 20%

XP 5 적
→ Heal 약 30%

XP 20 적
→ Heal 약 45%
```

회복량은 모두:

```text
15 HP
```

이다.

### 14. Body Repair Drop 확률

Body Repair는 Heal보다 낮은 확률로 구성한다.

```text
XP 1 적
→ 약 6%

XP 5 적
→ 약 12%

XP 20 적
→ 약 24%
```

이는 Prototype 검증용 수치이며
후반 밸런스 단계에서 조정한다.

### 15. 기존 ExperienceDropper Scene 구성 이전

Day13 Setup은 기존 테스트 적에 붙어 있던
`ExperienceDropper`의 XP 값과 Sprite를 읽는다.

그 후 기존 Component를 제거하고:

```text
EnemyDropController
```

로 교체한다.

따라서 기존 XP 보상 데이터를 유지하면서
Heal / Body Repair Drop을 추가한다.

### 16. WeaponGradeEffectHooks 추가

★3 / ★5 무기의 고유 효과를
후속 콘텐츠 단계에서 연결하기 위한 Hook을 추가했다.

제공 이벤트:

```text
GradeThreeTriggered
GradeFiveTriggered
```

### 17. Grade Effect Context

Hook 호출 시 다음 정보를 전달한다.

```text
WeaponData
Grade
Origin
Damage
Trigger
```

Trigger 종류:

```text
Acquired
Merged
Attack
```

따라서 이후 무기별 고유 효과가
획득 / 합성 / 공격 시점을 구분해서 처리할 수 있다.

### 18. ★3 Hook

Grade가 3 이상이면:

```text
GradeThreeTriggered
```

이 호출된다.

추후 예:

```text
Blade ★3
→ 범위 증가

Blaster ★3
→ Projectile +1
```

같은 효과를 연결할 수 있다.

### 19. ★5 Hook

Grade가 5 이상이면:

```text
GradeFiveTriggered
```

도 호출된다.

★5 상태에서는 ★3 Hook 역시 유지되므로:

```text
★5
=
★3 강화 효과
+
★5 최종 효과
```

형태로 설계할 수 있다.

### 20. WeaponManager Grade Hook 연동

`SnakeWeaponManager`가 다음 시점에
Grade Effect Hook을 호출하도록 연결했다.

```text
Weapon Acquire
Weapon Merge
Weapon Attack
```

실제 48종 무기의 고유 효과는 아직 만들지 않고
확장 인터페이스만 완성했다.

### 21. Level Up > Merge UI 우선순위

Day12 Merge UI는 실시간으로 진행되므로
Level Up과 동시에 발생할 수 있었다.

Day13에서는 우선순위를:

```text
Level Up
>
Merge
>
Normal Gameplay
```

으로 정리했다.

### 22. Merge 중 Level Up 발생

MergePanel이 열린 상태에서 XP 조건을 충족하면:

```text
LevelUpRequested
↓
MergePanel 자동 Close
↓
Merge 이동 상태 해제
↓
LevelUp 처리
```

순서로 진행한다.

### 23. Level Up 중 Merge 차단

다음 상태에서는 MergePanel을 열 수 없다.

```text
Experience.IsLevelUpPending
SnakeLevelUpController.IsPresentingLevelUp
```

따라서 Level Up UI가 진행 중일 때 M 키를 눌러도
Merge가 시작되지 않는다.

### 24. Prototype Debug Controls

Prototype 전체 기능을 빠르게 테스트하기 위한
`SnakePrototypeDebugControls`를 추가했다.

Debug 입력:

```text
H
→ Heal Pickup 생성

J
→ Body Repair Pickup 생성

3
→ 첫 Weapon을 Debug Blade ★3으로 변경

5
→ 첫 Weapon을 Debug Blade ★5으로 변경
```

기존 Debug 입력과 함께 사용할 수 있다.

### 25. 기존 Debug 입력과 통합 테스트

기존 기능:

```text
P
→ HP 피해

[
→ Body 제거

L
→ XP 추가

N
→ Merge용 동일 Weapon 2개 준비

M
→ Merge UI
```

와 Day13 입력을 조합해
핵심 Prototype 루프를 빠르게 확인할 수 있다.

### 26. Day13 자동 Scene 구성

`ProjectEpsilonDay13Setup`을 추가했다.

자동 처리 항목:

- Game Scene 열기
- Day12 핵심 Component 검사
- Earned Body 초기화
- WeaponGradeEffectHooks 추가
- SnakeWeaponManager와 Grade Hook 연결
- Merge Controller에 Level Up 상태 연결
- 기존 ExperienceDropper 데이터 읽기
- ExperienceDropper 제거
- EnemyDropController 추가
- XP / Heal / Body Repair Drop 설정
- SnakePrototypeDebugControls 추가
- Debug Blade 연결
- Prototype Hint UI 추가
- Scene 저장
- 이전 Day12 Setup 제거

### 27. Day12 Setup 제거

Day13 자동 Setup 실행 후:

```text
ProjectEpsilonDay12Setup.cs
```

는 제거된다.

Day12에서 구현한:

```text
Weapon Merge
Grade Damage
MergePanel
실시간 Merge Movement
```

기능 자체는 유지한다.

---

## Prototype 통합 흐름

Day13 완료 후 핵심 게임 루프는 다음과 같다.

```text
Snake Movement
↓
Body Weapon Auto Attack
↓
Enemy Damage
↓
Enemy Death
↓
XP / Recovery Drop
↓
XP Pickup
↓
Level Up
↓
Body Growth
↓
Weapon Candidate 3
↓
Weapon Acquire
↓
동일 Weapon 확보
↓
Weapon Merge
↓
★ Grade 상승
↓
Grade Damage 증가
↓
Heal / Body Repair
↓
다시 Combat
```

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- EarnedBodyCount 추가
- Current / Earned Body 분리
- MissingRepairableBodyCount 추가
- TryGainBodyFromLevelUp 추가
- TryRepairBody 추가
- Body Repair 성장 악용 방지
- Body Repair 시 Empty Weapon Slot 생성 구조 유지
- SnakeHealth.Heal 추가
- Heal +15
- RecoveryPickup 추가
- Heal Pickup
- Body Repair Pickup
- EnemyDropController 추가
- XP Drop 통합
- Heal Drop
- Body Repair Drop
- 기존 ExperienceDropper Scene 구성 이전
- WeaponGradeEffectHooks 추가
- GradeThreeTriggered
- GradeFiveTriggered
- Acquired / Merged / Attack Trigger
- WeaponManager Hook 연결
- Level Up > Merge 우선순위
- Level Up 시 Merge 자동 종료
- Level Up 중 Merge 시작 차단
- Prototype Debug Controls
- H / J / 3 / 5 Debug 입력
- Day13 자동 Scene Setup
- Day12 Setup 제거

현재 GitHub 커밋에는 CI / Actions 상태 체크가 등록되어 있지 않다.
따라서 Unity Editor 실제 컴파일 및 Play Mode 동작은
로컬 Unity 환경에서 최종 확인한다.

---

## 13일차 완료 기준

- [x] Earned Body와 Current Body 분리
- [x] Level Up Body 성장 분리
- [x] Body Repair
- [x] Body Repair 성장 제한
- [x] Repair된 Weapon Slot Empty 유지
- [x] HP +15 Heal
- [x] Heal Pickup
- [x] Body Repair Pickup
- [x] XP / Heal / Repair 공통 Drop
- [x] ★3 Effect Hook
- [x] ★5 Effect Hook
- [x] Acquire / Merge / Attack Hook
- [x] LevelUp / Merge 상태 우선순위
- [x] Prototype Debug 입력
- [x] Day13 자동 Scene 구성
- [x] Prototype 핵심 루프 연결

---

## 다음 개발 방향

Day13으로 첫 번째 Prototype 마일스톤을 마무리한다.

다음 Day14부터는 속성 시스템 단계로 진행한다.

초기 8속성:

```text
Physical
Fire
Cold
Electric
Poison
Explosion
Holy
Dark
```

주요 목표:

- 8속성 공통 데이터 구조
- Weapon Attribute 실제 전투 데이터 연결
- 속성별 상태이상 기반
- 속성 보유 개수 계산
- ×2 / ×4 / ×6 / ×8 시너지 기반
- 공격 태그와 속성 데이터 분리
- 이후 복합 속성 시너지 확장을 위한 공통 구조

Day14부터는 현재 완성된 Prototype 전투 루프 위에
빌드 다양성과 속성 정체성을 추가한다.
