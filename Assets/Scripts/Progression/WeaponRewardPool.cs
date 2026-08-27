using System;
using System.Collections.Generic;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Data;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [Serializable]
    public sealed class WeaponRewardEntry
    {
        [SerializeField] private WeaponData weapon;
        [SerializeField] private bool unlocked = true;

        public WeaponData Weapon => weapon;
        public bool Unlocked => unlocked;

        public WeaponRewardEntry(
            WeaponData weaponData,
            bool isUnlocked
        )
        {
            weapon = weaponData;
            unlocked = isUnlocked;
        }
    }

    [CreateAssetMenu(
        fileName = "WeaponRewardPool",
        menuName = "Project Epsilon/Progression/Weapon Reward Pool"
    )]
    public sealed class WeaponRewardPool : ScriptableObject
    {
        [SerializeField] private List<WeaponRewardEntry> entries =
            new List<WeaponRewardEntry>();

        public IReadOnlyList<WeaponRewardEntry> Entries => entries;

        public void Configure(
            IReadOnlyList<WeaponData> weapons
        )
        {
            entries.Clear();

            if (weapons == null)
            {
                return;
            }

            for (int index = 0; index < weapons.Count; index++)
            {
                WeaponData weapon = weapons[index];

                if (weapon == null)
                {
                    continue;
                }

                entries.Add(
                    new WeaponRewardEntry(
                        weapon,
                        true
                    )
                );
            }
        }

        public List<WeaponRewardCandidate> BuildCandidates(
            int level,
            SnakeWeaponManager weaponManager,
            int requestedCount
        )
        {
            int safeCount =
                Mathf.Max(0, requestedCount);

            List<WeaponRewardEntry> available =
                BuildAvailableEntries(
                    weaponManager
                );

            List<WeaponRewardCandidate> candidates =
                new List<WeaponRewardCandidate>(
                    safeCount
                );

            while (candidates.Count < safeCount &&
                available.Count > 0)
            {
                int selectedIndex =
                    UnityEngine.Random.Range(
                        0,
                        available.Count
                    );

                WeaponRewardEntry selected =
                    available[selectedIndex];

                available.RemoveAt(selectedIndex);

                WeaponData weapon =
                    selected.Weapon;

                int grade =
                    WeaponRewardRules.RollGrade(
                        level,
                        weapon.MaxGrade,
                        UnityEngine.Random.value
                    );

                candidates.Add(
                    new WeaponRewardCandidate(
                        weapon,
                        grade
                    )
                );
            }

            return candidates;
        }

        private List<WeaponRewardEntry> BuildAvailableEntries(
            SnakeWeaponManager weaponManager
        )
        {
            List<WeaponRewardEntry> result =
                new List<WeaponRewardEntry>();

            for (int index = 0; index < entries.Count; index++)
            {
                WeaponRewardEntry entry =
                    entries[index];

                if (entry == null ||
                    !entry.Unlocked ||
                    entry.Weapon == null)
                {
                    continue;
                }

                if (weaponManager != null &&
                    weaponManager.HasCompletedGradeFive(
                        entry.Weapon
                    ))
                {
                    continue;
                }

                result.Add(entry);
            }

            return result;
        }
    }
}
