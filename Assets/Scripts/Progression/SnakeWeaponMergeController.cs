using System.Collections.Generic;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Core;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using ProjectEpsilon.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(150)]
    public sealed class SnakeWeaponMergeController : MonoBehaviour
    {
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private SnakeMovement movement;
        [SerializeField] private SnakeStamina stamina;
        [SerializeField] private WeaponMergePanelController mergePanel;

        [Header("UI Priority")]
        [SerializeField] private SnakeExperience experience;
        [SerializeField] private SnakeLevelUpController levelUpController;

        [Range(0.1f, 1f)]
        [SerializeField] private float mergeMoveMultiplier = 0.7f;

        [Range(1, 3)]
        [SerializeField] private int maximumDisplayedCandidates = 3;

        private readonly List<WeaponMergeCandidate> currentCandidates =
            new List<WeaponMergeCandidate>();

        private bool subscribed;
        private bool mergeOpen;
        private bool pauseOwnedByInput; // 직접 일시정지 상태
        private bool selectingUpgrade; // 강화 선택 단계
        private WeaponMergeCandidate selectedMerge; // 미확정 합성 대상

        public bool IsMergeOpen => mergeOpen;

        public IReadOnlyList<WeaponMergeCandidate>
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

            if (mergePanel != null)
            {
                mergePanel.Hide();
            }

            ApplyMergeMovementState(
                false
            );
        }

        private void OnDisable()
        {
            Unsubscribe();

            mergeOpen = false;
            selectingUpgrade = false; // 미확정 강화 취소
            currentCandidates.Clear();

            ApplyMergeMovementState(
                false
            );
        }

        private void Update() // 합성 및 일시정지 입력 처리
        { // 메서드 시작
            if (IsLevelUpBlocking()) // 레벨업 선택 우선
            { // 조건 시작
                if (mergeOpen) // 열린 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                } // 조건 끝
                return; // 레벨업 중 입력 차단
            } // 조건 끝
            if (levelUpController != null && levelUpController.ConsumedEscapeThisFrame) // 레벨업 취소 입력 소비 확인
            { // 조건 시작
                return; // 같은 ESC로 일시정지 방지
            } // 조건 끝
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회
            if (keyboard == null) // 키보드 부재 확인
            { // 조건 시작
                return; // 입력 처리 종료
            } // 조건 끝
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (InputBindingManager.Instance != null && InputBindingManager.Instance.IsRebinding) // 키 재설정 중 확인
            { // 조건 시작
                return; // 재설정 입력 우선
            } // 조건 끝
            if (keyboard.escapeKey.wasPressedThisFrame) // ESC 입력 확인
            { // 조건 시작
                if (mergeOpen) // 합성 우선 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                    return; // 일시정지 방지
                } // 조건 끝
                if (manager != null && manager.IsPlaying) // 플레이 상태 확인
                { // 조건 시작
                    manager.PauseGame(); // 게임 일시정지
                    pauseOwnedByInput = true; // 직접 일시정지 기록
                } // 조건 끝
                else if (manager != null && manager.IsPaused && pauseOwnedByInput) // 직접 정지 상태 확인
                { // 조건 시작
                    manager.ResumeGame(); // 게임 재개
                    pauseOwnedByInput = false; // 직접 일시정지 해제
                } // 조건 끝
                return; // 중복 입력 방지
            } // 조건 끝
            if (manager != null && !manager.IsPlaying) // 플레이 중 여부 확인
            { // 조건 시작
                if (mergeOpen) // 미확정 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 플레이 종료 시 합성 취소
                } // 조건 끝
                return; // 정지 중 합성 차단
            } // 조건 끝
            if (keyboard.eKey.wasPressedThisFrame) // 합성 키 입력 확인
            { // 조건 시작
                if (mergeOpen) // 열린 합성 확인
                { // 조건 시작
                    CloseMergePanel(); // 합성 취소
                } // 조건 끝
                else // 닫힌 합성 확인
                { // 대안 시작
                    OpenMergePanel(); // 합성 화면 열기
                } // 대안 끝
                return; // 같은 프레임 후보 선택 방지
            } // 조건 끝
            if (!mergeOpen) // 합성 화면 상태 확인
            { // 조건 시작
                return; // 합성 선택 차단
            } // 조건 끝
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) // 첫 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(0); // 첫 합성 후보 선택
            } // 조건 끝
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) // 둘째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(1); // 둘째 합성 후보 선택
            } // 조건 끝
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) // 셋째 선택 키 확인
            { // 조건 시작
                HandleCandidateSelected(2); // 셋째 합성 후보 선택
            } // 조건 끝
        } // 메서드 끝

        public void Configure(
            SnakeWeaponManager manager,
            SnakeMovement movementController,
            SnakeStamina staminaController,
            WeaponMergePanelController panel,
            SnakeExperience experienceSource,
            SnakeLevelUpController levelController,
            float moveMultiplier
        )
        {
            Unsubscribe();

            weaponManager = manager;
            movement = movementController;
            stamina = staminaController;
            mergePanel = panel;
            experience = experienceSource;
            levelUpController = levelController;

            mergeMoveMultiplier =
                Mathf.Clamp(
                    moveMultiplier,
                    0.1f,
                    1f
                );

            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        public bool OpenMergePanel()
        {
            if (IsLevelUpBlocking())
            {
                return false;
            }

            BuildMergeCandidates();

            if (currentCandidates.Count <= 0 ||
                mergePanel == null)
            {
                return false;
            }

            mergeOpen = true;
            selectingUpgrade = false; // 강화 선택 초기화

            ApplyMergeMovementState(
                true
            );

            mergePanel.Show(
                currentCandidates
            );

            return true;
        }

        public void CloseMergePanel()
        {
            mergeOpen = false;
            selectingUpgrade = false; // 미확정 강화 취소
            currentCandidates.Clear();

            if (mergePanel != null)
            {
                mergePanel.Hide();
            }

            ApplyMergeMovementState(
                false
            );
        }

        private void HandleCandidateSelected(int candidateIndex) // 합성 대상 및 강화 선택
        { // 메서드 시작
            if (!mergeOpen || weaponManager == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying)) // 열린 화면과 플레이 상태 확인
            { // 조건 시작
                return; // 선택 무시
            } // 조건 끝
            if (!selectingUpgrade) // 합성 대상 선택 단계
            { // 조건 시작
                if (candidateIndex < 0 || candidateIndex >= currentCandidates.Count) // 후보 범위 확인
                { // 조건 시작
                    return; // 잘못된 후보 무시
                } // 조건 끝
                selectedMerge = currentCandidates[candidateIndex]; // 미확정 대상 저장
                if (!selectedMerge.IsValid) // 후보 유효성 확인
                { // 조건 시작
                    return; // 잘못된 대상 무시
                } // 조건 끝
                selectingUpgrade = true; // 강화 선택 단계 진입
                mergePanel.ShowUpgradeSelection(selectedMerge); // 공통 강화 후보 표시
                return; // 합성 확정 대기
            } // 조건 끝
            if (candidateIndex < 0 || candidateIndex > 2) // 강화 후보 범위 확인
            { // 조건 시작
                return; // 잘못된 강화 무시
            } // 조건 끝
            IReadOnlyList<SnakeWeaponSlot> slots = weaponManager.Slots; // 현재 슬롯 목록 조회
            if (selectedMerge.FirstSlotIndex >= slots.Count || selectedMerge.SecondSlotIndex >= slots.Count || // 슬롯 존재 확인
                slots[selectedMerge.FirstSlotIndex].Weapon != selectedMerge.Weapon || // 첫 무기 변경 확인
                slots[selectedMerge.SecondSlotIndex].Weapon != selectedMerge.Weapon || // 둘째 무기 변경 확인
                slots[selectedMerge.FirstSlotIndex].Grade != selectedMerge.CurrentGrade || // 첫 등급 변경 확인
                slots[selectedMerge.SecondSlotIndex].Grade != selectedMerge.CurrentGrade) // 둘째 등급 변경 확인
            { // 조건 시작
                CloseMergePanel(); // 대상 변경 시 합성 취소
                return; // 잘못된 합성 방지
            } // 조건 끝
            WeaponMergeUpgrade upgrade = (WeaponMergeUpgrade)(candidateIndex + 1); // 선택 강화 변환
            bool merged = weaponManager.TryMergeSlots(selectedMerge.FirstSlotIndex, selectedMerge.SecondSlotIndex, upgrade); // 합성과 강화 확정
            if (!merged) // 합성 실패 확인
            { // 조건 시작
                CloseMergePanel(); // 미확정 합성 취소
                return; // 실패 처리 종료
            } // 조건 끝
            CloseMergePanel(); // 합성 화면 닫기
        } // 메서드 끝

        private void HandleCloseRequested()
        {
            CloseMergePanel();
        }

        private void HandleLevelUpRequested(
            int level
        )
        {
            if (mergeOpen)
            {
                CloseMergePanel();
            }
        }

        private bool IsLevelUpBlocking()
        {
            return
                (experience != null &&
                experience.IsLevelUpPending) ||
                (levelUpController != null &&
                levelUpController.IsPresentingLevelUp);
        }

        private void BuildMergeCandidates()
        {
            currentCandidates.Clear();

            if (weaponManager == null)
            {
                return;
            }

            IReadOnlyList<SnakeWeaponSlot> slots =
                weaponManager.Slots;

            int candidateLimit =
                Mathf.Clamp(
                    maximumDisplayedCandidates,
                    1,
                    3
                );

            for (int firstIndex = 0;
                firstIndex < slots.Count &&
                currentCandidates.Count <
                    candidateLimit;
                firstIndex++)
            {
                SnakeWeaponSlot first =
                    slots[firstIndex];

                if (!CanMergeSlot(first) ||
                    ContainsWeaponGrade(
                        first.Weapon,
                        first.Grade
                    ))
                {
                    continue;
                }

                for (int secondIndex =
                    firstIndex + 1;
                    secondIndex < slots.Count;
                    secondIndex++)
                {
                    SnakeWeaponSlot second =
                        slots[secondIndex];

                    if (!CanMergePair(
                        first,
                        second
                    ))
                    {
                        continue;
                    }

                    currentCandidates.Add(
                        new WeaponMergeCandidate(
                            first.Weapon,
                            first.Grade,
                            firstIndex,
                            secondIndex
                        )
                    );

                    break;
                }
            }
        }

        private bool ContainsWeaponGrade(
            WeaponData weapon,
            int grade
        )
        {
            for (int index = 0;
                index < currentCandidates.Count;
                index++)
            {
                WeaponMergeCandidate candidate =
                    currentCandidates[index];

                if (candidate.Weapon ==
                    weapon &&
                    candidate.CurrentGrade ==
                    grade)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CanMergeSlot(
            SnakeWeaponSlot slot
        )
        {
            return
                slot != null &&
                !slot.IsEmpty &&
                slot.Weapon != null &&
                slot.Grade >= 1 &&
                slot.Grade < 5 &&
                slot.Grade <
                    slot.Weapon.MaxGrade;
        }

        private static bool CanMergePair(
            SnakeWeaponSlot first,
            SnakeWeaponSlot second
        )
        {
            return
                CanMergeSlot(first) &&
                CanMergeSlot(second) &&
                first.Weapon ==
                    second.Weapon &&
                first.Grade ==
                    second.Grade;
        }

        private void ApplyMergeMovementState(
            bool active
        )
        {
            GameManager manager = GameManager.Instance; // 게임 관리자 조회
            if (manager != null) // 관리자 연결 확인
            { // 조건 시작
                manager.SetGameplayTimeScale(active ? mergeMoveMultiplier : 1f); // 전체 게임 속도 적용
            } // 조건 끝
            if (movement != null)
            {
                movement.SetMergeMovementMode(
                    active,
                    mergeMoveMultiplier
                );
            }

            if (stamina != null)
            {
                stamina.SetBoostBlocked(
                    active
                );
            }
        }

        private void Subscribe()
        {
            if (subscribed ||
                mergePanel == null)
            {
                return;
            }

            mergePanel.CandidateSelected +=
                HandleCandidateSelected;

            mergePanel.CloseRequested +=
                HandleCloseRequested;

            if (experience != null)
            {
                experience.LevelUpRequested +=
                    HandleLevelUpRequested;
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                return;
            }

            if (mergePanel != null)
            {
                mergePanel.CandidateSelected -=
                    HandleCandidateSelected;

                mergePanel.CloseRequested -=
                    HandleCloseRequested;
            }

            if (experience != null)
            {
                experience.LevelUpRequested -=
                    HandleLevelUpRequested;
            }

            subscribed = false;
        }
    }
}
