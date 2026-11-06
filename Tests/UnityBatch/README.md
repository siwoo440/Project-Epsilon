---

# Unity 비대화형 통합 검증

이 폴더에는 실제로 실행한 Unity 에디터 배치 검증 스크립트를 보관한다. 제품 씬이나 런타임 스크립트가 아니며, 반드시 **별도 프로젝트 사본**에서 실행한다. 테스트는 끝나면 편집기 프로세스를 종료한다.

27일차 시작 무기 선택을 추가하면서 기존 배치에 공용 선택 헬퍼를 연결했다. 아래 과거 결과는 변경 전 스크립트와 해당 일차 제품의 실행 결과이며, 헬퍼를 연결한 현재 버전의 전체 Unity 회귀는 아직 완료되지 않았다.

---

## 준비와 실행

1. `Assets`, `Packages`, `ProjectSettings`를 포함한 Unity 프로젝트 사본을 별도 폴더에 준비한다. 원본 프로젝트나 다른 작업 중인 프로젝트를 사용하지 않는다.
2. 선택한 검증 스크립트와 `StartingWeaponBatchHelper.cs`를 사본의 `Assets/Editor`에 복사한다. 공용 헬퍼는 실제 시작 무기 첫 후보 버튼을 선택한 뒤 각 검증을 진행한다. 시작 무기 선택 자체를 검사하는 전용 스크립트에는 헬퍼를 적용하지 않는다. 원본 프로젝트의 `Assets`에는 복사하지 않는다.
3. 프로젝트 버전인 Unity **6000.3.21f1**의 `Unity.exe`를 사용한다. `-batchmode`, `-nographics`, 사본을 가리키는 `-projectPath`, 아래 표의 `-executeMethod`, 별도 `-logFile`을 지정한다. `-quit`은 플레이 모드 진입 전에 종료될 수 있으므로 사용하지 않는다.
4. 프로세스가 종료된 뒤 로그의 테스트 접두사와 종료 코드를 확인한다. Unity 초기 에셋 가져오기와 컴파일 시간은 플레이 시간과 별개다.

