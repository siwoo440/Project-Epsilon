using ProjectEpsilon.Data; // 무기 속성 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public static class WeaponAttributeControlRules // 냉기·전기 제어 규칙
    {
        public const int MaximumColdStacks = 3; // 냉기 최대 누적
        public const float ColdSlowDuration = 3f; // 임시 냉기 지속 시간

        public static bool IsActiveSynergy(int synergyStage) // 활성 시너지 여부 계산
        {
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 2; // 2단계 이상 활성
        }

        public static float GetColdMovementMultiplier(int synergyStage) // 냉기 이동 배율 계산
        {
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            {
                return 1f; // 기본 이동 배율
            }

            return 0.85f; // 기획서 ×2 감속 15퍼센트
        }

        public static float GetColdDuration(int synergyStage) // 냉기 지속 시간 계산
        {
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            {
                return 0f; // 지속 없음
            }

            return ColdSlowDuration; // 공통 임시 지속 시간
        }

        public static int GetColdPriority(int synergyStage) // 냉기 우선순위 계산
        {
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage); // 시너지 단계 우선순위
        }

        public static int ClampColdStacks(int stackCount) // 냉기 누적 제한
        {
            if (stackCount < 0) // 음수 누적 확인
            {
                return 0; // 최소 누적 반환
            }

            if (stackCount > MaximumColdStacks) // 최대 누적 초과 확인
            {
                return MaximumColdStacks; // 최대 누적 반환
            }

            return stackCount; // 정상 누적 반환
        }

        public static bool IsFreezeReady(int stackCount) // 빙결 준비 여부 계산
        {
            return ClampColdStacks(stackCount) >= MaximumColdStacks; // 최대 누적 도달 여부
        }

        public static float GetElectricChainRangeMultiplier(int synergyStage) // 전기 연쇄 범위 배율 계산
        {
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            {
                return 1f; // 기본 범위 배율
            }

            return 1.1f; // 기획서 ×2 연쇄 범위 10퍼센트 증가
        }

        public static int GetElectricSecondaryTargetCount(int synergyStage) // 전기 추가 대상 수 계산
        {
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            {
                return 0; // 추가 대상 없음
            }

            return 1; // 최소 연쇄 대상 한 개
        }
    }
}
