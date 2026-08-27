using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class StraightProjectile : MonoBehaviour
    {
        private Vector2 direction = Vector2.up;
        private WeaponAttributeAttackSnapshot attackSnapshot; // 공격 시점 속성 정보
        private WeaponAttributeCombatEffects attributeEffects; // 속성 효과 관리자
        private float speed = 8f;
        private float remainingLifetime = 3f;

        private void Awake()
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void Configure(
            Vector2 moveDirection,
            WeaponAttributeAttackSnapshot snapshot, // 공격 정보
            WeaponAttributeCombatEffects effects, // 속성 효과 관리자
            float projectileSpeed,
            float lifetime,
            Sprite visual
        )
        {
            direction = moveDirection.sqrMagnitude <= 0.0001f
                ? Vector2.up
                : moveDirection.normalized;

            attackSnapshot = snapshot; // 공격 정보 저장
            attributeEffects = effects; // 효과 관리자 저장
            speed = Mathf.Max(0.01f, projectileSpeed);
            remainingLifetime = Mathf.Max(0.1f, lifetime);

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = visual;
            renderer.color = new Color(0.45f, 1f, 1f, 1f);
            renderer.sortingOrder = 20;

            transform.localScale = new Vector3(0.18f, 0.18f, 1f);

            float angle =
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            transform.position +=
                (Vector3)(direction * speed * Time.deltaTime);

            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            WeaponTarget target = other.GetComponent<WeaponTarget>();

            if (target == null)
            {
                return;
            }

            if (attributeEffects != null) // 속성 효과 연결 확인
            { // 조건 시작
                attributeEffects.ApplyHit(attackSnapshot, target, transform.position); // 통합 명중 적용
            } // 조건 끝
            else // 효과 관리자 없음
            { // 대안 시작
                target.TakeDamage(attackSnapshot.DirectDamage); // 직접 피해 대체 적용
            } // 대안 끝

            Destroy(gameObject);
        }
    }
}
