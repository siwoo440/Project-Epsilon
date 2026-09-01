---
# 프로젝트 ε 개발 일지 — Day16

---
## 개발 목표

Day15의 Physical·Fire 전투 효과를 보존한 상태에서 Enemy 추적 이동과 공통 상태 시스템을 실제 Cold·Electric 전투 효과까지 연결한다.

- Day15 Scene 참조 보존 검증
- `EnemyData.MoveSpeed` 기반 Player 추적
- Slow / Stop 공통 상태 기반
- Cold 3단계 누적 상태와 감속 연결
- Electric 연쇄 공격 연결
- 공격 시점 Attribute Snapshot 유지
- Cold·Electric 빠른 시너지 테스트 입력
- Day16 Setup 저장·참조 검증 강화

---
## 기존 Day15 적용 확인

현재 `Game.unity`에는 Day15 핵심 구성이 저장되어 있다.

- `WeaponAttributeSynergyManager`
- `WeaponAttributeEffectHooks`
- `WeaponAttributeCombatEffects`
- `SnakeWeaponManager.AttributeCombatEffects`
- `AttributeSynergyHUD`
- `WeaponAttributeDebugControls`의 Fire / Physical 참조

Day16 Setup은 이 구성이 모두 정상인지 먼저 검사하고, 하나라도 누락되면 Day16 Scene 변경을 중단한다.

---
## Day16 추가 구현

### 1. Cold 제어 규칙

`WeaponAttributeControlRules`를 추가했다.

- Cold 최대 누적 3단계
- 활성 시너지 단계는 ×2 이상
- 현재 기획서에서 확정된 ×2 감속 +15%를 이동 배율 0.85로 적용
- 미확정 ×4 / ×6 / ×8 추가 수치는 임의 확장하지 않음
- 냉기 지속 시간은 기존 Day16 F5 검증값 3초를 임시 플레이테스트 기준으로 재사용
- 최대 누적 도달 여부를 `IsFreezeReady`로 노출
- 빙결의 추가 발동 조건은 기획서 미확정이므로 실제 Stop 적용은 보류

### 2. Cold 대상 상태

`WeaponTargetColdStatus`를 추가했다.

- 같은 단계 재명중 시 누적 증가와 시간 갱신
- 높은 단계 재명중 허용
- 낮은 단계 재명중 거부
- 최대 3누적 제한
- 공통 `WeaponTargetStatusController.ApplySlow` 사용
- `GameManager.IsPlaying` 상태에서만 지속 시간 진행

### 3. Electric 연쇄 공격

`WeaponAttributeCombatEffects`에 Electric 실제 명중 효과를 연결했다.

- ×2 이상에서 연쇄 공격 활성
- 현재 확정된 연쇄 범위 +10% 적용
- 미확정 상위 단계 범위·대상 수 추가 보정은 넣지 않음
- 현재 최소 연쇄 구조로 추가 대상 1개 처리
- 공격 시점 `DirectDamage` Snapshot을 연쇄 대상에도 사용
- 이미 맞은 대상을 다시 선택하지 않음
- 주 대상에서 가장 가까운 다음 Enemy를 선택
- 연쇄 명중도 `HitTriggered`와 Electric Pulse 발생

### 4. Cold·Electric 명중 표시

기존 명중 Pulse를 확장했다.

- Physical: 밝은 회색
- Fire: 주황색
- Cold: 하늘색
- Electric: 황록색

완성형 상태이상 VFX와 ×6 / ×8 강조 VFX는 개발 일정의 Day20 범위로 유지한다.

### 5. Debug 입력 확장

기존 입력을 유지한다.

```text
F1  → Fire ×2
F2  → Fire ×4
F3  → Fire ×6
F4  → Fire ×8
F5  → 가까운 Enemy 50% 감속 / 3초
F6  → 가까운 Enemy 정지 / 1초
F7  → 모든 Enemy 상태 해제
F8  → Cold ×2
F9  → Cold ×4
F10 → Cold ×6
F11 → Cold ×8
Shift + F8  → Electric ×2
Shift + F9  → Electric ×4
Shift + F10 → Electric ×6
Shift + F11 → Electric ×8
```

### 6. Day16 Setup 강화

`ProjectEpsilonDay16Setup`을 확장했다.

- 기존 활성 Scene 보존
- Game Scene Additive 열기
- 이미 열린 Game Scene에 미저장 변경이 있으면 자동 수정 중단
- Day15 Physical·Fire 참조를 먼저 검증
- 정확한 WeaponTarget 6개 검증
- Enemy 이동·Rigidbody2D·EnemyDirect·상태 관리자 구성
- Cold / Electric Debug Weapon 참조 연결
- Scene 저장 후 Day15와 Day16 전체 참조 재검증
- 모든 검증 성공 후에만 Day15 Setup 삭제

---
## 패치 변경 파일

생성:

- `Assets/Scripts/Combat/WeaponAttributeControlRules.cs`
- `Assets/Scripts/Combat/WeaponTargetColdStatus.cs`

수정:

- `Assets/Scripts/Combat/WeaponAttributeCombatEffects.cs`
- `Assets/Scripts/Debug/WeaponAttributeDebugControls.cs`
- `Assets/Editor/ProjectEpsilonDay16Setup.cs`
- `Assets/Editor/ProjectEpsilonDay16SetupRules.cs`

자동 Setup 성공 후 삭제:

- `Assets/Editor/ProjectEpsilonDay15Setup.cs`

---
## 정적 검증 결과

- 패치 ZIP `Assets` 전용 구성
- README / 설명 파일 ZIP 미포함
- 신규 `.meta` GUID 포함
- 중괄호 구조 주석 없음
- C# 중괄호 균형 검사 통과
- 기존 Day15 `Configure` 4인자 호출 호환 오버로드 유지
- 기존 Day16 SetupRules 호출 호환 오버로드 유지

현재 실행 환경에는 Unity Editor가 없어 실제 Unity Compile과 Play Mode는 미확인 상태다.

---
## Unity 확인 항목

- Console Compile Error 없음
- `[Project Epsilon] Day16 enemy movement, Cold and Electric setup complete.` 로그
- Enemy 6개 Player 추적
- F5 / F6 / F7 기존 상태 검증
- F8~F11 Cold 시너지 구성
- Cold 명중 시 감속 적용
- Cold 같은 단계 재명중 시 누적 증가
- Cold 최대 3누적 제한
- 낮은 Cold 시너지 단계가 높은 단계 상태를 덮어쓰지 않는지 확인
- Shift+F8~F11 Electric 시너지 구성
- Electric 명중 후 가까운 다른 Enemy 1개 연쇄 피해
- Merge 중 Enemy 이동과 상태 시간 진행
- Day15 Physical·Fire 공격과 HUD 회귀 확인

---
## 16일차 완료 기준

- [x] Day15 Scene 구성 보존 검증
- [x] EnemyData MoveSpeed 실제 이동 코드
- [x] Player 추적 이동
- [x] Slow / Stop 공통 상태
- [x] Cold 3단계 누적 구조
- [x] Cold 공통 Slow 연결
- [x] Cold 최대 누적 빙결 준비 Hook
- [x] Electric 연쇄 공격 연결
- [x] Cold·Electric 명중 Pulse
- [x] Cold·Electric Debug 입력
- [x] Day16 Setup 안전 조건 강화
- [x] 정적 패치 검사
- [ ] Unity Compile
- [ ] Day16 자동 Setup 실행
- [ ] Play Mode 회귀 테스트

---
## 다음 개발 방향

Day17은 기획서 일정대로 Poison·Explosion 시스템부터 진행한다.
