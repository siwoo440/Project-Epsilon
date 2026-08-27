using ProjectEpsilon.Core;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(140)]
    public sealed class SnakeLevelUpController : MonoBehaviour
    {
        [SerializeField] private SnakeExperience experience;
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeHealth health;
        [SerializeField] private LevelUpPanelController levelUpPanel;

        private bool subscribed;
        private bool presentingLevelUp;
        private bool ownsGamePause;

        public bool IsPresentingLevelUp =>
            presentingLevelUp;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start()
        {
            Subscribe();

            if (levelUpPanel != null)
            {
                levelUpPanel.Hide();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();

            if (Application.isPlaying &&
                ownsGamePause &&
                GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }

            ownsGamePause = false;
            presentingLevelUp = false;
        }

        public void Configure(
            SnakeExperience experienceSource,
            SnakeBodyManager manager,
            SnakeHealth snakeHealth,
            LevelUpPanelController panel
        )
        {
            Unsubscribe();

            experience = experienceSource;
            bodyManager = manager;
            health = snakeHealth;
            levelUpPanel = panel;

            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Subscribe()
        {
            if (subscribed ||
                experience == null ||
                levelUpPanel == null)
            {
                return;
            }

            experience.LevelUpRequested +=
                HandleLevelUpRequested;

            levelUpPanel.ContinueRequested +=
                HandleContinueRequested;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (experience != null)
            {
                experience.LevelUpRequested -=
                    HandleLevelUpRequested;
            }

            if (levelUpPanel != null)
            {
                levelUpPanel.ContinueRequested -=
                    HandleContinueRequested;
            }

            subscribed = false;
        }

        private void HandleLevelUpRequested(int level)
        {
            if (presentingLevelUp)
            {
                return;
            }

            ApplyLevelGrowth(
                out bool bodyGrew,
                out bool healthRestored
            );

            presentingLevelUp = true;

            GameManager gameManager =
                GameManager.Instance;

            if (!ownsGamePause &&
                gameManager != null &&
                gameManager.IsPlaying)
            {
                ownsGamePause = true;
                gameManager.PauseGame();
            }

            if (levelUpPanel == null)
            {
                presentingLevelUp = false;
                experience?.CompletePendingLevelUp();
                ResumeIfFinished();
                return;
            }

            levelUpPanel.Show(
                level,
                bodyGrew,
                bodyManager == null
                    ? 0
                    : bodyManager.CurrentBodyCount,
                bodyManager == null
                    ? 20
                    : bodyManager.MaximumBodyCount,
                healthRestored
            );
        }

        private void HandleContinueRequested()
        {
            if (!presentingLevelUp)
            {
                return;
            }

            presentingLevelUp = false;

            if (levelUpPanel != null)
            {
                levelUpPanel.Hide();
            }

            experience?.CompletePendingLevelUp();

            // 초과 XP로 다음 레벨업이 즉시 요청되면
            // 이벤트 처리 중 presentingLevelUp이 다시 true가 된다.
            ResumeIfFinished();
        }

        private void ApplyLevelGrowth(
            out bool bodyGrew,
            out bool healthRestored
        )
        {
            bodyGrew = false;
            healthRestored = false;

            if (bodyManager == null)
            {
                return;
            }

            if (bodyManager.CurrentBodyCount <
                bodyManager.MaximumBodyCount)
            {
                bodyGrew =
                    bodyManager.TryAddBody();

                return;
            }

            if (health != null)
            {
                health.ResetHealth();
                healthRestored = true;
            }
        }

        private void ResumeIfFinished()
        {
            if (presentingLevelUp ||
                !ownsGamePause)
            {
                return;
            }

            GameManager gameManager =
                GameManager.Instance;

            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }

            ownsGamePause = false;
        }
    }
}
