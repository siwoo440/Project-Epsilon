using System; // 구간 데이터 직렬화
using UnityEngine; // Unity 데이터 에셋

namespace ProjectEpsilon.Enemies // 적 출현 영역
{ // 이름 공간 시작
    [Serializable] // 구간 데이터 직렬화 허용
    public sealed class EnemyWaveStage // 시간별 일반 적 출현 설정
    { // 형식 시작
        public float startTime; // 구간 시작 시간
        public float spawnInterval = 1.3f; // 일반 적 출현 간격
        public int normalEnemyLimit = 40; // 일반 적 최대 수
        public float healthMultiplier = 1f; // 일반 적 체력 배율
        public float damageMultiplier = 1f; // 일반 적 피해 배율
        [Tooltip("순서: 병정개미, 사마귀, 말벌, 엘리트(0), 쇠똥구리, 노린재, 잠자리, 개미귀신, 장수풍뎅이")] public float[] spawnWeights; // 적 행동 종류별 출현 가중치
        public float GetSpawnWeight(EarlyEnemyKind kind) // 현재 구간의 적별 가중치
        { // 함수 시작
            int index = (int)kind; // 적 행동 번호
            return spawnWeights == null || index < 0 || index >= spawnWeights.Length ? 1f : Mathf.Max(0f, spawnWeights[index]); // 미설정 호환 또는 안전한 가중치
        } // 함수 종료
    } // 형식 종료

    public readonly struct EnemyWavePacing
    {
        public readonly float SpawnInterval;
        public readonly int NormalEnemyLimit;
        public readonly float HealthMultiplier;
        public readonly float DamageMultiplier;

        internal EnemyWavePacing(EnemyWaveStage from, EnemyWaveStage to, float progress)
        {
            SpawnInterval = Mathf.Lerp(from.spawnInterval, to.spawnInterval, progress);
            NormalEnemyLimit = Mathf.RoundToInt(Mathf.Lerp(from.normalEnemyLimit, to.normalEnemyLimit, progress));
            HealthMultiplier = InterpolateMultiplier(from.healthMultiplier, to.healthMultiplier, progress);
            DamageMultiplier = InterpolateMultiplier(from.damageMultiplier, to.damageMultiplier, progress);
        }

        private static float InterpolateMultiplier(float from, float to, float progress)
        {
            // 두 기준 배율 사이에 같은 비율의 성장을 매분 적용한다.
            if (from > 0f && to > 0f)
            {
                return from * Mathf.Pow(to / from, progress);
            }
            return Mathf.Max(0f, Mathf.Lerp(from, to, progress));
        }
    }

    [CreateAssetMenu(menuName = "Project Epsilon/Enemy Wave Settings")] // 출현 데이터 생성 메뉴
    public sealed class EnemyWaveSettings : ScriptableObject // 조정 가능한 출현 구간 데이터
    { // 형식 시작
        [SerializeField] private EnemyWaveStage[] stages; // 시간별 출현 구간 목록
        [SerializeField] private EarlyEnemySettings[] additionalEnemies; // 추가 일반 적과 조정 가능한 시험값
        [SerializeField] private EarlyEnemySettings[] additionalEliteRounds; // 후속 회차별 전용 엘리트 시험값
        [SerializeField, Range(0f, 1f)] private float enhancedEliteChance = 0.25f; // 후속 회차 강화형 확률 시험값
        [SerializeField, Min(1f)] private float enhancedHealthMultiplier = 3f; // 강화형 기본 체력 배율 시험값
        [SerializeField, Min(1f)] private float enhancedDamageMultiplier = 1.5f; // 강화형 기본 피해 배율 시험값
        [SerializeField] private Sprite[] enhancedEnemySprites; // 일반 적 종류별 강화형 원본 이미지
        public EarlyEnemySettings[] AdditionalEnemies => additionalEnemies; // 실제 추가 적 목록
        public EarlyEnemySettings[] AdditionalEliteRounds => additionalEliteRounds; // 실제 후속 엘리트 목록
        public float EnhancedEliteChance => Mathf.Clamp01(enhancedEliteChance); // 안전한 강화형 출현 확률
        public float EnhancedHealthMultiplier => Mathf.Max(1f, enhancedHealthMultiplier); // 안전한 강화형 체력 배율
        public float EnhancedDamageMultiplier => Mathf.Max(1f, enhancedDamageMultiplier); // 안전한 강화형 피해 배율
        public Sprite GetEnhancedSprite(EarlyEnemyKind kind) // 실제 적 종류의 강화형 이미지 조회
        { // 함수 시작
            int index = (int)kind; // 실제 적 종류 번호
            return enhancedEnemySprites != null && index >= 0 && index < enhancedEnemySprites.Length ? enhancedEnemySprites[index] : null; // 유효 이미지 또는 기본 이미지
        } // 함수 종료

        public float ResolveMultiplier(float elapsed, bool health) // 분 단위 능력치 배율
        { // 함수 시작
            EnemyWavePacing? pacing = SampleMinute(Mathf.FloorToInt(elapsed / 60f));
            return health ? pacing?.HealthMultiplier ?? 1f : pacing?.DamageMultiplier ?? 1f;
        }

        public EnemyWavePacing? SampleMinute(int minute)
        {
            float time = Mathf.Max(0, minute) * 60f;
            EnemyWaveStage previous = Resolve(time);
            if (previous == null)
            {
                return null;
            }
            EnemyWaveStage next = null;
            foreach (EnemyWaveStage stage in stages)
            {
                if (stage != null && stage.startTime > time && (next == null || stage.startTime < next.startTime))
                {
                    next = stage;
                }
            }
            if (next == null)
            {
                return new EnemyWavePacing(previous, previous, 0f);
            }
            float progress = Mathf.InverseLerp(previous.startTime, next.startTime, time);
            return new EnemyWavePacing(previous, next, progress);
        }

        public EnemyWaveStage Resolve(float elapsed) // 현재 시간에 해당하는 출현 구간
        { // 함수 시작
            EnemyWaveStage selected = null; // 선택된 구간 초기화
            if (stages == null) // 구간 목록 부재 확인
            { // 조건 시작
                return null; // 기존 설정 사용
            } // 조건 종료
            foreach (EnemyWaveStage stage in stages) // 출현 구간 순회
            { // 반복 시작
                if (stage != null && stage.startTime <= elapsed && (selected == null || stage.startTime > selected.startTime)) // 현재 시간에 가장 가까운 구간 확인
                { // 조건 시작
                    selected = stage; // 현재 구간 저장
                } // 조건 종료
            } // 반복 종료
            return selected; // 현재 구간 반환
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
