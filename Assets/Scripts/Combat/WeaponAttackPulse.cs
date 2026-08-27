using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WeaponAttackPulse : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Color startColor = Color.white;
        private float duration = 0.15f;
        private float remainingTime = 0.15f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(
            Sprite visual,
            float radius,
            Color color,
            float lifetime
        )
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            float safeRadius = Mathf.Max(0.05f, radius);

            startColor = color;
            duration = Mathf.Max(0.05f, lifetime);
            remainingTime = duration;

            spriteRenderer.sprite = visual;
            spriteRenderer.color = startColor;
            spriteRenderer.sortingOrder = 18;

            float diameter = safeRadius * 2f;

            transform.localScale = new Vector3(
                diameter,
                diameter,
                1f
            );
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (spriteRenderer == null)
            {
                return;
            }

            float alpha = Mathf.Clamp01(remainingTime / duration);
            Color fadedColor = startColor;
            fadedColor.a *= alpha;
            spriteRenderer.color = fadedColor;
        }
    }
}
