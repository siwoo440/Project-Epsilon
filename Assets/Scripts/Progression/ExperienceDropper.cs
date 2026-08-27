using ProjectEpsilon.Combat;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [RequireComponent(typeof(WeaponTarget))]
    public sealed class ExperienceDropper : MonoBehaviour
    {
        [SerializeField] private WeaponTarget target;
        [Min(1)] [SerializeField] private int experienceValue = 1;
        [SerializeField] private Sprite pickupSprite;

        private bool subscribed;

        private void Awake()
        {
            EnsureTarget();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Configure(
            int value,
            Sprite visual
        )
        {
            experienceValue = Mathf.Max(1, value);
            pickupSprite = visual;

            EnsureTarget();
            Subscribe();
        }

        private void EnsureTarget()
        {
            if (target == null)
            {
                target = GetComponent<WeaponTarget>();
            }
        }

        private void Subscribe()
        {
            EnsureTarget();

            if (subscribed || target == null)
            {
                return;
            }

            target.Died += HandleTargetDied;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || target == null)
            {
                subscribed = false;
                return;
            }

            target.Died -= HandleTargetDied;
            subscribed = false;
        }

        private void HandleTargetDied(WeaponTarget deadTarget)
        {
            SnakeExperience experience = SnakeExperience.Current;

            if (experience == null)
            {
                experience =
                    Object.FindFirstObjectByType<SnakeExperience>();
            }

            ExperiencePickup.Spawn(
                deadTarget.transform.position,
                experienceValue,
                pickupSprite,
                experience
            );
        }
    }
}
