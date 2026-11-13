using System; // 직렬화 형식
using System.Collections.Generic; // 생존 적 목록
using ProjectEpsilon.Combat; // 전투 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 구성
using ProjectEpsilon.Progression; // 적 보상
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Enemies // 적 영역
{
    public enum EarlyEnemyKind // 적 행동 종류
    {
        Chaser, // 추적자
        Predator, // 측면 포식자
        Shooter, // 원거리 사수
        Elite, // 돌진 엘리트
        Charger // 일반 예고 돌진 적
    }
    [Serializable] // 편집기 설정 허용
    public sealed class EarlyEnemySettings // 조정 가능한 적 설정
    {
        public EarlyEnemyKind kind; // 행동 종류
        public float health; // 체력
        public int damage; // 공격 피해
        public float speedRatio; // 기본 이동 속도 배율
        public float attackInterval; // 공격 간격
        public int projectileCount = 1; // 사격 투사체 수 임시값
        public float projectileWarningDuration = 0.35f; // 사격 방향 예고 시간 임시값
        public float projectileSpreadDegrees; // 사격 전체 부채각 임시값
        public bool opposedProjectileFan; // 반대 방향 부채꼴 추가 여부
        public float chargeWarningDuration = 0.6f; // 일반 돌진 예고 시간
        public float chargeDuration = 0.6f; // 일반 돌진 지속 시험값
        public float chargeSpeedMultiplier = 3f; // 일반 돌진 속도 시험 배율
        public int experience; // 경험치
        public float firstAppearance; // 최초 출현 시간
        public Color color = Color.white; // 구분 색상
        public Sprite enemySprite; // 적 종류별 외형 이미지
    }
    [DefaultExecutionOrder(100)] // 카메라 이동 이후 실행
    public sealed class EarlyEnemyWaveSpawner : MonoBehaviour // 초반 적 출현기
    {
        [SerializeField] private float spawnInterval = 1.5f; // 임시 출현 간격
        [SerializeField] private int maximumEnemies = 40; // 동시 적 상한
        [SerializeField] private float spawnDistance = 9f; // 출현 거리
        [SerializeField] private float spawnViewportMargin = 0.5f; // 화면 밖 출현 여유 임시값
        [SerializeField] private float despawnDistance = 22f; // 일반 적 정리 거리 임시값
        [SerializeField] private float shooterDistance = 4f; // 사수 유지 거리 임시값
        [SerializeField] private float projectileSpeed = 5f; // 투사체 속도 임시값
        [SerializeField] private float projectileLifetime = 3f; // 투사체 수명 임시값
        [SerializeField] private float eliteWarningDuration = 1f; // 엘리트 예고 시간 가정값
        [SerializeField] private float eliteChargeDuration = 0.6f; // 엘리트 돌진 시간 가정값
        [SerializeField] private float eliteChargeMultiplier = 3f; // 엘리트 돌진 속도 가정값
        [SerializeField] private float[] laterEliteAppearanceTimes = { 480f, 720f, 960f }; // 후속 엘리트 출현 시각
        [SerializeField] private float secondEliteConcurrentTime = 720f; // 엘리트 동시 두 마리 허용 시각
        [SerializeField] private int healAmount = 15; // 회복량 임시값
        [SerializeField] private float healDropChance = 0.25f; // 회복 보상 확률 임시값
        [SerializeField] private float bodyRepairDropChance = 0.1f; // 몸통 복구 확률 임시값
        [SerializeField] private float elapsed; // 전투 경과 시간
        [SerializeField] private float bossTransitionTime = 1200f; // 최종 보스 전환 시각
        [SerializeField] private Sprite visual; // 적과 보석 시각 자료
        [SerializeField] private EliteRewardController eliteRewards; // 엘리트 보상 대기열
        [SerializeField] private EarlyEnemySettings[] enemies = // 시험용 적 설정
        {
            new EarlyEnemySettings // 추적자 임시값
            {
                kind = EarlyEnemyKind.Chaser, // 추적 행동
                health = 25f, // 체력
                damage = 10, // 피해량
                speedRatio = 0.8f, // 속도 배율
                attackInterval = 1.2f, // 공격 간격
                experience = 1, // 경험치
                firstAppearance = 0f, // 출현 시점
                color = new Color(1f, 0.35f, 0.35f) // 표시 색상
            },
            new EarlyEnemySettings // 포식자 임시값
            {
                kind = EarlyEnemyKind.Predator, // 몸통 추적 행동
                health = 35f, // 체력
                damage = 12, // 피해량
                speedRatio = 0.9f, // 속도 배율
                attackInterval = 1.5f, // 공격 간격
                experience = 2, // 경험치
                firstAppearance = 60f, // 출현 시점
                color = new Color(1f, 0.65f, 0.25f) // 표시 색상
            },
            new EarlyEnemySettings // 사수 임시값
            {
                kind = EarlyEnemyKind.Shooter, // 원거리 행동
                health = 20f, // 체력
                damage = 8, // 피해량
                speedRatio = 0.7f, // 속도 배율
                attackInterval = 2f, // 공격 간격
                experience = 2, // 경험치
                firstAppearance = 120f, // 출현 시점
                color = new Color(0.75f, 0.4f, 1f) // 표시 색상
            },
            new EarlyEnemySettings // 엘리트 가정값
            {
                kind = EarlyEnemyKind.Elite, // 예고 돌진 행동
                health = 160f, // 체력
                damage = 15, // 피해량
                speedRatio = 0.9f, // 속도 배율
                attackInterval = 5f, // 공격 간격
                experience = 20, // 경험치
                firstAppearance = 240f, // 출현 시점
                color = new Color(1f, 0.1f, 0.75f) // 표시 색상
            }
        };
        private SnakeMovement player; // 플레이어 이동 참조
        private SnakeBodyManager body; // 플레이어 몸통 참조
        private SnakeHealth health; // 공용 체력 참조
        private EnemyWaveSettings waveSettings; // 시간별 출현 데이터
        private float nextSpawn; // 다음 출현 시각
        private int activeEnemies; // 생존 적 수
        private sealed class EnemyScaleRecord // 개별 적의 원본과 실행 설정
        {
            public EarlyEnemySettings source; // 원본 적 능력치
            public EarlyEnemySettings runtime; // 개별 적 실행 능력치
        }
        private readonly Dictionary<WeaponTarget, EnemyScaleRecord> enemyScales = new Dictionary<WeaponTarget, EnemyScaleRecord>(); // 생존 적 능력치 추적
        private int lastScalingMinute = -1; // 마지막 갱신 분
        private readonly List<WeaponTarget> targets = new List<WeaponTarget>(); // 생존 적 추적
        private readonly HashSet<WeaponTarget> eliteTargets = new HashSet<WeaponTarget>(); // 생존 엘리트 집합
        private readonly HashSet<WeaponTarget> bossSummonTargets = new HashSet<WeaponTarget>(); // 무보상 보스 소환 적 집합
        private readonly Queue<int> pendingEliteRounds = new Queue<int>(); // 출현 대기 회차
        private int nextEliteRound; // 다음 일정 회차
        private double previousClockTime; // 이전 게임 시간 표본
        private bool hasEnteredBossTransition; // 최종 보스 전환 여부
        public event Action BossTransitionStarted; // 최종 보스 전환 알림
        public event Action<bool> RewardableEnemyDied; // 실제 보상 대상 처치 알림
        public event Action<bool, bool> EnemyDied; // 실제 엘리트와 소환 적 처치 알림
        public float RecordingSpawnInterval => CurrentSpawnInterval; // 기록 시점 일반 적 출현 간격
        public int RecordingEnemyLimit => CurrentNormalEnemyLimit; // 기록 시점 일반 적 상한
        public float RecordingBossTime => Mathf.Max(0f, bossTransitionTime); // 실제 보스 전환 시각
        public IReadOnlyList<EarlyEnemySettings> RecordingEnemies // 실제 런 기록용 적 설정 목록
        { // 속성 시작
            get // 전체 설정 조회
            { // 조회 시작
                List<EarlyEnemySettings> result = new List<EarlyEnemySettings>(); // 기존 적과 추가 적 목록
                if (enemies != null) // 기존 적 설정 확인
                { // 조건 시작
                    result.AddRange(enemies); // 기존 적 설정 보존
                } // 조건 종료
                if (waveSettings != null && waveSettings.AdditionalEnemies != null) // 추가 적 설정 확인
                { // 조건 시작
                    result.AddRange(waveSettings.AdditionalEnemies); // 추가 적 설정 포함
                } // 조건 종료
                return result; // 실제 전체 기록 대상 반환
            } // 조회 종료
        } // 속성 종료
        public int ActiveEnemies => activeEnemies; // 생존 적 수 조회
        public int CurrentNormalEnemyCount => Mathf.Max(0, activeEnemies - eliteTargets.Count - bossSummonTargets.Count); // 엘리트와 소환 적을 제외한 일반 적 수
        public float CurrentSpawnInterval => Mathf.Max(0.1f, CurrentStage == null ? spawnInterval : CurrentStage.spawnInterval); // 현재 일반 적 출현 간격
        public int CurrentNormalEnemyLimit => Mathf.Max(1, CurrentStage == null ? maximumEnemies : CurrentStage.normalEnemyLimit); // 현재 일반 적 상한
        public float CurrentNormalHealthMultiplier => waveSettings == null ? 1f : waveSettings.ResolveMultiplier(elapsed, true); // 현재 일반 적 체력 배율
        public float CurrentNormalDamageMultiplier => waveSettings == null ? 1f : waveSettings.ResolveMultiplier(elapsed, false); // 현재 일반 적 피해 배율
        private EnemyWaveStage CurrentStage => waveSettings == null ? null : waveSettings.Resolve(elapsed); // 현재 시간의 출현 구간
        public float ElapsedSeconds => elapsed; // 실제 전투 경과 시간 조회
        public double CurrentRunSeconds => elapsed + (GameManager.Instance != null && (GameManager.Instance.IsPlaying || GameManager.Instance.IsPaused) ? Math.Max(0d, Time.timeAsDouble - previousClockTime) : 0d); // 종료 프레임을 포함한 전투 시간
        public bool HasEnteredBossTransition => hasEnteredBossTransition; // 최종 보스 전환 상태 조회
        private void OnEnable() // 출현기 활성 시간 초기화
        {
            waveSettings = Resources.Load<EnemyWaveSettings>("EnemyWaveSettings"); // 공용 출현 데이터 불러오기
            previousClockTime = Time.timeAsDouble; // 비활성 구간 제외 기준
        }
        private void Start() // 플레이 시작 처리
        {
            player = FindFirstObjectByType<SnakeMovement>(); // 플레이어 이동 조회
            body = FindFirstObjectByType<SnakeBodyManager>(); // 몸통 관리자 조회
            health = FindFirstObjectByType<SnakeHealth>(); // 체력 관리자 조회
            if (visual == null && player != null) // 시각 자료 부재 확인
            {
                SpriteRenderer renderer = player.GetComponentInChildren<SpriteRenderer>(); // 플레이어 렌더러 조회
                visual = renderer == null ? null : renderer.sprite; // 대체 스프라이트 저장
            }
        }
        private void LateUpdate() // 카메라 이동 후 출현 시간 갱신
        {
            double now = Time.timeAsDouble; // 이번 프레임 게임 시간
            float elapsedDelta = (float)Math.Max(0d, now - previousClockTime); // 지난 게임 시간 차이
            previousClockTime = now; // 다음 프레임 기준 저장
            GameManager manager = GameManager.Instance; // 현재 게임 관리자
            if (player == null || manager == null) // 시간 기준과 플레이어 확인
            {
                return; // 출현 중지
            }
            if (manager.CurrentState == GameState.Playing || manager.CurrentState == GameState.Paused) // 전투와 일시정지 상태 확인
            {
                elapsed += elapsedDelta; // 직전 프레임 게임 시간 누적
            }
            if (!manager.IsPlaying) // 일시정지 또는 종료 확인
            {
                return; // 출현과 전환 대기
            }
            RefreshEnemyScaling(); // 생존 적의 분 단위 능력치 갱신
            if (!hasEnteredBossTransition && elapsed >= Mathf.Max(0f, bossTransitionTime)) // 보스 전환 시각 확인
            {
                EnterBossTransition(); // 기존 전투 정리와 전환 알림
            }
            if (hasEnteredBossTransition) // 보스 전환 이후 확인
            {
                return; // 일반 적과 엘리트 출현 중지
            }
            CleanupDistantEnemies(); // 먼 일반 적 정리
            ProcessEliteSchedule(); // 엘리트 일정과 대기 출현 처리

            if (elapsed < nextSpawn || CurrentNormalEnemyCount >= CurrentNormalEnemyLimit) // 일반 적 상한과 출현 대기 확인
            {
                return; // 일반 출현 중지
            }
            nextSpawn = elapsed + CurrentSpawnInterval; // 현재 구간의 다음 출현 시각 저장
            EarlyEnemySettings selected = SelectNormalEnemy(); // 현재 구간의 일반 적 추첨
            if (selected != null) // 실제 선택 결과 확인
            { // 조건 시작
                Spawn(selected); // 선택된 일반 적 한 마리 생성
            } // 조건 종료
        }
        private EarlyEnemySettings SelectNormalEnemy() // 해금된 일반 적의 가중 추첨
        { // 함수 시작
            EarlyEnemySettings[] additional = waveSettings == null ? null : waveSettings.AdditionalEnemies; // 추가 일반 적 목록
            float total = 0f; // 전체 유효 가중치
            if (enemies != null) // 기존 적 목록 확인
            { // 조건 시작
                foreach (EarlyEnemySettings setting in enemies) // 기존 적 순회
                { // 반복 시작
                    total += NormalSpawnWeight(setting); // 기존 적 가중치 합산
                } // 반복 종료
            } // 조건 종료
            if (additional != null) // 추가 적 목록 확인
            { // 조건 시작
                foreach (EarlyEnemySettings setting in additional) // 추가 적 순회
                { // 반복 시작
                    total += NormalSpawnWeight(setting); // 추가 적 가중치 합산
                } // 반복 종료
            } // 조건 종료
            if (total <= 0f) // 유효한 출현 후보 부재 확인
            { // 조건 시작
                return null; // 출현 생략
            } // 조건 종료
            float choice = UnityEngine.Random.value * total; // 전체 가중치에서 난수 선택
            EarlyEnemySettings last = null; // 부동소수 경계의 마지막 후보
            if (enemies != null) // 기존 적 목록 확인
            { // 조건 시작
                foreach (EarlyEnemySettings setting in enemies) // 기존 적 순회
                { // 반복 시작
                    float weight = NormalSpawnWeight(setting); // 현재 적의 유효 가중치
                    if (weight <= 0f) // 출현 불가능한 적 확인
                    { // 조건 시작
                        continue; // 다음 후보 검사
                    } // 조건 종료
                    last = setting; // 마지막 유효 후보 보관
                    if (choice <= weight) // 선택한 가중치 범위 확인
                    { // 조건 시작
                        return setting; // 실제 선택 적 반환
                    } // 조건 종료
                    choice -= weight; // 다음 후보의 난수 범위
                } // 반복 종료
            } // 조건 종료
            if (additional != null) // 추가 적 목록 확인
            { // 조건 시작
                foreach (EarlyEnemySettings setting in additional) // 추가 적 순회
                { // 반복 시작
                    float weight = NormalSpawnWeight(setting); // 현재 적의 유효 가중치
                    if (weight <= 0f) // 출현 불가능한 적 확인
                    { // 조건 시작
                        continue; // 다음 후보 검사
                    } // 조건 종료
                    last = setting; // 마지막 유효 후보 보관
                    if (choice <= weight) // 선택한 가중치 범위 확인
                    { // 조건 시작
                        return setting; // 실제 선택 적 반환
                    } // 조건 종료
                    choice -= weight; // 다음 후보의 난수 범위
                } // 반복 종료
            } // 조건 종료
            return last; // 난수 경계에서 마지막 후보 사용
        } // 함수 종료
        private float NormalSpawnWeight(EarlyEnemySettings setting) // 현재 시점의 유효 출현 가중치
        { // 함수 시작
            if (setting == null || setting.kind == EarlyEnemyKind.Elite || elapsed < setting.firstAppearance) // 미해금 적과 엘리트 확인
            { // 조건 시작
                return 0f; // 일반 적 추첨 제외
            } // 조건 종료
            EnemyWaveStage stage = CurrentStage; // 현재 웨이브 구간
            return stage == null ? 1f : stage.GetSpawnWeight(setting.kind); // 구간 가중치 또는 기존 균등 추첨
        } // 함수 종료
        private void EnterBossTransition() // 기존 전투 무보상 정리
        {
            hasEnteredBossTransition = true; // 중복 전환 방지
            SnakeWeaponMergeController merge = FindFirstObjectByType<SnakeWeaponMergeController>(); // 진행 중 합성 조회
            if (merge != null && merge.IsMergeOpen) // 미확정 합성 확인
            {
                merge.CloseMergePanel(); // 기존 재료를 보존한 합성 보류
            }
            pendingEliteRounds.Clear(); // 미출현 엘리트 회차 폐기
            foreach (WeaponTarget target in targets) // 관리 중인 실제 적 순회
            {
                if (target == null) // 이미 제거된 적 확인
                {
                    continue; // 빈 대상 무시
                }
                target.Died -= HandleEnemyDied; // 처치와 보상 연결 해제
                target.gameObject.SetActive(false); // 지연 파괴 전 공격과 보상 차단
                Destroy(target.gameObject); // 사망 이벤트 없는 제거
            }
            targets.Clear(); // 일반 적과 엘리트 목록 비우기
            enemyScales.Clear(); // 기존 적 능력치 추적 해제
            eliteTargets.Clear(); // 생존 엘리트 목록 비우기
            bossSummonTargets.Clear(); // 이전 소환 적 목록 제거
            activeEnemies = 0; // 생존 적 수 초기화
            foreach (EnemyTestProjectile shot in FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None)) // 실제 적 투사체 순회
            {
                shot.gameObject.SetActive(false); // 지연 파괴 전 접촉 차단
                Destroy(shot.gameObject); // 기존 적 투사체 제거
            }
            foreach (RecoveryPickup pickup in FindObjectsByType<RecoveryPickup>(FindObjectsSortMode.None)) // 실제 회복 아이템 순회
            {
                if (!pickup.IsBodyRepair) // 체력 회복 아이템 확인
                {
                    continue; // 체력 회복 아이템 보존
                }
                pickup.gameObject.SetActive(false); // 지연 파괴 전 획득 차단
                Destroy(pickup.gameObject); // 몸통 복구 아이템 제거
            }
            BossTransitionStarted?.Invoke(); // 정리 완료 후 보스 전환 알림
        }
        private void Spawn(EarlyEnemySettings setting) // 적 생성
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized; // 출현 방향 추첨
            if (direction.sqrMagnitude < 0.01f) // 방향 부재 확인
            {
                direction = Vector2.right; // 대체 방향 설정
            }
            SpawnAt(setting, player.transform.position + (Vector3)(direction * ResolveSpawnDistance(direction)), false); // 기존 일반 적 생성 경로
        }
        public WeaponTarget SpawnBossSummon(Vector3 position) // 보스 패턴의 일반 적 소환
        {
            if (player == null || body == null || health == null || enemies == null || enemies.Length == 0 || enemies[0] == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || !GameManager.Instance.IsFinalBossPhase) // 실제 보스전과 적 설정 확인
            {
                return null; // 무효 소환 방지
            }
            return SpawnAt(enemies[0], position, true); // 기존 추적자 행동 재사용
        }
        public WeaponTarget SpawnMapChaser(Vector3 position) // 맵 요소의 보상 가능 추적자 소환
        {
            if (player == null || body == null || health == null || enemies == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || hasEnteredBossTransition || GameManager.Instance.IsFinalBossPhase || CurrentNormalEnemyCount >= CurrentNormalEnemyLimit) // 기존 전투 상태와 일반 적 상한 확인
            {
                return null; // 소환 불가 상태 유지
            }
            foreach (EarlyEnemySettings setting in enemies) // 기존 적 설정 순회
            {
                if (setting != null && setting.kind == EarlyEnemyKind.Chaser && elapsed >= setting.firstAppearance) // 해금된 추적자 설정 확인
                {
                    return SpawnAt(setting, position, false); // 기존 경험치와 체력 배율을 가진 일반 적 생성
                }
            }
            return null; // 실제 추적자 설정 부재
        }
        public void RemoveBossSummon(WeaponTarget target) // 생존 소환 적 무보상 제거
        {
            if (target == null || !targets.Remove(target)) // 관리 대상 확인
            {
                return; // 이미 제거된 대상 무시
            }
            target.Died -= HandleEnemyDied; // 처치 수량 연결 해제
            bossSummonTargets.Remove(target); // 무보상 소환 목록 제거
            enemyScales.Remove(target); // 소환 적 능력치 추적 해제
            activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 적 수 감소
            target.gameObject.SetActive(false); // 지연 파괴 전 접촉 차단
            Destroy(target.gameObject); // 사망 이벤트 없는 제거
        }
        private WeaponTarget SpawnAt(EarlyEnemySettings setting, Vector3 position, bool bossSummon) // 기존 적 생성 구조 재사용
        {
            EarlyEnemySettings source = setting; // 원본 적 능력치 보존
            setting = ScaleNormalEnemy(source); // 모든 일반 적과 엘리트의 능력치 복사
            GameObject enemy = new GameObject("EarlyEnemy_" + setting.kind); // 적 오브젝트 생성
            enemy.transform.position = position; // 실제 출현 위치
            SpriteRenderer renderer = enemy.AddComponent<SpriteRenderer>(); // 적 렌더러 생성
            renderer.sprite = setting.enemySprite != null ? setting.enemySprite : visual; // 종류별 이미지 또는 기존 대체 이미지
            renderer.color = setting.enemySprite != null ? Color.white : setting.color; // 전용 이미지 원색 또는 기존 구분 색상
            renderer.sortingOrder = 8; // 적 표시 순서
            enemy.transform.localScale = Vector3.one * (setting.kind == EarlyEnemyKind.Elite ? 1.6f : 1f); // 적 크기 두 배 적용
            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>(); // 적 충돌체 생성
            collider.isTrigger = true; // 접촉 감지 설정
            Rigidbody2D rigidbody = enemy.AddComponent<Rigidbody2D>(); // 적 물리체 생성
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 물리체 이동 방식
            rigidbody.gravityScale = 0f; // 중력 제거
            rigidbody.useFullKinematicContacts = true; // 운동학 충돌 감지
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 공격 가능 대상 생성
            target.Configure(setting.health); // 적 체력 설정
            enemy.AddComponent<EnemyHitFlash>(); // 실제 피격 표시 연결
            enemy.AddComponent<WeaponTargetStatusController>(); // 기존 상태 효과 적용
            enemy.AddComponent<WeaponTargetKnockbackController>(); // 기존 넉백 적용
            if (!bossSummon) // 일반 적 보상 여부
            {
                EnemyDropController drop = enemy.AddComponent<EnemyDropController>(); // 기존 보상 적용
                drop.Configure(setting.experience, healAmount, healDropChance, bodyRepairDropChance, visual, setting.kind != EarlyEnemyKind.Elite && setting.experience == 2 ? 2 : 1); // 기존 일반 적 보상 유지
            }
            SnakeContactHazard hazard = enemy.AddComponent<SnakeContactHazard>(); // 머리 직접 충돌 판정
            hazard.Configure(SnakeContactHazardType.EnemyDirect); // 적 직접 충돌 종류
            EarlyEnemyBehaviour behaviour = enemy.AddComponent<EarlyEnemyBehaviour>(); // 적 행동 생성
            behaviour.Configure(setting, player, body, health, visual, shooterDistance, projectileSpeed, projectileLifetime, eliteWarningDuration, eliteChargeDuration, eliteChargeMultiplier); // 적 행동 연결
            activeEnemies++; // 생존 적 증가
            targets.Add(target); // 생존 적 목록 추가
            enemyScales.Add(target, new EnemyScaleRecord { source = source, runtime = setting }); // 생존 적별 원본과 실행 설정 저장
            if (bossSummon) // 보스 소환 적 여부 확인
            {
                bossSummonTargets.Add(target); // 무보상 소환 적 구분
            }
            if (setting.kind == EarlyEnemyKind.Elite) // 엘리트 확인
            {
                eliteTargets.Add(target); // 정리 제외 대상 등록
            }
            target.Died += HandleEnemyDied; // 처치 시 수량 갱신 연결
            return target; // 실제 생성 대상 반환
        }
        private void RefreshEnemyScaling() // 생존 적의 분 단위 능력치 갱신
        {
            int minute = Mathf.FloorToInt(Mathf.Max(0f, elapsed) / 60f); // 완료된 전투 분
            if (minute == lastScalingMinute) // 동일한 분 확인
            {
                return; // 중복 갱신 생략
            }
            lastScalingMinute = minute; // 갱신 분 기록
            foreach (WeaponTarget target in targets) // 관리 중인 생존 적 순회
            {
                if (target == null || !target.IsAlive || !enemyScales.TryGetValue(target, out EnemyScaleRecord record)) // 유효한 생존 적 확인
                {
                    continue; // 제거된 적 건너뛰기
                }
                record.runtime.health = Mathf.Ceil(record.source.health * CurrentNormalHealthMultiplier); // 원본 기준 최대 체력 갱신
                record.runtime.damage = Mathf.CeilToInt(record.source.damage * CurrentNormalDamageMultiplier); // 원본 기준 공격력 갱신
                target.SetMaximumHealthPreservingRatio(record.runtime.health); // 기존 체력 비율 유지
            }
        }
        private EarlyEnemySettings ScaleNormalEnemy(EarlyEnemySettings source) // 일반 적 능력치 복사와 구간 배율 적용
        {
            return new EarlyEnemySettings // 원본과 분리된 출현 적 설정
            {
                kind = source.kind, // 기존 행동 종류
                health = Mathf.Ceil(source.health * CurrentNormalHealthMultiplier), // 구간 체력 배율과 올림
                damage = Mathf.CeilToInt(source.damage * CurrentNormalDamageMultiplier), // 구간 피해 배율과 올림
                speedRatio = source.speedRatio, // 기존 이동 속도
                attackInterval = source.attackInterval, // 기존 공격 간격
                projectileCount = source.projectileCount, // 기존 투사체 수
                projectileWarningDuration = source.projectileWarningDuration, // 기존 사격 예고 시간
                projectileSpreadDegrees = source.projectileSpreadDegrees, // 기존 투사체 각도
                opposedProjectileFan = source.opposedProjectileFan, // 기존 양방향 사격 여부
                chargeWarningDuration = source.chargeWarningDuration, // 일반 돌진 예고 시간
                chargeDuration = source.chargeDuration, // 일반 돌진 지속 시간
                chargeSpeedMultiplier = source.chargeSpeedMultiplier, // 일반 돌진 속도 배율
                experience = source.experience, // 기존 경험치
                firstAppearance = source.firstAppearance, // 기존 최초 출현 시각
                color = source.color, // 기존 표시 색상
                enemySprite = source.enemySprite // 기존 적 이미지
            };
        }
        private float ResolveSpawnDistance(Vector2 direction) // 화면 밖 출현 거리
        {
            float minimum = Mathf.Max(2f, spawnDistance); // 기존 최소 출현 거리
            Camera camera = Camera.main; // 현재 주 카메라 조회
            if (camera == null || !camera.orthographic || camera.orthographicSize <= 0f || camera.aspect <= 0f) // 직교 카메라 유효성 확인
            {
                return minimum; // 카메라 부재 시 기존 거리
            }
            float margin = Mathf.Max(0f, spawnViewportMargin); // 화면 밖 여유 거리
            float horizontal = margin / (2f * camera.orthographicSize * camera.aspect); // 가로 뷰포트 여유
            float vertical = margin / (2f * camera.orthographicSize); // 세로 뷰포트 여유
            Vector3 origin = camera.WorldToViewportPoint(player.transform.position); // 플레이어 화면 위치
            Vector3 step = camera.WorldToViewportPoint(player.transform.position + (Vector3)direction) - origin; // 단위 방향 화면 변화
            float xExit = float.PositiveInfinity; // 가로 경계 거리 초기화
            float yExit = float.PositiveInfinity; // 세로 경계 거리 초기화
            if (step.x > 0.000001f) // 오른쪽 경계 방향 확인
            {
                xExit = (1f + horizontal - origin.x) / step.x; // 오른쪽 화면 경계 거리
            }
            else if (step.x < -0.000001f) // 왼쪽 경계 방향 확인
            {
                xExit = (-horizontal - origin.x) / step.x; // 왼쪽 화면 경계 거리
            }
            if (step.y > 0.000001f) // 위쪽 경계 방향 확인
            {
                yExit = (1f + vertical - origin.y) / step.y; // 위쪽 화면 경계 거리
            }
            else if (step.y < -0.000001f) // 아래쪽 경계 방향 확인
            {
                yExit = (-vertical - origin.y) / step.y; // 아래쪽 화면 경계 거리
            }
            float exit = Mathf.Min(xExit, yExit); // 가장 먼저 벗어나는 경계 거리
            if (float.IsInfinity(exit) || float.IsNaN(exit)) // 비정상 투영 확인
            {
                return minimum; // 기존 거리 대체
            }
            return Mathf.Max(minimum, Mathf.Max(0f, exit) + 0.01f); // 최소 거리와 화면 밖 여유 적용
        }
        private void HandleEnemyDied(WeaponTarget target) // 적 처치 처리
        {
            if (target == null || !targets.Remove(target)) // 등록 대상과 중복 사망 확인
            {
                return; // 중복 사망 무시
            }
            enemyScales.Remove(target); // 처치 적 능력치 추적 해제
            bool elite = eliteTargets.Remove(target); // 실제 엘리트 처치 여부
            bool summon = bossSummonTargets.Remove(target); // 무보상 소환 처치 여부
            EnemyDied?.Invoke(elite, summon); // 실제 모든 적 처치 알림
            if (elite) // 엘리트 개별 처치 확인
            {
                eliteRewards?.QueueReward(); // 선택 보상 대기열 추가
            }
            if (!summon) // 보상 가능한 일반 적 또는 엘리트 확인
            {
                RewardableEnemyDied?.Invoke(elite); // 실제 런 성과 한 번 기록
            }
            activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 적 감소
        }
        public static bool ShouldDespawn(Vector3 position, Vector3 playerPosition, float distance, bool elite) // 거리 정리 조건
        {
            float safeDistance = Mathf.Max(0.1f, distance); // 최소 정리 거리
            return !elite && (position - playerPosition).sqrMagnitude > safeDistance * safeDistance; // 일반 적 거리 초과 여부
        }
        private void CleanupDistantEnemies() // 먼 일반 적 정리
        {
            float cleanupDistance = ResolveCleanupDistance(); // 현재 화면의 안전 정리 거리
            for (int index = targets.Count - 1; index >= 0; index--) // 생존 적 역순 확인
            {
                WeaponTarget target = targets[index]; // 현재 적 조회
                if (target == null) // 외부 제거 적 확인
                {
                    enemyScales.Remove(target); // 외부 제거 적 능력치 추적 해제
                    eliteTargets.Remove(target); // 외부 제거 엘리트 정리
                    bossSummonTargets.Remove(target); // 외부 제거 소환 적 정리
                    targets.RemoveAt(index); // 빈 대상 제거
                    activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 수 보정
                    continue; // 다음 적 확인
                }
                if (!ShouldDespawn(target.transform.position, player.transform.position, cleanupDistance, eliteTargets.Contains(target))) // 정리 조건 확인
                {
                    continue; // 유지 대상 건너뛰기
                }
                target.Died -= HandleEnemyDied; // 처치 수량 연결 해제
                enemyScales.Remove(target); // 거리 정리 적 능력치 추적 해제
                bossSummonTargets.Remove(target); // 거리 정리 소환 적 제외
                targets.RemoveAt(index); // 생존 목록 제거
                activeEnemies = Mathf.Max(0, activeEnemies - 1); // 생존 수 감소
                target.gameObject.SetActive(false); // 지연 파괴 전 보상과 공격 대상 해제
                Destroy(target.gameObject); // 보상 없이 적 제거
            }
        }
        private float ResolveCleanupDistance() // 생성 직후 정리 방지 거리
        {
            float safeDistance = Mathf.Max(despawnDistance, Mathf.Max(2f, spawnDistance) + 0.1f); // 기존 정리 및 출현 거리
            Camera camera = Camera.main; // 현재 주 카메라 조회
            if (camera == null || !camera.orthographic || camera.orthographicSize <= 0f || camera.aspect <= 0f) // 직교 카메라 유효성 확인
            {
                return safeDistance; // 카메라 부재 시 기본 거리
            }
            Vector2 offset = camera.transform.position - player.transform.position; // 플레이어와 카메라 거리
            float margin = Mathf.Max(0f, spawnViewportMargin); // 화면 밖 여유 거리
            float halfHeight = camera.orthographicSize + margin; // 여유를 포함한 세로 반폭
            float halfWidth = camera.orthographicSize * camera.aspect + margin; // 여유를 포함한 가로 반폭
            float cornerDistance = Mathf.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight); // 여유 경계의 최장 거리
            float screenDistance = offset.magnitude + cornerDistance + 0.1f; // 화면 밖 생성 보존 거리
            return Mathf.Max(safeDistance, screenDistance); // 현재 화면을 포함한 정리 거리
        }
        private void ProcessEliteSchedule() // 엘리트 일정과 동시 상한
        {
            if (enemies == null || enemies.Length <= 3 || enemies[3] == null) // 엘리트 설정 확인
            {
                return; // 설정 부재 시 일정 유지
            }
            int laterCount = laterEliteAppearanceTimes == null ? 0 : laterEliteAppearanceTimes.Length; // 후속 회차 수
            while (nextEliteRound <= laterCount) // 남은 일정 순회
            {
                float appearance = nextEliteRound == 0 ? enemies[3].firstAppearance : laterEliteAppearanceTimes[nextEliteRound - 1]; // 현재 회차의 출현 시각
                if (elapsed < appearance) // 다음 회차 시각 확인
                {
                    break; // 미래 일정 대기
                }
                pendingEliteRounds.Enqueue(nextEliteRound); // 도달한 회차 보류
                nextEliteRound++; // 다음 회차 선택
            }
            int concurrentLimit = elapsed >= secondEliteConcurrentTime ? 2 : 1; // 임시 시점별 동시 상한
            if (pendingEliteRounds.Count == 0 || eliteTargets.Count >= concurrentLimit) // 대기 회차와 엘리트 동시 상한 확인
            {
                return; // 자리 확보 전 대기
            }
            pendingEliteRounds.Dequeue(); // 가장 오래된 회차 소비
            Spawn(enemies[3]); // 동일 시험용 엘리트 출현
        }
    }
}
