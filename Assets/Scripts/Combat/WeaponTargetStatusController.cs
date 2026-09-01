using System; // 이벤트 형식 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public sealed class WeaponTargetStatusController : MonoBehaviour // 대상 상태 관리자
    { // 클래스 시작
        [SerializeField] private int slowPriority; // 감속 우선순위
        [SerializeField] private float slowMultiplier = 1f; // 감속 이동 배율
        [SerializeField] private float slowRemaining; // 감속 남은 시간
        [SerializeField] private int stopPriority; // 정지 우선순위
        [SerializeField] private float stopRemaining; // 정지 남은 시간

        public event Action StatusChanged; // 상태 변경 알림

        public bool IsSlowActive => WeaponTargetStatusRules.IsActive(slowRemaining); // 감속 활성 반환
        public bool IsStopActive => WeaponTargetStatusRules.IsActive(stopRemaining); // 정지 활성 반환
        public int SlowPriority => slowPriority; // 감속 우선순위 반환
        public int StopPriority => stopPriority; // 정지 우선순위 반환
        public float SlowRemaining => slowRemaining; // 감속 남은 시간 반환
        public float StopRemaining => stopRemaining; // 정지 남은 시간 반환
        public float CurrentMovementMultiplier => WeaponTargetStatusRules.ResolveMovementMultiplier(IsSlowActive, slowMultiplier, IsStopActive); // 최종 이동 배율 반환

        private void Update() // 상태 시간 갱신
        { // 메서드 시작
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부 확인
            { // 조건 시작
                return; // 시간 갱신 중단
            } // 조건 끝

            bool changed = false; // 변경 여부 초기화

            if (IsSlowActive) // 감속 활성 확인
            { // 조건 시작
                slowRemaining = WeaponTargetStatusRules.AdvanceDuration(slowRemaining, Time.deltaTime); // 감속 시간 감소

                if (!IsSlowActive) // 감속 종료 확인
                { // 조건 시작
                    slowPriority = 0; // 감속 우선순위 초기화
                    slowMultiplier = 1f; // 감속 배율 초기화
                    changed = true; // 변경 표시
                } // 조건 끝
            } // 조건 끝

            if (IsStopActive) // 정지 활성 확인
            { // 조건 시작
                stopRemaining = WeaponTargetStatusRules.AdvanceDuration(stopRemaining, Time.deltaTime); // 정지 시간 감소

                if (!IsStopActive) // 정지 종료 확인
                { // 조건 시작
                    stopPriority = 0; // 정지 우선순위 초기화
                    changed = true; // 변경 표시
                } // 조건 끝
            } // 조건 끝

            if (changed) // 상태 변경 확인
            { // 조건 시작
                StatusChanged?.Invoke(); // 변경 알림 전달
            } // 조건 끝
        } // 메서드 끝

        public bool ApplySlow(int priority, float movementMultiplier, float duration) // 감속 적용
        { // 메서드 시작
            float safeDuration = WeaponTargetStatusRules.NormalizeDuration(duration); // 지속 시간 정규화

            if (safeDuration <= 0f || !WeaponTargetStatusRules.ShouldReplace(slowPriority, priority)) // 적용 가능 여부 확인
            { // 조건 시작
                return false; // 적용 실패
            } // 조건 끝

            slowPriority = WeaponTargetStatusRules.NormalizePriority(priority); // 감속 우선순위 저장
            slowMultiplier = WeaponTargetStatusRules.NormalizeSlowMultiplier(movementMultiplier); // 감속 배율 저장
            slowRemaining = safeDuration; // 감속 시간 저장
            StatusChanged?.Invoke(); // 변경 알림 전달
            return true; // 적용 성공
        } // 메서드 끝

        public bool ApplyStop(int priority, float duration) // 정지 적용
        { // 메서드 시작
            float safeDuration = WeaponTargetStatusRules.NormalizeDuration(duration); // 지속 시간 정규화

            if (safeDuration <= 0f || !WeaponTargetStatusRules.ShouldReplace(stopPriority, priority)) // 적용 가능 여부 확인
            { // 조건 시작
                return false; // 적용 실패
            } // 조건 끝

            stopPriority = WeaponTargetStatusRules.NormalizePriority(priority); // 정지 우선순위 저장
            stopRemaining = safeDuration; // 정지 시간 저장
            StatusChanged?.Invoke(); // 변경 알림 전달
            return true; // 적용 성공
        } // 메서드 끝

        public void ClearAll() // 모든 상태 해제
        { // 메서드 시작
            bool hadStatus = IsSlowActive || IsStopActive; // 기존 상태 여부 확인
            slowPriority = 0; // 감속 우선순위 초기화
            slowMultiplier = 1f; // 감속 배율 초기화
            slowRemaining = 0f; // 감속 시간 초기화
            stopPriority = 0; // 정지 우선순위 초기화
            stopRemaining = 0f; // 정지 시간 초기화

            if (hadStatus) // 실제 변경 확인
            { // 조건 시작
                StatusChanged?.Invoke(); // 변경 알림 전달
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
