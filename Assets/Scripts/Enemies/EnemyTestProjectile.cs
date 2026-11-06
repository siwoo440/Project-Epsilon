using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 플레이어 체력과 몸통
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class EnemyTestProjectile : MonoBehaviour // 사수 시험용 투사체
    { // 클래스 시작
        private Vector2 direction; // 이동 방향
        private float speed; // 이동 속도
        private float lifetime; // 남은 수명
        private int damage; // 피해량
        private SnakeHealth health; // 공용 체력 참조
        private SnakeBodyManager body; // 몸통 관리자 참조
        private SnakeMovement player; // 머리 참조
        private Rigidbody2D movementBody; // 투사체 물리체
        private bool hitConsumed; // 동일 투사체 중복 명중 방지
        public void Configure(Vector2 moveDirection, float moveSpeed, float duration, int hitDamage, SnakeHealth sharedHealth, SnakeBodyManager segments, SnakeMovement head, Sprite visual) // 투사체 설정
        { // 메서드 시작
            direction = moveDirection.normalized; // 이동 방향 저장
            hitConsumed = false; // 재사용 투사체 명중 상태 초기화
            speed = Mathf.Max(0f, moveSpeed); // 이동 속도 저장
            lifetime = Mathf.Max(0.1f, duration); // 수명 저장
            damage = Mathf.Max(0, hitDamage); // 피해량 저장
            health = sharedHealth; // 공용 체력 저장
            body = segments; // 몸통 관리자 저장
            player = head; // 머리 저장
            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>(); // 투사체 렌더러 생성
            renderer.sprite = visual; // 투사체 시각 자료
            renderer.color = new Color(1f, 0.35f, 0.85f); // 투사체 색상
            renderer.sortingOrder = 20; // 투사체 표시 순서
            transform.localScale = Vector3.one * 0.16f; // 투사체 크기
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>(); // 투사체 충돌체 생성
            collider.isTrigger = true; // 충돌 감지 설정
            movementBody = gameObject.AddComponent<Rigidbody2D>(); // 투사체 물리체 생성
            movementBody.bodyType = RigidbodyType2D.Kinematic; // 물리 이동 방식
            movementBody.useFullKinematicContacts = true; // 운동학 충돌 감지
            movementBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 고속 충돌 감지
        } // 메서드 끝
        private void FixedUpdate() // 투사체 이동
        { // 메서드 시작
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 전투 진행 여부
            { // 조건 시작
                return; // 이동 중단
            } // 조건 끝
            movementBody.MovePosition(movementBody.position + direction * speed * Time.fixedDeltaTime); // 투사체 이동
            lifetime -= Time.fixedDeltaTime; // 남은 수명 감소
            if (lifetime <= 0f) // 수명 종료 확인
            { // 조건 시작
                Destroy(gameObject); // 투사체 제거
            } // 조건 끝
        } // 메서드 끝
        private void OnTriggerEnter2D(Collider2D other) // 투사체 명중 처리
        { // 메서드 시작
            if (!isActiveAndEnabled || hitConsumed || player == null || health == null) // 활성 상태와 중복 명중 확인
            { // 조건 시작
                return; // 명중 처리 생략
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 명중 몸통 조회
            bool isHead = other.transform == player.transform; // 머리 명중 여부
            bool isBody = segment != null && body != null && segment.transform.IsChildOf(body.transform); // 몸통 명중 여부
            if (!isHead && !isBody) // 플레이어 명중 확인
            { // 조건 시작
                return; // 다른 충돌 무시
            } // 조건 끝
            hitConsumed = true; // 파괴 전 중복 명중 차단
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 물리 스텝 충돌 처리기 조회
            if (resolver != null && resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            { // 조건 시작
                resolver.QueueHealthDamage(health, damage); // 파괴 전 대상과 피해량 값 예약
            } // 조건 끝
            else // 충돌 처리기 부재
            { // 대안 시작
                Debug.LogError("[Project Epsilon] 투사체 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
                health.TakeDamage(damage); // 기존 즉시 피해 대체
            } // 대안 끝
            Destroy(gameObject); // 명중 투사체 제거
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
