# 프로젝트 ε 개발 일지 — Day12

## 개발 목표

Day11에서 구현한 레벨업 무기 후보 선택과 Weapon Grade 데이터를 기반으로,
동일 무기·동일 등급의 2→1 합성 시스템과 ★등급 피해 배율을 실제 전투에 연결한다.

이번 일차의 핵심 목표:

- 동일 WeaponData + 동일 Grade 합성 판정
- ★1~★4의 2→1 상위 등급 합성
- ★5 합성 방지
- Body 길이를 유지한 채 Weapon 수만 감소
- 합성 결과를 Head 뒤 첫 Body에 재배치
- 합성 후 Empty Weapon Slot 생성
- 실시간 Merge UI
- 합성 중 자동 직진
- 합성 중 이동 속도 70%
- 합성 중 회전 입력 차단
- 합성 중 Boost 차단
- ★1~★5 피해 배율 적용
- Melee / StraightProjectile / Area에 동일한 Grade 피해 계산 적용
- Debug 합성 테스트 입력
- Day12 자동 Scene 구성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `27d5692156a38de5bf9a6025c2ee9340cc0b8756` |
| 기준 커밋 제목 | `12` |
| 이전 커밋 | `83a9ab9fd3c52f70d73606856e3f5af958f18b1a` |

---

## 진행 내용

### 1. WeaponGradeRules 추가

Weapon Grade가 실제 피해량에 반영되도록
공통 Grade 피해 규칙을 추가했다.

적용 배율:

```text
★1 = 100%
★2 = 125%
★3 = 150%
★4 = 180%
★5 = 220%
```

공통 계산식:

```text
Final Damage
=
Base Damage
×
Grade Multiplier
```

예:

```text
Base Damage = 10

★1 → 10
★2 → 12.5
★3 → 15
★4 → 18
★5 → 22
```

### 2. 모든 기본 공격 타입에 Grade 피해 적용

기존 Day11까지 Weapon Slot에는 Grade가 저장되었지만,
실제 공격 피해는 `WeaponData.BaseDamage`만 사용했다.

Day12에서는 `SnakeWeaponManager`가
Slot의 Grade를 기준으로 최종 피해량을 계산하도록 변경했다.

적용 대상:

```text
Melee
StraightProjectile
Area
```

따라서 세 공격 방식 모두 동일한 ★피해 배율을 사용한다.

### 3. WeaponMergeCandidate 추가

합성 가능한 무기 쌍을 표현하기 위해
`WeaponMergeCandidate` 구조를 추가했다.

후보가 보관하는 정보:

```text
WeaponData
CurrentGrade
ResultGrade
FirstSlotIndex
SecondSlotIndex
```

합성 결과 Grade는:

```text
ResultGrade
=
CurrentGrade + 1
```

로 계산한다.

### 4. 합성 가능 조건

합성은 다음 조건을 모두 만족해야 한다.

```text
WeaponData 동일
+
Grade 동일
+
Grade < 5
+
Grade < WeaponData.MaxGrade
+
서로 다른 두 Slot
```

예:

```text
Debug Blade ★1
+
Debug Blade ★1

→ 합성 가능
```

반면:

```text
Debug Blade ★1
+
Debug Blade ★2

→ 합성 불가
```

```text
Debug Blade ★1
+
Debug Cutter ★1

→ 합성 불가
```

### 5. 2→1 Grade 합성

현재 Grade 진행:

```text
★1 + ★1 → ★2
★2 + ★2 → ★3
★3 + ★3 → ★4
★4 + ★4 → ★5
```

★5는 최종 Grade이므로 추가 합성을 허용하지 않는다.

### 6. SnakeWeaponManager.TryMergeSlots 추가

실제 두 Weapon Slot의 합성을 수행하는
`TryMergeSlots()`를 추가했다.

처리 흐름:

```text
두 Slot 검사
↓
같은 무기 / 같은 Grade 확인
↓
두 Weapon 제거
↓
Result Grade +1 생성
↓
합성 결과 Head 뒤 첫 Slot 배치
↓
기존 나머지 Weapon을 앞쪽부터 재정렬
↓
뒤쪽에 Empty Slot 생성
```

### 7. Body 길이는 유지

Weapon Merge는 Body 자체를 제거하지 않는다.

예:

```text
합성 전

[Blade★1]
[Blaster★1]
[Blade★1]
[Pulse★1]
```

Body = 4
Weapon = 4

합성 후:

```text
[Blade★2]
[Blaster★1]
[Pulse★1]
[Empty]
```

Body = 4
Weapon = 3

즉:

```text
Body Count
→ 변화 없음

Weapon Count
→ -1

Empty Slot
→ +1
```

이다.

### 8. 합성 결과 Head 뒤 재배치

