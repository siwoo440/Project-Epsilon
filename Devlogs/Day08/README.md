# 프로젝트 ε 개발 일지 — Day08

## 개발 목표

Day07까지 완성한 Snake 이동·생존 코어 위에 첫 전투 시스템을 연결한다.

이번 일차에서는 프로젝트 ε의 핵심 규칙인
`Body 1칸 = 무기 슬롯 1개` 구조를 만들고,
각 Body의 실제 월드 위치에서 가장 가까운 적을 자동 탐색하여
직선 투사체를 발사하는 공통 무기 기반을 구현한다.

주요 목표:

- Body 수와 Weapon Slot 수 1:1 동기화
- Body 추가 시 빈 무기 슬롯 자동 추가
- Body 제거 시 해당 슬롯 자동 제거
- 기존 Body의 장착 무기 보존
- `WeaponData` 기반 무기 데이터 사용
- 시작 무기 `Debug Blaster` 자동 장착
- 각 Body의 실제 위치를 공격 원점으로 사용
- 사거리 내 가장 가까운 적 자동 탐색
- StraightProjectile 자동 공격
- 테스트 타겟 HP / 사망 처리
- Day08 테스트 환경 자동 구성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `2364383a4d70f401faf12fef79ef6eb70a04c95b` |
| 기준 커밋 제목 | `8` |

---

## 진행 내용

### 1. Combat 구조 추가

무기 관련 코드를 Player 구조와 분리하기 위해
`Assets/Scripts/Combat` 영역을 추가했다.

주요 구성:

```text
Combat
├─ SnakeWeaponManager
├─ SnakeWeaponSlot
├─ WeaponTarget
└─ StraightProjectile
```

`SnakeBodyManager`는 계속 Body의 생성·삭제·순서를 담당하고,
`SnakeWeaponManager`는 Body와 무기 슬롯의 연결만 담당하도록 역할을 분리했다.

### 2. Body 1칸 = 무기 슬롯 1개

`SnakeWeaponManager`가 `SnakeBodyManager.BodySegments`를 기준으로
현재 Body 수와 동일한 수의 `SnakeWeaponSlot`을 유지하도록 구현했다.

기본 시작 구조:

```text
Body_01 → Weapon Slot 0
Body_02 → Weapon Slot 1
Body_03 → Weapon Slot 2
```

따라서 시작 Body 3칸이면 Weapon Slot도 3개다.

### 3. Body 추가 시 슬롯 자동 증가

기존 Day05 Debug 입력을 이용해 Body를 추가하면
`BodyCountChanged` 이벤트를 통해 Weapon Slot도 자동 동기화된다.

```text
Body 3
Weapon Slot 3

] 입력

↓

Body 4
Weapon Slot 4
```

새로 생성된 Body의 슬롯은 Empty 상태로 시작한다.

### 4. Body 제거 시 슬롯 자동 제거

Body가 꼬리 쪽에서 제거되면 동일 Body를 소유한 Weapon Slot도 제거된다.

```text
Body_01 [Debug Blaster]
Body_02 [Empty]
Body_03 [Empty]
Body_04 [Empty]

Body -1

↓

Body_01 [Debug Blaster]
Body_02 [Empty]
Body_03 [Empty]
```

기존 Body가 유지되는 경우 해당 Body가 가지고 있던 무기 데이터도 유지한다.

### 5. Weapon Slot 구조

각 `SnakeWeaponSlot`은 다음 정보를 보유한다.

```text
Owner
WeaponData
Grade
Next Attack Time
```

`Owner`는 실제 `SnakeSegment`를 가리키므로
무기의 공격 원점으로 해당 Body의 Transform을 직접 사용할 수 있다.

### 6. 시작 무기 Debug Blaster

Day08 Setup이 자동으로 다음 ScriptableObject를 생성한다.

```text
Assets/Data/Weapons/DebugBlaster.asset
```

설정:

```text
ID = debug_blaster
Name = Debug Blaster

Attribute = Physical
AttackType = StraightProjectile

Damage = 10
Attack Interval = 1초
Range = 6

Projectile Speed = 8
Projectile Lifetime = 3초
Max Grade = 5
```

게임 시작 시 첫 번째 Body 슬롯에 자동 장착한다.

```text
Body_01 → Debug Blaster
Body_02 → Empty
Body_03 → Empty
```

### 7. WeaponData Projectile 데이터 확장

기존 `WeaponData`에 직선 투사체 테스트에 필요한 값을 추가했다.

```text
Projectile Speed
Projectile Lifetime
```

따라서 이후 무기별 투사체 이동속도와 생존시간을
ScriptableObject 데이터에서 조절할 수 있다.

### 8. Body 실제 위치에서 공격 생성

공격 원점은 Head가 아니라 무기를 가진 Body의 현재 월드 위치다.

```text
SnakeWeaponSlot.Owner
↓
Body Transform
↓
slot.Origin.position
↓
Projectile 생성
```

따라서 Snake가 곡선으로 이동해 몸통 위치가 계속 변해도
총알은 실제 무기가 장착된 Body 위치에서 발사된다.

### 9. 가장 가까운 적 자동 탐색

`WeaponTarget`이 현재 활성화된 타겟 목록을 관리한다.

각 무기는 자신의 Body 위치를 기준으로:

```text
Weapon Range 확인
↓
사거리 내 활성 Target 탐색
↓
거리 제곱 비교
↓
가장 가까운 Target 선택
```

방식으로 자동 타겟을 선택한다.

사거리 밖 Target은 공격하지 않는다.

### 10. 자동 공격 Cooldown

각 Weapon Slot은 자신의 다음 공격 가능 시간을 관리한다.

