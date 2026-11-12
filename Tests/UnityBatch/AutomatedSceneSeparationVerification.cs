using System; // 실제 예외와 대기 조건
using System.Collections; // 실제 프레임 대기
using ProjectEpsilon.Core; // 실제 전투 관리자
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.UI; // 실제 화면과 씬 구성
using UnityEditor; // 실제 Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 화면 기능
using UnityEngine.SceneManagement; // 실제 활성 씬 조회
using UnityEngine.UI; // 실제 배경과 버튼 조회

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 실제 플레이 진입 후 시험 복구
    public static class AutomatedSceneSeparationVerification // 실제 선택 화면과 전투 씬 분리 검증
    {
        private const string ActiveKey = "EpsilonSceneSeparationVerification"; // 실제 시험 진행 표식
        private static bool injected; // 실제 시험 실행기 생성 여부

        static AutomatedSceneSeparationVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 시험 진행 상태 확인
            {
                EditorApplication.update += Observe; // 실제 플레이 시작 관찰
            }
        }

        public static void Run() // 게임 씬 직접 실행 시험 시작
        {
            EditorSceneManager.OpenScene(SceneFrontendContext.GamePath); // 실제 게임 씬 직접 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 상태 기록
            EditorApplication.update -= Observe; // 기존 관찰 연결 해제
            EditorApplication.update += Observe; // 실제 시작 관찰 연결
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }

        private static void Observe() // 실제 플레이 시작과 실행기 준비
        {
            if (!EditorApplication.isPlaying || injected) // 실제 시작과 중복 실행 확인
            {
                return; // 실제 플레이 대기
            }
            injected = true; // 실행기 중복 생성 차단
            GameObject runner = new GameObject("SceneSeparationVerification"); // 실제 씬 간 시험 객체
            UnityEngine.Object.DontDestroyOnLoad(runner); // 실제 씬 전환 중 시험 보존
            runner.AddComponent<SceneSeparationVerificationRunner>(); // 실제 씬 분리 시험 시작
        }

        public static void Finish(bool passed, string message) // 실제 시험 결과와 종료
        {
            SessionState.SetBool(ActiveKey, false); // 실제 시험 진행 표식 제거
            EditorApplication.update -= Observe; // 실제 관찰 연결 해제
            Debug.Log("[SceneSeparationVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 실제 종료 코드 반환
        }
    }

    public sealed class SceneSeparationVerificationRunner : MonoBehaviour // 실제 화면 흐름 시험 실행기
    {
        private IEnumerator Start() // 실제 씬 분리와 배경 검증
        {
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath && Button("FrontendStartButton") != null, "Direct game start did not reach the separate main menu."); // 실제 게임 직접 실행 후 메인 메뉴 확인
            Require(GameManager.Instance == null && UnityEngine.Object.FindFirstObjectByType<SnakeMovement>() == null && UnityEngine.Object.FindFirstObjectByType<HUDController>() == null, "Gameplay objects remained in the main menu."); // 실제 메인 메뉴의 전투 객체 부재
            RequireOpaqueBackground("main"); // 실제 메인 메뉴 배경 확인
            Button("FrontendStartButton").onClick.Invoke(); // 실제 출격 준비 씬 이동
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.PreparationPath && Button("FrontendCharactersNextButton") != null, "Separate character selection did not appear."); // 실제 캐릭터 선택 씬 확인
            Require(GameManager.Instance == null && UnityEngine.Object.FindFirstObjectByType<SnakeMovement>() == null && UnityEngine.Object.FindFirstObjectByType<HUDController>() == null, "Gameplay objects remained behind character selection."); // 실제 선택 화면의 전투 객체 부재
            RequireOpaqueBackground("character"); // 실제 캐릭터 선택 배경 확인
            Button("FrontendCharactersNextButton").onClick.Invoke(); // 실제 맵 선택 이동
            yield return null; // 실제 맵 화면 구성 대기
            Require(Button("FrontendMapsNextButton") != null, "Map selection did not appear."); // 실제 맵 선택 확인
            RequireOpaqueBackground("map"); // 실제 맵 선택 배경 확인
            Button("FrontendMapsNextButton").onClick.Invoke(); // 실제 포탑 선택 이동
            yield return null; // 실제 포탑 화면 구성 대기
            Require(Button("FrontendStartingWeaponButton1") != null, "Separate starting turret selection did not appear."); // 실제 시작 포탑 선택 확인
            RequireOpaqueBackground("weapon"); // 실제 시작 포탑 선택 배경 확인
            Button("FrontendStartingWeaponButton1").onClick.Invoke(); // 실제 두 번째 포탑 선택과 출격
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && GameManager.Instance != null && GameManager.Instance.IsPlaying, "Prepared game scene did not start."); // 실제 전투 씬 진입 확인
            yield return WaitFor(() => RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == FrontendStage.Playing, "Gameplay frontend did not finish switching to play."); // 실제 전투 화면 상태 갱신 대기
            Require(UnityEngine.Object.FindFirstObjectByType<SnakeMovement>() != null && UnityEngine.Object.FindFirstObjectByType<HUDController>() != null && RunFrontendController.Instance != null && RunFrontendController.Instance.Stage == FrontendStage.Playing, "Gameplay or HUD did not initialize after preparation."); // 실제 플레이어와 전투 HUD 확인
            GameManager.Instance.AbandonRun(); // 실제 종료와 결과 화면 진입
            yield return WaitFor(() => SceneManager.GetActiveScene().path == SceneFrontendContext.ResultsPath && Button("FrontendRetryButton") != null, "Separate result scene did not appear."); // 실제 결과 씬 확인
            GameObject titleObject = GameObject.Find("FrontendTitle"); // 실제 결과 제목 객체
            Text title = titleObject == null ? null : titleObject.GetComponent<Text>(); // 실제 결과 제목 문구
            Require(title != null && title.text == "패배" && UnityEngine.Object.FindFirstObjectByType<SnakeMovement>() == null, "Result screen retained gameplay or wrong outcome."); // 실제 결과와 전투 씬 분리 확인
            AutomatedSceneSeparationVerification.Finish(true, "Direct game entry, opaque preparation backgrounds, gameplay and separate results passed."); // 실제 전체 시험 성공
        }

        private static IEnumerator WaitFor(Func<bool> condition, string failure) // 실제 씬 조건 대기
        {
            float deadline = Time.realtimeSinceStartup + 12f; // 실제 씬 전환 제한 시간
            while (!condition() && Time.realtimeSinceStartup < deadline) // 실제 조건과 제한 시각 확인
            {
                yield return null; // 실제 화면 갱신 프레임
            }
            Require(condition(), failure); // 실제 완료 조건 확인
        }

        private static void RequireOpaqueBackground(string stage) // 실제 불투명 화면 배경 확인
        {
            GameObject panelObject = GameObject.Find("FrontendPanel"); // 실제 전체 화면 패널
            Image panel = panelObject == null ? null : panelObject.GetComponent<Image>(); // 실제 패널 배경
            GameObject imageObject = GameObject.Find("FrontendBackground"); // 실제 선택 화면 이미지
            RawImage image = imageObject == null ? null : imageObject.GetComponent<RawImage>(); // 실제 배경 이미지
            Require(panel != null && Mathf.Approximately(panel.color.a, 1f) && image != null && image.texture != null && Mathf.Approximately(image.color.a, 1f) && !image.raycastTarget, "Opaque artwork background missing at " + stage); // 실제 불투명 배경과 입력 보존 확인
        }

        private static Button Button(string name) // 실제 활성 버튼 조회
        {
            GameObject item = GameObject.Find(name); // 실제 화면 객체 검색
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedSceneSeparationVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
