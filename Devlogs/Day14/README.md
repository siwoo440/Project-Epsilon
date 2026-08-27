---
# 프로젝트 ε 개발 일지 — Day14

---
## 개발 목표

Day13 Prototype 전투 루프 위에 8속성 무기 집계와 ×2 / ×4 / ×6 / ×8 시너지 프레임워크를 연결한다.

- Physical / Fire / Cold / Electric / Poison / Explosion / Holy / Dark
- 무기 1개당 해당 속성 1개 집계
- ★등급과 속성 개수 분리
- 무기 획득·교체·합성·Body 변화 시 자동 재집계
- 공격 시 속성과 현재 시너지 정보를 Effect Hook으로 전달

---
## 구현 내용

### 1. 속성 시너지 규칙

`WeaponAttributeSynergyRules`에 속성 인덱스와 시너지 단계를 정의했다.

```text
0~1개 → 비활성
2~3개 → ×2
4~5개 → ×4
6~7개 → ×6
8개 이상 → ×8
```

### 2. 자동 속성 집계

`WeaponAttributeSynergyManager`는 `SnakeWeaponManager.SlotsChanged`를 구독한다.

따라서 다음 변경이 발생하면 점유 Weapon Slot을 다시 세어 속성별 개수와 단계를 갱신한다.

- Weapon 획득 및 교체
- 동일 Weapon 합성
- Body 추가·제거·복구에 따른 Slot 변경

합성은 기존 두 무기를 하나의 결과 무기로 재구성하므로 해당 속성 개수도 1 감소한다.

### 3. 공격 속성 Effect Hook

`WeaponGradeEffectHooks`에 모든 공격 시점의 공통 알림을 추가했다.

`WeaponAttributeEffectHooks`는 공격 알림을 받아 다음 정보를 후속 속성 효과 구현 지점으로 전달한다.

```text
WeaponData
WeaponAttribute
현재 속성 개수
현재 시너지 단계
Weapon Grade
Origin
Damage
```

### 4. HUD와 레벨업 후보

- HUD 우측 상단에 8속성별 개수와 활성 단계 표시
- Level Up 무기 후보에 `Attribute` 표시

### 5. Debug Weapon과 빠른 테스트

Day14 전용 Debug WeaponData 8개를 생성했다.

```text
F1 → Fire ×2
F2 → Fire ×4
F3 → Fire ×6
F4 → Fire ×8
```

필요한 Body가 부족하면 Debug 입력이 최대 8개까지 Body를 확보한 뒤 무기를 재배치한다.

### 6. Day14 자동 Scene 구성

`ProjectEpsilonDay14Setup`은 Game Scene에 시너지 관리자, 속성 Hook, HUD, Debug 입력을 연결한다.

Scene 저장과 모든 참조의 실제 연결 여부를 확인한 경우에만 Day13 Setup을 삭제한다.

---
## 변경 파일

생성:

- `WeaponAttributeSynergyRules.cs`
- `WeaponAttributeSynergyManager.cs`
- `WeaponAttributeEffectHooks.cs`
- `WeaponAttributeHUDPresenter.cs`
- `WeaponAttributeDebugControls.cs`
- `ProjectEpsilonDay14Setup.cs`
- `ProjectEpsilonDay14SetupRules.cs`
- Day14 Debug WeaponData 8개

수정:

- `WeaponGradeEffectHooks.cs`
- `LevelUpPanelController.cs`
- `Game.unity`

삭제:

- `ProjectEpsilonDay13Setup.cs`

---
## 검증 결과

- 속성 단계 경계값과 Setup 삭제 안전 조건 테스트 통과
- Unity 6000.3.21f1 Roslyn Runtime 컴파일 통과
- Unity 6000.3.21f1 Roslyn Editor 컴파일 통과
- Day13 Setup 삭제 전 저장 성공·참조 동일성 검증 적용

GitHub CI는 등록되어 있지 않다. 정적 검토상 문제 없음, 실제 Unity Compile/Play Mode는 로컬 확인 필요.

---
## 14일차 완료 기준

- [x] 8속성 집계
- [x] ×2 / ×4 / ×6 / ×8 시너지 단계
- [x] ★등급과 속성 개수 분리
- [x] Weapon / Merge / Body 변화 자동 재집계
- [x] 속성 HUD
- [x] Level Up Attribute 표시
- [x] 공격 속성 Effect Hook
- [x] 8속성 Debug Weapon
- [x] F1~F4 Fire 시너지 테스트
- [x] Day14 자동 Scene Setup
- [x] Day13 Setup 안전 삭제

---
## 다음 개발 방향

Day15에서는 Physical·Fire의 실제 전투 효과를 구현한다.
