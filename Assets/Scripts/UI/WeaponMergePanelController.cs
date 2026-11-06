using System;
using System.Collections.Generic;
using ProjectEpsilon.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class WeaponMergePanelController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text stateText;

        [SerializeField] private Button candidateButton01;
        [SerializeField] private Button candidateButton02;
        [SerializeField] private Button candidateButton03;

        [SerializeField] private Text candidateLabel01;
        [SerializeField] private Text candidateLabel02;
        [SerializeField] private Text candidateLabel03;

        [SerializeField] private Button closeButton;

        private bool buttonsBound;

        public event Action<int> CandidateSelected;
        public event Action CloseRequested;

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
            Text state,
            Button button01,
            Text label01,
            Button button02,
            Text label02,
            Button button03,
            Text label03,
            Button closeControl
        )
        {
            UnbindButtons();

            titleText = title;
            stateText = state;

            candidateButton01 = button01;
            candidateLabel01 = label01;

            candidateButton02 = button02;
            candidateLabel02 = label02;

            candidateButton03 = button03;
            candidateLabel03 = label03;

            closeButton = closeControl;

            if (Application.isPlaying)
            {
                BindButtons();
            }
        }

        public void Show(
            IReadOnlyList<WeaponMergeCandidate> candidates
        )
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text =
                    "WEAPON MERGE";
            }

            if (stateText != null)
            {
                stateText.text =
                    "REAL-TIME / AUTO FORWARD / SPEED 70%";
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

        public void ShowUpgradeSelection(WeaponMergeCandidate candidate) // 합성 강화 후보 표시
        { // 메서드 시작
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "SELECT MERGE UPGRADE"; // 강화 선택 제목
            } // 조건 끝
            if (stateText != null) // 상태 문구 연결 확인
            { // 조건 시작
                stateText.text = $"{candidate.Weapon.DisplayName} / ESC: CANCEL"; // 대상과 취소 안내
            } // 조건 끝
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, "DAMAGE +5%"); // 공격력 선택 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, "ATTACK SPEED +5%"); // 공격 속도 선택 표시
            ConfigureUpgradeButton(candidateButton03, candidateLabel03, "RANGE / AREA +5%"); // 사거리 선택 표시
        } // 메서드 끝

        private static void ConfigureUpgradeButton(Button button, Text label, string caption) // 강화 버튼 표시
        { // 메서드 시작
            if (button != null) // 버튼 연결 확인
            { // 조건 시작
                button.gameObject.SetActive(true); // 후보 버튼 표시
                button.interactable = true; // 후보 선택 허용
            } // 조건 끝
            if (label != null) // 문구 연결 확인
            { // 조건 시작
                label.text = caption; // 강화 내용 표시
            } // 조건 끝
        } // 메서드 끝

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(
                    false
                );
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

        private void HandleClose()
        {
            CloseRequested?.Invoke();
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

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(
                    HandleClose
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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(
                    HandleClose
                );
            }

            buttonsBound = false;
        }

        private static void ConfigureCandidate(
            int index,
            Button button,
            Text label,
            IReadOnlyList<WeaponMergeCandidate> candidates
        )
        {
            bool available =
                candidates != null &&
                index >= 0 &&
                index < candidates.Count &&
                candidates[index].IsValid;

            if (button != null)
            {
                button.gameObject.SetActive(
                    available
                );

                button.interactable =
                    available;
            }

            if (!available ||
                label == null)
            {
                return;
            }

            WeaponMergeCandidate candidate =
                candidates[index];

            label.text =
                BuildCandidateLabel(
                    candidate
                );
        }

        private static string BuildCandidateLabel(
            WeaponMergeCandidate candidate
        )
        {
            string currentStars =
                new string(
                    '★',
                    Mathf.Clamp(
                        candidate.CurrentGrade,
                        1,
                        5
                    )
                );

            string resultStars =
                new string(
                    '★',
                    Mathf.Clamp(
                        candidate.ResultGrade,
                        1,
                        5
                    )
                );

            return
                $"{candidate.Weapon.DisplayName}\n" +
                $"{currentStars} + {currentStars}\n" +
                $"→ {resultStars}\n" +
                "2 → 1";
        }
    }
}
