namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class PrototypeBossPhase // 시험 보스 단계 상태
    { // 클래스 시작
        public bool IsPhaseTwo { get; private set; } // 두 번째 단계 여부

        public void Observe(float currentHealth, float maximumHealth, bool alive) // 실제 보스 체력 관찰
        { // 메서드 시작
            if (!IsPhaseTwo && alive && maximumHealth > 0f && currentHealth <= maximumHealth * 0.5f) // 생존 중 절반 경계 확인
            { // 조건 시작
                IsPhaseTwo = true; // 두 번째 단계 유지
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
