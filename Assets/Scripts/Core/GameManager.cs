using UnityEngine; // Unity 엔진 기능 연결
using ProjectEpsilon.Player; // 플레이어 체력 참조
using ProjectEpsilon.Progression; // 시작 무기 선택 참조
using ProjectEpsilon.Combat; // 최종 보스 대상 참조
using ProjectEpsilon.UI; // 실제 화면 흐름
using ProjectEpsilon.Recording; // 선택적 전투 기록
using ProjectEpsilon.Audio; // 실제 전투 효과음

namespace ProjectEpsilon.Core // 프로젝트 기능 영역
{
    public sealed class GameManager : MonoBehaviour // 게임·관리자 자료 구조
    {
        public static GameManager Instance { get; private set; } // 게임·관리자 구성 선언

        [SerializeField] private GameState currentState = GameState.Starting; // 필드·게임 설정

        private float gameplayTimeScale = 1f; // 플레이 시간 배율
        private SnakeHealth playerHealth; // 몸통 소진 감시 대상
        private WeaponTarget finalBoss; // 등록된 최종 보스
        private bool finalBossEncounterStarted; // 보스전 진입 여부
        private bool actualBossDefeated; // 등록된 보스 실제 처치 여부
        private RunTelemetry telemetry; // 선택적 전투 기록기

        public GameState CurrentState => currentState; // 게임·상태 값 반환
        public bool IsPlaying => currentState == GameState.Playing; // 현재·상태 값 반환
        public bool IsPaused => currentState == GameState.Paused; // 현재·상태 값 반환
        public bool IsFinalBossPhase => finalBossEncounterStarted; // 보스전 상태 조회

        private void Awake() // 초기화 기능 선언
        {
            // 중복 관리자 제거
            if (Instance != null && Instance != this) // 인스턴스 조건 확인
            {
                Destroy(gameObject); // 제거·게임 기능 실행
                return; // 현재 기능 종료
            }

            // 전역 관리자 등록
            Instance = this; // 인스턴스 값 설정
            if (GetComponent<GameplaySoundController>() == null) // 효과음 제어기 중복 확인
            { // 조건 시작
                gameObject.AddComponent<GameplaySoundController>(); // 실제 전투 효과음 연결
            } // 조건 종료
            if (GetComponent<PlayerCombatVfx>() == null) // 플레이어 시각 효과 중복 확인
            { // 조건 시작
                gameObject.AddComponent<PlayerCombatVfx>(); // 실제 플레이어 시각 효과 연결
            } // 조건 종료
            if (RunTelemetry.IsRequested()) // 전투 기록 실행 옵션 확인
            {
                telemetry = gameObject.AddComponent<RunTelemetry>(); // 실제 로컬 기록기 생성
            }
            DontDestroyOnLoad(gameObject); // 제거·불러오기 기능 실행
            if (FindFirstObjectByType<StartingWeaponSelection>(FindObjectsInactive.Include) != null) // 실제 출격 선택 씬 확인
            {
                gameObject.AddComponent<RunFrontendController>(); // 실제 메뉴 화면 구성
            }
            ApplyTimeScale(); // 적용·시간 기능 실행
        }

        private void Start() // 시작 기능 선언
        {
            if (currentState == GameState.Starting && UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == SceneFrontendContext.GamePath && !SceneFrontendContext.HasPendingStart && !AllowsDirectBatchGame()) // 준비 화면을 거치지 않은 게임 씬 직접 실행 확인
            {
                GameObject loader = new GameObject("GameEntryReloader"); // 실제 메인 메뉴 이동 보조 객체
                loader.AddComponent<RunSceneReloader>().Begin(gameObject, SceneFrontendContext.MainMenuPath, true); // 전투 씬 가림과 기존 관리자 제거
                return; // 직접 실행의 단일 씬 메뉴 차단
            }
            HUDController hud = FindFirstObjectByType<HUDController>(); // 실제 전투 HUD 조회
            if (hud != null && hud.GetComponent<Canvas>() != null && hud.GetComponent<BodySidebarController>() == null) // 실제 전투 캔버스와 패널 확인
            {
                hud.gameObject.AddComponent<BodySidebarController>(); // 실제 몸통 정보 패널 연결
            }
            playerHealth = FindFirstObjectByType<SnakeHealth>(); // 플레이어 체력 조회
            if (playerHealth != null) // 체력 존재 확인
            {
                playerHealth.BodyDepleted += HandleBodyDepleted; // 몸통 소진 연결
            }
            // 초기 플레이 상태 진입
            if (currentState == GameState.Starting) // 시작 상태 확인
            {
                StartingWeaponSelection selection = FindFirstObjectByType<StartingWeaponSelection>(FindObjectsInactive.Include); // 비활성 선택기를 포함한 조회
                if (selection == null) // 선택기 없는 기존 씬 확인
                {
                    StartGame(); // 기존 씬 자동 시작
                }
            }
        }

        private static bool AllowsDirectBatchGame() // 내부 배치의 게임 씬 직접 검증 확인
        {
            return Application.isBatchMode && System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-epsilonDirectGame") >= 0; // 명시적 배치 시험 옵션만 허용
        }

        private void OnDestroy() // 제거 기능 선언
        {
            if (finalBoss != null) // 보스 연결 확인
            {
                finalBoss.Died -= HandleFinalBossDied; // 보스 사망 연결 해제
            }
            if (playerHealth != null) // 체력 연결 확인
            {
                playerHealth.BodyDepleted -= HandleBodyDepleted; // 소진 이벤트 연결 해제
            }
            // 전역 참조 해제
            if (Instance == this) // 인스턴스 조건 확인
            {
                Instance = null; // 인스턴스 값 설정
            }
        }

