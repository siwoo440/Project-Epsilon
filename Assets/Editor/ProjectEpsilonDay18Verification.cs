using System; // 예외 형식 사용
using ProjectEpsilon.Combat; // Dark 규칙 사용
using ProjectEpsilon.Player; // 보호막 컴포넌트 사용
using UnityEditor; // 편집기 메뉴 사용
using UnityEngine; // Unity 오브젝트 사용

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 네임스페이스 시작
    public static class ProjectEpsilonDay18Verification // Day18 통합 검증기
    { // 클래스 시작
        [MenuItem("Project Epsilon/Day 18/Run Verification")] // 수동 검증 메뉴
        public static void Run() // 전체 검증 실행
        { // 메서드 시작
            VerifyShieldBreakEventOrder(); // 보호막 파괴 순서 검증
            VerifyDarkCursedKillRule(); // Dark 저주 처치 조건 검증
            Debug.Log("[Project Epsilon] Day18 verification passed."); // 성공 로그 출력
        } // 메서드 끝

        private static void VerifyShieldBreakEventOrder() // 보호막 종료 순서 검증
        { // 메서드 시작
            GameObject testObject = new GameObject("Day18ShieldVerification"); // 임시 검증 오브젝트 생성

            try // 임시 오브젝트 정리 보장
            { // 예외 보호 시작
                SnakeShieldController shield = testObject.AddComponent<SnakeShieldController>(); // 보호막 컴포넌트 추가
                int endedCount = 0; // 종료 이벤트 수 초기화
                shield.ShieldEnded += context => endedCount++; // 종료 이벤트 수집
                bool applied = shield.Apply(15, 3f, 8, 20f); // 8단계 보호막 적용
                int healthDamage = shield.Absorb(20); // 초과 피해 흡수
                Require(applied, "보호막 적용 실패"); // 적용 결과 확인
                Require(healthDamage == 5, "보호막 초과 피해 계산 오류"); // 잔여 피해 확인
                Require(endedCount == 0, "체력 피해 전 보호막 종료 발생"); // 조기 종료 차단 확인
                shield.CompleteDamageResolution(); // 체력 피해 이후 종료 완료
                Require(endedCount == 1, "체력 피해 후 보호막 종료 누락"); // 지연 종료 확인
            } // 예외 보호 끝
            finally // 임시 오브젝트 정리
            { // 정리 시작
                UnityEngine.Object.DestroyImmediate(testObject); // 임시 오브젝트 제거
            } // 정리 끝
        } // 메서드 끝

        private static void VerifyDarkCursedKillRule() // Dark 저주 처치 규칙 검증
        { // 메서드 시작
            Require(!WeaponAttributeDarkRules.ShouldHealOnKill(6, 0, 0f), "무저주 처치 회복 허용"); // 무저주 차단 확인
            Require(WeaponAttributeDarkRules.ShouldHealOnKill(6, 1, 0f), "저주 대상 처치 회복 차단"); // 저주 대상 허용 확인
        } // 메서드 끝

        private static void Require(bool condition, string message) // 검증 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException("[Project Epsilon] Day18 verification failed: " + message); // 검증 실패 예외
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
