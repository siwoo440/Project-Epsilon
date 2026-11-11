using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakeContactHazard : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private SnakeContactHazardType hazardType; // 필드·형식 설정

        public SnakeContactHazardType HazardType => hazardType; // 형식 값 반환

        public void Configure(SnakeContactHazardType type) // 구성·형식 기능 선언
        {
            hazardType = type; // 형식 값 설정
        }
    }
}
