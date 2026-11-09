using System; // 실제 예외 형식
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 씬 연결 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Enemies; // 실제 출현 시간
using ProjectEpsilon.Player; // 실제 캐릭터 선택
using ProjectEpsilon.Progression; // 실제 진행과 시작 포탑
using ProjectEpsilon.UI; // 실제 화면 흐름
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEditor; // 실제 Unity 배치
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // 실제 Unity 객체
using UnityEngine.InputSystem; // 실제 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 실제 키 상태
using UnityEngine.UI; // 실제 버튼과 문구

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 실제 도메인 재시작
    public static class AutomatedDay49FrontendVerification // 실제 화면 흐름 검증
    { // 클래스 시작
        private const string Key = "EpsilonDay49Frontend"; // 실제 시험 표식
        private static double started; // 실제 시험 시작 시간
        private static bool injected; // 실제 시험기 생성 여부
        static AutomatedDay49FrontendVerification() // 실제 도메인 재시작
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 실제 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시험 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시험 감시
            EditorApplication.isPlaying = true; // 실제 플레이 진입
        } // 메서드 끝
        private static void Tick() // 실제 시작 화면 확인
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 시작 대기
            { // 조건 시작
                return; // 실제 준비 대기
            } // 조건 끝
            if (started <= 0d) // 실제 시작 시간 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 실제 시간 제한 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 실제 시간 초과
                return; // 추가 시험 중단
            } // 조건 끝
            if (injected || GameManager.Instance == null || RunFrontendController.Instance == null || GameObject.Find("FrontendStartButton") == null) // 실제 시작 화면 준비 확인
            { // 조건 시작
                return; // 실제 화면 구성 대기
            } // 조건 끝
            injected = true; // 실제 시험 중복 방지
            GameObject runner = new GameObject("Day49FrontendRunner"); // 실제 지속 시험기
            UnityEngine.Object.DontDestroyOnLoad(runner); // 실제 재시작 중 시험 유지
            runner.AddComponent<Day49FrontendRunner>(); // 실제 화면 시나리오 시작
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 처리
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 시험 감시 해제
            Debug.Log($"[Day49FrontendVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day49FrontendRunner : MonoBehaviour // 실제 버튼과 재시작 시나리오
    { // 클래스 시작
        private IEnumerator Start() // 실제 씬 시나리오 실행
        { // 코루틴 시작
            Require(Application.productName == "Project-Epsilon-Day48-Isolated", "Test project is not isolated."); // 실제 사용자 저장 격리 확인
            PlayerPrefs.DeleteKey("ProjectEpsilon.MetaProgress.v1"); // 실제 격리 메타 초기화
            PlayerPrefs.SetString(FrontendOptions.SaveKey, "{}"); // 실제 손상 화면 설정 재현
            FrontendOptions corrupt = new FrontendOptions(); // 실제 손상 설정 읽기
            Require(!corrupt.IsAvailable && !corrupt.Change(10, 0, 0, false) && PlayerPrefs.GetString(FrontendOptions.SaveKey) == "{}", "Corrupt settings were overwritten."); // 실제 손상 원본 보존 확인
            PlayerPrefs.DeleteKey(FrontendOptions.SaveKey); // 실제 격리 화면 설정 초기화
            PlayerPrefs.Save(); // 실제 격리 저장 반영
            AudioListener.volume = 1f; // 실제 격리 기본 음량 복구
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 진행 관리자
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, new MetaProgressionService(new PlayerPrefsMetaStorage("ProjectEpsilon.MetaProgress.v1"))); // 실제 격리 저장 재연결
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 전용 화면
            typeof(RunFrontendController).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(frontend, new FrontendOptions()); // 실제 격리 설정 재연결
            CharacterRunInitializer characters = FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 선택기
            PrototypeMapSelection maps = FindFirstObjectByType<PrototypeMapSelection>(); // 실제 맵 선택기
            StartingWeaponSelection selection = FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 포탑
            LevelUpPanelController panel = FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 기존 포탑 화면
            Require(frontend.Stage == FrontendStage.Main && characters != null && characters.CharacterCount == 6 && maps != null && maps.ProfileCount == 3 && selection != null && panel != null, "Dedicated starting fixtures missing."); // 실제 시작과 데이터 확인
            GameObject background = GameObject.Find("FrontendBackground"); // 실제 메뉴 배경 조회
            Require(background != null && background.GetComponent<RawImage>() != null && background.GetComponent<RawImage>().texture != null && !background.GetComponent<RawImage>().raycastTarget, "Generated menu background missing or blocks input."); // 실제 이미지와 입력 확인
            Button weaponButton = (Button)typeof(LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 첫 포탑 버튼
            weaponButton.onClick.Invoke(); // 실제 이전 화면의 포탑 클릭 시도
            Require(GameManager.Instance.CurrentState == GameState.Starting, "Weapon selection leaked through main menu."); // 실제 단계 입력 차단 확인
            bool originalFullscreen = new FrontendOptions().Fullscreen; // 실제 최초 화면 모드
            Click("FrontendSettingsButton"); // 실제 주 메뉴 설정 진입
            Click("FrontendVolumeButton"); // 실제 음량 변경
            Click("FrontendShakeButton"); // 실제 흔들림 설정 변경
            Click("FrontendResolutionButton"); // 실제 해상도 변경
            Click("FrontendScreenModeButton"); // 실제 화면 모드 변경
            FrontendOptions loaded = new FrontendOptions(); // 실제 설정 재로드
            Require(loaded.IsAvailable && loaded.Volume == 0 && loaded.Shake == 0 && loaded.ResolutionIndex == 0 && loaded.Fullscreen != originalFullscreen && Mathf.Approximately(AudioListener.volume, 0f), $"Settings failed: volume={loaded.Volume}, shake={loaded.Shake}, resolution={loaded.ResolutionIndex}, fullscreen={loaded.Fullscreen}, initial={originalFullscreen}, audio={AudioListener.volume}."); // 실제 설정 저장 확인
            Click("FrontendInputSettingsButton"); // 실제 기존 입력 설정 열기
            SettingsMenuController inputSettings = FindFirstObjectByType<SettingsMenuController>(); // 실제 입력 설정
            Require(inputSettings != null && inputSettings.IsOpen, "Existing input settings did not open."); // 실제 입력 설정 연결 확인
            inputSettings.CloseSettings(); // 실제 기존 설정 닫기
            Click("FrontendSettingsBackButton"); // 실제 주 메뉴 복귀
            Click("FrontendStartButton"); // 실제 캐릭터 선택 진입
            Require(frontend.Stage == FrontendStage.Characters, "Character stage missing."); // 실제 단계 전환 확인
            Require(GameObject.Find("FrontendCharacterHead") != null && GameObject.Find("FrontendCharacterBody1") != null, "Actual character preview missing."); // 실제 머리와 몸통 미리보기 확인
            int mobile = -1; // 실제 기동형 번호
            int heavy = -1; // 실제 중장형 번호
            for (int index = 0; index < characters.CharacterCount; index++) // 실제 여섯 캐릭터 순회
            { // 반복 시작
                CharacterData candidate = characters.GetCharacter(index); // 실제 캐릭터 데이터
                Require(candidate != null && candidate.StartingWeapons != null && candidate.StartingWeapons.Weapons != null && candidate.StartingWeapons.Weapons.Length == 3, "Character data or three starting candidates missing."); // 실제 후보 구성 확인
                if (candidate.Id == "prototype_mobile") // 실제 기동형 확인
                { // 조건 시작
                    mobile = index; // 실제 기동형 번호 기록
                } // 조건 끝
                if (candidate.Id == "prototype_heavy") // 실제 중장형 확인
                { // 조건 시작
                    heavy = index; // 실제 중장형 번호 기록
                } // 조건 끝
                Require(FindButton("FrontendCharacterButton" + index) != null, "Character button missing."); // 실제 캐릭터 버튼 확인
            } // 반복 끝
            Require(mobile >= 0 && heavy >= 0 && !FindButton("FrontendCharacterButton" + mobile).interactable && !characters.TryApply(characters.GetCharacter(mobile)), "Locked character was selectable."); // 실제 캐릭터 잠금 확인
            Click("FrontendCharacterButton" + heavy); // 실제 중장형 선택
            Require(CharacterRunInitializer.ActiveCharacter == characters.GetCharacter(heavy) && FindFirstObjectByType<SnakeHealth>().MaximumHealth == Mathf.RoundToInt(characters.GetCharacter(heavy).BodyMaxHealth), "Selected character stats not applied."); // 실제 선택 능력 적용
            Text details = GameObject.Find("FrontendCharacterDetails").GetComponent<Text>(); // 실제 캐릭터 상세 문구
            Require(details.text.Contains("DAMAGE TAKEN") && details.text.Contains(characters.GetCharacter(heavy).StartingWeapons.Weapons[0].DisplayName), "Character passive or starting candidates not displayed."); // 실제 패시브와 후보 표시 확인
            Click("FrontendCharactersNextButton"); // 실제 맵 선택 진입
            Require(frontend.Stage == FrontendStage.Maps && FindButton("FrontendMapButton0").interactable && !FindButton("FrontendMapButton1").interactable && !FindButton("FrontendMapButton2").interactable && !maps.SelectMap(1), "Locked map selection failed."); // 실제 맵 잠금 확인
            Require(GameObject.Find("FrontendMapPreview0") != null && GameObject.Find("FrontendMapPreview1") != null && GameObject.Find("FrontendMapPreview2") != null, "Actual map previews missing."); // 실제 세 맵 미리보기 확인
            Require(meta.Service.TryCompleteRun("unlock-ui-test", 0d, 0, 0, true, 0), "Could not prepare boss unlock."); // 실제 평원 보스 해금 준비
            Click("FrontendMapsBackButton"); // 실제 캐릭터 화면 복귀
            Require(FindButton("FrontendCharacterButton" + mobile).interactable, "Unlocked character remained disabled."); // 실제 해금 캐릭터 버튼 확인
            Click("FrontendCharacterButton" + mobile); // 실제 해금 캐릭터 선택
            Require(CharacterRunInitializer.ActiveCharacter.Id == "prototype_mobile", "Unlocked character could not be selected."); // 실제 해금 캐릭터 적용
            Click("FrontendCharactersNextButton"); // 실제 해금 맵 화면 진입
            Require(FindButton("FrontendMapButton1").interactable && !FindButton("FrontendMapButton2").interactable, "Map unlock was not reflected in UI."); // 실제 맵 해금 표시 확인
            Click("FrontendMapButton1"); // 실제 해금된 폐허 선택
            Require(maps.SelectedIndex == 1, "Unlocked map could not be selected."); // 실제 맵 적용 확인
            Click("FrontendMapsBackButton"); // 실제 캐릭터 선택 복귀
            Click("FrontendCharacterButton" + heavy); // 실제 중장형 재선택
            Click("FrontendCharactersNextButton"); // 실제 폐허 선택 복귀
            Require(maps.SelectedIndex == 1, "Selected map changed while navigating back."); // 실제 맵 선택 보존 확인
            Click("FrontendMapsNextButton"); // 실제 시작 포탑 단계 진입
            Require(frontend.Stage == FrontendStage.Weapons, "Turret selection stage missing."); // 실제 출격 단계 확인
            weaponButton.onClick.Invoke(); // 실제 첫 시작 포탑 선택
            yield return null; // 실제 전투 단계 반영 대기
            Require(GameManager.Instance.IsPlaying && frontend.Stage == FrontendStage.Playing, "Actual turret selection did not start combat."); // 실제 출격 확인
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            SnakeWeaponMergeController merge = FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 관리자
            EarlyEnemyWaveSpawner wave = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            FindFirstObjectByType<SnakeMovement>().enabled = false; // 실제 플레이어 이동 통제
            wave.enabled = false; // 실제 적 출현 통제
            Require(weapons.TryEquipAt(1, weapons.Slots[0].Weapon, 1) && merge.OpenMergePanel(), "Could not prepare merge."); // 실제 같은 포탑 합성 준비
            InputSettings originalSettings = InputSystem.settings; // 실제 이전 입력 설정
            InputSettings testSettings = Instantiate(originalSettings); // 실제 격리 입력 설정
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 실제 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 실제 배치 포커스 무시
            InputSystem.settings = testSettings; // 실제 격리 입력 적용
            bool previousBackground = Application.runInBackground; // 실제 이전 백그라운드 상태
            Application.runInBackground = true; // 실제 가상 입력 허용
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); // 실제 가상 키보드
            yield return Tap(keyboard, Key.Escape); // 실제 합성 취소 입력
            Require(!merge.IsMergeOpen && GameManager.Instance.IsPlaying && frontend.Stage == FrontendStage.Playing, "Escape did not cancel merge before pause."); // 실제 합성 ESC 우선 확인
            yield return Tap(keyboard, Key.Escape); // 실제 일시정지 입력
            Require(GameManager.Instance.IsPaused && frontend.Stage == FrontendStage.Paused && Time.timeScale == 0f, "Manual pause did not show its menu."); // 실제 수동 정지 화면 확인
            Click("FrontendPauseSettingsButton"); // 실제 일시정지 설정 진입
            yield return Tap(keyboard, Key.Escape); // 실제 설정 중 ESC 입력
            Require(GameManager.Instance.IsPaused && frontend.Stage == FrontendStage.Settings, "Escape resumed combat behind settings."); // 실제 설정 중 정지 유지 확인
            Click("FrontendInputSettingsButton"); // 실제 기존 입력 설정 열기
            Require(inputSettings.IsOpen && GameManager.Instance.IsPaused, "Pause settings lost pause."); // 실제 설정 중 정지 확인
            inputSettings.CloseSettings(); // 실제 기존 입력 설정 닫기
            Click("FrontendSettingsBackButton"); // 실제 일시정지 화면 복귀
            Click("FrontendResumeButton"); // 실제 게임 재개 버튼
            Require(GameManager.Instance.IsPlaying && frontend.Stage == FrontendStage.Playing, "Resume button did not resume combat."); // 실제 재개 확인
            yield return Tap(keyboard, Key.Escape); // 실제 다시 일시정지
            typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wave, 120.01f); // 실제 통제 생존 시간
            typeof(EarlyEnemyWaveSpawner).GetField("previousClockTime", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wave, Time.timeAsDouble); // 실제 시간 표본 동기화
            Click("FrontendAbandonButton"); // 실제 포기 버튼
            yield return null; // 실제 결과 표시 대기
            Require(GameManager.Instance.CurrentState == GameState.Defeated && frontend.Stage == FrontendStage.Results && meta.LastAward == 1 && meta.Service.Cores == 6, "Abandon result or core reward failed."); // 실제 성과 저장 확인
            Text result = GameObject.Find("FrontendResultDetails").GetComponent<Text>(); // 실제 결과 문구
            Require(result.text.Contains("02:00") && result.text.Contains("CORES  +1") && result.text.Contains("Physical TURRET") && result.text.Contains("SAVED") && meta.Service.IsTurretAttributeUnlocked(0), "Result details do not match actual saved reward and unlock."); // 실제 시간과 저장 해금 확인
            Click("FrontendRetryButton"); // 실제 같은 캐릭터 재도전
            yield return WaitForStage(FrontendStage.Weapons); // 실제 씬 재시작 대기
            frontend = RunFrontendController.Instance; // 실제 새 화면 관리자
            meta = MetaProgressionController.Instance; // 실제 새 진행 관리자
            Require(frontend.Stage == FrontendStage.Weapons && meta.Service.Cores == 6 && FindFirstObjectByType<PrototypeMapSelection>().SelectedIndex == 1 && CharacterRunInitializer.ActiveCharacter.Id == "prototype_heavy", $"Retry state: stage={frontend.Stage}, cores={meta.Service.Cores}, map={FindFirstObjectByType<PrototypeMapSelection>().SelectedIndex}, character={(CharacterRunInitializer.ActiveCharacter == null ? "null" : CharacterRunInitializer.ActiveCharacter.Id)}."); // 실제 저장과 재도전 확인
            panel = FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 새 시작 포탑 화면
            weaponButton = (Button)typeof(LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel); // 실제 새 포탑 버튼
            weaponButton.onClick.Invoke(); // 실제 두 번째 출격
            yield return null; // 실제 전투 화면 전환 대기
            yield return Tap(keyboard, Key.Escape); // 실제 두 번째 일시정지
            Require(frontend.Stage == FrontendStage.Paused, "Second run pause failed."); // 실제 재도전 정지 확인
            Day48MemoryStorage failing = new Day48MemoryStorage { FailWrite = true }; // 실제 저장 실패 재현
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, new MetaProgressionService(failing)); // 실제 실패 저장소 연결
            Click("FrontendPauseMainButton"); // 실제 저장 실패 상태에서 주 메뉴 시도
            Require(meta.HasPendingSave && frontend.Stage == FrontendStage.Results && GameObject.Find("FrontendResultDetails").GetComponent<Text>().text.Contains("SAVE FAILED"), "Failed save did not block main menu or display error."); // 실제 성과 보존과 오류 확인
            failing.FailWrite = false; // 실제 저장 복구
            Click("FrontendResultMainButton"); // 실제 저장 재시도와 주 메뉴 복귀
            yield return WaitForStage(FrontendStage.Main); // 실제 주 메뉴 재시작 대기
            Require(RunFrontendController.Instance.Stage == FrontendStage.Main && MetaProgressionController.Instance.Service.Cores == 6, "Main menu return or saved progress failed."); // 실제 주 메뉴와 저장 확인
            InputSystem.RemoveDevice(keyboard); // 실제 가상 키보드 제거
            if (originalSettings != null) // 실제 이전 입력 설정 유효성 확인
            { // 조건 시작
                InputSystem.settings = originalSettings; // 실제 이전 입력 설정 복구
            } // 조건 끝
            Application.runInBackground = previousBackground; // 실제 이전 백그라운드 복구
            if (testSettings != null && InputSystem.settings != testSettings) // 실제 임시 설정 사용 여부 확인
            { // 조건 시작
                Destroy(testSettings); // 사용하지 않는 임시 입력 설정 제거
            } // 조건 끝
            AutomatedDay49FrontendVerification.Finish(0, "Six characters, locked maps, settings, merge priority, pause, abandon, result, retry and menu persistence passed."); // 실제 전체 시험 성공
        } // 코루틴 끝
        private static IEnumerator Tap(Keyboard keyboard, Key key) // 실제 키 입력과 해제
        { // 메서드 시작
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 실제 키 누름 전달
            yield return null; // 실제 입력 프레임 대기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 실제 키 해제 전달
            yield return null; // 실제 입력 해제 대기
        } // 코루틴 끝
        private static IEnumerator WaitForStage(FrontendStage stage) // 실제 새 씬 단계 대기
        { // 메서드 시작
            float deadline = Time.realtimeSinceStartup + 15f; // 실제 단계 제한 시간
            while (Time.realtimeSinceStartup < deadline && (RunFrontendController.Instance == null || RunFrontendController.Instance.Stage != stage)) // 실제 새 단계 확인
            { // 반복 시작
                yield return null; // 실제 씬과 초기화 대기
            } // 반복 끝
            Require(RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == stage, "Scene restart stage timed out."); // 실제 단계 전환 확인
        } // 코루틴 끝
        private static Button FindButton(string name) // 실제 활성 버튼 조회
        { // 메서드 시작
            GameObject item = GameObject.Find(name); // 실제 이름으로 조회
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        } // 메서드 끝
        private static void Click(string name) // 실제 버튼 이벤트 호출
        { // 메서드 시작
            Button button = FindButton(name); // 실제 버튼 조회
            Require(button != null && button.IsActive() && button.interactable, "Missing or disabled button: " + name); // 실제 사용 가능 확인
            button.onClick.Invoke(); // 실제 버튼 이벤트 실행
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 여부 확인
            { // 조건 시작
                AutomatedDay49FrontendVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 실제 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
