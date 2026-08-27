# 프로젝트 ε 개발 일지 — Day05

## 개발 목표

Day04에서 구현한 고정 Body 3개 Follow 구조를 실제 게임 규칙에서 사용할 수 있는 동적 Snake Body 관리 시스템으로 확장한다.

Head / Body / Tail의 역할을 구분하고,
시작 Body 3칸, 최대 Body 20칸, 런타임 Body 추가·제거, 꼬리 우선 제거,
동적 Follow 갱신과 HUD Body 수 표시까지 연결한다.

---

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Project Epsilon |
| 엔진 | Unity 6000.3.21f1 |
| 기준 커밋 | `ed330667eff685edbcf3c0dd3652fec0a80935c3` |
| 기준 커밋 제목 | `5` |

---

## 진행 내용

### 1. Snake Body 관리자 구조 추가

`SnakeBodyManager`를 추가하여 Body의 생성, 제거, 초기화, 개수 제한을 한 곳에서 관리하도록 구성했다.

기본 규칙:

```text
시작 Body = 3
최대 Body = 20
```

공개 구조를 통해 이후 레벨업, 피해, 몸 복구, 무기 슬롯 시스템에서 동일한 Body 관리 기능을 재사용할 수 있도록 했다.

주요 기능:

- 현재 Body 수 조회
- 최대 Body 수 조회
- Body 추가
- Body 제거
- 시작 상태로 초기화
- Body 목록 조회
- Tail 조회
- Body 수 변경 이벤트

### 2. 런타임 Body 추가

`TryAddBody()`를 통해 플레이 중 Body를 동적으로 추가할 수 있도록 구현했다.

현재 Body가 최대 20칸에 도달하면 추가 요청을 거부한다.

```text
3 / 20
↓
4 / 20
↓
...
20 / 20
```

새 Body는 항상 현재 Body 목록의 마지막에 추가되며,
생성 직후 Follow 구조가 다시 연결된다.

### 3. 꼬리 쪽 Body 제거

`TryRemoveBody()`를 통해 가장 마지막 Body부터 제거하도록 구현했다.

```text
Head
Body_01
Body_02
Body_03
Body_04
Tail

Body -1

↓

Head
Body_01
Body_02
Body_03
Tail
```

현재 단계에서는 Body가 0까지 줄어드는 것을 허용한다.

Game Over 및 피해 규칙은 이후 공유 HP / 충돌 시스템에서 연결한다.

### 4. Head / Body / Tail 역할 구분

각 세그먼트에 `SnakeSegment`를 추가하고
`SnakeSegmentType`을 통해 Body와 Tail을 구분하도록 구성했다.

```text
Body
- SegmentType = Body
- BodyIndex = 0 이상

Tail
- SegmentType = Tail
- BodyIndex = -1
```

Tail은 시각적으로 Snake의 마지막 부분을 구성하지만
현재 Body 수에는 포함하지 않는다.

따라서 HUD의:

```text
몸 3 / 20
```

은 Body 3개만 의미하며 Tail은 제외한다.

### 5. Tail Follow 추가

기존 `SnakeBodyFollower`를 확장하여 Body뿐 아니라 Tail도 Head의 이동 경로를 따라가도록 수정했다.

Tail 위치:

```text
Head
↓
Body × N
↓
Tail
```

Body 수가 증가하거나 감소해도 Tail은 항상 마지막 Body 뒤쪽의 경로 위치로 자동 재배치된다.

### 6. 동적 Body Follow 갱신

Day04에서는 Body Transform 배열이 3개로 고정되어 있었다.

Day05에서는 `SnakeBodyManager`가 현재 Body 목록을 기준으로
`SnakeBodyFollower`를 다시 Bind하도록 변경했다.

따라서 Body 수가:

```text
3 → 4 → 8 → 15 → 20
```

처럼 변경되어도 Follow 구조를 수동으로 다시 연결할 필요가 없다.

각 Body는 기존 `SnakePathRecorder`의 이동 경로를 그대로 사용한다.

### 7. Body 수 변경 이벤트

Body 추가 / 제거 / 초기화 시:

```text
BodyCountChanged
```

이벤트를 발생시키도록 구성했다.

이 구조는 UI뿐 아니라 이후 다음 시스템에서도 사용할 수 있다.

- 무기 슬롯 갱신
- 사망 판정
- 캐릭터 상태 갱신
- 몸 성장 연출
- 몸 복구 처리

