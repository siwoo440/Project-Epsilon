using System.Collections.Generic; // 밀린 몸통 상태 목록
using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(200)]
    public sealed class SnakeBodyFollower : MonoBehaviour
    {
        [SerializeField] private SnakePathRecorder pathRecorder;
        [SerializeField] private Transform[] bodySegments = new Transform[0];
        [SerializeField] private Transform tailSegment;
        [SerializeField] private float segmentSpacing = 0.58f;
        private readonly Dictionary<Transform, BossPushState> bossPushes = new Dictionary<Transform, BossPushState>(); // 몸통별 보스 밀림
        private readonly List<Transform> expiredBossPushes = new List<Transform>(); // 재사용할 만료 몸통 목록

        private sealed class BossPushState // 몸통의 임시 밀림 상태
        { // 클래스 시작
            public Vector3 Offset; // 경로 기준 밀림 거리
            public float Remaining; // 경로 복귀 남은 시간
        } // 클래스 끝

        public int SegmentCount => bodySegments == null ? 0 : bodySegments.Length;
        public float SegmentSpacing => segmentSpacing;

        private void LateUpdate()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            AdvanceBossPushes(Time.deltaTime); // 밀린 몸통의 경로 복귀 갱신
            FollowPath();
        }

        // Day04 Setup과의 호환성을 유지하기 위한 기존 Bind 오버로드
        public void Bind(
            SnakePathRecorder recorder,
            Transform[] segments,
            float spacing
        )
        {
            Bind(recorder, segments, null, spacing);
        }

        public void Bind(
            SnakePathRecorder recorder,
            Transform[] segments,
            Transform tail,
            float spacing
        )
        {
            pathRecorder = recorder;
            bodySegments = segments ?? new Transform[0];
            tailSegment = tail;
            segmentSpacing = Mathf.Max(0.05f, spacing);
            SnapToPath();
        }

        public void SetSegments(Transform[] segments, Transform tail)
        {
            bodySegments = segments ?? new Transform[0];
            tailSegment = tail;
            SnapToPath();
        }

        public void SnapToPath()
        {
            FollowPath();
        }

        public bool ApplyBossPush(SnakeSegment segment, Vector3 offset, float recoveryDuration) // 실제 몸통의 임시 밀림 적용
        { // 메서드 시작
            if (!isActiveAndEnabled || segment == null || !segment.IsBody || bodySegments == null || recoveryDuration <= 0f) // 활성 추적기와 몸통·복귀 시간 확인
            { // 조건 시작
                return false; // 유효하지 않은 밀림 거부
            } // 조건 끝
            bool owned = false; // 추적 중인 몸통 여부
            foreach (Transform item in bodySegments) // 실제 추적 몸통 순회
            { // 반복 시작
                if (item == segment.transform) // 실제 몸통 일치 확인
                { // 조건 시작
                    owned = true; // 추적 몸통 기록
                    break; // 탐색 종료
                } // 조건 끝
            } // 반복 끝
            if (!owned) // 다른 몸통 확인
            { // 조건 시작
                return false; // 비소유 몸통 밀림 거부
            } // 조건 끝
            if (!bossPushes.TryGetValue(segment.transform, out BossPushState state)) // 기존 밀림 상태 확인
            { // 조건 시작
                state = new BossPushState(); // 새 몸통 밀림 상태
                bossPushes.Add(segment.transform, state); // 실제 몸통 상태 등록
            } // 조건 끝
            state.Offset = offset; // 거리 누적 없는 현재 밀림 저장
            state.Remaining = recoveryDuration; // 실제 경로 복귀 시간 갱신
            return true; // 실제 밀림 적용 성공
        } // 메서드 끝

        private void AdvanceBossPushes(float deltaTime) // 몸통별 밀림 복귀 진행
        { // 메서드 시작
            if (bossPushes.Count == 0 || deltaTime <= 0f) // 활성 밀림과 시간 확인
            { // 조건 시작
                return; // 복귀 갱신 생략
            } // 조건 끝
            expiredBossPushes.Clear(); // 이전 만료 몸통 목록 초기화
            foreach (KeyValuePair<Transform, BossPushState> entry in bossPushes) // 실제 밀림 상태 순회
            { // 반복 시작
                BossPushState state = entry.Value; // 현재 몸통 밀림 상태
                float next = Mathf.Max(0f, state.Remaining - deltaTime); // 실제 남은 복귀 시간
                if (entry.Key == null || next <= 0f || state.Remaining <= 0f) // 제거된 몸통과 복귀 완료 확인
                { // 조건 시작
                    expiredBossPushes.Add(entry.Key); // 만료 몸통 기록
                    continue; // 다음 상태 확인
                } // 조건 끝
                state.Offset *= next / state.Remaining; // 남은 시간에 따른 선형 복귀
                state.Remaining = next; // 실제 남은 복귀 시간 저장
            } // 반복 끝
            foreach (Transform item in expiredBossPushes) // 만료 몸통 순회
            { // 반복 시작
                bossPushes.Remove(item); // 실제 만료 밀림 제거
            } // 반복 끝
        } // 메서드 끝

        private void FollowPath()
        {
            if (pathRecorder == null || !pathRecorder.IsReady)
            {
                return;
            }

            int bodyCount = bodySegments == null ? 0 : bodySegments.Length;

            for (int index = 0; index < bodyCount; index++)
            {
                Transform segment = bodySegments[index];

                if (segment == null)
                {
                    continue;
                }

                float distanceBehindHead = segmentSpacing * (index + 1);
                ApplyPathPose(segment, distanceBehindHead);
            }

            if (tailSegment != null)
            {
                float tailDistance = segmentSpacing * (bodyCount + 1);
                ApplyPathPose(tailSegment, tailDistance);
            }
        }

        private void ApplyPathPose(Transform segment, float distanceBehindHead)
        {
            Vector3 targetPosition = pathRecorder.GetPositionAtDistance(distanceBehindHead);
            if (bossPushes.TryGetValue(segment, out BossPushState state)) // 보스가 밀어낸 몸통 확인
            { // 조건 시작
                targetPosition += state.Offset; // 실제 경로에 임시 밀림 반영
            } // 조건 끝
            Vector3 forward = pathRecorder.GetForwardAtDistance(distanceBehindHead);

            segment.position = targetPosition;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;
            segment.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
