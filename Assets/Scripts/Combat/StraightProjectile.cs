using System.Collections.Generic; // 명중 대상 목록
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Data; // 무기 전달 설정
using ProjectEpsilon.Player; // 장애물 엄폐 판정
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    [RequireComponent(typeof(SpriteRenderer))] // 구성 요소·이미지 설정
    [RequireComponent(typeof(CircleCollider2D))] // 구성 요소·충돌체 설정
    [RequireComponent(typeof(Rigidbody2D))] // 구성 요소 설정
    public sealed class StraightProjectile : MonoBehaviour // 투사체·행동 자료 구조
    {
        private Vector2 direction = Vector2.up; // 벡터·방향 구성 선언
        private WeaponAttributeAttackSnapshot attackSnapshot; // 공격 시점 속성 정보
        private WeaponAttributeCombatEffects attributeEffects; // 속성 효과 관리자
        private float speed = 8f; // 속도 구성 선언
        private float remainingLifetime = 3f; // 남은 값 구성 선언
        private readonly HashSet<WeaponTarget> hitTargets = new HashSet<WeaponTarget>(); // 중복 명중 방지 목록
        private int remainingHits = 1; // 남은 명중 횟수
        private bool transferAttempted; // 추가 전달 처리 여부
        private float transferRange; // 발사 시점 전달 반경
        private float transferDamageMultiplier; // 발사 시점 전달 피해 배율
        private int configurationVersion; // 발사 구성 세대
        private SpriteRenderer artworkRenderer; // 실제 충돌체와 분리된 탄 이미지
        private readonly List<RaycastHit2D> pathHits = new List<RaycastHit2D>(); // 재사용 이동 경로 충돌 목록
        private CircleCollider2D projectileCollider; // 실제 투사체 충돌 범위

        private void Awake() // 초기화 기능 선언
        {
            CircleCollider2D collider = GetComponent<CircleCollider2D>(); // 충돌체·조회 값 설정
            projectileCollider = collider; // 이동 경로 검사에 사용할 실제 충돌체
            collider.isTrigger = true; // 충돌체·접촉 값 설정
            collider.radius = 0.3f; // 충돌체·반경 값 설정

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>(); // 조회·구성 요소 값 설정
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 몸통·형식 값 설정
            rigidbody.gravityScale = 0f; // 크기 값 설정
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation; // 회전 값 설정
        }

        public void Configure( // 구성 호출 시작
            Vector2 moveDirection, // 벡터·방향 항목 전달
            WeaponAttributeAttackSnapshot snapshot, // 공격 정보
            WeaponAttributeCombatEffects effects, // 속성 효과 관리자
            float projectileSpeed, // 투사체·속도 항목 전달
            float lifetime, // 대상 항목 전달
            Sprite visual // 이미지·시각 효과 처리
        ) // 앞선 항목 종료
        {
            direction = moveDirection.sqrMagnitude <= 0.0001f // 방향 처리
                ? Vector2.up // 벡터 처리
                : moveDirection.normalized; // 방향 처리

            configurationVersion++; // 새로운 발사 세대 기록
            attackSnapshot = snapshot; // 공격 정보 저장
            transferRange = snapshot.Weapon == null ? 0f : snapshot.Weapon.GradeThreeTransferRange; // 발사 시점 전달 반경 저장
            transferDamageMultiplier = snapshot.Weapon == null ? 0f : snapshot.Weapon.GetTransferDamageMultiplier(snapshot.Grade); // 발사 시점 등급별 전달 배율 저장
            hitTargets.Clear(); // 이전 명중 기록 초기화
            transferAttempted = false; // 이전 전달 상태 초기화
            remainingHits = snapshot.Weapon == null ? 1 : snapshot.Weapon.ProjectileHitLimit; // 무기별 명중 수 적용
            attributeEffects = effects; // 효과 관리자 저장
            speed = Mathf.Max(0.01f, projectileSpeed); // 속도·최대 값 설정
            remainingLifetime = Mathf.Max(0.1f, lifetime); // 남은 값·최대 값 설정

            SpriteRenderer renderer = GetComponent<SpriteRenderer>(); // 이미지·렌더러 값 설정
            renderer.enabled = true; // 새 발사의 기본 표시 복구
            if (artworkRenderer != null) // 이전 별도 이미지 확인
            {
                artworkRenderer.gameObject.SetActive(false); // 새 설정 전 이전 이미지 숨김
            }
            renderer.sprite = visual; // 렌더러·이미지 값 설정
            renderer.color = new Color(0.45f, 1f, 1f, 1f); // 렌더러·색상 값 설정
            renderer.sortingOrder = 20; // 렌더러 값 설정

            transform.localScale = new Vector3(0.18f, 0.18f, 1f); // 위치·크기 값 설정

            float angle = // 각도 값 설정
                Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // 방향 기능 실행

            transform.rotation = Quaternion.Euler(0f, 0f, angle); // 위치·회전 값 설정
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            Vector2 start = transform.position; // 현재 프레임의 실제 시작 위치
            float distance = speed * Time.deltaTime; // 현재 프레임의 이동 거리
            int moveVersion = configurationVersion; // 이동 중 재설정 확인용 발사 세대
            ContactFilter2D filter = ContactFilter2D.noFilter; // 기존 물리 계층 검사 조건
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer)); // 실제 충돌 계층 적용
            filter.useTriggers = true; // 적과 엄폐의 접촉 충돌체 포함
            float radius = projectileCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)); // 실제 표시 크기의 충돌 반경
            Physics2D.CircleCast(start, radius, direction, filter, pathHits, distance); // 이동 경로의 충돌을 가까운 순서로 수집
            foreach (RaycastHit2D hit in pathHits) // 실제 이동 경로의 충돌 순회
            { // 반복 시작
                Collider2D other = hit.collider; // 충돌한 실제 물체
                if (other == null || other == projectileCollider || Physics2D.GetIgnoreCollision(projectileCollider, other)) // 자기 자신과 무시된 충돌 제외
                { // 조건 시작
                    continue; // 다음 충돌 확인
                } // 조건 종료
                ProcessHit(other, hit.centroid); // 기존 명중과 엄폐 처리 재사용
                if (!isActiveAndEnabled || configurationVersion != moveVersion || remainingHits <= 0) // 제거 또는 다른 발사 여부 확인
                { // 조건 시작
                    return; // 이전 발사의 추가 이동 방지
                } // 조건 종료
            } // 반복 종료
            transform.position = start + direction * distance; // 충돌 처리 후 실제 위치 이동

            remainingLifetime -= Time.deltaTime; // 남은 값·시간 값 설정

            if (remainingLifetime <= 0f) // 남은 값 조건 확인
            {
                Destroy(gameObject); // 제거·게임 기능 실행
            }
        }

        public void SetArtwork(Sprite artwork, Color tint) // 실제 탄 이미지 설정
        {
            SpriteRenderer original = GetComponent<SpriteRenderer>(); // 기존 표시기 조회
            if (artwork == null) // 별도 이미지 부재 확인
            {
                original.enabled = true; // 기존 투사체 표시 유지
                if (artworkRenderer != null) // 기존 탄 이미지 확인
                {
                    artworkRenderer.gameObject.SetActive(false); // 이전 이미지 숨김
                }
                return; // 기존 판정과 표시 유지
            }
            if (artworkRenderer == null) // 실제 자식 이미지 부재 확인
            {
                GameObject child = new GameObject("ProjectileArtwork"); // 실제 탄 이미지 객체
                child.transform.SetParent(transform, false); // 실제 투사체에 연결
                child.transform.localScale = Vector3.one * 3.3f; // 충돌 범위와 독립된 표시 크기
                artworkRenderer = child.AddComponent<SpriteRenderer>(); // 실제 탄 표시기
            }
            artworkRenderer.gameObject.SetActive(true); // 실제 탄 이미지 표시
            artworkRenderer.sprite = artwork; // 생성한 탄 이미지 연결
            artworkRenderer.color = tint; // 실제 포탑 속성 색상
            artworkRenderer.sortingOrder = 21; // 바닥과 적 위 표시
            original.enabled = false; // 기존 디버그 표시 숨김
        }

        private void OnTriggerEnter2D(Collider2D other) // 투사체 명중 처리
        { // 함수 시작
            ProcessHit(other, transform.position); // 기존 물리 접촉의 명중 처리
        } // 함수 종료

        private void ProcessHit(Collider2D other, Vector3 hitPosition) // 경로와 물리 접촉의 공통 명중
        { // 함수 시작
            if (StopForGameState()) // 실제 정지 또는 종료 상태 확인
            { // 조건 시작
                return; // 종료 후 추가 명중 차단
            } // 조건 종료
            SnakeContactHazard cover = other.GetComponent<SnakeContactHazard>(); // 기존 장애물 엄폐 조회
            if (isActiveAndEnabled && remainingHits > 0 && cover != null && cover.BlocksProjectiles) // 활성 탄과 실제 엄폐 확인
            {
                gameObject.SetActive(false); // 지연 파괴 전 후속 명중 차단
                Destroy(gameObject); // 엄폐물에 막힌 포탑 탄 제거
                return; // 적 명중 처리 중단
            }
            WeaponTarget target = other.GetComponent<WeaponTarget>(); // 최초 명중 대상
            if (!isActiveAndEnabled || remainingHits <= 0 || target == null || !target.isActiveAndEnabled || !target.IsAlive || !hitTargets.Add(target)) // 활성 상태와 명중 한도 확인
            {
                return; // 잘못된 명중 무시
            }
            int hitVersion = configurationVersion; // 현재 발사 세대 보관
            remainingHits--; // 동기 재진입 전 명중 횟수 확정
            WeaponTarget transfer = null; // 추가 전달 대상 초기화
            WeaponData weapon = attackSnapshot.Weapon; // 발사한 무기 데이터
            if (!transferAttempted && attackSnapshot.Grade >= 3 && weapon != null && transferRange > 0f) // 삼 등급 추가 전달 확인
            {
                transferAttempted = true; // 투사체당 한 번 전달 시도
                transfer = WeaponTarget.FindClosest(target.transform.position, transferRange, target); // 최초 적 주변의 다른 최근접 적
            }
            ApplyProjectileHit(attackSnapshot, target, hitPosition); // 실제 충돌 지점의 최초 대상 속성 명중
            if (!isActiveAndEnabled || configurationVersion != hitVersion) // 비활성 또는 재설정 확인
            {
                return; // 이전 발사의 후속 처리 차단
            }
            if (StopForGameState()) // 최초 명중 중 승리 또는 정지 확인
            { // 조건 시작
                return; // 종료 후 추가 대상 전달 차단
            } // 조건 종료
            if (transfer != null && transfer.isActiveAndEnabled && transfer.IsAlive && hitTargets.Add(transfer)) // 유효한 미명중 전달 대상 확인
            {
                WeaponAttributeAttackSnapshot additional = new WeaponAttributeAttackSnapshot(attackSnapshot.Weapon, attackSnapshot.Attribute, attackSnapshot.AttributeCount, attackSnapshot.SynergyStage, attackSnapshot.Grade, attackSnapshot.Origin, attackSnapshot.DirectDamage * transferDamageMultiplier); // 발사 정보와 전달 피해 배율
                ApplyProjectileHit(additional, transfer, transfer.transform.position); // 추가 대상 속성 명중
                if (!isActiveAndEnabled || configurationVersion != hitVersion) // 추가 명중 중 수명 변경 확인
                {
                    return; // 이전 발사의 파괴 차단
                }
            }
            if (remainingHits <= 0) // 관통 횟수 소진 확인
            {
                Destroy(gameObject); // 투사체 제거
            }
        }

        private bool StopForGameState() // 정지와 종료 중 추가 명중 방지
        { // 함수 시작
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            if (game == null || game.IsPlaying) // 실제 전투 진행 여부
            { // 조건 시작
                return false; // 기존 명중 처리 계속
            } // 조건 종료
            if (game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated || remainingHits <= 0) // 종료 또는 이미 소진된 탄 확인
            { // 조건 시작
                gameObject.SetActive(false); // 지연 파괴 전 후속 접촉 차단
                Destroy(gameObject); // 종료 또는 소진된 탄 제거
            } // 조건 종료
            return true; // 정지 중 후속 명중 중단
        } // 함수 종료

        private void ApplyProjectileHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 position) // 투사체 속성 명중 적용
        {
            if (attributeEffects != null) // 속성 효과 연결 확인
            {
                attributeEffects.ApplyHit(attack, target, position); // 기존 통합 명중 적용
                return; // 기본 피해 중복 방지
            }
            target.TakeDamage(attack.DirectDamage); // 효과 관리자 없는 기본 피해
        }
    }
}
