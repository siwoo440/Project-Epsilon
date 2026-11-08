using System; // 절단 결과 알림
using System.Collections.Generic; // 예약 피해 목록
using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeExternalCollision : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeInvulnerability invulnerability;

        [SerializeField] private int enemyBodyLoss = 1;
        [SerializeField] private float enemyContactProtection = 1f;
        [SerializeField] private int obstacleDamage = 25;
        [SerializeField] private float obstacleInvulnerability = 1.5f;

        private float nextEnemyContactAllowedTime;
        private bool stepRegistered; // 물리 스텝 등록 상태
        private int registeredGeneration; // 마지막 물리 등록 세대
        private bool reportedFallback; // 즉시 처리 오류 기록 상태
        private bool pendingEnemyContact; // 예약된 적 직접 충돌
        private bool pendingObstacleContact; // 예약된 장애물 충돌
        private SnakeSelfCollision pendingSelfCollision; // 예약된 자기 충돌
        private bool pendingUnconditionalSelfCollision; // 몸통 정보 없는 기존 자기 충돌
        private readonly List<PendingSelfSegment> pendingSelfSegments = new List<PendingSelfSegment>(); // 같은 스텝 자기 충돌 몸통
        private readonly List<Action<bool>> pendingEliteCuts = new List<Action<bool>>(); // 예약된 엘리트 절단
        private readonly List<PendingHealthDamage> pendingHealthDamage = new List<PendingHealthDamage>(); // 예약된 공용 체력 피해

        private readonly struct PendingSelfSegment // 자기 충돌 몸통과 재확인 여부
        { // 구조체 시작
            public readonly SnakeSegment Segment; // 실제 자기 충돌 몸통
            public readonly bool RequireStillTouching; // 물리 후 겹침 재확인 여부
            public PendingSelfSegment(SnakeSegment segment, bool requireStillTouching) // 실제 몸통 예약 생성
            { // 생성자 시작
                Segment = segment; // 실제 몸통 저장
                RequireStillTouching = requireStillTouching; // 물리 후 재확인 설정
            } // 생성자 끝
        } // 구조체 끝

        private readonly struct PendingHealthDamage // 스텝 시작 시 피해 정보
        { // 구조체 시작
            public readonly SnakeHealth Target; // 피해 대상
            public readonly int Amount; // 확정된 피해량

            public PendingHealthDamage(SnakeHealth target, int amount) // 피해 정보 생성
            { // 생성자 시작
                Target = target; // 대상 참조 보관
                Amount = amount; // 피해량 값 보관
            } // 생성자 끝
        } // 구조체 끝

        public bool EnemyContactProtected =>
            Time.time < nextEnemyContactAllowedTime;

        public float EnemyContactProtectionRemaining =>
            Mathf.Max(0f, nextEnemyContactAllowedTime - Time.time);

        private void OnEnable() // 물리 스텝 처리 연결
        { // 메서드 시작
            reportedFallback = false; // 오류 기록 상태 초기화
            if (Application.isPlaying) // 플레이 상태 확인
            { // 조건 시작
                RegisterPhysicsStep(); // 물리 직후 처리 등록
            } // 조건 끝
        } // 메서드 끝

        private void FixedUpdate() // 물리 등록 상태 확인
        { // 메서드 시작
            if (Application.isPlaying && (!stepRegistered || registeredGeneration != SnakePhysicsStepDispatcher.Generation)) // 플레이 시작과 등록 세대 확인
            { // 조건 시작
                RegisterPhysicsStep(); // 누락된 물리 등록 복구
            } // 조건 끝
        } // 메서드 끝

        private void RegisterPhysicsStep() // 물리 스텝 등록
        { // 메서드 시작
            stepRegistered = SnakePhysicsStepDispatcher.Register(ResolvePendingContacts); // 물리 직후 처리 등록
            registeredGeneration = SnakePhysicsStepDispatcher.Generation; // 현재 등록 세대 저장
        } // 메서드 끝

        private void OnDisable() // 물리 스텝 처리 해제
        { // 메서드 시작
            SnakePhysicsStepDispatcher.Unregister(ResolvePendingContacts); // 물리 완료 구독 해제
            stepRegistered = false; // 등록 상태 해제
            CancelPendingContacts(); // 예약 취소와 절단 결과 반환
        } // 메서드 끝

        public void Bind(
            SnakeBodyManager manager,
            SnakeHealth snakeHealth,
            SnakeInvulnerability invulnerabilityController,
            int directEnemyBodyLoss,
            float directEnemyProtection,
            int obstacleSharedDamage,
            float obstacleFullInvulnerability
        )
        {
            bodyManager = manager;
            health = snakeHealth;
            invulnerability = invulnerabilityController;

            enemyBodyLoss = Mathf.Max(1, directEnemyBodyLoss);
            enemyContactProtection = Mathf.Max(0f, directEnemyProtection);
            obstacleDamage = Mathf.Max(0, obstacleSharedDamage);
            obstacleInvulnerability = Mathf.Max(
                0f,
                obstacleFullInvulnerability
            );

            nextEnemyContactAllowedTime = 0f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled) // 충돌 처리기 활성 확인
            { // 조건 시작
                return; // 비활성 충돌 무시
            } // 조건 끝

            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>();

            if (hazard == null)
            {
                return;
            }

            switch (hazard.HazardType)
            {
                case SnakeContactHazardType.EnemyDirect:
                    QueueEnemyContact(); // 직접 충돌 예약
                    break;

                case SnakeContactHazardType.Obstacle:
                    QueueObstacleContact(); // 장애물 충돌 예약
                    break;
            }
        }

        public void QueueSelfCollision(SnakeSelfCollision source) // 자기 충돌 예약
        { // 메서드 시작
            QueueSelfCollision(source, null); // 기존 몸통 정보 없는 예약 유지
        } // 메서드 끝

        public void QueueSelfCollision(SnakeSelfCollision source, SnakeSegment segment) // 몸통별 자기 충돌 예약
        { // 메서드 시작
            QueueSelfCollision(source, segment, false); // 실제 접촉 콜백 예약
        } // 메서드 끝

        public void QueueDeferredSelfCollision(SnakeSelfCollision source, SnakeSegment segment) // 유예 종료 겹침 예약
        { // 메서드 시작
            QueueSelfCollision(source, segment, true); // 물리 후 실제 겹침 재확인 예약
        } // 메서드 끝

        private void QueueSelfCollision(SnakeSelfCollision source, SnakeSegment segment, bool requireStillTouching) // 실제 자기 충돌 예약 공통 처리
        { // 메서드 시작
            if (source == null) // 충돌 대상 확인
            { // 조건 시작
                return; // 잘못된 예약 무시
            } // 조건 끝
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                if (segment != null && segment.IsBossPushGraceActive) // 해당 몸통의 보스 유예 확인
                { // 조건 시작
                    source.DeferBossPushContact(segment); // 지속 겹침의 나중 재판정
                    return; // 현재 자기 충돌 생략
                } // 조건 끝
                if (requireStillTouching && !source.IsStillTouching(segment)) // 유예 접촉의 현재 겹침 확인
                { // 조건 시작
                    return; // 이미 분리된 몸통 무시
                } // 조건 끝
                source.TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingSelfCollision = source; // 자기 충돌 수집
            if (segment == null) // 몸통 정보 없는 기존 예약 확인
            { // 조건 시작
                pendingUnconditionalSelfCollision = true; // 기존 자기 충돌 보존
            } // 조건 끝
            else // 실제 몸통을 포함한 예약 확인
            { // 대안 시작
                pendingSelfSegments.Add(new PendingSelfSegment(segment, requireStillTouching)); // 해당 몸통과 재확인 여부 저장
            } // 대안 끝
        } // 메서드 끝

        public void QueueEliteCut(Action<bool> completed) // 엘리트 절단 예약
        { // 메서드 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                completed?.Invoke(TryApplyEliteCut()); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingEliteCuts.Add(completed); // 절단 결과 수신자 보관
        } // 메서드 끝

        public void QueueHealthDamage(SnakeHealth target, int damage) // 적의 공용 체력 피해 예약
        { // 메서드 시작
            if (target == null || damage <= 0) // 유효 대상과 피해 확인
            { // 조건 시작
                return; // 무효 피해 제외
            } // 조건 끝
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                target.TakeDamage(damage); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingHealthDamage.Add(new PendingHealthDamage(target, damage)); // 대상과 피해량 값 보관
        } // 메서드 끝

        private void QueueEnemyContact() // 적 직접 충돌 예약
        { // 메서드 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                TryApplyEnemyDirectContact(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingEnemyContact = true; // 직접 충돌 수집
        } // 메서드 끝

        private void QueueObstacleContact() // 장애물 충돌 예약
        { // 메서드 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                TryApplyObstacleContact(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingObstacleContact = true; // 장애물 충돌 수집
        } // 메서드 끝

        private void ResolvePendingContacts() // 같은 물리 스텝 충돌 우선 처리
        { // 메서드 시작
            if (pendingSelfCollision == null && !pendingEnemyContact && !pendingObstacleContact && pendingEliteCuts.Count == 0 && pendingHealthDamage.Count == 0) // 빈 물리 스텝 확인
            { // 조건 시작
                return; // 불필요한 목록 복사 방지
            } // 조건 끝
            SnakeSelfCollision self = pendingSelfCollision; // 자기 충돌 보관
            bool unconditionalSelf = pendingUnconditionalSelfCollision; // 기존 자기 충돌 보관
            PendingSelfSegment[] selfSegments = pendingSelfSegments.Count == 0 ? Array.Empty<PendingSelfSegment>() : pendingSelfSegments.ToArray(); // 예약된 몸통 목록 보관
            bool enemy = pendingEnemyContact; // 적 직접 충돌 보관
            bool obstacle = pendingObstacleContact; // 장애물 충돌 보관
            Action<bool>[] cuts = pendingEliteCuts.Count == 0 ? Array.Empty<Action<bool>>() : pendingEliteCuts.ToArray(); // 절단 요청 복사
            PendingHealthDamage[] damages = pendingHealthDamage.Count == 0 ? Array.Empty<PendingHealthDamage>() : pendingHealthDamage.ToArray(); // 피해 정보 복사
            ClearPendingContacts(); // 다음 물리 스텝 준비
            if (self != null && self.isActiveAndEnabled) // 활성 자기 충돌 확인
            { // 조건 시작
                bool applySelf = unconditionalSelf; // 기존 무조건 자기 충돌 여부
                foreach (PendingSelfSegment candidate in selfSegments) // 예약된 실제 몸통 순회
                { // 반복 시작
                    SnakeSegment segment = candidate.Segment; // 현재 실제 몸통 조회
                    if (segment == null) // 파괴된 몸통 확인
                    { // 조건 시작
                        continue; // 무효 접촉 제외
                    } // 조건 끝
                    if (candidate.RequireStillTouching && !self.IsStillTouching(segment)) // 물리 스텝 이후 실제 겹침 확인
                    { // 조건 시작
                        continue; // 같은 스텝에 분리된 몸통 제외
                    } // 조건 끝
                    if (segment.IsBossPushGraceActive) // 같은 스텝 보스 밀림 유예 확인
                    { // 조건 시작
                        self.DeferBossPushContact(segment); // 유예 종료 후 지속 겹침 재판정
                    } // 조건 끝
                    else // 보호받지 않는 실제 몸통 확인
                    { // 대안 시작
                        applySelf = true; // 기존 자기 충돌 적용
                    } // 대안 끝
                } // 반복 끝
                if (applySelf) // 적용 가능한 자기 충돌 확인
                { // 조건 시작
                    self.TryApplySelfCollisionPenalty(); // 자기 몸통 손실과 무적
                } // 조건 끝
            } // 조건 끝
            if (enemy && !IsUnavailable()) // 적 충돌과 활성·생존 상태 확인
            { // 조건 시작
                TryApplyEnemyDirectContact(); // 적 직접 몸통 손실
            } // 조건 끝
            foreach (Action<bool> completed in cuts) // 엘리트 절단 요청 순회
            { // 반복 시작
                bool applied = !IsUnavailable() && TryApplyEliteCut(); // 활성 상태와 절단 확인
                completed?.Invoke(applied); // 개별 절단 결과 전달
            } // 반복 끝
            if (obstacle && !IsUnavailable()) // 장애물 충돌과 활성·생존 확인
            { // 조건 시작
                TryApplyObstacleContact(); // 장애물 피해와 새 무적
            } // 조건 끝
            foreach (PendingHealthDamage damage in damages) // 예약된 적 피해 순회
            { // 반복 시작
                if (IsUnavailable()) // 비활성 또는 패배 확인
                { // 조건 시작
                    break; // 남은 피해 중단
                } // 조건 끝
                if (damage.Target != null) // 피해 대상 생존 확인
                { // 조건 시작
                    damage.Target.TakeDamage(damage.Amount); // 보호막과 무적을 통한 피해
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private bool IsBodyDepleted() // 남은 몸통 확인
        { // 메서드 시작
            return bodyManager != null && bodyManager.CurrentBodyCount <= 0; // 몸통 소진 여부
        } // 메서드 끝

        private bool IsUnavailable() // 충돌 처리 대상 상태 확인
        { // 메서드 시작
            return this == null || !isActiveAndEnabled || IsBodyDepleted(); // 파괴·비활성 또는 몸통 소진 여부
        } // 메서드 끝

        private bool CanQueueContacts() // 현재 물리 예약 가능 여부
        { // 메서드 시작
            return stepRegistered && SnakePhysicsStepDispatcher.CanQueue; // 등록과 자동 물리 방식 확인
        } // 메서드 끝

        private void CancelPendingContacts() // 비활성화 시 요청 취소
        { // 메서드 시작
            Action<bool>[] cuts = pendingEliteCuts.ToArray(); // 미처리 절단 결과 보관
            ClearPendingContacts(); // 수집한 충돌 제거
            foreach (Action<bool> completed in cuts) // 절단 요청 순회
            { // 반복 시작
                completed?.Invoke(false); // 절단 취소 결과 전달
            } // 반복 끝
        } // 메서드 끝

        private void ClearPendingContacts() // 수집한 충돌 초기화
        { // 메서드 시작
            pendingSelfCollision = null; // 자기 충돌 초기화
            pendingUnconditionalSelfCollision = false; // 기존 자기 충돌 초기화
            pendingSelfSegments.Clear(); // 몸통별 자기 충돌 초기화
            pendingEnemyContact = false; // 적 직접 충돌 초기화
            pendingObstacleContact = false; // 장애물 충돌 초기화
            pendingEliteCuts.Clear(); // 엘리트 절단 초기화
            pendingHealthDamage.Clear(); // 공용 체력 피해 초기화
        } // 메서드 끝

        private void ReportImmediateFallback() // 물리 스텝 처리 누락 알림
        { // 메서드 시작
            if (reportedFallback) // 중복 오류 확인
            { // 조건 시작
                return; // 중복 기록 방지
            } // 조건 끝
            reportedFallback = true; // 오류 기록 상태 저장
            Debug.LogError("[Project Epsilon] 충돌 처리기가 물리 스텝에 등록되지 않아 우선순위가 보장되지 않습니다."); // 즉시 판정 한계 알림
        } // 메서드 끝

        public bool TryApplyEnemyDirectContact()
        {
            if (IsFullInvulnerable())
            {
                return false;
            }

            if (Time.time < nextEnemyContactAllowedTime)
            {
                return false;
            }

            if (bodyManager == null)
            {
                return false;
            }

            int removed = bodyManager.RemoveBodies(enemyBodyLoss);

            if (removed <= 0)
            {
                return false;
            }

            nextEnemyContactAllowedTime =
                Time.time + enemyContactProtection;

            return true;
        }

        public bool TryApplyEliteCut() // 엘리트 예고 절단 처리
        { // 메서드 시작
            if (IsFullInvulnerable() || bodyManager == null) // 전체 무적과 몸통 확인
            { // 조건 시작
                return false; // 절단 차단
            } // 조건 끝
            return bodyManager.RemoveBodies(1) > 0; // 직접 충돌 보호와 독립된 절단
        } // 메서드 끝

        public bool TryApplyObstacleContact()
        {
            if (IsFullInvulnerable())
            {
                return false;
            }

            if (health == null || obstacleDamage <= 0)
            {
                return false;
            }

            bool damaged = health.TakeDamage(obstacleDamage);

            if (!damaged)
            {
                return false;
            }

            invulnerability?.StartInvulnerability(
                obstacleInvulnerability
            );

            return true;
        }

        private bool IsFullInvulnerable()
        {
            return invulnerability != null &&
                invulnerability.IsInvulnerable;
        }
    }
}
