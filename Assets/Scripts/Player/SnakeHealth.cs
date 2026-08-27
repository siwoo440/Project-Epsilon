using System;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeHealth : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeInvulnerability invulnerability;
        [SerializeField] private int maximumHealth = 100;
        [SerializeField] private int currentHealth = 100;

        private bool subscribed;

        public event Action<int, int> HealthChanged;
        public event Action BodyDepleted;

        public int CurrentHealth => currentHealth;
        public int MaximumHealth => maximumHealth;
        public bool IsInvulnerable =>
            invulnerability != null && invulnerability.IsInvulnerable;

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            NormalizeHealth();
            Subscribe();
            NotifyHealthChanged();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            SnakeBodyManager manager,
            SnakeInvulnerability invulnerabilityController,
            int healthPerBody
        )
        {
            Unsubscribe();

            bodyManager = manager;
            invulnerability = invulnerabilityController;
            maximumHealth = Mathf.Max(1, healthPerBody);
            currentHealth = maximumHealth;

            Subscribe();
            NotifyHealthChanged();
        }

        public bool TakeDamage(int damage)
        {
            int remainingDamage = Mathf.Max(0, damage);

            if (remainingDamage <= 0 || IsInvulnerable)
            {
                return false;
            }

            if (bodyManager == null || bodyManager.CurrentBodyCount <= 0)
            {
                SetHealth(0);
                return false;
            }

            while (remainingDamage > 0 && bodyManager.CurrentBodyCount > 0)
            {
                if (remainingDamage < currentHealth)
                {
                    SetHealth(currentHealth - remainingDamage);
                    remainingDamage = 0;
                    break;
                }

                remainingDamage -= currentHealth;
                SetHealth(0);

                int removed = bodyManager.RemoveBodies(1);

                if (removed <= 0 || bodyManager.CurrentBodyCount <= 0)
                {
                    SetHealth(0);
                    BodyDepleted?.Invoke();
                    break;
                }

                SetHealth(maximumHealth);
            }

            return true;
        }

        public void ResetHealth()
        {
            SetHealth(
                bodyManager != null && bodyManager.CurrentBodyCount <= 0
                    ? 0
                    : maximumHealth
            );
        }

        private void HandleBodyCountChanged(int current, int maximum)
        {
            if (current > 0 || currentHealth == 0)
            {
                return;
            }

            SetHealth(0);
            BodyDepleted?.Invoke();
        }

        private void Subscribe()
        {
            if (subscribed || bodyManager == null)
            {
                return;
            }

            bodyManager.BodyCountChanged += HandleBodyCountChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || bodyManager == null)
            {
                subscribed = false;
                return;
            }

            bodyManager.BodyCountChanged -= HandleBodyCountChanged;
            subscribed = false;
        }

        private void NormalizeHealth()
        {
            maximumHealth = Mathf.Max(1, maximumHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);
        }

        private void SetHealth(int value)
        {
            int nextHealth = Mathf.Clamp(value, 0, maximumHealth);

            if (currentHealth == nextHealth)
            {
                return;
            }

            currentHealth = nextHealth;
            NotifyHealthChanged();
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke(currentHealth, maximumHealth);
        }
    }
}
