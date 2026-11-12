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
            if (!runActive || finished) // 실제 런 상태 확인
            {
                return; // 종료된 런 집계 차단
            }
            kills = kills == int.MaxValue ? kills : kills + 1; // 안전한 적 처치 누적
            if (elite && elites < int.MaxValue) // 실제 엘리트 여부 확인
            {
                elites++; // 실제 엘리트 처치 누적
            }
        }
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
                LastUnlockNotice += "폐허 회랑  "; // 실제 폐허 해금 표시
            }
            if (!hadSwamp && Service.IsMapUnlocked(2)) // 새 습지 해금 확인
            {
                LastUnlockNotice += "변이 습지  "; // 실제 습지 해금 표시
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
