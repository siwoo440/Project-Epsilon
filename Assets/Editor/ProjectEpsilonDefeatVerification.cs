using System; // 예외 형식
using System.Reflection; // 초기화 호출
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 체력
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDefeatVerification // 패배 경로 검증
    {
        public static void Run() // 검증 실행
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 게임 씬 열기
            GameManager manager = UnityEngine.Object.FindFirstObjectByType<GameManager>(); // 게임 관리자 조회
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 체력 관리자 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            if (manager == null || health == null || body == null) // 씬 구성 확인
            {
                throw new InvalidOperationException("패배 검증 대상 누락"); // 구성 실패 보고
            }
            try // 시간 배율 정리 보장
            {
                manager.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null); // 게임 초기화
                health.Configure(body, null, 100); // 피해 검증 설정
                health.TakeDamage(int.MaxValue); // 마지막 몸통까지 피해
                if (manager.CurrentState.ToString() != "Defeated") // 패배 전환 확인
                {
                    throw new InvalidOperationException("몸통 소진 후 패배 미전환"); // 검증 실패 보고
                }
                manager.ResumeGame(); // 일시정지 복귀 시도
                if (manager.CurrentState.ToString() != "Defeated") // 패배 유지 확인
                {
                    throw new InvalidOperationException("패배 상태에서 재개 허용"); // 검증 실패 보고
                }
            }
            finally // 시간 배율 복구
            {
                Time.timeScale = 1f; // 편집기 시간 복구
            }
            Debug.Log("몸통 소진 패배 전환 검증 완료"); // 검증 성공 표시
        }
    }
}
