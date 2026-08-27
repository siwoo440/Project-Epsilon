using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(200)]
    public sealed class SnakeBodyFollower : MonoBehaviour
    {
        [SerializeField] private SnakePathRecorder pathRecorder;
        [SerializeField] private Transform[] bodySegments;
        [SerializeField] private float segmentSpacing = 0.58f;

        public int SegmentCount => bodySegments == null ? 0 : bodySegments.Length;

        private void LateUpdate()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            FollowPath();
        }

        public void Bind(
            SnakePathRecorder recorder,
            Transform[] segments,
            float spacing
        )
        {
            pathRecorder = recorder;
            bodySegments = segments;
            segmentSpacing = Mathf.Max(0.05f, spacing);
            SnapToPath();
        }

        public void SnapToPath()
        {
            FollowPath();
        }

        private void FollowPath()
        {
            if (pathRecorder == null || !pathRecorder.IsReady || bodySegments == null)
            {
                return;
            }

            for (int index = 0; index < bodySegments.Length; index++)
            {
                Transform segment = bodySegments[index];

                if (segment == null)
                {
                    continue;
                }

                float distanceBehindHead = segmentSpacing * (index + 1);
                Vector3 targetPosition = pathRecorder.GetPositionAtDistance(distanceBehindHead);
                Vector3 forward = pathRecorder.GetForwardAtDistance(distanceBehindHead);

                segment.position = targetPosition;

                if (forward.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;
                    segment.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
        }
    }
}
