using ProjectEpsilon.Combat;
using ProjectEpsilon.Core;
using ProjectEpsilon.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Progression
{
    public sealed class SnakeWeaponMergeDebugControls : MonoBehaviour
    {
        [SerializeField] private SnakeWeaponManager weaponManager;
        [SerializeField] private WeaponData debugWeapon;

        public void Configure(
            SnakeWeaponManager manager,
            WeaponData weapon
        )
        {
            weaponManager = manager;
            debugWeapon = weapon;
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

            if (keyboard == null ||
                !keyboard.nKey.wasPressedThisFrame)
            {
                return;
            }

            PrepareDebugMergePair();
        }

        public bool PrepareDebugMergePair()
        {
            if (weaponManager == null ||
                debugWeapon == null ||
                weaponManager.SlotCount < 2)
            {
                return false;
            }

            bool first =
                weaponManager.TryEquipAt(
                    0,
                    debugWeapon,
                    1
                );

            bool second =
                weaponManager.TryEquipAt(
                    1,
                    debugWeapon,
                    1
                );

            return first && second;
        }
    }
}