### 8. HUD Body 수 연동

`SnakeBodyHUDPresenter`를 추가하여
`SnakeBodyManager.BodyCountChanged` 이벤트를 HUD와 연결했다.

따라서 실제 Body 수가 변경되면 HUD의 Body 표시도 함께 갱신된다.

```text
3 / 20
→ Body 추가
4 / 20
→ Body 제거
3 / 20
```

기존 `HUDController.SetBodyCount()` 구조를 재사용한다.

### 9. Day05 Debug 테스트 입력

레벨업과 피해 시스템이 아직 구현되지 않았기 때문에
Body 증감 기능을 즉시 확인할 수 있는 테스트 입력을 추가했다.

```text
] = Body +1
[ = Body -1
```

테스트 입력은 `SnakeBodyDebugControls`로 별도 분리하여
실제 성장 / 피해 시스템이 구현된 이후 쉽게 제거할 수 있도록 했다.

### 10. Day05 자동 Scene 구성

`ProjectEpsilonDay5Setup`을 추가하여 기존 Day04 Snake 구조를 동적 Body 구조로 자동 전환하도록 구성했다.

목표 구조:

```text
===Gameplay===
├─ Player
│  ├─ PlayerInputReader
│  ├─ SnakeMovement
│  └─ SnakePathRecorder
│
└─ SnakeBody
   ├─ SnakeBodyFollower
   ├─ SnakeBodyManager
   ├─ SnakeBodyDebugControls
   ├─ Body_01
   ├─ Body_02
   ├─ Body_03
   └─ Tail

===UI===
└─ HUDCanvas
   ├─ HUDController
   └─ SnakeBodyHUDPresenter
```

Day05 자동 Setup이 적용되면 이전 `ProjectEpsilonDay4Setup`은 정리 대상으로 처리한다.

---

## 확인 결과

최신 GitHub 커밋을 기준으로 다음 항목을 확인했다.

- `SnakeBodyManager` 추가
- 시작 Body 3칸 설정
- 최대 Body 20칸 제한
- 런타임 Body 추가 구현
- 가장 마지막 Body 제거 구현
- `SnakeSegment` 추가
- Body / Tail 역할 구분
- Tail을 Body Count에서 제외
- `SnakeBodyFollower` 동적 Body 목록 지원
- Tail Follow 지원
- Body 변경 이벤트 구현
- `SnakeBodyHUDPresenter`를 통한 HUD 연동
- `[` / `]` Debug Body 증감 입력
- Day05 자동 Scene Setup 추가
- `Game.unity`에 Day05 구성 변경 반영
- 이전 Day04 Setup 정리

현재 GitHub Actions / CI 상태 체크는 구성되어 있지 않다.
따라서 Unity Editor의 실제 컴파일 및 Play Mode 동작은 로컬 Unity Console에서 최종 확인한다.

---

## 5일차 완료 기준

- [x] Head / Body / Tail 역할 구분
- [x] Body 중앙 관리자 추가
- [x] 시작 Body 3칸 적용
- [x] 최대 Body 20칸 적용
- [x] 런타임 Body +1
- [x] 런타임 Body -1
- [x] 꼬리 쪽부터 Body 제거
- [x] Tail을 Body Count에서 제외
- [x] Body 개수 변경 시 Follow 자동 갱신
- [x] Tail Follow
- [x] Body Count 변경 이벤트
- [x] HUD Body Count 연동
- [x] Debug 증감 입력
- [x] Day05 자동 Scene 구성

---

## 다음 개발 방향

다음 6일차에서는 Body 관리 시스템 위에 피해와 자기 충돌 규칙을 연결한다.

주요 목표:

- 공유 Body HP
- Head가 자기 Body와 충돌하는 판정
- 자기 충돌 시 꼬리 쪽 Body 2칸 제거
- 자기 충돌 후 2초 무적
- 공유 HP가 0이 되면 꼬리 Body 1칸 제거
- 초과 피해를 다음 Body HP로 이월
- Body가 감소해도 Tail / Follow / HUD가 즉시 정상 갱신
- 이후 적 / 장애물 충돌이 사용할 공통 피해 API 준비

Day05에서 만든 `SnakeBodyManager`를 Body 제거의 단일 진입점으로 유지하여
이후 피해, 충돌, 성장, 무기 슬롯 시스템이 같은 구조를 공유하도록 진행한다.
