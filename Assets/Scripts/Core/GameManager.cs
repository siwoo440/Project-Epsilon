using UnityEngine;
using ProjectEpsilon.Player; // 플레이어 체력 참조
using ProjectEpsilon.Progression; // 시작 무기 선택 참조
using ProjectEpsilon.Combat; // 최종 보스 대상 참조
using ProjectEpsilon.UI; // 실제 화면 흐름

namespace ProjectEpsilon.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState currentState = GameState.Starting;

        private float gameplayTimeScale = 1f; // 플레이 시간 배율
        private SnakeHealth playerHealth; // 몸통 소진 감시 대상
        private WeaponTarget finalBoss; // 등록된 최종 보스
        private bool finalBossEncounterStarted; // 보스전 진입 여부
        private bool actualBossDefeated; // 등록된 보스 실제 처치 여부

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsFinalBossPhase => finalBossEncounterStarted; // 보스전 상태 조회

        private void Awake()
        {
            // 중복 관리자 제거
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 전역 관리자 등록
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (FindFirstObjectByType<StartingWeaponSelection>(FindObjectsInactive.Include) != null) // 실제 출격 선택 씬 확인
            { // 조건 시작
                gameObject.AddComponent<RunFrontendController>(); // 실제 메뉴 화면 구성
            } // 조건 끝
            ApplyTimeScale();
        }

        private void Start()
        {
            playerHealth = FindFirstObjectByType<SnakeHealth>(); // 플레이어 체력 조회
            if (playerHealth != null) // 체력 존재 확인
            { // 조건 시작
                playerHealth.BodyDepleted += HandleBodyDepleted; // 몸통 소진 연결
            } // 조건 끝
            // 초기 플레이 상태 진입
            if (currentState == GameState.Starting) // 시작 상태 확인
            { // 조건 시작
                StartingWeaponSelection selection = FindFirstObjectByType<StartingWeaponSelection>(FindObjectsInactive.Include); // 비활성 선택기를 포함한 조회
                if (selection == null) // 선택기 없는 기존 씬 확인
                { // 조건 시작
                    StartGame(); // 기존 씬 자동 시작
                } // 조건 끝
            } // 조건 끝
        }

        private void OnDestroy()
        {
            if (finalBoss != null) // 보스 연결 확인
            { // 조건 시작
                finalBoss.Died -= HandleFinalBossDied; // 보스 사망 연결 해제
            } // 조건 끝
            if (playerHealth != null) // 체력 연결 확인
            { // 조건 시작
                playerHealth.BodyDepleted -= HandleBodyDepleted; // 소진 이벤트 연결 해제
            } // 조건 끝
            // 전역 참조 해제
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartGame()
        {
            if (currentState == GameState.Starting) // 실제 새 런 시작 확인
            { // 조건 시작
                actualBossDefeated = false; // 새 런의 보스 처치 초기화
                MetaProgressionController.Instance?.BeginRun(); // 실제 성과 집계 시작
            } // 조건 끝
            finalBossEncounterStarted = false; // 새 전투의 보스전 상태 초기화
            SetState(GameState.Playing);
        }

        public void PauseGame()
        {
            // 플레이 중 일시정지 허용
            if (currentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            // 일시정지 상태에서 플레이 복귀
            if (currentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void SetState(GameState nextState)
        {
            if ((nextState == GameState.Defeated || nextState == GameState.Victory) && currentState != GameState.Defeated && currentState != GameState.Victory) // 실제 첫 종료 상태 확인
            { // 조건 시작
                MetaProgressionController.Instance?.CompleteRun(actualBossDefeated && nextState == GameState.Victory); // 실제 성과 저장과 중복 지급 방지
            } // 조건 끝
            // 게임 상태 변경
            currentState = nextState;
            ApplyTimeScale();
        }

        public void SetGameplayTimeScale(float scale) // 플레이 시간 배율 설정
        { // 메서드 시작
            gameplayTimeScale = Mathf.Clamp(scale, 0.1f, 1f); // 안전 배율 저장
            ApplyTimeScale(); // 현재 상태 반영
        } // 메서드 끝

        public bool RegisterFinalBoss(WeaponTarget target) // 최종 보스 등록
        { // 메서드 시작
            if (target == null || !target.IsAlive || currentState == GameState.Defeated || currentState == GameState.Victory) // 등록 가능 상태 확인
            { // 조건 시작
                return false; // 등록 거부
            } // 조건 끝
            if (finalBoss != null) // 기존 보스 확인
            { // 조건 시작
                return finalBoss == target; // 동일 대상 재등록 여부
            } // 조건 끝
            finalBoss = target; // 최종 보스 저장
            finalBossEncounterStarted = true; // 보스전 상태 저장
            finalBoss.Died += HandleFinalBossDied; // 실제 사망 이벤트 연결
            return true; // 등록 성공
        } // 메서드 끝

        private void HandleFinalBossDied(WeaponTarget target) // 등록 보스 사망 처리
        { // 메서드 시작
            if (target != finalBoss) // 등록 대상 확인
            { // 조건 시작
                return; // 다른 대상 무시
            } // 조건 끝
            finalBoss.Died -= HandleFinalBossDied; // 중복 승리 연결 해제
            actualBossDefeated = true; // 등록 보스의 실제 처치 기록
            finalBoss = null; // 보스 참조 해제
            if (currentState != GameState.Defeated && currentState != GameState.Victory) // 종료 상태 확인
            { // 조건 시작
                SetState(GameState.Victory); // 승리 상태 전환
            } // 조건 끝
        } // 메서드 끝

        private void HandleBodyDepleted() // 몸통 소진 처리
        { // 메서드 시작
            SetState(GameState.Defeated); // 패배 상태 전환
        } // 메서드 끝

        public void AbandonRun() // 실제 진행 중 런 포기
        { // 메서드 시작
            if (currentState == GameState.Playing || currentState == GameState.Paused) // 실제 포기 가능 상태 확인
            { // 조건 시작
                SetState(GameState.Defeated); // 기존 패배 화면과 실제 보상 처리
            } // 조건 끝
        } // 메서드 끝

        public bool RestartRun() // 실제 종료 후 현재 씬 재시작
        { // 메서드 시작
            if (currentState != GameState.Defeated && currentState != GameState.Victory) // 실제 종료 상태 확인
            { // 조건 시작
                return false; // 진행 중 재시작 방지
            } // 조건 끝
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 저장 관리자
            if (meta != null && meta.HasPendingSave && !meta.CompleteRun(false)) // 실패한 보상의 재저장 확인
            { // 조건 시작
                return false; // 저장 실패 시 성과 보존
            } // 조건 끝
            GameObject loader = new GameObject("RunSceneReloader"); // 실제 재시작 보조 객체
            loader.AddComponent<RunSceneReloader>().Begin(gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene().path); // 관리자 제거 후 실제 씬 재로딩
            return true; // 재시작 예약 완료
        } // 메서드 끝

        private void ApplyTimeScale()
        {
            // 일시정지 시간 배율 적용
            Time.timeScale = currentState == GameState.Starting || currentState == GameState.Paused || currentState == GameState.Defeated || currentState == GameState.Victory ? 0f : gameplayTimeScale; // 종료 및 정지 상태 시간 배율
        }
    }
}
