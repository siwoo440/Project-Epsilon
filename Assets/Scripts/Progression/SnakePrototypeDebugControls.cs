using ProjectEpsilon.Combat;
using ProjectEpsilon.Core;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Progression
{
    public sealed class SnakePrototypeDebugControls : MonoBehaviour
    {
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private WeaponData debugWeapon;
        [SerializeField] private Sprite pickupSprite;

        public void Configure(
            SnakeHealth snakeHealth,
            SnakeBodyManager manager,
            SnakeWeaponManager weapons,
            WeaponData gradeTestWeapon,
            Sprite visual
        )
        {
            health = snakeHealth;
            bodyManager = manager;
            weaponManager = weapons;
            debugWeapon = gradeTestWeapon;
            pickupSprite = visual;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.hKey.wasPressedThisFrame)
            {
                SpawnRecovery(
                    RecoveryPickupType.Heal
                );
            }

            if (keyboard.jKey.wasPressedThisFrame)
            {
                SpawnRecovery(
                    RecoveryPickupType.BodyRepair
                );
            }

            if (!keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed) // 등급 디버그 조합 확인
            { // 조건 시작
                return; // 숫자 단독 입력 보호
            } // 조건 끝

            SnakeWeaponMergeController mergeController = GetComponent<SnakeWeaponMergeController>(); // 합성 화면 조회
            if (mergeController != null && mergeController.IsMergeOpen) // 합성 선택 중 확인
            { // 조건 시작
                return; // 합성 선택 입력 보호
            } // 조건 끝

            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SetDebugGrade(3);
            }

            if (keyboard.digit5Key.wasPressedThisFrame)
            {
                SetDebugGrade(5);
            }
        }

        private void SpawnRecovery(
            RecoveryPickupType type
        )
        {
            RecoveryPickup.Spawn(
                transform.position +
                transform.right *
                (type ==
                RecoveryPickupType.Heal
                    ? 0.8f
                    : -0.8f),
                transform,
                health,
                bodyManager,
                type,
                type ==
                RecoveryPickupType.Heal
                    ? 15
                    : 1,
                pickupSprite
            );
        }

        private void SetDebugGrade(
            int grade
        )
        {
            if (weaponManager == null ||
                debugWeapon == null ||
                weaponManager.SlotCount <= 0)
            {
                return;
            }

            weaponManager.TryEquipAt(
                0,
                debugWeapon,
                grade
            );
        }
    }
}
