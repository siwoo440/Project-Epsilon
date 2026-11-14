using UnityEngine; // 유니티 설정 기능

namespace ProjectEpsilon.Progression // 진행 설정 영역
{ // 영역 시작
    [CreateAssetMenu(menuName = "Project Epsilon/Run Score Settings")] // 점수 설정 생성 메뉴
    public sealed class RunScoreSettings : ScriptableObject // 전투 점수 시험 기준
    { // 설정 시작
        [SerializeField, Min(0)] private int experienceTarget; // 경험치 만점 기준, 0은 미정
        [SerializeField, Min(0)] private int normalKillTarget; // 일반 처치 만점 기준, 0은 미정
        [SerializeField, Min(0)] private int fullRecoveryPickupScore = 100; // 가득 찬 회복 아이템 점수
        public int ExperienceTarget => Mathf.Max(0, experienceTarget); // 안전한 경험치 기준
        public int NormalKillTarget => Mathf.Max(0, normalKillTarget); // 안전한 일반 처치 기준
        public int FullRecoveryPickupScore => Mathf.Max(0, fullRecoveryPickupScore); // 안전한 아이템 점수
    } // 설정 종료
} // 영역 종료
