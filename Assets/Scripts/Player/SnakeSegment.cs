using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakeSegment : MonoBehaviour // 몸통·행동 자료 구조
    {
        [SerializeField] private SnakeSegmentType segmentType = SnakeSegmentType.Body; // 필드·몸통 설정
        [SerializeField] private int bodyIndex = -1; // 필드·몸통 설정
        private float bossPushGraceUntil; // 보스 밀림 자기 충돌 유예 종료

        public SnakeSegmentType SegmentType => segmentType; // 몸통·형식 값 반환
        public int BodyIndex => bodyIndex; // 몸통·순서 값 반환
        public bool IsBody => segmentType == SnakeSegmentType.Body; // 몸통·형식 값 반환
        public bool IsTail => segmentType == SnakeSegmentType.Tail; // 몸통·형식 값 반환
        public bool IsBossPushGraceActive => Time.time < bossPushGraceUntil; // 이 몸통의 보스 밀림 유예 여부

        public void BeginBossPushGrace(float duration) // 이 몸통의 보스 밀림 유예 시작
        {
            bossPushGraceUntil = Mathf.Max(bossPushGraceUntil, Time.time + Mathf.Max(0f, duration)); // 기존 유예를 보존한 종료 시각
        }

        public void Configure(SnakeSegmentType type, int index) // 구성·몸통 기능 선언
        {
            segmentType = type; // 몸통·형식 값 설정
            bodyIndex = type == SnakeSegmentType.Body ? Mathf.Max(0, index) : -1; // 몸통·순서 기능 실행
        }
    }
}
