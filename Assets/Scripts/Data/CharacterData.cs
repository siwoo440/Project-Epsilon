using ProjectEpsilon.Progression; // 시작 무기 후보 데이터
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Data // 프로젝트 기능 영역
{
    public enum CharacterPassive // 캐릭터별 상시 패시브
    {
        BalancedRecovery, // 균형형 회복
        MobileRecovery, // 기동형 회복
        HeavyDefense, // 중장형 피해 경감
        FusionCandidates, // 합성형 후보 가중치
        HolySpecialization, // 신성 공격과 회복 강화
        DarkSpecialization // 암흑 처치 회복 강화
    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Project Epsilon/Data/Character")] // 생성·파일 설정
    public sealed class CharacterData : ScriptableObject // 캐릭터·데이터 자료 구조
    {
        [Header("Identity")] // 대상 설정
        [SerializeField] private string id = "character_id"; // 필드·식별자 설정
        [SerializeField] private string displayName = "Character"; // 필드·이름 설정

        [Header("Base Stats")] // 대상 설정
        [Min(1)] [SerializeField] private int startingBodyCount = 3; // 최소·필드 설정
        [Min(1f)] [SerializeField] private float bodyMaxHealth = 100f; // 최소·필드 설정
        [Min(0.1f)] [SerializeField] private float moveSpeedMultiplier = 1f; // 최소·필드 설정
        [Min(0.1f)] [SerializeField] private float turnPowerMultiplier = 1f; // 최소·필드 설정
        [Header("Provisional Passive and Starting Weapons")] // 임시 패시브와 시작 후보
        [SerializeField] private CharacterPassive passive = CharacterPassive.BalancedRecovery; // 상시 패시브 종류
        [Min(0f)] [SerializeField] private float passiveMultiplier = 1f; // 주 효과 임시 배율
        [Min(0f)] [SerializeField] private float secondaryMultiplier = 1f; // 보조 효과 임시 배율
        [Min(1f)] [SerializeField] private float preferredCandidateWeight = 1f; // 특화 후보 임시 가중치
        [SerializeField] private StartingWeaponOptions startingWeapons; // 캐릭터별 시작 무기 후보

        public string Id => id; // 식별자 값 반환
        public string DisplayName => displayName; // 이름 값 반환
        public int StartingBodyCount => startingBodyCount; // 몸통·개수 값 반환
        public float BodyMaxHealth => bodyMaxHealth; // 몸통·최대 값 반환
        public float MoveSpeedMultiplier => moveSpeedMultiplier; // 속도·배율 값 반환
        public float TurnPowerMultiplier => turnPowerMultiplier; // 배율 값 반환
        public CharacterPassive Passive => passive; // 상시 패시브 종류
        public float PassiveMultiplier => Mathf.Max(0f, passiveMultiplier); // 안전한 주 효과 배율
        public float SecondaryMultiplier => Mathf.Max(0f, secondaryMultiplier); // 안전한 보조 효과 배율
        public float PreferredCandidateWeight => Mathf.Max(1f, preferredCandidateWeight); // 안전한 후보 가중치
        public StartingWeaponOptions StartingWeapons => startingWeapons; // 캐릭터별 시작 무기 후보
    }
}
