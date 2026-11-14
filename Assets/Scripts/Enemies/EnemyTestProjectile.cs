using System.Collections.Generic; // 이동 경로 충돌 목록
using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 체력과 몸통
using UnityEngine; // Unity 기능

using ProjectEpsilon.Recording; // 실제 피해 원인 기록

namespace ProjectEpsilon.Enemies // 적 영역
{
    public sealed class EnemyTestProjectile : MonoBehaviour // 사수 시험용 투사체
    {
        private Vector2 direction; // 이동 방향
        private float speed; // 이동 속도
        private float lifetime; // 남은 수명
        private int damage; // 피해량
        private DamageOrigin origin = DamageOrigin.EnemyProjectile; // 실제 발사체 피해 원인
        private SnakeHealth health; // 공용 체력 참조
        private SnakeBodyManager body; // 몸통 관리자 참조
        private SnakeMovement player; // 머리 참조
        private Rigidbody2D movementBody; // 투사체 물리체
        private bool hitConsumed; // 동일 투사체 중복 명중 방지
        private CircleCollider2D projectileCollider; // 실제 탄 충돌 범위
        private readonly List<RaycastHit2D> pathHits = new List<RaycastHit2D>(); // 재사용 이동 경로 충돌 목록
        public void Configure(Vector2 moveDirection, float moveSpeed, float duration, int hitDamage, SnakeHealth sharedHealth, SnakeBodyManager segments, SnakeMovement head, Sprite visual, DamageOrigin damageOrigin = DamageOrigin.EnemyProjectile) // 투사체와 실제 원인 설정
        {
            direction = moveDirection.normalized; // 이동 방향 저장
            hitConsumed = false; // 재사용 투사체 명중 상태 초기화
            speed = Mathf.Max(0f, moveSpeed); // 이동 속도 저장
            lifetime = Mathf.Max(0.1f, duration); // 수명 저장
            damage = Mathf.Max(0, hitDamage); // 피해량 저장
            origin = damageOrigin; // 실제 발사 원인 저장
            health = sharedHealth; // 공용 체력 저장
            body = segments; // 몸통 관리자 저장
            player = head; // 머리 저장
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>(); // 투사체 렌더러 생성
            renderer.sprite = visual; // 투사체 시각 자료
            renderer.color = new Color(1f, 0.35f, 0.85f); // 투사체 색상
            renderer.sortingOrder = 20; // 투사체 표시 순서
            transform.localScale = Vector3.one * 0.16f; // 투사체 크기
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>(); // 투사체 충돌체 생성
            projectileCollider = collider; // 실제 이동 구간 검사에 사용할 충돌체
            collider.isTrigger = true; // 충돌 감지 설정
            movementBody = gameObject.AddComponent<Rigidbody2D>(); // 투사체 물리체 생성
            movementBody.bodyType = RigidbodyType2D.Kinematic; // 물리 이동 방식
            movementBody.useFullKinematicContacts = true; // 운동학 충돌 감지
            movementBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 고속 충돌 감지
        }
        private void FixedUpdate() // 투사체 이동
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부
            {
                return; // 이동 중단
            }
            float distance = Mathf.Min(speed * Time.fixedDeltaTime, Physics2D.maxTranslationSpeed); // 실제 물리 단계의 최대 이동 거리를 반영한 검사 거리
            ContactFilter2D filter = ContactFilter2D.noFilter; // 기존 물리 계층 검사 조건
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); // 실제 충돌 계층 적용
            filter.useTriggers = true; // 실제 머리와 몸통 및 엄폐 접촉 포함
            float radius = projectileCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)); // 실제 크기의 탄 충돌 반경
            Physics2D.CircleCast(movementBody.position, radius, direction, filter, pathHits, distance); // 실제 이동 구간의 충돌 거리순 조회
            foreach (RaycastHit2D hit in pathHits) // 실제 이동 구간의 충돌 순회
            { // 반복 시작
                Collider2D other = hit.collider; // 실제 충돌 대상
                if (other == null || other == projectileCollider || Physics2D.GetIgnoreCollision(projectileCollider, other)) // 자기 자신과 무시된 충돌 제외
                { // 조건 시작
                    continue; // 다음 실제 충돌 확인
                } // 조건 종료
                ProcessHit(other); // 기존 엄폐와 피해 예약 재사용
                if (hitConsumed || !isActiveAndEnabled) // 실제 명중 또는 엄폐 차단 확인
                { // 조건 시작
                    return; // 제거될 탄의 추가 이동 방지
                } // 조건 종료
            } // 반복 종료
            movementBody.MovePosition(movementBody.position + direction * speed * Time.fixedDeltaTime); // 기존 물리 이동과 속도 상한 유지
            lifetime -= Time.fixedDeltaTime; // 남은 수명 감소
            if (lifetime <= 0f) // 수명 종료 확인
            {
                Destroy(gameObject); // 투사체 제거
            }
        }
        private void OnTriggerEnter2D(Collider2D other) // 투사체 명중 처리
        { // 함수 시작
            ProcessHit(other); // 기존 물리 접촉 처리 재사용
        } // 함수 종료
        private void ProcessHit(Collider2D other) // 이동 구간과 실제 접촉의 공통 처리
        { // 함수 시작
            if (!isActiveAndEnabled || hitConsumed || player == null || health == null) // 활성 상태와 중복 명중 확인
            {
                return; // 명중 처리 생략
            }
            SnakeContactHazard cover = other.GetComponent<SnakeContactHazard>(); // 기존 장애물 엄폐 조회
            if (cover != null && cover.BlocksProjectiles) // 실제 엄폐 충돌 확인
            {
                hitConsumed = true; // 같은 프레임 중복 명중 차단
                gameObject.SetActive(false); // 지연 파괴 전 후속 접촉 차단
                Destroy(gameObject); // 엄폐물에 막힌 적 탄 제거
                return; // 플레이어 명중 처리 중단
            }
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 명중 몸통 조회
            bool isHead = other.transform == player.transform; // 머리 명중 여부
            bool isBody = segment != null && body != null && segment.transform.IsChildOf(body.transform); // 몸통 명중 여부
            if (!isHead && !isBody) // 플레이어 명중 확인
            {
                return; // 다른 충돌 무시
            }
            hitConsumed = true; // 파괴 전 중복 명중 차단
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 물리 스텝 충돌 처리기 조회
            if (resolver != null && resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            {
                resolver.QueueHealthDamage(health, damage, origin); // 파괴 전 대상과 피해량 값 예약
            }
            else // 충돌 처리기 부재
            {
                Debug.LogError("[Project Epsilon] 투사체 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
                health.TakeDamage(damage, origin); // 기존 즉시 피해 대체
            }
            Destroy(gameObject); // 명중 투사체 제거
        }
    }
}
