using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    [RequireComponent(typeof(WeaponTarget))] // 공격 대상 필수
    public sealed class WeaponTargetPoisonStatus : MonoBehaviour // 독 약화 상태
    {
        [SerializeField] private WeaponTarget target; // 적용 대상 참조
        [SerializeField] private int synergyStage; // 현재 독 단계
        [SerializeField] private int stackCount; // 현재 독 중첩 수
        [SerializeField] private float remainingDuration; // 남은 지속 시간

        public int SynergyStage => synergyStage; // 현재 독 단계 반환
        public int StackCount => stackCount; // 현재 중첩 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환
        public bool IsActive => target != null && target.IsAlive && synergyStage >= 2 && remainingDuration > 0f; // 독 활성 상태 반환
        public float OutgoingDamageMultiplier => IsActive ? WeaponAttributePoisonRules.GetOutgoingDamageMultiplier(synergyStage) : 1f; // 주는 피해 배율 반환
        public float IncomingDamageMultiplier => IsActive ? WeaponAttributePoisonRules.GetIncomingDamageMultiplier(synergyStage) : 1f; // 받는 피해 배율 반환

        private void Awake() // 초기 참조 구성
        {
            ResolveTarget(); // 대상 참조 확보
        }

        private void Update() // 독 지속 시간 갱신
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임 진행 상태 확인
            {
                return; // 타이머 정지
            }

            if (target == null || !target.IsAlive) // 대상 생존 확인
            {
                Destroy(this); // 상태 제거
                return; // 갱신 중단
            }

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.deltaTime); // 남은 시간 감소

            if (remainingDuration <= 0f) // 지속 종료 확인
            {
                Destroy(this); // 상태 제거
            }
        }

        public bool Apply(WeaponTarget damageTarget, int incomingStage) // 독 상태 적용
        {
            if (damageTarget == null || !damageTarget.IsAlive) // 대상 유효성 확인
            {
                return false; // 적용 실패
            }

            if (!WeaponAttributePoisonRules.ShouldReplace(synergyStage, incomingStage)) // 단계 교체 규칙 확인
            {
                return false; // 낮은 단계 거부
            }

            target = damageTarget; // 대상 저장
            synergyStage = WeaponAttributeSynergyRules.ResolveStage(incomingStage); // 독 단계 저장
            stackCount = stackCount >= int.MaxValue ? int.MaxValue : stackCount + 1; // 독 중첩 증가
            remainingDuration = WeaponAttributePoisonRules.NormalizeDuration(WeaponAttributePoisonRules.TemporaryDuration); // 임시 지속 시간 갱신
            return true; // 적용 성공
        }

        public void Clear() // 독 상태 해제
        {
            synergyStage = 0; // 단계 초기화
            stackCount = 0; // 중첩 초기화
            remainingDuration = 0f; // 시간 초기화
            Destroy(this); // 상태 제거
        }

        private void ResolveTarget() // 대상 참조 확보
        {
            if (target == null) // 대상 없음 확인
            {
                target = GetComponent<WeaponTarget>(); // 같은 오브젝트 대상 조회
            }
        }
    }
}
