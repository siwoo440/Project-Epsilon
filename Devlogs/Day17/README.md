---
# 프로젝트 ε 개발 일지 — Day17

---
## 개발 목표

Day16의 Enemy 추적 이동, 공통 상태 시스템, Cold·Electric 전투 효과를 보존한 상태에서 Poison·Explosion 속성 효과를 실제 전투 흐름에 연결한다.

- Poison 약화 상태와 중첩 구조 구현
- Poison 공격력 감소·방어 약화 배율 연결
- Explosion 범위 증가와 중심 거리 피해 구현
- Explosion 넉백 상태와 Enemy 이동 우선순위 연결
- Poison·Explosion 명중 Pulse 확장
- Poison·Explosion 빠른 시너지 테스트 입력
- Day17 Setup 저장·참조 검증과 Day16 Setup 정리

---
## 최신 커밋 기준

Day17 개발 코드는 GitHub `main`의 아래 커밋에 올라가 있다.

- Commit: `556404352c4348708b375706ff9b77d0cc1adbd9`
- 기존 커밋 메시지: `17`
- Day16 기준 커밋보다 1개 커밋 앞선 상태
- `Devlogs/Day17/README.md`는 해당 커밋에 아직 포함되어 있지 않음

GitHub Commit Status는 별도 CI 상태가 등록되어 있지 않으므로 Unity Compile과 Play Mode 성공 여부는 로컬 Unity에서 확인해야 한다.

---
## 기존 Day16 적용 확인

Day17 Setup은 기존 Day16 구성이 정상인지 먼저 확인한다.

- 정확한 WeaponTarget 6개
- `EnemyMovementController`
- `WeaponTargetStatusController`
- `EnemyStatusDebugControls`
- `WeaponAttributeSynergyManager`
- `WeaponAttributeEffectHooks`
- `WeaponAttributeCombatEffects`
- Cold / Electric Debug Weapon 참조

기존 Day16 기반이 불완전하면 Day17 Scene 변경을 중단하여 이전 기능을 보호한다.

---
## Day17 추가 구현

### 1. Poison 규칙

`WeaponAttributePoisonRules`를 추가했다.

- ×2 이상에서 Poison 시너지 활성
- 적 공격력 배율 0.95 적용
- 임시 방어 약화 배율 1.05 적용
- 동일 단계 재적용 허용
- 높은 단계 재적용 허용
- 낮은 단계 재적용 거부
- 임시 Poison 지속 시간 3초

공격력 -5%는 현재 Poison 상태에서 `OutgoingDamageMultiplier`로 제공한다. 실제 Enemy 공격 피해 계산부와의 최종 연결은 Enemy 공격 시스템 확장 시 사용하도록 기반을 마련했다.

### 2. Poison 대상 상태

`WeaponTargetPoisonStatus`를 추가했다.

- 현재 Poison 시너지 단계 저장
- 명중할 때마다 중첩 수 증가
- 같은 단계 또는 높은 단계 재명중 시 지속 시간 갱신
- 낮은 단계 재명중 거부
- `GameManager.IsPlaying` 상태에서만 지속 시간 진행
- 지속 종료 시 상태 컴포넌트 자동 제거
- `OutgoingDamageMultiplier`와 `IncomingDamageMultiplier` 외부 제공

`WeaponTarget.TakeDamage`는 Poison의 `IncomingDamageMultiplier`를 실제 최종 피해에 반영하도록 수정했다.

### 3. Explosion 규칙

`WeaponAttributeExplosionRules`를 추가했다.

- ×2 이상에서 Explosion 시너지 활성
- 폭발 범위 +10% 적용
- 중심에 가까울수록 추가 피해 적용
- 임시 중심 최대 추가 피해 +25%
- 임시 넉백 거리 0.75
- 임시 넉백 시간 0.18초

확정되지 않은 상위 시너지 단계별 추가 수치는 임의로 확장하지 않고 현재 공통값으로 유지한다.

### 4. Explosion 범위 피해

`WeaponAttributeCombatEffects`에 Explosion 전용 명중 흐름을 추가했다.

