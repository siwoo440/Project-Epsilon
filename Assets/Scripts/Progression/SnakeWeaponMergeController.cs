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

        [Range(0.1f, 1f)]
        [SerializeField] private float mergeMoveMultiplier = 0.7f;

        [Range(1, 3)]
        [SerializeField] private int maximumDisplayedCandidates = 3;

        private readonly List<WeaponMergeCandidate> currentCandidates =
            new List<WeaponMergeCandidate>();

        private bool subscribed;
        private bool mergeOpen;

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
            currentCandidates.Clear();

            ApplyMergeMovementState(
                false
            );
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                if (mergeOpen)
                {
                    CloseMergePanel();
                }

                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null ||
                !keyboard.mKey.wasPressedThisFrame)
            {
                return;
            }

            if (mergeOpen)
            {
                CloseMergePanel();
                return;
            }

            OpenMergePanel();
        }

        public void Configure(
            SnakeWeaponManager manager,
            SnakeMovement movementController,
            SnakeStamina staminaController,
            WeaponMergePanelController panel,
            float moveMultiplier
        )
        {
            Unsubscribe();

            weaponManager = manager;
            movement = movementController;
            stamina = staminaController;
            mergePanel = panel;
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
            BuildMergeCandidates();

            if (currentCandidates.Count <= 0 ||
                mergePanel == null)
            {
                return false;
            }

            mergeOpen = true;

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
            currentCandidates.Clear();

            if (mergePanel != null)
            {
                mergePanel.Hide();
            }

            ApplyMergeMovementState(
                false
            );
        }

        private void HandleCandidateSelected(
            int candidateIndex
        )
        {
            if (!mergeOpen ||
                candidateIndex < 0 ||
                candidateIndex >= currentCandidates.Count ||
                weaponManager == null)
            {
                return;
            }

            WeaponMergeCandidate candidate =
                currentCandidates[candidateIndex];

            if (!candidate.IsValid)
            {
                return;
            }

            bool merged =
                weaponManager.TryMergeSlots(
                    candidate.FirstSlotIndex,
                    candidate.SecondSlotIndex
                );

            if (!merged)
            {
                BuildMergeCandidates();

                if (currentCandidates.Count <= 0)
                {
                    CloseMergePanel();
                    return;
                }

                mergePanel?.Show(
                    currentCandidates
                );

                return;
            }

            CloseMergePanel();
        }

        private void HandleCloseRequested()
        {
            CloseMergePanel();
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
                currentCandidates.Count < candidateLimit;
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

                if (candidate.Weapon == weapon &&
                    candidate.CurrentGrade == grade)
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
                first.Weapon == second.Weapon &&
                first.Grade == second.Grade;
        }

        private void ApplyMergeMovementState(
            bool active
        )
        {
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

            subscribed = false;
        }
    }
}