| 스크립트 | 실행할 메서드 | 실제로 실행한 조건 |
| --- | --- | --- |
| `AutomatedPilotVerification.cs` | `ProjectEpsilon.Editor.AutomatedPilotVerification.Run` | 정상 속도 300초, 가상 A/D·Space 회피 입력, 레벨업·보상 UI 자동 선택. 무적·능력치 변경 없음. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedEliteRewardVerification.cs` | `ProjectEpsilon.Editor.AutomatedEliteRewardVerification.Run` | 출현기 시간을 240초로 설정하고 엘리트를 강제 처치. XP 보석과 보상 일시정지, 실제 UI 버튼 이벤트, 장착·재개 확인 |
| `AutomatedBodyHudVerification.cs` | `ProjectEpsilon.Editor.AutomatedBodyHudVerification.Run` | 초기 물리 단계 뒤 게임을 일시정지하고 성장·손실·복구 API를 직접 호출. 세 수치, 이벤트, HUD 텍스트와 너비 확인 |
| `AutomatedCombatProgressionVerification.cs` | `ProjectEpsilon.Editor.AutomatedCombatProgressionVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체 없는 정지 표적을 배치. 실제 몸통 무기 공격, 보석, 레벨업과 UI 버튼 이벤트 확인 |
| `AutomatedEarlyWeaponVerification.cs` | `ProjectEpsilon.Editor.AutomatedEarlyWeaponVerification.Run` | 이동·몸통 추적·자연 스폰을 끄고 세 번째 몸통과 정지 표적을 배치. 실제 시험 무기 6종의 자동 공격과 실제 출현기로 통제 생성·강제 처치한 일반 적 3종의 경험치 확인 |
| `AutomatedSteelSwordGradeVerification.cs` | `ProjectEpsilon.Editor.AutomatedSteelSwordGradeVerification.Run` | 실제 시작 버튼을 자동 선택하고 이동·몸통 추적·자연 스폰을 끈 뒤 실제 세 번째 몸통과 정지 표적을 배치. 강철 검 등급별 자동 공격, 실제 사망 콜백의 대상 변경과 물리 시너지 확인 |
| `AutomatedExperienceMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedExperienceMergeVerification.Run` | 실제 씬에서 플레이어 이동과 자연 출현을 끄고 보석과 검사 설정을 통제. 자동 병합과 경험치 보존, 우선순위, 일시정지, 활성 상태 및 실제 수집 확인 |
| `AutomatedExperienceMergePartialVerification.cs` | `ProjectEpsilon.Editor.AutomatedExperienceMergePartialVerification.Run` | 실제 씬에서 이동·자연 출현을 끄고 보석·검사 예산을 통제. 다중 프레임 부분 그룹, 동적 삽입, 선택 재료 이동·수집과 경험치 보존 확인 |
| `AutomatedStartingWeaponVerification.cs` | `ProjectEpsilon.Editor.AutomatedStartingWeaponVerification.Run` | 공용 헬퍼 없이 세 번의 새 플레이에서 실제 시작 후보 UI, 가상 숫자 입력, 시작 전 정지와 첫 몸통 장착 확인. `RunDisabledBeforeStart`는 선택기 비활성·재구성 경계를 별도로 확인 |
| `AutomatedSettingsPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedSettingsPriorityVerification.Run` | XP 주입과 만석 슬롯을 강제하고 가상 ESC·숫자 입력 및 실제 UI 버튼 이벤트 사용. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedCollisionPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedCollisionPriorityVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체를 제어. 실제 2D 물리 접촉과 별도의 인위 콜백 순서·수명주기 시험 사용 |
| `AutomatedMergeSelectionVerification.cs` | `ProjectEpsilon.Editor.AutomatedMergeSelectionVerification.Run` | 실제 게임 씬에서 자연 출현과 충돌을 끄고 몸통 20칸과 무기를 강제 구성. 가상 E·ESC·이동 입력과 실제 UI 버튼 이벤트로 합성 선택, 시너지 미리보기와 우선순위 확인 |
| `AutomatedRewardMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedRewardMergeVerification.Run` | 실제 게임 씬에서 이동과 자연 출현을 끄고 장착 무기·강화와 미장착 보상 값을 제어. UI 없이 실제 무기 관리자의 보상 합성 API와 시너지 확인 |
| `AutomatedLevelRewardMergeVerification.cs` | `ProjectEpsilon.Editor.AutomatedLevelRewardMergeVerification.RunAll` | 실제 게임 씬에서 자연 출현과 자기 충돌을 끄고 경험치·장착 무기·엘리트 후보를 제어. 가상 E·ESC·숫자 입력, 실제 UI 버튼 이벤트와 통제된 2D 적 접촉 사용 |
| `AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 정상 속도, 자연 적 출현·무기 공격·보석 획득과 가상 A/D·Space 조종. 능력치와 경험치 주입 없음, 선택 화면은 자동 선택 |
| `NaturalTrajectory/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 정상 속도와 자연 출현을 유지하고, 조종기가 가속·회전 경로와 몸통 접촉 위험을 비교한 세 번째 시험 |
| `NaturalBalanced/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 24일차 합성 구현 사본에서 공격 거리 선호를 높이고 실제 무기 거리와 적 피해를 기록한 네 번째 시험 |
| `NaturalFocus/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 자연 적 한 명의 체력·거리를 추적하면서 연속 공격과 안전 이동을 시험한 다섯 번째 시험 |
| `NaturalDiagnostics/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 동일 조종 정책에서 실제 위치 변화·추적 적 속도·조향 후보 점수를 기록하는 최대 60초 진단 |
| `NaturalCurve/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 적을 향한 곡선 재접근 후보를 비교한 자연 전투 시험 |
| `NaturalHold/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 적 재접근을 위한 단시간 회전 유지와 근사 접촉 위험 검사를 시험한 자연 전투 시험 |
| `NaturalHoldAll/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 부상 여부와 관계없이 단시간 회전 유지와 위험 시 해제를 시험한 자연 전투 시험 |
| `NaturalDirection/AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 같은 방향을 최대 4초 유지하며 위험 시 해제하는 자연 전투 시험 |

자연 전투 스크립트는 같은 클래스 이름을 사용하므로 실행할 버전 하나만 사본의 `Assets/Editor/AutomatedNaturalCombatVerification.cs`로 복사한다. 여러 파일을 동시에 복사하면 중복 클래스로 컴파일할 수 없다.

예를 들어 자동 조종 검증은 다음과 같이 실행한다. 경로는 설치된 Unity와 별도 프로젝트 사본에 맞게 지정한다.

```powershell
& "C:/Program Files/Unity/Hub/Editor/6000.3.21f1/Editor/Unity.exe" -batchmode -nographics -projectPath "<검증 사본 경로>" -executeMethod ProjectEpsilon.Editor.AutomatedPilotVerification.Run -logFile "<검증 로그 경로>" # 자동 생존 검증
```

---

## 결과 해석

- 자동 조종은 사람의 수동 플레이가 아니다. 300초 생존 여부와 경험치·레벨업 발생 여부를 따로 확인한다.
- 자연 전투 최초 시도는 조종 입력 확인을 위해 초기에 A·D·Space를 순서대로 입력했다. Unity 6000.3.21f1, 정상 속도, 무적·능력치·경험치·적 출현 조작 없이 6처치와 보석 XP 6/10을 얻었고, 게임 시간 57.2초에 레벨 1로 패배해 종료 코드 1이었다. 가상 입력 및 실제 회전·가속은 확인됐으나 자연 레벨업은 확인되지 않았다. 보관한 자연 전투 스크립트는 이 시도에 해당한다.
- 자연 전투 두 번째 시도는 초기 고정 입력을 제거하고 머리와 몸통의 예상 접촉 위험을 회피하는 시험 조종기를 사용했다. 같은 게임 조건에서 1처치와 보석 XP 1/10을 얻었고, 게임 시간 12.6초에 레벨 1로 패배해 종료 코드 1이었다. 두 시도 모두 자동 조종 결과이며 게임의 난이도 문제를 단독으로 입증하지 않는다.
- 자연 전투 세 번째 시도는 23일차 제품 사본, 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종 경로를 비교했다. 게임 시간 300.0초, 실제 플레이 시간 300.5초에 몸통 3칸·HP 92로 생존했고 A/D·Space 입력과 실제 회전·가속을 확인했다. 다만 적 처치 0, 경험치 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. 재현 코드는 `NaturalTrajectory` 하위 폴더에 있다.
- 자연 전투 네 번째 시도는 24일차 합성 구현이 포함된 커밋 전 사본을 사용했다. 사본의 `SnakeWeaponManager.cs` SHA-256은 `1B14F30D04A9B87FD50D90A0C1FCF0DF3EA74A6C02FFA30D83AD209819E96760`, `SnakeWeaponMergeController.cs` SHA-256은 `6B25697BEE51C38A68EB20473EA94031DC2EFCF2642D76FFC95CFD0B6A8AB746`였다. 두 해시는 해당 파일만 식별하며 사본 전체의 일치를 증명하지 않는다.
- 네 번째 시도는 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종하고 UI 선택을 자동화했다. 게임 시간 300.0초, 실제 플레이 시간 302.9초에 몸통 3칸·HP 64로 생존했고 A/D·Space 입력과 실제 회전·가속, 네 차례의 적 체력 감소를 기록했다. 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalBalanced` 하위 폴더에 있다.
- 자연 전투 다섯 번째 시도는 24일차 `8cafb51`의 `Assets`, `Packages`, `ProjectSettings` 추적 파일 427개에 25일차 `SnakeWeaponManager.cs`만 추가 반영한 사본에서 실행했다. 해당 무기 관리자 SHA-256은 `9E731853B3AAE04710DD1935761C7FE4CE117C8EC3756003B9F25029D593E2F1`이다. 정상 속도·자연 출현·가상 입력을 사용하고 무적·능력치·XP·적 출현을 조작하지 않았다. UI 선택은 자동화했다.
- 다섯 번째 시도는 두 추적자의 체력이 각각 25→15→5로 감소했지만 처치와 XP는 없었다. 게임 시간 300.0초, 실제 플레이 시간 300.1초에 몸통 3칸·HP 100으로 생존했고 실제 A/D·Space 입력과 회전·가속을 확인했다. 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalFocus` 하위 폴더에 있다.
- 별도의 무작위 출현 실행에서 단일 적 추적 조종의 이동 진단을 진행하다 26일차 Unity 검증을 위해 중단했다. 마지막 로그는 게임 시간 18.03초다. 13.02초의 0.5초 표본에서 플레이어 평균 이동 속도는 3.00, 적은 2.40이었고, 적은 전진 방향 뒤쪽에 있었다. 당시 직진 후보 점수는 5.93, 우회전 6.73, 좌회전 169.42였으며 직진이 선택됐다. 10.52초 가속 중 플레이어 평균 속도는 4.50, 적은 2.40이었다. 이는 관찰 구간에서 거리 증가에 기여할 수 있는 근거이나 다섯 번째 시도와는 다른 무작위 실행이므로 그 시험의 거리 정체 원인을 확정하지 못한다. 실행 셸은 중단되어 코드 1로 끝났으며 예정된 60초 진단은 완료되지 않았다. 재현 코드는 `NaturalDiagnostics` 하위 폴더에 있다.
- 곡선 재접근 시도는 25일차 `d8a7b8c` 기준에 26일차 `SnakeLevelUpController.cs`, `SnakeWeaponMergeController.cs`, `LevelUpPanelController.cs`, `WeaponMergePanelController.cs` 네 파일을 동기화한 사본을 사용했다. 인계 후 제품 파일은 추가 동기화하지 않았다. 네 파일의 SHA-256은 순서대로 `0D1F929775CC5EBB33BD627D7781A92192368F2FD0F09D17BA1251DBE93685E6`, `3506A88FF4F4DCDB3366F0E820C7174A05355E888BC3705BFE1A5DF9AB3AB35A`, `3913E859FD46006A1663A360DC12C36E7F7A615BA91C581B6CB9472168F141D5`, `2F7CA9D705A18388308F9275819785E0F0DE0A6B261B1FEECB15CA4449BF2592`이다. 이 해시는 해당 파일만 식별한다.
- 곡선 재접근 시도는 정상 속도와 자연 출현, 무적·능력치·경험치 주입 없이 조종하고 UI 선택을 자동화했다. 게임 시간 300.0초, 실제 플레이 시간 300.3초에 몸통 3칸·HP 100으로 생존했고 실제 A/D·Space 입력과 회전·가속을 확인했다. 추적자 한 명에게 25→15, 다른 한 명에게 25→15→5의 피해를 주었으나 처치 0, XP 0, 레벨 1, 보상 선택 0으로 성장 조건에 실패해 종료 코드 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalCurve` 하위 폴더에 있다.
- 회전 유지 시도는 27일차 `2fa479c1f10f5e2aa38153c8fc450df76c715d12` 제품 스냅샷과 실질 내용이 일치하는 사본에서 진행했다. 확인한 제품 파일 433개 중 406개는 SHA-256이 바이트 단위로 일치했고, 26개는 줄바꿈, 후보 자산 메타 파일 1개는 빈 필드의 끝 공백만 달랐다. 인계 후 제품 파일은 동기화하지 않았다.
- 회전 유지 시도에서는 시작 화면이 게임 시간을 정지한 상태에서 실제 첫 후보 버튼 이벤트를 실행해 강철 검 ★1 하나가 첫 몸통에 장착된 것을 확인했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·적 출현·시간 배율은 시험 코드가 변경하지 않았으며 UI 선택은 자동화했다. 마지막 기록은 게임 시간 120.0초·실제 플레이 시간 119.9초, 몸통 3칸·HP 100, 처치 0·XP 0·레벨 1이었다. 부상 적 조건을 충족하지 못해 회전 유지 정책은 한 번도 발동하지 않았다. 28일차 시험을 위해 실행 셸에 중단 신호를 보냈으며 셸 종료 코드는 1이다. 300초 생존, 회전 정책 효과와 자연 레벨업은 이 미완료 실행에서 검증되지 않았다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalHold` 하위 폴더에 있다.
- 부상 조건 없는 회전 유지 시도는 27일차 `2fa479c`의 제품 파일 433개를 확인한 사본에 28일차 `WeaponData.cs`, `WeaponTarget.cs`, `SnakeWeaponManager.cs`, `DebugBlade.asset`만 동기화한 커밋 전 제품 스냅샷에서 실행했다. 네 파일의 SHA-256은 순서대로 `8DFC662A1932123340A4254CC1237F074C83DB21E0E87E11950F8CAFBF4077B5`, `3B57D558300F096D4B8702ACB4195F6EAE387A961A1A2293883732FCDCDEE364`, `733A92CD3FC7E2E703298987E5CB26A736DD1A006698853D62516ACCA542CA0B`, `A2905BDC6CB90D2A7DC47B2867EFBAB083894483632AD69B13CEF4852198BEC1`이다. 이 해시는 해당 파일만 식별한다.
- 부상 조건 없는 회전 유지 시도에서는 시간 정지 상태에서 실제 시작 첫 후보 버튼 이벤트로 강철 검 ★1 하나를 장착했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·출현·시간 배율을 조작하지 않았으며 UI 선택은 자동화했다. 게임 시간 300.0초·실제 플레이 시간 313.8초에 몸통 3칸·HP 100으로 생존하고 A/D·Space 입력 및 실제 회전·가속을 확인했다. 유지 회전 273회 중 연속 회전 방향이 바뀐 경우는 215회, 예측 위험 조기 해제는 2회였다. 실제 처치·XP·보상 선택은 0이었고 자연 성장 조건에 실패해 Unity 종료 코드는 1이었다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalHoldAll` 하위 폴더에 있다.
- 최대 4초 방향 유지 시도는 바로 앞 시험과 같은 27일차 `2fa479c` 기준 및 28일차 제품 네 파일 동기화 사본에서 진행했다. 시험 시작 전 같은 사본의 Unity 프로세스가 없었고, 네 제품 파일과 시작 선택기·씬의 SHA-256이 앞 시험과 동일함을 확인했다. 인계 후 제품 파일은 동기화하지 않았다.
- 최대 4초 방향 유지 시도는 시간 정지 상태에서 실제 시작 첫 후보 버튼 이벤트로 강철 검 ★1 하나를 장착했다. 정상 속도·자연 출현·가상 입력을 사용했고 무적·능력치·경험치·출현·시간 배율을 조작하지 않았으며 UI 선택은 자동화했다. 게임 시간과 실제 플레이 시간 모두 300.0초에 몸통 2칸·HP 30으로 생존하고 A/D·Space 입력 및 실제 회전·가속을 확인했다. 자연 적 2처치·보석 XP 2/10·레벨 1·보상 선택 0으로 자연 성장 조건에 실패해 Unity 종료 코드는 1이었다. 방향 고정 86회와 예측 위험 관련 해제 39회를 기록했다. Unity 편집기 SearchDatabase 시작 색인 예외가 1회 발생했다. 재현 코드는 `NaturalDirection` 하위 폴더에 있다.
- 강제 출현·처치와 통제 표적 시험은 자연 출현 순서, 적 충돌, 난이도 또는 5분 생존의 대체 근거가 아니다.
- 26일차 `11f4a25`의 추적 자산·설정 427개 해시가 일치하는 사본에서 시험 무기 6종의 공격 원점·직접 피해·공격 주기를 확인한 배치는 종료 코드 0이었다. 실제 세 번째 몸통을 머리에서 멀리 고정해 공격했고, 강철 창의 2명 관통, 철퇴의 3명과 화염 고리의 2명 범위 적중 및 화염 무기의 기본 화상도 확인했다. 실제 출현기 설정으로 일반 적을 통제 생성·강제 처치한 뒤 추적자·포식자·사수의 소형 보석 1/2/2개와 흡수 경험치 1/2/2를 확인했다. 자연 전투나 사람이 조작한 플레이 결과는 아니다.
- 강철 검 통제 배치는 27일차 `2fa479c` 기준에서 ★3의 첫 공격이 한 명만 적중해 종료 코드 1을 기록했다. 28일차 네 제품 파일 반영 후 동일 실제 씬 배치는 종료 코드 0이었다. ★1·2는 가까운 적 1명, ★3·4·5는 가까운 적 2명에게 각각 10·12.5·15·18·22 직접 피해를 주고 0.8초 공격 주기를 유지했다. 화염 검 ★3·5의 단일 공격, 첫 대상 사망 중 다른 대상 비활성·재등록 및 신규 대상 생성, 물리 시너지 ×2에서 두 대상에 각각 16.5 피해도 확인했다. 표적은 정지 상태이며 사망 콜백의 대상 변경은 시험 코드가 유발했다.
- 보석 병합 통제 배치는 28일차 `b573b98` 사본에서 관리자가 없어 종료 코드 1을 기록했다. 같은 스크립트 SHA-256 `1A222F01ABC1125176DA9480B1A327B25FFA030D485B57AB9215DE4FDFA96223`을 사용해 29일차 제품 파일과 씬 연결을 반영한 실제 게임 씬 배치는 종료 코드 0이었다. 1×5→5와 5×4→20의 경험치 보존, 최초 검사에서 화면 밖·원거리 우선, 병합 예산 1, 일시정지와 재개, 비활성화·재활성화와 외부 파괴 완료 후 제외, 실제 근거리 수집 경험치 5를 확인했다. 최초 검사의 인접 후보 검사 5회, 예산에 포함된 거리 계산 13회, 별도 머리 거리 계산 15회를 기록했다. 이 조합과 검사 설정은 기획 미정인 구현 가정이며 보석 값 1·5·20은 기획 기준이다. 보석 배치와 경험치 요구량은 시험에서 통제했으므로 자연 전투 결과가 아니다. 외부에서 임의로 `Destroy`한 같은 프레임의 후보 제외는 검증하거나 보장하지 않는다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 저예산 부분 그룹 보강 배치는 같은 29일차 동결 사본에서 Unity 종료 코드 0으로 통과했다. 실제 시작 버튼을 자동 선택하고 이동·자연 출현을 끄며 레벨업 요구량을 임시 10000으로 설정했다. 기준 1개·그룹 1개·검사 예산 17에서 실제 서로 다른 프레임에 부분 재료 3개가 보존된 뒤 새 보석 삽입, 선택 재료 이동 후 복귀, 선택 재료의 실제 수집 후 대체 보석 추가를 각각 검증했다. 각 검사와 지연 파괴 경계에서 수집 경험치와 미소비 보석값의 합이 보존됐고, 유효 그룹은 이후 병합됐다. 관찰된 재료 거리 계산 최대는 15회였고 별도로 계산되는 머리 거리 검사는 최대 25회였다. 따라서 전체 보석 순회와 정렬까지 17회로 제한한 결과는 아니다. Unity Editor Search 시작 인덱싱 예외가 별도 1회 발생했다.
- 충돌 시험의 실제 동시 접촉 순서는 로그에서 따로 확인한다. 인위적으로 호출한 양쪽 콜백 순서와 수명주기 초기화는 실제 물리 콜백이나 씬 전환을 재현한 결과와 구분한다.
- 합성 시험은 24일차 제품 코드에서 Unity 6000.3.21f1 배치 종료 코드 0으로 통과했다. 네 단계 ESC 취소, 20번 몸통 선택, 양쪽 강화 계승, 속성 수와 단계 즉시 갱신, 몸통 손실·레벨업·엘리트 보상 우선순위를 확인했다. 실제 시간 0.400초에 게임 시간 0.280초가 진행됐으며, 회전과 가속 입력 중 방향을 유지하고 직진했다. 이 결과는 통제된 장비와 가상 입력에 대한 검증이다.
- 같은 합성 배치에서 시너지 미리보기 추가 후 속성 무기 수 2~9의 단계 하락 경계와 유지 구간을 네 선택 화면에서 확인하고, 다른 속성으로 선택을 옮겼을 때의 표시, ESC 취소 시 보존, 4→3 합성 후 실제 단계와 예상의 일치도 확인했다. 대표 상태 문구의 필요 크기는 최대 462.46×15.59, 영역 크기는 620×32였으며 20번 몸통과 강화 선택 버튼 문구의 높이도 확인했다. Unity 배치 종료 코드는 0이다.
- 미장착 보상 합성의 같은 실제 씬 배치는 24일차 `8cafb51` 기준에서 해당 API가 없어 종료 코드 1을 기록했고, 25일차 API 반영 후 종료 코드 0으로 통과했다. 빈 슬롯과 만석, 두 강화 계승, 부적합 요청 시 무변경, 무기 수·시너지 유지, 기존 장착 무기 2→1 합성을 확인했다. 조작한 장비와 강화를 사용하는 낮은 수준 API 시험이며 보상 UI 검증은 포함하지 않는다.
- 레벨업 보상 합성의 26일차 초기 Unity 배치는 실제 2D 접촉 후 직접 주입한 초과 피해로 패배할 때 보상 패널이 남아 종료 코드 1을 기록했다. 별도 경계 시험에서는 숫자 2 중복 적용, 제어기 비활성 후 화면·후보 복구, 마지막 호환 몸통 손실 후 합성 버튼 상태를 각각 확인해 실패를 기록했다. 수정 후 `RunAll`은 가상 입력, 실제 버튼 이벤트, 통제된 충돌과 직접 치명 피해로 이 경계와 빈 슬롯·만석·연속 레벨업·엘리트 대기 흐름을 확인해 종료 코드 0으로 통과했다. 시험 사례 사이에 테스트 코드가 상태를 초기화하므로 자연 전투 결과와 구분한다.
- 시작 무기 선택의 비활성 `Configure` 수정 직전 실제 씬 배치는 새 플레이 세 번에서 임시 후보 강철 검·강철 창·화염 고리의 첫 몸통 장착, 버튼과 가상 숫자 입력, 선택 전 시간·이동·출현 정지 및 선택 후 재개를 확인해 종료 코드 0이었다. 이후 비활성 선택기의 `Configure` 화면 복구 문제를 수정하고 별도 실제 씬 배치에서 비활성 유지와 첫 후보 선택을 확인해 종료 코드 0이었다. 최종 수정본으로 세 후보와 공용 헬퍼가 적용된 기존 배치를 다시 실행하려 했으나 Unity 초기화가 정체해 중단했고, 독립 사본의 캐시 복사도 미완료이므로 최종 전체 회귀는 확인되지 않았다.
- HUD 시험의 텍스트 너비 비교는 배치 환경에서 측정 가능한 경우에만 유효하다. 실제 화면에서의 가독성은 별도로 확인한다.
- 실행 환경에 따라 Unity Editor의 SearchDatabase 시작 인덱싱 예외가 발생할 수 있다. 게임 스크립트 예외와 구분하고 로그에서 실제 발생 여부를 확인한다.
