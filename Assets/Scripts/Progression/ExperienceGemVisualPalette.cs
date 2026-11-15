using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Progression // 성장 영역
{ // 이름 공간 시작
    public sealed class ExperienceGemVisualPalette : MonoBehaviour // 경험치 보석 이미지 설정
    { // 형식 시작
        [SerializeField] private Sprite small; // 소형 보석 이미지
        [SerializeField] private Sprite medium; // 중형 보석 이미지
        [SerializeField] private Sprite large; // 대형 보석 이미지

        public Sprite Resolve(int value) // 실제 경험치별 이미지 조회
        { // 함수 시작
            if (value >= 20) // 대형 보석 기준 확인
            { // 조건 시작
                return large; // 대형 이미지 반환
            } // 조건 종료
            if (value >= 5) // 중형 보석 기준 확인
            { // 조건 시작
                return medium; // 중형 이미지 반환
            } // 조건 종료
            return small; // 소형 이미지 반환
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
