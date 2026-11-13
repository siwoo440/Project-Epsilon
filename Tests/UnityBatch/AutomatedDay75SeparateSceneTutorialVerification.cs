using System; // 예외 처리
using ProjectEpsilon.Core; // 전투 상태
using ProjectEpsilon.UI; // 실제 화면 흐름
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.SceneManagement; // 실제 씬 확인
using UnityEngine.UI; // 실제 버튼 입력

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay75SeparateSceneTutorialVerification // 독립 씬 첫 안내 시험
    { // 시험 시작
        private const string Key = "EpsilonDay75SeparateSceneTutorial"; // 시험 진행 표식
        private static bool injected; // 중복 실행 방지
        static AutomatedDay75SeparateSceneTutorialVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실행기 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            if (Application.productName != "Project-Epsilon-Day75-Flow-Isolated") // 독립 저장 공간 확인
            { // 조건 시작
                throw new InvalidOperationException("독립 씬 시험 저장 공간이 격리되지 않음"); // 사용자 저장 보호
            } // 조건 종료
            PlayerPrefs.DeleteKey("ProjectEpsilon.TutorialCompleted.v1"); // 격리된 첫 플레이 상태
            PlayerPrefs.Save(); // 격리 설정 저장
            EditorSceneManager.OpenScene(SceneFrontendContext.GamePath); // 실제 직접 실행 경로
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실행기 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 플레이 진입 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || injected) // 플레이와 중복 확인
            { // 조건 시작
                return; // 실행 준비 대기
            } // 조건 종료
            injected = true; // 실행기 중복 방지
            GameObject runner = new GameObject("Day75SeparateSceneTutorialRunner"); // 실제 씬 이동 시험기
            UnityEngine.Object.DontDestroyOnLoad(runner); // 씬 이동 중 시험 유지
            runner.AddComponent<Day75SeparateSceneTutorialRunner>(); // 실제 화면 흐름 시작
        } // 함수 종료
        public static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 실행 감시 해제
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day75SeparateSceneTutorial] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day75SeparateSceneTutorial] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class Day75SeparateSceneTutorialRunner : MonoBehaviour // 실제 씬 흐름 시험기
    { // 실행기 시작
        private int phase; // 현재 화면 단계
        private float deadline; // 전체 시험 제한 시각
        private void Start() // 시험 제한 시각 설정
        { // 함수 시작
            deadline = Time.realtimeSinceStartup + 45f; // 실제 시간 기준 제한
        } // 함수 종료
        private void Update() // 실제 메뉴와 전투 흐름
        { // 함수 시작
            try // 실제 화면 단계 실행
            { // 실행 시작
                if (Time.realtimeSinceStartup >= deadline) // 제한 시간 확인
                { // 조건 시작
                    throw new InvalidOperationException("독립 씬 안내 시간 초과: " + phase); // 실제 대기 단계 기록
                } // 조건 종료
                if (phase == 0 && SceneManager.GetActiveScene().path == SceneFrontendContext.MainMenuPath && Click("FrontendStartButton")) // 실제 메인 메뉴 시작
                { // 조건 시작
                    phase = 1; // 캐릭터 선택 대기
                } // 조건 종료
                else if (phase == 1 && SceneManager.GetActiveScene().path == SceneFrontendContext.PreparationPath && Click("FrontendCharactersNextButton")) // 실제 캐릭터 선택
                { // 조건 시작
                    phase = 2; // 맵 선택 대기
                } // 조건 종료
                else if (phase == 2 && Click("FrontendMapsNextButton")) // 실제 맵 선택
                { // 조건 시작
                    phase = 3; // 포탑 선택 대기
                } // 조건 종료
                else if (phase == 3 && Click("FrontendStartingWeaponButton0")) // 실제 시작 포탑 선택
                { // 조건 시작
                    phase = 4; // 첫 전투 안내 대기
                } // 조건 종료
                else if (phase == 4 && SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && RunFrontendController.Instance != null && RunFrontendController.Instance.Stage.ToString() == "Tutorial") // 실제 전투 안내 확인
                { // 조건 시작
                    if (GameManager.Instance == null || !GameManager.Instance.IsPaused || !Click("FrontendTutorialSkipButton")) // 안내 정지와 건너뛰기 확인
                    { // 조건 시작
                        throw new InvalidOperationException("첫 전투 안내 또는 건너뛰기 실패"); // 실제 실패 원인
                    } // 조건 종료
                    phase = 5; // 전투 재개 대기
                } // 조건 종료
                else if (phase == 5 && GameManager.Instance != null && GameManager.Instance.IsPlaying && RunFrontendController.Instance.Stage.ToString() == "Playing") // 실제 전투 재개 확인
                { // 조건 시작
                    if (PlayerPrefs.GetInt("ProjectEpsilon.TutorialCompleted.v1", 0) != 1) // 완료 저장 확인
                    { // 조건 시작
                        throw new InvalidOperationException("독립 씬 안내 완료 저장 실패"); // 실제 저장 실패
                    } // 조건 종료
                    AutomatedDay75SeparateSceneTutorialVerification.Finish(0, "Main menu, preparation, first-run tutorial and combat transition passed."); // 실제 흐름 성공
                } // 조건 종료
            } // 실행 종료
            catch (Exception error) // 실제 흐름 오류 확인
            { // 실패 시작
                AutomatedDay75SeparateSceneTutorialVerification.Finish(1, error.ToString()); // 실패 원인 기록
            } // 실패 종료
        } // 함수 종료
        private static bool Click(string name) // 활성 버튼 선택
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 실제 버튼 객체
            Button button = item == null ? null : item.GetComponent<Button>(); // 실제 버튼
            if (button == null || !button.interactable) // 입력 가능 여부 확인
            { // 조건 시작
                return false; // 화면 구성 대기
            } // 조건 종료
            button.onClick.Invoke(); // 실제 버튼 동작
            return true; // 선택 완료 반환
        } // 함수 종료
    } // 실행기 종료
} // 영역 종료
