# 프로젝트 ε 개발 일지 — Day07

## 개발 목표

Day06에서 구현한 공유 HP / 자기충돌 생존 시스템을 확장하여
외부 위험 요소와 Boost / Stamina 시스템을 연결하고 Snake 핵심 생존 단계를 마무리한다.

이번 일차의 핵심 목표:

- 적과 Head 직접 충돌 규칙
- 적 직접 충돌 시 Body 1칸 즉시 손실
- 적 직접 충돌 반복 방지용 1초 보호시간
- 장애물 충돌 시 공유 HP 25 피해
- 장애물 충돌 후 1.5초 전체 무적
- Boost 이동속도 1.5배
- Stamina 100 / 약 4초 연속 Boost
- Boost 종료 후 1초 뒤 Stamina 회복
- 초당 20 Stamina 회복
- Stamina HUD 실제 연동
- 테스트용 Enemy / Obstacle 자동 생성

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `1779aa3b77ddb26a5ea2bcc7e64aafb82f0d4734` |
| 기준 커밋 제목 | `7` |

---

## 진행 내용

### 1. Boost / Stamina 시스템 추가

`SnakeStamina`를 추가하여 기존 `PlayerInputReader.BoostPressed` 입력과 연결했다.

기본값:

```text
Maximum Stamina = 100
Boost Multiplier = 1.5
Drain Per Second = 25
Recovery Delay = 1초
Recovery Per Second = 20
```

최대 Stamina 상태에서 Boost를 유지하면 약 4초 동안 사용할 수 있다.

```text
100 ÷ 25 = 4초
```

### 2. Boost 이동속도 적용

기존 `SnakeMovement`를 수정하여 고정 이동속도 대신
`SnakeStamina.CurrentSpeedMultiplier`를 적용하도록 변경했다.

기본 이동속도:

```text
3
```

Boost 중:

```text
3 × 1.5 = 4.5
```

`SnakeMovement`는 Stamina 계산을 직접 하지 않고,
`SnakeStamina`가 계산한 현재 속도 배율만 사용하도록 역할을 분리했다.

### 3. Stamina 소진 처리

Boost 중 Stamina가 0에 도달하면 즉시 Boost를 종료한다.

또한 Boost 키를 계속 누른 상태에서 Stamina가 0 근처를 반복하며
Boost가 켜졌다 꺼지는 현상을 방지하기 위해,
완전히 소진된 경우 Boost 키를 한 번 놓아야 다시 사용할 수 있도록 처리했다.

### 4. Stamina 회복

Boost 입력이 끝나면 즉시 회복하지 않고 1초 동안 대기한다.

```text
Boost 종료
↓
1초 대기
↓
초당 20 회복
```

Stamina가 0인 상태에서는 약 5초 동안 회복하면 다시 100이 된다.

### 5. Stamina HUD 연동

`SnakeStaminaHUDPresenter`를 추가하여 기존 HUD의 Stamina 표시를 실제 값과 연결했다.

```text
SnakeStamina
↓
StaminaChanged
↓
SnakeStaminaHUDPresenter
↓
HUDController.SetStamina()
```

따라서 Boost 중 Stamina 감소와 회복이 HUD에 실시간 반영된다.

### 6. 외부 충돌 시스템 추가

`SnakeExternalCollision`을 추가하여 자기충돌 외의 외부 위험 요소를 처리하도록 구성했다.

현재 처리 대상:

```text
EnemyDirect
Obstacle
```

위험 요소 구분에는 `SnakeContactHazard`와 `SnakeContactHazardType`을 사용한다.

### 7. 적 직접 충돌 규칙

Head가 적과 직접 충돌하면 공유 HP를 사용하지 않고 Body를 직접 제거한다.

```text
Head → EnemyDirect
↓
Body -1
↓
HP 변화 없음
```

예:

```text
Body 6
HP 70 / 100

적 직접 충돌

↓

Body 5
HP 70 / 100
```

### 8. 적 직접 충돌 보호시간

적과 겹친 상태에서 Body가 연속으로 제거되는 것을 방지하기 위해
적 직접 충돌 전용 보호시간을 추가했다.

```text
Enemy Contact Protection = 1초
```

이 보호시간은 Day06의 Full Invulnerability와는 별개이며,
적 직접 충돌에만 적용된다.

### 9. 장애물 충돌 규칙

Head가 장애물에 충돌하면 Day06의 공유 HP 시스템을 사용한다.

```text
Obstacle
↓
SnakeHealth.TakeDamage(25)
```

예:

```text
HP 100
↓ 장애물
HP 75
```

### 10. 장애물 충돌 후 전체 무적

장애물 피해가 정상 적용되면 Day06에서 만든 `SnakeInvulnerability`를 이용하여
1.5초 동안 Full Invulnerability를 적용한다.

```text
장애물 충돌
↓
HP -25
↓
1.5초 전체 무적
```

### 11. 충돌 우선순위 통합

현재 Snake 생존 충돌 규칙은 다음과 같이 정리됐다.

