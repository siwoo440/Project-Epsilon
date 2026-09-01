namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public static class WeaponAttributeHolyRules // 신성 속성 규칙
    { // 클래스 시작
        public const float HealChance = 0.15f; // 임시 회복 확률
        public const int HealAmount = 1; // 회복량
        public const int ShieldHitInterval = 5; // 임시 보호막 명중 주기
        public const int ShieldAmount = 15; // 임시 보호막량
        public const float ShieldDuration = 3f; // 임시 보호막 지속 시간
        public const float ShieldBurstRadius = 2f; // 임시 보호막 폭발 범위

        public static float CalculateDirectDamage(float baseDamage, int synergyStage) // 직접 피해 계산
        { // 메서드 시작
            float safeDamage = baseDamage < 0f ? 0f : baseDamage; // 음수 피해 제거
            return safeDamage * GetDirectDamageMultiplier(synergyStage); // 최종 피해 반환
        } // 메서드 끝

        public static float GetDirectDamageMultiplier(int synergyStage) // 직접 피해 배율 조회
        { // 메서드 시작
            return NormalizeStage(synergyStage) >= 2 ? 1.08f : 1f; // 2단계 이상 8퍼센트 증가
        } // 메서드 끝

        public static bool ShouldHeal(int synergyStage, float randomValue) // 명중 회복 여부 계산
        { // 메서드 시작
            float safeRandom = Clamp01(randomValue); // 난수 범위 보정
            return NormalizeStage(synergyStage) >= 4 && safeRandom < HealChance; // 4단계 이상 확률 판정
        } // 메서드 끝

        public static bool ShouldGrantShield(int synergyStage, int holyHitCount) // 보호막 발동 여부 계산
        { // 메서드 시작
            return NormalizeStage(synergyStage) >= 6 && holyHitCount > 0 && holyHitCount % ShieldHitInterval == 0; // 6단계 이상 주기 판정
        } // 메서드 끝

        public static bool CanBurstOnShieldEnd(int synergyStage) // 보호막 종료 폭발 여부 계산
        { // 메서드 시작
            return NormalizeStage(synergyStage) >= 8; // 8단계 여부 반환
        } // 메서드 끝

        public static float CalculateShieldBurstDamage(float sourceDamage, int synergyStage) // 보호막 폭발 피해 계산
        { // 메서드 시작
            float safeDamage = sourceDamage < 0f ? 0f : sourceDamage; // 음수 피해 제거
            return CanBurstOnShieldEnd(synergyStage) ? safeDamage * 0.5f : 0f; // 임시 절반 피해 반환
        } // 메서드 끝

        private static int NormalizeStage(int synergyStage) // 시너지 단계 정규화
        { // 메서드 시작
            if (synergyStage >= 8) // 8단계 경계 확인
            { // 조건 시작
                return 8; // 8단계 반환
            } // 조건 끝

            if (synergyStage >= 6) // 6단계 경계 확인
            { // 조건 시작
                return 6; // 6단계 반환
            } // 조건 끝

            if (synergyStage >= 4) // 4단계 경계 확인
            { // 조건 시작
                return 4; // 4단계 반환
            } // 조건 끝

            if (synergyStage >= 2) // 2단계 경계 확인
            { // 조건 시작
                return 2; // 2단계 반환
            } // 조건 끝

            return 0; // 비활성 반환
        } // 메서드 끝

        private static float Clamp01(float value) // 0부터 1 범위 제한
        { // 메서드 시작
            if (value < 0f) // 하한 확인
            { // 조건 시작
                return 0f; // 하한 반환
            } // 조건 끝

            return value > 1f ? 1f : value; // 상한 제한 반환
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
