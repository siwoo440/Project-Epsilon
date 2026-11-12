using ProjectEpsilon.Core; // 게임 진행 상태
using UnityEngine; // Unity 구성 요소

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetCorrosiveBurnStatus : MonoBehaviour // 반응 전용 지속 피해
    {
        private WeaponTarget target; // 실제 피해 대상
        private float remainingDuration; // 반응 남은 시간
        private float damagePerSecond; // 반응 초당 피해

        public void Apply(WeaponTarget victim, float duration, float damage) // 반응 지속 피해 갱신
        {
            target = victim; // 실제 피해 대상 저장
            remainingDuration = Mathf.Max(0f, duration); // 안전한 지속 시간 저장
            damagePerSecond = Mathf.Max(0f, damage); // 안전한 초당 피해 저장
        }

        private void Update() // 반응 지속 피해 처리
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 전투 정지 여부 확인
            {
                return; // 일시정지 중 갱신 방지
            }
            if (target == null || !target.IsAlive || remainingDuration <= 0f) // 대상과 남은 시간 확인
            {
                Destroy(this); // 종료된 반응 제거
                return; // 피해 처리 종료
            }
            float delta = Mathf.Min(Time.deltaTime, remainingDuration); // 남은 시간에 맞춘 피해 간격
            remainingDuration = Mathf.Max(0f, remainingDuration - delta); // 남은 반응 시간 감소
            target.TakeReactionDamage(damagePerSecond * delta); // 추가 속성 부여 없는 반응 피해
        }
    }
}
