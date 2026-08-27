using UnityEngine;

namespace ProjectEpsilon.Combat
{
    public static class WeaponGradeRules
    {
        public static float GetDamageMultiplier(int grade)
        {
            switch (Mathf.Clamp(grade, 1, 5))
            {
                case 2:
                    return 1.25f;

                case 3:
                    return 1.50f;

                case 4:
                    return 1.80f;

                case 5:
                    return 2.20f;

                default:
                    return 1f;
            }
        }

        public static float CalculateDamage(
            float baseDamage,
            int grade
        )
        {
            return Mathf.Max(0f, baseDamage) *
                GetDamageMultiplier(grade);
        }
    }
}
