using System; // 예외 형식
using ProjectEpsilon.Player; // 몸통 충돌 판정
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    public static class ProjectEpsilonEliteCutVerification // 엘리트 절단 검증
    { // 클래스 시작
        public static void Run() // 검증 실행
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 게임 씬 열기
            SnakeExternalCollision collision = UnityEngine.Object.FindFirstObjectByType<SnakeExternalCollision>(); // 적 충돌 판정 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            SnakeHealth health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 체력 관리자 조회
            SnakeInvulnerability immunity = UnityEngine.Object.FindFirstObjectByType<SnakeInvulnerability>(); // 전체 무적 조회
            if (collision == null || body == null || health == null || immunity == null || body.CurrentBodyCount < 3) // 검증 구성 확인
            { // 조건 시작
                throw new InvalidOperationException("엘리트 절단 검증 구성 누락"); // 구성 실패 보고
            } // 조건 끝
            collision.Bind(body, health, immunity, 1, 1f, 25, 1.5f); // 적 직접 충돌 설정
            immunity.ClearInvulnerability(); // 전체 무적 초기화
            int initial = body.CurrentBodyCount; // 초기 몸통 수 저장
            if (!collision.TryApplyEnemyDirectContact()) // 일반 직접 충돌 실행
            { // 조건 시작
                throw new InvalidOperationException("일반 직접 충돌 실패"); // 검증 실패 보고
            } // 조건 끝
            if (!collision.EnemyContactProtected || !collision.TryApplyEliteCut() || body.CurrentBodyCount != initial - 2) // 보호 중 독립 절단 확인
            { // 조건 시작
                throw new InvalidOperationException("일반 충돌 보호가 엘리트 절단을 차단"); // 검증 실패 보고
            } // 조건 끝
            immunity.StartInvulnerability(10f); // 전체 무적 적용
            if (collision.TryApplyEliteCut() || body.CurrentBodyCount != initial - 2) // 전체 무적 우선 확인
            { // 조건 시작
                throw new InvalidOperationException("전체 무적 중 엘리트 절단 허용"); // 검증 실패 보고
            } // 조건 끝
            Debug.Log("엘리트 절단 보호 판정 검증 완료"); // 검증 성공 표시
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
