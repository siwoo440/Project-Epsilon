using UnityEngine;

namespace ProjectEpsilon.Progression
{
    public static class WeaponRewardRules
    {
        public static float GetGradeTwoChance(int level)
        {
            if (level >= 20)
            {
                return 0.20f;
            }

            if (level >= 15)
            {
                return 0.15f;
            }

            if (level >= 10)
            {
                return 0.10f;
            }

            return 0f;
        }

        public static int RollGrade(
            int level,
            int maximumGrade,
            float randomValue
        )
        {
            int safeMaximumGrade =
                Mathf.Clamp(maximumGrade, 1, 5);

            if (safeMaximumGrade < 2)
            {
                return 1;
            }

            float chance =
                GetGradeTwoChance(level);

            return Mathf.Clamp01(randomValue) < chance
                ? 2
                : 1;
        }
    }
}
