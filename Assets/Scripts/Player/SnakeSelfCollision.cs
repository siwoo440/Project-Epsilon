using System.Collections.Generic; // 유예 중인 몸통 목록
using UnityEngine; // Unity 엔진 기능 연결
using ProjectEpsilon.Core; // 실제 게임 종료 상태
using ProjectEpsilon.Recording; // 실제 자기 충돌 기록

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    [RequireComponent(typeof(CircleCollider2D))] // 구성 요소·충돌체 설정
    [RequireComponent(typeof(Rigidbody2D))] // 구성 요소 설정
    public sealed class SnakeSelfCollision : MonoBehaviour // 충돌·행동 자료 구조
    {
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private SnakeInvulnerability invulnerability; // 필드·무적 설정
        [SerializeField] private int bodyLoss = 2; // 필드·몸통 설정
        [SerializeField] private float invulnerabilityDuration = 2f; // 필드·무적 설정
        private bool reportedMissingResolver; // 충돌 처리기 누락 기록
        private readonly HashSet<SnakeSegment> deferredBossPushContacts = new HashSet<SnakeSegment>(); // 보스 밀림으로 미룬 몸통 접촉
        private readonly List<SnakeSegment> deferredBossPushChecks = new List<SnakeSegment>(); // 실제 유예 종료 확인 목록
        private Collider2D headCollider; // 실제 머리 충돌체

        public void Bind( // 연결 호출 시작
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            SnakeInvulnerability invulnerabilityController, // 무적·제어기 항목 전달
            int lossCount, // 개수 항목 전달
            float duration // 지속 시간 처리
        ) // 앞선 항목 종료
        {
            bodyManager = manager; // 몸통·관리자 값 설정
            invulnerability = invulnerabilityController; // 무적·제어기 값 설정
            bodyLoss = Mathf.Max(1, lossCount); // 몸통·최대 값 설정
            invulnerabilityDuration = Mathf.Max(0f, duration); // 무적·지속 시간 값 설정
        }

        private void OnTriggerEnter2D(Collider2D other) // 접촉·충돌체 기능 선언
        {
            if (!isActiveAndEnabled) // 충돌 처리기 활성 확인
            {
                return; // 비활성 충돌 무시
            }

            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 몸통·조회 값 설정

            if (segment == null || !segment.IsBody) // 몸통 조건 확인
            {
                return; // 현재 기능 종료
            }

            QueueOrDefer(segment); // 실제 몸통별 유예와 충돌 예약
        }

        private void OnTriggerStay2D(Collider2D other) // 유예 중 유지된 자기 접촉
        {
            if (!isActiveAndEnabled) // 실제 자기 충돌 활성 확인
            {
                return; // 비활성 접촉 무시
            }
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
            if (segment == null || !deferredBossPushContacts.Contains(segment) || segment.IsBossPushGraceActive) // 유예했던 몸통과 유예 종료 확인
            {
                return; // 다른 접촉 또는 유예 유지
            }
            deferredBossPushContacts.Remove(segment); // 재판정 전 유예 기록 제거
            QueueOrDefer(segment); // 유지된 접촉의 실제 재판정
        }

        private void FixedUpdate() // 절전 중에도 유예 접촉 재판정
        {
            if (!isActiveAndEnabled || deferredBossPushContacts.Count == 0) // 활성 상태와 유예 접촉 확인
            {
                return; // 불필요한 재판정 생략
            }
            if (headCollider == null) // 실제 머리 충돌체 확인
            {
                headCollider = GetComponent<Collider2D>(); // 실제 머리 충돌체 조회
            }
            if (headCollider == null || !headCollider.enabled) // 유효한 머리 충돌체 확인
            {
                deferredBossPushContacts.Clear(); // 비활성 접촉 기록 제거
                return; // 실제 접촉 확인 생략
            }
            deferredBossPushChecks.Clear(); // 이전 검사 목록 초기화
            foreach (SnakeSegment segment in deferredBossPushContacts) // 유예 중 몸통 순회
            {
                if (segment == null || !segment.IsBossPushGraceActive) // 파괴된 몸통 또는 유예 종료 확인
                {
                    deferredBossPushChecks.Add(segment); // 실제 접촉 검사 대상 추가
                }
            }
            foreach (SnakeSegment segment in deferredBossPushChecks) // 유예 종료 몸통 순회
            {
                deferredBossPushContacts.Remove(segment); // 검사한 유예 기록 제거
                if (segment == null) // 파괴된 몸통 확인
                {
                    continue; // 무효 몸통 제외
                }
                if (IsStillTouching(segment)) // 절전 상태의 실제 겹침 확인
                {
                    QueueOrDefer(segment, true); // 물리 후 겹침을 재확인할 예약
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other) // 실제 몸통 접촉 종료
        {
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 접촉 종료 몸통 조회
            if (segment != null) // 실제 몸통 확인
            {
                deferredBossPushContacts.Remove(segment); // 종료된 유예 접촉 제거
            }
        }

        private void OnDisable() // 자기 충돌 비활성 정리
        {
            deferredBossPushContacts.Clear(); // 이전 유예 접촉 초기화
            deferredBossPushChecks.Clear(); // 이전 유예 검사 목록 초기화
        }

        public void DeferBossPushContact(SnakeSegment segment) // 보스 밀림 접촉 유예
        {
            if (segment == null || !segment.IsBody) // 실제 몸통 확인
            {
                return; // 무효 유예 무시
            }
            deferredBossPushContacts.RemoveWhere(item => item == null); // 파괴된 유예 몸통 정리
            deferredBossPushContacts.Add(segment); // 실제 겹침 재판정 보류
        }

        public bool IsStillTouching(SnakeSegment segment) // 실제 머리와 몸통 겹침 확인
        {
            if (segment == null) // 실제 몸통 확인
            {
                return false; // 파괴된 몸통 제외
            }
            if (headCollider == null) // 실제 머리 충돌체 확인
            {
                headCollider = GetComponent<Collider2D>(); // 실제 머리 충돌체 조회
            }
            Collider2D bodyCollider = segment.GetComponent<Collider2D>(); // 실제 몸통 충돌체 조회
            return headCollider != null && headCollider.enabled && bodyCollider != null && bodyCollider.enabled && headCollider.IsTouching(bodyCollider); // 실제 물리 겹침 여부
        }

        private void QueueOrDefer(SnakeSegment segment, bool requireStillTouching = false) // 실제 자기 충돌 예약
        {
            if (segment.IsBossPushGraceActive) // 해당 몸통의 보스 밀림 유예 확인
            {
                DeferBossPushContact(segment); // 유예 종료 후 겹침 재판정
                return; // 현재 자기 충돌 방지
            }
            SnakeExternalCollision resolver = GetComponent<SnakeExternalCollision>(); // 물리 스텝 충돌 처리기 조회
            if (resolver == null || !resolver.isActiveAndEnabled) // 충돌 처리기 연결 확인
            {
                if (!reportedMissingResolver) // 누락 오류 중복 확인
                {
                    reportedMissingResolver = true; // 누락 오류 기록 상태
                    Debug.LogError("[Project Epsilon] 자기 충돌 처리기가 없어 충돌 우선순위가 보장되지 않습니다."); // 구성 누락 알림
                }
                if (!requireStillTouching || IsStillTouching(segment)) // 유예 접촉의 현재 겹침 확인
                {
                    TryApplySelfCollisionPenalty(); // 기존 즉시 판정
                }
                return; // 예약 종료
            }
            if (requireStillTouching) // 유예 종료 뒤 절전 접촉 확인
            {
                resolver.QueueDeferredSelfCollision(this, segment); // 물리 후 실제 겹침 재확인 예약
            }
            else // 실제 물리 콜백 확인
            {
                resolver.QueueSelfCollision(this, segment); // 같은 물리 스텝에 해당 몸통 예약
            }
        }

        public bool TryApplySelfCollisionPenalty() // 시도·적용 기능 선언
        {
            GameManager game = GameManager.Instance; // 실제 게임 관리자 조회
            if (game != null && (game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated)) // 이미 확정된 승패 확인
            {
                return false; // 종료 이후 자기 충돌 차단
            }
            if (bodyManager == null) // 몸통·관리자 조건 확인
            {
                return false; // 실패 결과 반환
            }

            if (invulnerability != null && invulnerability.IsInvulnerable) // 무적 조건 확인
            {
                return false; // 실패 결과 반환
            }

            int loss = Mathf.Min(bodyLoss, bodyManager.CurrentBodyCount); // 실제 손실 가능한 몸통 수
            if (loss > 0) // 실제 손실 여부 확인
            {
                RunTelemetry.RecordDamage(DamageOrigin.SelfCollision, 0, loss); // 실제 자기 충돌 원인 기록
            }
            int removed = bodyManager.RemoveBodies(bodyLoss); // 몸통·관리자 값 설정

            if (removed <= 0) // 대상 조건 확인
            {
                return false; // 실패 결과 반환
            }

            invulnerability?.StartInvulnerability(invulnerabilityDuration); // 무적·시작 기능 실행
            return true; // 성공 결과 반환
        }
    }
}
