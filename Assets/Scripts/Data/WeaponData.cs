using UnityEngine;

namespace ProjectEpsilon.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Project Epsilon/Data/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "weapon_id";
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private Sprite artwork; // 무기 장착 이미지

        [Header("Classification")]
        [SerializeField] private WeaponAttribute attribute = WeaponAttribute.Physical;
        [SerializeField] private WeaponAttackType attackType = WeaponAttackType.Melee;

        [Header("Base Stats")]
        [Min(0f)] [SerializeField] private float baseDamage = 10f;
        [Min(0.01f)] [SerializeField] private float attackInterval = 1f;
        [Min(0f)] [SerializeField] private float range = 5f;
        [Range(1, 5)] [SerializeField] private int maxGrade = 5;
        [Range(1, 2)] [SerializeField] private int gradeThreeMeleeTargetLimit = 1; // 삼 등급 근접 최대 대상 수
        [Min(1f)] [SerializeField] private float gradeThreeAreaRangeMultiplier = 1f; // 삼 등급 범위 배율 임시값
        [Range(1, 3)] [SerializeField] private int gradeFiveMeleeTargetLimit = 2; // 오 등급 근접 최대 대상 수
        [Min(1f)] [SerializeField] private float gradeFiveAreaRangeMultiplier = 1f; // 오 등급 범위 배율 임시값

        [Header("Projectile")]
        [Min(0.01f)] [SerializeField] private float projectileSpeed = 8f;
        [Min(0.1f)] [SerializeField] private float projectileLifetime = 3f;
        [Min(1)] [SerializeField] private int projectileHitLimit = 1; // 투사체 최대 명중 수
        [Min(0f)] [SerializeField] private float gradeThreeTransferRange = 0f; // 삼 등급 추가 전달 반경 임시값
        [Min(0f)] [SerializeField] private float gradeThreeTransferDamageMultiplier = 1f; // 추가 전달 피해 배율 임시값
        [Min(1f)] [SerializeField] private float gradeFiveTransferDamageScale = 1f; // 오 등급 전달 피해 추가 배율
        [Min(0f)] [SerializeField] private float burnDuration = 3f; // 기본 화상 지속 시간
        [Min(0f)] [SerializeField] private float burnDamagePerSecond = 1f; // 기본 화상 초당 피해

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Artwork => artwork; // 무기 표시 이미지
        public Color ArtworkTint // 속성별 포탑 표시 색상
        { // 속성 시작
            get // 색상 조회
            { // 접근자 시작
                switch (attribute) // 무기 속성 확인
                { // 분기 시작
                    case WeaponAttribute.Fire: return new Color(1f, 0.55f, 0.4f, 1f); // 화염색
                    case WeaponAttribute.Cold: return new Color(0.5f, 0.85f, 1f, 1f); // 냉기색
                    case WeaponAttribute.Electric: return new Color(1f, 0.95f, 0.45f, 1f); // 전기색
                    case WeaponAttribute.Poison: return new Color(0.55f, 1f, 0.5f, 1f); // 독색
                    case WeaponAttribute.Explosion: return new Color(1f, 0.68f, 0.3f, 1f); // 폭발색
                    case WeaponAttribute.Holy: return new Color(1f, 0.95f, 0.65f, 1f); // 신성색
                    case WeaponAttribute.Dark: return new Color(0.7f, 0.5f, 0.95f, 1f); // 암흑색
                    default: return Color.white; // 기본 물리색
                } // 분기 끝
            } // 접근자 끝
        } // 속성 끝
        public WeaponAttribute Attribute => attribute;
        public WeaponAttackType AttackType => attackType;
        public float BaseDamage => baseDamage;
        public float AttackInterval => attackInterval;
        public float Range => range;
        public int MaxGrade => maxGrade;
        public int GetMeleeTargetLimit(int grade) => grade >= 5 ? Mathf.Clamp(gradeFiveMeleeTargetLimit, GetMeleeTargetLimit(3), 3) : grade >= 3 ? Mathf.Clamp(gradeThreeMeleeTargetLimit, 1, 2) : 1; // 등급별 안전한 근접 대상 수
        public float GetAreaRangeMultiplier(int grade) => grade >= 5 ? Mathf.Max(GetAreaRangeMultiplier(3), gradeFiveAreaRangeMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeAreaRangeMultiplier) : 1f; // 등급별 안전한 범위 배율
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public int ProjectileHitLimit => Mathf.Max(1, projectileHitLimit); // 안전한 최대 명중 수
        public float GradeThreeTransferRange => Mathf.Max(0f, gradeThreeTransferRange); // 안전한 추가 전달 반경
        public float GradeThreeTransferDamageMultiplier => Mathf.Max(0f, gradeThreeTransferDamageMultiplier); // 안전한 추가 전달 피해 배율
        public float GetTransferDamageMultiplier(int grade) => GradeThreeTransferDamageMultiplier * (grade >= 5 ? Mathf.Max(1f, gradeFiveTransferDamageScale) : 1f); // 등급별 전달 피해 배율
        public float BurnDuration => Mathf.Max(0f, burnDuration); // 화상 지속 시간 반환
        public float BurnDamagePerSecond => Mathf.Max(0f, burnDamagePerSecond); // 화상 초당 피해 반환
    }
}
