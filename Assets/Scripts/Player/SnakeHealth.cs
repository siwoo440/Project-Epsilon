using System;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeHealth : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeInvulnerability invulnerability;
        [SerializeField] private SnakeShieldController shieldController; // 공유 보호막 참조
        [SerializeField] private int maximumHealth = 100;
        [SerializeField] private int currentHealth = 100;

        private bool subscribed;

        public event Action<int, int> HealthChanged;
        public event Action BodyDepleted;

        public int CurrentHealth => currentHealth;
        public int MaximumHealth => maximumHealth;
        public SnakeShieldController ShieldController => shieldController; // 공유 보호막 반환

        public bool IsInvulnerable =>
            invulnerability != null &&
            invulnerability.IsInvulnerable;

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
            invulnerability =
                invulnerabilityController;

            maximumHealth =
                Mathf.Max(
                    1,
                    healthPerBody
                );

            currentHealth =
                maximumHealth;

            Subscribe();
            NotifyHealthChanged();
        }

        public bool TakeDamage(int damage)
        {
            int remainingDamage =
                Mathf.Max(
                    0,
                    damage
                );

            if (remainingDamage <= 0 ||
                IsInvulnerable)
            {
                return false;
            }

            if (shieldController != null) // 보호막 연결 확인
            { // 조건 시작
                remainingDamage = shieldController.Absorb(remainingDamage); // 보호막 우선 흡수

                if (remainingDamage <= 0) // 완전 흡수 확인
                { // 조건 시작
                    shieldController.CompleteDamageResolution(); // 보호막 종료 효과 완료
                    return true; // 피해 처리 성공
                } // 조건 끝
            } // 조건 끝

            if (bodyManager == null ||
                bodyManager.CurrentBodyCount <= 0)
            {
                SetHealth(0);
                shieldController?.CompleteDamageResolution(); // 체력 처리 뒤 보호막 종료 완료
                return false;
            }

            while (remainingDamage > 0 &&
                bodyManager.CurrentBodyCount > 0)
            {
                if (remainingDamage <
                    currentHealth)
                {
                    SetHealth(
                        currentHealth -
                        remainingDamage
                    );

                    remainingDamage = 0;
                    break;
                }

                remainingDamage -=
                    currentHealth;

                SetHealth(0);

                int removed =
                    bodyManager.RemoveBodies(1);

                if (removed <= 0 ||
                    bodyManager.CurrentBodyCount <= 0)
                {
                    SetHealth(0);
                    BodyDepleted?.Invoke();
                    break;
                }

                SetHealth(
                    maximumHealth
                );
            }

            shieldController?.CompleteDamageResolution(); // 체력 처리 뒤 보호막 종료 완료
            return true;
        }

        public void BindShield(SnakeShieldController shield) // 보호막 참조 연결
        { // 메서드 시작
            shieldController = shield; // 보호막 저장
        } // 메서드 끝

        public bool Heal(int amount)
        {
            int safeAmount =
                Mathf.Max(
                    0,
                    amount
                );

            if (safeAmount <= 0 ||
                currentHealth >= maximumHealth ||
                bodyManager == null ||
                bodyManager.CurrentBodyCount <= 0)
            {
                return false;
            }

            SetHealth(
                currentHealth +
                safeAmount
            );

            return true;
        }

        public void ResetHealth()
        {
            SetHealth(
                bodyManager != null &&
                bodyManager.CurrentBodyCount <= 0
                    ? 0
                    : maximumHealth
            );
        }

        private void HandleBodyCountChanged(
            int current,
            int maximum
        )
        {
            if (current > 0 ||
                currentHealth == 0)
            {
                return;
            }

            SetHealth(0);
            BodyDepleted?.Invoke();
        }

        private void Subscribe()
        {
            if (subscribed ||
                bodyManager == null)
            {
                return;
            }

            bodyManager.BodyCountChanged +=
                HandleBodyCountChanged;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed ||
                bodyManager == null)
            {
                subscribed = false;
                return;
            }

            bodyManager.BodyCountChanged -=
                HandleBodyCountChanged;

            subscribed = false;
        }

        private void NormalizeHealth()
        {
            maximumHealth =
                Mathf.Max(
                    1,
                    maximumHealth
                );

            currentHealth =
                Mathf.Clamp(
                    currentHealth,
                    0,
                    maximumHealth
                );
        }

        private void SetHealth(int value)
        {
            int nextHealth =
                Mathf.Clamp(
                    value,
                    0,
                    maximumHealth
                );

            if (currentHealth ==
                nextHealth)
            {
                return;
            }

            currentHealth =
                nextHealth;

            NotifyHealthChanged();
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke(
                currentHealth,
                maximumHealth
            );
        }
    }
}
