using System; // 예외 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.UI; // 실제 화면 흐름
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 전투 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.InputSystem; // 가상 키보드
using UnityEngine.InputSystem.LowLevel; // 실제 키 상태
using UnityEngine.UI; // 실제 버튼과 안내

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay75TutorialVerification // 첫 플레이 안내 시험
    { // 시험 시작
        private const string Key = "EpsilonDay75TutorialVerification"; // 시험 진행 표식
        private const string TutorialKey = "ProjectEpsilon.TutorialCompleted.v1"; // 실제 안내 완료 표식
        private static double started; // 시작 대기 시각
        private static bool injected; // 중복 실행 방지
        static AutomatedDay75TutorialVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            if (Application.productName != "Project-Epsilon-Day75-Tutorial-Isolated") // 독립 저장 공간 확인
            { // 조건 시작
                throw new InvalidOperationException("튜토리얼 시험 저장 공간이 격리되지 않음"); // 사용자 저장 보호
            } // 조건 종료
            PlayerPrefs.DeleteKey(TutorialKey); // 격리된 첫 플레이 상태
            PlayerPrefs.Save(); // 격리 설정 저장
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 튜토리얼 시작 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || injected) // 플레이와 중복 확인
            { // 조건 시작
                return; // 시작 준비 대기
            } // 조건 종료
            if (started <= 0d) // 제한 시간 기준 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각
            } // 조건 종료
            if (EditorApplication.timeSinceStartup - started > 20d) // 첫 안내 대기 제한
            { // 조건 시작
                Finish(1, "첫 전투의 튜토리얼이 열리지 않음"); // 안내 누락 기록
                return; // 추가 실행 방지
            } // 조건 종료
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Starting) // 실제 시작 단계 확인
            { // 조건 시작
                StartingWeaponBatchHelper.HandleStartingSelection(); // 기존 시작 포탑 선택
                return; // 다음 프레임 대기
            } // 조건 종료
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 화면 관리자
            if (frontend == null || frontend.Stage.ToString() != "Tutorial" || GameManager.Instance == null || !GameManager.Instance.IsPaused) // 첫 안내와 정지 확인
            { // 조건 시작
                return; // 안내 시작 대기
            } // 조건 종료
            injected = true; // 중복 시험 방지
            new GameObject("Day75TutorialRunner").AddComponent<Day75TutorialRunner>(); // 실제 화면 시험 실행
        } // 함수 종료
        public static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day75Tutorial] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day75Tutorial] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class Day75TutorialRunner : MonoBehaviour // 실제 안내 화면 시험기
    { // 실행기 시작
        private IEnumerator Start() // 예외를 포함한 화면 흐름
        { // 함수 시작
            IEnumerator steps = RunSteps(); // 실제 시험 단계
            while (true) // 실제 단계 순회
            { // 반복 시작
                object current; // 현재 대기 값
                try // 실제 단계 실행
                { // 실행 시작
                    if (!steps.MoveNext()) // 시험 완료 확인
                    { // 조건 시작
                        break; // 반복 종료
                    } // 조건 종료
                    current = steps.Current; // 현재 대기 값 저장
                } // 실행 종료
                catch (Exception error) // 실제 시험 오류 확인
                { // 실패 시작
                    AutomatedDay75TutorialVerification.Finish(1, error.ToString()); // 실패 원인 기록
                    yield break; // 후속 시험 중단
                } // 실패 종료
                yield return current; // 실제 프레임 대기
            } // 반복 종료
            AutomatedDay75TutorialVerification.Finish(0, "First-run pages, completion, saved state, replay, ESC and pause ownership passed."); // 성공 결과 기록
        } // 함수 종료
        private IEnumerator RunSteps() // 실제 안내와 설정 흐름
        { // 함수 시작
            Application.runInBackground = true; // 비대화형 입력 허용
            InputSettings inputSettings = Instantiate(InputSystem.settings); // 격리된 입력 설정
            inputSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 입력 전달
            inputSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = inputSettings; // 실제 시험 입력 설정
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>(); // 실제 가상 키보드
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 화면 관리자
            Require(frontend != null && GameManager.Instance.IsPaused && Time.timeScale == 0f, "첫 안내 일시정지 실패"); // 첫 안내 정지 확인
            for (int index = 0; index < 8; index++) // 기획서 여덟 단계 순회
            { // 반복 시작
                Text page = FindText("FrontendTutorialBody"); // 실제 안내 내용
                Require(page != null && !string.IsNullOrWhiteSpace(page.text), "안내 내용 누락: " + index); // 실제 안내 문구 확인
                if (index == 0 && InputBindingManager.Instance != null) // 실제 첫 조작 안내 확인
                { // 조건 시작
                    string left = InputBindingManager.Instance.GetBindingDisplayName(RebindableInput.TurnLeft); // 현재 왼쪽 키 이름
                    Require(page.text.Contains(left) && page.text.Contains("방향키"), "실제 회전 키 안내 불일치"); // 실제 키와 방향키 안내 확인
                } // 조건 종료
                if (index == 5) // 실제 합성 안내 확인
                { // 조건 시작
                    Require(page.text.Contains("같은 등급") && page.text.Contains("E 키") && page.text.Contains("즉시"), "즉시 합성 규칙 안내 불일치"); // 현재 합성 규칙 확인
                } // 조건 종료
                if (index == 7) // 실제 최종 안내 확인
                { // 조건 시작
                    Require(page.text.Contains("20분") && page.text.Contains("최종 보스"), "최종 보스 시간 안내 불일치"); // 확정 승리 조건 확인
                } // 조건 종료
                Button next = FindButton("FrontendTutorialNextButton"); // 실제 다음 단계 버튼
                Require(next != null && next.interactable, "안내 단계 버튼 누락: " + index); // 실제 단계 진행 확인
                next.onClick.Invoke(); // 실제 다음 단계 선택
                yield return null; // 화면 교체 대기
            } // 반복 종료
            Require(frontend.Stage.ToString() == "Playing" && GameManager.Instance.IsPlaying && PlayerPrefs.GetInt("ProjectEpsilon.TutorialCompleted.v1", 0) == 1, "첫 안내 완료 또는 저장 실패"); // 자동 안내 완료 확인
            yield return Tap(keyboard, Key.Escape); // 실제 일시정지 입력
            Require(frontend.Stage.ToString() == "Paused" && GameManager.Instance.IsPaused, "기존 일시정지 진입 실패"); // 기존 수동 일시정지 확인
            FindButton("FrontendPauseSettingsButton").onClick.Invoke(); // 실제 설정 진입
            yield return null; // 설정 화면 대기
            Button replay = FindButton("FrontendTutorialReplayButton"); // 실제 안내 다시 보기
            Require(replay != null && replay.interactable, "설정의 다시 보기 누락"); // 실제 다시 보기 확인
            replay.onClick.Invoke(); // 실제 안내 다시 열기
            yield return null; // 안내 화면 대기
            Require(frontend.Stage.ToString() == "Tutorial" && GameManager.Instance.IsPaused, "다시 보기의 정지 상태 실패"); // 실제 안내와 정지 확인
            FindButton("FrontendTutorialSkipButton").onClick.Invoke(); // 실제 안내 건너뛰기 버튼
            yield return null; // 설정 화면 복귀 대기
            Require(frontend.Stage.ToString() == "Settings" && GameManager.Instance.IsPaused, "건너뛰기가 기존 일시정지를 해제함"); // 건너뛰기 정지 유지 확인
            FindButton("FrontendTutorialReplayButton").onClick.Invoke(); // 실제 안내 다시 열기
            yield return null; // 안내 화면 대기
            yield return Tap(keyboard, Key.Escape); // 실제 안내 건너뛰기 입력
            Require(frontend.Stage.ToString() == "Settings" && GameManager.Instance.IsPaused, "안내 취소가 기존 일시정지를 해제함"); // 설정과 정지 소유 확인
            FindButton("FrontendSettingsBackButton").onClick.Invoke(); // 기존 일시정지 화면 복귀
            yield return null; // 화면 전환 대기
            FindButton("FrontendResumeButton").onClick.Invoke(); // 실제 전투 재개
            yield return null; // 실제 전투 전환 대기
            Require(GameManager.Instance.IsPlaying && frontend.Stage.ToString() == "Playing", "안내 이후 전투 재개 실패"); // 실제 전투 복귀 확인
            InputSystem.RemoveDevice(keyboard); // 가상 키보드 정리
        } // 함수 종료
        private static IEnumerator Tap(Keyboard keyboard, Key key) // 실제 키 입력
        { // 함수 시작
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 키 누름 전달
            yield return null; // 실제 입력 프레임 대기
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 키 해제 전달
            yield return null; // 실제 해제 프레임 대기
        } // 함수 종료
        private static Button FindButton(string name) // 실제 버튼 조회
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 활성 버튼 객체
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        } // 함수 종료
        private static Text FindText(string name) // 실제 안내 글자 조회
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 활성 글자 객체
            return item == null ? null : item.GetComponent<Text>(); // 실제 글자 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
    } // 실행기 종료
} // 영역 종료
