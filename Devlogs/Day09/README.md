# 프로젝트 ε 개발 일지 — Day09

## 개발 목표

Day08에서 구현한 `Body 1칸 = Weapon Slot 1개` 구조와 자동 공격 기반을 확장하여,
실제 전투 루프의 첫 단계인 **공격 → 적 처치 → XP 드롭 → Head 흡수 → XP 누적** 흐름을 완성한다.

이번 일차의 핵심 목표:

- Melee / StraightProjectile / Area 공격 3종 구현
- 공격 타입별 공통 실행 구조
- 각 Body의 실제 위치를 공격 원점으로 유지
- 테스트 무기 3종 구성
- 적 사망 이벤트
- XP Gem 1 / 5 / 20 드롭
- Head 중심 XP 흡수
- XP 누적 시스템
- XP HUD 실제 연동
- Day09 테스트 환경 자동 구성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `a2fdf0e3ce647a5447eb60118136d17d93f13acb` |
| 기준 커밋 제목 | `9` |
| 이전 커밋 | `ba39305a4f45e5b13674339f684b53f929cdb3a3` |

---

## 진행 내용

### 1. 공격 타입 실행 구조 확장

Day08의 `SnakeWeaponManager`는 `StraightProjectile`만 실행하던 구조였다.

Day09에서는 `WeaponAttackType`을 기준으로 공격을 분기하도록 확장했다.

```text
Weapon Slot
↓
WeaponData.AttackType
↓
TryAttack()

├─ Melee
├─ StraightProjectile
└─ Area
```

현재 Day09에서 실제 실행되는 공격 타입은 다음 3종이다.

```text
Melee
StraightProjectile
Area
```

`HomingProjectile`, `Persistent`, `Special`은 이후 확장을 위해 기존 enum만 유지한다.

### 2. Melee 공격

`Melee`는 무기가 장착된 Body 위치를 기준으로 가장 가까운 적을 탐색한다.

```text
Body 위치
↓
Melee Range 안의 가장 가까운 Target
↓
즉시 Damage
```

공격이 성공하면 짧은 Debug Pulse를 생성하여 공격 범위를 시각적으로 확인할 수 있다.

테스트 무기:

```text
Debug Blade
Damage = 14
Attack Interval = 0.75초
Range = 1.35
```

### 3. StraightProjectile 공격 유지 및 조정

Day08의 직선 투사체 구조는 그대로 유지한다.

```text
Body 위치
↓
가장 가까운 Target 탐색
↓
Projectile 생성
↓
직선 이동
↓
명중 시 Damage
```

테스트 무기 `Debug Blaster`의 공격 간격은 Day09 테스트 흐름에 맞게 조정했다.

```text
Damage = 10
Attack Interval = 0.9초
Range = 6
Projectile Speed = 8
Projectile Lifetime = 3초
```

### 4. Area 공격

`Area` 공격은 해당 무기가 장착된 Body 위치를 중심으로 범위 내 모든 적에게 피해를 준다.

```text
Body 위치
↓
Area Range 검색
↓
범위 안 모든 WeaponTarget
↓
동시 Damage
```

`WeaponTarget.DamageAllInRange()`를 추가하여 범위 검색과 다중 피해를 공통 처리한다.

테스트 무기:

```text
Debug Pulse
Damage = 8
Attack Interval = 1.4초
Range = 1.8
```

공격 성공 시 범위를 확인할 수 있는 Debug Pulse가 표시된다.

### 5. Body 3칸에 테스트 무기 3종 배치

Day09 Setup은 시작 Body를 3칸으로 맞춘 뒤 각 슬롯에 테스트 무기를 배치한다.

```text
Body_01
→ Debug Blade
→ Melee

Body_02
→ Debug Blaster
→ StraightProjectile

Body_03
→ Debug Pulse
→ Area
```

따라서 한 번의 Play Mode에서 세 공격 방식을 동시에 확인할 수 있다.

### 6. WeaponAttackPulse 추가

Melee와 Area는 투사체가 없어 공격 여부가 눈에 잘 보이지 않기 때문에
`WeaponAttackPulse`를 추가했다.

주요 목적:

