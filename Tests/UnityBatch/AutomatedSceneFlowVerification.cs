using System; // 실제 예외와 대기 조건
using System.Collections; // 실제 프레임 대기
using ProjectEpsilon.Combat; // 실제 포탑 장착
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 시작 포탑 데이터
using ProjectEpsilon.Player; // 실제 몸통과 캐릭터
using ProjectEpsilon.Progression; // 실제 저장과 해금
using ProjectEpsilon.UI; // 실제 화면과 씬 역할
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEditor; // 실제 배치 제어
using UnityEditor.SceneManagement; // 실제 시작 씬 열기
using UnityEngine; // Unity 실행 기능
using UnityEngine.EventSystems; // 실제 UI 이벤트
using UnityEngine.InputSystem.UI; // 실제 UI 입력 모듈
using UnityEngine.SceneManagement; // 실제 씬 상태
using UnityEngine.UI; // 실제 버튼 입력

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 실제 플레이 모드 재연결
    public static class AutomatedSceneFlowVerification // 실제 분리 씬 전환 시험
    {
        private const string Key = "EpsilonSceneFlowVerification"; // 실제 시험 진행 표식
        private static bool started; // 실제 시험 실행 여부
        static AutomatedSceneFlowVerification() // 실제 도메인 재시작 연결
        {
            if (SessionState.GetBool(Key, false)) // 실제 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시험 시작 감시
            }
        }
        public static void Run() // 실제 Unity 배치 진입점
        {
            EditorSceneManager.OpenScene(SceneFrontendContext.MainMenuPath); // 실제 메인 메뉴 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 플레이 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 플레이 시작 확인
        {
            if (!EditorApplication.isPlaying || started) // 실제 시작과 중복 확인
            {
                return; // 실제 플레이 시작 대기
            }
            started = true; // 실제 시험 중복 방지
            GameObject runner = new GameObject("SceneFlowVerification"); // 실제 배치 시험 객체
            UnityEngine.Object.DontDestroyOnLoad(runner); // 실제 씬 간 시험 유지
            runner.AddComponent<SceneFlowVerificationRunner>(); // 실제 씬 전환 시험 실행
        }
        public static void Finish(int code, string message) // 실제 배치 종료
        {
            if (code == 0) // 실제 시험 성공 확인
            {
                Debug.Log("[SceneFlowVerification] PASS: " + message); // 실제 성공 결과
            }
            else // 실제 시험 실패 확인
            {
                Debug.LogError("[SceneFlowVerification] FAIL: " + message); // 실제 실패 결과
            }
            SessionState.SetBool(Key, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 연결 해제
            EditorApplication.Exit(code); // 실제 시험 종료 코드
        }
    }
    public sealed class SceneFlowVerificationRunner : MonoBehaviour // 실제 화면 흐름 시험 실행기
    {
        private IEnumerator Start() // 실제 예외를 포함한 시험 실행
        {
            IEnumerator steps = RunSteps(); // 실제 화면 흐름 단계
            while (true) // 실제 시험 완료까지 반복
            {
                bool moved; // 실제 다음 단계 여부
                try // 실제 시험 단계 실행
                {
                    moved = steps.MoveNext(); // 실제 다음 단계 진행
                }
                catch (Exception error) // 실제 시험 오류 확인
                {
                    AutomatedSceneFlowVerification.Finish(1, error.ToString()); // 실제 오류와 실패 종료
                    yield break; // 추가 시험 중단
                }
                if (!moved) // 실제 시험 완료 확인
                {
                    yield break; // 실제 실행 종료
                }
                yield return steps.Current; // 실제 다음 프레임 대기
            }
        }
        private IEnumerator RunSteps() // 실제 화면과 저장 검증
        {
            Require(Application.productName == "Project-Epsilon-SceneFlow-Isolated", "User save profile is not isolated."); // 실제 사용자 저장 격리 확인
            yield return null; // 실제 메뉴 초기화 대기
            PlayerPrefs.DeleteKey("ProjectEpsilon.MetaProgress.v1"); // 실제 격리 프로필의 이전 시험 저장 제거
            PlayerPrefs.Save(); // 실제 격리 저장 초기화 반영
            MetaProgressionController menuMeta = MetaProgressionController.Instance; // 실제 메뉴 진행 관리자
            typeof(MetaProgressionController).GetProperty("Service").SetValue(menuMeta, new MetaProgressionService(new PlayerPrefsMetaStorage("ProjectEpsilon.MetaProgress.v1"))); // 실제 초기 저장 서비스 재연결
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath && Button("FrontendStartButton") != null, "Main menu did not initialize."); // 실제 메인 메뉴 확인
            InputSystemUIInputModule module = EventSystem.current == null ? null : EventSystem.current.GetComponent<InputSystemUIInputModule>(); // 실제 UI 입력 모듈
            Require(module != null && module.actionsAsset != null, "Main menu input actions missing."); // 실제 UI 입력 구성 확인
            Button("FrontendSettingsButton").onClick.Invoke(); // 실제 메뉴 설정 열기
            yield return null; // 실제 설정 구성 대기
            Button("FrontendInputSettingsButton").onClick.Invoke(); // 실제 키 설정 열기
            yield return null; // 실제 키 설정 표시 대기
            SettingsMenuController settings = FindFirstObjectByType<SettingsMenuController>(); // 실제 기존 키 설정기
            Require(settings != null && settings.IsOpen && Button("InputClose") != null, "Standalone input settings did not open."); // 실제 메뉴 키 설정 확인
            Button("InputClose").onClick.Invoke(); // 실제 키 설정 닫기
            yield return null; // 실제 입력 복귀 대기
            Button("FrontendSettingsBackButton").onClick.Invoke(); // 실제 메인 메뉴 복귀
            yield return null; // 실제 메뉴 갱신 대기
            Button("FrontendStartButton").onClick.Invoke(); // 실제 준비 씬 이동
            yield return null; // 실제 준비 씬 초기화 대기
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.PreparationPath && Button("FrontendCharacterButton2") != null, "Preparation character screen missing."); // 실제 캐릭터 선택 확인
            Require(!Button("FrontendCharacterButton1").interactable, "Locked character became selectable."); // 실제 캐릭터 해금 차단 확인
            Button("FrontendCharacterButton2").onClick.Invoke(); // 실제 중장형 캐릭터 선택
            yield return null; // 실제 캐릭터 정보 갱신 대기
            CharacterData selected = CharacterRunInitializer.SelectedCharacter; // 실제 선택된 캐릭터
            Require(selected != null && selected.Id == "heavy", "Character selection was not retained."); // 실제 선택 데이터 확인
            WeaponData turret = selected.StartingWeapons.Weapons[1]; // 실제 두 번째 시작 포탑
            Button("FrontendCharactersNextButton").onClick.Invoke(); // 실제 맵 선택 이동
            yield return null; // 실제 맵 화면 갱신 대기
            Require(!Button("FrontendMapButton1").interactable, "Locked map became selectable."); // 실제 맵 해금 차단 확인
            MetaProgressionController preparationMeta = MetaProgressionController.Instance; // 실제 준비 화면 저장 관리자
            Require(preparationMeta.Service.TryCompleteRun("scene-unlock", 0d, 0, 0, true, 0), "Could not unlock the test map."); // 실제 격리 저장의 맵 해금
            Button("FrontendMapButton0").onClick.Invoke(); // 실제 맵 화면의 해금 상태 갱신
            yield return null; // 실제 해금 버튼 갱신 대기
            Require(Button("FrontendMapButton1").interactable, "Unlocked map is still disabled."); // 실제 새 맵 해금 확인
            Button("FrontendMapButton1").onClick.Invoke(); // 실제 해금된 두 번째 맵 선택
            yield return null; // 실제 선택 맵 갱신 대기
            Button("FrontendMapsNextButton").onClick.Invoke(); // 실제 포탑 선택 이동
            for (int frame = 0; frame < 120 && (SceneManager.GetActiveScene().path != SceneFrontendContext.GamePath || FindFirstObjectByType<StartingWeaponSelection>() == null || !FindFirstObjectByType<StartingWeaponSelection>().IsSelecting); frame++) // 실제 맵의 포탑 선택 대기
            { // 반복 시작
                yield return null; // 실제 게임 씬 초기화 대기
            } // 반복 종료
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && FindFirstObjectByType<StartingWeaponSelection>() != null && FindFirstObjectByType<StartingWeaponSelection>().IsSelecting, "Game scene starting selection missing."); // 실제 맵 안 시작 선택 확인
            StartingWeaponBatchHelper.HandleStartingSelection(1); // 실제 두 번째 포탑 선택
            for (int frame = 0; frame < 120 && (GameManager.Instance == null || !GameManager.Instance.IsPlaying); frame++) // 실제 전투 시작 대기
            {
                yield return null; // 실제 씬 초기화 프레임 대기
            }
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && GameManager.Instance != null && GameManager.Instance.IsPlaying, "Game scene did not start."); // 실제 전투 씬과 시작 확인
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeHealth health = FindFirstObjectByType<SnakeHealth>(); // 실제 공용 체력
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 장착 관리자
            PrototypeMapSelection maps = FindFirstObjectByType<PrototypeMapSelection>(); // 실제 적용 맵
            Require(CharacterRunInitializer.ActiveCharacter == selected && body != null && body.CurrentBodyCount == 4 && health != null && health.MaximumHealth == 125 && weapons != null && weapons.Slots[0].Weapon == turret && maps != null && maps.SelectedIndex == 1, $"Prepared data mismatch: selected={selected?.Id}, active={CharacterRunInitializer.ActiveCharacter?.Id}, body={body?.CurrentBodyCount}, hp={health?.MaximumHealth}, weapon={weapons?.Slots[0].Weapon?.Id}, expected={turret?.Id}, map={maps?.SelectedIndex}."); // 실제 선택과 능력치 전달 확인
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 전투 성과 관리자
            SceneFlowMemoryStorage storage = new SceneFlowMemoryStorage(); // 실제 저장 실패 시험 저장소
            MetaProgressionService service = new MetaProgressionService(storage); // 실제 임시 저장 서비스
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, service); // 실제 시험 저장소 교체
            storage.FailWrite = true; // 실제 저장 실패 재현
            GameManager.Instance.AbandonRun(); // 실제 런 포기와 성과 저장
            yield return null; // 실제 결과 씬 전환 대기
            yield return null; // 실제 결과 화면 생성 대기
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.ResultsPath && meta.HasPendingSave && Button("FrontendRetryButton") != null, "Results or failed save state was lost."); // 실제 결과와 저장 실패 보존 확인
            Button("FrontendRetryButton").onClick.Invoke(); // 실제 저장 실패 중 재도전
            yield return null; // 실제 실패 처리 대기
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.ResultsPath && meta.HasPendingSave, "Failed save did not block retry."); // 실제 저장 실패의 이동 차단 확인
            storage.FailWrite = false; // 실제 저장 성공 복원
            Button("FrontendRetryButton").onClick.Invoke(); // 실제 재도전 재시도
            for (int frame = 0; frame < 120 && (SceneManager.GetActiveScene().path != SceneFrontendContext.GamePath || FindFirstObjectByType<StartingWeaponSelection>() == null || !FindFirstObjectByType<StartingWeaponSelection>().IsSelecting); frame++) // 실제 맵의 재도전 선택 대기
            { // 반복 시작
                yield return null; // 실제 새 전투 씬 초기화 대기
            } // 반복 종료
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && FindFirstObjectByType<StartingWeaponSelection>() != null && FindFirstObjectByType<StartingWeaponSelection>().IsSelecting && storage.Content != null, "Retry did not save and return to turret selection."); // 실제 저장 후 맵의 포탑 재선택 확인
            StartingWeaponBatchHelper.HandleStartingSelection(1); // 실제 같은 포탑 재도전
            for (int frame = 0; frame < 120 && (GameManager.Instance == null || !GameManager.Instance.IsPlaying); frame++) // 실제 재도전 시작 대기
            {
                yield return null; // 실제 재도전 초기화 프레임 대기
            }
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && GameManager.Instance != null && GameManager.Instance.IsPlaying && CharacterRunInitializer.ActiveCharacter.Id == "heavy" && FindFirstObjectByType<PrototypeMapSelection>().SelectedIndex == 1, "Retry lost selected character or map."); // 실제 재도전 캐릭터와 맵 확인
            GameObject bossObject = new GameObject("SceneFlowTestBoss"); // 실제 등록할 시험 보스
            WeaponTarget boss = bossObject.AddComponent<WeaponTarget>(); // 실제 공격 대상
            Require(GameManager.Instance.RegisterFinalBoss(boss), "Could not register final boss."); // 실제 보스 등록 확인
            boss.TakeDamage(1000f); // 실제 등록 보스 처치
            yield return null; // 실제 결과 씬 전환 대기
            yield return null; // 실제 결과 버튼 생성 대기
            GameObject titleObject = GameObject.Find("FrontendTitle"); // 실제 결과 제목 객체
            Text title = titleObject == null ? null : titleObject.GetComponent<Text>(); // 실제 결과 제목
            Require(title != null && title.text == "승리" && MetaProgressionController.Instance.Service.IsMapUnlocked(2), "Victory result or boss unlock was lost."); // 실제 승리 표시와 다음 맵 해금 확인
            Text scoreDetails = GameObject.Find("FrontendScoreDetails")?.GetComponent<Text>(); // 실제 분리 결과 씬의 점수 상세
            Require(scoreDetails != null && scoreDetails.text.Contains("총점") && scoreDetails.text.Contains("보스 처치"), "Victory score details missing in results scene."); // 실제 씬 전환 후 상세 점수 확인
            Button("FrontendResultMainButton").onClick.Invoke(); // 실제 메인 메뉴 복귀
            yield return null; // 실제 종료 관리자 제거 대기
            yield return null; // 실제 메뉴 씬 로드 대기
            yield return null; // 실제 메뉴 화면 생성 대기
            Require(SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath && GameManager.Instance == null && Button("FrontendStartButton") != null, "Main menu return kept old game state."); // 실제 이전 전투 정리 확인
            AutomatedSceneFlowVerification.Finish(0, "Menu, controls, preparation, selected loadout, save failure, retry and main menu passed."); // 실제 전체 시험 성공
        }
        private static Button Button(string name) // 실제 활성 버튼 조회
        {
            GameObject item = GameObject.Find(name); // 실제 화면 객체 검색
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
    }
    public sealed class SceneFlowMemoryStorage : IMetaStorage // 실제 저장 실패 시험 저장소
    {
        public string Content; // 실제 저장 내용
        public bool FailWrite; // 실제 저장 실패 여부
        public bool TryRead(out string json) // 실제 시험 저장 읽기
        {
            json = Content; // 실제 저장 내용 반환
            return true; // 실제 읽기 성공
        }
        public bool TryWrite(string json) // 실제 시험 저장 기록
        {
            if (FailWrite) // 실제 저장 실패 확인
            {
                return false; // 실제 기록 실패 반환
            }
            Content = json; // 실제 저장 내용 기록
            return true; // 실제 기록 성공
        }
    }
}
