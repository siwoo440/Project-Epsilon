using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Player; // 실제 몸통과 경로 추적
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{ // 영역 시작
    public sealed class PrototypeBossBodyPush : MonoBehaviour // 시험 보스의 몸통 밀림
    { // 클래스 시작
        private SnakeBodyManager body; // 실제 플레이어 몸통 관리자
        private SnakeBodyFollower follower; // 실제 몸통 경로 추적기
        private float pushDistance; // 몸통 밀림 거리 임시값
        private float recoveryDuration; // 원래 경로 복귀 시간 임시값
        private float selfCollisionGrace; // 몸통별 자기 충돌 유예 임시값

        public void Configure(SnakeBodyManager manager, SnakeBodyFollower bodyFollower, float distance, float recovery, float grace) // 실제 몸통 밀림 설정
        { // 메서드 시작
            body = manager; // 실제 몸통 관리자 저장
            follower = bodyFollower; // 실제 경로 추적기 저장
            pushDistance = Mathf.Max(0f, distance); // 안전한 밀림 거리
            recoveryDuration = Mathf.Max(0.01f, recovery); // 안전한 경로 복귀 시간
            selfCollisionGrace = Mathf.Max(0f, grace); // 안전한 자기 충돌 유예
        } // 메서드 끝

        private void OnTriggerEnter2D(Collider2D other) // 실제 보스와 몸통 진입
        { // 메서드 시작
            PushBody(other); // 실제 몸통 밀림 적용
        } // 메서드 끝
        private void OnTriggerStay2D(Collider2D other) // 실제 보스와 몸통 지속 접촉
        { // 메서드 시작
            PushBody(other); // 실제 접촉 중 밀림 갱신
        } // 메서드 끝
        private void PushBody(Collider2D other) // 실제 접촉 몸통 밀림
        { // 메서드 시작
            if (!isActiveAndEnabled || other == null || body == null || follower == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || pushDistance <= 0f) // 실제 활성 전투와 밀림 설정 확인
            { // 조건 시작
                return; // 비활성 또는 유효하지 않은 접촉 무시
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
            if (segment == null || !segment.IsBody || !segment.transform.IsChildOf(body.transform)) // 실제 플레이어 몸통 확인
            { // 조건 시작
                return; // 머리와 다른 충돌 무시
            } // 조건 끝
            Vector3 offset = segment.transform.position - transform.position; // 실제 보스에서 몸통 방향
            offset.z = 0f; // 이차원 방향 고정
            Vector3 direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : Vector3.right; // 안전한 밀림 방향
            if (!follower.ApplyBossPush(segment, direction * pushDistance, recoveryDuration)) // 실제 추적 몸통 밀림 확인
            { // 조건 시작
                return; // 유효하지 않은 몸통 유예 방지
            } // 조건 끝
            segment.BeginBossPushGrace(selfCollisionGrace); // 해당 몸통의 자기 충돌 유예
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
