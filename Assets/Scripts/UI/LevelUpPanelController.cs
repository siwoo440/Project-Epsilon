using System;
using System.Collections.Generic;
using ProjectEpsilon.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class LevelUpPanelController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text growthText;

        [Header("Weapon Candidates")]
        [SerializeField] private Button candidateButton01;
        [SerializeField] private Button candidateButton02;
        [SerializeField] private Button candidateButton03;

        [SerializeField] private Text candidateLabel01;
        [SerializeField] private Text candidateLabel02;
        [SerializeField] private Text candidateLabel03;

        private bool buttonsBound;

        public event Action<int> CandidateSelected;

        private void Awake()
        {
            BindButtons();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void Configure(
            Text title,
            Text level,
            Text growth,
            Button button01,
            Text label01,
            Button button02,
            Text label02,
            Button button03,
            Text label03
        )
        {
            UnbindButtons();

            titleText = title;
            levelText = level;
            growthText = growth;

            candidateButton01 = button01;
            candidateLabel01 = label01;

            candidateButton02 = button02;
            candidateLabel02 = label02;

            candidateButton03 = button03;
            candidateLabel03 = label03;

            if (Application.isPlaying)
            {
                BindButtons();
            }
        }

        public void Show(
            int level,
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored,
            IReadOnlyList<WeaponRewardCandidate> candidates
        )
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "LEVEL UP!";
            }

            if (levelText != null)
            {
                levelText.text =
                    $"Lv. {Mathf.Max(1, level)}";
            }

            if (growthText != null)
            {
                growthText.text =
                    BuildGrowthText(
                        bodyGrew,
                        currentBodyCount,
                        maximumBodyCount,
                        healthRestored
                    );
            }

            ConfigureCandidate(
                0,
                candidateButton01,
                candidateLabel01,
                candidates
            );

            ConfigureCandidate(
                1,
                candidateButton02,
                candidateLabel02,
                candidates
            );

            ConfigureCandidate(
                2,
                candidateButton03,
                candidateLabel03,
                candidates
            );

            BindButtons();
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void HandleCandidate01()
        {
            CandidateSelected?.Invoke(0);
        }

        private void HandleCandidate02()
        {
            CandidateSelected?.Invoke(1);
        }

        private void HandleCandidate03()
        {
            CandidateSelected?.Invoke(2);
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (candidateButton01 != null)
            {
                candidateButton01.onClick.AddListener(
                    HandleCandidate01
                );
            }

            if (candidateButton02 != null)
            {
                candidateButton02.onClick.AddListener(
                    HandleCandidate02
                );
            }

            if (candidateButton03 != null)
            {
                candidateButton03.onClick.AddListener(
                    HandleCandidate03
                );
            }

            buttonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!buttonsBound)
            {
                return;
            }

            if (candidateButton01 != null)
            {
                candidateButton01.onClick.RemoveListener(
                    HandleCandidate01
                );
            }

            if (candidateButton02 != null)
            {
                candidateButton02.onClick.RemoveListener(
                    HandleCandidate02
                );
            }

            if (candidateButton03 != null)
            {
                candidateButton03.onClick.RemoveListener(
                    HandleCandidate03
                );
            }

            buttonsBound = false;
        }

        private static void ConfigureCandidate(
            int candidateIndex,
            Button button,
            Text label,
            IReadOnlyList<WeaponRewardCandidate> candidates
        )
        {
            bool hasCandidate =
                candidates != null &&
                candidateIndex >= 0 &&
                candidateIndex < candidates.Count &&
                candidates[candidateIndex].IsValid;

            if (button != null)
            {
                button.gameObject.SetActive(
                    hasCandidate
                );

                button.interactable =
                    hasCandidate;
            }

            if (!hasCandidate || label == null)
            {
                return;
            }

            WeaponRewardCandidate candidate =
                candidates[candidateIndex];

            label.text =
                BuildCandidateLabel(
                    candidate
                );
        }

        private static string BuildCandidateLabel(
            WeaponRewardCandidate candidate
        )
        {
            if (!candidate.IsValid)
            {
                return "-";
            }

            string stars =
                new string(
                    '★',
                    Mathf.Clamp(
                        candidate.Grade,
                        1,
                        5
                    )
                );

            return
                $"{candidate.Weapon.DisplayName} {stars}\n" +
                $"Attribute: {candidate.Weapon.Attribute}\n" + // 무기 속성 표시
                $"{candidate.Weapon.AttackType}\n" +
                $"DMG {candidate.Weapon.BaseDamage:0.#}";
        }

        private static string BuildGrowthText(
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored
        )
        {
            int safeCurrent =
                Mathf.Max(
                    0,
                    currentBodyCount
                );

            int safeMaximum =
                Mathf.Max(
                    1,
                    maximumBodyCount
                );

            if (bodyGrew)
            {
                int previous =
                    Mathf.Max(
                        0,
                        safeCurrent - 1
                    );

                return
                    $"Body {previous} → {safeCurrent} / {safeMaximum}\n" +
                    "Choose 1 Weapon";
            }

            if (healthRestored)
            {
                return
                    $"Body MAX {safeMaximum} / {safeMaximum}\n" +
                    "HP FULL RESTORE + Choose 1 Weapon";
            }

            return
                $"Choose 1 Weapon";
        }
    }
}

