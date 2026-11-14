using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Data // 프로젝트 기능 영역
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Project Epsilon/Data/Weapon")] // 생성·파일 설정
    public sealed class WeaponData : ScriptableObject // 포탑·데이터 자료 구조
    {
        [Header("Identity")] // 대상 설정
        [SerializeField] private string id = "weapon_id"; // 필드·식별자 설정
        [SerializeField] private string displayName = "포탑"; // 필드·이름 설정
        [SerializeField] private Sprite artwork; // 무기 장착 이미지
        [SerializeField] private Sprite icon; // 포탑 선택과 패널 아이콘

        [Header("Classification")] // 대상 설정
        [SerializeField] private WeaponAttribute attribute = WeaponAttribute.Physical; // 필드·포탑 설정
        [SerializeField] private WeaponAttackType attackType = WeaponAttackType.Melee; // 필드·포탑 설정

        [Header("Base Stats")] // 대상 설정
        [Min(0f)] [SerializeField] private float baseDamage = 10f; // 최소·필드 설정
        [Min(0.01f)] [SerializeField] private float attackInterval = 1f; // 최소·필드 설정
        [Min(0f)] [SerializeField] private float range = 5f; // 최소·필드 설정
        [Range(1, 5)] [SerializeField] private int maxGrade = 5; // 범위·필드 설정
        [Range(1, 2)] [SerializeField] private int gradeThreeMeleeTargetLimit = 1; // 삼 등급 근접 최대 대상 수
        [Min(1f)] [SerializeField] private float gradeThreeAreaRangeMultiplier = 1f; // 삼 등급 범위 배율 임시값
        [Range(1, 3)] [SerializeField] private int gradeFiveMeleeTargetLimit = 2; // 오 등급 근접 최대 대상 수
        [Min(1f)] [SerializeField] private float gradeFiveAreaRangeMultiplier = 1f; // 오 등급 범위 배율 임시값

        [Header("Projectile")] // 투사체 설정
        [Min(0.01f)] [SerializeField] private float projectileSpeed = 8f; // 최소·필드 설정
        [Min(0.1f)] [SerializeField] private float projectileLifetime = 3f; // 최소·필드 설정
        [Min(1)] [SerializeField] private int projectileHitLimit = 1; // 투사체 최대 명중 수
        [Min(0f)] [SerializeField] private float gradeThreeTransferRange = 0f; // 삼 등급 추가 전달 반경 임시값
        [Min(0f)] [SerializeField] private float gradeThreeTransferDamageMultiplier = 1f; // 추가 전달 피해 배율 임시값
        [Min(1f)] [SerializeField] private float gradeFiveTransferDamageScale = 1f; // 오 등급 전달 피해 추가 배율
        [Min(0f)] [SerializeField] private float burnDuration = 3f; // 기본 화상 지속 시간
        [Min(0f)] [SerializeField] private float burnDamagePerSecond = 1f; // 기본 화상 초당 피해
        [Min(0)] [SerializeField] private int gradeFiveWideStrikeInterval; // 오 등급 강타 공격 주기
        [Min(0f)] [SerializeField] private float gradeFiveWideStrikeRadius; // 오 등급 강타 반경
        [Min(0f)] [SerializeField] private float gradeFiveWideStrikeDamageMultiplier = 1f; // 오 등급 강타 피해 배율
        [Min(0f)] [SerializeField] private float gradeFiveBurnExplosionRadius; // 화상 적 처치 폭발 반경
        [Min(0f)] [SerializeField] private float gradeFiveBurnExplosionDamageMultiplier; // 화상 적 처치 폭발 피해 배율
        [Min(0)] [SerializeField] private int gradeThreeExtraProjectileHits; // 삼 등급 추가 관통 횟수 시험값
        [Min(0)] [SerializeField] private int gradeFiveExtraProjectileHits; // 오 등급 추가 관통 횟수 시험값
        [Min(1f)] [SerializeField] private float gradeThreeAttackSpeedMultiplier = 1f; // 삼 등급 공격속도 배율 시험값
        [Min(1f)] [SerializeField] private float gradeFiveAttackSpeedMultiplier = 1f; // 오 등급 공격속도 배율 시험값
        [Min(0)] [SerializeField] private int gradeThreePeriodicInterval; // 삼 등급 주기 공격 횟수 시험값
        [Min(1f)] [SerializeField] private float gradeThreePeriodicDamageMultiplier = 1f; // 삼 등급 주기 피해 배율 시험값
        [Min(0)] [SerializeField] private int gradeFivePeriodicInterval; // 오 등급 주기 공격 횟수 시험값
        [Min(1f)] [SerializeField] private float gradeFivePeriodicDamageMultiplier = 1f; // 오 등급 주기 피해 배율 시험값
        [Min(0f)] [SerializeField] private float gradeFiveAreaKnockbackDistance; // 오 등급 범위 넉백 거리 시험값
        [Min(0f)] [SerializeField] private float gradeFiveAreaKnockbackDuration = 0.2f; // 오 등급 범위 넉백 시간 시험값
        [Min(1f)] [SerializeField] private float gradeThreeBurnedDamageMultiplier = 1f; // 삼 등급 화상 대상 피해 배율 시험값
        [Min(1f)] [SerializeField] private float gradeFiveBurnedDamageMultiplier = 1f; // 오 등급 화상 대상 피해 배율 시험값
        [Min(1f)] [SerializeField] private float gradeThreeBurnDurationMultiplier = 1f; // 삼 등급 화상 시간 배율 시험값
        [Min(1f)] [SerializeField] private float gradeFiveBurnDurationMultiplier = 1f; // 오 등급 화상 시간 배율 시험값
        [Min(1f)] [SerializeField] private float gradeThreeBurnDamageMultiplier = 1f; // 삼 등급 화상 피해 배율 시험값
        [Min(1f)] [SerializeField] private float gradeFiveBurnDamageMultiplier = 1f; // 오 등급 화상 피해 배율 시험값

        public string Id => id; // 식별자 값 반환
        public string DisplayName => displayName; // 이름 값 반환
        public Sprite Artwork => artwork; // 무기 표시 이미지
        public Sprite Icon => icon != null ? icon : artwork; // 아이콘과 기존 그림 대체
        public Color ArtworkDisplayTint => Color.white; // 원본 포탑 그림 색상 유지
        public Color ArtworkTint // 속성별 공격 효과 색상
        {
            get // 색상 조회
            {
                switch (attribute) // 무기 속성 확인
                {
                    case WeaponAttribute.Fire: return new Color(1f, 0.55f, 0.4f, 1f); // 화염색
                    case WeaponAttribute.Cold: return new Color(0.5f, 0.85f, 1f, 1f); // 냉기색
                    case WeaponAttribute.Electric: return new Color(1f, 0.95f, 0.45f, 1f); // 전기색
                    case WeaponAttribute.Poison: return new Color(0.55f, 1f, 0.5f, 1f); // 독색
                    case WeaponAttribute.Explosion: return new Color(1f, 0.68f, 0.3f, 1f); // 폭발색
                    case WeaponAttribute.Holy: return new Color(1f, 0.95f, 0.65f, 1f); // 신성색
                    case WeaponAttribute.Dark: return new Color(0.7f, 0.5f, 0.95f, 1f); // 암흑색
                    default: return Color.white; // 기본 물리색
                }
            }
        }
        public WeaponAttribute Attribute => attribute; // 포탑·속성 값 반환
        public WeaponAttackType AttackType => attackType; // 포탑·공격 값 반환
        public float BaseDamage => baseDamage; // 피해 값 반환
        public float AttackInterval => attackInterval; // 공격·간격 값 반환
        public float Range => range; // 범위 값 반환
        public int MaxGrade => maxGrade; // 최대·등급 값 반환
        public int GetMeleeTargetLimit(int grade) => grade >= 5 ? Mathf.Clamp(gradeFiveMeleeTargetLimit, GetMeleeTargetLimit(3), 3) : grade >= 3 ? Mathf.Clamp(gradeThreeMeleeTargetLimit, 1, 2) : 1; // 등급별 안전한 근접 대상 수
        public float GetAreaRangeMultiplier(int grade) => grade >= 5 ? Mathf.Max(GetAreaRangeMultiplier(3), gradeFiveAreaRangeMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeAreaRangeMultiplier) : 1f; // 등급별 안전한 범위 배율
        public float ProjectileSpeed => projectileSpeed; // 투사체·속도 값 반환
        public float ProjectileLifetime => projectileLifetime; // 투사체 값 반환
        public int ProjectileHitLimit => Mathf.Max(1, projectileHitLimit); // 안전한 최대 명중 수
        public int GetProjectileHitLimit(int grade) => ProjectileHitLimit + (grade >= 5 ? Mathf.Max(Mathf.Max(0, gradeThreeExtraProjectileHits), gradeFiveExtraProjectileHits) : grade >= 3 ? Mathf.Max(0, gradeThreeExtraProjectileHits) : 0); // 등급별 실제 관통 횟수
        public float GetGradeAttackSpeedMultiplier(int grade) => grade >= 5 ? Mathf.Max(Mathf.Max(1f, gradeThreeAttackSpeedMultiplier), gradeFiveAttackSpeedMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeAttackSpeedMultiplier) : 1f; // 등급별 실제 공격속도 배율
        public int GetPeriodicInterval(int grade) => grade >= 5 && gradeFivePeriodicInterval > 0 ? gradeFivePeriodicInterval : grade >= 3 ? Mathf.Max(0, gradeThreePeriodicInterval) : 0; // 등급별 주기 공격 간격
        public float GetPeriodicDamageMultiplier(int grade) => grade >= 5 && gradeFivePeriodicInterval > 0 ? Mathf.Max(1f, gradeFivePeriodicDamageMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreePeriodicDamageMultiplier) : 1f; // 등급별 주기 피해 배율
        public float GetAreaKnockbackDistance(int grade) => grade >= 5 ? Mathf.Max(0f, gradeFiveAreaKnockbackDistance) : 0f; // 등급별 범위 넉백 거리
        public float GradeFiveAreaKnockbackDuration => Mathf.Max(0f, gradeFiveAreaKnockbackDuration); // 안전한 범위 넉백 시간
        public float GetBurnedDamageMultiplier(int grade) => grade >= 5 ? Mathf.Max(Mathf.Max(1f, gradeThreeBurnedDamageMultiplier), gradeFiveBurnedDamageMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeBurnedDamageMultiplier) : 1f; // 등급별 화상 대상 직접 피해 배율
        public float GetBurnDurationMultiplier(int grade) => grade >= 5 ? Mathf.Max(Mathf.Max(1f, gradeThreeBurnDurationMultiplier), gradeFiveBurnDurationMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeBurnDurationMultiplier) : 1f; // 등급별 화상 지속 시간 배율
        public float GetBurnDamageMultiplier(int grade) => grade >= 5 ? Mathf.Max(Mathf.Max(1f, gradeThreeBurnDamageMultiplier), gradeFiveBurnDamageMultiplier) : grade >= 3 ? Mathf.Max(1f, gradeThreeBurnDamageMultiplier) : 1f; // 등급별 화상 지속 피해 배율
        public float GradeThreeTransferRange => Mathf.Max(0f, gradeThreeTransferRange); // 안전한 추가 전달 반경
        public float GradeThreeTransferDamageMultiplier => Mathf.Max(0f, gradeThreeTransferDamageMultiplier); // 안전한 추가 전달 피해 배율
        public float GetTransferDamageMultiplier(int grade) => GradeThreeTransferDamageMultiplier * (grade >= 5 ? Mathf.Max(1f, gradeFiveTransferDamageScale) : 1f); // 등급별 전달 피해 배율
        public float BurnDuration => Mathf.Max(0f, burnDuration); // 화상 지속 시간 반환
        public float BurnDamagePerSecond => Mathf.Max(0f, burnDamagePerSecond); // 화상 초당 피해 반환
        public int GradeFiveWideStrikeInterval => Mathf.Max(0, gradeFiveWideStrikeInterval); // 안전한 강타 주기
        public float GradeFiveWideStrikeRadius => Mathf.Max(0f, gradeFiveWideStrikeRadius); // 안전한 강타 반경
        public float GradeFiveWideStrikeDamageMultiplier => Mathf.Max(0f, gradeFiveWideStrikeDamageMultiplier); // 안전한 강타 피해 배율
        public float GradeFiveBurnExplosionRadius => Mathf.Max(0f, gradeFiveBurnExplosionRadius); // 안전한 화상 폭발 반경
        public float GradeFiveBurnExplosionDamageMultiplier => Mathf.Max(0f, gradeFiveBurnExplosionDamageMultiplier); // 안전한 화상 폭발 피해 배율
    }
}
