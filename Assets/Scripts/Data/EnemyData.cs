using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Data // 프로젝트 기능 영역
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Project Epsilon/Data/Enemy")] // 생성·파일 설정
    public sealed class EnemyData : ScriptableObject // 적·데이터 자료 구조
    {
        [Header("Identity")] // 대상 설정
        [SerializeField] private string id = "enemy_id"; // 필드·식별자 설정
        [SerializeField] private string displayName = "적"; // 필드·이름 설정

        [Header("Base Stats")] // 대상 설정
        [Min(1f)] [SerializeField] private float maxHealth = 20f; // 최소·필드 설정
        [Min(0f)] [SerializeField] private float contactDamage = 10f; // 최소·필드 설정
        [Min(0f)] [SerializeField] private float moveSpeed = 1f; // 최소·필드 설정
        [Min(0)] [SerializeField] private int experienceValue = 1; // 최소·필드 설정

        public string Id => id; // 식별자 값 반환
        public string DisplayName => displayName; // 이름 값 반환
        public float MaxHealth => maxHealth; // 최대·체력 값 반환
        public float ContactDamage => contactDamage; // 피해 값 반환
        public float MoveSpeed => moveSpeed; // 속도 값 반환
        public int ExperienceValue => experienceValue; // 경험치·값 값 반환
    }
}
