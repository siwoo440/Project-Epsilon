using System;
using System.Collections.Generic;
using ProjectEpsilon.Progression;
using ProjectEpsilon.Combat; // 무기 슬롯 표시
using ProjectEpsilon.Data; // 무기 속성 표시
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

        public void ShowSlotSelection(SnakeWeaponSlot slot, int slotIndex, int cursor, int count, bool first) // 재료 선택 화면
        { // 메서드 시작
            gameObject.SetActive(true); // 합성 화면 표시
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = first ? "SELECT FIRST MATERIAL" : "SELECT SECOND MATERIAL"; // 재료 단계 안내
            } // 조건 끝
            if (stateText != null) // 상태 문구 연결 확인
            { // 조건 시작
                stateText.text = $"{cursor + 1}/{count} | ESC: CANCEL"; // 위치 및 취소 안내
            } // 조건 끝
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, "< PREV"); // 이전 슬롯 버튼
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, FormatSlot(slot, slotIndex)); // 현재 슬롯 버튼
            ConfigureUpgradeButton(candidateButton03, candidateLabel03, "NEXT >"); // 다음 슬롯 버튼
            BindButtons(); // 버튼 연결
        } // 메서드 끝

        public void ShowInheritanceSelection(SnakeWeaponSlot first, int firstIndex, SnakeWeaponSlot second, int secondIndex) // 강화 계승 화면
        { // 메서드 시작
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "CHOOSE ENHANCEMENTS"; // 계승 단계 안내
            } // 조건 끝
            if (stateText != null) // 상태 문구 연결 확인
            { // 조건 시작
                stateText.text = "CHOOSE MATERIAL | ESC: CANCEL"; // 계승 및 취소 안내
            } // 조건 끝
            ConfigureUpgradeButton(candidateButton01, candidateLabel01, FormatSlot(first, firstIndex)); // 첫 재료 강화 표시
            ConfigureUpgradeButton(candidateButton02, candidateLabel02, FormatSlot(second, secondIndex)); // 둘째 재료 강화 표시
            if (candidateButton03 != null) // 남는 버튼 확인
            { // 조건 시작
                candidateButton03.gameObject.SetActive(false); // 남는 버튼 숨김
            } // 조건 끝
        } // 메서드 끝

        private static string FormatSlot(SnakeWeaponSlot slot, int slotIndex) // 재료 정보 작성
        { // 메서드 시작
            return $"BODY {slotIndex + 1}: {slot.Weapon.DisplayName} ★{slot.Grade}\nDMG +{slot.DamageUpgrades * 5}%  SPD +{slot.SpeedUpgrades * 5}%  RNG +{slot.RangeUpgrades * 5}%"; // 슬롯 및 강화 요약
        } // 메서드 끝

        public void ShowSynergyPreview(WeaponAttribute attribute, int count, int resultCount, int stage, int resultStage) // 시너지 변화 표시
        { // 메서드 시작
            if (stateText == null) // 상태 문구 연결 확인
            { // 조건 시작
                return; // 표시 생략
            } // 조건 끝
            string current = stage == 0 ? "OFF" : $"x{stage}"; // 현재 단계 문구
            string result = resultStage == 0 ? "OFF" : $"x{resultStage}"; // 합성 후 단계 문구
            stateText.text += $" | {attribute}: {count} -> {resultCount} | {current} -> {result}"; // 속성 수 및 단계 표시
        } // 메서드 끝

        public void ShowUpgradeSelection(WeaponMergeCandidate candidate) // 합성 강화 후보 표시
        { // 메서드 시작
            if (titleText != null) // 제목 연결 확인
            { // 조건 시작
                titleText.text = "SELECT MERGE UPGRADE"; // 강화 선택 제목
            } // 조건 끝
            if (stateText != null) // 상태 문구 연결 확인
            { // 조건 시작
                stateText.text = "ESC: CANCEL"; // 대상과 취소 안내
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
