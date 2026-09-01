namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public static class WeaponTargetStatusRules // 대상 상태 규칙
    { // 클래스 시작
        public static bool ShouldReplace(int currentPriority, int incomingPriority) // 상태 교체 여부 계산
        { // 메서드 시작
            int safeCurrent = NormalizePriority(currentPriority); // 현재 우선순위 정규화
            int safeIncoming = NormalizePriority(incomingPriority); // 신규 우선순위 정규화
            return safeIncoming > 0 && safeIncoming >= safeCurrent; // 동일 이상 우선순위 허용
        } // 메서드 끝

        public static int NormalizePriority(int priority) // 우선순위 정규화
        { // 메서드 시작
            return priority < 0 ? 0 : priority; // 음수 제거
        } // 메서드 끝

        public static float NormalizeSlowMultiplier(float multiplier) // 감속 배율 정규화
        { // 메서드 시작
            if (multiplier < 0.05f) // 최소 배율 확인
            { // 조건 시작
                return 0.05f; // 최소 배율 반환
            } // 조건 끝

            if (multiplier > 1f) // 최대 배율 확인
            { // 조건 시작
                return 1f; // 최대 배율 반환
            } // 조건 끝

            return multiplier; // 정상 배율 반환
        } // 메서드 끝

        public static float NormalizeDuration(float duration) // 지속 시간 정규화
        { // 메서드 시작
            return duration < 0f ? 0f : duration; // 음수 제거
        } // 메서드 끝

        public static float AdvanceDuration(float remaining, float deltaTime) // 지속 시간 감소
        { // 메서드 시작
            float safeRemaining = NormalizeDuration(remaining); // 남은 시간 정규화
            float safeDelta = NormalizeDuration(deltaTime); // 경과 시간 정규화
            float next = safeRemaining - safeDelta; // 다음 시간 계산
            return next < 0f ? 0f : next; // 0 이상 반환
        } // 메서드 끝

        public static bool IsActive(float remaining) // 상태 활성 여부 계산
        { // 메서드 시작
            return NormalizeDuration(remaining) > 0f; // 양수 시간 여부 반환
        } // 메서드 끝

        public static float ResolveMovementMultiplier(bool slowActive, float slowMultiplier, bool stopActive) // 최종 이동 배율 계산
        { // 메서드 시작
            if (stopActive) // 정지 상태 확인
            { // 조건 시작
                return 0f; // 완전 정지 반환
            } // 조건 끝

            if (!slowActive) // 감속 상태 없음 확인
            { // 조건 시작
                return 1f; // 기본 배율 반환
            } // 조건 끝

            return NormalizeSlowMultiplier(slowMultiplier); // 감속 배율 반환
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
