---
# 프로젝트 ε 개발 일지 — Day18

---
## 개발 목표

기존 8속성 시너지 시스템에 Holy·Dark 전투 효과를 연결하고, 회복·공유 보호막·저주·체력 흡수가 실제 공격 흐름에서 동작하도록 구현한다.

- Holy 단계별 피해 증가·회복·보호막 구현
- Holy ×8 보호막 종료·파괴 폭발 구현
- Dark 최대 5중첩 저주 구현
- Dark 저주 중첩 피해·처치 회복·체력 흡수 구현
- Dark ×8 최대 저주 처치 전파 구현
- HP HUD 보호막 표시
- Holy·Dark Debug 입력과 Day18 자동 Setup 구성

---
## 개발 기준

- 기준 커밋: `37ae61609965e817233b883c1a51fac3439592c4`
- 기준 커밋 제목: `17일차 : Poison·Explosion 속성 전투 효과 및 넉백 시스템 구현`
- Unity 버전: `6000.3.21f1`
- Day19 복합 속성 시너지는 이번 범위에서 제외

GitHub CI는 등록되어 있지 않다. 이번 개발은 Unity 배치 컴파일과 Editor 검증기를 실행했으며, 실제 Play Mode 전투 체감과 입력 테스트는 로컬 확인이 필요하다.

---
## Day18 추가 구현

### 1. Holy 시너지 규칙

`WeaponAttributeHolyRules`를 추가했다.

- ×2 이상: 직접 피해 +8%
- ×4 이상: 명중 시 낮은 확률로 공유 HP 1 회복
- ×6 이상: 일정 명중 횟수마다 단기 공유 보호막 생성
- ×8: 보호막 종료 또는 파괴 시 Holy 범위 폭발
- ×8 보호막 종료 또는 파괴 시 공유 HP 1 회복
- 음수 피해와 잘못된 단계 입력 방어

### 2. 공유 보호막

`SnakeShieldRules`와 `SnakeShieldController`를 추가했다.

- 기존 보호막보다 큰 보호막만 적용
- 보호막이 체력보다 먼저 피해 흡수
- 보호막 초과 피해만 기존 Body 공유 HP 흐름으로 전달
- 게임 진행 상태에서만 지속 시간 감소
- 자연 종료와 피해 파괴 구분
- 보호막 파괴 종료 효과를 잔여 체력 피해 처리 뒤 실행
- 보호막 변경·종료 이벤트 제공

`SnakeHealth`는 기존 Body별 공유 HP 처리 전에 보호막을 적용하도록 확장했다.

### 3. Holy 플레이어 효과

`WeaponAttributePlayerEffects`에서 Holy 명중 효과를 처리한다.

- Holy 단계 변경 시 누적 명중 수 초기화
- ×4 회복 확률 판정
- ×6 보호막 명중 주기 판정
- 마지막 Holy 공격 정보 저장
- 실제 Player 위치를 보호막 폭발 중심으로 사용
- 폭발 범위 대상마다 Holy Hit Hook 전달

### 4. Dark 저주 상태

`WeaponTargetDarkCurseStatus`를 추가했다.

- Dark ×2 이상 명중 시 저주 1중첩
- 최대 5중첩 제한
- 재명중 시 지속 시간 갱신
- 게임 진행 상태에서만 지속 시간 감소
- 지속 시간 종료 시 중첩 초기화

### 5. Dark 피해·흡수·처치 효과

`WeaponAttributeDarkRules`와 `WeaponAttributePlayerEffects`에 Dark 효과를 구현했다.

- ×4 이상에서 기존 저주 중첩마다 직접 피해 증가
- 실제 적용 피해량을 기준으로 체력 흡수 계산
- 소수 흡수량 누적 후 정수 단위 공유 HP 회복
- 최대 체력 상태의 흡수량 비축 방지
- ×6 이상에서 저주 대상 처치 시 낮은 확률로 공유 HP 1 회복
- 저주 없는 대상 처치 시 회복 차단
- ×8 최대 저주 대상 처치 시 주변 적에게 저주 전파
- ×8 체력 흡수율 강화

### 6. 실제 피해량 반환

`WeaponTarget.TakeDamageAndReport`를 추가했다.

