using UnityEngine; // Unity 음원 데이터
namespace ProjectEpsilon.Audio // 게임 소리 영역
{ // 영역 시작
    [CreateAssetMenu(menuName = "Project Epsilon/Background Music Settings")] // 배경음 데이터 생성
    public sealed class BackgroundMusicSettings : ScriptableObject // 교체 가능한 배경음 설정
    { // 데이터 시작
        [SerializeField] private AudioClip menu; // 메뉴 임시 배경음
        [SerializeField] private AudioClip plain; // 평원 임시 배경음
        [SerializeField] private AudioClip ruins; // 폐허 임시 배경음
        [SerializeField] private AudioClip swamp; // 습지 임시 배경음
        [SerializeField] private AudioClip boss; // 보스 임시 배경음
        [SerializeField, Range(0f, 1f)] private float volume = 0.3f; // 배경음 개별 음량
        public AudioClip Menu => menu; // 메뉴 배경음 반환
        public AudioClip Boss => boss; // 보스 배경음 반환
        public float Volume => Mathf.Clamp01(volume); // 안전한 배경음 음량
        public AudioClip Map(int index) // 실제 맵별 음원 조회
        { // 함수 시작
            if (index == 1) // 폐허 맵 확인
            { // 조건 시작
                return ruins; // 폐허 음원 반환
            } // 조건 종료
            if (index == 2) // 습지 맵 확인
            { // 조건 시작
                return swamp; // 습지 음원 반환
            } // 조건 종료
            return plain; // 기본 평원 음원 반환
        } // 함수 종료
    } // 데이터 종료
} // 영역 종료
