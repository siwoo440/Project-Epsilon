using System.Collections.Generic; // 밀린 몸통 상태 목록
using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(200)] // 대상 설정
    public sealed class SnakeBodyFollower : MonoBehaviour // 몸통·행동 자료 구조
    {
        [SerializeField] private SnakePathRecorder pathRecorder; // 필드·경로 설정
        [SerializeField] private Transform[] bodySegments = new Transform[0]; // 필드·위치 설정
        [SerializeField] private Transform tailSegment; // 필드·위치 설정
        [SerializeField] private float segmentSpacing = 0.58f; // 필드·몸통 설정
        private readonly Dictionary<Transform, BossPushState> bossPushes = new Dictionary<Transform, BossPushState>(); // 몸통별 보스 밀림
        private readonly List<Transform> expiredBossPushes = new List<Transform>(); // 재사용할 만료 몸통 목록

        private sealed class BossPushState // 몸통의 임시 밀림 상태
        {
            public Vector3 Offset; // 경로 기준 밀림 거리
            public float Remaining; // 경로 복귀 남은 시간
        }

        public int SegmentCount => bodySegments == null ? 0 : bodySegments.Length; // 몸통·개수 값 반환
        public float SegmentSpacing => segmentSpacing; // 몸통 값 반환

        private void LateUpdate() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            AdvanceBossPushes(Time.deltaTime); // 밀린 몸통의 경로 복귀 갱신
            FollowPath(); // 경로 기능 실행
        }

        // Day04 Setup과의 호환성을 유지하기 위한 기존 Bind 오버로드
        public void Bind( // 연결 호출 시작
            SnakePathRecorder recorder, // 경로 항목 전달
            Transform[] segments, // 위치·몸통 항목 전달
            float spacing // 대상 처리
        ) // 앞선 항목 종료
        {
            Bind(recorder, segments, null, spacing); // 연결·몸통 기능 실행
        }

        public void Bind( // 연결 호출 시작
            SnakePathRecorder recorder, // 경로 항목 전달
            Transform[] segments, // 위치·몸통 항목 전달
            Transform tail, // 위치 항목 전달
            float spacing // 대상 처리
        ) // 앞선 항목 종료
        {
            pathRecorder = recorder; // 경로 값 설정
            bodySegments = segments ?? new Transform[0]; // 몸통·위치 값 설정
            tailSegment = tail; // 몸통 값 설정
            segmentSpacing = Mathf.Max(0.05f, spacing); // 몸통·최대 값 설정
            SnapToPath(); // 경로 기능 실행
        }

        public void SetSegments(Transform[] segments, Transform tail) // 설정·몸통 기능 선언
        {
            bodySegments = segments ?? new Transform[0]; // 몸통·위치 값 설정
            tailSegment = tail; // 몸통 값 설정
            SnapToPath(); // 경로 기능 실행
        }

        public void SnapToPath() // 경로 기능 선언
        {
            FollowPath(); // 경로 기능 실행
        }

        public bool ApplyBossPush(SnakeSegment segment, Vector3 offset, float recoveryDuration) // 실제 몸통의 임시 밀림 적용
        {
            if (!isActiveAndEnabled || segment == null || !segment.IsBody || bodySegments == null || recoveryDuration <= 0f) // 활성 추적기와 몸통·복귀 시간 확인
            {
                return false; // 유효하지 않은 밀림 거부
            }
            bool owned = false; // 추적 중인 몸통 여부
            foreach (Transform item in bodySegments) // 실제 추적 몸통 순회
            {
                if (item == segment.transform) // 실제 몸통 일치 확인
                {
                    owned = true; // 추적 몸통 기록
                    break; // 탐색 종료
                }
            }
            if (!owned) // 다른 몸통 확인
            {
                return false; // 비소유 몸통 밀림 거부
            }
            if (!bossPushes.TryGetValue(segment.transform, out BossPushState state)) // 기존 밀림 상태 확인
            {
                state = new BossPushState(); // 새 몸통 밀림 상태
                bossPushes.Add(segment.transform, state); // 실제 몸통 상태 등록
            }
            state.Offset = offset; // 거리 누적 없는 현재 밀림 저장
            state.Remaining = recoveryDuration; // 실제 경로 복귀 시간 갱신
            return true; // 실제 밀림 적용 성공
        }

        private void AdvanceBossPushes(float deltaTime) // 몸통별 밀림 복귀 진행
        {
            if (bossPushes.Count == 0 || deltaTime <= 0f) // 활성 밀림과 시간 확인
            {
                return; // 복귀 갱신 생략
            }
            expiredBossPushes.Clear(); // 이전 만료 몸통 목록 초기화
            foreach (KeyValuePair<Transform, BossPushState> entry in bossPushes) // 실제 밀림 상태 순회
            {
                BossPushState state = entry.Value; // 현재 몸통 밀림 상태
                float next = Mathf.Max(0f, state.Remaining - deltaTime); // 실제 남은 복귀 시간
                if (entry.Key == null || next <= 0f || state.Remaining <= 0f) // 제거된 몸통과 복귀 완료 확인
                {
                    expiredBossPushes.Add(entry.Key); // 만료 몸통 기록
                    continue; // 다음 상태 확인
                }
                state.Offset *= next / state.Remaining; // 남은 시간에 따른 선형 복귀
                state.Remaining = next; // 실제 남은 복귀 시간 저장
            }
            foreach (Transform item in expiredBossPushes) // 만료 몸통 순회
            {
                bossPushes.Remove(item); // 실제 만료 밀림 제거
            }
        }

        private void FollowPath() // 경로 기능 선언
        {
            if (pathRecorder == null || !pathRecorder.IsReady) // 경로 조건 확인
            {
                return; // 현재 기능 종료
            }

            int bodyCount = bodySegments == null ? 0 : bodySegments.Length; // 몸통·개수 처리

            for (int index = 0; index < bodyCount; index++) // 순서·몸통 반복 처리
            {
                Transform segment = bodySegments[index]; // 위치·몸통 값 설정

                if (segment == null) // 몸통 조건 확인
                {
                    continue; // 다음 반복 진행
                }

                float distanceBehindHead = segmentSpacing * (index + 1); // 거리·몸통 값 설정
                ApplyPathPose(segment, distanceBehindHead); // 적용·경로 기능 실행
            }

            if (tailSegment != null) // 몸통 조건 확인
            {
                float tailDistance = segmentSpacing * (bodyCount + 1); // 거리·몸통 값 설정
                ApplyPathPose(tailSegment, tailDistance); // 적용·경로 기능 실행
            }
        }

        private void ApplyPathPose(Transform segment, float distanceBehindHead) // 적용·경로 기능 선언
        {
            Vector3 targetPosition = pathRecorder.GetPositionAtDistance(distanceBehindHead); // 벡터·대상 값 설정
            if (bossPushes.TryGetValue(segment, out BossPushState state)) // 보스가 밀어낸 몸통 확인
            {
                targetPosition += state.Offset; // 실제 경로에 임시 밀림 반영
            }
            Vector3 forward = pathRecorder.GetForwardAtDistance(distanceBehindHead); // 벡터·경로 값 설정

            segment.position = targetPosition; // 몸통·위치 값 설정

            if (forward.sqrMagnitude <= 0.0001f) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f; // 각도 값 설정
            segment.rotation = Quaternion.Euler(0f, 0f, angle); // 몸통·회전 값 설정
        }
    }
}
