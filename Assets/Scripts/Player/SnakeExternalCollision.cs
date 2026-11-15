using System; // 절단 결과 알림
using System.Collections.Generic; // 예약 피해 목록
using UnityEngine; // Unity 엔진 기능 연결
using ProjectEpsilon.Core; // 실제 게임 진행 상태
using ProjectEpsilon.Recording; // 실제 피해 원인 기록

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{ // 이름 공간 시작
    public sealed class SnakeExternalCollision : MonoBehaviour // 충돌·행동 자료 구조
    { // 형식 시작
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private SnakeInvulnerability invulnerability; // 필드·무적 설정

        [SerializeField] private int enemyBodyLoss = 1; // 필드·적 설정
        [SerializeField] private float enemyContactProtection = 1f; // 필드·적 설정
        [SerializeField] private int obstacleDamage = 25; // 필드·피해 설정
        [SerializeField] private float obstacleInvulnerability = 1.5f; // 필드·무적 설정
        [SerializeField] private float obstacleRecoveryGrace = 0.3f; // 장애물 이탈 후 재접촉 유예

        private float nextEnemyContactAllowedTime; // 다음 값·적 구성 선언
        private float nextObstacleContactAllowedTime; // 장애물 재접촉 허용 시각
        private bool wasInvulnerableInObstacle; // 무적 중 장애물 접촉 여부
        private Collider2D headCollider; // 실제 머리 충돌체
        private readonly HashSet<Collider2D> obstacleContacts = new HashSet<Collider2D>(); // 실제 접촉 중인 장애물
        private readonly List<Collider2D> recoveryOverlaps = new List<Collider2D>(); // 안전 위치 주변 충돌체
        private readonly List<Vector2> recoveryCandidates = new List<Vector2>(); // 장애물 경계 후보 위치
        private bool stepRegistered; // 물리 스텝 등록 상태
        private int registeredGeneration; // 마지막 물리 등록 세대
        private bool reportedFallback; // 즉시 처리 오류 기록 상태
        private bool pendingEnemyContact; // 예약된 적 직접 충돌
        private bool pendingObstacleContact; // 예약된 장애물 충돌
        private SnakeSelfCollision pendingSelfCollision; // 예약된 자기 충돌
        private bool pendingUnconditionalSelfCollision; // 몸통 정보 없는 기존 자기 충돌
        private readonly List<PendingSelfSegment> pendingSelfSegments = new List<PendingSelfSegment>(); // 같은 스텝 자기 충돌 몸통
        private readonly List<PendingCut> pendingEliteCuts = new List<PendingCut>(); // 예약된 절단과 실제 원인
        private readonly List<PendingHealthDamage> pendingHealthDamage = new List<PendingHealthDamage>(); // 예약된 공용 체력 피해

        private readonly struct PendingSelfSegment // 자기 충돌 몸통과 재확인 여부
        { // 형식 시작
            public readonly SnakeSegment Segment; // 실제 자기 충돌 몸통
            public readonly bool RequireStillTouching; // 물리 후 겹침 재확인 여부
            public PendingSelfSegment(SnakeSegment segment, bool requireStillTouching) // 실제 몸통 예약 생성
            { // 함수 시작
                Segment = segment; // 실제 몸통 저장
                RequireStillTouching = requireStillTouching; // 물리 후 재확인 설정
            } // 함수 종료
        } // 형식 종료

        private readonly struct PendingCut // 실제 절단 요청과 원인
        { // 형식 시작
            public readonly Action<bool> Completed; // 실제 절단 결과 수신자
            public readonly DamageOrigin Origin; // 실제 절단 원인
            public PendingCut(Action<bool> completed, DamageOrigin origin) // 실제 절단 요청 생성
            { // 함수 시작
                Completed = completed; // 실제 결과 수신자 보관
                Origin = origin; // 실제 절단 원인 보관
            } // 함수 종료
        } // 형식 종료

        private readonly struct PendingHealthDamage // 스텝 시작 시 피해 정보
        { // 형식 시작
            public readonly SnakeHealth Target; // 피해 대상
            public readonly int Amount; // 확정된 피해량
            public readonly DamageOrigin Origin; // 실제 예약 피해 원인

            public PendingHealthDamage(SnakeHealth target, int amount, DamageOrigin origin) // 피해 정보 생성
            { // 함수 시작
                Target = target; // 대상 참조 보관
                Amount = amount; // 피해량 값 보관
                Origin = origin; // 실제 피해 원인 보관
            } // 함수 종료
        } // 형식 종료

        public bool EnemyContactProtected => // 적 값 반환
            Time.time < nextEnemyContactAllowedTime; // 시간·다음 값 처리

        public float EnemyContactProtectionRemaining => // 적·남은 값 값 반환
            Mathf.Max(0f, nextEnemyContactAllowedTime - Time.time); // 최대·다음 값 기능 실행

        private void OnEnable() // 물리 스텝 처리 연결
        { // 함수 시작
            reportedFallback = false; // 오류 기록 상태 초기화
            if (Application.isPlaying) // 플레이 상태 확인
            { // 조건 시작
                RegisterPhysicsStep(); // 물리 직후 처리 등록
            } // 조건 종료
        } // 함수 종료

        private void FixedUpdate() // 물리 등록 상태 확인
        { // 함수 시작
            if (Application.isPlaying && (!stepRegistered || registeredGeneration != SnakePhysicsStepDispatcher.Generation)) // 플레이 시작과 등록 세대 확인
            { // 조건 시작
                RegisterPhysicsStep(); // 누락된 물리 등록 복구
            } // 조건 종료
            UpdateObstacleRecovery(); // 무적 종료 후 장애물 이탈 확인
        } // 함수 종료

        private void RegisterPhysicsStep() // 물리 스텝 등록
        { // 함수 시작
            stepRegistered = SnakePhysicsStepDispatcher.Register(ResolvePendingContacts); // 물리 직후 처리 등록
            registeredGeneration = SnakePhysicsStepDispatcher.Generation; // 현재 등록 세대 저장
        } // 함수 종료

        private void OnDisable() // 물리 스텝 처리 해제
        { // 함수 시작
            SnakePhysicsStepDispatcher.Unregister(ResolvePendingContacts); // 물리 완료 구독 해제
            stepRegistered = false; // 등록 상태 해제
            CancelPendingContacts(); // 예약 취소와 절단 결과 반환
            obstacleContacts.Clear(); // 이전 장애물 접촉 제거
            wasInvulnerableInObstacle = false; // 이전 장애물 이탈 상태 초기화
        } // 함수 종료

        public void Bind( // 연결 호출 시작
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            SnakeHealth snakeHealth, // 체력 항목 전달
            SnakeInvulnerability invulnerabilityController, // 무적·제어기 항목 전달
            int directEnemyBodyLoss, // 적·몸통 항목 전달
            float directEnemyProtection, // 적 항목 전달
            int obstacleSharedDamage, // 피해 항목 전달
            float obstacleFullInvulnerability // 무적 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            bodyManager = manager; // 몸통·관리자 값 설정
            health = snakeHealth; // 체력 값 설정
            invulnerability = invulnerabilityController; // 무적·제어기 값 설정

            enemyBodyLoss = Mathf.Max(1, directEnemyBodyLoss); // 적·몸통 값 설정
            enemyContactProtection = Mathf.Max(0f, directEnemyProtection); // 적·최대 값 설정
            obstacleDamage = Mathf.Max(0, obstacleSharedDamage); // 피해·최대 값 설정
            obstacleInvulnerability = Mathf.Max( // 무적·최대 호출 시작
                0f, // 대상 항목 전달
                obstacleFullInvulnerability // 무적 처리
            ); // 앞선 항목 종료

            nextEnemyContactAllowedTime = 0f; // 다음 값·적 값 설정
            nextObstacleContactAllowedTime = 0f; // 장애물 유예 시각 초기화
            wasInvulnerableInObstacle = false; // 이전 장애물 이탈 상태 초기화
            obstacleContacts.Clear(); // 이전 장애물 접촉 초기화
        } // 함수 종료

        private void OnTriggerEnter2D(Collider2D other) // 접촉·충돌체 기능 선언
        { // 함수 시작
            if (!isActiveAndEnabled) // 충돌 처리기 활성 확인
            { // 조건 시작
                return; // 비활성 충돌 무시
            } // 조건 종료

            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>(); // 조회·구성 요소 값 설정

            if (hazard == null) // 대상 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            switch (hazard.HazardType) // 형식 종류 분기
            { // 조건 시작
                case SnakeContactHazardType.EnemyDirect: // 형식·적 경우 선택
                    QueueEnemyContact(); // 직접 충돌 예약
                    break; // 반복 또는 분기 종료

                case SnakeContactHazardType.Obstacle: // 형식 경우 선택
                    obstacleContacts.Add(other); // 실제 장애물 접촉 보관
                    QueueObstacleContact(); // 장애물 충돌 예약
                    break; // 반복 또는 분기 종료
            } // 조건 종료
        } // 함수 종료

        private void OnTriggerStay2D(Collider2D other) // 지속 중인 장애물 접촉 확인
        { // 함수 시작
            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>(); // 실제 접촉 위험 조회
            if (hazard != null && hazard.HazardType == SnakeContactHazardType.Obstacle) // 실제 장애물 확인
            { // 조건 시작
                obstacleContacts.Add(other); // 유지 중인 장애물 접촉 보관
            } // 조건 종료
        } // 함수 종료

        private void OnTriggerExit2D(Collider2D other) // 종료된 장애물 접촉 정리
        { // 함수 시작
            obstacleContacts.Remove(other); // 이전 장애물 접촉 제거
        } // 함수 종료

        private void UpdateObstacleRecovery() // 무적 종료 시 장애물 이탈
        { // 함수 시작
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 진행 상태 확인
            { // 조건 시작
                return; // 정지 또는 종료 중 이탈 방지
            } // 조건 종료
            obstacleContacts.RemoveWhere(item => item == null || !item.isActiveAndEnabled); // 사라진 장애물 접촉 정리
            if (IsFullInvulnerable()) // 실제 전체 무적 확인
            { // 조건 시작
                wasInvulnerableInObstacle |= obstacleContacts.Count > 0; // 무적 중 장애물 접촉 기록
                return; // 무적 종료까지 대기
            } // 조건 종료
            if (!wasInvulnerableInObstacle) // 이전 장애물 접촉 확인
            { // 조건 시작
                return; // 이탈할 장애물 부재
            } // 조건 종료
            wasInvulnerableInObstacle = false; // 종료된 무적 상태 소비
            if (headCollider == null) // 실제 머리 충돌체 확인
            { // 조건 시작
                headCollider = GetComponent<Collider2D>(); // 실제 머리 충돌체 조회
            } // 조건 종료
            if (headCollider == null || !headCollider.enabled) // 유효한 충돌체 확인
            { // 조건 시작
                return; // 충돌체 없는 이탈 방지
            } // 조건 종료
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            Rigidbody2D headBody = GetComponent<Rigidbody2D>(); // 실제 머리 물리체
            if (headBody == null) // 실제 물리체 존재 확인
            { // 조건 시작
                return; // 안전 위치 검색 생략
            } // 조건 종료
            Vector2 original = headBody.position; // 실제 기존 물리 위치
            ContactFilter2D filter = ContactFilter2D.noFilter; // 전체 물리 계층 검색 조건
            filter.useTriggers = true; // 장애물 트리거 포함
            recoveryOverlaps.Clear(); // 이전 겹침 목록 제거
            recoveryCandidates.Clear(); // 이전 안전 위치 후보 제거
            Physics2D.OverlapCollider(headCollider, filter, recoveryOverlaps); // 실제 현재 머리 겹침 조회
            foreach (Collider2D other in recoveryOverlaps) // 현재 겹침 목록 순회
            { // 반복 시작
                SnakeContactHazard hazard = other == null ? null : other.GetComponent<SnakeContactHazard>(); // 실제 위험 종류 조회
                if (hazard == null || hazard.HazardType != SnakeContactHazardType.Obstacle) // 장애물 여부 확인
                { // 조건 시작
                    continue; // 다른 충돌체 제외
                } // 조건 종료
                ColliderDistance2D distance = headCollider.Distance(other); // 실제 충돌체 간 이탈 거리
                if (distance.isValid && distance.isOverlapped) // 실제 장애물 겹침 확인
                { // 조건 시작
                    recoveryCandidates.Add(original + distance.normal * (distance.distance - 0.01f)); // 가장 가까운 장애물 경계 후보
                } // 조건 종료
            } // 반복 종료
            if (recoveryCandidates.Count == 0) // 실제 겹친 장애물 부재 확인
            { // 조건 시작
                return; // 기존 위치 유지
            } // 조건 종료
            Vector2 best = original; // 실제 안전 위치 후보
            float bestDistance = float.PositiveInfinity; // 최단 이동 거리 초기화
            foreach (Vector2 candidate in recoveryCandidates) // 실제 경계 후보 순회
            { // 반복 시작
                float distance = Vector2.Distance(original, candidate); // 실제 후보 이동 거리
                if (distance < bestDistance && IsObstacleFree(candidate, headBody.rotation, filter)) // 더 가깝고 안전한 후보 확인
                { // 조건 시작
                    best = candidate; // 실제 안전 위치 저장
                    bestDistance = distance; // 최단 이동 거리 갱신
                } // 조건 종료
            } // 반복 종료
            if (float.IsPositiveInfinity(bestDistance)) // 인접 장애물로 경계 후보가 막힌 경우
            { // 조건 시작
                for (float radius = 0.05f; radius <= 8f && float.IsPositiveInfinity(bestDistance); radius += 0.05f) // 가까운 반경부터 안전 위치 검색
                { // 반복 시작
                    for (int direction = 0; direction < 32; direction++) // 주변 서른두 방향 순회
                    { // 반복 시작
                        float angle = direction * Mathf.PI * 2f / 32f; // 실제 검색 방향 각도
                        Vector2 candidate = original + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius; // 실제 주변 위치 후보
                        if (IsObstacleFree(candidate, headBody.rotation, filter)) // 모든 장애물과 분리 확인
                        { // 조건 시작
                            Vector2 directionVector = (candidate - original).normalized; // 실제 안전 방향
                            float lower = Mathf.Max(0f, radius - 0.05f); // 이전 검색 반경
                            float upper = radius; // 현재 안전 반경
                            for (int refinement = 0; refinement < 7; refinement++) // 경계 거리 정밀 탐색
                            { // 반복 시작
                                float middle = (lower + upper) * 0.5f; // 현재 중간 반경
                                if (IsObstacleFree(original + directionVector * middle, headBody.rotation, filter)) // 중간 위치 안전 여부
                                { // 조건 시작
                                    upper = middle; // 더 가까운 안전 반경
                                } // 조건 종료
                                else // 중간 위치 장애물 겹침
                                { // 조건 시작
                                    lower = middle; // 더 먼 위치 검색
                                } // 조건 종료
                            } // 반복 종료
                            if (upper < bestDistance) // 더 가까운 안전 방향 확인
                            { // 조건 시작
                                best = original + directionVector * upper; // 실제 안전 위치 저장
                                bestDistance = upper; // 실제 이동 거리 저장
                            } // 조건 종료
                        } // 조건 종료
                    } // 반복 종료
                } // 반복 종료
            } // 조건 종료
            if (!float.IsPositiveInfinity(bestDistance)) // 실제 안전 위치 발견 확인
            { // 조건 시작
                transform.position = new Vector3(best.x, best.y, transform.position.z); // 실제 안전 위치로 머리 이동
                Physics2D.SyncTransforms(); // 이동한 실제 물리 위치 반영
                nextObstacleContactAllowedTime = Time.time + Mathf.Max(0f, obstacleRecoveryGrace); // 장애물 재접촉 유예 시작
                return; // 안전 위치 확정
            } // 조건 종료
            Debug.LogWarning("[Project Epsilon] 겹친 장애물에서 안전한 위치를 찾지 못했습니다."); // 해결 불가 상태 기록
        } // 함수 종료

        private bool IsObstacleFree(Vector2 position, float rotation, ContactFilter2D filter) // 후보 위치의 장애물 겹침 확인
        { // 함수 시작
            recoveryOverlaps.Clear(); // 이전 후보 겹침 제거
            headCollider.Overlap(position, rotation, filter, recoveryOverlaps); // 실제 후보 위치 물리 검색
            foreach (Collider2D other in recoveryOverlaps) // 실제 겹침 목록 순회
            { // 반복 시작
                SnakeContactHazard hazard = other == null ? null : other.GetComponent<SnakeContactHazard>(); // 실제 위험 종류 조회
                if (hazard != null && hazard.HazardType == SnakeContactHazardType.Obstacle) // 실제 장애물 겹침 확인
                { // 조건 시작
                    return false; // 장애물 위치 거절
                } // 조건 종료
            } // 반복 종료
            return true; // 실제 장애물 없는 위치
        } // 함수 종료

        public void QueueSelfCollision(SnakeSelfCollision source) // 자기 충돌 예약
        { // 함수 시작
            QueueSelfCollision(source, null); // 기존 몸통 정보 없는 예약 유지
        } // 함수 종료

        public void QueueSelfCollision(SnakeSelfCollision source, SnakeSegment segment) // 몸통별 자기 충돌 예약
        { // 함수 시작
            QueueSelfCollision(source, segment, false); // 실제 접촉 콜백 예약
        } // 함수 종료

        public void QueueDeferredSelfCollision(SnakeSelfCollision source, SnakeSegment segment) // 유예 종료 겹침 예약
        { // 함수 시작
            QueueSelfCollision(source, segment, true); // 물리 후 실제 겹침 재확인 예약
        } // 함수 종료

        private void QueueSelfCollision(SnakeSelfCollision source, SnakeSegment segment, bool requireStillTouching) // 실제 자기 충돌 예약 공통 처리
        { // 함수 시작
            if (source == null) // 충돌 대상 확인
            { // 조건 시작
                return; // 잘못된 예약 무시
            } // 조건 종료
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                if (segment != null && segment.IsBossPushGraceActive) // 해당 몸통의 보스 유예 확인
                { // 조건 시작
                    source.DeferBossPushContact(segment); // 지속 겹침의 나중 재판정
                    return; // 현재 자기 충돌 생략
                } // 조건 종료
                if (requireStillTouching && !source.IsStillTouching(segment)) // 유예 접촉의 현재 겹침 확인
                { // 조건 시작
                    return; // 이미 분리된 몸통 무시
                } // 조건 종료
                source.TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 종료
            pendingSelfCollision = source; // 자기 충돌 수집
            if (segment == null) // 몸통 정보 없는 기존 예약 확인
            { // 조건 시작
                pendingUnconditionalSelfCollision = true; // 기존 자기 충돌 보존
            } // 조건 종료
            else // 실제 몸통을 포함한 예약 확인
            { // 조건 시작
                pendingSelfSegments.Add(new PendingSelfSegment(segment, requireStillTouching)); // 해당 몸통과 재확인 여부 저장
            } // 조건 종료
        } // 함수 종료

        public void QueueEliteCut(Action<bool> completed, DamageOrigin origin = DamageOrigin.EliteCut) // 엘리트 절단 예약
        { // 함수 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                completed?.Invoke(TryApplyEliteCut(origin)); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 종료
            pendingEliteCuts.Add(new PendingCut(completed, origin)); // 절단 결과 수신자 보관
        } // 함수 종료

        public void QueueHealthDamage(SnakeHealth target, int damage, DamageOrigin origin = DamageOrigin.Unknown) // 적의 공용 체력 피해 예약
        { // 함수 시작
            if (target == null || damage <= 0) // 유효 대상과 피해 확인
            { // 조건 시작
                return; // 무효 피해 제외
            } // 조건 종료
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                target.TakeDamage(damage, origin); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 종료
            pendingHealthDamage.Add(new PendingHealthDamage(target, damage, origin)); // 대상과 피해량 값 보관
        } // 함수 종료

        private void QueueEnemyContact() // 적 직접 충돌 예약
        { // 함수 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                TryApplyEnemyDirectContact(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 종료
            pendingEnemyContact = true; // 직접 충돌 수집
        } // 함수 종료

        private void QueueObstacleContact() // 장애물 충돌 예약
        { // 함수 시작
            if (!CanQueueContacts()) // 물리 스텝 등록 확인
            { // 조건 시작
                ReportImmediateFallback(); // 우선순위 미보장 알림
                TryApplyObstacleContact(); // 기존 즉시 판정
                return; // 예약 종료
            } // 조건 종료
            pendingObstacleContact = true; // 장애물 충돌 수집
        } // 함수 종료

        private void ResolvePendingContacts() // 같은 물리 스텝 충돌 우선 처리
        { // 함수 시작
            GameManager game = GameManager.Instance; // 실제 게임 종료 상태 조회
            if (game != null && (game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated)) // 이미 확정된 승패 확인
            { // 조건 시작
                CancelPendingContacts(); // 종료 후 충돌 취소와 절단 결과 반환
                return; // 종료된 전투의 피해 차단
            } // 조건 종료
            if (pendingSelfCollision == null && !pendingEnemyContact && !pendingObstacleContact && pendingEliteCuts.Count == 0 && pendingHealthDamage.Count == 0) // 빈 물리 스텝 확인
            { // 조건 시작
                return; // 불필요한 목록 복사 방지
            } // 조건 종료
            SnakeSelfCollision self = pendingSelfCollision; // 자기 충돌 보관
            bool unconditionalSelf = pendingUnconditionalSelfCollision; // 기존 자기 충돌 보관
            PendingSelfSegment[] selfSegments = pendingSelfSegments.Count == 0 ? Array.Empty<PendingSelfSegment>() : pendingSelfSegments.ToArray(); // 예약된 몸통 목록 보관
            bool enemy = pendingEnemyContact; // 적 직접 충돌 보관
            bool obstacle = pendingObstacleContact; // 장애물 충돌 보관
            PendingCut[] cuts = pendingEliteCuts.Count == 0 ? Array.Empty<PendingCut>() : pendingEliteCuts.ToArray(); // 절단 요청 복사
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
                    } // 조건 종료
                    if (candidate.RequireStillTouching && !self.IsStillTouching(segment)) // 물리 스텝 이후 실제 겹침 확인
                    { // 조건 시작
                        continue; // 같은 스텝에 분리된 몸통 제외
                    } // 조건 종료
                    if (segment.IsBossPushGraceActive) // 같은 스텝 보스 밀림 유예 확인
                    { // 조건 시작
                        self.DeferBossPushContact(segment); // 유예 종료 후 지속 겹침 재판정
                    } // 조건 종료
                    else // 보호받지 않는 실제 몸통 확인
                    { // 조건 시작
                        applySelf = true; // 기존 자기 충돌 적용
                    } // 조건 종료
                } // 반복 종료
                if (applySelf) // 적용 가능한 자기 충돌 확인
                { // 조건 시작
                    self.TryApplySelfCollisionPenalty(); // 자기 충돌 체력 피해와 무적
                } // 조건 종료
            } // 조건 종료
            if (enemy && !IsUnavailable()) // 적 충돌과 활성·생존 상태 확인
            { // 조건 시작
                TryApplyEnemyDirectContact(); // 적 직접 몸통 손실
            } // 조건 종료
            foreach (PendingCut cut in cuts) // 엘리트 절단 요청 순회
            { // 반복 시작
                bool applied = !IsUnavailable() && TryApplyEliteCut(cut.Origin); // 활성 상태와 절단 확인
                cut.Completed?.Invoke(applied); // 개별 절단 결과 전달
            } // 반복 종료
            if (obstacle && !IsUnavailable()) // 장애물 충돌과 활성·생존 확인
            { // 조건 시작
                TryApplyObstacleContact(); // 장애물 피해와 새 무적
            } // 조건 종료
            foreach (PendingHealthDamage damage in damages) // 예약된 적 피해 순회
            { // 반복 시작
                if (IsUnavailable()) // 비활성 또는 패배 확인
                { // 조건 시작
                    break; // 남은 피해 중단
                } // 조건 종료
                if (damage.Target != null) // 피해 대상 생존 확인
                { // 조건 시작
                    damage.Target.TakeDamage(damage.Amount, damage.Origin); // 보호막과 무적을 통한 피해
                } // 조건 종료
            } // 반복 종료
        } // 함수 종료

        private bool IsBodyDepleted() // 남은 몸통 확인
        { // 함수 시작
            return bodyManager != null && bodyManager.CurrentBodyCount <= 0; // 몸통 소진 여부
        } // 함수 종료

        private bool IsUnavailable() // 충돌 처리 대상 상태 확인
        { // 함수 시작
            return this == null || !isActiveAndEnabled || IsBodyDepleted() || IsRunEnded(); // 파괴·비활성·몸통 소진 또는 종료 여부
        } // 함수 종료

        private static bool IsRunEnded() // 실제 확정된 승패 확인
        { // 함수 시작
            GameManager game = GameManager.Instance; // 실제 게임 관리자 조회
            return game != null && (game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated); // 실제 종료 상태 반환
        } // 함수 종료

        private bool CanQueueContacts() // 현재 물리 예약 가능 여부
        { // 함수 시작
            return stepRegistered && SnakePhysicsStepDispatcher.CanQueue; // 등록과 자동 물리 방식 확인
        } // 함수 종료

        private void CancelPendingContacts() // 비활성화 시 요청 취소
        { // 함수 시작
            PendingCut[] cuts = pendingEliteCuts.ToArray(); // 미처리 절단 결과 보관
            ClearPendingContacts(); // 수집한 충돌 제거
            foreach (PendingCut cut in cuts) // 절단 요청 순회
            { // 반복 시작
                cut.Completed?.Invoke(false); // 절단 취소 결과 전달
            } // 반복 종료
        } // 함수 종료

        private void ClearPendingContacts() // 수집한 충돌 초기화
        { // 함수 시작
            pendingSelfCollision = null; // 자기 충돌 초기화
            pendingUnconditionalSelfCollision = false; // 기존 자기 충돌 초기화
            pendingSelfSegments.Clear(); // 몸통별 자기 충돌 초기화
            pendingEnemyContact = false; // 적 직접 충돌 초기화
            pendingObstacleContact = false; // 장애물 충돌 초기화
            pendingEliteCuts.Clear(); // 엘리트 절단 초기화
            pendingHealthDamage.Clear(); // 공용 체력 피해 초기화
        } // 함수 종료

        private void ReportImmediateFallback() // 물리 스텝 처리 누락 알림
        { // 함수 시작
            if (reportedFallback) // 중복 오류 확인
            { // 조건 시작
                return; // 중복 기록 방지
            } // 조건 종료
            reportedFallback = true; // 오류 기록 상태 저장
            Debug.LogError("[Project Epsilon] 충돌 처리기가 물리 스텝에 등록되지 않아 우선순위가 보장되지 않습니다."); // 즉시 판정 한계 알림
        } // 함수 종료

        public bool TryApplyEnemyDirectContact() // 시도·적용 기능 선언
        { // 함수 시작
            if (IsRunEnded() || IsFullInvulnerable()) // 종료 상태와 전체 무적 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            if (Time.time < nextEnemyContactAllowedTime) // 시간·다음 값 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            if (bodyManager == null) // 몸통·관리자 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            int loss = Mathf.Min(enemyBodyLoss, bodyManager.CurrentBodyCount); // 실제 손실 가능한 몸통 수
            if (loss > 0) // 실제 적 접촉 손실 확인
            { // 조건 시작
                RunTelemetry.RecordDamage(DamageOrigin.EnemyContact, 0, loss); // 실제 적 직접 충돌 기록
            } // 조건 종료
            int removed = bodyManager.RemoveBodies(enemyBodyLoss); // 몸통·관리자 값 설정

            if (removed <= 0) // 대상 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            nextEnemyContactAllowedTime = // 다음 값·적 값 설정
                Time.time + enemyContactProtection; // 시간·적 처리

            return true; // 성공 결과 반환
        } // 함수 종료

        public bool TryApplyEliteCut(DamageOrigin origin = DamageOrigin.EliteCut) // 엘리트 예고 절단 처리
        { // 함수 시작
            if (IsRunEnded() || IsFullInvulnerable() || bodyManager == null) // 종료 상태와 전체 무적 및 몸통 확인
            { // 조건 시작
                return false; // 절단 차단
            } // 조건 종료
            if (bodyManager.CurrentBodyCount > 0) // 실제 절단 가능한 몸통 확인
            { // 조건 시작
                RunTelemetry.RecordDamage(origin, 0, 1); // 실제 엘리트 또는 보스 절단 기록
            } // 조건 종료
            return bodyManager.RemoveBodies(1) > 0; // 직접 충돌 보호와 독립된 절단
        } // 함수 종료

        public bool TryApplyObstacleContact() // 시도·적용 기능 선언
        { // 함수 시작
            if (IsFullInvulnerable() || Time.time < nextObstacleContactAllowedTime) // 무적 또는 장애물 재접촉 유예 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            if (health == null || obstacleDamage <= 0) // 체력·피해 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            bool damaged = health.TakeDamage(obstacleDamage, DamageOrigin.Obstacle); // 체력·받기 값 설정

            if (!damaged) // 대상 조건 확인
            { // 조건 시작
                return false; // 실패 결과 반환
            } // 조건 종료

            invulnerability?.StartInvulnerability( // 무적·시작 호출 시작
                obstacleInvulnerability // 무적 처리
            ); // 앞선 항목 종료

            return true; // 성공 결과 반환
        } // 함수 종료

        private bool IsFullInvulnerable() // 무적 기능 선언
        { // 함수 시작
            return invulnerability != null && // 무적 결과 반환
                invulnerability.IsInvulnerable; // 무적 처리
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