합성 결과는 항상 첫 번째 Body Weapon Slot으로 이동한다.

```text
Head
↓
Merged Weapon
↓
기존 Weapon
↓
기존 Weapon
↓
Empty
↓
Tail
```

이는 Day11에서 새 무기를 Head 뒤로 넣던 배치 원칙과 동일한 방향이다.

### 9. SnakeWeaponMergeController 추가

실시간 합성 상태를 관리하기 위해
`SnakeWeaponMergeController`를 추가했다.

주요 역할:

- Merge 가능한 Weapon Pair 검색
- MergePanel 열기 / 닫기
- 후보 선택 처리
- SnakeWeaponManager 합성 호출
- Merge 중 이동 상태 전환
- Merge 중 Boost 차단
- M 키 입력 처리

### 10. Merge 후보 검색

현재 Body Weapon Slot을 검사하여
동일 WeaponData + 동일 Grade인 쌍을 찾는다.

동일한 WeaponData + Grade 조합은
MergePanel에 한 번만 표시한다.

MergePanel은 현재 최대 3개의
**서로 다른 합성 가능한 무기/등급 쌍**을 표시한다.

예:

```text
Debug Blade ★1 + ★1
Debug Needle ★2 + ★2
Debug Pulse ★1 + ★1
```

현재 구현은 합성 가능한 쌍을 고르는 Prototype UI이며,
하나의 합성에서 서로 다른 3개의 진화 결과를 선택하는 분기 구조는
아직 구현하지 않았다.

### 11. 실시간 Merge UI

레벨업 UI와 달리 Merge UI는 게임을 일시정지하지 않는다.

```text
M
↓
MergePanel Open
↓
게임 계속 진행
```

MergePanel 구조:

```text
MergePanel
├─ Title
├─ StateText
├─ MergeCandidate_01
├─ MergeCandidate_02
├─ MergeCandidate_03
└─ CloseButton
```

### 12. M 키 Merge UI

Play 중:

```text
M
```

키로 MergePanel을 연다.

다시 M을 누르거나 Close Button을 선택하면
MergePanel을 닫는다.

Merge 가능한 Weapon Pair가 없으면
Panel은 열리지 않는다.

### 13. 합성 중 자동 직진

기존 SnakeMovement는:

```text
Rotate
+
MoveForward
```

를 항상 수행했다.

Day12에서는 `mergeMovementMode`를 추가했다.

Merge 중:

```text
Rotate
→ 실행하지 않음

MoveForward
→ 계속 실행
```

따라서 MergePanel을 보고 있는 동안
Snake는 Merge 시작 시점의 현재 방향으로 계속 직진한다.

### 14. 합성 중 이동속도 70%

Merge 상태의 이동 속도 배율:

```text
0.7
```

현재 이동 계산은:

```text
Base Move Speed
×
Stamina Speed Multiplier
×
Merge Speed Multiplier
```

형태로 확장되었다.

Merge가 끝나면 배율은 다시 정상 상태로 복원된다.

### 15. 합성 중 회전 입력 차단

MergePanel이 열린 동안
SnakeMovement는 `Rotate()`를 호출하지 않는다.

따라서:

```text
A / D
Left / Right
```

회전 입력을 해도 방향이 변경되지 않는다.

Merge 종료 후 정상 회전 입력이 다시 활성화된다.

### 16. 합성 중 Boost 차단

`SnakeStamina`에 외부 Boost 차단 상태를 추가했다.

Merge 중:

```text
Boost
→ 차단

Current Boost
→ 즉시 종료
```

Merge UI를 닫으면 Boost 사용이 다시 가능해진다.

Merge 중에는 Stamina가 불필요하게 소모되지 않고
일반 회복 로직은 유지된다.

### 17. WeaponMergePanelController 추가

Merge UI 전용 Controller를 추가했다.

주요 기능:

- 합성 후보 최대 3개 표시
- 현재 Grade 표시
- 결과 Grade 표시
- Candidate Button 이벤트
- Close Button 이벤트
- Merge 상태 안내

후보 예:

```text
Debug Blade
★ + ★
→ ★★
2 → 1
```

### 18. Debug Merge 테스트 기능

실제 게임에서 같은 무기를 두 개 얻을 때까지 기다리지 않고
합성 흐름을 빠르게 확인할 수 있도록
`SnakeWeaponMergeDebugControls`를 추가했다.

Debug 입력:

```text
N
```

실행 시:

```text
Slot 0
→ Debug Blade ★1

Slot 1
→ Debug Blade ★1
```

을 강제로 준비한다.

따라서 테스트 흐름은:

```text
N
↓
M
↓
Debug Blade 합성 후보
↓
후보 선택
↓
Debug Blade ★2
```

이다.

### 19. Merge Hint UI