- Explosion은 일반 단일 피해 처리 전에 전용 범위 처리로 분기
- Area 무기는 공격 원점을 폭발 중심으로 사용
- 그 외 공격은 실제 명중 위치를 폭발 중심으로 사용
- 확장된 폭발 범위 안의 모든 `WeaponTarget` 방문
- 중심 거리에 따라 피해 배율 계산
- 생존 대상에 넉백 적용
- 각 대상마다 `HitTriggered` 전달
- 각 대상마다 Explosion Hit Pulse 생성
- 동일 프레임·동일 무기·동일 중심 중복 폭발 방지

### 5. 넉백 상태와 이동 우선순위

`WeaponTargetKnockbackController`를 추가했다.

- 넉백 방향 저장
- 남은 거리와 남은 시간 저장
- 넉백 면역 설정 지원
- 프레임별 이동량 계산
- 넉백 종료 시 자동 정리

`EnemyMovementController`는 넉백 이동을 일반 추적보다 먼저 처리한다.

```text
Knockback
   ↓
Stop / Slow가 반영된 CurrentMoveSpeed
   ↓
일반 Player 추적 이동
```

Debug Enemy는 Day17 Setup에서 넉백 면역을 끈 상태로 구성한다.

### 6. Poison·Explosion 명중 표시

기존 Attribute Hit Pulse 지원 범위를 확장했다.

- Poison Pulse 추가
- Explosion Pulse 추가
- 기존 Physical / Fire / Cold / Electric Pulse 유지

완성형 독 구름, 폭발 파편, 카메라 흔들림 등은 이후 VFX 단계에서 확장한다.

### 7. Debug 입력 확장

기존 Day15·Day16 입력을 유지하면서 Ctrl 조합으로 Day17 속성을 추가했다.

```text
F1  → Fire ×2
F2  → Fire ×4
F3  → Fire ×6
F4  → Fire ×8

F5  → 가까운 Enemy Slow
F6  → 가까운 Enemy Stop
F7  → Enemy 상태 해제

F8  → Cold ×2
F9  → Cold ×4
F10 → Cold ×6
F11 → Cold ×8

Shift + F8  → Electric ×2
Shift + F9  → Electric ×4
Shift + F10 → Electric ×6
Shift + F11 → Electric ×8

Ctrl + F8  → Poison ×2
Ctrl + F9  → Poison ×4
Ctrl + F10 → Poison ×6
Ctrl + F11 → Poison ×8

Ctrl + Shift + F8  → Explosion ×2
Ctrl + Shift + F9  → Explosion ×4
Ctrl + Shift + F10 → Explosion ×6
Ctrl + Shift + F11 → Explosion ×8
```

### 8. Day17 Setup

`ProjectEpsilonDay17Setup`을 구성했다.

- 기존 활성 Scene 보존
- `Game.unity` Additive 열기
- Game Scene에 미저장 변경이 있으면 자동 수정 중단
- Day16 기반 구성 선검증
- 정확한 WeaponTarget 6개 검증
- 각 Enemy에 `WeaponTargetKnockbackController` 확보
- Enemy 이동 관리자에 넉백 참조 연결
- Poison / Explosion Debug Weapon 참조 연결
- Scene 저장 후 Day16 기반과 Day17 참조 재검증
- 전체 검증 성공 후 Day16 Setup 정리
- 완료 로그: `[Project Epsilon] Day17 Poison and Explosion setup complete.`

---
## 패치 변경 파일

생성:

- `Assets/Editor/ProjectEpsilonDay17Setup.cs.meta`
- `Assets/Editor/ProjectEpsilonDay17SetupRules.cs`
- `Assets/Editor/ProjectEpsilonDay17SetupRules.cs.meta`
- `Assets/Scripts/Combat/WeaponAttributeExplosionRules.cs`
- `Assets/Scripts/Combat/WeaponAttributeExplosionRules.cs.meta`
- `Assets/Scripts/Combat/WeaponAttributePoisonRules.cs`
- `Assets/Scripts/Combat/WeaponAttributePoisonRules.cs.meta`
- `Assets/Scripts/Combat/WeaponTargetKnockbackController.cs`
- `Assets/Scripts/Combat/WeaponTargetKnockbackController.cs.meta`
- `Assets/Scripts/Combat/WeaponTargetPoisonStatus.cs`
- `Assets/Scripts/Combat/WeaponTargetPoisonStatus.cs.meta`

