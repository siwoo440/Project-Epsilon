# 프로젝트 ε 개발 일지 — Day06

## 개발 목표

Day05에서 완성한 동적 Body 관리 구조 위에 실제 생존 규칙을 연결한다.

이번 일차의 핵심은 다음과 같다.

- Body 전체가 공유하는 HP 시스템
- HP가 0이 될 때 꼬리 Body 제거
- 초과 피해를 다음 Body HP로 이월
- Head가 자기 Body와 충돌하는 판정
- 자기 충돌 시 HP를 무시하고 Body 2칸 직접 제거
- 자기 충돌 후 2초 전체 무적
- 실제 HP와 HUD 연결
- 이후 적 / 장애물 충돌이 사용할 공통 피해 기반 마련

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `02ef4609fb08cc5f8c03038145154a5f9653be7e` |
| 기준 커밋 제목 | `6` |

---

## 진행 내용

### 1. 공유 HP 시스템 추가

`SnakeHealth`를 추가하여 현재 Body 전체가 하나의 HP 값을 공유하도록 구성했다.

기본값:

```text
Maximum HP = 100
Current HP = 100
```

일반 피해는 먼저 공유 HP에서 차감된다.

예:

```text
Body 3
HP 100 / 100

25 피해

↓

Body 3
HP 75 / 100
```

### 2. HP 0 시 꼬리 Body 제거

현재 HP보다 큰 피해를 받으면 HP가 0이 된 시점에
`SnakeBodyManager.RemoveBodies(1)`을 호출하여 가장 뒤쪽 Body를 제거한다.

Body가 남아 있으면 다음 Body의 HP를 다시 100으로 설정한다.

```text
Body 3
HP 20 / 100

50 피해

↓

Body 1칸 제거
남은 피해 30

↓

Body 2
HP 70 / 100
```

### 3. 초과 피해 이월

한 번의 공격으로 여러 Body의 HP를 넘어가는 경우
남은 피해를 다음 Body에 계속 적용하도록 구성했다.

예:

```text
Body 3
HP 100 / 100

250 피해

↓

첫 Body 100 소모
두 번째 Body 100 소모
세 번째 Body에 50 피해

↓

Body 1
HP 50 / 100
```

Body 전체가 소진되면 HP는 0으로 유지된다.

### 4. 다중 Body 제거 API 추가

기존의 한 칸 제거 기능을 확장하여
`SnakeBodyManager.RemoveBodies(count)`를 추가했다.

주요 사용 예:

```text
일반 HP 파괴
→ RemoveBodies(1)

자기 충돌
→ RemoveBodies(2)
```

Body 제거 후에는 기존 Day05 구조를 그대로 사용하여 다음 요소가 자동 갱신된다.

- Body Index
- Body 이름
- Body Follow 목록
- Tail 위치
- HUD Body Count

### 5. 자기 몸 충돌 판정

`SnakeSelfCollision`을 추가하여 Head가 자신의 Body Collider와 접촉했을 때
자기 충돌로 처리하도록 구성했다.

판정 대상:

```text
SnakeSegmentType.Body
```

Tail은 자기 충돌 판정 대상에서 제외한다.

### 6. 자기 충돌 직접 Body 손실

자기 충돌은 공유 HP를 거치지 않는다.

```text
Head → 자기 Body 충돌

↓

HP 변화 없음
Body 2칸 직접 제거
```

예:

```text
Body 8
HP 65 / 100

자기 충돌

↓

Body 6
HP 65 / 100
```

남아 있는 Body가 2개보다 적으면 가능한 만큼만 제거한다.

### 7. 2초 전체 무적

`SnakeInvulnerability`를 추가하여 자기 충돌 직후 2초간 전체 무적을 적용한다.

```text
자기 충돌
↓
Body -2
↓
2초 Invulnerability
```

무적 상태에서는 `SnakeHealth.TakeDamage()`도 일반 피해를 거부한다.

따라서 자기 몸과 계속 겹쳐 있거나 이후 다른 피해 판정이 발생하더라도
2초 동안 반복 피해가 발생하지 않는다.

이 무적 시스템은 다음 일차의 적 / 장애물 피해에도 재사용할 수 있다.

### 8. Head 물리 구성

자기 충돌 Trigger 판정을 위해 Head에 다음 요소를 연결했다.

```text
CircleCollider2D
Rigidbody2D
```

Rigidbody 설정:

```text
Body Type = Kinematic
Gravity Scale = 0
Freeze Rotation
```

Head Collider는 Trigger 방식으로 사용한다.

### 9. Body Collider 자동 생성

`SnakeBodyManager`가 Body를 생성하거나 재정렬할 때
각 Body에 `CircleCollider2D`를 자동으로 보장하도록 수정했다.