```text
자기 Body 충돌
→ Body -2
→ Full Invulnerability 2초

Enemy 직접 충돌
→ Body -1
→ Enemy 전용 보호 1초

Obstacle 충돌
→ 공유 HP -25
→ Full Invulnerability 1.5초
```

Full Invulnerability 상태에서는 적 직접 충돌과 장애물 피해도 차단한다.

### 12. 테스트용 위험 요소 자동 생성

실제 Enemy 시스템과 맵 시스템이 아직 구현되지 않았기 때문에
Day07 기능을 확인할 수 있도록 테스트 오브젝트를 Scene에 자동 생성한다.

```text
===Environment===
└─ Day07_TestHazards
   ├─ TestEnemy
   └─ TestObstacle
```

`TestEnemy`:

- EnemyDirect 타입
- Trigger Collider
- 적 직접 충돌 규칙 테스트

`TestObstacle`:

- Obstacle 타입
- Trigger Collider
- 공유 HP 피해 / 무적 테스트

### 13. Day07 자동 Scene 구성

`ProjectEpsilonDay7Setup`을 추가하여 Day06 구조를 Day07 구조로 자동 확장한다.

주요 Player 구조:

```text
Player
├─ PlayerInputReader
├─ SnakeMovement
├─ SnakePathRecorder
├─ SnakeStamina
├─ SnakeHealth
├─ SnakeInvulnerability
├─ SnakeSelfCollision
├─ SnakeExternalCollision
├─ CircleCollider2D
└─ Rigidbody2D
```

HUD에는 `SnakeStaminaHUDPresenter`를 추가한다.

Day07 Setup 적용 후 이전 `ProjectEpsilonDay6Setup`은 정리 대상으로 처리한다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeStamina` 추가
- 최대 Stamina 100
- Boost 배율 1.5배
- 초당 25 Stamina 소모
- 약 4초 연속 Boost
- Boost 종료 후 1초 회복 지연
- 초당 20 Stamina 회복
- 완전 소진 시 Boost 키 재입력 요구
- `SnakeMovement`에 Stamina 속도 배율 연결
- `SnakeStaminaHUDPresenter` 추가
- `SnakeExternalCollision` 추가
- EnemyDirect 충돌 시 Body 1칸 제거
- Enemy 직접 충돌 1초 보호시간
- Enemy 직접 충돌 시 공유 HP 유지
- Obstacle 충돌 시 공유 HP 25 피해
- Obstacle 충돌 후 1.5초 Full Invulnerability
- Full Invulnerability 우선 판정
- `SnakeContactHazard` / `SnakeContactHazardType` 추가
- TestEnemy 자동 생성
- TestObstacle 자동 생성
- Day07 자동 Scene Setup 추가
- 이전 Day06 Setup 정리

현재 저장소에는 GitHub Actions / CI 상태 체크가 구성되어 있지 않다.
따라서 Unity Editor 실제 컴파일과 Play Mode 물리 충돌 동작은 로컬 Unity Console에서 최종 확인한다.

---

## 7일차 완료 기준

- [x] 적 직접 충돌 시스템
- [x] 적 직접 충돌 시 Body 1칸 직접 손실
- [x] 적 충돌 시 HP 유지
- [x] 적 직접 충돌 1초 보호시간
- [x] 장애물 공유 HP 25 피해
- [x] 장애물 후 1.5초 전체 무적
- [x] Full Invulnerability 우선순위 통합
- [x] Boost 1.5배 이동속도
- [x] Stamina 최대 100
- [x] 약 4초 Boost 소모 구조
- [x] Boost 종료 후 1초 회복 지연
- [x] 초당 20 Stamina 회복
- [x] Stamina HUD 실제 연동
- [x] 테스트용 Enemy / Obstacle 생성
- [x] Day07 자동 Scene 구성

---

## 2단계 완료 상태

Day04~07에서 Snake 핵심·생존 단계의 기본 시스템을 완성했다.

```text
Day04
자동 전진 / 곡선 회전 / 경로 기반 Body Follow

Day05
Head / Body / Tail 구분 / 동적 Body 추가·제거 / 최대 20

Day06
공유 HP / 초과 피해 / 자기충돌 / 2초 무적

Day07
적·장애물 충돌 / Boost / Stamina / 생존 규칙 통합
```

---

## 다음 개발 방향

다음 Day08부터 3단계 무기·성장 시스템 개발을 시작한다.

주요 목표:

- Body 1칸 = 무기 슬롯 1개 구조
- WeaponData 기반 런타임 무기 인스턴스
- 각 Body와 무기 슬롯 연결
- 가까운 적 자동 탐색
- 자동 공격 공통 구조
- 실제 Body 월드 위치에서 공격 생성
- 이후 근접 / 직선 투사체 / AoE 무기 테스트 기반 마련

Day07까지 구축한 Body 시스템을 그대로 유지하면서
다음 단계부터 Body 길이가 곧 전투력으로 이어지는 구조를 구현한다.
