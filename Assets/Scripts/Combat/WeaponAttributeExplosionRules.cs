using UnityEngine; // 거리 계산 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public static class WeaponAttributeExplosionRules // 폭발 속성 규칙
    {
        public const float TemporaryCenterDamageBonus = 0.25f; // 임시 중심 추가 피해
        public const float TemporaryKnockbackDistance = 0.75f; // 임시 넉백 거리
        public const float TemporaryKnockbackDuration = 0.18f; // 임시 넉백 시간

        public static bool IsActiveSynergy(int synergyStage) // 폭발 시너지 활성 확인
        {
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 2; // ×2 이상 활성 반환
        }

        public static float GetRangeMultiplier(int synergyStage) // 폭발 범위 배율 계산
        {
            return IsActiveSynergy(synergyStage) ? 1.1f : 1f; // 기획서 ×2 범위 10퍼센트 증가 적용
        }

        public static float ResolveRange(float baseRange, int synergyStage) // 실제 폭발 범위 계산
        {
            return Mathf.Max(0f, baseRange) * GetRangeMultiplier(synergyStage); // 안전 범위 반환
        }

        public static float CalculateCenterDamage(float directDamage, float distance, float radius) // 중심 거리 피해 계산
        {
            float safeDamage = Mathf.Max(0f, directDamage); // 기본 피해 정규화
            float safeRadius = Mathf.Max(0.0001f, radius); // 0 나눗셈 방지
            float normalizedDistance = Mathf.Clamp01(Mathf.Max(0f, distance) / safeRadius); // 중심 거리 비율 계산
            float bonus = Mathf.Lerp(TemporaryCenterDamageBonus, 0f, normalizedDistance); // 중심 추가 피해 감쇠
            return safeDamage * (1f + bonus); // 임시 중심 피해 반환
        }

        public static float GetKnockbackDistance(int synergyStage) // 넉백 거리 계산
        {
            return TemporaryKnockbackDistance; // 임시 넉백 거리 반환
        }

        public static float GetKnockbackDuration(int synergyStage) // 넉백 시간 계산
        {
            return TemporaryKnockbackDuration; // 임시 넉백 시간 반환
        }
    }
}
