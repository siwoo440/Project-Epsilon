using ProjectEpsilon.Combat; // 적 상태와 넉백
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 체력과 몸통
using UnityEngine; // Unity 기능

using ProjectEpsilon.Recording; // 실제 피해 원인 기록

namespace ProjectEpsilon.Enemies // 적 영역
{
    public sealed class EarlyEnemyBehaviour : MonoBehaviour // 시험용 적 행동
    {
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
        private Material warningMaterial; // 예고선 전용 재질
        public void Configure(EarlyEnemySettings data, SnakeMovement target, SnakeBodyManager segments, SnakeHealth sharedHealth, Sprite sprite, float keepDistance, float shotSpeed, float shotLifetime, float warningDuration, float chargeDuration, float chargeMultiplier) // 행동 연결
        {
            settings = data; // 능력 설정 저장
            player = target; // 플레이어 저장
            body = segments; // 몸통 관리자 저장
            health = sharedHealth; // 공용 체력 저장
            visual = sprite; // 시각 자료 저장
            shooterDistance = keepDistance; // 사수 유지 거리 저장
            projectileSpeed = shotSpeed; // 투사체 속도 저장
            projectileLifetime = shotLifetime; // 투사체 수명 저장
            eliteWarningDuration = data.kind == EarlyEnemyKind.Charger ? data.chargeWarningDuration : warningDuration; // 일반 적 또는 엘리트 예고 시간
            eliteChargeDuration = data.kind == EarlyEnemyKind.Charger ? data.chargeDuration : chargeDuration; // 일반 적 또는 엘리트 돌진 지속 시간
            eliteChargeMultiplier = data.kind == EarlyEnemyKind.Charger ? data.chargeSpeedMultiplier : chargeMultiplier; // 일반 적 또는 엘리트 돌진 배율
            movementBody = GetComponent<Rigidbody2D>(); // 물리체 조회
            statuses = GetComponent<WeaponTargetStatusController>(); // 상태 효과 조회
            knockback = GetComponent<WeaponTargetKnockbackController>(); // 넉백 조회
            nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 첫 공격 시각
        }
        private void FixedUpdate() // 적 이동 처리
        {
            if (settings == null || player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 진행 상태 확인
            {
                return; // 이동 중단
            }
            if (knockback != null && knockback.TryConsumeMovement(Time.fixedDeltaTime, out Vector2 displacement)) // 넉백 우선 확인
            {
                if (settings.kind == EarlyEnemyKind.Shooter) // 사격 행동 확인
                { // 조건 시작
                    HideWarning(); // 넉백 중 이전 사격 예고 제거
                } // 조건 종료
                movementBody.MovePosition(movementBody.position + displacement); // 넉백 이동
                return; // 일반 이동 중단
            }
            Vector2 offset = TargetPosition() - movementBody.position; // 목표 방향 계산
            if (settings.enemySprite != null && (statuses == null || statuses.CurrentMovementMultiplier > 0f)) // 전용 이미지와 움직임 가능 상태 확인
            {
                Vector2 facing = (settings.kind == EarlyEnemyKind.Elite || settings.kind == EarlyEnemyKind.Charger) && chargePhase != 0 ? chargeDirection : settings.kind == EarlyEnemyKind.Shooter ? ShooterTargetPosition() - movementBody.position : offset; // 고정 돌진 또는 실제 추적·발사 목표 방향
                if (facing.sqrMagnitude > 0.0001f) // 유효한 목표 방향 확인
                {
                    movementBody.MoveRotation(Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f); // 위쪽 기준 이미지의 실제 목표 회전
                }
            }
            if ((settings.kind == EarlyEnemyKind.Elite || settings.kind == EarlyEnemyKind.Charger) && TickCharge(offset)) // 예고 돌진 처리
            {
                return; // 추적 이동 생략
            }
            if (settings.kind == EarlyEnemyKind.Shooter) // 사수 동작 확인
            {
                if (Time.time >= nextAttack) // 사격 주기 확인
                {
                    HideWarning(); // 발사 직전 예고 제거
                    FireProjectile(ShooterTargetPosition() - movementBody.position); // 몸통 방향 투사체 발사
                    nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 사격 시각
                }
                else if (settings.projectileWarningDuration > 0f && Time.time >= nextAttack - settings.projectileWarningDuration) // 발사 전 예고 구간 확인
                { // 조건 시작
                    ShowShooterWarning(ShooterTargetPosition() - movementBody.position); // 현재 발사 목표 예고
                } // 조건 종료
                else // 발사 예고 구간 밖
                { // 조건 시작
                    HideWarning(); // 이전 사격 예고 제거
                } // 조건 종료
                if (offset.magnitude < shooterDistance * 0.8f) // 근접 거리 확인
                {
                    offset = -offset; // 후퇴 방향 설정
                }
                else if (offset.magnitude <= shooterDistance) // 유지 거리 확인
                {
                    return; // 이동 중지
                }
            }
            Move(offset.normalized, 1f); // 추적 또는 후퇴 이동
        }
        private Vector2 TargetPosition() // 목표 위치 조회
        {
            if (settings.kind == EarlyEnemyKind.Predator && body != null && body.BodySegments.Count > 0) // 몸통 우선 추적 확인
            {
                float best = float.MaxValue; // 최근접 거리 초기화
                Vector2 target = player.transform.position; // 기본 목표 설정
                foreach (SnakeSegment segment in body.BodySegments) // 몸통 순회
                {
                    if (segment == null) // 소멸 몸통 확인
                    {
                        continue; // 다음 몸통 이동
                    }
                    float distance = ((Vector2)segment.transform.position - movementBody.position).sqrMagnitude; // 몸통 거리 계산
                    if (distance < best) // 최근접 몸통 확인
                    {
                        best = distance; // 최단 거리 저장
                        target = segment.transform.position; // 목표 몸통 저장
                    }
                }
                return target; // 최근접 몸통 반환
            }
            return player.transform.position; // 머리 위치 반환
        }
        private Vector2 ShooterTargetPosition() // 사수 발사 목표
        {
            Vector2 fallback = player.transform.position; // 몸통 부재 시 머리 목표
            if (body == null) // 몸통 관리자 확인
            {
                return fallback; // 머리 목표 반환
            }
            float best = float.MaxValue; // 최근접 거리 초기화
            Vector2 selected = fallback; // 기본 발사 목표
            foreach (SnakeSegment segment in body.BodySegments) // 몸통 목록 순회
            {
                if (segment == null || !segment.gameObject.activeInHierarchy) // 유효 몸통 확인
                {
                    continue; // 다음 몸통 확인
                }
                Vector2 position = segment.transform.position; // 몸통 위치
                float distance = (position - movementBody.position).sqrMagnitude; // 몸통까지 거리 제곱
                if (distance < 0.001f || distance >= best) // 발사 방향과 최근접 여부 확인
                {
                    continue; // 유효하지 않은 목표 제외
                }
                best = distance; // 최근접 거리 갱신
                selected = position; // 발사 목표 갱신
            }
            return selected; // 최종 발사 목표 반환
        }
        private void Move(Vector2 direction, float multiplier) // 물리 이동
        {
            float statusMultiplier = statuses == null ? 1f : statuses.CurrentMovementMultiplier; // 상태 이동 배율
            float speed = player.BaseMoveSpeed * Mathf.Max(0f, settings.speedRatio) * statusMultiplier * multiplier * EnemyMovementRules.SpeedMultiplier; // 캐릭터 보정과 독립된 적 이동 속도
            movementBody.MovePosition(movementBody.position + direction * speed * Time.fixedDeltaTime); // 적 위치 이동
        }
        private bool TickCharge(Vector2 offset) // 엘리트 예고와 돌진
        {
            if (chargePhase == 0 && Time.time >= nextAttack) // 돌진 준비 시각 확인
            {
                chargeDirection = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.up; // 돌진 방향 고정
                chargePhase = 1; // 예고 단계 진입
                chargePhaseEnd = Time.time + Mathf.Max(0.1f, eliteWarningDuration); // 예고 종료 시각
                ShowWarning(); // 예고선 표시
            }
            if (chargePhase == 1) // 예고 단계 확인
            {
                if (Time.time >= chargePhaseEnd) // 예고 종료 확인
                {
                    chargePhase = 2; // 돌진 단계 진입
                    chargeCut = false; // 돌진 절단 초기화
                    chargeCutPending = false; // 이전 절단 대기 초기화
                    chargePhaseEnd = Time.time + Mathf.Max(0.1f, eliteChargeDuration); // 돌진 종료 시각
                    if (warning != null) // 예고선 존재 확인
                    {
                        warning.enabled = false; // 예고선 숨김
                    }
                }
                return true; // 일반 이동 중지
            }
            if (chargePhase == 2) // 돌진 단계 확인
            {
                if (Time.time >= chargePhaseEnd) // 돌진 종료 확인
                {
                    chargePhase = 0; // 대기 단계 복귀
                    nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 돌진 시각
                    return false; // 일반 이동 허용
                }
                Move(chargeDirection, Mathf.Max(1f, eliteChargeMultiplier)); // 고정 방향 돌진
                return true; // 일반 이동 중지
            }
            return false; // 일반 추적 허용
        }
        private void ShowWarning() // 돌진 방향 예고
        {
            EnsureWarning(); // 공용 예고선 준비
            warning.startColor = Color.red; // 돌진 시작 색상
            warning.endColor = Color.red; // 돌진 끝 색상
            warning.startWidth = 0.12f; // 돌진 시작 두께
            warning.endWidth = 0.12f; // 돌진 끝 두께
            warning.enabled = true; // 예고선 표시
            warning.SetPosition(0, transform.position); // 예고선 시작점
            float warningDistance = settings.kind == EarlyEnemyKind.Charger ? player.BaseMoveSpeed * Mathf.Max(0f, settings.speedRatio) * Mathf.Max(1f, eliteChargeMultiplier) * Mathf.Max(0.1f, eliteChargeDuration) * EnemyMovementRules.SpeedMultiplier : 6f * EnemyMovementRules.SpeedMultiplier; // 일반 돌진 실제 거리 또는 기존 엘리트 거리
            warning.SetPosition(1, transform.position + (Vector3)(chargeDirection * warningDistance)); // 맵에 표시할 예고선 끝점
        }
        private void EnsureWarning() // 공용 예고선 준비
        { // 함수 시작
            if (warning == null) // 예고선 부재 확인
            { // 조건 시작
                warning = gameObject.AddComponent<LineRenderer>(); // 예고선 생성
                warningMaterial = new Material(Shader.Find("Sprites/Default")); // 예고선 전용 재질 생성
                warning.material = warningMaterial; // 생성한 재질 연결
                warning.sortingOrder = 40; // 포탑과 적 위 예고 순서
                warning.positionCount = 2; // 예고선 점 수
            } // 조건 종료
        } // 함수 종료
        private void ShowShooterWarning(Vector2 offset) // 사격 방향 예고
        { // 함수 시작
            if (offset.sqrMagnitude < 0.001f) // 발사 방향 부재 확인
            { // 조건 시작
                HideWarning(); // 유효하지 않은 예고 제거
                return; // 표시 중단
            } // 조건 종료
            EnsureWarning(); // 공용 예고선 준비
            Color color = new Color(1f, 0.65f, 0.1f, 0.9f); // 사격 예고 색상
            warning.startColor = color; // 시작 예고 색상
            warning.endColor = color; // 끝 예고 색상
            warning.startWidth = 0.08f; // 시작 예고 두께
            warning.endWidth = 0.08f; // 끝 예고 두께
            float distance = Mathf.Min(offset.magnitude, Mathf.Max(0f, projectileSpeed) * Mathf.Max(0f, projectileLifetime)); // 목표와 투사체 최대 거리
            warning.SetPosition(0, transform.position); // 현재 적 위치
            warning.SetPosition(1, transform.position + (Vector3)(offset.normalized * distance)); // 현재 목표 방향
            warning.enabled = true; // 사격 예고 표시
        } // 함수 종료
        private void HideWarning() // 예고선 숨김
        { // 함수 시작
            if (warning != null) // 생성된 예고선 확인
            { // 조건 시작
                warning.enabled = false; // 이전 예고 제거
            } // 조건 종료
        } // 함수 종료
        private void OnDisable() // 적 행동 중지 처리
        { // 함수 시작
            HideWarning(); // 중지된 행동의 예고 제거
        } // 함수 종료
        private void OnDestroy() // 적 제거 시 예고 자원 정리
        { // 함수 시작
            if (warningMaterial != null) // 생성한 재질 확인
            { // 조건 시작
                Destroy(warningMaterial); // 예고 재질 제거
            } // 조건 종료
        } // 함수 종료
        private void OnTriggerStay2D(Collider2D other) // 적 접촉 지속 처리
        {
            if (!isActiveAndEnabled || settings == null || player == null || health == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || !IsPlayerPart(other)) // 활성 상태와 유효 접촉 확인
            {
                return; // 접촉 처리 생략
            }
            if (settings.kind == EarlyEnemyKind.Elite && chargePhase == 2 && !chargeCut && !chargeCutPending) // 엘리트 전용 돌진 절단 확인
            {
                SnakeExternalCollision collision = player.GetComponent<SnakeExternalCollision>(); // 직접 충돌 판정 조회
                if (collision != null && collision.isActiveAndEnabled) // 물리 스텝 처리기 확인
                {
                    chargeCutPending = true; // 중복 절단 예약 방지
                    collision.QueueEliteCut(HandleEliteCutResolved); // 절단과 결과 예약
                }
                else // 처리기 부재 확인
                {
                    ReportMissingResolver(); // 구성 누락 알림
                }
            }
            if (settings.kind == EarlyEnemyKind.Shooter || Time.time < nextAttack) // 근접 공격 가능 여부
            {
                return; // 근접 피해 생략
            }
            WeaponTarget target = GetComponent<WeaponTarget>(); // 적 전투 대상 조회
            float damageMultiplier = target == null ? 1f : target.OutgoingDamageMultiplier; // 적 피해 상태 배율
            int damage = Mathf.CeilToInt(settings.damage * damageMultiplier); // 현재 적 피해량 확정
            SnakeExternalCollision resolver = player.GetComponent<SnakeExternalCollision>(); // 물리 스텝 처리기 조회
            if (resolver != null && resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            {
                resolver.QueueHealthDamage(health, damage, DamageOrigin.EnemyMelee); // 피해 대상과 확정 피해량 예약
            }
            else // 충돌 처리기 부재
            {
                ReportMissingResolver(); // 구성 누락 알림
                health.TakeDamage(damage, DamageOrigin.EnemyMelee); // 기존 즉시 피해 대체
            }
            nextAttack = Time.time + Mathf.Max(0.1f, settings.attackInterval); // 다음 근접 공격 시각
        }
        private void HandleEliteCutResolved(bool applied) // 엘리트 절단 결과 처리
        {
            chargeCutPending = false; // 절단 결과 대기 해제
            chargeCut |= applied; // 성공한 절단 기록
        }
        private void ReportMissingResolver() // 물리 충돌 처리기 누락 알림
        {
            if (reportedMissingResolver) // 오류 중복 기록 확인
            {
                return; // 중복 기록 방지
            }
            reportedMissingResolver = true; // 오류 기록 상태 저장
            Debug.LogError("[Project Epsilon] 적 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
        }
        private bool IsPlayerPart(Collider2D other) // 플레이어 접촉 확인
        {
            if (other.transform == player.transform) // 머리 접촉 확인
            {
                return true; // 머리 접촉 반환
            }
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 몸통 구성 조회
            return segment != null && body != null && segment.transform.IsChildOf(body.transform); // 플레이어 몸통 여부 반환
        }
        private void FireProjectile(Vector2 offset) // 사수 투사체 생성
        {
            if (offset.sqrMagnitude < 0.001f) // 방향 부재 확인
            {
                return; // 발사 생략
            }
            Vector2 aim = offset.normalized; // 실제 목표 방향 정규화
            int count = Mathf.Clamp(settings.projectileCount, 1, 9); // 안전한 투사체 수
            float spread = Mathf.Clamp(settings.projectileSpreadDegrees, 0f, 180f); // 안전한 전체 부채각
            WeaponTarget target = GetComponent<WeaponTarget>(); // 실제 사수 전투 상태
            float damageMultiplier = target == null ? 1f : target.OutgoingDamageMultiplier; // 발사 시점 독 약화 배율
            int shotDamage = Mathf.CeilToInt(settings.damage * damageMultiplier); // 기존 올림 규칙의 발사 피해
            int fanCount = settings.opposedProjectileFan ? 2 : 1; // 맵별 부채꼴 방향 수
            for (int fan = 0; fan < fanCount; fan++) // 실제 부채꼴 방향 순회
            { // 영역 시작
                Vector2 fanAim = fan == 0 ? aim : -aim; // 전방 또는 반대 방향
                for (int index = 0; index < count; index++) // 부채꼴 사격 순회
                { // 영역 시작
                    float angle = count == 1 ? 0f : -spread * 0.5f + spread * index / (count - 1); // 목표 중심의 대칭 각도
                    float radians = angle * Mathf.Deg2Rad; // 회전 각도의 라디안 변환
                    float cosine = Mathf.Cos(radians); // 수평 회전 성분
                    float sine = Mathf.Sin(radians); // 수직 회전 성분
                    Vector2 direction = new Vector2(fanAim.x * cosine - fanAim.y * sine, fanAim.x * sine + fanAim.y * cosine); // 실제 투사체 발사 방향
                    GameObject shot = new GameObject("Enemy_Projectile"); // 투사체 오브젝트 생성
                    shot.transform.position = transform.position; // 발사 위치 설정
                    EnemyTestProjectile projectile = shot.AddComponent<EnemyTestProjectile>(); // 투사체 행동 추가
                    projectile.Configure(direction, projectileSpeed, projectileLifetime, shotDamage, health, body, player, visual, GetComponent<PrototypeBossChargeAttack>() == null ? DamageOrigin.EnemyProjectile : DamageOrigin.BossProjectile); // 발사 시점 피해와 투사체 설정
                } // 영역 종료
            } // 영역 종료
        }
    }
}
