using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Progression // 성장 영역
{
    public sealed class ExperienceGemVisualPalette : MonoBehaviour // 경험치 보석 이미지 설정
    {
        [SerializeField] private Sprite small; // 소형 보석 이미지
        [SerializeField] private Sprite medium; // 중형 보석 이미지
        [SerializeField] private Sprite large; // 대형 보석 이미지

        public Sprite Resolve(int value) // 실제 경험치별 이미지 조회
        {
            if (value >= 20) // 대형 보석 기준 확인
            {
                return large; // 대형 이미지 반환
            }
            if (value >= 5) // 중형 보석 기준 확인
            {
                return medium; // 중형 이미지 반환
            }
            return small; // 소형 이미지 반환
        }
    }
}
