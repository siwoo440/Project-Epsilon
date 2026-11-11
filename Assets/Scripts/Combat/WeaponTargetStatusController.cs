using System; // 이벤트 형식 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetStatusController : MonoBehaviour // 대상 상태 관리자
    {
        [SerializeField] private int slowPriority; // 감속 우선순위
        [SerializeField] private float slowMultiplier = 1f; // 감속 이동 배율
        [SerializeField] private float slowRemaining; // 감속 남은 시간
        [SerializeField] private int stopPriority; // 정지 우선순위
        [SerializeField] private float stopRemaining; // 정지 남은 시간
        [SerializeField] private bool coldFreezeResistant; // 냉기 빙결 저항 여부
        [SerializeField] private float coldFreezeSlowMultiplier = 0.5f; // 빙결 대체 강한 감속 임시값
        private int coldResistancePriority; // 보스 냉기 저항 우선순위
        private float coldResistanceRemaining; // 보스 강한 감속 남은 시간

        public event Action StatusChanged; // 상태 변경 알림

        public bool IsSlowActive => WeaponTargetStatusRules.IsActive(slowRemaining); // 감속 활성 반환
        public bool IsStopActive => WeaponTargetStatusRules.IsActive(stopRemaining); // 정지 활성 반환
        public int SlowPriority => slowPriority; // 감속 우선순위 반환
        public int StopPriority => stopPriority; // 정지 우선순위 반환
        public float SlowRemaining => slowRemaining; // 감속 남은 시간 반환
        public float StopRemaining => stopRemaining; // 정지 남은 시간 반환
        public bool IsColdResistanceSlowActive => WeaponTargetStatusRules.IsActive(coldResistanceRemaining); // 강한 냉기 감속 여부
        public float CurrentMovementMultiplier // 기존 상태와 강한 감속의 최종 배율
        {
            get // 이동 배율 조회
            {
                float movementMultiplier = WeaponTargetStatusRules.ResolveMovementMultiplier(IsSlowActive, slowMultiplier, IsStopActive); // 기존 정지와 감속 배율
                return IsColdResistanceSlowActive ? Mathf.Min(movementMultiplier, coldFreezeSlowMultiplier) : movementMultiplier; // 강한 감속 병합 결과
            }
        }

        private void Update() // 상태 시간 갱신
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부 확인
            {
                return; // 시간 갱신 중단
            }

            bool changed = false; // 변경 여부 초기화

            if (IsSlowActive) // 감속 활성 확인
            {
                slowRemaining = WeaponTargetStatusRules.AdvanceDuration(slowRemaining, Time.deltaTime); // 감속 시간 감소

                if (!IsSlowActive) // 감속 종료 확인
                {
                    slowPriority = 0; // 감속 우선순위 초기화
                    slowMultiplier = 1f; // 감속 배율 초기화
                    changed = true; // 변경 표시
                }
            }

            if (IsStopActive) // 정지 활성 확인
            {
                stopRemaining = WeaponTargetStatusRules.AdvanceDuration(stopRemaining, Time.deltaTime); // 정지 시간 감소

                if (!IsStopActive) // 정지 종료 확인
                {
                    stopPriority = 0; // 정지 우선순위 초기화
                    changed = true; // 변경 표시
                }
            }

            if (IsColdResistanceSlowActive) // 보스 강한 감속 확인
            {
                coldResistanceRemaining = WeaponTargetStatusRules.AdvanceDuration(coldResistanceRemaining, Time.deltaTime); // 강한 감속 시간 감소
                if (!IsColdResistanceSlowActive) // 강한 감속 종료 확인
                {
                    coldResistancePriority = 0; // 강한 감속 우선순위 초기화
                    changed = true; // 실제 상태 변경 표시
                }
            }

            if (changed) // 상태 변경 확인
            {
                StatusChanged?.Invoke(); // 변경 알림 전달
            }
        }

        public bool ApplySlow(int priority, float movementMultiplier, float duration) // 감속 적용
        {
            float safeDuration = WeaponTargetStatusRules.NormalizeDuration(duration); // 지속 시간 정규화

            if (safeDuration <= 0f || !WeaponTargetStatusRules.ShouldReplace(slowPriority, priority)) // 적용 가능 여부 확인
            {
                return false; // 적용 실패
            }

            slowPriority = WeaponTargetStatusRules.NormalizePriority(priority); // 감속 우선순위 저장
            slowMultiplier = WeaponTargetStatusRules.NormalizeSlowMultiplier(movementMultiplier); // 감속 배율 저장
            slowRemaining = safeDuration; // 감속 시간 저장
            StatusChanged?.Invoke(); // 변경 알림 전달
            return true; // 적용 성공
        }

        public bool ApplyStop(int priority, float duration) // 정지 적용
        {
            float safeDuration = WeaponTargetStatusRules.NormalizeDuration(duration); // 지속 시간 정규화

            if (safeDuration <= 0f || !WeaponTargetStatusRules.ShouldReplace(stopPriority, priority)) // 적용 가능 여부 확인
            {
                return false; // 적용 실패
            }

            stopPriority = WeaponTargetStatusRules.NormalizePriority(priority); // 정지 우선순위 저장
            stopRemaining = safeDuration; // 정지 시간 저장
            StatusChanged?.Invoke(); // 변경 알림 전달
            return true; // 적용 성공
        }

        public void ConfigureColdFreezeResistance(bool resistant, float strongSlowMultiplier) // 냉기 빙결 저항 설정
        {
            bool wasActive = IsColdResistanceSlowActive; // 기존 강한 감속 여부
            coldFreezeResistant = resistant; // 실제 빙결 저항 설정
            coldFreezeSlowMultiplier = WeaponTargetStatusRules.NormalizeSlowMultiplier(strongSlowMultiplier); // 안전한 강한 감속 배율
            if (!resistant) // 저항 해제 확인
            {
                coldResistancePriority = 0; // 강한 감속 우선순위 해제
                coldResistanceRemaining = 0f; // 강한 감속 시간 해제
            }
            if (wasActive) // 기존 강한 감속 확인
            {
                StatusChanged?.Invoke(); // 변경된 저항 상태 알림
            }
        }

        public bool ApplyColdFreeze(int priority, float duration) // 냉기 빙결 또는 저항 적용
        {
            if (!coldFreezeResistant) // 일반 적 빙결 확인
            {
                return ApplyStop(priority, duration); // 기존 정지 상태 재사용
            }
            float safeDuration = WeaponTargetStatusRules.NormalizeDuration(duration); // 안전한 지속 시간
            if (safeDuration <= 0f || !WeaponTargetStatusRules.ShouldReplace(coldResistancePriority, priority)) // 강한 감속 적용 가능 확인
            {
                return false; // 적용 실패
            }
            coldResistancePriority = WeaponTargetStatusRules.NormalizePriority(priority); // 강한 감속 우선순위 저장
            coldResistanceRemaining = safeDuration; // 강한 감속 지속 시간 저장
            StatusChanged?.Invoke(); // 실제 상태 변경 알림
            return true; // 보스 강한 감속 적용
        }

        public void ClearAll() // 모든 상태 해제
        {
            bool hadStatus = IsSlowActive || IsStopActive || IsColdResistanceSlowActive; // 기존 상태 여부 확인
            slowPriority = 0; // 감속 우선순위 초기화
            slowMultiplier = 1f; // 감속 배율 초기화
            slowRemaining = 0f; // 감속 시간 초기화
            stopPriority = 0; // 정지 우선순위 초기화
            stopRemaining = 0f; // 정지 시간 초기화
            coldResistancePriority = 0; // 강한 감속 우선순위 초기화
            coldResistanceRemaining = 0f; // 강한 감속 시간 초기화

            if (hadStatus) // 실제 변경 확인
            {
                StatusChanged?.Invoke(); // 변경 알림 전달
            }
        }
    }
}