HUD에 테스트 입력 안내를 추가했다.

```text
N: Debug Merge Pair
M: Merge
```

Prototype 단계에서 Merge 기능을 빠르게 확인하기 위한 Debug UI다.

### 20. Day12 자동 Scene 구성

`ProjectEpsilonDay12Setup`을 추가했다.

자동 처리 항목:

- Game Scene 열기
- Player / SnakeBody / HUDCanvas 검색
- SnakeWeaponManager 연결
- SnakeMovement 연결
- SnakeStamina 연결
- MergePanel 생성
- WeaponMergePanelController 연결
- SnakeWeaponMergeController 추가
- SnakeWeaponMergeDebugControls 추가
- Debug Blade 연결
- Merge Hint 생성
- Scene 저장
- 이전 Day11 Setup 제거

### 21. Day11 Setup 제거

Day12 Setup과 이전 자동 Setup이 충돌하지 않도록:

```text
ProjectEpsilonDay11Setup.cs
ProjectEpsilonDay11Setup.cs.meta
```

를 제거했다.

Day11에서 만든:

```text
WeaponRewardPool
LevelUp Weapon Candidate
★1 / ★2 Reward
AcquireWeapon
```

기능 자체는 그대로 유지한다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `WeaponGradeRules` 추가
- ★1 100%
- ★2 125%
- ★3 150%
- ★4 180%
- ★5 220%
- Melee Grade Damage 적용
- StraightProjectile Grade Damage 적용
- Area Grade Damage 적용
- `WeaponMergeCandidate` 추가
- 동일 WeaponData 합성 판정
- 동일 Grade 합성 판정
- ★5 합성 방지
- Weapon MaxGrade 제한 반영
- `TryMergeSlots()` 추가
- 2 Weapon → 1 Weapon
- Body Count 유지
- Empty Slot +1
- 합성 결과 Head 뒤 배치
- 기존 Weapon 순서 재정렬
- `SnakeWeaponMergeController` 추가
- 최대 3개 Merge Pair 표시
- 실시간 Merge UI
- M 키 Open / Close
- 합성 중 자동 직진
- 합성 중 회전 차단
- 합성 중 이동 속도 70%
- 합성 중 Boost 차단
- Merge 종료 시 정상 이동 복원
- `WeaponMergePanelController` 추가
- `SnakeWeaponMergeDebugControls` 추가
- N 키 Debug Blade ★1 ×2 준비
- Merge Hint UI
- Day12 자동 Scene Setup
- Day11 Setup 제거

현재 GitHub 커밋에는 CI / Actions 상태 체크가 등록되어 있지 않다.
따라서 Unity Editor 실제 컴파일 및 Play Mode 동작은
로컬 Unity 환경에서 최종 확인한다.

---

## 12일차 완료 기준

- [x] 같은 Weapon + 같은 Grade 합성 판정
- [x] ★1 + ★1 → ★2
- [x] ★2 + ★2 → ★3
- [x] ★3 + ★3 → ★4
- [x] ★4 + ★4 → ★5
- [x] ★5 합성 방지
- [x] 2→1 Weapon Merge
- [x] Body 길이 유지
- [x] Empty Slot +1
- [x] 합성 결과 Head 뒤 배치
- [x] 실시간 Merge UI
- [x] Merge 후보 최대 3개 표시
- [x] Merge 중 자동 직진
- [x] Merge 중 이동 속도 70%
- [x] Merge 중 회전 입력 차단
- [x] Merge 중 Boost 차단
- [x] ★1~★5 Damage 배율
- [x] 3종 공격 방식 Grade Damage 적용
- [x] N / M Debug 테스트 흐름
- [x] Day12 자동 Scene 구성

---

## 다음 개발 방향

다음 Day13에서는 Prototype 단계의 마지막 확장과 통합 작업을 진행한다.

주요 목표:

- ★3 고유 효과 Hook
- ★5 최종 효과 Hook
- Weapon Grade별 고유 효과 확장 인터페이스
- 회복 아이템
- 공유 HP 회복
- Body 복구 아이템
- 손실된 Empty Body 복구
- 기본 Item Drop 구조
- Enemy Drop 공통 처리
- 이동 → 공격 → 적 처치 → XP → 레벨업 → Body 성장 → 무기 선택 → 합성의 전체 루프 통합 테스트
- Prototype 1차 밸런스 조정
- 핵심 루프에서 발생하는 예외와 상태 충돌 점검

Day13이 완료되면:

```text
Movement
→ Combat
→ Enemy Death
→ XP
→ Level Up
→ Body Growth
→ Weapon Reward
→ Weapon Merge
→ Item Recovery
→ Combat
```

의 반복 가능한 핵심 Prototype 루프가 완성된다.
