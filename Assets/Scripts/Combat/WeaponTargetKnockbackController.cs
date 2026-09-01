using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetKnockbackController : MonoBehaviour // 대상 넉백 상태 관리자
    {
        [SerializeField] private bool knockbackImmune; // 넉백 면역 여부
        [SerializeField] private Vector2 direction; // 현재 넉백 방향
        [SerializeField] private float remainingDistance; // 남은 넉백 거리
        [SerializeField] private float remainingDuration; // 남은 넉백 시간

        public bool KnockbackImmune => knockbackImmune; // 면역 상태 반환
        public bool IsActive => !knockbackImmune && remainingDistance > 0f && remainingDuration > 0f; // 넉백 활성 상태 반환
        public float RemainingDistance => remainingDistance; // 남은 거리 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환

        public void Configure(bool immune) // 넉백 설정 구성
        {
            knockbackImmune = immune; // 면역 상태 저장

            if (knockbackImmune) // 면역 전환 확인
            {
                Clear(); // 진행 중 넉백 해제
            }
        }

        public bool Apply(Vector2 knockbackDirection, float distance, float duration) // 넉백 적용
        {
            float safeDistance = Mathf.Max(0f, distance); // 거리 정규화
            float safeDuration = Mathf.Max(0f, duration); // 시간 정규화

            if (knockbackImmune || safeDistance <= 0f || safeDuration <= 0f) // 적용 가능 여부 확인
            {
                return false; // 적용 실패
            }

            Vector2 safeDirection = knockbackDirection.sqrMagnitude <= 0.0001f ? Vector2.up : knockbackDirection.normalized; // 방향 정규화
            direction = safeDirection; // 넉백 방향 저장
            remainingDistance = safeDistance; // 넉백 거리 저장
            remainingDuration = safeDuration; // 넉백 시간 저장
            return true; // 적용 성공
        }

        public bool TryConsumeMovement(float deltaTime, out Vector2 displacement) // 프레임 넉백 이동 계산
        {
            displacement = Vector2.zero; // 기본 이동량 초기화

            if (!IsActive || deltaTime <= 0f) // 이동 가능 여부 확인
            {
                return false; // 이동 없음 반환
            }

            float stepTime = Mathf.Min(deltaTime, remainingDuration); // 실제 진행 시간 계산
            float speed = remainingDuration <= 0f ? 0f : remainingDistance / remainingDuration; // 남은 기준 속도 계산
            float stepDistance = Mathf.Min(remainingDistance, speed * stepTime); // 이번 이동 거리 계산
            displacement = direction * stepDistance; // 이동 벡터 계산
            remainingDistance = Mathf.Max(0f, remainingDistance - stepDistance); // 남은 거리 감소
            remainingDuration = Mathf.Max(0f, remainingDuration - stepTime); // 남은 시간 감소

            if (remainingDistance <= 0f || remainingDuration <= 0f) // 넉백 종료 확인
            {
                remainingDistance = 0f; // 거리 종료 정리
                remainingDuration = 0f; // 시간 종료 정리
            }

            return displacement.sqrMagnitude > 0f; // 실제 이동 여부 반환
        }

        public void Clear() // 넉백 즉시 해제
        {
            direction = Vector2.zero; // 방향 초기화
            remainingDistance = 0f; // 거리 초기화
            remainingDuration = 0f; // 시간 초기화
        }
    }
}
