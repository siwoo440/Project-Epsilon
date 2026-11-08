using System.Collections.Generic; // 명중 대상 목록
using ProjectEpsilon.Core;
using ProjectEpsilon.Data; // 무기 전달 설정
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
        private readonly HashSet<WeaponTarget> hitTargets = new HashSet<WeaponTarget>(); // 중복 명중 방지 목록
        private int remainingHits = 1; // 남은 명중 횟수
        private bool transferAttempted; // 추가 전달 처리 여부
        private float transferRange; // 발사 시점 전달 반경
        private float transferDamageMultiplier; // 발사 시점 전달 피해 배율
        private int configurationVersion; // 발사 구성 세대

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

            configurationVersion++; // 새로운 발사 세대 기록
            attackSnapshot = snapshot; // 공격 정보 저장
            transferRange = snapshot.Weapon == null ? 0f : snapshot.Weapon.GradeThreeTransferRange; // 발사 시점 전달 반경 저장
            transferDamageMultiplier = snapshot.Weapon == null ? 0f : snapshot.Weapon.GetTransferDamageMultiplier(snapshot.Grade); // 발사 시점 등급별 전달 배율 저장
            hitTargets.Clear(); // 이전 명중 기록 초기화
            transferAttempted = false; // 이전 전달 상태 초기화
            remainingHits = snapshot.Weapon == null ? 1 : snapshot.Weapon.ProjectileHitLimit; // 무기별 명중 수 적용
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

        private void OnTriggerEnter2D(Collider2D other) // 투사체 명중 처리
        { // 메서드 시작
            WeaponTarget target = other.GetComponent<WeaponTarget>(); // 최초 명중 대상
            if (!isActiveAndEnabled || remainingHits <= 0 || target == null || !target.isActiveAndEnabled || !target.IsAlive || !hitTargets.Add(target)) // 활성 상태와 명중 한도 확인
            { // 조건 시작
                return; // 잘못된 명중 무시
            } // 조건 끝
            int hitVersion = configurationVersion; // 현재 발사 세대 보관
            remainingHits--; // 동기 재진입 전 명중 횟수 확정
            WeaponTarget transfer = null; // 추가 전달 대상 초기화
            WeaponData weapon = attackSnapshot.Weapon; // 발사한 무기 데이터
            if (!transferAttempted && attackSnapshot.Grade >= 3 && weapon != null && transferRange > 0f) // 삼 등급 추가 전달 확인
            { // 조건 시작
                transferAttempted = true; // 투사체당 한 번 전달 시도
                transfer = WeaponTarget.FindClosest(target.transform.position, transferRange, target); // 최초 적 주변의 다른 최근접 적
            } // 조건 끝
            ApplyProjectileHit(attackSnapshot, target, transform.position); // 최초 대상 속성 명중
            if (!isActiveAndEnabled || configurationVersion != hitVersion) // 비활성 또는 재설정 확인
            { // 조건 시작
                return; // 이전 발사의 후속 처리 차단
            } // 조건 끝
            if (transfer != null && transfer.isActiveAndEnabled && transfer.IsAlive && hitTargets.Add(transfer)) // 유효한 미명중 전달 대상 확인
            { // 조건 시작
                WeaponAttributeAttackSnapshot additional = new WeaponAttributeAttackSnapshot(attackSnapshot.Weapon, attackSnapshot.Attribute, attackSnapshot.AttributeCount, attackSnapshot.SynergyStage, attackSnapshot.Grade, attackSnapshot.Origin, attackSnapshot.DirectDamage * transferDamageMultiplier); // 발사 정보와 전달 피해 배율
                ApplyProjectileHit(additional, transfer, transfer.transform.position); // 추가 대상 속성 명중
                if (!isActiveAndEnabled || configurationVersion != hitVersion) // 추가 명중 중 수명 변경 확인
                { // 조건 시작
                    return; // 이전 발사의 파괴 차단
                } // 조건 끝
            } // 조건 끝
            if (remainingHits <= 0) // 관통 횟수 소진 확인
            { // 조건 시작
                Destroy(gameObject); // 투사체 제거
            } // 조건 끝
        } // 메서드 끝

        private void ApplyProjectileHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 position) // 투사체 속성 명중 적용
        { // 메서드 시작
            if (attributeEffects != null) // 속성 효과 연결 확인
            { // 조건 시작
                attributeEffects.ApplyHit(attack, target, position); // 기존 통합 명중 적용
                return; // 기본 피해 중복 방지
            } // 조건 끝
            target.TakeDamage(attack.DirectDamage); // 효과 관리자 없는 기본 피해
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
