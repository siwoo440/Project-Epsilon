using System; // 실제 예외와 조건
using System.Collections; // 실제 씬 전환 대기
using System.Reflection; // 실제 후보 버튼 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터와 포탑 데이터
using ProjectEpsilon.Player; // 실제 캐릭터 적용
using ProjectEpsilon.Progression; // 실제 선택과 해금
using ProjectEpsilon.UI; // 실제 화면과 씬 구성
using ProjectEpsilon.World; // 실제 맵 구성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.InputSystem; // 실제 입력 시험
using UnityEngine.InputSystem.LowLevel; // 실제 키 입력 상태
using UnityEngine.SceneManagement; // 실제 씬 조회
using UnityEngine.UI; // 실제 버튼과 카드

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay90MapStartingSelectionVerification // 실제 맵 진입과 시작 포탑 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay90StartingSelection"; // 시험 상태 표식
        private static bool injected; // 중복 실행 방지
        static AutomatedDay90MapStartingSelectionVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene(SceneFrontendContext.GamePath); // 실제 게임 직접 진입
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 플레이 시작 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || injected) // 실제 시작과 중복 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            injected = true; // 중복 실행 방지
            GameObject runner = new GameObject("Day90MapStartingSelectionRunner"); // 실제 씬 전환 시험 객체
            UnityEngine.Object.DontDestroyOnLoad(runner); // 실제 씬 전환 간 시험 보존
            runner.AddComponent<Day90MapStartingSelectionRunner>(); // 실제 흐름 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 배치 결과
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day90MapStartingSelectionVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 실제 종료 코드
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day90MapStartingSelectionRunner : MonoBehaviour // 실제 시작 흐름 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 메뉴와 전투 씬 검증
        { // 함수 시작
            Require(Application.productName.EndsWith("-Isolated", StringComparison.Ordinal), "독립 시험 저장 필요"); // 실제 사용자 저장 보호
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath && Button("FrontendStartButton") != null, "실제 메인 메뉴 누락"); // 실제 분리 메인 메뉴 확인
            Require(Button("FrontendStartButton").GetComponent<Image>().sprite != null && Button("FrontendStartButton").GetComponent<Image>().sprite.texture.name == "ButtonFrame", "실제 메인 메뉴 버튼 프레임 누락"); // 실제 시작 버튼 이미지 확인
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            Require(meta != null && meta.Service != null, "실제 해금 관리자 누락"); // 실제 저장 구성 확인
            if (!meta.IsMapUnlocked(1)) // 실제 폐허 해금 확인
            { // 조건 시작
                Require(meta.Service.TryCompleteRun("day90-map-unlock", 0d, 0, 0, true, 0), "독립 시험의 폐허 해금 실패"); // 실제 독립 저장의 평원 승리
            } // 조건 종료
            Button("FrontendStartButton").onClick.Invoke(); // 실제 출격 준비 진입
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.PreparationPath && Button("FrontendCharacterButton2") != null, "실제 캐릭터 선택 누락"); // 실제 캐릭터 선택 확인
            Button("FrontendCharacterButton2").onClick.Invoke(); // 실제 중장형 선택
            yield return null; // 실제 선택 화면 갱신 대기
            Button("FrontendCharactersNextButton").onClick.Invoke(); // 실제 맵 선택 이동
            yield return null; // 실제 맵 화면 갱신 대기
            Image mapCard = GameObject.Find("FrontendMapCard0").GetComponent<Image>(); // 실제 첫 맵 카드
            Require(mapCard.sprite != null && mapCard.sprite.texture.name == "RewardCardFrame" && !mapCard.raycastTarget, "실제 맵 카드 프레임 누락"); // 카드 디자인과 입력 보존 확인
            Button ruins = Button("FrontendMapButton1"); // 실제 폐허 선택 버튼
            Require(ruins != null && ruins.interactable, "해금된 폐허 선택 누락"); // 실제 맵 해금 확인
            ruins.onClick.Invoke(); // 실제 폐허 선택
            yield return null; // 실제 선택 맵 갱신 대기
            Button next = Button("FrontendMapsNextButton"); // 실제 출격 버튼
            Require(next != null, "실제 맵 출격 버튼 누락"); // 실제 출격 구성 확인
            next.onClick.Invoke(); // 실제 선택 맵 진입
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && Ready(), "맵 선택 후 게임 씬의 시작 포탑 카드 누락"); // 실제 전투 씬 선택 확인
            yield return null; // 실제 청크 생성 대기
            yield return null; // 실제 HUD와 카드 갱신 대기
            PrototypeMapSelection maps = FindFirstObjectByType<PrototypeMapSelection>(); // 실제 게임 맵 선택기
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 게임 맵 청크
            StartingWeaponSelection selection = FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 포탑 선택기
            CharacterData character = CharacterRunInitializer.ActiveCharacter; // 실제 선택 캐릭터
            Require(maps != null && maps.SelectedIndex == 1 && map != null && map.Profile == maps.SelectedProfile && map.ActiveCount == 9, "선택한 폐허 맵이 시작 포탑 뒤에 생성되지 않음"); // 실제 맵과 배경 확인
            GameObject center = map.GetChunk(Vector2Int.zero); // 실제 중앙 청크
            SpriteRenderer ground = center == null || center.transform.Find("Ground") == null ? null : center.transform.Find("Ground").GetComponent<SpriteRenderer>(); // 실제 선택 맵 바닥
            Require(ground != null && ground.sprite == map.Profile.Ground, "실제 선택 맵 바닥 이미지 오류"); // 실제 배경 이미지 확인
            Require(character != null && character.Id == "heavy" && GameManager.Instance.CurrentState == GameState.Starting && Mathf.Approximately(Time.timeScale, 0f), "캐릭터 보존 또는 선택 중 시간 정지 오류"); // 실제 캐릭터와 시작 상태 확인
            Require(selection != null && selection.IsSelecting && GameObject.Find("FrontendWeaponBackButton") == null && GameObject.Find("FrontendStartingWeaponBackButton") == null && GameObject.Find("FrontendMapsBackButton") == null, "시작 포탑 선택의 뒤로 가기 노출"); // 실제 뒤로 가기 제거 확인
            LevelUpPanelController panel = FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 공용 레벨업 카드
            FieldInfo candidateField = typeof(LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 첫 후보 버튼
            Button candidate = candidateField == null || panel == null ? null : candidateField.GetValue(panel) as Button; // 실제 공용 카드 버튼
            Require(panel != null && panel.gameObject.activeInHierarchy && candidate != null && candidate.interactable, "실제 공용 레벨업 포탑 카드 누락"); // 실제 카드 재사용 확인
            Image panelBackground = panel.GetComponent<Image>(); // 실제 시작 카드 배경
            Require(panelBackground != null && panelBackground.color.a <= 0.7f, "시작 카드의 불투명 배경이 선택 맵을 가림"); // 실제 선택 맵을 위한 배경 투명도 확인
            FieldInfo originalOpacityField = typeof(LevelUpPanelController).GetField("originalBackgroundOpacity", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 기존 배경 불투명도 필드
            Require(originalOpacityField != null, "기존 보상 배경 기록 누락"); // 실제 복원값 확인
            float originalOpacity = (float)originalOpacityField.GetValue(panel); // 실제 씬의 기존 보상 불투명도
            panel.ShowEliteRewards(new[] { "회복", "복구", "포탑" }, new[] { true, true, true }); // 실제 정예 보상 화면 전환
            Require(Mathf.Approximately(panelBackground.color.a, originalOpacity), "정예 보상 전환 시 기존 배경이 복원되지 않음"); // 실제 정예 보상 배경 복원 확인
            selection.RefreshStartingDisplay(); // 실제 시작 포탑 후보 복원
            Require(panelBackground.color.a <= 0.7f && selection.IsSelecting, "정예 보상 이후 시작 카드 배경 복원 실패"); // 실제 시작 배경 재적용 확인
            WeaponRewardCandidate[] levelCandidates = { new WeaponRewardCandidate(character.StartingWeapons.Weapons[0], 1) }; // 실제 포탑 데이터의 레벨업 후보
            panel.Show(2, false, 4, 4, false, levelCandidates); // 실제 일반 레벨업 카드 전환
            Require(Mathf.Approximately(panelBackground.color.a, originalOpacity), "일반 레벨업 전환 시 기존 배경이 복원되지 않음"); // 실제 일반 레벨업 배경 복원 확인
            selection.RefreshStartingDisplay(); // 실제 시작 포탑 후보 재복원
            Require(panelBackground.color.a <= 0.7f && selection.IsSelecting, "레벨업 이후 시작 카드 배경 복원 실패"); // 실제 시작 배경 재적용 확인
            InputSettings originalSettings = InputSystem.settings; // 실제 이전 입력 설정
            InputSettings testSettings = UnityEngine.Object.Instantiate(originalSettings); // 실제 독립 입력 설정
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 실제 프로젝트 설정 보호
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 실제 시험 입력 적용
            bool originalBackground = Application.runInBackground; // 실제 이전 백그라운드 설정
            Application.runInBackground = true; // 실제 배치 키 입력 허용
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); // 실제 가상 키보드
            yield return Tap(keyboard, Key.UpArrow); // 실제 맵 변경 키 입력
            yield return Tap(keyboard, Key.RightArrow); // 실제 캐릭터 변경 키 입력
            yield return Tap(keyboard, Key.Escape); // 실제 뒤로 가기 키 입력
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && maps.SelectedIndex == 1 && CharacterRunInitializer.ActiveCharacter == character && selection.IsSelecting, "시작 선택 중 이전 화면 또는 맵·캐릭터 변경 허용"); // 실제 시작 선택 잠금 확인
            WeaponData expected = character.StartingWeapons.Weapons[0]; // 실제 첫 시작 포탑
            candidate.onClick.Invoke(); // 실제 레벨업 카드 선택
            yield return WaitFor(() => GameManager.Instance != null && GameManager.Instance.IsPlaying, "시작 포탑 선택 후 전투 시작 실패"); // 실제 포탑 선택과 전투 시작 확인
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 몸통 포탑 관리자
            Require(weapons != null && weapons.Slots[0].Weapon == expected && !selection.IsSelecting, "선택한 시작 포탑 장착 오류"); // 실제 첫 몸통 장착 확인
            Require(Mathf.Approximately(panelBackground.color.a, originalOpacity), "시작 선택 후 일반 보상 배경이 복원되지 않음"); // 실제 기존 레벨업 배경 복원 확인
            InputSystem.RemoveDevice(keyboard); // 실제 가상 키보드 제거
            if (originalSettings != null) // 실제 이전 입력 설정 생존 확인
            { // 조건 시작
                InputSystem.settings = originalSettings; // 실제 이전 입력 설정 복구
            } // 조건 종료
            Application.runInBackground = originalBackground; // 실제 이전 백그라운드 설정 복구
            if (testSettings != null && InputSystem.settings != testSettings) // 실제 임시 설정 사용 상태 확인
            { // 조건 시작
                Destroy(testSettings); // 사용하지 않는 임시 입력 설정 제거
            } // 조건 종료
            GameManager.Instance.AbandonRun(); // 실제 전투 포기와 결과 전환
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.ResultsPath && Button("FrontendRetryButton") != null, "실제 결과 화면 누락"); // 실제 결과 화면 확인
            Button("FrontendRetryButton").onClick.Invoke(); // 실제 동일 조건 재도전
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && Ready(), "재도전이 게임 씬의 포탑 선택으로 돌아가지 않음"); // 실제 재도전 선택 확인
            yield return null; // 실제 재도전 맵 생성 대기
            yield return null; // 실제 재도전 HUD 갱신 대기
            maps = FindFirstObjectByType<PrototypeMapSelection>(); // 실제 재도전 맵 선택기
            map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 재도전 맵 청크
            Require(maps != null && maps.SelectedIndex == 1 && map != null && map.ActiveCount == 9 && CharacterRunInitializer.ActiveCharacter != null && CharacterRunInitializer.ActiveCharacter.Id == "heavy", "재도전 시 이전 맵 또는 캐릭터 손실"); // 실제 재도전 조건 보존 확인
            Require(GameObject.Find("FrontendWeaponBackButton") == null && Mathf.Approximately(Time.timeScale, 0f), "재도전 시작 선택에서 뒤로 가기 또는 시간 진행"); // 실제 재도전 잠금 확인
            AutomatedDay90MapStartingSelectionVerification.Finish(true, "Selected map background, existing cards, locked start and retry passed."); // 실제 전체 결과
        } // 함수 종료
        private static bool Ready() // 실제 시작 포탑 선택 준비 확인
        { // 함수 시작
            StartingWeaponSelection selection = FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 선택기
            return selection != null && selection.IsSelecting && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Starting && RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == FrontendStage.Weapons; // 실제 게임 씬 선택 상태
        } // 함수 종료
        private static IEnumerator Tap(Keyboard keyboard, Key key) // 실제 키 누름과 해제
        { // 함수 시작
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 실제 가상 키 누름
            yield return null; // 실제 키 입력 프레임
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 실제 가상 키 해제
            yield return null; // 실제 키 해제 프레임
        } // 함수 종료
        private static IEnumerator WaitFor(Func<bool> condition, string message) // 실제 씬 조건 대기
        { // 함수 시작
            float deadline = Time.realtimeSinceStartup + 20f; // 실제 대기 제한
            while (!condition() && Time.realtimeSinceStartup < deadline) // 실제 조건과 제한 확인
            { // 반복 시작
                yield return null; // 실제 다음 프레임
            } // 반복 종료
            Require(condition(), message); // 실제 완료 조건 확인
        } // 함수 종료
        private static Button Button(string name) // 실제 활성 버튼 조회
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 실제 버튼 객체
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay90MapStartingSelectionVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
