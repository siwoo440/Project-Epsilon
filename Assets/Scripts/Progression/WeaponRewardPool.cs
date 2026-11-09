using System;
using System.Collections.Generic;
using ProjectEpsilon.Combat;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player; // 현재 캐릭터 패시브
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
                int selectedIndex = SelectCandidateIndex(available, weaponManager, CharacterRunInitializer.ActiveCharacter); // 실제 캐릭터 가중치 추첨

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

        public static float GetCandidateWeight(WeaponData weapon, SnakeWeaponManager weapons, CharacterData character) // 캐릭터별 실제 후보 가중치
        { // 메서드 시작
            if (weapon == null || character == null) // 무기와 캐릭터 확인
            { // 조건 시작
                return 1f; // 기본 균등 가중치
            } // 조건 끝
            if (character.Passive == CharacterPassive.HolySpecialization && weapon.Attribute == WeaponAttribute.Holy) // 신성 특화 후보 확인
            { // 조건 시작
                return character.PreferredCandidateWeight; // 신성 후보 가중치
            } // 조건 끝
            if (character.Passive == CharacterPassive.DarkSpecialization && weapon.Attribute == WeaponAttribute.Dark) // 암흑 특화 후보 확인
            { // 조건 시작
                return character.PreferredCandidateWeight; // 암흑 후보 가중치
            } // 조건 끝
            if (character.Passive == CharacterPassive.FusionCandidates && weapons != null) // 합성형 보유 무기 확인
            { // 조건 시작
                foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 장착 슬롯 순회
                { // 반복 시작
                    if (slot != null && !slot.IsEmpty && slot.Weapon == weapon) // 동일 보유 무기 확인
                    { // 조건 시작
                        return character.PreferredCandidateWeight; // 보유 무기 후보 가중치
                    } // 조건 끝
                } // 반복 끝
            } // 조건 끝
            return 1f; // 기본 균등 가중치
        } // 메서드 끝

        private static int SelectCandidateIndex(List<WeaponRewardEntry> available, SnakeWeaponManager weapons, CharacterData character) // 중복 없는 가중 무기 추첨
        { // 메서드 시작
            List<WeaponData> candidates = new List<WeaponData>(available.Count); // 실제 무기 후보 목록
            foreach (WeaponRewardEntry entry in available) // 실제 후보 순회
            { // 반복 시작
                candidates.Add(entry.Weapon); // 공통 추첨에 무기 전달
            } // 반복 끝
            return SelectWeaponIndex(candidates, weapons, character); // 레벨과 엘리트 공통 추첨
        } // 메서드 끝

        public static int SelectWeaponIndex(IReadOnlyList<WeaponData> available, SnakeWeaponManager weapons, CharacterData character) // 실제 후보 가중 추첨
        { // 메서드 시작
            if (available == null || available.Count == 0) // 후보 목록 확인
            { // 조건 시작
                return -1; // 선택 가능한 무기 없음
            } // 조건 끝
            if (character == null || (character.Passive != CharacterPassive.FusionCandidates && character.Passive != CharacterPassive.HolySpecialization && character.Passive != CharacterPassive.DarkSpecialization)) // 가중 특화 캐릭터 확인
            { // 조건 시작
                return UnityEngine.Random.Range(0, available.Count); // 기존 균등 추첨 유지
            } // 조건 끝
            float total = 0f; // 전체 후보 가중치
            foreach (WeaponData weapon in available) // 실제 후보 순회
            { // 반복 시작
                total += GetCandidateWeight(weapon, weapons, character); // 실제 후보 가중치 누적
            } // 반복 끝
            float selected = UnityEngine.Random.value * total; // 실제 가중 추첨값
            for (int index = 0; index < available.Count; index++) // 실제 후보 순회
            { // 반복 시작
                selected -= GetCandidateWeight(available[index], weapons, character); // 현재 후보 가중치 차감
                if (selected < 0f) // 현재 후보 선택 확인
                { // 조건 시작
                    return index; // 실제 후보 위치 반환
                } // 조건 끝
            } // 반복 끝
            return available.Count - 1; // 난수 상한의 마지막 후보
        } // 메서드 끝

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

                if (MetaProgressionController.Instance != null && !MetaProgressionController.Instance.IsTurretUnlocked(entry.Weapon)) // 실제 포탑 해금 확인
                { // 조건 시작
                    continue; // 미해금 포탑 제외
                } // 조건 끝

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
