using System; // 예외 처리
using System.Reflection; // 실제 냉기 해제 확인
using ProjectEpsilon.Combat; // 실제 전투 상태
using UnityEditor; // Unity 배치 실행
using UnityEngine; // Unity 구성 요소

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    public static class AutomatedColdStatusConsumptionVerification // 냉기 재료 소비 검증
    {
        public static void Run() // 실제 Unity 배치 진입
        {
            try // 실제 상태 경계 검사
            {
                GameObject item = new GameObject("ColdConsumptionTarget"); // 실제 대상 생성
                WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 전투 대상
                WeaponTargetStatusController statuses = item.AddComponent<WeaponTargetStatusController>(); // 실제 이동 상태
                WeaponTargetColdStatus cold = item.AddComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태
                MethodInfo clear = typeof(WeaponTargetColdStatus).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance); // 냉기 전용 소비 경로 조회
                Require(clear != null, "Cold status has no source-specific consumption."); // 냉기 소비 경로 확인
                Require(cold.Apply(target, 2), "Cold status could not be applied."); // 실제 냉기 감속 적용
                Require(statuses.ApplyStop(9, 5f), "Independent stop could not be applied."); // 독립 정지 적용
                clear.Invoke(cold, null); // 실제 냉기 소비
                Require(!statuses.IsSlowActive && statuses.IsStopActive, "Cold consumption removed an independent stop or retained cold slow."); // 독립 상태 보존 확인
                statuses.ClearAll(); // 다음 경계 초기화
                Require(cold.Apply(target, 2), "Cold status could not be reapplied."); // 냉기 재적용
                Require(statuses.ApplySlow(9, 0.2f, 5f), "Independent replacement slow could not be applied."); // 높은 우선순위의 다른 감속 적용
                clear.Invoke(cold, null); // 실제 냉기 소비
                Require(statuses.IsSlowActive && Mathf.Approximately(statuses.CurrentMovementMultiplier, 0.2f), "Cold consumption removed an independent replacement slow."); // 다른 원인의 감속 보존 확인
                statuses.ClearAll(); // 다음 경계 초기화
                Require(cold.Apply(target, 6), "Cold freeze setup failed."); // 첫 냉기 적용
                Require(cold.Apply(target, 6) && cold.Apply(target, 6) && cold.Apply(target, 6) && statuses.IsStopActive, "Actual cold freeze did not activate."); // 실제 누적 빙결 확인
                clear.Invoke(cold, null); // 빙결 상태 소비
                Require(!statuses.IsStopActive && !statuses.IsSlowActive, "Cold consumption retained cold freeze or slow."); // 냉기 빙결과 감속 해제 확인
                statuses.ConfigureColdFreezeResistance(true, 0.5f); // 실제 보스 빙결 저항 설정
                Require(cold.Apply(target, 6) && cold.Apply(target, 6) && cold.Apply(target, 6) && cold.Apply(target, 6), "Resistant cold setup failed."); // 실제 저항 냉기 적용
                Require(statuses.IsColdResistanceSlowActive && !statuses.IsStopActive, "Resistant target was frozen or lacked cold resistance slow."); // 보스 저항 감속 확인
                clear.Invoke(cold, null); // 보스 냉기 상태 소비
                Require(!statuses.IsColdResistanceSlowActive && !statuses.IsStopActive && !statuses.IsSlowActive, "Cold consumption retained resistant-target slow."); // 냉기 저항 감속 해제 확인
                Debug.Log("[ColdStatusConsumptionVerification] PASS: Cold consumption preserved unrelated movement effects."); // 실제 성공 기록
                UnityEngine.Object.DestroyImmediate(item); // 시험 대상 제거
                EditorApplication.Exit(0); // 성공 종료
            }
            catch (Exception exception) // 실패 상황 확인
            {
                Debug.LogError("[ColdStatusConsumptionVerification] FAIL: " + exception.Message); // 실제 실패 기록
                EditorApplication.Exit(1); // 실패 종료
            }
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 실패 원인 전달
            }
        }
    }
}
