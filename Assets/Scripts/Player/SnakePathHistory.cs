using System.Collections.Generic;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakePathHistory
    {
        private readonly List<Vector3> points = new List<Vector3>();
        private readonly float minimumPointDistance;
        private readonly float maximumPathLength;

        private Vector3 lastCommittedPosition;

        public int PointCount => points.Count;

        public SnakePathHistory(float minimumPointDistance, float maximumPathLength)
        {
            this.minimumPointDistance = Mathf.Max(0.001f, minimumPointDistance);
            this.maximumPathLength = Mathf.Max(this.minimumPointDistance, maximumPathLength);
        }

        public void Reset(Vector3 headPosition, Vector3 forward, float initialLength)
        {
            points.Clear();

            Vector3 flatForward = new Vector3(forward.x, forward.y, 0f);

            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = Vector3.up;
            }

            flatForward.Normalize();
            lastCommittedPosition = headPosition;
            points.Add(headPosition);

            float requiredLength = Mathf.Max(initialLength, maximumPathLength);

            for (
                float distance = minimumPointDistance;
                distance <= requiredLength + minimumPointDistance;
                distance += minimumPointDistance
            )
            {
                points.Add(headPosition - flatForward * distance);
            }
        }

        public void Record(Vector3 headPosition)
        {
            if (points.Count == 0)
            {
                Reset(headPosition, Vector3.up, maximumPathLength);
                return;
            }

            points[0] = headPosition;

            if (Vector3.Distance(lastCommittedPosition, headPosition) < minimumPointDistance)
            {
                return;
            }

            points.Insert(1, lastCommittedPosition);
            lastCommittedPosition = headPosition;
            TrimToMaximumLength();
        }

        public Vector3 SampleDistanceBehind(float distance)
        {
            if (points.Count == 0)
            {
                return Vector3.zero;
            }

            if (distance <= 0f)
            {
                return points[0];
            }

            float walkedDistance = 0f;

            for (int index = 0; index < points.Count - 1; index++)
            {
                Vector3 from = points[index];
                Vector3 to = points[index + 1];
                float segmentLength = Vector3.Distance(from, to);

                if (segmentLength <= 0.0001f)
                {
                    continue;
                }

                if (walkedDistance + segmentLength >= distance)
                {
                    float remainingDistance = distance - walkedDistance;
                    float interpolation = remainingDistance / segmentLength;
                    return Vector3.Lerp(from, to, interpolation);
                }

                walkedDistance += segmentLength;
            }

            return points[points.Count - 1];
        }

        private void TrimToMaximumLength()
        {
            if (points.Count < 2)
            {
                return;
            }

            float walkedDistance = 0f;

            for (int index = 0; index < points.Count - 1; index++)
            {
                Vector3 from = points[index];
                Vector3 to = points[index + 1];
                float segmentLength = Vector3.Distance(from, to);

                if (segmentLength <= 0.0001f)
                {
                    continue;
                }

                if (walkedDistance + segmentLength < maximumPathLength)
                {
                    walkedDistance += segmentLength;
                    continue;
                }

                float remainingDistance = maximumPathLength - walkedDistance;
                float interpolation = Mathf.Clamp01(remainingDistance / segmentLength);
                Vector3 endpoint = Vector3.Lerp(from, to, interpolation);

                points[index + 1] = endpoint;

                int removeStart = index + 2;

                if (removeStart < points.Count)
                {
                    points.RemoveRange(removeStart, points.Count - removeStart);
                }

                return;
            }
        }
    }
}
