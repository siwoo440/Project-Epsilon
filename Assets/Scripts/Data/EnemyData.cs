using UnityEngine;

namespace ProjectEpsilon.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Project Epsilon/Data/Enemy")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "enemy_id";
        [SerializeField] private string displayName = "Enemy";

        [Header("Base Stats")]
        [Min(1f)] [SerializeField] private float maxHealth = 20f;
        [Min(0f)] [SerializeField] private float contactDamage = 10f;
        [Min(0f)] [SerializeField] private float moveSpeed = 1f;
        [Min(0)] [SerializeField] private int experienceValue = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public float MaxHealth => maxHealth;
        public float ContactDamage => contactDamage;
        public float MoveSpeed => moveSpeed;
        public int ExperienceValue => experienceValue;
    }
}