```text
CircleCollider2D
Is Trigger = true
```

따라서 런타임에서 Body가 새로 추가되어도
별도의 수동 Collider 설정 없이 자기충돌 대상이 된다.

Tail에는 자기충돌용 Body Collider를 요구하지 않는다.

### 10. 실제 HP HUD 연동

`SnakeHealthHUDPresenter`를 추가하여
기존 HUD의 HP Text를 `SnakeHealth`와 연결했다.

```text
SnakeHealth
↓
HealthChanged
↓
SnakeHealthHUDPresenter
↓
HUDController.SetHealth()
```

따라서 피해를 받으면 HUD도 실제 현재 HP 값으로 갱신된다.

### 11. Debug 피해 테스트

적과 장애물이 아직 구현되지 않은 상태에서도
피해 흐름을 확인할 수 있도록 테스트 입력을 추가했다.

```text
P = 25 일반 피해
O = 120 일반 피해
K = 자기충돌 강제 테스트
```

기존 Day05 Body Debug 입력도 유지한다.

```text
] = Body +1
[ = Body -1
```

### 12. Day06 자동 Scene 구성

`ProjectEpsilonDay6Setup`을 추가하여 Day05 구조를
Day06 피해 / 충돌 구조로 자동 확장하도록 구성했다.

주요 Player 구조:

```text
Player
├─ PlayerInputReader
├─ SnakeMovement
├─ SnakePathRecorder
├─ CircleCollider2D
├─ Rigidbody2D
├─ SnakeInvulnerability
├─ SnakeHealth
├─ SnakeSelfCollision
└─ SnakeDamageDebugControls
```

HUD에는 `SnakeHealthHUDPresenter`가 추가된다.

Day06 Setup 적용 후 이전 `ProjectEpsilonDay5Setup`은 정리 대상으로 처리한다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeHealth` 추가
- 기본 공유 HP 100
- 일반 피해 HP 차감
- HP 0 시 Body 1칸 제거
- 초과 피해 다음 Body로 이월
- Body 전체 소진 시 HP 0 처리
- `RemoveBodies(count)` 기반 다중 Body 제거
- `SnakeInvulnerability` 추가
- 자기 충돌 후 2초 무적
- 무적 중 일반 피해 차단
- `SnakeSelfCollision` 추가
- 자기 Body만 충돌 대상으로 사용
- Tail 자기충돌 판정 제외
- 자기 충돌 시 Body 2칸 직접 제거
- Head `CircleCollider2D` / `Rigidbody2D` 구성
- 동적 Body Collider 자동 생성
- `SnakeHealthHUDPresenter`를 통한 HP HUD 연결
- Debug 피해 테스트 입력 추가
- Day06 자동 Scene Setup 추가
- `Game.unity`에 Day06 구성 변경 반영

현재 저장소에는 GitHub Actions / CI 상태 체크가 구성되어 있지 않다.
따라서 Unity Editor 실제 컴파일과 Play Mode 충돌 동작은 로컬 Unity Console에서 최종 확인한다.

---

## 6일차 완료 기준

- [x] 공유 HP 시스템 구현
- [x] HP 0 시 꼬리 Body 제거
- [x] 초과 피해 이월
- [x] 다중 Body 제거 API
- [x] 자기 Body 충돌 판정
- [x] 자기 충돌 시 Body 2칸 직접 제거
- [x] 자기 충돌 시 HP 유지
- [x] 2초 전체 무적
- [x] 무적 중 일반 피해 차단
- [x] Head 물리 Trigger 구성
- [x] Body Collider 자동 관리
- [x] Tail 자기충돌 제외
- [x] 실제 HP HUD 연결
- [x] Debug 피해 테스트 입력
- [x] Day06 자동 Scene 구성

---

## 다음 개발 방향

다음 7일차에서는 Snake 핵심 생존 단계의 마지막 기능을 연결한다.

주요 목표:

- Head와 적의 직접 충돌 규칙
- 적 직접 충돌 시 Body 1칸 즉시 손실
- 적 반복 충돌 보호 시간
- 장애물 충돌 시 공유 HP 25 피해
- 장애물 충돌 후 1.5초 전체 무적
- Boost 이동
- Stamina 100
- 약 4초 연속 Boost
- Boost 종료 후 1초 뒤 Stamina 회복
- 무적 우선순위 통합
- Stage 2 전체 통합 테스트

Day07이 끝나면 자동 전진, 곡선 회전, 동적 Body,
공유 HP, 자기충돌, 적/장애물 충돌, Boost/Stamina까지 연결되어
Snake 생존 코어 단계가 마무리된다.