이름 변경:

- `Assets/Editor/ProjectEpsilonDay16Setup.cs` → `Assets/Editor/ProjectEpsilonDay17Setup.cs`

수정:

- `Assets/Scenes/Game.unity`
- `Assets/Scripts/Combat/WeaponAttributeCombatEffects.cs`
- `Assets/Scripts/Combat/WeaponTarget.cs`
- `Assets/Scripts/Debug/WeaponAttributeDebugControls.cs`
- `Assets/Scripts/Enemies/EnemyMovementController.cs`

삭제:

- `Assets/Editor/ProjectEpsilonDay16Setup.cs.meta`

---
## 임시 밸런스 값

현재 Day17 구현에서 확정되지 않은 값은 Rules 파일에 임시값으로 분리했다.

```text
Poison 지속 시간            3초
Poison 받는 피해 배율       1.05
Explosion 중심 최대 보너스  +25%
Explosion 넉백 거리         0.75
Explosion 넉백 시간         0.18초
```

현재 코드에서 명시적으로 적용된 ×2 기준값은 다음과 같다.

```text
Poison 적 공격력 배율       0.95
Explosion 범위 배율         1.10
```

---
## 정적 확인 결과

- Day17 구현 커밋이 `main` 최신 커밋으로 확인됨
- Day16 기준보다 1개 커밋 앞선 상태 확인
- Day17 관련 18개 파일 변경 확인
- `Game.unity` Day17 변경 포함
- Poison / Explosion Rules 분리 확인
- Poison 상태와 WeaponTarget 피해 배율 연결 확인
- Explosion 범위 피해와 Knockback 연결 확인
- Enemy 이동에서 Knockback 우선 처리 확인
- Day17 Debug Weapon 참조와 단축키 확장 확인
- GitHub Commit Status / CI는 등록되어 있지 않음

현재 환경에서 Unity Editor Compile과 Play Mode는 실행하지 않았으므로 실제 Unity 실행 검증은 확인 필요다.

---
## Unity 확인 항목

- Console Compile Error 없음
- `[Project Epsilon] Day17 Poison and Explosion setup complete.` 로그
- Enemy 6개 기존 추적 이동 유지
- F5 / F6 / F7 상태 기능 회귀 확인
- F8~F11 Cold 회귀 확인
- Shift+F8~F11 Electric 회귀 확인
- Ctrl+F8~F11 Poison 시너지 구성
- Poison 명중 시 상태 생성과 중첩 증가
- Poison 동일 단계 재명중 시 지속 시간 갱신
- 낮은 Poison 단계가 높은 단계를 덮어쓰지 않는지 확인
- Poison 대상이 받는 피해 증가 적용
- Ctrl+Shift+F8~F11 Explosion 시너지 구성
- Explosion 범위 +10% 적용 확인
- 폭발 중심과 외곽 피해 차이 확인
- Explosion 명중 대상 넉백 확인
- 넉백 중 추적 이동보다 넉백 이동이 우선되는지 확인
- 기존 Physical / Fire / Cold / Electric 전투 효과 회귀 확인

---
## 17일차 완료 기준

- [x] Poison 시너지 Rules
- [x] Poison 중첩 상태
- [x] Poison 공격력 감소 배율 기반
- [x] Poison 받는 피해 배율 실제 적용
- [x] Explosion 범위 증가
- [x] Explosion 중심 거리 피해
- [x] Explosion 넉백 상태
- [x] Enemy 넉백 우선 이동
- [x] Poison·Explosion 명중 Pulse
- [x] Poison·Explosion Debug 입력
- [x] Day17 Setup 구성
- [x] Day16 Setup 정리 구조
- [x] GitHub 최신 커밋 정적 확인
- [ ] Unity Compile
- [ ] Day17 자동 Setup 실행 확인
- [ ] Play Mode 회귀 테스트
- [ ] Poison 적 공격력 감소의 실제 Enemy 공격 피해 연결

---
## 다음 개발 방향

Day18은 Poison·Explosion의 로컬 Play Mode 검증 결과를 반영한 뒤 다음 속성 또는 Enemy 전투 확장 범위를 진행한다.