- Poison 받는 피해 배율 적용 이후 실제 감소 체력 반환
- 대상의 남은 체력보다 큰 피해는 실제 감소량만 반환
- 기존 `TakeDamage` 호출 방식 유지
- 기존 사망·XP Drop 흐름 유지

### 7. 전투 파이프라인 통합

`WeaponAttributeCombatEffects`를 확장했다.

- Holy 직접 피해 계산 연결
- Dark 공격 전 대상 저주 중첩 조회
- Dark ×4 중첩 피해를 반영한 공격 Snapshot 생성
- Holy 명중 회복·보호막 처리
- Dark 명중 흡수·저주·처치 효과 처리
- Holy·Dark Hit Hook 전달
- Holy 금빛 Pulse와 Dark 보라색 Pulse 추가
- 기존 Physical·Fire·Cold·Electric·Poison·Explosion 처리 유지

### 8. HUD 확장

기존 HP HUD에 공유 보호막 수치를 함께 표시한다.

```text
HP 100 / 100  Shield 15
```

- HP 변경 시 Shield 수치 유지
- Shield 변경 시 HP와 Shield 동시 갱신
- 보호막 미적용 상태는 `Shield 0` 표시

### 9. Debug 입력

기존 입력 조합을 유지하면서 Alt 조합을 추가했다.

```text
Alt + F8  → Holy ×2
Alt + F9  → Holy ×4
Alt + F10 → Holy ×6
Alt + F11 → Holy ×8

Alt + Shift + F8  → Dark ×2
Alt + Shift + F9  → Dark ×4
Alt + Shift + F10 → Dark ×6
Alt + Shift + F11 → Dark ×8
```

### 10. Day18 자동 Setup

`ProjectEpsilonDay18Setup`을 추가했다.

- Day17 전투·Debug 기반 선검증
- Holy·Dark Debug Weapon 참조 연결
- `SnakeShieldController` 자동 추가
- `WeaponAttributePlayerEffects` 자동 추가
- 공유 HP·보호막·전투 효과·HUD 참조 연결
- Scene 저장 후 Day17 기반과 Day18 참조 재검증
- 전체 검증 성공 후 Day17 Setup 삭제
- 완료 로그: `[Project Epsilon] Day18 Holy and Dark setup complete.`

### 11. Day18 Editor 검증기

`ProjectEpsilonDay18Verification`을 추가했다.

- 보호막 15에 피해 20 적용 시 체력 전달 피해 5 확인
- 잔여 체력 피해 처리 전 보호막 종료 이벤트 차단
- 체력 피해 처리 후 보호막 종료 이벤트 1회 확인
- Dark ×6 무저주 대상 처치 회복 차단 확인
- Dark ×6 저주 대상 처치 회복 조건 확인

---
## 임시 밸런스 값

기획 문서에서 정확한 수치가 지정되지 않은 항목은 Rules 파일에 임시값으로 격리했다.

```text
Holy 명중 회복 확률       15%
Holy 보호막 발동 주기     5회 명중
Holy 보호막량             15
Holy 보호막 지속 시간     3초
Holy 폭발 반경            2
Holy 폭발 피해            적용 공격 피해의 50%

Dark 저주 지속 시간       6초
Dark 저주당 피해 증가     +6%
Dark ×6 처치 회복 확률    15%
Dark 기본 체력 흡수율     4%
Dark ×8 체력 흡수율       8%
Dark 저주 전파 반경       2.5
Dark 저주 전파 중첩       1
```

---
## 패치 변경 파일

생성:

