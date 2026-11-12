using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakeContactHazard : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private SnakeContactHazardType hazardType; // 필드·형식 설정
        [SerializeField] private bool blocksProjectiles; // 직선 투사체 엄폐 여부

        public SnakeContactHazardType HazardType => hazardType; // 형식 값 반환
        public bool BlocksProjectiles => blocksProjectiles; // 투사체 엄폐 여부 반환

        public void SetProjectileCover(bool active) // 장애물의 투사체 엄폐 설정
        {
            blocksProjectiles = active; // 실제 엄폐 여부 저장
        }

        public void Configure(SnakeContactHazardType type) // 구성·형식 기능 선언
        {
            hazardType = type; // 형식 값 설정
        }
    }
}
