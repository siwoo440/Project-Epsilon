using UnityEngine; // Unity 데이터 기능

namespace ProjectEpsilon.World // 맵 영역
{ // 영역 시작
    [CreateAssetMenu(fileName = "PrototypeMapProfile", menuName = "Project Epsilon/Prototype Map")] // 맵 데이터 생성 메뉴
    public sealed class PrototypeMapProfile : ScriptableObject // 맵별 시험 데이터
    { // 클래스 시작
        [SerializeField] private string displayName = "OPEN PLAINS"; // 시작 화면 맵 이름
        [SerializeField] private Sprite ground; // 맵 바닥 이미지
        [SerializeField] private Color bossTint = Color.white; // 임시 보스 구분 색상
        [Min(1f)] [SerializeField] private float bossHealth = 600f; // 보스 체력 임시값
        [Min(0f)] [SerializeField] private float bossSpeedRatio = 0.75f; // 보스 이동 비율 임시값
        [Min(0.1f)] [SerializeField] private float bossAttackInterval = 2f; // 보스 사격 간격 임시값
        [Range(1, 9)] [SerializeField] private int bossProjectileCount = 3; // 첫 단계 발사 수 임시값
        [Range(1, 9)] [SerializeField] private int bossPhaseTwoProjectileCount = 5; // 두 번째 단계 발사 수 임시값
        [Min(0f)] [SerializeField] private float obstacleChance; // 청크 장애물 확률 임시값
        [Range(0f, 1f)] [SerializeField] private float specialZoneChance; // 청크 특수 지역 확률 임시값
        [Range(0, 2)] [SerializeField] private int obstaclePattern; // 사전 배치 패턴 종류
        [Min(0.5f)] [SerializeField] private float zoneRadius = 2.5f; // 특수 지역 반경 임시값
        [Range(0.1f, 1f)] [SerializeField] private float zoneSlowMultiplier = 0.7f; // 이동 감속 임시 배율
        [Min(1f)] [SerializeField] private float zoneExperienceMultiplier = 1.5f; // 경험치 임시 배율
        [Min(0.1f)] [SerializeField] private float zoneTickSeconds = 2f; // 피해와 회복 간격 임시값
        [Min(0)] [SerializeField] private int zoneDamage = 5; // 위험 지역 피해 임시값
        [Min(0)] [SerializeField] private int zoneHealing = 2; // 회복 지역 회복량 임시값
        public string DisplayName => displayName; // 맵 표시 이름
        public Sprite Ground => ground; // 실제 바닥 이미지
        public Color BossTint => bossTint; // 실제 임시 보스 색상
        public float BossHealth => Mathf.Max(1f, bossHealth); // 안전한 보스 체력
        public float BossSpeedRatio => Mathf.Max(0f, bossSpeedRatio); // 안전한 이동 비율
        public float BossAttackInterval => Mathf.Max(0.1f, bossAttackInterval); // 안전한 사격 간격
        public int BossProjectileCount => Mathf.Clamp(bossProjectileCount, 1, 9); // 안전한 첫 단계 발사 수
        public int BossPhaseTwoProjectileCount => Mathf.Clamp(bossPhaseTwoProjectileCount, 1, 9); // 안전한 두 번째 발사 수
        public float ObstacleChance => Mathf.Clamp01(obstacleChance); // 안전한 장애물 확률
        public float SpecialZoneChance => Mathf.Clamp01(specialZoneChance); // 안전한 특수 지역 확률
        public int ObstaclePattern => Mathf.Clamp(obstaclePattern, 0, 2); // 안전한 패턴 번호
        public float ZoneRadius => Mathf.Max(0.5f, zoneRadius); // 안전한 지역 반경
        public float ZoneSlowMultiplier => Mathf.Clamp(zoneSlowMultiplier, 0.1f, 1f); // 안전한 감속 배율
        public float ZoneExperienceMultiplier => Mathf.Max(1f, zoneExperienceMultiplier); // 안전한 경험치 배율
        public float ZoneTickSeconds => Mathf.Max(0.1f, zoneTickSeconds); // 안전한 효과 간격
        public int ZoneDamage => Mathf.Max(0, zoneDamage); // 안전한 지역 피해
        public int ZoneHealing => Mathf.Max(0, zoneHealing); // 안전한 지역 회복
    } // 클래스 끝
} // 영역 끝
