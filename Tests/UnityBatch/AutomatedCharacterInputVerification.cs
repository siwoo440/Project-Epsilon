using System; // 예외 형식
using System.Reflection; // 실제 캐릭터 연결 조회
using System.Collections; // 코루틴 형식
using UnityEngine.InputSystem; // 실제 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 실제 키 상태
using UnityEngine.UI; // 실제 후보 버튼과 문구
using ProjectEpsilon.UI; // 실제 시작 화면
using ProjectEpsilon.Combat; // 실제 무기와 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Player; // 실제 플레이어 구성
using ProjectEpsilon.Progression; // 실제 시작 후보와 무기 풀
using ProjectEpsilon.World; // 실제 시작 맵 선택
using UnityEditor; // 유니티 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedCharacterInputVerification // 시작 화면 실제 캐릭터 선택 시험
    {
        private const string Key = "EpsilonCharacterInputVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedCharacterInputVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 플레이 감시 연결
            }
        }
        public static void Run() // 실제 씬 시험 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 시작 화면 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 플레이 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            {
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            }
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 시작 화면 단계
            if (frontend != null && frontend.Stage != FrontendStage.Weapons) // 실제 포탑 선택 전 단계 확인
            {
                StartingWeaponBatchHelper.HandleStartingSelection(); // 실제 메뉴 단계 진행
                return; // 실제 포탑 화면 대기
            }
            StartingWeaponSelection selection = UnityEngine.Object.FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 선택기
            if (injected || selection == null || !selection.IsSelecting || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 시작 상태와 실행 여부 확인
            {
                return; // 실제 시작 화면 대기
            }
            injected = true; // 실행 중복 방지
            selection.gameObject.AddComponent<CharacterInputVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[CharacterInputVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과 기록
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }
    public sealed class CharacterInputVerificationRunner : MonoBehaviour // 실제 시작 캐릭터 입력 실행기
    {
        private IEnumerator Start() // 실제 시작 화면 입력 검증
        {
            InputSettings originalSettings = InputSystem.settings; // 이전 입력 설정
            InputSettings testSettings = UnityEngine.Object.Instantiate(originalSettings); // 메모리 입력 설정 사본
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 실제 설정 파일 보존
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 키 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 실제 시험 입력 설정 적용
            bool originalBackground = Application.runInBackground; // 이전 백그라운드 실행 설정
            Application.runInBackground = true; // 실제 배치 입력 처리 허용
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); // 실제 가상 키보드 생성
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 체력
            StartingWeaponSelection selection = GetComponent<StartingWeaponSelection>(); // 실제 시작 선택기
            LevelUpPanelController panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 시작 화면
            Require(body != null && health != null && selection != null && panel != null, "Character input fixtures missing."); // 실제 시작 구성 확인
            SettingsMenuController settings = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>(); // 실제 설정 화면
            Button closeSettings = settings == null ? null : (Button)typeof(SettingsMenuController).GetField("closeButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settings); // 실제 설정 닫기 버튼
            Require(settings != null && closeSettings != null, "Settings controls missing."); // 실제 설정 화면과 닫기 버튼 확인
            settings.OpenSettings(); // 실제 설정 화면 열기
            PrototypeMapSelection maps = UnityEngine.Object.FindFirstObjectByType<PrototypeMapSelection>(); // 실제 맵 선택기
            Require(maps != null && maps.ProfileCount == 3 && maps.SelectedIndex == 0, "Default map selection missing."); // 실제 기본 맵 확인
            yield return Tap(keyboard, Key.DownArrow); // 설정 중 실제 맵 변경 입력
            Require(maps.SelectedIndex == 0, "Map input leaked through settings."); // 설정 화면 입력 우선 확인
            yield return Tap(keyboard, Key.RightArrow); // 설정 화면 위 캐릭터 변경 입력
            Require(settings.IsOpen && CharacterRunInitializer.ActiveCharacter.Id == "balanced", "Character input leaked through settings."); // 설정 우선 입력 차단 확인
            closeSettings.onClick.Invoke(); // 실제 설정 화면 닫기
            yield return Tap(keyboard, Key.DownArrow); // 실제 다음 맵 키 입력
            Text guide = (Text)typeof(LevelUpPanelController).GetField("growthText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 시작 안내 문구
            Require(maps.SelectedIndex == 1 && guide != null && guide.text.Contains("폐허 회랑"), "Map arrow input or visible selection failed."); // 실제 맵 변경과 안내 확인
            yield return Tap(keyboard, Key.UpArrow); // 실제 이전 맵 키 입력
            Require(maps.SelectedIndex == 0, "Map arrow did not return to plains."); // 실제 첫 맵 복귀 확인
            yield return Tap(keyboard, Key.UpArrow); // 실제 맵 역순 순환 입력
            Require(maps.SelectedIndex == 2, "Map selection did not wrap backwards."); // 실제 세 번째 맵 순환 확인
            yield return Tap(keyboard, Key.DownArrow); // 실제 기본 맵 복귀 입력
            yield return Tap(keyboard, Key.RightArrow); // 실제 다음 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter != null && CharacterRunInitializer.ActiveCharacter.Id == "mobile" && health.MaximumHealth == 85 && body.CurrentBodyCount == 3, "Right arrow did not select mobile character."); // 실제 기동형 선택과 능력치 확인
            Text label = (Text)typeof(LevelUpPanelController).GetField("levelText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 시작 화면 캐릭터 문구
            Require(label != null && label.text.Contains("기동형"), "Selected character name is not shown."); // 실제 선택 캐릭터 문구 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 다음 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "heavy" && health.MaximumHealth == 125 && body.CurrentBodyCount == 4, "Right arrow did not select heavy character."); // 실제 중장형 선택과 능력치 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 다음 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "fusion" && health.MaximumHealth == 95, "Fusion character was not reachable."); // 실제 합성형 선택 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 다음 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "holy" && health.MaximumHealth == 100, "Holy character was not reachable."); // 실제 신성형 선택 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 다음 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "dark" && health.MaximumHealth == 90, "Dark character was not reachable."); // 실제 암흑형 선택 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 목록 끝 순환 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "balanced" && health.MaximumHealth == 100, "Character selection did not wrap to balanced."); // 실제 목록 순환 확인
            yield return Tap(keyboard, Key.RightArrow); // 실제 기동형 이동 입력
            yield return Tap(keyboard, Key.RightArrow); // 실제 중장형 이동 입력
            yield return Tap(keyboard, Key.LeftArrow); // 실제 이전 캐릭터 키 입력
            Require(CharacterRunInitializer.ActiveCharacter.Id == "mobile" && health.MaximumHealth == 85 && body.CurrentBodyCount == 3 && selection.IsSelecting, "Left arrow did not restore mobile character."); // 실제 기동형 복귀와 후보 유지 확인
            WeaponData chosen = CharacterRunInitializer.ActiveCharacter.StartingWeapons.Weapons[0]; // 선택 캐릭터 첫 시작 무기
            Button button = (Button)typeof(LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 첫 무기 버튼
            Require(button != null && button.interactable, "Character starting weapon button missing."); // 실제 시작 무기 선택 확인
            button.onClick.Invoke(); // 실제 시작 무기 버튼 선택
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            Require(GameManager.Instance.IsPlaying && weapons.Slots[0].Weapon == chosen && health.MaximumHealth == 85, "Character-specific weapon did not start the run."); // 실제 캐릭터별 무기와 게임 시작 확인
            Require(!maps.SelectMap(1) && maps.SelectedIndex == 0, "Map changed after the run started."); // 전투 중 맵 변경 차단 확인
            InputSystem.RemoveDevice(keyboard); // 실제 가상 키보드 제거
            if (originalSettings != null) // 이전 입력 설정 유효성 확인
            {
                InputSystem.settings = originalSettings; // 이전 입력 설정 복구
            }
            Application.runInBackground = originalBackground; // 이전 백그라운드 설정 복구
            if (testSettings != null && InputSystem.settings != testSettings) // 임시 설정 사용 여부 확인
            {
                Destroy(testSettings); // 사용하지 않는 임시 입력 설정 제거
            }
            AutomatedCharacterInputVerification.Finish(0, "Actual starting-screen arrow selection and character-specific weapon start."); // 실제 전체 성공 기록
        }
        private static IEnumerator Tap(Keyboard keyboard, Key key) // 실제 키 누름과 해제
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 가상 키 누름 전달
            yield return null; // 실제 입력 프레임 대기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 가상 키 해제 전달
            yield return null; // 실제 입력 해제 대기
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedCharacterInputVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
