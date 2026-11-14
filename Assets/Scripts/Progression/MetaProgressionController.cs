using System; // 실제 런 식별자
using ProjectEpsilon.Combat; // 실제 속성 집계
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 속성
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 해금 캐릭터 이름
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Progression // 영구 성장 영역
{
    [DefaultExecutionOrder(-200)] // 캐릭터 초기화 전 저장 로드
    public sealed class MetaProgressionController : MonoBehaviour // 실제 런 성과와 영구 성장 연결
    {
        private const string SaveKey = "ProjectEpsilon.MetaProgress.v1"; // 입력 설정과 분리된 저장 키
        [SerializeField] private MetaUnlockConfig unlockConfig; // 미정 해금 대상 시험 설정
        private EarlyEnemyWaveSpawner spawner; // 실제 전투 시간과 처치 집계
        private WeaponAttributeSynergyManager synergy; // 실제 속성 시너지
        private string runId; // 현재 런의 고유 식별자
        private int kills; // 보상 가능한 적 처치 수
        private int elites; // 실제 엘리트 처치 수
        private RunScoreSettings scoreSettings; // 조정 가능한 점수 기준
        private SnakeExperience scoreExperience; // 실제 경험치 획득 대상
        private long totalExperience; // 이번 전투의 실제 경험치
        private int currentScore; // 현재 전투 점수
        private bool scoreFinalized; // 승리 점수 확정 여부
        public int CurrentScore => currentScore; // 현재 표시 점수
        public int BossScore { get; private set; } // 보스 처치 점수
        public int ExperienceScore { get; private set; } // 경험치 점수
        public int NormalKillScore { get; private set; } // 일반 적 처치 점수
        public int EliteScore { get; private set; } // 엘리트 처치 점수
        public int HealthScore { get; private set; } // 남은 체력 점수
        public int BodyScore { get; private set; } // 남은 몸통 점수
        public int ExperienceScoreTarget { get; private set; } // 점수 계산에 사용한 경험치 기준
        public int NormalKillScoreTarget { get; private set; } // 점수 계산에 사용한 처치 기준
        public int NormalKills => Math.Max(0, kills - elites); // 일반 적 처치 수
        public long TotalExperience => totalExperience; // 누적 실제 경험치
        public bool ScoreFinalized => scoreFinalized; // 승리 점수 확정 상태
        public event Action<int, int> ScoreChanged; // 현재 점수와 증가량 알림
        private bool runActive; // 현재 런 보상 가능 여부
        private bool finished; // 현재 런 보상 확정 여부
        private bool pending; // 저장 실패 보상 대기
        private double pendingSeconds; // 저장 대기 생존 시간
        private bool pendingBoss; // 저장 대기 보스 처치
        private int pendingMap; // 저장 대기 맵 번호
        public static MetaProgressionController Instance { get; private set; } // 현재 실제 관리자
        public MetaProgressionService Service { get; private set; } // 실제 메타 진행 서비스
        public MetaProgressionMenu Menu { get; private set; } // 실제 시작 성장 메뉴
        public bool HasPendingSave => pending; // 저장 실패 재시도 상태
        private bool hadRuins; // 런 시작 폐허 해금
        private bool hadSwamp; // 런 시작 습지 해금
        private bool hadCharacter; // 런 시작 캐릭터 해금
        private int hadTurretAttributes; // 런 시작 포탑 속성 해금
        public double LastRunSeconds => pendingSeconds; // 실제 종료 생존 시간
        public string LastUnlockNotice { get; private set; } // 실제 새 해금 요약
        public int LastAward { get; private set; } // 마지막 실제 코어 지급량
        public int Kills => kills; // 실제 적 처치 수
        public int Elites => elites; // 실제 엘리트 처치 수
        public bool RerollUsed { get; private set; } // 현재 런 후보 재선택 여부
        public bool CanReroll => runActive && !finished && !RerollUsed && Level(MetaNode.Reroll) > 0; // 실제 재선택 가능 여부
        private void Awake() // 실제 저장 초기화
        {
            if (Instance != null && Instance != this) // 중복 관리자 확인
            {
                Destroy(this); // 중복 진행 관리자 제거
                return; // 추가 초기화 차단
            }
            Instance = this; // 실제 관리자 등록
            scoreSettings = Resources.Load<RunScoreSettings>("RunScoreSettings"); // 점수 기준 데이터 로드
            Service = new MetaProgressionService(new PlayerPrefsMetaStorage(SaveKey)); // 입력과 분리된 메타 저장 로드
            Menu = gameObject.AddComponent<MetaProgressionMenu>(); // 실제 시작 성장 메뉴 생성
            Menu.Configure(this); // 실제 영구 성장 연결
        }
        private void Start() // 실제 출현과 시너지 구독
        {
            spawner = GetComponent<EarlyEnemyWaveSpawner>(); // 같은 관리자 적 출현기
            synergy = FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 시너지 관리자
            if (spawner != null) // 실제 출현기 확인
            {
                spawner.RewardableEnemyDied += HandleEnemyDied; // 실제 보상 가능한 처치 구독
            }
            if (synergy != null) // 실제 시너지 확인
            {
                synergy.SynergyChanged += HandleSynergyChanged; // 실제 속성 변화 구독
            }
        }
        private void OnDestroy() // 실제 구독 정리
        {
            UnbindScoreExperience(); // 경험치 점수 구독 정리
            if (spawner != null) // 실제 출현기 확인
            {
                spawner.RewardableEnemyDied -= HandleEnemyDied; // 실제 처치 구독 해제
            }
            if (synergy != null) // 실제 시너지 확인
            {
                synergy.SynergyChanged -= HandleSynergyChanged; // 실제 시너지 구독 해제
            }
            if (Instance == this) // 실제 현재 관리자 확인
            {
                Instance = null; // 이전 런 전역 참조 제거
            }
        }
        public void BeginRun() // 실제 새 런 시작
        {
            if (runActive || pending) // 진행 중 또는 저장 대기 확인
            {
                return; // 기존 성과 덮어쓰기 방지
            }
            runId = Guid.NewGuid().ToString("N"); // 중복 지급 방지 식별자
            kills = 0; // 실제 처치 수 초기화
            elites = 0; // 실제 엘리트 수 초기화
            totalExperience = 0; // 새 전투의 경험치 초기화
            currentScore = 0; // 새 전투의 점수 초기화
            BossScore = ExperienceScore = NormalKillScore = EliteScore = HealthScore = BodyScore = 0; // 점수 항목 초기화
            ExperienceScoreTarget = scoreSettings == null ? 0 : scoreSettings.ExperienceTarget; // 새 전투의 경험치 기준
            NormalKillScoreTarget = scoreSettings == null ? 0 : scoreSettings.NormalKillTarget; // 새 전투의 일반 적 기준
            scoreFinalized = false; // 이전 승리 확정 상태 제거
            UnbindScoreExperience(); // 이전 플레이어 경험치 구독 해제
            scoreExperience = SnakeExperience.Current; // 현재 플레이어 경험치 조회
            if (scoreExperience != null) // 실제 경험치 대상 확인
            { // 조건 시작
                scoreExperience.ExperienceGained += HandleScoreExperience; // 실제 경험치 획득 구독
            } // 조건 종료
            ScoreChanged?.Invoke(0, 0); // 새 전투 점수 표시 초기화
            LastAward = 0; // 이전 보상 초기화
            RerollUsed = false; // 실제 런 재선택 초기화
            hadRuins = Service != null && Service.IsMapUnlocked(1); // 기존 폐허 해금 기록
            hadSwamp = Service != null && Service.IsMapUnlocked(2); // 기존 습지 해금 기록
            hadCharacter = Service != null && Service.IsCharacterUnlocked(0); // 기존 캐릭터 해금 기록
            hadTurretAttributes = AttributeMask(); // 기존 포탑 속성 기록
            pendingSeconds = 0d; // 이전 런 시간 초기화
            LastUnlockNotice = string.Empty; // 이전 해금 표시 초기화
            finished = false; // 실제 종료 상태 초기화
            runActive = true; // 실제 런 집계 시작
            HandleSynergyChanged(); // 시작 시 이미 충족한 속성 확인
        }
        private void HandleEnemyDied(bool elite) // 실제 적 처치 집계
        {
            if (!runActive || finished || pending) // 실제 런 상태 확인
            {
                return; // 종료된 런 집계 차단
            }
            kills = kills == int.MaxValue ? kills : kills + 1; // 안전한 적 처치 누적
            if (elite && elites < int.MaxValue) // 실제 엘리트 여부 확인
            {
                elites++; // 실제 엘리트 처치 누적
            }
            RefreshLiveScore(); // 실제 처치 점수 갱신
        }
        private void UnbindScoreExperience() // 경험치 이벤트 연결 해제
        { // 함수 시작
            if (scoreExperience != null) // 이전 경험치 대상 확인
            { // 조건 시작
                scoreExperience.ExperienceGained -= HandleScoreExperience; // 이전 획득 구독 해제
                scoreExperience = null; // 이전 대상 참조 제거
            } // 조건 종료
        } // 함수 종료
        private void HandleScoreExperience(int amount) // 실제 획득 경험치 집계
        { // 함수 시작
            if (!runActive || finished || pending || amount <= 0) // 유효한 전투 획득 확인
            { // 조건 시작
                return; // 종료 또는 무효 획득 제외
            } // 조건 종료
            totalExperience = totalExperience > long.MaxValue - amount ? long.MaxValue : totalExperience + amount; // 안전한 경험치 누적
            RefreshLiveScore(); // 실제 획득 점수 갱신
        } // 함수 종료
        private static int FractionScore(long value, long target, int maximum) // 기준 대비 점수 계산
        { // 함수 시작
            if (target <= 0 || value <= 0) // 미정 기준 또는 무득점 확인
            { // 조건 시작
                return 0; // 미정 항목 점수 제외
            } // 조건 종료
            return (int)Math.Floor(Math.Min(1d, (double)value / target) * maximum); // 상한이 있는 비율 점수
        } // 함수 종료
        private void SetScore(int value) // 점수 표시와 증가량 알림
        { // 함수 시작
            int previous = currentScore; // 이전 점수 기록
            currentScore = Mathf.Clamp(value, 0, 100000); // 전체 점수 상한 적용
            if (currentScore != previous) // 점수 변동 확인
            { // 조건 시작
                ScoreChanged?.Invoke(currentScore, Math.Max(0, currentScore - previous)); // 실제 증가량 전달
            } // 조건 종료
        } // 함수 종료
        private void RefreshLiveScore() // 전투 중 달성 점수 계산
        { // 함수 시작
            ExperienceScoreTarget = scoreSettings == null ? 0 : scoreSettings.ExperienceTarget; // 실제 사용한 경험치 기준
            NormalKillScoreTarget = scoreSettings == null ? 0 : scoreSettings.NormalKillTarget; // 실제 사용한 일반 적 기준
            ExperienceScore = FractionScore(totalExperience, ExperienceScoreTarget, 20000); // 경험치 점수
            NormalKillScore = FractionScore(NormalKills, NormalKillScoreTarget, 10000); // 일반 적 점수
            EliteScore = FractionScore(elites, 4, 10000); // 엘리트 점수
            SetScore(ExperienceScore + NormalKillScore + EliteScore); // 현재 누적 점수 반영
        } // 함수 종료
        private void FinalizeScore(bool victory) // 실제 종료 시 점수 확정
        { // 함수 시작
            if (!victory) // 실제 보스 승리 여부 확인
            { // 조건 시작
                return; // 패배와 포기의 승리 보너스 제외
            } // 조건 종료
            SnakeHealth health = FindFirstObjectByType<SnakeHealth>(); // 실제 남은 공용 체력
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 남은 몸통
            HealthScore = health == null ? 0 : FractionScore(health.CurrentHealth, health.MaximumHealth, 10000); // 남은 체력 점수
            BodyScore = body == null ? 0 : FractionScore(body.CurrentBodyCount, body.EarnedBodyCount, 10000); // 남은 몸통 점수
            BossScore = 40000; // 실제 보스 처치 점수
            scoreFinalized = true; // 승리 점수 확정 표시
            SetScore(currentScore + BossScore + HealthScore + BodyScore); // 보스와 생존 보너스 반영
        } // 함수 종료
        private void HandleSynergyChanged() // 실제 속성 포탑 해금 갱신
        {
            if (!runActive || finished || synergy == null || unlockConfig == null || Service == null || !Service.IsAvailable) // 실제 런과 설정 확인
            {
                return; // 무효 해금 차단
            }
            for (int index = 0; index < 8; index++) // 실제 여덟 속성 순회
            {
                if (!Service.IsTurretAttributeUnlocked(index) && synergy.GetCount((WeaponAttribute)index) >= unlockConfig.SynergyThreshold) // 시험 해금 조건 확인
                {
                    Service.TryUnlockTurretAttribute(index); // 실제 해금 자동 저장
                }
            }
        }
        public bool CompleteRun(bool bossDefeated) // 실제 런 종료와 보상 저장
        {
            if (!runActive || finished || Service == null) // 실제 진행 상태 확인
            {
                return finished; // 중복 보상 차단
            }
            if (!pending) // 최초 종료 정보 확인
            {
                pendingSeconds = spawner == null ? 0d : spawner.CurrentRunSeconds; // 실제 전투 시간 표본
                pendingBoss = bossDefeated; // 실제 최종 보스 처치
                PrototypeMapSelection map = GetComponent<PrototypeMapSelection>(); // 실제 선택 맵
                pendingMap = map == null ? 0 : map.SelectedIndex; // 실제 맵 번호
                FinalizeScore(bossDefeated); // 최초 종료 시 점수 한 번 확정
                UnbindScoreExperience(); // 종료 후 경험치 집계 중지
                pending = true; // 실패해도 같은 성과 보존
            }
            int before = Service.Cores; // 저장 전 실제 코어
            if (!Service.TryCompleteRun(runId, pendingSeconds, kills, elites, pendingBoss, pendingMap)) // 실제 저장과 중복 방지 확인
            {
                Debug.LogError("[Project Epsilon] 런 보상을 저장하지 못했습니다. 재시도 전 기록을 유지합니다."); // 저장 실패 알림
                return false; // 저장 전 종료 확정 차단
            }
            LastAward = Math.Max(0, Service.Cores - before); // 실제 지급된 코어
            LastUnlockNotice = string.Empty; // 실제 새 해금 초기화
            if (!hadRuins && Service.IsMapUnlocked(1)) // 새 폐허 해금 확인
            {
                LastUnlockNotice += "폐허  "; // 실제 폐허 해금 표시
            }
            if (!hadSwamp && Service.IsMapUnlocked(2)) // 새 습지 해금 확인
            {
                LastUnlockNotice += "습지  "; // 실제 습지 해금 표시
            }
            if (!hadCharacter && Service.IsCharacterUnlocked(0)) // 새 캐릭터 해금 확인
            {
                LastUnlockNotice += UnlockedCharacterName() + "  "; // 실제 설정된 캐릭터 이름
            }
            int newAttributes = AttributeMask() & ~hadTurretAttributes; // 실제 새 포탑 속성
            for (int index = 0; index < 8; index++) // 실제 속성별 해금 순회
            {
                if ((newAttributes & (1 << index)) != 0) // 실제 새 속성 확인
                {
                    LastUnlockNotice += WeaponDisplayNames.Attribute((WeaponAttribute)index) + " 포탑  "; // 실제 해금 포탑의 한국어 이름
                }
            }
            pending = false; // 저장 대기 해제
            finished = true; // 실제 보상 확정
            runActive = false; // 종료 후 처치 집계 중지
            return true; // 실제 보상 완료
        }
        private string UnlockedCharacterName() // 실제 해금 캐릭터 표시 이름
        {
            CharacterRunInitializer initializer = FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 목록
            string id = unlockConfig == null ? null : unlockConfig.FirstBossCharacterId; // 실제 설정된 캐릭터 식별자
            for (int index = 0; initializer != null && index < initializer.CharacterCount; index++) // 실제 캐릭터 순회
            {
                CharacterData candidate = initializer.GetCharacter(index); // 실제 캐릭터 데이터
                if (candidate != null && candidate.Id == id) // 실제 설정 대상 확인
                {
                    return candidate.DisplayName; // 실제 표시 이름 반환
                }
            }
            return "캐릭터"; // 실제 이름 부재 시 기본 안내
        }
        private int AttributeMask() // 실제 속성 해금 집계
        {
            int mask = 0; // 실제 해금 비트
            for (int index = 0; index < 8; index++) // 실제 여덟 속성 순회
            {
                if (Service != null && Service.IsTurretAttributeUnlocked(index)) // 실제 속성 해금 확인
                {
                    mask |= 1 << index; // 실제 해금 비트 기록
                }
            }
            return mask; // 실제 속성 해금 반환
        }
        public bool TryUseReroll() // 실제 런당 후보 재선택 사용
        {
            if (!CanReroll) // 실제 성장과 사용 여부 확인
            {
                return false; // 재선택 차단
            }
            RerollUsed = true; // 실제 런당 사용 기록
            return true; // 실제 사용 완료
        }
        public static int Level(MetaNode node) // 실제 활성 성장 단계
        {
            return Instance == null || Instance.Service == null ? 0 : Instance.Service.GetLevel(node); // 저장 부재 시 기존 능력 유지
        }
        public bool IsMapUnlocked(int index) // 실제 시작 맵 해금
        {
            return index == 0 || Service != null && Service.IsAvailable && Service.IsMapUnlocked(index); // 기본 맵과 저장 기준 해금
        }
        public bool IsCharacterUnlocked(string id) // 실제 시작 캐릭터 해금
        {
            return unlockConfig == null || id != unlockConfig.FirstBossCharacterId || Service != null && Service.IsAvailable && Service.IsCharacterUnlocked(0); // 미정 대상 외 기존 캐릭터 유지
        }
        public bool IsTurretUnlocked(WeaponData weapon) // 실제 보상 포탑 해금
        {
            int attribute = unlockConfig == null || weapon == null ? -1 : unlockConfig.GetTurretAttribute(weapon.Id); // 실제 임시 해금 대상 조회
            return attribute < 0 || Service != null && Service.IsAvailable && Service.IsTurretAttributeUnlocked(attribute); // 미정 대상 외 기존 포탑 유지
        }
    }
}
