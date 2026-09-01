namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public static class WeaponAttributeDarkRules // 암흑 속성 규칙
    { // 클래스 시작
        public const int MaximumCurseStacks = 5; // 최대 저주 중첩
        public const float CurseDuration = 6f; // 임시 저주 지속 시간
        public const float DamagePerCurseStack = 0.06f; // 임시 중첩 피해 증가율
        public const float KillHealChance = 0.15f; // 임시 처치 회복 확률
        public const int KillHealAmount = 1; // 처치 회복량
        public const float BaseAbsorptionRatio = 0.04f; // 임시 기본 흡수율
        public const float EnhancedAbsorptionRatio = 0.08f; // 임시 강화 흡수율
        public const float CurseSpreadRadius = 2.5f; // 임시 저주 전파 범위
        public const int CurseSpreadStacks = 1; // 임시 저주 전파 중첩

        public static int NormalizeCurseStacks(int stacks) // 저주 중첩 정규화
        { // 메서드 시작
            if (stacks < 0) // 하한 확인
            { // 조건 시작
                return 0; // 최소 중첩 반환
            } // 조건 끝

            return stacks > MaximumCurseStacks ? MaximumCurseStacks : stacks; // 최대 중첩 제한
        } // 메서드 끝

        public static int ApplyCurseStack(int currentStacks, int synergyStage) // 저주 한 중첩 적용
        { // 메서드 시작
            int safeCurrent = NormalizeCurseStacks(currentStacks); // 현재 중첩 보정
            return NormalizeStage(synergyStage) >= 2 ? NormalizeCurseStacks(safeCurrent + 1) : safeCurrent; // 2단계 이상 중첩 증가
        } // 메서드 끝

        public static float CalculateDirectDamage(float baseDamage, int synergyStage, int curseStacks) // 저주 기반 직접 피해 계산
        { // 메서드 시작
            float safeDamage = baseDamage < 0f ? 0f : baseDamage; // 음수 피해 제거
            return safeDamage * GetDamageMultiplier(synergyStage, curseStacks); // 최종 피해 반환
        } // 메서드 끝

        public static float GetDamageMultiplier(int synergyStage, int curseStacks) // 저주 피해 배율 조회
        { // 메서드 시작
            if (NormalizeStage(synergyStage) < 4) // 4단계 미만 확인
            { // 조건 시작
                return 1f; // 기본 배율 반환
            } // 조건 끝

            return 1f + NormalizeCurseStacks(curseStacks) * DamagePerCurseStack; // 중첩별 배율 반환
        } // 메서드 끝

        public static float CalculateAbsorption(float dealtDamage, int synergyStage) // 피해 흡수량 계산
        { // 메서드 시작
            float safeDamage = dealtDamage < 0f ? 0f : dealtDamage; // 음수 피해 제거
            return safeDamage * GetAbsorptionRatio(synergyStage); // 흡수량 반환
        } // 메서드 끝

        public static float GetAbsorptionRatio(int synergyStage) // 피해 흡수율 조회
        { // 메서드 시작
            int stage = NormalizeStage(synergyStage); // 단계 정규화

            if (stage >= 8) // 8단계 확인
            { // 조건 시작
                return EnhancedAbsorptionRatio; // 강화 흡수율 반환
            } // 조건 끝

            return stage >= 2 ? BaseAbsorptionRatio : 0f; // 기본 흡수율 반환
        } // 메서드 끝

        public static bool ShouldHealOnKill(int synergyStage, int curseStacks, float randomValue) // 처치 회복 여부 계산
        { // 메서드 시작
            float safeRandom = Clamp01(randomValue); // 난수 범위 보정
            return NormalizeStage(synergyStage) >= 6 && NormalizeCurseStacks(curseStacks) > 0 && safeRandom < KillHealChance; // 6단계 저주 대상 확률 판정
        } // 메서드 끝

        public static bool ShouldSpreadCurse(int synergyStage, int curseStacks) // 저주 전파 여부 계산
        { // 메서드 시작
            return NormalizeStage(synergyStage) >= 8 && NormalizeCurseStacks(curseStacks) >= MaximumCurseStacks; // 8단계 최대 저주 판정
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
