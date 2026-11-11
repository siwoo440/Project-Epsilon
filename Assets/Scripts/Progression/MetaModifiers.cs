using ProjectEpsilon.Player; // 실제 몸통 상태

namespace ProjectEpsilon.Progression // 영구 성장 영역
{
    public static class MetaModifiers // 실제 성장 단계별 능력
    {
        public static float HealthMultiplier => 1f + 0.04f * MetaProgressionController.Level(MetaNode.Health); // 몸통 최대 체력 배율
        public static float HealingMultiplier => 1f + 0.05f * MetaProgressionController.Level(MetaNode.Healing); // 실제 회복 배율
        public static float TurnMultiplier => 1f + 0.03f * MetaProgressionController.Level(MetaNode.Turn); // 실제 회전력 배율
        public static float StaminaMultiplier => 1f + 0.05f * MetaProgressionController.Level(MetaNode.Stamina); // 실제 최대 스태미나 배율
        public static float AttackSpeedMultiplier => 1f + 0.02f * MetaProgressionController.Level(MetaNode.AttackSpeed); // 실제 포탑 공격속도 배율
        public static float ExperienceMultiplier => 1f + 0.03f * MetaProgressionController.Level(MetaNode.Experience); // 실제 경험치 배율
        public static float PickupRangeMultiplier => 1f + 0.05f * MetaProgressionController.Level(MetaNode.PickupRange); // 실제 보석 흡수 범위 배율
        public static float DamageMultiplier(int grade) // 실제 포탑 피해 배율
        {
            float basic = 1f + 0.02f * MetaProgressionController.Level(MetaNode.Damage); // 실제 기본 피해 증가
            return basic * (grade >= 3 && MetaProgressionController.Level(MetaNode.HighGradeDamage) > 0 ? 1.05f : 1f); // 고등급 최종 효과 적용
        }
        public static float LastBodySpeedMultiplier(SnakeBodyManager body) // 마지막 몸통 이동 효과
        {
            return body != null && body.CurrentBodyCount == 1 && MetaProgressionController.Level(MetaNode.LastBodySpeed) > 0 ? 1.1f : 1f; // 실제 마지막 몸통 확인
        }
        public static float DrainMultiplier(float current, float maximum) // 낮은 스태미나 소모 효과
        {
            return maximum > 0f && current <= maximum * 0.25f && MetaProgressionController.Level(MetaNode.EmergencyBoost) > 0 ? 0.8f : 1f; // 실제 낮은 스태미나 확인
        }
    }
}
