using System; // 실제 예외 형식
using System.Reflection; // 실제 실패 처리 조회
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Recording; // 실제 전투 기록
using UnityEditor; // Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedNaturalFailureRecordVerification // 실패 시 부분 기록 검증
    {
        private const string ActiveKey = "EpsilonNaturalFailureRecordVerification"; // 시험 진행 표식
        private static double started; // 실제 시작 시각

        static AutomatedNaturalFailureRecordVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 진행 상태 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시 연결
            }
        }

        public static void Run() // 실제 Unity 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }

        private static void Tick() // 실제 게임과 기록 초기화 확인
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 실제 시험 시작 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 실제 시험 시간 제한
            {
                Debug.LogError("[NaturalFailureRecordVerification] Setup timed out."); // 실제 준비 실패 기록
                EditorApplication.Exit(2); // 준비 실패 종료
                return; // 추가 처리 차단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 선택 진행
            {
                return; // 실제 전투 시작 대기
            }
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || RunTelemetry.Instance == null || RunTelemetry.Instance.Current == null) // 실제 기록과 전투 상태 확인
            {
                return; // 실제 기록 초기화 대기
            }
            SessionState.SetBool(ActiveKey, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 중복 실패 호출 차단
            MethodInfo failure = typeof(AutomatedNaturalCombatVerification).GetMethod("Fail", BindingFlags.Static | BindingFlags.NonPublic); // 실제 조종기 실패 처리
            if (failure == null) // 실제 실패 처리 경로 확인
            {
                Debug.LogError("[NaturalFailureRecordVerification] Failure handler missing."); // 실제 구성 실패 기록
                EditorApplication.Exit(2); // 구성 오류 종료
                return; // 추가 처리 차단
            }
            Debug.Log("[NaturalFailureRecordVerification] Triggering controlled failure; expected exit 1."); // 인위적 실패 조건 기록
            failure.Invoke(null, new object[] { "controlled failure record verification" }); // 실제 실패와 부분 기록 처리
        }
    }
}
