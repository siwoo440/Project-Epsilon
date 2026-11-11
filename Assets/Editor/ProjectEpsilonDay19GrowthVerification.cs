using System; // 예외 형식
using System.Reflection; // 초기화 호출
using ProjectEpsilon.Player; // 몸통 관리
using ProjectEpsilon.Progression; // 경험치 계산
using UnityEngine; // 유니티 객체

namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDay19GrowthVerification // 성장 규칙 검증
    {
        public static void Run() // 검증 실행
        {
            Require(SnakeExperience.CalculateRequiredExperience(10, 1.12f, 2) == 11, "2레벨 필요 경험치"); // 반올림 확인
            Require(SnakeExperience.CalculateRequiredExperience(10, 1.12f, 3) == 13, "3레벨 필요 경험치"); // 성장 계산 확인
            GameObject player = new GameObject("GrowthVerification"); // 임시 플레이어
            try // 정리 보장
            {
                SnakeBodyManager bodies = player.AddComponent<SnakeBodyManager>(); // 몸통 관리자
                bodies.Configure(null, null, null, 3, 30, 0.58f); // 초과 상한 설정
                typeof(SnakeBodyManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(bodies, null); // 시작 몸통 생성
                Require(bodies.MaximumBodyCount == 20, "절대 몸통 상한"); // 절대 상한 확인
                Require(bodies.CurrentBodyCount == 3, "시작 몸통 수"); // 시작 수 확인
                bodies.RemoveBodies(1); // 몸통 손실
                Require(bodies.TryGainBodyFromLevelUp(), "성장 실패"); // 성장 실행
                Require(bodies.CurrentBodyCount == 3 && bodies.EarnedBodyCount == 4, "손실 후 성장"); // 현재 수와 성장 상한 확인
                Debug.Log("[Project Epsilon] Day19 growth verification passed."); // 성공 기록
            }
            finally // 임시 객체 정리
            {
                UnityEngine.Object.DestroyImmediate(player); // 객체 제거
            }
        }

        private static void Require(bool condition, string message) // 조건 검증
        {
            if (!condition) // 실패 조건
            {
                throw new InvalidOperationException("Day19 growth verification failed: " + message); // 실패 보고
            }
        }
    }
}
