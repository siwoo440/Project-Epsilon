using System; // 시험 예외 처리
using System.Reflection; // 실제 조종기 설정 확인
using UnityEditor; // Unity 배치 제어
using UnityEngine; // Unity 로그 출력

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 플레이 진입 후 시험 재연결
    public static class AutomatedTwentyMinuteTargetVerification // 실제 20분 목표 인수 검증
    {
        private const string ActiveKey = "EpsilonTwentyMinuteTargetVerification"; // 진행 중인 시험 표식
        private static double started; // 시험 시작 시각

        static AutomatedTwentyMinuteTargetVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 상태 확인
            {
                EditorApplication.update += Observe; // 실제 조종기 초기화 관찰
            }
        }

        public static void Run() // 실제 20분 목표 시험 시작
        {
            SessionState.SetBool(ActiveKey, true); // 시험 진행 상태 기록
            EditorApplication.update -= Observe; // 중복 관찰 해제
            EditorApplication.update += Observe; // 실제 조종기 초기화 관찰
            AutomatedNaturalCombatVerification.Run(); // 실제 자연 전투 조종기 시작
        }

        private static void Observe() // 실제 배치 인수 적용 확인
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 플레이 진입 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 시험 제한 시간 확인
            {
                Finish(false, "Natural pilot did not initialize."); // 초기화 실패 기록
                return; // 후속 검사 중단
            }
            BindingFlags fields = BindingFlags.Static | BindingFlags.NonPublic; // 실제 조종기 내부 필드 범위
            Type pilot = typeof(AutomatedNaturalCombatVerification); // 실제 자연 전투 조종기 형식
            FieldInfo initialized = pilot.GetField("randomSeedInitialized", fields); // 실제 배치 인수 적용 여부
            FieldInfo target = pilot.GetField("targetSurvivalSeconds", fields); // 실제 생존 관찰 목표
            if (initialized == null || target == null) // 실제 시험 구성 확인
            {
                Finish(false, "Natural pilot fields are missing."); // 구성 누락 기록
                return; // 후속 검사 중단
            }
            if (!(bool)initialized.GetValue(null)) // 실제 인수 적용 대기
            {
                return; // 다음 갱신 대기
            }
            float seconds = (float)target.GetValue(null); // 실제 적용된 목표 시간
            Finish(Mathf.Approximately(seconds, 1200f), "Requested 1200-second target; actual=" + seconds); // 실제 인수 적용 결과
        }

        private static void Finish(bool passed, string message) // 시험 결과 기록과 종료
        {
            SessionState.SetBool(ActiveKey, false); // 시험 진행 표식 제거
            EditorApplication.update -= Observe; // 시험 관찰 해제
            Debug.Log("[TwentyMinuteTargetVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 실제 결과 종료 코드 반환
        }
    }
}