`Debug Blaster` 기준:

```text
Attack Interval = 1초
```

따라서 타겟이 사거리 안에 존재하면 약 1초 간격으로 자동 공격한다.

타겟이 없을 경우 Cooldown을 소비하지 않고
다음 Update에서 다시 타겟을 탐색한다.

### 11. StraightProjectile 구현

Day08에서는 `WeaponAttackType` 중
`StraightProjectile`만 실제 공격 동작으로 구현했다.

흐름:

```text
Body 위치
↓
가장 가까운 Target
↓
방향 계산
↓
StraightProjectile 생성
↓
직선 이동
↓
Target Collider 충돌
↓
Damage 적용
↓
Projectile 제거
```

Projectile은 Kinematic `Rigidbody2D`와 Trigger `CircleCollider2D`를 사용한다.

### 12. WeaponTarget HP 시스템

테스트 적 역할을 위한 `WeaponTarget`을 추가했다.

기본 테스트 HP:

```text
30
```

피해를 받을 때 현재 HP를 감소시키고
0 이하가 되면 GameObject를 제거한다.

`Debug Blaster`가 Damage 10이므로:

```text
1발 → 20 HP
2발 → 10 HP
3발 → 0 HP → 제거
```

흐름을 확인할 수 있다.

### 13. Day08 테스트 타겟 자동 생성

Day08 Setup이 다음 테스트 구조를 자동 생성한다.

```text
===Environment===
└─ Day08_TestTargets
   ├─ Target_01
   ├─ Target_02
   └─ Target_03
```

각 Target에는:

- SpriteRenderer
- CircleCollider2D
- WeaponTarget
- HP 30

이 적용된다.

서로 다른 위치에 배치하여 가장 가까운 적 자동 탐색을 확인할 수 있다.

### 14. Day07 테스트 환경 정리

이전 단계의 충돌 테스트용:

```text
Day07_TestHazards
├─ TestEnemy
└─ TestObstacle
```

은 Day08 Setup에서 자동 제거한다.

실제 Day07 생존 시스템 코드 자체는 유지하고
테스트용 Scene 오브젝트만 다음 단계에 맞게 교체한다.

### 15. Day08 자동 Scene 구성

`ProjectEpsilonDay8Setup`이 다음 작업을 자동 처리한다.

- 기존 `SnakeBodyManager` 검색
- `SnakeWeaponManager` 추가 및 연결
- `DebugBlaster.asset` 생성
- 첫 Body에 Debug Blaster 장착
- Day07 TestHazards 제거
- Day08 TestTargets 3개 생성
- Scene 저장
- 이전 Day07 Setup 정리

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- Combat 스크립트 구조 추가
- `SnakeWeaponManager` 추가
- `SnakeWeaponSlot` 추가
- Body 목록 기반 Weapon Slot 동기화
- Body Count 변경 이벤트 구독
- Body 추가 시 Empty Slot 추가
- Body 제거 시 대응 Slot 제거
- 유지되는 Body의 기존 Weapon 보존
- 시작 Body 첫 슬롯에 Debug Blaster 장착
- `WeaponData`에 Projectile Speed / Lifetime 추가
- Debug Blaster ScriptableObject 생성
- Physical / StraightProjectile 설정
- Damage 10
- Attack Interval 1초
- Range 6
- Projectile Speed 8
- Projectile Lifetime 3초
- 실제 Body 월드 위치를 공격 원점으로 사용
- 가장 가까운 WeaponTarget 자동 탐색
- StraightProjectile 생성 및 이동
- Projectile 충돌 시 Damage 적용
- WeaponTarget HP 30
- HP 0 Target 제거
- Test Target 3개 자동 구성
- Day07 테스트 오브젝트 정리
- Day08 자동 Scene Setup 추가

현재 저장소에는 GitHub Actions / CI 상태 체크가 구성되어 있지 않다.
따라서 Unity Editor 실제 컴파일과 Play Mode 사격·충돌 동작은
로컬 Unity Console에서 최종 확인한다.

---

## 8일차 완료 기준

- [x] Body 1칸 = Weapon Slot 1개
- [x] Body / Weapon Slot 자동 동기화
- [x] Body 추가 시 Empty Slot 생성
- [x] Body 제거 시 Slot 제거
- [x] 기존 장착 무기 보존
- [x] WeaponData 기반 런타임 무기
- [x] Debug Blaster 자동 생성
- [x] 첫 Body에 시작 무기 장착
- [x] Body 실제 월드 위치에서 공격
- [x] 사거리 기반 Target 탐색
- [x] 가장 가까운 Target 선택
- [x] Slot별 공격 Cooldown
- [x] StraightProjectile 공격
- [x] Projectile 이동 / 수명
- [x] Target 충돌 피해
- [x] Target HP / 사망
- [x] 테스트 Target 3개 생성
- [x] Day08 자동 Scene 구성

---

## 다음 개발 방향

다음 Day09에서는 Day08의 공통 Weapon Slot / Auto Attack 구조를 확장한다.

주요 목표:

- 공격 타입별 실행 구조 분리
- Melee 테스트 무기
- StraightProjectile 구조 보강
- Area 공격 테스트
- 최소 3종 공격 방식 공통화
- 무기별 Body 위치 공격 검증
- 공격 연출용 간단한 Debug Visual
- 다수 Body에 여러 무기를 장착했을 때 독립 Cooldown 검증

Day08에서 만든 `SnakeWeaponManager`와 `SnakeWeaponSlot`을 유지하면서
Day09부터 다양한 무기 타입을 같은 슬롯 시스템에서 실행할 수 있도록 확장한다.
