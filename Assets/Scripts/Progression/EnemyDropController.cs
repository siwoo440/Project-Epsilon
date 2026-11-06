using ProjectEpsilon.Combat;
using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [RequireComponent(typeof(WeaponTarget))]
    public sealed class EnemyDropController : MonoBehaviour
    {
        [SerializeField] private WeaponTarget target;

        [Header("Experience")]
        [Min(1)]
        [SerializeField] private int experienceValue = 1;
        [SerializeField] private int experienceGemCount = 1; // 경험치 보석 개수

        [Header("Recovery")]
        [Range(0f, 1f)]
        [SerializeField] private float healDropChance = 0.25f;

        [Range(0f, 1f)]
        [SerializeField] private float bodyRepairDropChance = 0.10f;

        [Min(1)]
        [SerializeField] private int healAmount = 15;

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
            int experience,
            int healing,
            float healChance,
            float repairChance,
            Sprite visual, // 보석 시각 자료
            int gemCount = 1 // 경험치 보석 개수
        )
        {
            experienceValue =
                Mathf.Max(
                    1,
                    experience
                );

            healAmount =
                Mathf.Max(
                    1,
                    healing
                );

            healDropChance =
                Mathf.Clamp01(
                    healChance
                );

            bodyRepairDropChance =
                Mathf.Clamp01(
                    repairChance
                );

            pickupSprite =
                visual;
            experienceGemCount = Mathf.Clamp(gemCount, 1, experienceValue); // 경험치 보석 수 제한

            EnsureTarget();
            Subscribe();
        }

        private void EnsureTarget()
        {
            if (target == null)
            {
                target =
                    GetComponent<WeaponTarget>();
            }
        }

        private void Subscribe()
        {
            EnsureTarget();

            if (subscribed ||
                target == null)
            {
                return;
            }

            target.Died +=
                HandleTargetDied;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed ||
                target == null)
            {
                subscribed = false;
                return;
            }

            target.Died -=
                HandleTargetDied;

            subscribed = false;
        }

        private void HandleTargetDied(
            WeaponTarget deadTarget
        )
        {
            if (deadTarget == null)
            {
                return;
            }

            SnakeExperience experience =
                SnakeExperience.Current;

            if (experience == null)
            {
                experience =
                    Object.FindFirstObjectByType<SnakeExperience>();
            }

            SnakeHealth health =
                Object.FindFirstObjectByType<SnakeHealth>();

            SnakeBodyManager bodyManager =
                Object.FindFirstObjectByType<SnakeBodyManager>();

            Transform receiver =
                health == null
                    ? null
                    : health.transform;

            Vector3 position =
                deadTarget.transform.position;

            int count = Mathf.Clamp(experienceGemCount, 1, experienceValue); // 유효 보석 수 계산
            for (int index = 0; index < count; index++) // 경험치 보석 생성 반복
            { // 반복 시작
                int gemValue = experienceValue / count + (index < experienceValue % count ? 1 : 0); // 보석별 경험치 분배
                float offset = (index - (count - 1) * 0.5f) * 0.3f; // 보석 간 간격 계산
                ExperiencePickup.Spawn(position + Vector3.right * offset, gemValue, pickupSprite, experience); // 경험치 보석 생성
            } // 반복 끝

            if (health != null &&
                Random.value <=
                healDropChance)
            {
                RecoveryPickup.Spawn(
                    position +
                    Vector3.right *
                    0.22f,
                    receiver,
                    health,
                    bodyManager,
                    RecoveryPickupType.Heal,
                    healAmount,
                    pickupSprite
                );
            }

            if (bodyManager != null &&
                Random.value <=
                bodyRepairDropChance)
            {
                RecoveryPickup.Spawn(
                    position +
                    Vector3.left *
                    0.22f,
                    receiver,
                    health,
                    bodyManager,
                    RecoveryPickupType.BodyRepair,
                    1,
                    pickupSprite
                );
            }
        }
    }
}
