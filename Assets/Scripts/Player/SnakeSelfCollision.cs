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
            SnakeSegment segment = other.GetComponent<SnakeSegment>();

            if (segment == null || !segment.IsBody)
            {
                return;
            }

            TryApplySelfCollisionPenalty();
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
