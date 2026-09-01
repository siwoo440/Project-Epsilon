using UnityEngine; // 수치 정규화 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public static class WeaponAttributePoisonRules // 독 속성 규칙
    {
        public const float TemporaryDuration = 3f; // 임시 독 지속 시간
        public const float TemporaryDefenseExposureMultiplier = 1.05f; // 임시 방어 약화 배율

        public static bool IsActiveSynergy(int synergyStage) // 독 시너지 활성 확인
        {
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 2; // ×2 이상 활성 반환
        }

        public static float GetOutgoingDamageMultiplier(int synergyStage) // 적 공격력 배율 계산
        {
            return IsActiveSynergy(synergyStage) ? 0.95f : 1f; // 기획서 ×2 공격력 5퍼센트 감소 적용
        }

        public static float GetIncomingDamageMultiplier(int synergyStage) // 적 받는 피해 배율 계산
        {
            return IsActiveSynergy(synergyStage) ? TemporaryDefenseExposureMultiplier : 1f; // 임시 방어 약화 적용
        }

        public static bool ShouldReplace(int currentStage, int incomingStage) // 독 단계 교체 여부 계산
        {
            int current = WeaponAttributeSynergyRules.ResolveStage(currentStage); // 현재 단계 정규화
            int incoming = WeaponAttributeSynergyRules.ResolveStage(incomingStage); // 신규 단계 정규화
            return incoming >= 2 && incoming >= current; // 동일 이상 단계 허용
        }

        public static float NormalizeDuration(float duration) // 지속 시간 정규화
        {
            return Mathf.Max(0f, duration); // 음수 시간 제거
        }
    }
}
