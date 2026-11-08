using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeSegment : MonoBehaviour
    {
        [SerializeField] private SnakeSegmentType segmentType = SnakeSegmentType.Body;
        [SerializeField] private int bodyIndex = -1;
        private float bossPushGraceUntil; // 보스 밀림 자기 충돌 유예 종료

        public SnakeSegmentType SegmentType => segmentType;
        public int BodyIndex => bodyIndex;
        public bool IsBody => segmentType == SnakeSegmentType.Body;
        public bool IsTail => segmentType == SnakeSegmentType.Tail;
        public bool IsBossPushGraceActive => Time.time < bossPushGraceUntil; // 이 몸통의 보스 밀림 유예 여부

        public void BeginBossPushGrace(float duration) // 이 몸통의 보스 밀림 유예 시작
        { // 메서드 시작
            bossPushGraceUntil = Mathf.Max(bossPushGraceUntil, Time.time + Mathf.Max(0f, duration)); // 기존 유예를 보존한 종료 시각
        } // 메서드 끝

        public void Configure(SnakeSegmentType type, int index)
        {
            segmentType = type;
            bodyIndex = type == SnakeSegmentType.Body ? Mathf.Max(0, index) : -1;
        }
    }
}
