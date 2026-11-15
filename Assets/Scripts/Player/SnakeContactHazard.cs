using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{ // 이름 공간 시작
    public sealed class SnakeContactHazard : MonoBehaviour // 행동 자료 구조
    { // 형식 시작
        [SerializeField] private SnakeContactHazardType hazardType; // 필드·형식 설정
        [SerializeField] private bool blocksProjectiles; // 직선 투사체 엄폐 여부

        public SnakeContactHazardType HazardType => hazardType; // 형식 값 반환
        public bool BlocksProjectiles => blocksProjectiles; // 투사체 엄폐 여부 반환

        public void SetProjectileCover(bool active) // 장애물의 투사체 엄폐 설정
        { // 함수 시작
            blocksProjectiles = active; // 실제 엄폐 여부 저장
        } // 함수 종료

        public void Configure(SnakeContactHazardType type) // 구성·형식 기능 선언
        { // 함수 시작
            hazardType = type; // 형식 값 설정
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