- `Assets/Editor/ProjectEpsilonDay18Setup.cs`
- `Assets/Editor/ProjectEpsilonDay18Setup.cs.meta`
- `Assets/Editor/ProjectEpsilonDay18SetupRules.cs`
- `Assets/Editor/ProjectEpsilonDay18SetupRules.cs.meta`
- `Assets/Editor/ProjectEpsilonDay18Verification.cs`
- `Assets/Editor/ProjectEpsilonDay18Verification.cs.meta`
- `Assets/Scripts/Combat/WeaponAttributeDarkRules.cs`
- `Assets/Scripts/Combat/WeaponAttributeDarkRules.cs.meta`
- `Assets/Scripts/Combat/WeaponAttributeHolyRules.cs`
- `Assets/Scripts/Combat/WeaponAttributeHolyRules.cs.meta`
- `Assets/Scripts/Combat/WeaponAttributePlayerEffects.cs`
- `Assets/Scripts/Combat/WeaponAttributePlayerEffects.cs.meta`
- `Assets/Scripts/Combat/WeaponTargetDarkCurseStatus.cs`
- `Assets/Scripts/Combat/WeaponTargetDarkCurseStatus.cs.meta`
- `Assets/Scripts/Player/SnakeShieldController.cs`
- `Assets/Scripts/Player/SnakeShieldController.cs.meta`
- `Assets/Scripts/Player/SnakeShieldRules.cs`
- `Assets/Scripts/Player/SnakeShieldRules.cs.meta`
- `Devlogs/Day18/README.md`

수정:

- `Assets/Scenes/Game.unity`
- `Assets/Scripts/Combat/WeaponAttributeCombatEffects.cs`
- `Assets/Scripts/Combat/WeaponAttributeDamageRules.cs`
- `Assets/Scripts/Combat/WeaponTarget.cs`
- `Assets/Scripts/Debug/WeaponAttributeDebugControls.cs`
- `Assets/Scripts/Player/SnakeHealth.cs`
- `Assets/Scripts/UI/HUDController.cs`
- `Assets/Scripts/UI/HUDTextFormatter.cs`
- `Assets/Scripts/UI/SnakeHealthHUDPresenter.cs`

삭제:

- `Assets/Editor/ProjectEpsilonDay17Setup.cs`
- `Assets/Editor/ProjectEpsilonDay17Setup.cs.meta`

---
## 검증 결과

- Day18 순수 규칙 테스트 29개 통과
- Unity `6000.3.21f1` 배치 컴파일 성공
- Day18 Scene Setup 실행·저장·참조 재검증 성공
- Day18 Editor 통합 검증 성공
- 배포 파일 27개 해시 일치 확인
- 독립 코드 재검토 결과 Critical 0건, Important 0건
- `git diff --check -- Assets` 오류 없음
- GitHub CI 미등록

실제 Unity Play Mode 전투 입력과 체감 검증은 로컬 확인이 필요하다.

---
## Unity 확인 항목

- Console Compile Error 없음
- `Game.unity` Play Mode 진입
- 기존 6속성 공격과 Enemy Drop 회귀 확인
- Alt+F8~F11 Holy 단계 구성 확인
- Holy ×2 직접 피해 +8% 확인
- Holy ×4 명중 회복 확인
- Holy ×6 5회 명중 보호막 확인
- HP HUD Shield 수치 확인
- Holy ×8 보호막 종료·파괴 폭발과 HP 1 회복 확인
- Alt+Shift+F8~F11 Dark 단계 구성 확인
- Dark 저주 최대 5중첩 확인
- Dark ×4 중첩별 피해 증가 확인
- Dark 체력 흡수 확인
- Dark ×6 저주 대상 처치 회복 확인
- Dark ×8 최대 저주 처치 전파와 흡수 강화 확인

---
## 18일차 완료 기준

- [x] Holy 단계별 피해·회복 규칙
- [x] 공유 보호막과 SnakeHealth 연결
- [x] Holy ×8 보호막 종료 폭발
- [x] Dark 최대 5중첩 저주
- [x] Dark 중첩 피해 증가
- [x] Dark 실제 피해 기반 체력 흡수
- [x] Dark 저주 대상 처치 회복
- [x] Dark ×8 저주 전파와 흡수 강화
- [x] Holy·Dark Hit Hook과 Pulse
- [x] HP HUD Shield 표시
- [x] Holy·Dark Debug 입력
- [x] Day18 자동 Setup과 Day17 Setup 정리
- [x] Unity 배치 컴파일
- [x] Day18 Setup 실행·재검증
- [x] Day18 Editor 통합 검증
- [ ] 실제 Play Mode 전투·입력 검증

---
## 다음 개발 방향

Day19에서는 Day18 Play Mode 결과를 반영한 뒤 Holy+Fire, Dark+Poison, Holy+Dark 복합 속성 시너지를 구현한다.
