using UnityEngine;

namespace ProjectEpsilon.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Project Epsilon/Data/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "weapon_id";
        [SerializeField] private string displayName = "Weapon";

        [Header("Classification")]
        [SerializeField] private WeaponAttribute attribute = WeaponAttribute.Physical;
        [SerializeField] private WeaponAttackType attackType = WeaponAttackType.Melee;

        [Header("Base Stats")]
        [Min(0f)] [SerializeField] private float baseDamage = 10f;
        [Min(0.01f)] [SerializeField] private float attackInterval = 1f;
        [Min(0f)] [SerializeField] private float range = 5f;
        [Range(1, 5)] [SerializeField] private int maxGrade = 5;
        [Range(1, 2)] [SerializeField] private int gradeThreeMeleeTargetLimit = 1; // 삼 등급 근접 최대 대상 수

        [Header("Projectile")]
        [Min(0.01f)] [SerializeField] private float projectileSpeed = 8f;
        [Min(0.1f)] [SerializeField] private float projectileLifetime = 3f;
        [Min(1)] [SerializeField] private int projectileHitLimit = 1; // 투사체 최대 명중 수
        [Min(0f)] [SerializeField] private float burnDuration = 3f; // 기본 화상 지속 시간
        [Min(0f)] [SerializeField] private float burnDamagePerSecond = 1f; // 기본 화상 초당 피해

        public string Id => id;
        public string DisplayName => displayName;
        public WeaponAttribute Attribute => attribute;
        public WeaponAttackType AttackType => attackType;
        public float BaseDamage => baseDamage;
        public float AttackInterval => attackInterval;
        public float Range => range;
        public int MaxGrade => maxGrade;
        public int GetMeleeTargetLimit(int grade) => grade >= 3 ? Mathf.Clamp(gradeThreeMeleeTargetLimit, 1, 2) : 1; // 등급별 근접 대상 수
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int ProjectileHitLimit => Mathf.Max(1, projectileHitLimit); // 안전한 최대 명중 수
        public float BurnDuration => Mathf.Max(0f, burnDuration); // 화상 지속 시간 반환
        public float BurnDamagePerSecond => Mathf.Max(0f, burnDamagePerSecond); // 화상 초당 피해 반환
    }
}
