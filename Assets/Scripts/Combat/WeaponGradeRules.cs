using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    public static class WeaponGradeRules // 포탑·등급 자료 구조
    {
        public static float GetDamageMultiplier(int grade) // 조회·피해 기능 선언
        {
            switch (Mathf.Clamp(grade, 1, 5)) // 등급 종류 분기
            {
                case 2: // 대상 경우 선택
                    return 1.25f; // 대상 결과 반환

                case 3: // 대상 경우 선택
                    return 1.50f; // 대상 결과 반환

                case 4: // 대상 경우 선택
                    return 1.80f; // 대상 결과 반환

                case 5: // 대상 경우 선택
                    return 2.20f; // 대상 결과 반환

                default: // 기본 경우 선택
                    return 1f; // 대상 결과 반환
            }
        }

        public static float CalculateDamage( // 계산·피해 호출 시작
            float baseDamage, // 피해 항목 전달
            int grade // 등급 처리
        ) // 앞선 항목 종료
        {
            return Mathf.Max(0f, baseDamage) * // 최대·피해 결과 반환
                GetDamageMultiplier(grade); // 조회·피해 기능 실행
        }
    }
}
