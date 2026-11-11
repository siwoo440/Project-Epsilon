using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Recording; // 실제 선택적 기록기
using UnityEditor; // 실제 배치 제어
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 실제 Unity 객체

namespace ProjectEpsilon.Editor // 실제 시험 영역
{
    [InitializeOnLoad] // 실제 도메인 갱신
    public static class AutomatedDay50NoRecordingVerification // 기본 기록 비활성 확인
    {
        private const string Key = "EpsilonDay50NoRecord"; // 실제 시험 상태 키
        private static double started; // 실제 제한 시간 기준
        static AutomatedDay50NoRecordingVerification() // 실제 도메인 재시작
        {
            if (SessionState.GetBool(Key, false)) // 실제 진행 여부 확인
            {
                EditorApplication.update += Tick; // 실제 시험 감시 등록
            }
        }
        public static void Run() // 실제 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 상태 등록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시험 감시 등록
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 전투 시작 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 상태 확인
            {
                return; // 실제 플레이 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 제한 시간 확인
            {
                Finish(false, "Timed out."); // 실제 시간 초과
                return; // 실제 시험 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 메뉴와 시작 포탑 진행
            {
                return; // 실제 전투 진입 대기
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 상태 확인
            {
                return; // 실제 전투 대기
            }
            Finish(RunTelemetry.Instance == null, RunTelemetry.Instance == null ? "Default run has no recorder." : "Recorder was unexpectedly active."); // 실제 기본 기록 비활성 확인
        }
        private static void Finish(bool success, string message) // 실제 시험 결과 처리
        {
            SessionState.SetBool(Key, false); // 실제 시험 상태 해제
            EditorApplication.update -= Tick; // 실제 시험 감시 해제
            Debug.Log($"[Day50NoRecordingVerification] {(success ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(success ? 0 : 1); // 실제 종료 코드
        }
    }
}
