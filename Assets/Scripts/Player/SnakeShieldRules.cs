namespace ProjectEpsilon.Player // 플레이어 영역
{
    public static class SnakeShieldRules // 보호막 계산 규칙
    {
        public static int ResolveAppliedShield(int currentShield, int incomingShield) // 보호막 적용량 계산
        {
            int safeCurrent = currentShield < 0 ? 0 : currentShield; // 현재 보호막 보정
            int safeIncoming = incomingShield < 0 ? 0 : incomingShield; // 신규 보호막 보정
            return safeIncoming > safeCurrent ? safeIncoming : safeCurrent; // 큰 보호막 반환
        }

        public static int ResolveShieldAfterDamage(int currentShield, int incomingDamage) // 피해 후 보호막 계산
        {
            int safeShield = currentShield < 0 ? 0 : currentShield; // 현재 보호막 보정
            int safeDamage = incomingDamage < 0 ? 0 : incomingDamage; // 피해량 보정
            int remaining = safeShield - safeDamage; // 잔여 보호막 계산
            return remaining < 0 ? 0 : remaining; // 음수 제거 반환
        }

        public static int ResolveHealthDamage(int currentShield, int incomingDamage) // 보호막 후 체력 피해 계산
        {
            int safeShield = currentShield < 0 ? 0 : currentShield; // 현재 보호막 보정
            int safeDamage = incomingDamage < 0 ? 0 : incomingDamage; // 피해량 보정
            int remaining = safeDamage - safeShield; // 잔여 피해 계산
            return remaining < 0 ? 0 : remaining; // 음수 제거 반환
        }
    }
}
