using System; // 예외 형식
using ProjectEpsilon.Enemies; // 적 출현 규칙
using UnityEngine; // 좌표 형식

namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonEnemyDespawnVerification // 적 정리 거리 검증
    {
        public static void Run() // 검증 실행
        {
            Vector3 player = Vector3.zero; // 플레이어 기준 위치
            Require(!EarlyEnemyWaveSpawner.ShouldDespawn(new Vector3(21f, 0f, 0f), player, 22f, false), "거리 이내 일반 적 제거"); // 거리 이내 적 유지 확인
            Require(EarlyEnemyWaveSpawner.ShouldDespawn(new Vector3(23f, 0f, 0f), player, 22f, false), "거리 밖 일반 적 유지"); // 먼 일반 적 제거 확인
            Require(!EarlyEnemyWaveSpawner.ShouldDespawn(new Vector3(100f, 0f, 0f), player, 22f, true), "먼 엘리트 제거"); // 엘리트 유지 확인
            Debug.Log("초반 적 정리 거리 검증 완료"); // 검증 성공 표시
        }
        private static void Require(bool condition, string message) // 검증 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                throw new InvalidOperationException(message); // 검증 실패 보고
            }
        }
    }
}
