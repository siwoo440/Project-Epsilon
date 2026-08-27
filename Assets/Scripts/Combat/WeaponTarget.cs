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

        public float CurrentHealth => currentHealth;
        public float MaximumHealth => maximumHealth;
        public bool IsAlive => currentHealth > 0f;

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
        }

        public void Configure(float health)
        {
            maximumHealth = Mathf.Max(1f, health);
            currentHealth = maximumHealth;
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
                Destroy(gameObject);
            }
        }

        public static WeaponTarget FindClosest(Vector3 origin, float maximumRange)
        {
            float safeRange = Mathf.Max(0f, maximumRange);
            float bestDistanceSquared = safeRange * safeRange;
            WeaponTarget closest = null;

            for (int index = ActiveTargets.Count - 1; index >= 0; index--)
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
    }
}
