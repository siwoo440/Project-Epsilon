using UnityEngine;

namespace ProjectEpsilon.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Project Epsilon/Data/Character")]
    public sealed class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "character_id";
        [SerializeField] private string displayName = "Character";

        [Header("Base Stats")]
        [Min(1)] [SerializeField] private int startingBodyCount = 3;
        [Min(1f)] [SerializeField] private float bodyMaxHealth = 100f;
        [Min(0.1f)] [SerializeField] private float moveSpeedMultiplier = 1f;
        [Min(0.1f)] [SerializeField] private float turnPowerMultiplier = 1f;

        public string Id => id;
        public string DisplayName => displayName;
        public int StartingBodyCount => startingBodyCount;
        public float BodyMaxHealth => bodyMaxHealth;
        public float MoveSpeedMultiplier => moveSpeedMultiplier;
        public float TurnPowerMultiplier => turnPowerMultiplier;
    }
}
