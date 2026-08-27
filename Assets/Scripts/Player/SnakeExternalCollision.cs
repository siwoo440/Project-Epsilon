using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeExternalCollision : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeInvulnerability invulnerability;

        [SerializeField] private int enemyBodyLoss = 1;
        [SerializeField] private float enemyContactProtection = 1f;
        [SerializeField] private int obstacleDamage = 25;
        [SerializeField] private float obstacleInvulnerability = 1.5f;

        private float nextEnemyContactAllowedTime;

        public bool EnemyContactProtected =>
            Time.time < nextEnemyContactAllowedTime;

        public float EnemyContactProtectionRemaining =>
            Mathf.Max(0f, nextEnemyContactAllowedTime - Time.time);

        public void Bind(
            SnakeBodyManager manager,
            SnakeHealth snakeHealth,
            SnakeInvulnerability invulnerabilityController,
            int directEnemyBodyLoss,
            float directEnemyProtection,
            int obstacleSharedDamage,
            float obstacleFullInvulnerability
        )
        {
            bodyManager = manager;
            health = snakeHealth;
            invulnerability = invulnerabilityController;

            enemyBodyLoss = Mathf.Max(1, directEnemyBodyLoss);
            enemyContactProtection = Mathf.Max(0f, directEnemyProtection);
            obstacleDamage = Mathf.Max(0, obstacleSharedDamage);
            obstacleInvulnerability = Mathf.Max(
                0f,
                obstacleFullInvulnerability
            );

            nextEnemyContactAllowedTime = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>();

            if (hazard == null)
            {
                return;
            }

            switch (hazard.HazardType)
            {
                case SnakeContactHazardType.EnemyDirect:
                    TryApplyEnemyDirectContact();
                    break;

                case SnakeContactHazardType.Obstacle:
                    TryApplyObstacleContact();
                    break;
            }
        }

        public bool TryApplyEnemyDirectContact()
        {
            if (IsFullInvulnerable())
            {
                return false;
            }

            if (Time.time < nextEnemyContactAllowedTime)
            {
                return false;
            }

            if (bodyManager == null)
            {
                return false;
            }

            int removed = bodyManager.RemoveBodies(enemyBodyLoss);

            if (removed <= 0)
            {
                return false;
            }

            nextEnemyContactAllowedTime =
                Time.time + enemyContactProtection;

            return true;
        }

        public bool TryApplyObstacleContact()
        {
            if (IsFullInvulnerable())
            {
                return false;
            }

            if (health == null || obstacleDamage <= 0)
            {
                return false;
            }

            bool damaged = health.TakeDamage(obstacleDamage);

            if (!damaged)
            {
                return false;
            }

            invulnerability?.StartInvulnerability(
                obstacleInvulnerability
            );

            return true;
        }

        private bool IsFullInvulnerable()
        {
            return invulnerability != null &&
                invulnerability.IsInvulnerable;
        }
    }
}
