using System; // 예외 처리
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.UI; // 실제 화면 흐름
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 전투 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay75TutorialPersistenceVerification // 안내 저장 복원 시험
    { // 시험 시작
        private const string Key = "EpsilonDay75TutorialPersistence"; // 시험 진행 표식
        private static double deadline; // 실제 전투 확인 시각
        private static double started; // 전체 시험 시작 시각
        static AutomatedDay75TutorialPersistenceVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 준비 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            if (Application.productName != "Project-Epsilon-Day75-Tutorial-Isolated" || PlayerPrefs.GetInt("ProjectEpsilon.TutorialCompleted.v1", 0) != 1) // 격리 저장과 이전 완료 확인
            { // 조건 시작
                throw new InvalidOperationException("격리된 이전 안내 완료 기록이 없음"); // 잘못된 시험 환경 차단
            } // 조건 종료
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 전투 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 준비 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 재시작 후 안내 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 시작 대기
            } // 조건 종료
            if (started <= 0d) // 제한 시간 기준 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각
            } // 조건 종료
            if (EditorApplication.timeSinceStartup - started > 20d) // 전투 진입 제한
            { // 조건 시작
                Finish(1, "저장 후 전투 진입 시간 초과"); // 실패 결과 기록
                return; // 추가 처리 방지
            } // 조건 종료
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Starting) // 시작 단계 확인
            { // 조건 시작
                StartingWeaponBatchHelper.HandleStartingSelection(); // 기존 시작 포탑 선택
                return; // 다음 프레임 대기
            } // 조건 종료
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 화면 관리자
            if (frontend == null || GameManager.Instance == null) // 전투 구성 확인
            { // 조건 시작
                return; // 실제 구성 대기
            } // 조건 종료
            if (frontend.Stage.ToString() == "Tutorial") // 자동 안내 재표시 확인
            { // 조건 시작
                Finish(1, "완료된 안내가 다시 자동 표시됨"); // 중복 안내 실패 기록
                return; // 추가 처리 방지
            } // 조건 종료
            if (!GameManager.Instance.IsPlaying || frontend.Stage.ToString() != "Playing") // 전투 상태 확인
            { // 조건 시작
                return; // 실제 전투 대기
            } // 조건 종료
            if (deadline <= 0d) // 전투 유지 검사 시작
            { // 조건 시작
                deadline = EditorApplication.timeSinceStartup + 1d; // 실제 전투 유지 시간
            } // 조건 종료
            if (EditorApplication.timeSinceStartup >= deadline) // 전투 유지 시간 확인
            { // 조건 시작
                Finish(0, "Saved tutorial completion prevented automatic replay after restart."); // 성공 결과 기록
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 처리
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 전투 감시 해제
            if (code == 0) // 성공 여부 확인
            { // 조건 시작
                Debug.Log("[Day75TutorialPersistence] PASS: " + message); // 성공 결과 기록
            } // 조건 종료
            else // 실패 여부 확인
            { // 조건 시작
                Debug.LogError("[Day75TutorialPersistence] FAIL: " + message); // 실패 결과 기록
            } // 조건 종료
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 시험 종료
} // 영역 종료
