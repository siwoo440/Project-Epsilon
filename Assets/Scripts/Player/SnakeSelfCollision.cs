using UnityEngine;

namespace ProjectEpsilon.Player
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SnakeSelfCollision : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeInvulnerability invulnerability;
        [SerializeField] private int bodyLoss = 2;
        [SerializeField] private float invulnerabilityDuration = 2f;
        private bool reportedMissingResolver; // 충돌 처리기 누락 기록

        public void Bind(
            SnakeBodyManager manager,
            SnakeInvulnerability invulnerabilityController,
            int lossCount,
            float duration
        )
        {
            bodyManager = manager;
            invulnerability = invulnerabilityController;
            bodyLoss = Mathf.Max(1, lossCount);
            invulnerabilityDuration = Mathf.Max(0f, duration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled) // 충돌 처리기 활성 확인
            { // 조건 시작
                return; // 비활성 충돌 무시
            } // 조건 끝

            SnakeSegment segment = other.GetComponent<SnakeSegment>();

            if (segment == null || !segment.IsBody)
            {
                return;
            }

            SnakeExternalCollision resolver = GetComponent<SnakeExternalCollision>(); // 물리 스텝 충돌 처리기 조회
            if (resolver == null || !resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            { // 조건 시작
                if (!reportedMissingResolver) // 누락 오류 중복 확인
                { // 조건 시작
                    reportedMissingResolver = true; // 누락 오류 기록 상태
                    Debug.LogError("[Project Epsilon] 자기 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
                } // 조건 끝
                TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            resolver.QueueSelfCollision(this); // 같은 물리 스텝에 충돌 예약
        }

        public bool TryApplySelfCollisionPenalty()
        {
            if (bodyManager == null)
            {
                return false;
            }

            if (invulnerability != null && invulnerability.IsInvulnerable)
            {
                return false;
            }

            int removed = bodyManager.RemoveBodies(bodyLoss);

            if (removed <= 0)
            {
                return false;
            }

            invulnerability?.StartInvulnerability(invulnerabilityDuration);
            return true;
        }
    }
}