- Melee 공격 발생 위치 확인
- Area 공격 범위 확인
- Debug Visual 제공
- 짧은 시간 후 자동 제거

실제 최종 VFX가 아니라 공격 시스템 확인용 Debug 연출이다.

### 7. WeaponTarget 사망 이벤트 추가

기존 `WeaponTarget`은 HP가 0이 되면 바로 GameObject를 삭제했다.

Day09에서는 사망 시 다른 시스템이 반응할 수 있도록 `Died` 이벤트를 추가했다.

```text
TakeDamage()
↓
HP <= 0
↓
Die()
↓
Died Event
↓
GameObject 제거
```

중복 사망 처리를 막기 위해 `deathHandled` 상태도 추가했다.

### 8. 범위 피해 지원

`WeaponTarget`에 다음 기능을 추가했다.

```text
DamageAllInRange(
    origin,
    maximumRange,
    damage
)
```

활성 Target 목록을 순회해 범위 안의 살아 있는 Target에만 피해를 적용한다.

이 기능은 Day09의 Area 공격뿐 아니라 이후 폭발 / 범위 속성 공격에도 재사용할 수 있다.

### 9. Progression 구조 추가

XP 관련 기능을 Combat / Player 코드와 분리하기 위해
새 `Progression` 영역을 추가했다.

```text
Assets/Scripts/Progression
├─ SnakeExperience
├─ ExperienceDropper
└─ ExperiencePickup
```

전투 코드는 적 사망까지만 담당하고,
XP 생성 / 흡수 / 누적은 Progression 계층이 담당한다.

### 10. SnakeExperience 구현

플레이어의 현재 XP를 관리하는 `SnakeExperience`를 추가했다.

현재 Day09에서는 레벨업을 처리하지 않고 XP 누적까지만 담당한다.

```text
CurrentExperience
PreviewRequiredExperience
AddExperience()
ExperienceChanged
```

현재 미리보기 요구 XP는:

```text
10
```

으로 설정되어 있다.

실제 XP 요구량 계산과 Level Up은 Day10에서 연결한다.

### 11. 적 사망 시 XP Gem 드롭

`ExperienceDropper`는 `WeaponTarget.Died` 이벤트를 구독한다.

```text
Enemy HP 0
↓
WeaponTarget.Died
↓
ExperienceDropper
↓
ExperiencePickup Spawn
```

적마다 XP 값을 따로 설정할 수 있다.

Day09 테스트 값:

```text
Small XP = 1
Medium XP = 5
Large XP = 20
```

### 12. ExperiencePickup 구현

XP Gem은 적이 죽은 위치에서 생성된다.

Gem은 플레이어 Head가 일정 거리 안으로 들어오기 전에는 정지해 있다.

```text
XP Gem
↓
Head와 거리 확인
↓
Attraction Range 안으로 진입
↓
Head 방향으로 이동
↓
Collect Distance 도달
↓
XP 획득
↓
Gem 제거
```

기본 값:

```text
Attraction Range = 2.6
Attraction Speed = 6
Collect Distance = 0.22
```

### 13. Head 중심 흡수

XP의 수신자는 `SnakeExperience`가 붙어 있는 Player GameObject다.

즉 Body가 아니라 Head 위치를 기준으로 Gem이 끌려온다.

```text
Body가 Gem을 스침
→ 획득하지 않음

Head가 접근
→ Gem 흡수
→ XP 증가
```

이는 기획의 Head 중심 XP 획득 규칙과 일치한다.

### 14. XP Gem 크기 차등

XP 값에 따라 Debug Gem 크기를 다르게 설정했다.

```text
XP 1
→ Small

XP 5
→ Medium

XP 20
→ Large
```

동일한 Debug Sprite를 사용하지만 크기로 보상 가치를 구분할 수 있다.

### 15. XP HUD 실제 연동

`SnakeExperienceHUDPresenter`를 추가하여 기존 HUD의 Experience 표시와 연결했다.

```text
SnakeExperience
↓
ExperienceChanged
↓
SnakeExperienceHUDPresenter
↓
HUDController.SetExperience()
```

따라서 XP Gem을 먹으면 HUD 값도 실시간으로 변경된다.

