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

        public int SegmentCount => bodySegments == null ? 0 : bodySegments.Length;
        public float SegmentSpacing => segmentSpacing;

        private void LateUpdate()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

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
