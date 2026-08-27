using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ExperiencePickup : MonoBehaviour
    {
        [SerializeField] private SnakeExperience receiver;
        [Min(1)] [SerializeField] private int experienceValue = 1;
        [Min(0.1f)] [SerializeField] private float attractionRange = 2.6f;
        [Min(0.1f)] [SerializeField] private float attractionSpeed = 6f;
        [Min(0.01f)] [SerializeField] private float collectDistance = 0.22f;

        private SpriteRenderer spriteRenderer;
        private bool collected;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(
            SnakeExperience experienceReceiver,
            int value,
            Sprite visual
        )
        {
            receiver = experienceReceiver;
            experienceValue = Mathf.Max(1, value);
            collected = false;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = visual;
            spriteRenderer.color = new Color(1f, 0.9f, 0.25f, 1f);
            spriteRenderer.sortingOrder = 16;

            float scale = ResolveVisualScale(experienceValue);
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void Update()
        {
            if (collected)
            {
                return;
            }

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            if (receiver == null)
            {
                receiver = SnakeExperience.Current;

                if (receiver == null)
                {
                    return;
                }
            }

            Vector3 targetPosition = receiver.transform.position;
            Vector3 delta = targetPosition - transform.position;
            float collectDistanceSquared =
                collectDistance * collectDistance;

            if (delta.sqrMagnitude <= collectDistanceSquared)
            {
                Collect();
                return;
            }

            float attractionRangeSquared =
                attractionRange * attractionRange;

            if (delta.sqrMagnitude > attractionRangeSquared)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                attractionSpeed * Time.deltaTime
            );

            if ((targetPosition - transform.position).sqrMagnitude <=
                collectDistanceSquared)
            {
                Collect();
            }
        }

        private void Collect()
        {
            if (collected || receiver == null)
            {
                return;
            }

            collected = true;
            receiver.AddExperience(experienceValue);
            Destroy(gameObject);
        }

        private static float ResolveVisualScale(int value)
        {
            if (value >= 20)
            {
                return 0.34f;
            }

            if (value >= 5)
            {
                return 0.26f;
            }

            return 0.18f;
        }

        public static ExperiencePickup Spawn(
            Vector3 position,
            int value,
            Sprite visual,
            SnakeExperience receiver
        )
        {
            GameObject pickupObject =
                new GameObject($"XP_Gem_{Mathf.Max(1, value)}");

            pickupObject.transform.position = position;

            ExperiencePickup pickup =
                pickupObject.AddComponent<ExperiencePickup>();

            pickup.Configure(
                receiver,
                value,
                visual
            );

            return pickup;
        }
    }
}
