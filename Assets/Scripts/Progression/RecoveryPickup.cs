using ProjectEpsilon.Core;
using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    public enum RecoveryPickupType
    {
        Heal,
        BodyRepair
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class RecoveryPickup : MonoBehaviour
    {
        [SerializeField] private Transform receiver;
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private RecoveryPickupType pickupType =
            RecoveryPickupType.Heal;

        [Min(1)]
        [SerializeField] private int value = 15;

        [Min(0.1f)]
        [SerializeField] private float attractionRange = 2.6f;

        [Min(0.1f)]
        [SerializeField] private float attractionSpeed = 6f;

        [Min(0.01f)]
        [SerializeField] private float collectDistance = 0.22f;

        private SpriteRenderer spriteRenderer;
        private bool collected;

        private void Awake()
        {
            spriteRenderer =
                GetComponent<SpriteRenderer>();
        }

        public void Configure(
            Transform target,
            SnakeHealth snakeHealth,
            SnakeBodyManager manager,
            RecoveryPickupType type,
            int amount,
            Sprite visual
        )
        {
            receiver = target;
            health = snakeHealth;
            bodyManager = manager;
            pickupType = type;
            value = Mathf.Max(1, amount);
            collected = false;

            if (spriteRenderer == null)
            {
                spriteRenderer =
                    GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite =
                visual;

            spriteRenderer.sortingOrder =
                17;

            if (pickupType ==
                RecoveryPickupType.Heal)
            {
                spriteRenderer.color =
                    new Color(
                        0.35f,
                        1f,
                        0.45f,
                        1f
                    );

                transform.localScale =
                    new Vector3(
                        0.27f,
                        0.27f,
                        1f
                    );
            }
            else
            {
                spriteRenderer.color =
                    new Color(
                        0.35f,
                        0.85f,
                        1f,
                        1f
                    );

                transform.localScale =
                    new Vector3(
                        0.31f,
                        0.31f,
                        1f
                    );
            }
        }

        private void Update()
        {
            if (collected ||
                !CanApply())
            {
                return;
            }

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            ResolveReceiver();

            if (receiver == null)
            {
                return;
            }

            Vector3 targetPosition =
                receiver.position;

            Vector3 delta =
                targetPosition -
                transform.position;

            float collectDistanceSquared =
                collectDistance *
                collectDistance;

            if (delta.sqrMagnitude <=
                collectDistanceSquared)
            {
                Collect();
                return;
            }

            float attractionRangeSquared =
                attractionRange *
                attractionRange;

            if (delta.sqrMagnitude >
                attractionRangeSquared)
            {
                return;
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    attractionSpeed *
                    Time.deltaTime
                );

            if ((targetPosition -
                transform.position)
                .sqrMagnitude <=
                collectDistanceSquared)
            {
                Collect();
            }
        }

        private bool CanApply()
        {
            ResolveSystems();

            if (pickupType ==
                RecoveryPickupType.Heal)
            {
                return
                    health != null &&
                    health.CurrentHealth <
                        health.MaximumHealth &&
                    bodyManager != null &&
                    bodyManager.CurrentBodyCount > 0;
            }

            return
                bodyManager != null &&
                bodyManager.MissingRepairableBodyCount > 0;
        }

        private void Collect()
        {
            if (collected)
            {
                return;
            }

            bool applied;

            if (pickupType ==
                RecoveryPickupType.Heal)
            {
                applied =
                    health != null &&
                    health.Heal(value);
            }
            else
            {
                applied =
                    bodyManager != null &&
                    bodyManager.TryRepairBody();
            }

            if (!applied)
            {
                return;
            }

            collected = true;
            Destroy(gameObject);
        }

        private void ResolveReceiver()
        {
            if (receiver != null)
            {
                return;
            }

            ResolveSystems();

            if (health != null)
            {
                receiver =
                    health.transform;
            }
        }

        private void ResolveSystems()
        {
            if (health == null)
            {
                health =
                    Object.FindFirstObjectByType<SnakeHealth>();
            }

            if (bodyManager == null)
            {
                bodyManager =
                    Object.FindFirstObjectByType<SnakeBodyManager>();
            }
        }

        public static RecoveryPickup Spawn(
            Vector3 position,
            Transform receiver,
            SnakeHealth health,
            SnakeBodyManager bodyManager,
            RecoveryPickupType type,
            int value,
            Sprite visual
        )
        {
            GameObject pickupObject =
                new GameObject(
                    type ==
                    RecoveryPickupType.Heal
                        ? "Pickup_Heal"
                        : "Pickup_BodyRepair"
                );

            pickupObject.transform.position =
                position;

            RecoveryPickup pickup =
                pickupObject.AddComponent<RecoveryPickup>();

            pickup.Configure(
                receiver,
                health,
                bodyManager,
                type,
                value,
                visual
            );

            return pickup;
        }
    }
}
