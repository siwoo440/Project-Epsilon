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

        [Header("Projectile")]
        [Min(0.01f)] [SerializeField] private float projectileSpeed = 8f;
        [Min(0.1f)] [SerializeField] private float projectileLifetime = 3f;

        public string Id => id;
        public string DisplayName => displayName;
        public WeaponAttribute Attribute => attribute;
        public WeaponAttackType AttackType => attackType;
        public float BaseDamage => baseDamage;
        public float AttackInterval => attackInterval;
        public float Range => range;
        public int MaxGrade => maxGrade;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
    }
}
