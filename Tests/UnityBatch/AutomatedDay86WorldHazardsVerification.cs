using System; // 예외 처리
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.World; // 실제 맵 관리자
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay86WorldHazardsVerification // 맵 위험 요소 연결 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay86WorldHazards"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay86WorldHazardsVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 맵 위험 연결 시험
            { // 시험 시작
                PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
                Type controllerType = typeof(PrototypeChunkMap).Assembly.GetType("ProjectEpsilon.World.WorldEventController"); // 실제 맵 위험 관리 형식
                Require(map != null && controllerType != null && map.GetComponent(controllerType) != null, "맵 위험 관리자 연결 누락"); // 실제 청크 관리 연결 확인
                Require(map.Profile != null && controllerType.GetMethod("TryBeginLocust") != null && controllerType.GetMethod("TryBeginRockfall") != null, "황충떼 또는 낙석 시작 경로 누락"); // 두 위험 요소의 실제 경로 확인
                Finish(0, "World hazard controller and both event paths are present."); // 성공 결과
            } // 시험 종료
            catch (Exception error) // 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실제 실패 기록
            } // 실패 종료
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day86WorldHazardsVerification] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
