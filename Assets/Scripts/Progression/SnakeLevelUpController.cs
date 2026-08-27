using System.Collections.Generic;
using ProjectEpsilon.Combat;
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
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private WeaponRewardPool rewardPool;
        [SerializeField] private LevelUpPanelController levelUpPanel;

        private readonly List<WeaponRewardCandidate> currentCandidates =
            new List<WeaponRewardCandidate>();

        private bool subscribed;
        private bool presentingLevelUp;
        private bool ownsGamePause;

        public bool IsPresentingLevelUp =>
            presentingLevelUp;

        public IReadOnlyList<WeaponRewardCandidate>
            CurrentCandidates =>
                currentCandidates;

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
            currentCandidates.Clear();
        }

        public void Configure(
            SnakeExperience experienceSource,
            SnakeBodyManager manager,
            SnakeHealth snakeHealth,
            SnakeWeaponManager weapons,
            WeaponRewardPool pool,
            LevelUpPanelController panel
        )
        {
            Unsubscribe();

            experience = experienceSource;
            bodyManager = manager;
            health = snakeHealth;
            weaponManager = weapons;
            rewardPool = pool;
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

            levelUpPanel.CandidateSelected +=
                HandleCandidateSelected;

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
                levelUpPanel.CandidateSelected -=
                    HandleCandidateSelected;
            }

            subscribed = false;
        }

        private void HandleLevelUpRequested(
            int level
        )
        {
            if (presentingLevelUp)
            {
                return;
            }

            ApplyLevelGrowth(
                out bool bodyGrew,
                out bool healthRestored
            );

            BuildCurrentCandidates(
                level
            );

            if (currentCandidates.Count <= 0)
            {
                Debug.LogWarning(
                    "[Project Epsilon] 사용 가능한 무기 후보가 없어 레벨업 보상을 건너뜁니다."
                );

                experience?.CompletePendingLevelUp();
                ResumeIfFinished();
                return;
            }

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

            levelUpPanel.Show(
                level,
                bodyGrew,
                bodyManager == null
                    ? 0
                    : bodyManager.CurrentBodyCount,
                bodyManager == null
                    ? 20
                    : bodyManager.MaximumBodyCount,
                healthRestored,
                currentCandidates
            );
        }

        private void HandleCandidateSelected(
            int candidateIndex
        )
        {
            if (!presentingLevelUp ||
                candidateIndex < 0 ||
                candidateIndex >=
                    currentCandidates.Count)
            {
                return;
            }

            WeaponRewardCandidate candidate =
                currentCandidates[
                    candidateIndex
                ];

            if (!candidate.IsValid ||
                weaponManager == null)
            {
                return;
            }

            bool acquired =
                weaponManager.AcquireWeapon(
                    candidate.Weapon,
                    candidate.Grade
                );

            if (!acquired)
            {
                Debug.LogWarning(
                    "[Project Epsilon] 선택한 무기를 장착하지 못했습니다."
                );

                return;
            }

            presentingLevelUp = false;
            currentCandidates.Clear();

            if (levelUpPanel != null)
            {
                levelUpPanel.Hide();
            }

            experience?.CompletePendingLevelUp();
            ResumeIfFinished();
        }

        private void BuildCurrentCandidates(
            int level
        )
        {
            currentCandidates.Clear();

            if (rewardPool == null)
            {
                return;
            }

            List<WeaponRewardCandidate> generated =
                rewardPool.BuildCandidates(
                    level,
                    weaponManager,
                    3
                );

            currentCandidates.AddRange(
                generated
            );
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
                    bodyManager.TryGainBodyFromLevelUp();

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
