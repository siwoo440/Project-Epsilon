using UnityEngine;

namespace ProjectEpsilon.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState currentState = GameState.Starting;

        public GameState CurrentState => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;

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
            ApplyTimeScale();
        }

        private void Start()
        {
            // 초기 플레이 상태 진입
            if (currentState == GameState.Starting)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            // 전역 참조 해제
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartGame()
        {
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
            // 게임 상태 변경
            currentState = nextState;
            ApplyTimeScale();
        }

        private void ApplyTimeScale()
        {
            // 일시정지 시간 배율 적용
            Time.timeScale = currentState == GameState.Paused ? 0f : 1f;
        }
    }
}
