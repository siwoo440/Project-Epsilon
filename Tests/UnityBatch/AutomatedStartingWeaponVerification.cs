using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 선택기와 UI 조회
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Core; // 시작 게임 상태
using ProjectEpsilon.Data; // 시작 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현
using ProjectEpsilon.Player; // 몸통과 이동
using ProjectEpsilon.Progression; // 경험치 상태
using ProjectEpsilon.UI; // 실제 시작 화면
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 실제 UI 버튼

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedStartingWeaponVerification // 실제 시작 무기 선택 검증
    {
        private const string ActiveKey = "EpsilonStartingWeaponVerification"; // 배치 진행 표식
        private const string IndexKey = "EpsilonStartingWeaponOptionIndex"; // 현재 선택 후보
        private const string ReloadKey = "EpsilonStartingWeaponReloadPending"; // 플레이 재진입 대기
        private const string DisabledKey = "EpsilonStartingWeaponDisabledBeforeStart"; // 시작 전 비활성 시험
        private const string ReenabledKey = "EpsilonStartingWeaponReenabled"; // 시작 선택기 복구 상태
        private static double started; // 개별 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedStartingWeaponVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 여부
            {
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 시작 감시 연결
            }
        }

        public static void Run() // Unity 배치 진입점
        {
            RunScenario(false); // 기본 세 후보 시험
        }

        public static void RunDisabledBeforeStart() // 시작 전 선택기 비활성 시험
        {
            RunScenario(true); // 비활성 및 재활성 경계 시험
        }

        private static void RunScenario(bool disableBeforeStart) // 시험 종류와 플레이 시작
        {
            SessionState.SetBool(ActiveKey, true); // 시험 진행 상태 저장
            SessionState.SetInt(IndexKey, 0); // 첫 후보 선택부터 시작
            SessionState.SetBool(ReloadKey, false); // 재진입 상태 초기화
            SessionState.SetBool(DisabledKey, disableBeforeStart); // 시작 전 비활성 시험 저장
            SessionState.SetBool(ReenabledKey, false); // 재활성 상태 초기화
            EditorApplication.update -= Tick; // 중복 연결 제거
            EditorApplication.update += Tick; // 시작 감시 연결
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            if (disableBeforeStart) // 시작 전 비활성 시험 확인
            {
                Type selector = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.StartingWeaponSelection"); // 실제 선택기 형식 조회
                SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 씬 플레이어 조회
                MonoBehaviour component = selector == null || player == null ? null : player.GetComponent(selector) as MonoBehaviour; // 실제 선택기 조회
                if (component == null) // 선택기 연결 확인
                {
                    Finish(1, "Starting selector is absent before the disabled-start test."); // 구성 오류 기록
                    return; // 플레이 생략
                }
                component.enabled = false; // 저장하지 않은 씬 메모리에서 시작 전 비활성화
            }
            EditorApplication.isPlaying = true; // 첫 플레이 모드 진입
        }

        private static void Tick() // 플레이 재진입과 제한 시간 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 정지 상태 확인
            {
                if (SessionState.GetBool(ReloadKey, false) && !EditorApplication.isPlayingOrWillChangePlaymode) // 재진입 가능 여부
                {
                    SessionState.SetBool(ReloadKey, false); // 재진입 대기 해제
                    EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 다시 열기
                    EditorApplication.isPlaying = true; // 다음 후보 플레이 진입
                }
                return; // 플레이 진입 대기
            }
            if (started <= 0d) // 개별 실행 최초 시각
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 개별 실행 제한 확인
            {
                Finish(1, "Timed out waiting for start selection."); // 시간 초과 보고
                return; // 감시 중단
            }
            if (injected) // 실행기 생성 여부
            {
                return; // 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            if (player == null || game == null) // 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            Type selectionType = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.StartingWeaponSelection"); // 제품 시작 선택기 형식 조회
            if (selectionType == null && game.IsPlaying) // 기존 자동 시작 여부
            {
                Finish(1, "Starting weapon selection is absent and gameplay started automatically."); // 기준 버전 실패 기록
                return; // 실행기 생략
            }
            if (selectionType == null) // 선택기 형식 부재 확인
            {
                return; // 씬 초기화 대기
            }
            MonoBehaviour selection = player.GetComponent(selectionType) as MonoBehaviour; // 실제 선택기 조회
            PropertyInfo selecting = selectionType.GetProperty("IsSelecting"); // 선택 진행 상태 조회
            if (SessionState.GetBool(DisabledKey, false) && !SessionState.GetBool(ReenabledKey, false) && selection != null) // 시작 전 비활성 경계 확인
            {
                if (EditorApplication.timeSinceStartup - started < 0.25d) // 실제 시작 시점 대기
                {
                    return; // 게임 시작 처리 대기
                }
                if (game.CurrentState != GameState.Starting || !Mathf.Approximately(Time.timeScale, 0f)) // 비활성 중 자동 시작 확인
                {
                    Finish(1, $"Disabled selector allowed gameplay before choice: state={game.CurrentState}, scale={Time.timeScale:F2}."); // 시작 우회 실패 기록
                    return; // 후속 시험 중단
                }
                selection.enabled = true; // 실제 선택기 재활성화
                SessionState.SetBool(ReenabledKey, true); // 재활성 상태 기록
                Debug.Log("[StartingWeaponVerification] Disabled-before-Start remained frozen; selector re-enabled."); // 실제 경계 결과 기록
            }
            if (selection == null || selecting == null || !(bool)selecting.GetValue(selection)) // 실제 후보 화면 준비 확인
            {
                return; // 선택 표시 대기
            }
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<StartingWeaponVerificationRunner>(); // 실제 씬 시험 실행
        }

        public static int OptionIndex => SessionState.GetInt(IndexKey, 0); // 현재 선택 후보 조회

        public static void CompleteOption() // 현재 후보 시험 완료
        {
            int next = OptionIndex + 1; // 다음 후보 번호
            if (SessionState.GetBool(DisabledKey, false)) // 비활성 경계 시험 완료
            {
                Finish(0, "Disabled selector stayed frozen, reactivated and selected the first weapon in one fresh Game scene."); // 한 후보 경계 성공 기록
                return; // 추가 플레이 생략
            }
            if (next >= 3) // 세 후보 시험 완료
            {
                Finish(0, "Three fresh Game scene starts, actual buttons and virtual numeric key, blocked settings, lifecycle and first body weapon."); // 전체 성공 기록
                return; // 추가 플레이 생략
            }
            SessionState.SetInt(IndexKey, next); // 다음 후보 저장
            SessionState.SetBool(ReloadKey, true); // 플레이 재진입 예약
            injected = false; // 다음 실행기 허용
            started = 0d; // 다음 시작 시각 초기화
            EditorApplication.isPlaying = false; // 현재 플레이 종료
        }

        public static void Finish(int code, string message) // 결과 기록과 배치 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            SessionState.SetBool(ReloadKey, false); // 재진입 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[StartingWeaponVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과
            EditorApplication.Exit(code); // 프로세스 종료 코드
        }
    }

    public sealed class StartingWeaponVerificationRunner : MonoBehaviour // 실제 시작 선택 실행기
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance; // 내부 필드 검색
        private MonoBehaviour selection; // 실제 시작 선택기
        private SnakeMovement movement; // 실제 플레이어 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeHealth health; // 실제 공용 체력
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private SnakeExperience experience; // 실제 경험치 관리자
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private LevelUpPanelController panel; // 실제 공용 선택 UI
        private SettingsMenuController settings; // 실제 설정 화면
        private WeaponData[] candidates; // 실제 후보 데이터
        private Keyboard keyboard; // 시험용 가상 키보드
        private InputSettings originalSettings; // 기존 입력 설정
        private InputSettings testSettings; // 메모리 입력 설정
        private bool originalBackground; // 기존 백그라운드 상태

        private IEnumerator Start() // 한 후보의 실제 시작 시험
        {
            Prepare(); // 실제 씬과 가상 입력 준비
            int option = AutomatedStartingWeaponVerification.OptionIndex; // 이번 실행의 후보 번호
            Vector3 initialPosition = movement.transform.position; // 선택 전 머리 위치
            Quaternion initialRotation = movement.transform.rotation; // 선택 전 머리 방향
            float initialGameTime = Time.time; // 선택 전 게임 시간
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A, Key.Space)); // 회전과 가속 입력 유지
            yield return new WaitForSecondsRealtime(0.25f); // 선택 전 실제 시간 경과
            Require(GameManager.Instance.CurrentState == GameState.Starting && Mathf.Approximately(Time.timeScale, 0f) && Mathf.Approximately(Time.time, initialGameTime) && Vector3.Distance(movement.transform.position, initialPosition) < 0.001f && Quaternion.Angle(movement.transform.rotation, initialRotation) < 0.01f, "Time or movement advanced before selection."); // 시작 전 정지 확인
            Require(spawner.ActiveEnemies == 0 && Mathf.Approximately((float)typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateInstance).GetValue(spawner), 0f), "Enemies or wave time advanced before selection."); // 시작 전 출현 차단
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 회전과 가속 해제
            yield return null; // 입력 해제 대기
            if (option == 0) // 첫 실행의 추가 경계
            {
                ClickSettings("openButton"); // 실제 설정 열기 버튼
                Require(settings.IsOpen, "Settings could not open during start selection."); // 설정 표시 확인
                ClickCandidate(2); // 설정 뒤 실제 후보 버튼 이벤트
                yield return KeyTap(Key.Digit3); // 설정 위 가상 숫자 입력
                Require(GameManager.Instance.CurrentState == GameState.Starting && IsSelecting() && settings.IsOpen, "Starting selection leaked through settings."); // 설정 우선 확인
                ClickSettings("closeButton"); // 실제 설정 닫기 버튼
                Require(!settings.IsOpen && IsSelecting() && GameManager.Instance.CurrentState == GameState.Starting, "Closing settings changed start selection."); // 시작 선택 보존
                selection.enabled = false; // 선택기만 비활성화
                yield return null; // 실제 비활성 처리
                Require(!panel.gameObject.activeInHierarchy && GameManager.Instance.CurrentState == GameState.Starting, "Disabled starting selector left its panel or started gameplay."); // 비활성 상태 확인
                UnityEngine.Object options = selection.GetType().GetField("options", PrivateInstance).GetValue(selection) as UnityEngine.Object; // 기존 후보 데이터 조회
                MethodInfo configure = selection.GetType().GetMethod("Configure"); // 실제 재구성 메서드 조회
                configure.Invoke(selection, new object[] { weapons, panel, options }); // 비활성 상태에서 같은 구성 적용
                Debug.Log($"[StartingWeaponVerification] Disabled Configure: enabled={selection.enabled}, panel={panel.gameObject.activeInHierarchy}, state={GameManager.Instance.CurrentState}."); // 실제 비활성 재구성 상태
                Require(!selection.enabled && !panel.gameObject.activeInHierarchy && GameManager.Instance.CurrentState == GameState.Starting, "Configuring a disabled starting selector reopened its panel."); // 비활성 설정 시 화면 유지 확인
                selection.enabled = true; // 선택기 재활성화
                yield return null; // 실제 화면 복구 대기
                Require(IsSelecting() && panel.gameObject.activeInHierarchy && GameManager.Instance.CurrentState == GameState.Starting, "Starting selector did not restore its panel."); // 선택 화면 복구 확인
                yield return KeyTap(Key.Escape); // 실제 ESC 입력
                yield return KeyTap(Key.E); // 실제 일반 합성 키 입력
                Require(IsSelecting() && GameManager.Instance.CurrentState == GameState.Starting, "ESC or E started or paused the run before selection."); // 시작 전 입력 우선
            }
            if (option == 1) // 둘째 후보의 키보드 시험
            {
                yield return KeyTap(Key.Digit2); // 실제 숫자 2 선택
            }
            else // 첫째와 셋째 후보의 버튼 시험
            {
                ClickCandidate(option); // 실제 UI 버튼 이벤트
            }
            Require(GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f) && !IsSelecting() && !panel.gameObject.activeInHierarchy, "Starting selection did not start and close correctly: " + option); // 실제 게임 시작 확인
            Require(body.CurrentBodyCount == 3 && body.EarnedBodyCount == 3 && health.CurrentHealth == 100 && weapons.SlotCount == 3 && weapons.OccupiedSlotCount == 1, "Unexpected initial body, health, or equipped count."); // 시작 생존 상태 확인
            Require(weapons.Slots[0].Weapon == candidates[option] && weapons.Slots[0].Grade == 1 && weapons.Slots[0].Origin == body.BodySegments[0].transform && weapons.Slots[1].IsEmpty && weapons.Slots[2].IsEmpty, "Selected weapon was not equipped on first body: " + option); // 실제 첫 몸통 장착 확인
            WeaponData selected = weapons.Slots[0].Weapon; // 확정된 무기 기록
            Button hiddenButton = CandidateButton((option + 1) % 3); // 숨긴 다른 후보 버튼
            hiddenButton.onClick.Invoke(); // 종료 후 중복 버튼 이벤트
            Require(weapons.Slots[0].Weapon == selected && weapons.OccupiedSlotCount == 1, "A duplicate button changed the selected weapon."); // 중복 확정 방지
            Vector3 playingStart = movement.transform.position; // 게임 시작 직후 위치
            yield return new WaitForSecondsRealtime(0.25f); // 실제 전투 시간 진행
            Require(GameManager.Instance.IsPlaying && Vector3.Distance(movement.transform.position, playingStart) > 0.05f && (float)typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateInstance).GetValue(spawner) > 0f, "Movement or wave timer did not resume after choosing."); // 선택 후 자동 전진과 시간 확인
            Debug.Log($"[StartingWeaponVerification] option={option + 1} weapon={selected.DisplayName} grade={weapons.Slots[0].Grade} preStartFrozen=true settingsBlocked={(option == 0)} input={(option == 1 ? "digit2" : "button")} bodyOrigin=true resumed=true."); // 실제 후보별 결과
            Cleanup(); // 시험 입력 설정 복구
            AutomatedStartingWeaponVerification.CompleteOption(); // 다음 새 플레이 또는 종료
        }

        private void Prepare() // 실제 씬 구성과 가상 입력 준비
        {
            movement = GetComponent<SnakeMovement>(); // 플레이어 이동 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            health = GetComponent<SnakeHealth>(); // 공용 체력 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 경험치 관리자 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 적 출현기 조회
            panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 공용 선택 화면 조회
            settings = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>(); // 실제 설정 화면 조회
            Type selectorType = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.StartingWeaponSelection"); // 시작 선택기 형식 조회
            selection = selectorType == null ? null : GetComponent(selectorType) as MonoBehaviour; // 실제 선택기 조회
            Require(movement != null && body != null && health != null && weapons != null && experience != null && spawner != null && panel != null && settings != null && selection != null, "Required Game scene starting components are missing."); // 실제 씬 구성 확인
            UnityEngine.Object options = selection.GetType().GetField("options", PrivateInstance).GetValue(selection) as UnityEngine.Object; // 실제 시작 후보 에셋 조회
            Require(options != null, "Starting weapon options asset is missing."); // 후보 에셋 연결 확인
            candidates = (WeaponData[])options.GetType().GetProperty("Weapons").GetValue(options); // 실제 후보 무기 목록
            string[] names = { "DebugBlade", "DebugNeedle", "DebugPulse" }; // 기획상 미확정 임시 후보
            Require(candidates != null && candidates.Length == 3 && panel.gameObject.activeInHierarchy && IsSelecting(), "Starting candidate panel is unavailable."); // 시작 후보 수와 표시 확인
            for (int index = 0; index < candidates.Length; index++) // 실제 세 후보 순회
            {
                WeaponData expected = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/" + names[index] + ".asset"); // 임시 후보 실제 에셋
                Require(expected != null && candidates[index] == expected && CandidateButton(index).interactable && CandidateButton(index).gameObject.activeInHierarchy && CandidateLabel(index).Contains(expected.DisplayName), "Starting option asset or label mismatch: " + index); // 후보 순서 및 실제 UI 확인
                for (int previous = 0; previous < index; previous++) // 이전 후보 순회
                {
                    Require(candidates[previous] != candidates[index], "Starting weapon candidates are duplicated."); // 중복 후보 방지 확인
                }
            }
            Require(experience.CurrentLevel == 1 && experience.CurrentExperience == 0, "Starting experience is not level 1 and zero XP."); // 시작 경험치 확인
            originalSettings = InputSystem.settings; // 기존 입력 설정 저장
            testSettings = UnityEngine.Object.Instantiate(originalSettings); // 메모리 입력 설정 복사
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 설정 파일 저장 방지
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 시험 입력 설정 적용
            originalBackground = Application.runInBackground; // 기존 백그라운드 상태 저장
            Application.runInBackground = true; // 배치 입력 처리 허용
            keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
            Debug.Log("[StartingWeaponVerification] Actual fresh Game scene; starting options are temporary; virtual keyboard and real UI events; memory-only input settings."); // 시험 조건 기록
        }

        private bool IsSelecting() // 실제 시작 선택 상태 조회
        {
            return (bool)selection.GetType().GetProperty("IsSelecting").GetValue(selection); // 실제 선택 상태 반환
        }

        private Button CandidateButton(int index) // 실제 후보 버튼 조회
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 직렬화 버튼 필드
            Button button = (Button)typeof(LevelUpPanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 실제 버튼 조회
            Require(button != null, "Starting candidate button missing: " + index); // 버튼 연결 확인
            return button; // 실제 버튼 반환
        }

        private string CandidateLabel(int index) // 실제 후보 문구 조회
        {
            string[] names = { "candidateLabel01", "candidateLabel02", "candidateLabel03" }; // 직렬화 문구 필드
            Text label = (Text)typeof(LevelUpPanelController).GetField(names[index], PrivateInstance).GetValue(panel); // 실제 문구 조회
            Require(label != null, "Starting candidate label missing: " + index); // 문구 연결 확인
            return label.text; // 표시 문구 반환
        }

        private void ClickCandidate(int index) // 실제 후보 버튼 이벤트
        {
            Button button = CandidateButton(index); // 실제 버튼 조회
            Require(button.interactable && button.gameObject.activeInHierarchy, "Starting candidate button unavailable: " + index); // 선택 가능 여부 확인
            button.onClick.Invoke(); // 실제 버튼 이벤트 실행
        }

        private void ClickSettings(string field) // 실제 설정 버튼 이벤트
        {
            Button button = (Button)typeof(SettingsMenuController).GetField(field, PrivateInstance).GetValue(settings); // 직렬화 설정 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Settings button unavailable: " + field); // 설정 버튼 확인
            button.onClick.Invoke(); // 실제 설정 버튼 이벤트
        }

        private IEnumerator KeyTap(Key key) // 가상 키 누름과 해제
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 키 누름 전달
            yield return null; // 첫 입력 프레임
            yield return null; // 실제 입력 처리
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 키 해제 전달
            yield return null; // 키 해제 프레임
            yield return null; // 후속 처리 대기
        }

        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedStartingWeaponVerification.Finish(1, message); // 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }

        private void Cleanup() // 가상 입력과 메모리 설정 정리
        {
            InputSystem.RemoveDevice(keyboard); // 가상 키보드 제거
            if (originalSettings != null) // 기존 설정 존재 확인
            {
                InputSystem.settings = originalSettings; // 기존 입력 설정 복원
            }
            Application.runInBackground = originalBackground; // 원래 백그라운드 복원
            if (testSettings != null && InputSystem.settings != testSettings) // 설정 사본 사용 여부 확인
            {
                UnityEngine.Object.Destroy(testSettings); // 사용하지 않는 설정 사본 제거
            }
        }
    }
}
