using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    public sealed class WeaponTarget : MonoBehaviour
    {
        private static readonly List<WeaponTarget> ActiveTargets =
            new List<WeaponTarget>();

        [SerializeField] private float maximumHealth = 30f;
        [SerializeField] private float currentHealth = 30f;

        private bool deathHandled;

        public event Action<WeaponTarget> Died;

        public float CurrentHealth => currentHealth;
        public float MaximumHealth => maximumHealth;
        public bool IsAlive => !deathHandled && currentHealth > 0f;

        private void OnEnable()
        {
            if (!ActiveTargets.Contains(this))
            {
                ActiveTargets.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveTargets.Remove(this);
        }

        private void Awake()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);

            if (currentHealth <= 0f || currentHealth > maximumHealth)
            {
                currentHealth = maximumHealth;
            }

            deathHandled = false;
        }

        public void Configure(float health)
        {
            maximumHealth = Mathf.Max(1f, health);
            currentHealth = maximumHealth;
            deathHandled = false;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }

            float safeDamage = Mathf.Max(0f, damage);

            if (safeDamage <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - safeDamage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (deathHandled)
            {
                return;
            }

            deathHandled = true;
            currentHealth = 0f;

            Died?.Invoke(this);
            Destroy(gameObject);
        }

        public static WeaponTarget FindClosest(
            Vector3 origin,
            float maximumRange
        )
        {
            float safeRange = Mathf.Max(0f, maximumRange);
            float bestDistanceSquared = safeRange * safeRange;
            WeaponTarget closest = null;

            for (int index = ActiveTargets.Count - 1;
                index >= 0;
                index--)
            {
                WeaponTarget target = ActiveTargets[index];

                if (target == null)
                {
                    ActiveTargets.RemoveAt(index);
                    continue;
                }

                if (!target.isActiveAndEnabled || !target.IsAlive)
                {
                    continue;
                }

                float distanceSquared =
                    (target.transform.position - origin).sqrMagnitude;

                if (distanceSquared > bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                closest = target;
            }

            return closest;
        }

        public static int DamageAllInRange(
            Vector3 origin,
            float maximumRange,
            float damage
        )
        {
            float safeRange = Mathf.Max(0f, maximumRange);
            float safeDamage = Mathf.Max(0f, damage);

            if (safeRange <= 0f || safeDamage <= 0f)
            {
                return 0;
            }

            float rangeSquared = safeRange * safeRange;
            int hitCount = 0;

            for (int index = ActiveTargets.Count - 1;
                index >= 0;
                index--)
            {
                WeaponTarget target = ActiveTargets[index];

                if (target == null)
                {
                    ActiveTargets.RemoveAt(index);
                    continue;
                }

                if (!target.isActiveAndEnabled || !target.IsAlive)
                {
                    continue;
                }

                float distanceSquared =
                    (target.transform.position - origin).sqrMagnitude;

                if (distanceSquared > rangeSquared)
                {
                    continue;
                }

                target.TakeDamage(safeDamage);
                hitCount++;
            }

            return hitCount;
        }
    }
}
