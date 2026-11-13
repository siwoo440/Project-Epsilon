using System; // 직렬화 형식
using UnityEngine; // Unity 음원 데이터

namespace ProjectEpsilon.Audio // 게임 소리 영역
{ // 영역 시작
    [Serializable] // 효과음 설정 직렬화
    public sealed class GameplaySoundCue // 개별 효과음 설정
    { // 설정 시작
        [SerializeField] private AudioClip clip; // 실제 효과음 파일
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f; // 개별 음량
        [SerializeField, Min(0f)] private float minimumInterval = 0.1f; // 최소 재생 간격
        public AudioClip Clip => clip; // 실제 효과음 반환
        public float Volume => Mathf.Clamp01(volume); // 안전한 개별 음량
        public float MinimumInterval => Mathf.Max(0f, minimumInterval); // 안전한 반복 간격
    } // 설정 종료
    [CreateAssetMenu(menuName = "Project Epsilon/Gameplay Sound Settings")] // 음원 데이터 생성
    public sealed class GameplaySoundSettings : ScriptableObject // 전투 효과음 데이터
    { // 데이터 시작
        [SerializeField] private GameplaySoundCue attack = new GameplaySoundCue(); // 포탑 공격음
        [SerializeField] private GameplaySoundCue hit = new GameplaySoundCue(); // 플레이어 피격음
        [SerializeField] private GameplaySoundCue bodyLoss = new GameplaySoundCue(); // 몸통 손실음
        [SerializeField] private GameplaySoundCue levelUp = new GameplaySoundCue(); // 레벨업 알림음
        [SerializeField] private GameplaySoundCue merge = new GameplaySoundCue(); // 포탑 합성음
        [SerializeField] private GameplaySoundCue elite = new GameplaySoundCue(); // 엘리트 출현음
        [SerializeField] private GameplaySoundCue boss = new GameplaySoundCue(); // 보스 전환음
        public GameplaySoundCue Attack => attack; // 공격음 설정 반환
        public GameplaySoundCue Hit => hit; // 피격음 설정 반환
        public GameplaySoundCue BodyLoss => bodyLoss; // 몸통 손실음 설정 반환
        public GameplaySoundCue LevelUp => levelUp; // 레벨업음 설정 반환
        public GameplaySoundCue Merge => merge; // 합성음 설정 반환
        public GameplaySoundCue Elite => elite; // 엘리트 알림 설정 반환
        public GameplaySoundCue Boss => boss; // 보스 알림 설정 반환
    } // 데이터 종료
} // 영역 종료
