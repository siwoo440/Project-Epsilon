using ProjectEpsilon.Data; // 무기 속성 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public static class WeaponAttributeDamageRules // 속성 피해 규칙
    {
        public static float CalculateDirectDamage(float gradeDamage, WeaponAttribute attribute, int synergyStage) // 직접 피해 계산
        {
            float safeDamage = gradeDamage < 0f ? 0f : gradeDamage; // 음수 피해 제거

            if (attribute == WeaponAttribute.Holy) // 신성 속성 여부 확인
            {
                return WeaponAttributeHolyRules.CalculateDirectDamage(safeDamage, synergyStage); // 신성 증폭 피해 반환
            }

            if (attribute != WeaponAttribute.Physical) // 물리 속성 여부 확인
            {
                return safeDamage; // 원래 피해 반환
            }

            return safeDamage * GetPhysicalMultiplier(synergyStage); // 물리 증폭 피해 반환
        }

        public static float GetPhysicalMultiplier(int synergyStage) // 물리 배율 조회
        {
            int stage = NormalizeStage(synergyStage); // 단계 정규화

            return stage >= 2 ? 1.1f : 1f; // 2단계 피해 증가 유지
        }

        public static float CalculatePhysicalCriticalDamage(float directDamage, int synergyStage, float roll) // 물리 치명타 피해 계산
        {
            float safeDamage = directDamage < 0f ? 0f : directDamage; // 음수 피해 제거
            int stage = NormalizeStage(synergyStage); // 단계 정규화
            if (stage < 4 || roll >= 0.1f) // 치명타 단계와 확률 확인
            {
                return safeDamage; // 일반 피해 반환
            }
            return safeDamage * (stage >= 8 ? 2.5f : 1.75f); // 치명타 피해 배율 적용
        }

        public static float GetFireDuration(int synergyStage, float baseDuration = 3f) // 화상 지속 시간 조회
        {
            int stage = NormalizeStage(synergyStage); // 단계 정규화
            float safeDuration = baseDuration < 0f ? 0f : baseDuration; // 음수 지속 시간 제거
            return safeDuration * (stage >= 4 ? 1.3f : 1f); // 4단계 지속 시간 증가
        }

        public static float CalculateFireDamagePerSecond(float baseDamage, int synergyStage) // 화상 초당 피해 계산
        {
            float safeDamage = baseDamage < 0f ? 0f : baseDamage; // 음수 피해 제거
            int stage = NormalizeStage(synergyStage); // 단계 정규화
            return safeDamage * (stage >= 2 ? 1.2f : 1f); // 2단계 피해 증가
        }

        public static bool ShouldReplaceBurn(int currentStage, int incomingStage) // 화상 교체 여부 계산
        {
            return true; // 재적중 시 화상 갱신
        }

        private static int NormalizeStage(int synergyStage) // 단계 정규화
        {
            if (synergyStage >= 8) // 8단계 경계 확인
            {
                return 8; // 8단계 반환
            }

            if (synergyStage >= 6) // 6단계 경계 확인
            {
                return 6; // 6단계 반환
            }

            if (synergyStage >= 4) // 4단계 경계 확인
            {
                return 4; // 4단계 반환
            }

            if (synergyStage >= 2) // 2단계 경계 확인
            {
                return 2; // 2단계 반환
            }

            return 0; // 비활성 반환
        }
    }
}
