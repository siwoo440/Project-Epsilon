using System; // 실제 형식 조회
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 조종기와 시간 필드 조회
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 출현 시계
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Recording; // 실제 전투 기록
using UnityEditor; // Unity 배치 제어
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 플레이 진입 후 감시 복구
    public static class AutomatedSurvivalMilestonesVerification // 실제 전투 표본 경계 검증
    {
        private const string ActiveKey = "EpsilonSurvivalMilestonesVerification"; // 시험 진행 표식
        private static bool injected; // 실제 시험 실행기 생성 여부
        private static double started; // 실제 시험 시작 시각

        static AutomatedSurvivalMilestonesVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 상태 확인
            {
                EditorApplication.update += Observe; // 실제 플레이 시작 관찰
            }
        }

        public static void Run() // 실제 조종기와 시험 시작
        {
            SessionState.SetBool(ActiveKey, true); // 시험 진행 상태 기록
            EditorApplication.update -= Observe; // 중복 관찰 해제
            EditorApplication.update += Observe; // 실제 플레이 시작 관찰
            AutomatedNaturalCombatVerification.Run(); // 실제 자연 전투 조종기 시작
        }

        private static void Observe() // 실제 플레이와 기록기 초기화 확인
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 실제 플레이 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 시험 제한 시간 확인
            {
                Fail("Timed out waiting for milestones."); // 초기화 실패 기록
                return; // 추가 처리 중단
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (injected || player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || RunTelemetry.Instance == null || RunTelemetry.Instance.Current == null) // 실제 전투와 기록 준비 확인
            {
                return; // 초기화 또는 시험 결과 대기
            }
            injected = true; // 시험 실행기 중복 생성 방지
            player.gameObject.AddComponent<SurvivalMilestonesVerificationRunner>(); // 실제 시계 경계 통제 시작
        }

        public static void Fail(string message) // 통제 시험 실패 기록
        {
            SessionState.SetBool(ActiveKey, false); // 시험 진행 표식 제거
            EditorApplication.update -= Observe; // 관찰 해제
            Debug.Log("[SurvivalMilestonesVerification] FAIL: " + message); // 실패 사유 기록
            EditorApplication.Exit(1); // 실제 실패 종료 코드
        }
    }

    public sealed class SurvivalMilestonesVerificationRunner : MonoBehaviour // 실제 전투 기록 경계 통제
    {
        private static readonly BindingFlags Fields = BindingFlags.Static | BindingFlags.NonPublic; // 실제 조종기 내부 필드 범위

        private IEnumerator Start() // 실제 시간 경계와 표본 관찰
        {
            Type pilot = typeof(AutomatedNaturalCombatVerification); // 실제 자연 조종기 형식
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현 시계
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 출현 경과 시간
            FieldInfo progress = pilot.GetField("naturalProgressObserved", Fields); // 실제 자연 성장 확인 상태
            FieldInfo elite = pilot.GetField("sawElite", Fields); // 실제 엘리트 관찰 상태
            if (spawner == null || elapsed == null || progress == null || elite == null) // 실제 시험 구성 확인
            {
                AutomatedSurvivalMilestonesVerification.Fail("Actual pilot fixtures missing."); // 구성 누락 기록
                yield break; // 시험 중단
            }
            progress.SetValue(null, true); // 시간 경계 시험용 성장 조건 통제
            elite.SetValue(null, true); // 시간 경계 시험용 엘리트 조건 통제
            string[] flags = { "fiveMinuteCaptured", "tenMinuteCaptured", "fifteenMinuteCaptured" }; // 실제 구간별 표본 완료 필드
            float[] times = { 299.95f, 599.95f, 899.95f }; // 실제 구간 직전 강제 시각
            for (int index = 0; index < flags.Length; index++) // 실제 앞선 세 경계 순회
            {
                FieldInfo captured = pilot.GetField(flags[index], Fields); // 실제 표본 완료 필드
                if (captured == null) // 실제 필드 존재 확인
                {
                    AutomatedSurvivalMilestonesVerification.Fail("Missing milestone " + flags[index]); // 필드 누락 기록
                    yield break; // 시험 중단
                }
                elapsed.SetValue(spawner, times[index]); // 실제 출현 시계 경계 직전 통제
                float deadline = Time.realtimeSinceStartup + 5f; // 실제 표본 기록 시간 제한
                while (!(bool)captured.GetValue(null) && Time.realtimeSinceStartup < deadline) // 실제 표본 기록 대기
                {
                    yield return null; // 실제 게임 프레임 진행
                }
                if (!(bool)captured.GetValue(null)) // 실제 표본 기록 확인
                {
                    AutomatedSurvivalMilestonesVerification.Fail("Milestone not captured: " + flags[index]); // 표본 누락 기록
                    yield break; // 시험 중단
                }
            }
            SessionState.SetBool("EpsilonSurvivalMilestonesVerification", false); // 시험 감시 자동 재시작 방지
            Debug.Log("[SurvivalMilestonesVerification] Controlled clock jumps reached 5, 10 and 15 minutes; awaiting real 20-minute transition and pilot record."); // 통제된 시험 조건 기록
            elapsed.SetValue(spawner, 1199.95f); // 실제 보스 전환 경계 직전 통제
        }
    }
}