예:

```text
XP 0 / 10
↓
XP 1 Gem 획득
XP 1 / 10
↓
XP 5 Gem 획득
XP 6 / 10
```

Day09에서는 10을 넘어가도 레벨업하지 않고 계속 누적된다.

### 16. Day09 테스트 타겟 구성

기존 `Day08_TestTargets`는 제거하고 새로운 테스트 환경을 구성했다.

```text
===Environment===
└─ Day09_TestTargets
   ├─ Target_XP01_A
   ├─ Target_XP01_B
   ├─ Target_XP05
   ├─ Target_XP20
   ├─ Target_Close_Left
   └─ Target_Close_Right
```

각 타겟은 서로 다른 HP / XP 값을 가져 다음을 한 번에 확인할 수 있다.

- 근접 공격
- 직선 투사체
- 범위 공격
- 동시 피해
- 사망
- XP 1 / 5 / 20 드롭
- Head 흡수

### 17. Day09 자동 Scene 구성

`ProjectEpsilonDay9Setup`은 다음 작업을 자동 처리한다.

- Game Scene 열기
- Player / SnakeBodyManager 검색
- Debug Blade 생성
- Debug Blaster 갱신
- Debug Pulse 생성
- Body를 시작 3칸으로 초기화
- Body 3칸에 3종 테스트 무기 장착
- `SnakeExperience` 추가
- XP HUD Presenter 연결
- Day08 TestTargets 제거
- Day09 TestTargets 생성
- Scene 저장
- 이전 Day08 Setup 제거

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeWeaponManager`의 공격 타입 분기 추가
- Melee 공격 구현
- StraightProjectile 공격 유지
- Area 공격 구현
- Body 위치 기반 공격 원점 유지
- Body 3개에 테스트 무기 3종 장착
- `WeaponAttackPulse` 추가
- `WeaponTarget.Died` 이벤트 추가
- 중복 사망 방지
- `DamageAllInRange()` 추가
- Progression 폴더 추가
- `SnakeExperience` 추가
- XP 누적 이벤트 추가
- `ExperienceDropper` 추가
- `ExperiencePickup` 추가
- XP 1 / 5 / 20 드롭 지원
- Head 중심 Attraction / Collection
- XP Gem 크기 차등
- `SnakeExperienceHUDPresenter` 추가
- HUD Experience 실제 연동
- Day09 테스트 타겟 6개 구성
- Day08 테스트 타겟 제거
- Day08 Setup 제거
- Day09 자동 Scene Setup 추가

현재 GitHub 커밋에는 CI / Actions 상태 체크가 등록되어 있지 않다.
따라서 Unity Editor 실제 컴파일 및 Play Mode 동작은 로컬 Unity 환경에서 최종 확인한다.

---

## 9일차 완료 기준

- [x] Melee 공격
- [x] StraightProjectile 공격
- [x] Area 공격
- [x] 공격 타입별 실행 구조
- [x] Body 실제 위치 공격
- [x] 3종 Debug Weapon
- [x] 적 HP / 사망
- [x] 사망 이벤트
- [x] Area 다중 피해
- [x] XP Dropper
- [x] XP Gem 1 / 5 / 20
- [x] Head 중심 XP 흡수
- [x] XP 누적
- [x] XP HUD 실제 연결
- [x] Day09 테스트 환경
- [x] Day09 자동 Scene 구성

---

## 다음 개발 방향

다음 Day10에서는 Day09의 `SnakeExperience`를 실제 성장 시스템으로 확장한다.

주요 목표:

- Level 시스템
- XP 요구량 곡선
- XP가 요구량을 넘으면 Level Up
- 초과 XP 이월
- 레벨업 시 Body +1
- Body 20 미만 성장 규칙
- Body 20 상태의 레벨업 처리
- 레벨업 UI
- Level / XP HUD 실제 동기화
- 레벨업 중 게임 일시정지
- 이후 Day11 무기 선택 후보 시스템이 호출될 수 있는 공통 LevelUp 이벤트

Day09의 전투 → XP 획득 루프를 유지하면서,
Day10부터 XP가 실제 Snake 성장으로 이어지게 만든다.
