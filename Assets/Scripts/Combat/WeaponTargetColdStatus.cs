using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetColdStatus : MonoBehaviour // 냉기 누적 상태
    {
        [SerializeField] private WeaponTarget target; // 대상 참조
        [SerializeField] private WeaponTargetStatusController statusController; // 이동 상태 관리자
        [SerializeField] private int synergyStage; // 적용 시너지 단계
        [SerializeField] private int stackCount; // 현재 냉기 누적
        [SerializeField] private float remainingDuration; // 냉기 남은 시간

        public int SynergyStage => synergyStage; // 현재 시너지 단계
        public int StackCount => stackCount; // 현재 누적 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환
        public bool IsFreezeReady => WeaponAttributeControlRules.IsFreezeReady(stackCount); // 빙결 준비 반환

        private void Awake() // 초기 참조 구성
        {
            ResolveReferences(); // 대상 참조 확보
        }

        private void Update() // 냉기 시간 갱신
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 게임 진행 여부 확인
            {
                return; // 시간 갱신 중단
            }

            if (target == null || !target.IsAlive) // 대상 생존 여부 확인
            {
                Destroy(this); // 냉기 상태 제거
                return; // 처리 중단
            }

            remainingDuration = WeaponTargetStatusRules.AdvanceDuration(remainingDuration, Time.deltaTime); // 냉기 시간 감소

            if (remainingDuration > 0f) // 냉기 유지 여부 확인
            {
                return; // 유지 처리 종료
            }

            synergyStage = 0; // 시너지 단계 초기화
            stackCount = 0; // 냉기 누적 초기화
            Destroy(this); // 냉기 상태 제거
        }

        public bool Apply(WeaponTarget damageTarget, int incomingStage) // 냉기 적용 요청
        {
            if (damageTarget == null || !damageTarget.IsAlive) // 대상 유효성 확인
            {
                return false; // 적용 실패
            }

            int resolvedStage = WeaponAttributeSynergyRules.ResolveStage(incomingStage); // 신규 시너지 단계 계산

            if (!WeaponAttributeControlRules.IsActiveSynergy(resolvedStage)) // 활성 단계 여부 확인
            {
                return false; // 비활성 냉기 거부
            }

            if (synergyStage > 0 && resolvedStage < synergyStage) // 낮은 단계 재적용 확인
            {
                return false; // 낮은 단계 거부
            }

            target = damageTarget; // 대상 저장
            ResolveReferences(); // 상태 관리자 확보

            if (statusController == null) // 상태 관리자 없음 확인
            {
                statusController = damageTarget.gameObject.AddComponent<WeaponTargetStatusController>(); // 상태 관리자 추가
            }

            float duration = WeaponAttributeControlRules.GetColdDuration(resolvedStage); // 냉기 지속 시간 계산
            float movementMultiplier = WeaponAttributeControlRules.GetColdMovementMultiplier(resolvedStage); // 냉기 이동 배율 계산
            int priority = WeaponAttributeControlRules.GetColdPriority(resolvedStage); // 냉기 우선순위 계산
            bool applied = statusController.ApplySlow(priority, movementMultiplier, duration); // 공통 감속 적용

            if (!applied) // 공통 감속 실패 확인
            {
                return false; // 냉기 갱신 중단
            }

            synergyStage = resolvedStage; // 적용 단계 저장
            stackCount = WeaponAttributeControlRules.ClampColdStacks(stackCount + 1); // 냉기 누적 증가
            remainingDuration = duration; // 냉기 시간 갱신
            return true; // 적용 성공
        }

        private void ResolveReferences() // 필수 참조 확보
        {
            if (target == null) // 대상 없음 확인
            {
                target = GetComponent<WeaponTarget>(); // 같은 오브젝트 대상 조회
            }

            if (statusController == null) // 상태 관리자 없음 확인
            {
                statusController = GetComponent<WeaponTargetStatusController>(); // 같은 오브젝트 상태 조회
            }
        }
    }
}
