---
# 프로젝트 ε 개발 일지 — Day15

---
## 개발 목표

Day14의 8속성 집계와 Effect Hook 위에 Physical·Fire의 실제 전투 효과를 연결한다.

- Physical 시너지 단계별 직접 피해 증가
- Fire 시너지 단계별 화상 지속 피해
- Melee / StraightProjectile / Area 공통 명중 처리
- 공격 시점 속성 정보를 실제 명중까지 유지
- 화상 재적용과 단계 교체 규칙 적용

---
## 구현 내용

### 1. Physical 직접 피해 증가

`WeaponAttributeDamageRules`에 Physical 시너지 피해 배율을 정의했다.

```text
비활성 → 100%
×2 → 110%
×4 → 120%
×6 → 135%
×8 → 150%
```

★등급 피해 계산 후 Physical 시너지 배율을 적용한다. 다른 속성의 직접 피해에는 Physical 배율을 적용하지 않는다.

### 2. Fire 화상 지속 피해

Fire 무기가 명중하면 시너지 단계에 따라 대상에게 화상을 적용한다.

```text
×2 → 2초 / 직접 피해의 10% 초당 피해
×4 → 3초 / 직접 피해의 15% 초당 피해
×6 → 4초 / 직접 피해의 20% 초당 피해
×8 → 5초 / 직접 피해의 25% 초당 피해
```

- 동일 단계 재명중 시 지속 시간 갱신
- 높은 단계 재명중 시 높은 화상으로 교체
- 낮은 단계 화상은 높은 단계 화상을 덮어쓰지 않음
- 화상 피해는 추가 화상을 발생시키지 않음
- 대상 사망 또는 지속 시간 종료 시 화상 상태 제거

### 3. 공격 Snapshot과 공통 명중 처리

`WeaponAttributeAttackSnapshot`은 공격이 시작된 시점의 정보를 저장한다.

```text
WeaponData
WeaponAttribute
AttributeCount
SynergyStage
Weapon Grade
Origin
DirectDamage
```

Projectile이 이동하는 동안 Body나 Weapon 구성이 바뀌어도 발사 시점의 속성 단계와 피해를 유지한다.

Melee·StraightProjectile·Area는 모두 `WeaponAttributeCombatEffects.ApplyHit`를 통해 피해와 속성 효과를 적용한다.

### 4. 실제 명중 Effect Hook

`WeaponAttributeEffectHooks`에 `HitTriggered`를 추가했다.

공격 발생 Hook과 실제 명중 Hook을 분리해 이후 Cold·Electric·Poison 등 대상 기반 효과가 실제 명중 대상을 사용할 수 있게 했다.

### 5. 전투 시각 효과

- Physical 활성 명중: 밝은 회색 Pulse
- Fire 활성 명중: 주황색 Pulse
- 화상 대상: 주황색 점멸
- 화상 종료: 원래 Sprite 색상 복구

### 6. Day15 자동 Scene 구성

`ProjectEpsilonDay15Setup`은 `Game.unity`에 `WeaponAttributeCombatEffects`를 추가하고 다음 참조를 연결한다.

- `SnakeWeaponManager`
- `WeaponAttributeSynergyManager`
- `WeaponAttributeEffectHooks`
- 명중 Pulse Sprite

Scene 저장과 참조 동일성 검증을 통과한 경우에만 Day14 Setup을 제거한다.

### 7. 추가 Sprite 반영

최신 임시 커밋에 포함된 Sprite PNG 50개와 대응 meta 50개를 프로젝트 에셋으로 유지했다.

이 Sprite들은 Day15 Physical·Fire 전투 로직의 필수 의존성이 아니며, 후속 아이템·속성·보상 UI에서 사용할 수 있는 별도 아트 리소스다.

---
## 변경 파일

생성:

- `WeaponAttributeDamageRules.cs`
- `WeaponAttributeHitContext.cs`
- `WeaponAttributeCombatEffects.cs`
- `WeaponTargetBurnStatus.cs`
- `ProjectEpsilonDay15Setup.cs`
- `ProjectEpsilonDay15SetupRules.cs`
- 추가 Sprite PNG 50개와 meta 50개

수정:

- `SnakeWeaponManager.cs`
- `StraightProjectile.cs`
- `WeaponAttributeEffectHooks.cs`
- `WeaponTarget.cs`
- `Game.unity`

삭제:

- `ProjectEpsilonDay14Setup.cs`

---
## 검증 결과

- Day15 속성 피해 규칙 테스트 통과
- Physical ×2 / ×4 / ×6 / ×8 경계값 확인
- Fire 지속 시간과 초당 피해 경계값 확인
- 동일·높은·낮은 단계 화상 교체 규칙 확인
- Unity 6000.3.21f1 Batch Compile 종료 코드 `0`
- Unity Compile Log에서 C# 컴파일 오류 없음
- Git diff 공백 오류 없음

GitHub CI는 등록되어 있지 않다. 정적 검토와 Unity Batch Compile상 문제 없음, 실제 Unity Play Mode는 로컬 확인 필요.

---
## Unity Play Mode 확인 항목

- F1 → Fire ×2와 Physical ×6
- F2 → Fire ×4와 Physical ×4
- F3 → Fire ×6와 Physical ×2
- F4 → Fire ×8
- Melee·StraightProjectile·Area 실제 명중
- Physical 단계별 직접 피해 변화
- Fire 화상 지속 피해와 재명중 시간 갱신
- 낮은 Fire 단계의 높은 화상 덮어쓰기 차단
- 적 사망 시 화상 컴포넌트와 색상 처리

---
## 15일차 완료 기준

- [x] Physical 단계별 직접 피해 증가 규칙
- [x] Fire 단계별 화상 규칙
- [x] 공격 시점 Attribute Snapshot
- [x] Melee·Projectile·Area 공통 명중 처리
- [x] 실제 명중 Effect Hook
- [x] 화상 갱신·교체 규칙
- [x] Physical·Fire 명중 시각 효과
- [x] Day15 자동 Scene Setup
- [x] Day14 Setup 안전 제거
- [x] Unity Batch Compile
- [ ] Unity Play Mode 수동 확인

---
## 다음 개발 방향

Day16에서는 Enemy 이동 기반과 공통 상태이상 시스템을 구현한다.

- `EnemyData.MoveSpeed` 실제 이동 적용
- Player 또는 Snake Head 추적
- 게임 정지 상태와 Enemy 이동 연동
- 상태이상 지속 시간·갱신·해제 공통 처리
- Cold 감속과 이후 Electric·Poison 효과를 위한 확장 지점
