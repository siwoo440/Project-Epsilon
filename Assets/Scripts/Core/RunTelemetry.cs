using System; // 실제 시간과 예외
using System.Collections.Generic; // 실제 기록 목록
using System.IO; // 실제 로컬 파일 저장
using ProjectEpsilon.Combat; // 실제 포탑 상태
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Enemies; // 실제 전투 시간
using ProjectEpsilon.Player; // 실제 플레이어 상태
using ProjectEpsilon.Progression; // 실제 경험치와 진행
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEngine; // Unity 실행 환경

namespace ProjectEpsilon.Recording // 전투 기록 영역
{
    public enum DamageOrigin // 실제 피해 원인
    {
        Unknown, // 확인되지 않은 피해
        EnemyMelee, // 일반 적 근접 공격
        EnemyProjectile, // 일반 적 투사체
        BossProjectile, // 최종 보스 투사체
        BossCharge, // 최종 보스 돌진
        BossHazard, // 최종 보스 위험 구역
        SpecialZone, // 맵 위험 지역
        Obstacle, // 장애물 충돌
        SelfCollision, // 자기 몸통 충돌
        EnemyContact, // 적 직접 충돌
        EliteCut, // 엘리트 절단
        BossCut // 최종 보스 절단
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunWeaponSnapshot // 실제 몸통별 포탑 상태
    {
        public int slot; // 실제 몸통 슬롯
        public string weaponId; // 실제 포탑 식별자
        public int grade; // 실제 포탑 등급
        public int damageUpgrades; // 피해 강화 횟수
        public int speedUpgrades; // 속도 강화 횟수
        public int rangeUpgrades; // 범위 강화 횟수
        public string attribute; // 실제 포탑 속성
        public string attackType; // 실제 공격 유형
        public float baseDamage; // 실제 기본 피해
        public float attackInterval; // 실제 기본 공격 간격
        public float effectiveRange; // 실제 설정 범위
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunEnemyCondition // 실제 시험 적 시작 설정
    {
        public string kind; // 실제 적 행동 종류
        public float health; // 실제 적 체력
        public int damage; // 실제 적 피해
        public float speedRatio; // 실제 적 속도 배율
        public float attackInterval; // 실제 적 공격 간격
        public int experience; // 실제 처치 경험치
        public float firstAppearance; // 실제 첫 출현 시각
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunSnapshot // 실제 시점별 전투 상태
    {
        public string reason; // 실제 표본 수집 이유
        public double seconds; // 실제 전투 경과 시간
        public int frame; // 실제 Unity 프레임
        public int level; // 실제 레벨
        public int currentExperience; // 실제 남은 경험치
        public long totalExperience; // 실제 누적 획득 경험치
        public long droppedGemExperience; // 실제 생성 보석의 원본 경험치
        public long collectedGemExperience; // 실제 수집 보석의 원본 경험치
        public int kills; // 실제 일반 적·엘리트·소환 적·보스 처치
        public int rewardableKills; // 실제 코어 보상 대상 적 처치
        public int summonedKills; // 실제 보스 소환 적 처치
        public int bossKills; // 실제 최종 보스 처치
        public int elites; // 실제 엘리트 처치
        public int currentBodies; // 실제 현재 몸통
        public int earnedBodies; // 실제 성장 몸통 상한
        public int bodyLimit; // 실제 절대 몸통 상한
        public int health; // 실제 현재 공용 체력
        public int maximumHealth; // 실제 최대 공용 체력
        public List<RunWeaponSnapshot> weapons = new List<RunWeaponSnapshot>(); // 실제 슬롯 목록
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunDamageEvent // 실제 유효 피해 기록
    {
        public double seconds; // 실제 피해 시각
        public int frame; // 실제 피해 프레임
        public string origin; // 실제 피해 원인
        public int healthDamage; // 보호막 이후 실제 체력 피해량
        public int shieldDamage; // 실제 보호막 흡수 피해량
        public int bodyLoss; // 실제 직접 손실 예정 몸통
        public int healthBefore; // 실제 피해 전 체력
        public int bodiesBefore; // 실제 피해 전 몸통
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunInputEvent // 실제 이동 입력 변화
    {
        public double seconds; // 실제 입력 시각
        public int frame; // 실제 입력 프레임
        public float turn; // 실제 회전 입력
        public bool boost; // 실제 가속 입력
    }
    [Serializable] // JSON 저장 대상
    public sealed class RunRecording // 실제 한 판 전투 기록
    {
        public int version = 1; // 실제 기록 형식
        public int gemAccountingVersion = 1; // 보석 생성·수집 집계 형식
        public string runId; // 실제 기록 식별자
        public string startedUtc; // 실제 시작 UTC 시각
        public string unityVersion; // 실제 Unity 버전
        public string applicationVersion; // 실제 게임 버전
        public bool seedProvided; // 명시적 난수 시드 여부
        public int seed; // 실제 지정 난수 시드
        public float fixedDeltaTime; // 실제 고정 물리 간격
        public float randomProbe; // 난수 상태를 바꾸지 않은 시드 확인값
        public float spawnInterval; // 실제 일반 적 출현 간격
        public int enemyLimit; // 실제 동시 일반 적 상한
        public float bossTransitionTime; // 실제 보스 전환 시각
        public float playerMoveSpeed; // 실제 시작 이동 속도
        public float playerTurnSpeed; // 실제 시작 회전 속도
        public float maximumStamina; // 실제 시작 최대 스태미나
        public int experienceBaseRequirement; // 실제 기본 경험치 요구량
        public float experienceGrowthMultiplier; // 실제 경험치 증가율
        public float obstacleChance; // 실제 맵 장애물 확률
        public float specialZoneChance; // 실제 맵 특수 지역 확률
        public List<RunEnemyCondition> enemies = new List<RunEnemyCondition>(); // 실제 시험 적 설정
        public int targetFrameRate; // 실제 목표 프레임 설정
        public string characterId; // 실제 선택 캐릭터
        public int mapIndex; // 실제 선택 맵 번호
        public string mapName; // 실제 선택 맵 이름
        public int[] permanentLevels; // 실제 영구 성장 단계
        public string outcome; // 실제 종료 결과
        public string lastDamageOrigin; // 실제 마지막 피해 원인
        public List<RunSnapshot> snapshots = new List<RunSnapshot>(); // 실제 전투 표본
        public List<RunDamageEvent> damages = new List<RunDamageEvent>(); // 실제 유효 피해 목록
        public List<RunInputEvent> inputs = new List<RunInputEvent>(); // 실제 이동 입력 변화
    }
    public sealed class RunTelemetry : MonoBehaviour // 선택적 로컬 전투 기록기
    {
        private const float SampleInterval = 10f; // 실제 정기 표본 간격
        private EarlyEnemyWaveSpawner wave; // 실제 전투 시계
        private MetaProgressionController meta; // 실제 처치 집계
        private SnakeExperience experience; // 실제 경험치
        private SnakeBodyManager body; // 실제 몸통
        private SnakeHealth health; // 실제 공용 체력
        private SnakeWeaponManager weapons; // 실제 포탑 슬롯
        private PlayerInputReader input; // 실제 플레이어 입력
        private RunRecording recording; // 실제 현재 런 기록
        private double nextSample; // 다음 정기 표본 시각
        private long totalExperience; // 실제 누적 경험치
        private long droppedGemExperience; // 실제 생성 보석 경험치 합계
        private long collectedGemExperience; // 실제 수집 보석 원본 경험치 합계
        private int totalKills; // 실제 모든 적 처치
        private int summonedKills; // 실제 소환 적 처치
        private int bossKills; // 실제 최종 보스 처치
        private bool active; // 실제 기록 중 여부
        private int startFrame; // 실제 시작 프레임 기준
        private bool hasInput; // 이전 입력 기록 여부
        private float lastTurn; // 이전 회전 입력
        private bool lastBoost; // 이전 가속 입력
        public static RunTelemetry Instance { get; private set; } // 실제 기록기 참조
        public string LastSavedPath { get; private set; } // 실제 저장 완료 경로
        public string LastError { get; private set; } // 실제 기록 오류
        public RunRecording Current => recording; // 실제 기록 조회
        public static bool IsRequested() // 실제 실행 옵션 확인
        {
            foreach (string arg in Environment.GetCommandLineArgs()) // 실제 실행 인수 순회
            {
                if (arg == "-epsilonRecord") // 실제 전투 기록 옵션 확인
                {
                    return true; // 실제 기록 활성
                }
            }
            return false; // 기본 게임 기록 비활성
        }
        private void Awake() // 실제 기록기 등록
        {
            Instance = this; // 실제 현재 기록기 저장
        }
        public void BeginRun() // 실제 전투 기록 시작
        {
            if (active) // 기존 기록 진행 확인
            {
                return; // 중복 시작 차단
            }
            wave = GetComponent<EarlyEnemyWaveSpawner>(); // 실제 같은 관리자 시계
            meta = GetComponent<MetaProgressionController>(); // 실제 같은 관리자 집계
            experience = FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 조회
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            health = FindFirstObjectByType<SnakeHealth>(); // 실제 체력 조회
            weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 조회
            input = FindFirstObjectByType<PlayerInputReader>(); // 실제 이동 입력 조회
            int seed; // 실제 실행 시드
            bool hasSeed = TryGetSeed(out seed); // 실제 시드 지정 여부
            if (hasSeed) // 실제 재현 시드 확인
            {
                UnityEngine.Random.InitState(seed); // 실제 게임 난수 초기화
            }
            UnityEngine.Random.State randomState = UnityEngine.Random.state; // 실제 시작 난수 상태 보존
            float randomProbe = UnityEngine.Random.value; // 동일 시드 확인용 표본
            UnityEngine.Random.state = randomState; // 실제 게임 난수 순서 복원
            CharacterData character = CharacterRunInitializer.ActiveCharacter; // 실제 선택 캐릭터
            PrototypeMapSelection map = GetComponent<PrototypeMapSelection>(); // 실제 선택 맵
            PrototypeMapProfile profile = map == null ? null : map.SelectedProfile; // 실제 선택 맵 설정
            SnakeMovement movement = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeStamina stamina = FindFirstObjectByType<SnakeStamina>(); // 실제 스태미나 설정
            recording = new RunRecording // 실제 새 런 기록 생성
            {
                runId = Guid.NewGuid().ToString("N"), // 실제 중복 방지 식별자
                startedUtc = DateTime.UtcNow.ToString("O"), // 실제 UTC 시작 시간
                unityVersion = Application.unityVersion, // 실제 Unity 버전
                applicationVersion = Application.version, // 실제 게임 버전
                seedProvided = hasSeed, // 실제 난수 시드 존재
                seed = seed, // 실제 지정 시드
                fixedDeltaTime = Time.fixedDeltaTime, // 실제 고정 물리 간격
                randomProbe = randomProbe, // 실제 시작 난수 확인값
                spawnInterval = wave == null ? 0f : wave.RecordingSpawnInterval, // 실제 적 출현 간격
                enemyLimit = wave == null ? 0 : wave.RecordingEnemyLimit, // 실제 적 출현 상한
                bossTransitionTime = wave == null ? 0f : wave.RecordingBossTime, // 실제 보스 전환 시각
                playerMoveSpeed = movement == null ? 0f : movement.MoveSpeed, // 실제 시작 이동 속도
                playerTurnSpeed = movement == null ? 0f : movement.TurnSpeed, // 실제 시작 회전 속도
                maximumStamina = stamina == null ? 0f : stamina.MaximumStamina, // 실제 시작 최대 스태미나
                experienceBaseRequirement = experience == null ? 0 : experience.BaseRequirement, // 실제 기본 경험치 요구량
                experienceGrowthMultiplier = experience == null ? 0f : experience.GrowthMultiplier, // 실제 경험치 증가율
                obstacleChance = profile == null ? 0f : profile.ObstacleChance, // 실제 맵 장애물 확률
                specialZoneChance = profile == null ? 0f : profile.SpecialZoneChance, // 실제 맵 특수 지역 확률
                targetFrameRate = Application.targetFrameRate, // 실제 목표 프레임
                characterId = character == null ? "" : character.Id, // 실제 캐릭터 식별자
                mapIndex = map == null ? -1 : map.SelectedIndex, // 실제 맵 번호
                mapName = map == null || map.SelectedProfile == null ? "" : map.SelectedProfile.DisplayName, // 실제 맵 이름
                permanentLevels = new int[12] // 실제 영구 성장 단계
            };
            for (int index = 0; index < recording.permanentLevels.Length; index++) // 실제 열두 성장 순회
            {
                recording.permanentLevels[index] = meta == null || meta.Service == null ? 0 : meta.Service.GetLevel((MetaNode)index); // 실제 시작 단계 기록
            }
            IReadOnlyList<EarlyEnemySettings> recordedEnemies = wave == null ? null : wave.RecordingEnemies; // 기존 적과 추가 적 설정
            if (recordedEnemies != null) // 실제 기록 대상 확인
            {
                foreach (EarlyEnemySettings enemy in recordedEnemies) // 실제 적 설정 순회
                {
                    if (enemy != null) // 실제 설정 존재 확인
                    {
                        recording.enemies.Add(new RunEnemyCondition { kind = enemy.kind.ToString(), health = enemy.health, damage = enemy.damage, speedRatio = enemy.speedRatio, attackInterval = enemy.attackInterval, experience = enemy.experience, firstAppearance = enemy.firstAppearance }); // 실제 적 조건 기록
                    }
                }
            }
            startFrame = Time.frameCount; // 실제 시작 프레임 기록
            totalExperience = 0L; // 실제 누적 경험치 초기화
            droppedGemExperience = 0L; // 실제 생성 보석 경험치 초기화
            collectedGemExperience = 0L; // 실제 수집 보석 경험치 초기화
            totalKills = 0; // 실제 모든 처치 초기화
            summonedKills = 0; // 실제 소환 적 처치 초기화
            bossKills = 0; // 실제 최종 보스 처치 초기화
            nextSample = SampleInterval; // 실제 첫 정기 표본 예약
            LastSavedPath = null; // 실제 이전 파일 경로 초기화
            LastError = null; // 실제 이전 오류 초기화
            hasInput = false; // 실제 이전 입력 초기화
            active = true; // 실제 기록 시작
            if (wave != null) // 실제 적 출현기 확인
            {
                wave.EnemyDied += OnEnemyDied; // 실제 모든 적 처치 연결
            }
            if (experience != null) // 실제 경험치 구성 확인
            {
                experience.ExperienceGained += OnExperience; // 실제 획득 이벤트 연결
            }
            if (weapons != null) // 실제 포탑 구성 확인
            {
                weapons.SlotsChanged += OnSlotsChanged; // 실제 포탑 변동 연결
            }
            Capture("start"); // 실제 시작 상태 기록
        }
        private static bool TryGetSeed(out int seed) // 실제 실행 시드 조회
        {
            string[] args = Environment.GetCommandLineArgs(); // 실제 실행 인수
            for (int index = 0; index + 1 < args.Length; index++) // 실제 인수 순회
            {
                if (args[index] == "-epsilonSeed" && int.TryParse(args[index + 1], out seed)) // 실제 정수 시드 확인
                {
                    return true; // 실제 시드 적용
                }
            }
            seed = 0; // 시드 미지정 기본값
            return false; // 실제 미지정 상태
        }
        private void Update() // 실제 시간과 입력 표본
        {
            if (!active || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 실제 전투 진행 확인
            {
                return; // 정지 중 기록 생략
            }
            double seconds = Seconds(); // 실제 전투 시계
            if (seconds >= nextSample) // 실제 정기 표본 확인
            {
                Capture("interval"); // 실제 현재 전투 표본
                nextSample = (Math.Floor(seconds / SampleInterval) + 1d) * SampleInterval; // 실제 다음 표본 시각
            }
            if (input != null) // 실제 이동 입력 확인
            {
                float turn = input.TurnInput; // 실제 회전 입력
                bool boost = input.BoostPressed; // 실제 가속 입력
                if (!hasInput || !Mathf.Approximately(turn, lastTurn) || boost != lastBoost) // 실제 입력 변화 확인
                {
                    recording.inputs.Add(new RunInputEvent { seconds = seconds, frame = Time.frameCount - startFrame, turn = turn, boost = boost }); // 실제 입력 변화 저장
                    lastTurn = turn; // 실제 이전 회전 저장
                    lastBoost = boost; // 실제 이전 가속 저장
                    hasInput = true; // 실제 입력 기록 완료
                }
            }
        }
        private void OnEnemyDied(bool elite, bool summoned) // 실제 일반·엘리트·소환 적 처치 집계
        {
            if (!active) // 실제 기록 중 확인
            {
                return; // 종료 후 집계 차단
            }
            totalKills = totalKills == int.MaxValue ? totalKills : totalKills + 1; // 실제 전체 적 처치 누적
            if (summoned && summonedKills < int.MaxValue) // 실제 소환 적 확인
            {
                summonedKills++; // 실제 소환 적 처치 누적
            }
        }
        public void RecordBossKill() // 실제 최종 보스 처치 집계
        {
            if (active && bossKills == 0) // 실제 기록 중 첫 보스 확인
            {
                bossKills = 1; // 실제 보스 처치 기록
                totalKills = totalKills == int.MaxValue ? totalKills : totalKills + 1; // 실제 전체 처치 누적
            }
        }
        private void OnExperience(int amount) // 실제 획득 경험치 집계
        {
            if (active && amount > 0) // 실제 기록 중 획득 확인
            {
                totalExperience = totalExperience > long.MaxValue - amount ? long.MaxValue : totalExperience + amount; // 실제 경험치 안전 누적
            }
        }
        public static void RecordGemSpawn(int amount) // 실제 생성 보석 원본 경험치 집계
        {
            RunTelemetry self = Instance; // 실제 선택적 기록기 조회
            if (self == null || !self.active || amount <= 0) // 실제 기록과 유효 수량 확인
            {
                return; // 비활성 또는 무효 집계 차단
            }
            self.droppedGemExperience = self.droppedGemExperience > long.MaxValue - amount ? long.MaxValue : self.droppedGemExperience + amount; // 실제 생성량 안전 누적
        }
        public static void RecordGemCollection(int amount) // 실제 수집 보석 원본 경험치 집계
        {
            RunTelemetry self = Instance; // 실제 선택적 기록기 조회
            if (self == null || !self.active || amount <= 0) // 실제 기록과 유효 수량 확인
            {
                return; // 비활성 또는 무효 집계 차단
            }
            self.collectedGemExperience = self.collectedGemExperience > long.MaxValue - amount ? long.MaxValue : self.collectedGemExperience + amount; // 실제 수집량 안전 누적
        }
        private void OnSlotsChanged() // 실제 포탑 구성 변경
        {
            Capture("loadout"); // 실제 포탑 변경 표본
        }
        private double Seconds() // 실제 전투 시간 조회
        {
            return wave == null ? 0d : wave.CurrentRunSeconds; // 실제 출현기 전투 시간
        }
        public static void RecordDamage(DamageOrigin origin, int healthDamage, int bodyLoss, int shieldDamage = 0) // 실제 유효 피해 기록
        {
            RunTelemetry self = Instance; // 실제 기록기 조회
            if (self == null || !self.active || self.recording == null) // 실제 기록 상태 확인
            {
                return; // 기록 비활성 시 동작 유지
            }
            self.recording.lastDamageOrigin = origin.ToString(); // 실제 마지막 피해 원인
            self.recording.damages.Add(new RunDamageEvent // 실제 피해 항목 생성
            {
                seconds = self.Seconds(), // 실제 피해 시각
                frame = Time.frameCount - self.startFrame, // 실제 피해 프레임
                origin = origin.ToString(), // 실제 피해 원인
                healthDamage = Mathf.Max(0, healthDamage), // 실제 체력 피해
                shieldDamage = Mathf.Max(0, shieldDamage), // 실제 보호막 흡수 피해
                bodyLoss = Mathf.Max(0, bodyLoss), // 실제 직접 몸통 손실
                healthBefore = self.health == null ? 0 : self.health.CurrentHealth, // 실제 피해 전 체력
                bodiesBefore = self.body == null ? 0 : self.body.CurrentBodyCount // 실제 피해 전 몸통
            });
        }
        public void Capture(string reason) // 실제 현재 상태 표본
        {
            if (!active || recording == null) // 실제 기록 상태 확인
            {
                return; // 비활성 표본 방지
            }
            RunSnapshot sample = new RunSnapshot // 실제 전투 표본 생성
            {
                reason = reason, // 실제 표본 수집 이유
                seconds = Seconds(), // 실제 전투 경과 시간
                frame = Time.frameCount - startFrame, // 실제 런 상대 프레임
                level = experience == null ? 0 : experience.CurrentLevel, // 실제 현재 레벨
                currentExperience = experience == null ? 0 : experience.CurrentExperience, // 실제 남은 경험치
                totalExperience = totalExperience, // 실제 누적 획득 경험치
                droppedGemExperience = droppedGemExperience, // 실제 생성 보석 원본 경험치
                collectedGemExperience = collectedGemExperience, // 실제 수집 보석 원본 경험치
                kills = totalKills, // 실제 모든 적 처치
                rewardableKills = meta == null ? 0 : meta.Kills, // 실제 보상 대상 처치
                summonedKills = summonedKills, // 실제 보스 소환 적 처치
                bossKills = bossKills, // 실제 최종 보스 처치
                elites = meta == null ? 0 : meta.Elites, // 실제 엘리트 처치
                currentBodies = body == null ? 0 : body.CurrentBodyCount, // 실제 현재 몸통 수
                earnedBodies = body == null ? 0 : body.EarnedBodyCount, // 실제 성장 최대 몸통
                bodyLimit = body == null ? 0 : body.MaximumBodyCount, // 실제 절대 몸통 상한
                health = health == null ? 0 : health.CurrentHealth, // 실제 현재 체력
                maximumHealth = health == null ? 0 : health.MaximumHealth // 실제 최대 체력
            };
            if (weapons != null) // 실제 포탑 관리자 확인
            {
                int slotCount = body == null ? weapons.Slots.Count : Mathf.Min(body.CurrentBodyCount, weapons.Slots.Count); // 남은 몸통의 슬롯 수
                for (int index = 0; index < slotCount; index++) // 실제 몸통 슬롯 순회
                {
                    SnakeWeaponSlot slot = weapons.Slots[index]; // 실제 현재 슬롯
                    sample.weapons.Add(new RunWeaponSnapshot // 실제 슬롯 상태 생성
                    {
                        slot = index, // 실제 몸통 슬롯 번호
                        weaponId = slot == null || slot.Weapon == null ? "" : slot.Weapon.Id, // 실제 포탑 식별자
                        grade = slot == null || slot.IsEmpty ? 0 : slot.Grade, // 실제 포탑 등급
                        damageUpgrades = slot == null ? 0 : slot.DamageUpgrades, // 실제 피해 강화
                        speedUpgrades = slot == null ? 0 : slot.SpeedUpgrades, // 실제 속도 강화
                        rangeUpgrades = slot == null ? 0 : slot.RangeUpgrades, // 실제 범위 강화
                        attribute = slot == null || slot.Weapon == null ? "" : slot.Weapon.Attribute.ToString(), // 실제 포탑 속성
                        attackType = slot == null || slot.Weapon == null ? "" : slot.Weapon.AttackType.ToString(), // 실제 공격 유형
                        baseDamage = slot == null || slot.Weapon == null ? 0f : slot.Weapon.BaseDamage, // 실제 포탑 기본 피해
                        attackInterval = slot == null || slot.Weapon == null ? 0f : slot.Weapon.AttackInterval, // 실제 포탑 기본 주기
                        effectiveRange = slot == null ? 0f : slot.EffectiveRange // 실제 강화 설정 범위
                    });
                }
            }
            recording.snapshots.Add(sample); // 실제 표본 기록
        }
        public void Finish(GameState outcome) // 실제 전투 종료 기록
        {
            if (!active || recording == null) // 실제 기록 중 확인
            {
                return; // 중복 종료 차단
            }
            recording.outcome = outcome.ToString(); // 실제 승리 또는 패배
            Capture("end"); // 실제 마지막 상태 저장
            active = false; // 이후 중복 기록 방지
            Unsubscribe(); // 실제 이벤트 연결 해제
            try // 실제 로컬 파일 저장
            {
                string directory = Path.Combine(Application.persistentDataPath, "CombatRecords"); // 실제 앱별 기록 폴더
                Directory.CreateDirectory(directory); // 실제 기록 폴더 준비
                string path = Path.Combine(directory, "run_" + recording.runId + ".json"); // 실제 고유 기록 경로
                File.WriteAllText(path, JsonUtility.ToJson(recording, true)); // 실제 JSON 파일 저장
                LastSavedPath = path; // 실제 저장 완료 경로 기록
                Debug.Log("[Project Epsilon] 전투 기록 저장 완료: " + path); // 실제 로컬 저장 결과
            }
            catch (Exception error) // 실제 파일 저장 오류
            {
                LastError = error.Message; // 실제 기록 오류 보관
                Debug.LogError("[Project Epsilon] 전투 기록 저장 실패: " + error.Message); // 실제 기록 오류 안내
            }
        }
        private void Unsubscribe() // 실제 기록 이벤트 정리
        {
            if (wave != null) // 실제 적 출현기 확인
            {
                wave.EnemyDied -= OnEnemyDied; // 실제 모든 적 처치 구독 해제
            }
            if (experience != null) // 실제 경험치 구성 확인
            {
                experience.ExperienceGained -= OnExperience; // 실제 획득 구독 해제
            }
            if (weapons != null) // 실제 포탑 구성 확인
            {
                weapons.SlotsChanged -= OnSlotsChanged; // 실제 포탑 구독 해제
            }
        }
        private void OnDestroy() // 실제 종료 정리
        {
            Unsubscribe(); // 실제 이벤트 구독 해제
            if (Instance == this) // 실제 현재 기록기 확인
            {
                Instance = null; // 실제 전역 참조 제거
            }
        }
    }
}
