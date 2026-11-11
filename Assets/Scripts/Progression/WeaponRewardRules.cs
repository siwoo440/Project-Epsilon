using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public static class WeaponRewardRules // 포탑·보상 자료 구조
    {
        public static float GetGradeTwoChance(int level) // 조회·등급 기능 선언
        {
            if (level >= 20) // 레벨 조건 확인
            {
                return 0.20f; // 대상 결과 반환
            }

            if (level >= 15) // 레벨 조건 확인
            {
                return 0.15f; // 대상 결과 반환
            }

            if (level >= 10) // 레벨 조건 확인
            {
                return 0.10f; // 대상 결과 반환
            }

            return 0f; // 대상 결과 반환
        }

        public static int RollGrade( // 등급 호출 시작
            int level, // 레벨 항목 전달
            int maximumGrade, // 최대·등급 항목 전달
            float randomValue // 난수·값 처리
        ) // 앞선 항목 종료
        {
            int safeMaximumGrade = // 최대·등급 값 설정
                Mathf.Clamp(maximumGrade, 1, 5); // 최대·등급 기능 실행

            if (safeMaximumGrade < 2) // 최대·등급 조건 확인
            {
                return 1; // 대상 결과 반환
            }

            float chance = // 확률 값 설정
                GetGradeTwoChance(level); // 조회·등급 기능 실행

            return Mathf.Clamp01(randomValue) < chance // 난수·값 결과 반환
                ? 2 // 대상 처리
                : 1; // 대상 처리
        }
    }
}
