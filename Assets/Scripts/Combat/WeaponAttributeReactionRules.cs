using ProjectEpsilon.Data; // 포탑 속성 종류

namespace ProjectEpsilon.Combat // 전투 규칙 영역
{
    public enum WeaponAttributeReaction // 속성 반응 종류
    {
        None, // 반응 없음
        CorrosiveBurn, // 화염과 독
        CoolingDischarge, // 냉기와 전기
        ShatteringImpact, // 물리와 폭발
        SacredExplosion, // 신성과 화염
        CorrosiveCurse, // 암흑과 독
        LightDarkCancel // 신성과 암흑
    }

    public static class WeaponAttributeReactionRules // 속성 반응 조합과 우선순위
    {
        public const int AttributeCount = 8; // 전체 속성 수

        public static WeaponAttributeReaction ResolvePair(WeaponAttribute first, WeaponAttribute second) // 순서와 무관한 조합 판정
        {
            if (IsPair(first, second, WeaponAttribute.Fire, WeaponAttribute.Poison)) // 화염과 독 확인
            {
                return WeaponAttributeReaction.CorrosiveBurn; // 부식성 연소 반환
            }
            if (IsPair(first, second, WeaponAttribute.Cold, WeaponAttribute.Electric)) // 냉기와 전기 확인
            {
                return WeaponAttributeReaction.CoolingDischarge; // 냉각 방전 반환
            }
            if (IsPair(first, second, WeaponAttribute.Physical, WeaponAttribute.Explosion)) // 물리와 폭발 확인
            {
                return WeaponAttributeReaction.ShatteringImpact; // 파쇄 충격 반환
            }
            if (IsPair(first, second, WeaponAttribute.Holy, WeaponAttribute.Fire)) // 신성과 화염 확인
            {
                return WeaponAttributeReaction.SacredExplosion; // 성화 폭발 반환
            }
            if (IsPair(first, second, WeaponAttribute.Dark, WeaponAttribute.Poison)) // 암흑과 독 확인
            {
                return WeaponAttributeReaction.CorrosiveCurse; // 부식 저주 반환
            }
            if (IsPair(first, second, WeaponAttribute.Holy, WeaponAttribute.Dark)) // 신성과 암흑 확인
            {
                return WeaponAttributeReaction.LightDarkCancel; // 명암 상쇄 반환
            }
            return WeaponAttributeReaction.None; // 정의되지 않은 조합
        }

        public static bool TrySelectPartner(WeaponAttribute incoming, int[] firstAppliedOrder, out WeaponAttribute partner, out WeaponAttributeReaction reaction) // 가장 오래된 반응 재료 선택
        {
            partner = default; // 선택 속성 초기화
            reaction = WeaponAttributeReaction.None; // 반응 종류 초기화
            if ((int)incoming < 0 || (int)incoming >= AttributeCount || firstAppliedOrder == null || firstAppliedOrder.Length != AttributeCount) // 속성과 재료 목록 확인
            {
                return false; // 잘못된 입력 거부
            }
            int oldest = int.MaxValue; // 최초 적용 순서 초기화
            for (int index = 0; index < AttributeCount; index++) // 기획서 속성 순서 순회
            {
                int order = firstAppliedOrder[index]; // 해당 속성 최초 적용 순서
                WeaponAttributeReaction candidate = ResolvePair(incoming, (WeaponAttribute)index); // 현재 조합 확인
                if (order < 0 || order >= oldest || candidate == WeaponAttributeReaction.None) // 미적용 또는 낮은 우선순위 확인
                {
                    continue; // 다음 속성 확인
                }
                oldest = order; // 가장 오래된 적용 순서 갱신
                partner = (WeaponAttribute)index; // 반응 재료 저장
                reaction = candidate; // 반응 종류 저장
            }
            return reaction != WeaponAttributeReaction.None; // 반응 재료 존재 여부
        }

        private static bool IsPair(WeaponAttribute first, WeaponAttribute second, WeaponAttribute left, WeaponAttribute right) // 양방향 조합 확인
        {
            return (first == left && second == right) || (first == right && second == left); // 순서 무관 비교
        }
    }
}
