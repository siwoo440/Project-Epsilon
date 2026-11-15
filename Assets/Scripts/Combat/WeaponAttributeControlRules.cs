using ProjectEpsilon.Data; // 무기 속성 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 이름 공간 시작
    public static class WeaponAttributeControlRules // 냉기·전기 제어 규칙
    { // 형식 시작
        public const int MaximumColdStacks = 3; // 냉기 최대 누적
        public const float ColdSlowDuration = 3f; // 임시 냉기 지속 시간

        public static bool IsActiveSynergy(int synergyStage) // 활성 시너지 여부 계산
        { // 함수 시작
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 2; // 2단계 이상 활성
        } // 함수 종료

        public static float GetColdMovementMultiplier(int synergyStage, int stackCount) // 냉기 누적별 이동 배율
        { // 함수 시작
            float slow = ClampColdStacks(stackCount) * 0.1f; // 기본 누적당 감속량
            float synergyMultiplier = IsActiveSynergy(synergyStage) ? 1.15f : 1f; // 감속량 15퍼센트 강화 임시 해석
            return 1f - slow * synergyMultiplier; // 실제 이동 배율 반환
        } // 함수 종료

        public static float GetColdDuration(int synergyStage) // 냉기 지속 시간 계산
        { // 함수 시작
            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 4 ? ColdSlowDuration * 1.25f : ColdSlowDuration; // 네 단계 지속 시간 증가
        } // 함수 종료

        public static int GetColdPriority(int synergyStage) // 냉기 우선순위 계산
        { // 함수 시작
            int stage = WeaponAttributeSynergyRules.ResolveStage(synergyStage); // 실제 시너지 단계
            return stage > 0 ? stage : 1; // 기본 냉기 우선순위 보장
        } // 함수 종료

        public static int ClampColdStacks(int stackCount) // 냉기 누적 제한
        { // 함수 시작
            if (stackCount < 0) // 음수 누적 확인
            { // 조건 시작
                return 0; // 최소 누적 반환
            } // 조건 종료

            if (stackCount > MaximumColdStacks) // 최대 누적 초과 확인
            { // 조건 시작
                return MaximumColdStacks; // 최대 누적 반환
            } // 조건 종료

            return stackCount; // 정상 누적 반환
        } // 함수 종료

        public static bool IsFreezeReady(int stackCount) // 빙결 준비 여부 계산
        { // 함수 시작
            return ClampColdStacks(stackCount) >= MaximumColdStacks; // 최대 누적 도달 여부
        } // 함수 종료

        public static float GetElectricChainRangeMultiplier(int synergyStage) // 전기 연쇄 범위 배율 계산
        { // 함수 시작
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            { // 조건 시작
                return 1f; // 기본 범위 배율
            } // 조건 종료

            return 1.1f; // 기획서 ×2 연쇄 범위 10퍼센트 증가
        } // 함수 종료

        public static int GetElectricSecondaryTargetCount(int synergyStage) // 전기 추가 대상 수 계산
        { // 함수 시작
            if (!IsActiveSynergy(synergyStage)) // 비활성 단계 확인
            { // 조건 시작
                return 0; // 추가 대상 없음
            } // 조건 종료

            return WeaponAttributeSynergyRules.ResolveStage(synergyStage) >= 4 ? 2 : 1; // 사 단계 추가 대상 한 개 증가
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
