using ProjectEpsilon.Combat; // 적 상태와 넉백
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 체력과 몸통
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class EarlyEnemyBehaviour : MonoBehaviour // 시험용 적 행동
    { // 클래스 시작
        private float shooterDistance; // 사수 유지 거리
        private float projectileSpeed; // 투사체 임시 속도
        private float projectileLifetime; // 투사체 임시 수명
        private float eliteWarningDuration; // 엘리트 예고 시간
        private float eliteChargeDuration; // 엘리트 돌진 시간
        private float eliteChargeMultiplier; // 엘리트 돌진 속도
        private EarlyEnemySettings settings; // 적 능력 설정
        private SnakeMovement player; // 플레이어 이동 참조
        private SnakeBodyManager body; // 플레이어 몸통 참조
        private SnakeHealth health; // 공용 체력 참조
        private Sprite visual; // 투사체 시각 자료
        private Rigidbody2D movementBody; // 적 물리체 참조
        private WeaponTargetStatusController statuses; // 적 상태 효과 참조
        private WeaponTargetKnockbackController knockback; // 적 넉백 참조
        private float nextAttack; // 다음 공격 시각
        private float chargePhaseEnd; // 돌진 단계 종료 시각
        private int chargePhase; // 대기 예고 돌진 단계
        private bool chargeCut; // 돌진 중 절단 여부
        private bool chargeCutPending; // 돌진 절단 결과 대기
        private bool reportedMissingResolver; // 충돌 처리기 누락 기록
        private Vector2 chargeDirection; // 예고된 돌진 방향
        private LineRenderer warning; // 돌진 예고선
        public void Configure(EarlyEnemySettings data, SnakeMovement target, SnakeBodyManager segments, SnakeHealth sharedHealth, Sprite sprite, float keepDistance, float shotSpeed, float shotLifetime, float warningDuration, float chargeDuration, float chargeMultiplier) // 행동 연결
        { // 메서드 시작
            settings = data; // 능력 설정 저장
            player = target; // 플레이어 저장
            body = segments; // 몸통 관리자 저장
            health = sharedHealth; // 공용 체력 저장
            visual = sprite; // 시각 자료 저장
            shooterDistance = keepDistance; // 사수 유지 거리 저장
            projectileSpeed = shotSpeed; // 투사체 속도 저장
            projectileLifetime = shotLifetime; // 투사체 수명 저장
            eliteWarningDuration = warningDuration; // 엘리트 예고 시간 저장
            eliteChargeDuration = chargeDuration; // 엘리트 돌진 시간 저장
            eliteChargeMultiplier = chargeMultiplier; // 엘리트 돌진 배율 저장
            movementBody = GetComponent<Rigidbody2D>(); // 물리체 조회
            statuses = GetComponent<WeaponTargetStatusController>(); // 상태 효과 조회
            knockback = GetComponent<WeaponTargetKnockbackController>(); // 넉백 조회
            nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 첫 공격 시각
        } // 메서드 끝
        private void FixedUpdate() // 적 이동 처리
        { // 메서드 시작
            if (settings == null || player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 진행 상태 확인
            { // 조건 시작
                return; // 이동 중단
            } // 조건 끝
            if (knockback != null && knockback.TryConsumeMovement(Time.fixedDeltaTime, out Vector2 displacement)) // 넉백 우선 확인
            { // 조건 시작
                movementBody.MovePosition(movementBody.position + displacement); // 넉백 이동
                return; // 일반 이동 중단
            } // 조건 끝
            Vector2 offset = TargetPosition() - movementBody.position; // 목표 방향 계산
            if (settings.kind == EarlyEnemyKind.Elite && TickCharge(offset)) // 엘리트 돌진 처리
            { // 조건 시작
                return; // 추적 이동 생략
            } // 조건 끝
            if (settings.kind == EarlyEnemyKind.Shooter) // 사수 동작 확인
            { // 조건 시작
                if (Time.time >= nextAttack) // 사격 주기 확인
                { // 조건 시작
                    FireProjectile(ShooterTargetPosition() - movementBody.position); // 몸통 방향 투사체 발사
                    nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 사격 시각
                } // 조건 끝
                if (offset.magnitude < shooterDistance * 0.8f) // 근접 거리 확인
                { // 조건 시작
                    offset = -offset; // 후퇴 방향 설정
                } // 조건 끝
                else if (offset.magnitude <= shooterDistance) // 유지 거리 확인
                { // 조건 시작
                    return; // 이동 중지
                } // 조건 끝
            } // 조건 끝
            Move(offset.normalized, 1f); // 추적 또는 후퇴 이동
        } // 메서드 끝
        private Vector2 TargetPosition() // 목표 위치 조회
        { // 메서드 시작
            if (settings.kind == EarlyEnemyKind.Predator && body != null && body.BodySegments.Count > 0) // 몸통 우선 추적 확인
            { // 조건 시작
                float best = float.MaxValue; // 최근접 거리 초기화
                Vector2 target = player.transform.position; // 기본 목표 설정
                foreach (SnakeSegment segment in body.BodySegments) // 몸통 순회
                { // 반복 시작
                    if (segment == null) // 소멸 몸통 확인
                    { // 조건 시작
                        continue; // 다음 몸통 이동
                    } // 조건 끝
                    float distance = ((Vector2)segment.transform.position - movementBody.position).sqrMagnitude; // 몸통 거리 계산
                    if (distance < best) // 최근접 몸통 확인
                    { // 조건 시작
                        best = distance; // 최단 거리 저장
                        target = segment.transform.position; // 목표 몸통 저장
                    } // 조건 끝
                } // 반복 끝
                return target; // 최근접 몸통 반환
            } // 조건 끝
            return player.transform.position; // 머리 위치 반환
        } // 메서드 끝
        private Vector2 ShooterTargetPosition() // 사수 발사 목표
        { // 메서드 시작
            Vector2 fallback = player.transform.position; // 몸통 부재 시 머리 목표
            if (body == null) // 몸통 관리자 확인
            { // 조건 시작
                return fallback; // 머리 목표 반환
            } // 조건 끝
            float best = float.MaxValue; // 최근접 거리 초기화
            Vector2 selected = fallback; // 기본 발사 목표
            foreach (SnakeSegment segment in body.BodySegments) // 몸통 목록 순회
            { // 반복 시작
                if (segment == null || !segment.gameObject.activeInHierarchy) // 유효 몸통 확인
                { // 조건 시작
                    continue; // 다음 몸통 확인
                } // 조건 끝
                Vector2 position = segment.transform.position; // 몸통 위치
                float distance = (position - movementBody.position).sqrMagnitude; // 몸통까지 거리 제곱
                if (distance < 0.001f || distance >= best) // 발사 방향과 최근접 여부 확인
                { // 조건 시작
                    continue; // 유효하지 않은 목표 제외
                } // 조건 끝
                best = distance; // 최근접 거리 갱신
                selected = position; // 발사 목표 갱신
            } // 반복 끝
            return selected; // 최종 발사 목표 반환
        } // 메서드 끝
        private void Move(Vector2 direction, float multiplier) // 물리 이동
        { // 메서드 시작
            float statusMultiplier = statuses == null ? 1f : statuses.CurrentMovementMultiplier; // 상태 이동 배율
            float speed = player.MoveSpeed * Mathf.Max(0f, settings.speedRatio) * statusMultiplier * multiplier; // 최종 이동 속도
            movementBody.MovePosition(movementBody.position + direction * speed * Time.fixedDeltaTime); // 적 위치 이동
        } // 메서드 끝
        private bool TickCharge(Vector2 offset) // 엘리트 예고와 돌진
        { // 메서드 시작
            if (chargePhase == 0 && Time.time >= nextAttack) // 돌진 준비 시각 확인
            { // 조건 시작
                chargeDirection = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.up; // 돌진 방향 고정
                chargePhase = 1; // 예고 단계 진입
                chargePhaseEnd = Time.time + Mathf.Max(0.1f, eliteWarningDuration); // 예고 종료 시각
                ShowWarning(); // 예고선 표시
            } // 조건 끝
            if (chargePhase == 1) // 예고 단계 확인
            { // 조건 시작
                if (Time.time >= chargePhaseEnd) // 예고 종료 확인
                { // 조건 시작
                    chargePhase = 2; // 돌진 단계 진입
                    chargeCut = false; // 돌진 절단 초기화
                    chargeCutPending = false; // 이전 절단 대기 초기화
                    chargePhaseEnd = Time.time + Mathf.Max(0.1f, eliteChargeDuration); // 돌진 종료 시각
                    if (warning != null) // 예고선 존재 확인
                    { // 조건 시작
                        warning.enabled = false; // 예고선 숨김
                    } // 조건 끝
                } // 조건 끝
                return true; // 일반 이동 중지
            } // 조건 끝
            if (chargePhase == 2) // 돌진 단계 확인
            { // 조건 시작
                if (Time.time >= chargePhaseEnd) // 돌진 종료 확인
                { // 조건 시작
                    chargePhase = 0; // 대기 단계 복귀
                    nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 돌진 시각
                    return false; // 일반 이동 허용
                } // 조건 끝
                Move(chargeDirection, Mathf.Max(1f, eliteChargeMultiplier)); // 고정 방향 돌진
                return true; // 일반 이동 중지
            } // 조건 끝
            return false; // 일반 추적 허용
        } // 메서드 끝
        private void ShowWarning() // 돌진 방향 예고
        { // 메서드 시작
            if (warning == null) // 예고선 부재 확인
            { // 조건 시작
                warning = gameObject.AddComponent<LineRenderer>(); // 예고선 생성
                warning.material = new Material(Shader.Find("Sprites/Default")); // 기본 스프라이트 재질
                warning.startColor = Color.red; // 시작 색상
                warning.endColor = Color.red; // 끝 색상
                warning.startWidth = 0.12f; // 시작 두께
                warning.endWidth = 0.12f; // 끝 두께
                warning.sortingOrder = 30; // 예고 표시 순서
                warning.positionCount = 2; // 예고선 점 수
            } // 조건 끝
            warning.enabled = true; // 예고선 표시
            warning.SetPosition(0, transform.position); // 예고선 시작점
            warning.SetPosition(1, transform.position + (Vector3)(chargeDirection * 6f)); // 예고선 끝점
        } // 메서드 끝
        private void OnTriggerStay2D(Collider2D other) // 적 접촉 지속 처리
        { // 메서드 시작
            if (!isActiveAndEnabled || settings == null || player == null || health == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || !IsPlayerPart(other)) // 활성 상태와 유효 접촉 확인
            { // 조건 시작
                return; // 접촉 처리 생략
            } // 조건 끝
            if (chargePhase == 2 && !chargeCut && !chargeCutPending) // 예고 돌진 절단 확인
            { // 조건 시작
                SnakeExternalCollision collision = player.GetComponent<SnakeExternalCollision>(); // 직접 충돌 판정 조회
                if (collision != null && collision.isActiveAndEnabled) // 물리 스텝 처리기 확인
                { // 조건 시작
                    chargeCutPending = true; // 중복 절단 예약 방지
                    collision.QueueEliteCut(HandleEliteCutResolved); // 절단과 결과 예약
                } // 조건 끝
                else // 처리기 부재 확인
                { // 대안 시작
                    ReportMissingResolver(); // 구성 누락 알림
                } // 대안 끝
            } // 조건 끝
            if (settings.kind == EarlyEnemyKind.Shooter || Time.time < nextAttack) // 근접 공격 가능 여부
            { // 조건 시작
                return; // 근접 피해 생략
            } // 조건 끝
            WeaponTarget target = GetComponent<WeaponTarget>(); // 적 전투 대상 조회
            float damageMultiplier = target == null ? 1f : target.OutgoingDamageMultiplier; // 적 피해 상태 배율
            int damage = Mathf.CeilToInt(settings.damage * damageMultiplier); // 현재 적 피해량 확정
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 물리 스텝 처리기 조회
            if (resolver != null && resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            { // 조건 시작
                resolver.QueueHealthDamage(health, damage); // 피해 대상과 확정 피해량 예약
            } // 조건 끝
            else // 충돌 처리기 부재
            { // 대안 시작
                ReportMissingResolver(); // 구성 누락 알림
                health.TakeDamage(damage); // 기존 즉시 피해 대체
            } // 대안 끝
            nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 근접 공격 시각
        } // 메서드 끝
        private void HandleEliteCutResolved(bool applied) // 엘리트 절단 결과 처리
        { // 메서드 시작
            chargeCutPending = false; // 절단 결과 대기 해제
            chargeCut |= applied; // 성공한 절단 기록
        } // 메서드 끝
        private void ReportMissingResolver() // 물리 충돌 처리기 누락 알림
        { // 메서드 시작
            if (reportedMissingResolver) // 오류 중복 기록 확인
            { // 조건 시작
                return; // 중복 기록 방지
            } // 조건 끝
            reportedMissingResolver = true; // 오류 기록 상태 저장
            Debug.LogError("[Project Epsilon] 적 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
        } // 메서드 끝
        private bool IsPlayerPart(Collider2D other) // 플레이어 접촉 확인
        { // 메서드 시작
            if (other.transform == player.transform) // 머리 접촉 확인
            { // 조건 시작
                return true; // 머리 접촉 반환
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 몸통 구성 조회
            return segment != null && body != null && segment.transform.IsChildOf(body.transform); // 플레이어 몸통 여부 반환
        } // 메서드 끝
        private void FireProjectile(Vector2 offset) // 사수 투사체 생성
        { // 메서드 시작
            if (offset.sqrMagnitude < 0.001f) // 방향 부재 확인
            { // 조건 시작
                return; // 발사 생략
            } // 조건 끝
            GameObject shot = new GameObject("Enemy_Projectile"); // 투사체 오브젝트 생성
            shot.transform.position = transform.position; // 발사 위치 설정
            EnemyTestProjectile projectile = shot.AddComponent<EnemyTestProjectile>(); // 투사체 행동 추가
            projectile.Configure(offset.normalized, projectileSpeed, projectileLifetime, settings.damage, health, body, player, visual); // 투사체 설정
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
