---

# Unity 비대화형 통합 검증

이 폴더에는 실제로 실행한 Unity 에디터 배치 검증 스크립트를 보관한다. 제품 씬이나 런타임 스크립트가 아니며, 반드시 **별도 프로젝트 사본**에서 실행한다. 테스트는 끝나면 편집기 프로세스를 종료한다.

---

## 준비와 실행

1. `Assets`, `Packages`, `ProjectSettings`를 포함한 Unity 프로젝트 사본을 별도 폴더에 준비한다. 원본 프로젝트나 다른 작업 중인 프로젝트를 사용하지 않는다.
2. 이 폴더의 검증 스크립트를 사본의 `Assets/Editor`에 복사한다. 원본 프로젝트의 `Assets`에는 복사하지 않는다.
3. 프로젝트 버전인 Unity **6000.3.21f1**의 `Unity.exe`를 사용한다. `-batchmode`, `-nographics`, 사본을 가리키는 `-projectPath`, 아래 표의 `-executeMethod`, 별도 `-logFile`을 지정한다. `-quit`은 플레이 모드 진입 전에 종료될 수 있으므로 사용하지 않는다.
4. 프로세스가 종료된 뒤 로그의 테스트 접두사와 종료 코드를 확인한다. Unity 초기 에셋 가져오기와 컴파일 시간은 플레이 시간과 별개다.

| 스크립트 | 실행할 메서드 | 실제로 실행한 조건 |
| --- | --- | --- |
| `AutomatedPilotVerification.cs` | `ProjectEpsilon.Editor.AutomatedPilotVerification.Run` | 정상 속도 300초, 가상 A/D·Space 회피 입력, 레벨업·보상 UI 자동 선택. 무적·능력치 변경 없음. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedEliteRewardVerification.cs` | `ProjectEpsilon.Editor.AutomatedEliteRewardVerification.Run` | 출현기 시간을 240초로 설정하고 엘리트를 강제 처치. XP 보석과 보상 일시정지, 실제 UI 버튼 이벤트, 장착·재개 확인 |
| `AutomatedBodyHudVerification.cs` | `ProjectEpsilon.Editor.AutomatedBodyHudVerification.Run` | 초기 물리 단계 뒤 게임을 일시정지하고 성장·손실·복구 API를 직접 호출. 세 수치, 이벤트, HUD 텍스트와 너비 확인 |
| `AutomatedCombatProgressionVerification.cs` | `ProjectEpsilon.Editor.AutomatedCombatProgressionVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체 없는 정지 표적을 배치. 실제 몸통 무기 공격, 보석, 레벨업과 UI 버튼 이벤트 확인 |
| `AutomatedSettingsPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedSettingsPriorityVerification.Run` | XP 주입과 만석 슬롯을 강제하고 가상 ESC·숫자 입력 및 실제 UI 버튼 이벤트 사용. 배치 포커스를 위해 입력 설정의 저장되지 않는 메모리 사본 사용 |
| `AutomatedCollisionPriorityVerification.cs` | `ProjectEpsilon.Editor.AutomatedCollisionPriorityVerification.Run` | 플레이어 이동과 자연 스폰을 끄고 충돌체를 제어. 실제 2D 물리 접촉과 별도의 인위 콜백 순서·수명주기 시험 사용 |
| `AutomatedNaturalCombatVerification.cs` | `ProjectEpsilon.Editor.AutomatedNaturalCombatVerification.Run` | 정상 속도, 자연 적 출현·무기 공격·보석 획득과 가상 A/D·Space 조종. 능력치와 경험치 주입 없음, 선택 화면은 자동 선택 |

예를 들어 자동 조종 검증은 다음과 같이 실행한다. 경로는 설치된 Unity와 별도 프로젝트 사본에 맞게 지정한다.

```powershell
& "C:/Program Files/Unity/Hub/Editor/6000.3.21f1/Editor/Unity.exe" -batchmode -nographics -projectPath "<검증 사본 경로>" -executeMethod ProjectEpsilon.Editor.AutomatedPilotVerification.Run -logFile "<검증 로그 경로>" # 자동 생존 검증
```

---

## 결과 해석

- 자동 조종은 사람의 수동 플레이가 아니다. 300초 생존 여부와 경험치·레벨업 발생 여부를 따로 확인한다.
- 자연 전투 최초 시도는 조종 입력 확인을 위해 초기에 A·D·Space를 순서대로 입력했다. Unity 6000.3.21f1, 정상 속도, 무적·능력치·경험치·적 출현 조작 없이 6처치와 보석 XP 6/10을 얻었고, 게임 시간 57.2초에 레벨 1로 패배해 종료 코드 1이었다. 가상 입력 및 실제 회전·가속은 확인됐으나 자연 레벨업은 확인되지 않았다. 보관한 자연 전투 스크립트는 이 시도에 해당한다.
- 자연 전투 두 번째 시도는 초기 고정 입력을 제거하고 머리와 몸통의 예상 접촉 위험을 회피하는 시험 조종기를 사용했다. 같은 게임 조건에서 1처치와 보석 XP 1/10을 얻었고, 게임 시간 12.6초에 레벨 1로 패배해 종료 코드 1이었다. 두 시도 모두 자동 조종 결과이며 게임의 난이도 문제를 단독으로 입증하지 않는다.
- 강제 출현·처치와 통제 표적 시험은 자연 출현 순서, 적 충돌, 난이도 또는 5분 생존의 대체 근거가 아니다.
- 충돌 시험의 실제 동시 접촉 순서는 로그에서 따로 확인한다. 인위적으로 호출한 양쪽 콜백 순서와 수명주기 초기화는 실제 물리 콜백이나 씬 전환을 재현한 결과와 구분한다.
- HUD 시험의 텍스트 너비 비교는 배치 환경에서 측정 가능한 경우에만 유효하다. 실제 화면에서의 가독성은 별도로 확인한다.
- 실행 환경에 따라 Unity Editor의 SearchDatabase 시작 인덱싱 예외가 발생할 수 있다. 게임 스크립트 예외와 구분하고 로그에서 실제 발생 여부를 확인한다.
