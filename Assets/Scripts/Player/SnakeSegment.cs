using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeSegment : MonoBehaviour
    {
        [SerializeField] private SnakeSegmentType segmentType = SnakeSegmentType.Body;
        [SerializeField] private int bodyIndex = -1;

        public SnakeSegmentType SegmentType => segmentType;
        public int BodyIndex => bodyIndex;
        public bool IsBody => segmentType == SnakeSegmentType.Body;
        public bool IsTail => segmentType == SnakeSegmentType.Tail;

        public void Configure(SnakeSegmentType type, int index)
        {
            segmentType = type;
            bodyIndex = type == SnakeSegmentType.Body ? Mathf.Max(0, index) : -1;
        }
    }
}
