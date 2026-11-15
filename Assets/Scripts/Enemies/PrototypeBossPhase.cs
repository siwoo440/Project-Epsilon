namespace ProjectEpsilon.Enemies // 적 영역
{ // 이름 공간 시작
    public sealed class PrototypeBossPhase // 시험 보스 단계 상태
    { // 형식 시작
        public bool IsPhaseTwo { get; private set; } // 두 번째 단계 여부

        public void Observe(float currentHealth, float maximumHealth, bool alive) // 실제 보스 체력 관찰
        { // 함수 시작
            if (!IsPhaseTwo && alive && maximumHealth > 0f && currentHealth <= maximumHealth * 0.5f) // 생존 중 절반 경계 확인
            { // 조건 시작
                IsPhaseTwo = true; // 두 번째 단계 유지
            } // 조건 종료
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
