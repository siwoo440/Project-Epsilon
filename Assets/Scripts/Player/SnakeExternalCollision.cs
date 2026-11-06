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
        private readonly List<Action<bool>> pendingEliteCuts = new List<Action<bool>>(); // 예약된 엘리트 절단
        private readonly List<PendingHealthDamage> pendingHealthDamage = new List<PendingHealthDamage>(); // 예약된 공용 체력 피해

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
            if (source == null) // 충돌 대상 확인
            { // 조건 시작
                return; // 잘못된 예약 무시
            } // 조건 끝
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                source.TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 끝
            pendingSelfCollision = source; // 자기 충돌 수집
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
            bool enemy = pendingEnemyContact; // 적 직접 충돌 보관
            bool obstacle = pendingObstacleContact; // 장애물 충돌 보관
            Action<bool>[] cuts = pendingEliteCuts.Count == 0 ? Array.Empty<Action<bool>>() : pendingEliteCuts.ToArray(); // 절단 요청 복사
            PendingHealthDamage[] damages = pendingHealthDamage.Count == 0 ? Array.Empty<PendingHealthDamage>() : pendingHealthDamage.ToArray(); // 피해 정보 복사
            ClearPendingContacts(); // 다음 물리 스텝 준비
            if (self != null && self.isActiveAndEnabled) // 활성 자기 충돌 확인
            { // 조건 시작
                self.TryApplySelfCollisionPenalty(); // 자기 몸통 손실과 무적
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
