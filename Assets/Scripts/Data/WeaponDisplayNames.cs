namespace ProjectEpsilon.Data // 포탑 표시 이름 영역
{
    public static class WeaponDisplayNames // 포탑 정보의 한국어 표시
    {
        public static string Attribute(WeaponAttribute attribute) // 실제 속성의 한국어 이름
        {
            switch (attribute) // 실제 속성 확인
            {
                case WeaponAttribute.Physical: return "물리"; // 물리 속성 이름
                case WeaponAttribute.Fire: return "화염"; // 화염 속성 이름
                case WeaponAttribute.Cold: return "냉기"; // 냉기 속성 이름
                case WeaponAttribute.Electric: return "전기"; // 전기 속성 이름
                case WeaponAttribute.Poison: return "독"; // 독 속성 이름
                case WeaponAttribute.Explosion: return "폭발"; // 폭발 속성 이름
                case WeaponAttribute.Holy: return "신성"; // 신성 속성 이름
                case WeaponAttribute.Dark: return "암흑"; // 암흑 속성 이름
                default: return "알 수 없음"; // 알 수 없는 속성
            }
        }
        public static string AttackType(WeaponAttackType type) // 실제 공격 유형의 한국어 이름
        {
            switch (type) // 실제 공격 유형 확인
            {
                case WeaponAttackType.Melee: return "근접 공격"; // 근접 공격 이름
                case WeaponAttackType.StraightProjectile: return "직선 투사체"; // 직선 공격 이름
                case WeaponAttackType.HomingProjectile: return "추적 투사체"; // 추적 공격 이름
                case WeaponAttackType.Area: return "범위 공격"; // 범위 공격 이름
                case WeaponAttackType.Persistent: return "지속 공격"; // 지속 공격 이름
                case WeaponAttackType.Special: return "특수 공격"; // 특수 공격 이름
                default: return "알 수 없음"; // 알 수 없는 공격 유형
            }
        }
    }
}
