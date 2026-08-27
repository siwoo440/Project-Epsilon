using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(100)]
    public sealed class SnakePathRecorder : MonoBehaviour
    {
        [SerializeField] private float minimumPointDistance = 0.04f;
        [SerializeField] private float maximumPathLength = 18f;
        [SerializeField] private float initialPathLength = 14f;

        private SnakePathHistory pathHistory;

        public bool IsReady => pathHistory != null && pathHistory.PointCount > 1;

        private void Awake()
        {
            ResetHistory();
        }

        private void OnEnable()
        {
            if (pathHistory == null)
            {
                ResetHistory();
            }
        }

        private void LateUpdate()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            pathHistory?.Record(transform.position);
        }

        public void ResetHistory()
        {
            pathHistory = new SnakePathHistory(minimumPointDistance, maximumPathLength);
            pathHistory.Reset(transform.position, transform.up, initialPathLength);
        }

        public Vector3 GetPositionAtDistance(float distanceBehindHead)
        {
            if (pathHistory == null)
            {
                return transform.position;
            }

            return pathHistory.SampleDistanceBehind(Mathf.Max(0f, distanceBehindHead));
        }

        public Vector3 GetForwardAtDistance(float distanceBehindHead)
        {
            Vector3 ahead = GetPositionAtDistance(Mathf.Max(0f, distanceBehindHead - 0.08f));
            Vector3 behind = GetPositionAtDistance(distanceBehindHead + 0.08f);
            Vector3 direction = ahead - behind;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return transform.up;
            }

            return direction.normalized;
        }
    }
}