        public void StartGame() // 시작·게임 기능 선언
        {
            if (currentState == GameState.Defeated || currentState == GameState.Victory) // 이미 종료된 전투 확인
            {
                return; // 종료된 전투의 재시작 차단
            }
            if (currentState == GameState.Starting) // 실제 새 런 시작 확인
            {
                actualBossDefeated = false; // 새 런의 보스 처치 초기화
                MetaProgressionController.Instance?.BeginRun(); // 실제 성과 집계 시작
                telemetry?.BeginRun(); // 실제 전투 기록과 시드 시작
            }
            finalBossEncounterStarted = false; // 새 전투의 보스전 상태 초기화
            SetState(GameState.Playing); // 설정·상태 기능 실행
        }

        public void PauseGame() // 일시정지·게임 기능 선언
        {
            // 플레이 중 일시정지 허용
            if (currentState == GameState.Playing) // 현재·상태 조건 확인
            {
                SetState(GameState.Paused); // 설정·상태 기능 실행
            }
        }

        public void ResumeGame() // 재개·게임 기능 선언
        {
            // 일시정지 상태에서 플레이 복귀
            if (currentState == GameState.Paused) // 현재·상태 조건 확인
            {
                SetState(GameState.Playing); // 설정·상태 기능 실행
            }
        }

        public void SetState(GameState nextState) // 설정·상태 기능 선언
        {
            if ((currentState == GameState.Defeated || currentState == GameState.Victory) && nextState != currentState) // 이미 확정된 종료 결과 확인
            {
                return; // 종료 이후 결과 변경 차단
            }
            if ((nextState == GameState.Defeated || nextState == GameState.Victory) && currentState != GameState.Defeated && currentState != GameState.Victory) // 실제 첫 종료 상태 확인
            {
                MetaProgressionController.Instance?.CompleteRun(actualBossDefeated && nextState == GameState.Victory); // 실제 성과 저장과 중복 지급 방지
                telemetry?.Finish(nextState); // 실제 종료 상태와 로컬 기록 저장
            }
            // 게임 상태 변경
            currentState = nextState; // 현재·상태 값 설정
            ApplyTimeScale(); // 적용·시간 기능 실행
        }

        public void SetGameplayTimeScale(float scale) // 플레이 시간 배율 설정
        {
            gameplayTimeScale = Mathf.Clamp(scale, 0.1f, 1f); // 안전 배율 저장
            ApplyTimeScale(); // 현재 상태 반영
        }

        public bool RegisterFinalBoss(WeaponTarget target) // 최종 보스 등록
        {
            if (target == null || !target.IsAlive || currentState == GameState.Defeated || currentState == GameState.Victory) // 등록 가능 상태 확인
            {
                return false; // 등록 거부
            }
            if (finalBoss != null) // 기존 보스 확인
            {
                return finalBoss == target; // 동일 대상 재등록 여부
            }
            finalBoss = target; // 최종 보스 저장
            finalBossEncounterStarted = true; // 보스전 상태 저장
            finalBoss.Died += HandleFinalBossDied; // 실제 사망 이벤트 연결
            return true; // 등록 성공
        }

        private void HandleFinalBossDied(WeaponTarget target) // 등록 보스 사망 처리
        {
            if (target != finalBoss) // 등록 대상 확인
            {
                return; // 다른 대상 무시
            }
            finalBoss.Died -= HandleFinalBossDied; // 중복 승리 연결 해제
            finalBoss = null; // 보스 참조 해제
            if (currentState == GameState.Defeated || currentState == GameState.Victory) // 이미 종료된 전투 확인
            {
                return; // 종료 후 보스 처치 성과 차단
            }
            actualBossDefeated = true; // 등록 보스의 실제 처치 기록
            telemetry?.RecordBossKill(); // 실제 최종 보스 처치 기록
            SetState(GameState.Victory); // 승리 상태 전환
        }

        private void HandleBodyDepleted() // 몸통 소진 처리
        {
            SetState(GameState.Defeated); // 패배 상태 전환
        }

        public void AbandonRun() // 실제 진행 중 런 포기
        {
            if (currentState == GameState.Playing || currentState == GameState.Paused) // 실제 포기 가능 상태 확인
            {
                SetState(GameState.Defeated); // 기존 패배 화면과 실제 보상 처리
            }
        }

        public bool RestartRun() // 실제 종료 후 현재 씬 재시작
        {
            return RestartToScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path); // 기존 현재 씬 재시작 유지
        }

        public bool RestartToScene(string scenePath) // 실제 저장 후 지정 씬으로 복귀
        {
            if (string.IsNullOrEmpty(scenePath) || currentState != GameState.Defeated && currentState != GameState.Victory) // 실제 대상과 종료 상태 확인
            {
                return false; // 진행 중 재시작 방지
            }
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 저장 관리자
            if (meta != null && meta.HasPendingSave && !meta.CompleteRun(false)) // 실패한 보상의 재저장 확인
            {
                return false; // 저장 실패 시 성과 보존
            }
            GameObject loader = new GameObject("RunSceneReloader"); // 실제 재시작 보조 객체
            loader.AddComponent<RunSceneReloader>().Begin(gameObject, scenePath); // 관리자 제거 후 실제 지정 씬 이동
            return true; // 재시작 예약 완료
        }

        private void ApplyTimeScale() // 적용·시간 기능 선언
        {
            // 일시정지 시간 배율 적용
            Time.timeScale = currentState == GameState.Starting || currentState == GameState.Paused || currentState == GameState.Defeated || currentState == GameState.Victory ? 0f : gameplayTimeScale; // 종료 및 정지 상태 시간 배율
        }
    }
}
