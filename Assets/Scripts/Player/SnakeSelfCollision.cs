using System.Collections.Generic; // 유예 중인 몸통 목록
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SnakeSelfCollision : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeInvulnerability invulnerability;
        [SerializeField] private int bodyLoss = 2;
        [SerializeField] private float invulnerabilityDuration = 2f;
        private bool reportedMissingResolver; // 충돌 처리기 누락 기록
        private readonly HashSet<SnakeSegment> deferredBossPushContacts = new HashSet<SnakeSegment>(); // 보스 밀림으로 미룬 몸통 접촉
        private readonly List<SnakeSegment> deferredBossPushChecks = new List<SnakeSegment>(); // 실제 유예 종료 확인 목록
        private Collider2D headCollider; // 실제 머리 충돌체

        public void Bind(
            SnakeBodyManager manager,
            SnakeInvulnerability invulnerabilityController,
            int lossCount,
            float duration
        )
        {
            bodyManager = manager;
            invulnerability = invulnerabilityController;
            bodyLoss = Mathf.Max(1, lossCount);
            invulnerabilityDuration = Mathf.Max(0f, duration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled) // 충돌 처리기 활성 확인
            { // 조건 시작
                return; // 비활성 충돌 무시
            } // 조건 끝

            SnakeSegment segment = other.GetComponent<SnakeSegment>();

            if (segment == null || !segment.IsBody)
            {
                return;
            }

            QueueOrDefer(segment); // 실제 몸통별 유예와 충돌 예약
        }

        private void OnTriggerStay2D(Collider2D other) // 유예 중 유지된 자기 접촉
        { // 메서드 시작
            if (!isActiveAndEnabled) // 실제 자기 충돌 활성 확인
            { // 조건 시작
                return; // 비활성 접촉 무시
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
            if (segment == null || !deferredBossPushContacts.Contains(segment) || segment.IsBossPushGraceActive) // 유예했던 몸통과 유예 종료 확인
            { // 조건 시작
                return; // 다른 접촉 또는 유예 유지
            } // 조건 끝
            deferredBossPushContacts.Remove(segment); // 재판정 전 유예 기록 제거
            QueueOrDefer(segment); // 유지된 접촉의 실제 재판정
        } // 메서드 끝

        private void FixedUpdate() // 절전 중에도 유예 접촉 재판정
        { // 메서드 시작
            if (!isActiveAndEnabled || deferredBossPushContacts.Count == 0) // 활성 상태와 유예 접촉 확인
            { // 조건 시작
                return; // 불필요한 재판정 생략
            } // 조건 끝
            if (headCollider == null) // 실제 머리 충돌체 확인
            { // 조건 시작
                headCollider = GetComponent<Collider2D>(); // 실제 머리 충돌체 조회
            } // 조건 끝
            if (headCollider == null || !headCollider.enabled) // 유효한 머리 충돌체 확인
            { // 조건 시작
                deferredBossPushContacts.Clear(); // 비활성 접촉 기록 제거
                return; // 실제 접촉 확인 생략
            } // 조건 끝
            deferredBossPushChecks.Clear(); // 이전 검사 목록 초기화
            foreach (SnakeSegment segment in deferredBossPushContacts) // 유예 중 몸통 순회
            { // 반복 시작
                if (segment == null || !segment.IsBossPushGraceActive) // 파괴된 몸통 또는 유예 종료 확인
                { // 조건 시작
                    deferredBossPushChecks.Add(segment); // 실제 접촉 검사 대상 추가
                } // 조건 끝
            } // 반복 끝
            foreach (SnakeSegment segment in deferredBossPushChecks) // 유예 종료 몸통 순회
            { // 반복 시작
                deferredBossPushContacts.Remove(segment); // 검사한 유예 기록 제거
                if (segment == null) // 파괴된 몸통 확인
                { // 조건 시작
                    continue; // 무효 몸통 제외
                } // 조건 끝
                if (IsStillTouching(segment)) // 절전 상태의 실제 겹침 확인
                { // 조건 시작
                    QueueOrDefer(segment, true); // 물리 후 겹침을 재확인할 예약
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private void OnTriggerExit2D(Collider2D other) // 실제 몸통 접촉 종료
        { // 메서드 시작
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 접촉 종료 몸통 조회
            if (segment != null) // 실제 몸통 확인
            { // 조건 시작
                deferredBossPushContacts.Remove(segment); // 종료된 유예 접촉 제거
            } // 조건 끝
        } // 메서드 끝

        private void OnDisable() // 자기 충돌 비활성 정리
        { // 메서드 시작
            deferredBossPushContacts.Clear(); // 이전 유예 접촉 초기화
            deferredBossPushChecks.Clear(); // 이전 유예 검사 목록 초기화
        } // 메서드 끝

        public void DeferBossPushContact(SnakeSegment segment) // 보스 밀림 접촉 유예
        { // 메서드 시작
            if (segment == null || !segment.IsBody) // 실제 몸통 확인
            { // 조건 시작
                return; // 무효 유예 무시
            } // 조건 끝
            deferredBossPushContacts.RemoveWhere(item => item == null); // 파괴된 유예 몸통 정리
            deferredBossPushContacts.Add(segment); // 실제 겹침 재판정 보류
        } // 메서드 끝

        public bool IsStillTouching(SnakeSegment segment) // 실제 머리와 몸통 겹침 확인
        { // 메서드 시작
            if (segment == null) // 실제 몸통 확인
            { // 조건 시작
                return false; // 파괴된 몸통 제외
            } // 조건 끝
            if (headCollider == null) // 실제 머리 충돌체 확인
            { // 조건 시작
                headCollider = GetComponent<Collider2D>(); // 실제 머리 충돌체 조회
            } // 조건 끝
            Collider2D bodyCollider = segment.GetComponent<Collider2D>(); // 실제 몸통 충돌체 조회
            return headCollider != null && headCollider.enabled && bodyCollider != null && bodyCollider.enabled && headCollider.IsTouching(bodyCollider); // 실제 물리 겹침 여부
        } // 메서드 끝

        private void QueueOrDefer(SnakeSegment segment, bool requireStillTouching = false) // 실제 자기 충돌 예약
        { // 메서드 시작
            if (segment.IsBossPushGraceActive) // 해당 몸통의 보스 밀림 유예 확인
            { // 조건 시작
                DeferBossPushContact(segment); // 유예 종료 후 겹침 재판정
                return; // 현재 자기 충돌 방지
            } // 조건 끝
            SnakeExternalCollision resolver = GetComponent<SnakeExternalCollision>(); // 물리 스텝 충돌 처리기 조회
            if (resolver == null || !resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            { // 조건 시작
                if (!reportedMissingResolver) // 누락 오류 중복 확인
                { // 조건 시작
                    reportedMissingResolver = true; // 누락 오류 기록 상태
                    Debug.LogError("[Project Epsilon] 자기 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
                } // 조건 끝
                if (!requireStillTouching || IsStillTouching(segment)) // 유예 접촉의 현재 겹침 확인
                { // 조건 시작
                    TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                } // 조건 끝
                return; // 예약 종료
            } // 조건 끝
            if (requireStillTouching) // 유예 종료 뒤 절전 접촉 확인
            { // 조건 시작
                resolver.QueueDeferredSelfCollision(this, segment); // 물리 후 실제 겹침 재확인 예약
            } // 조건 끝
            else // 실제 물리 콜백 확인
            { // 대안 시작
                resolver.QueueSelfCollision(this, segment); // 같은 물리 스텝에 해당 몸통 예약
            } // 조건 끝
        } // 메서드 끝

        public bool TryApplySelfCollisionPenalty()
        {
            if (bodyManager == null)
            {
                return false;
            }

            if (invulnerability != null && invulnerability.IsInvulnerable)
            {
                return false;
            }

            int removed = bodyManager.RemoveBodies(bodyLoss);

            if (removed <= 0)
            {
                return false;
            }

            invulnerability?.StartInvulnerability(invulnerabilityDuration);
            return true;
        }
    }
}
