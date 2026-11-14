using ProjectEpsilon.Core; // 게임 진행 상태
using UnityEngine; // Unity 구성 요소

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetCorrosiveCurseStatus : MonoBehaviour // 반응 전용 공격과 방어 약화
    {
        private float remainingDuration; // 반응 남은 시간
        private float outgoingMultiplier = 1f; // 주는 피해 배율
        private float incomingMultiplier = 1f; // 받는 피해 배율
        public bool IsActive => remainingDuration > 0f; // 실제 부식 저주 지속 여부
        public float OutgoingMultiplier => remainingDuration > 0f ? outgoingMultiplier : 1f; // 활성 공격력 배율
        public float IncomingMultiplier => remainingDuration > 0f ? incomingMultiplier : 1f; // 활성 받는 피해 배율

        public void Apply(float duration, float outgoing, float incoming) // 반응 약화 갱신
        {
            remainingDuration = Mathf.Max(0f, duration); // 안전한 지속 시간 저장
            outgoingMultiplier = Mathf.Clamp01(outgoing); // 안전한 공격력 배율 저장
            incomingMultiplier = Mathf.Max(1f, incoming); // 안전한 받는 피해 배율 저장
            if (IsActive) // 실제 지속 시간 확인
            { // 조건 시작
                WeaponTargetStatusIndicators.Ensure(GetComponent<WeaponTarget>()); // 실제 부식 저주 표시 연결
            } // 조건 종료
        }

        private void Update() // 반응 약화 지속 시간 처리
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 전투 정지 여부 확인
            {
                return; // 일시정지 중 갱신 방지
            }
            remainingDuration = Mathf.Max(0f, remainingDuration - Time.deltaTime); // 남은 반응 시간 감소
            if (remainingDuration <= 0f) // 약화 종료 확인
            {
                Destroy(this); // 종료된 반응 제거
            }
        }
    }
}
