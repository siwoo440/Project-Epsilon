using System; // C# 기본 기능 연결
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Combat; // 전투 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using ProjectEpsilon.Player; // 현재 캐릭터 패시브
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    [Serializable] // 대상 설정
    public sealed class WeaponRewardEntry // 포탑·보상 자료 구조
    {
        [SerializeField] private WeaponData weapon; // 필드·포탑 설정
        [SerializeField] private bool unlocked = true; // 필드 설정

        public WeaponData Weapon => weapon; // 포탑·데이터 값 반환
        public bool Unlocked => unlocked; // 대상 값 반환

        public WeaponRewardEntry( // 포탑·보상 호출 시작
            WeaponData weaponData, // 포탑·데이터 항목 전달
            bool isUnlocked // 대상 처리
        ) // 앞선 항목 종료
        {
            weapon = weaponData; // 포탑·데이터 값 설정
            unlocked = isUnlocked; // 대상 값 설정
        }
    }

    [CreateAssetMenu( // 생성 설정
        fileName = "WeaponRewardPool", // 파일·이름 값 설정
        menuName = "Project Epsilon/Progression/Weapon Reward Pool" // 이름·프로젝트 값 설정
    )] // 대상 처리
    public sealed class WeaponRewardPool : ScriptableObject // 포탑·보상 자료 구조
    {
        [SerializeField] private float focusedCandidateChance = 0.4f; // 첫 카드의 최다 보유 포탑 집중 확률
        [SerializeField] private List<WeaponRewardEntry> entries = // 필드·목록 설정
            new List<WeaponRewardEntry>(); // 목록·포탑 객체 생성

        public IReadOnlyList<WeaponRewardEntry> Entries => entries; // 목록·포탑 값 반환

        public void Configure( // 구성 호출 시작
            IReadOnlyList<WeaponData> weapons // 목록·포탑 처리
        ) // 앞선 항목 종료
        {
            entries.Clear(); // 대상 기능 실행

            if (weapons == null) // 포탑 조건 확인
            {
                return; // 현재 기능 종료
            }

            for (int index = 0; index < weapons.Count; index++) // 순서·포탑 반복 처리
            {
                WeaponData weapon = weapons[index]; // 포탑·데이터 값 설정

                if (weapon == null) // 포탑 조건 확인
                {
                    continue; // 다음 반복 진행
                }

                entries.Add( // 추가 호출 시작
                    new WeaponRewardEntry( // 포탑·보상 객체 생성
                        weapon, // 포탑 항목 전달
                        true // 대상 처리
                    ) // 앞선 항목 종료
                ); // 앞선 항목 종료
            }
        }

        public List<WeaponRewardCandidate> BuildCandidates( // 목록·포탑 호출 시작
            int level, // 레벨 항목 전달
            SnakeWeaponManager weaponManager, // 포탑·관리자 항목 전달
            int requestedCount // 개수 처리
        ) // 앞선 항목 종료
        {
            int safeCount = // 개수 값 설정
                Mathf.Max(0, requestedCount); // 최대·개수 기능 실행

            List<WeaponRewardEntry> available = // 목록·포탑 값 설정
                BuildAvailableEntries( // 구성 호출 시작
                    weaponManager // 포탑·관리자 처리
                ); // 앞선 항목 종료

            List<WeaponRewardCandidate> candidates = // 목록·포탑 값 설정
                new List<WeaponRewardCandidate>( // 목록·포탑 객체 생성
                    safeCount // 개수 처리
                ); // 앞선 항목 종료

            while (candidates.Count < safeCount && // 후보·개수 반복 조건
                available.Count > 0) // 개수 처리
            {
                int selectedIndex = -1; // 실제 선택 후보 초기화
                if (candidates.Count == 0 && UnityEngine.Random.value < Mathf.Clamp01(focusedCandidateChance)) // 첫 카드의 집중 추첨 확률 확인
                {
                    selectedIndex = SelectFocusedCandidateIndex(available, weaponManager); // 최다 보유 포탑 집중 추첨
                }
                if (selectedIndex < 0) // 집중 대상이 없거나 일반 추첨인 경우
                {
                    selectedIndex = SelectCandidateIndex(available, weaponManager, CharacterRunInitializer.ActiveCharacter); // 기존 캐릭터 가중치 추첨
                }

                WeaponRewardEntry selected = // 포탑·보상 값 설정
                    available[selectedIndex]; // 선택 값·순서 처리

                available.RemoveAt(selectedIndex); // 제거·선택 값 기능 실행

                WeaponData weapon = // 포탑·데이터 값 설정
                    selected.Weapon; // 선택 값·포탑 처리

                int grade = // 등급 값 설정
                    WeaponRewardRules.RollGrade( // 포탑·보상 호출 시작
                        level, // 레벨 항목 전달
                        weapon.MaxGrade, // 포탑·최대 항목 전달
                        UnityEngine.Random.value // Unity·난수 처리
                    ); // 앞선 항목 종료

                candidates.Add( // 후보·추가 호출 시작
                    new WeaponRewardCandidate( // 포탑·보상 객체 생성
                        weapon, // 포탑 항목 전달
                        grade // 등급 처리
                    ) // 앞선 항목 종료
                ); // 앞선 항목 종료
            }

            return candidates; // 후보 결과 반환
        }

        public static float GetCandidateWeight(WeaponData weapon, SnakeWeaponManager weapons, CharacterData character) // 캐릭터별 실제 후보 가중치
        {
            if (weapon == null || character == null) // 무기와 캐릭터 확인
            {
                return 1f; // 기본 균등 가중치
            }
            if (character.Passive == CharacterPassive.HolySpecialization && weapon.Attribute == WeaponAttribute.Holy) // 신성 특화 후보 확인
            {
                return character.PreferredCandidateWeight; // 신성 후보 가중치
            }
            if (character.Passive == CharacterPassive.DarkSpecialization && weapon.Attribute == WeaponAttribute.Dark) // 암흑 특화 후보 확인
            {
                return character.PreferredCandidateWeight; // 암흑 후보 가중치
            }
            if (character.Passive == CharacterPassive.FusionCandidates && weapons != null) // 합성형 보유 무기 확인
            {
                foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 장착 슬롯 순회
                {
                    if (slot != null && !slot.IsEmpty && string.Equals(slot.Weapon.Id, weapon.Id, StringComparison.Ordinal)) // 동일 식별자 보유 포탑 확인
                    {
                        return character.PreferredCandidateWeight; // 보유 무기 후보 가중치
                    }
                }
            }
            return 1f; // 기본 균등 가중치
        }

        private static int SelectCandidateIndex(List<WeaponRewardEntry> available, SnakeWeaponManager weapons, CharacterData character) // 중복 없는 가중 무기 추첨
        {
            List<WeaponData> candidates = new List<WeaponData>(available.Count); // 실제 무기 후보 목록
            foreach (WeaponRewardEntry entry in available) // 실제 후보 순회
            {
                candidates.Add(entry.Weapon); // 공통 추첨에 무기 전달
            }
            return SelectWeaponIndex(candidates, weapons, character); // 레벨과 엘리트 공통 추첨
        }

        private static int SelectFocusedCandidateIndex(List<WeaponRewardEntry> available, SnakeWeaponManager weapons) // 최다 보유 포탑의 균등 집중 추첨
        {
            if (weapons == null || available == null || available.Count == 0) // 실제 장착 슬롯과 후보 확인
            {
                return -1; // 집중 대상 없는 일반 추첨
            }
            int maximum = 0; // 실제 최다 보유 개수
            List<int> tied = new List<int>(); // 실제 최다 보유 동률 후보
            for (int index = 0; index < available.Count; index++) // 실제 선택 가능한 포탑 순회
            {
                string id = available[index].Weapon.Id; // 실제 후보 포탑 식별자
                int count = 0; // 현재 종류의 보유 개수
                foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 장착 슬롯 순회
                {
                    if (slot != null && !slot.IsEmpty && string.Equals(slot.Weapon.Id, id, StringComparison.Ordinal)) // 등급과 무관한 동일 포탑 확인
                    {
                        count++; // 실제 보유 개수 증가
                    }
                }
                if (count > maximum) // 더 많이 보유한 종류 확인
                {
                    maximum = count; // 실제 최다 보유 개수 갱신
                    tied.Clear(); // 이전 동률 후보 제거
                    tied.Add(index); // 실제 새 최다 후보 기록
                }
                else if (count > 0 && count == maximum) // 같은 최다 보유 종류 확인
                {
                    tied.Add(index); // 실제 동률 후보 추가
                }
            }
            return tied.Count == 0 ? -1 : tied[UnityEngine.Random.Range(0, tied.Count)]; // 실제 동률 균등 선택 또는 일반 추첨
        }

        public static int SelectWeaponIndex(IReadOnlyList<WeaponData> available, SnakeWeaponManager weapons, CharacterData character) // 실제 후보 가중 추첨
        {
            if (available == null || available.Count == 0) // 후보 목록 확인
            {
                return -1; // 선택 가능한 무기 없음
            }
            if (character == null || (character.Passive != CharacterPassive.FusionCandidates && character.Passive != CharacterPassive.HolySpecialization && character.Passive != CharacterPassive.DarkSpecialization)) // 가중 특화 캐릭터 확인
            {
                return UnityEngine.Random.Range(0, available.Count); // 기존 균등 추첨 유지
            }
            float total = 0f; // 전체 후보 가중치
            foreach (WeaponData weapon in available) // 실제 후보 순회
            {
                total += GetCandidateWeight(weapon, weapons, character); // 실제 후보 가중치 누적
            }
            float selected = UnityEngine.Random.value * total; // 실제 가중 추첨값
            for (int index = 0; index < available.Count; index++) // 실제 후보 순회
            {
                selected -= GetCandidateWeight(available[index], weapons, character); // 현재 후보 가중치 차감
                if (selected < 0f) // 현재 후보 선택 확인
                {
                    return index; // 실제 후보 위치 반환
                }
            }
            return available.Count - 1; // 난수 상한의 마지막 후보
        }

        private List<WeaponRewardEntry> BuildAvailableEntries( // 목록·포탑 호출 시작
            SnakeWeaponManager weaponManager // 포탑·관리자 처리
        ) // 앞선 항목 종료
        {
            List<WeaponRewardEntry> result = // 목록·포탑 값 설정
                new List<WeaponRewardEntry>(); // 목록·포탑 객체 생성
            HashSet<string> includedIds = new HashSet<string>(StringComparer.Ordinal); // 실제 후보 식별자 중복 제거

            for (int index = 0; index < entries.Count; index++) // 순서·개수 반복 처리
            {
                WeaponRewardEntry entry = // 포탑·보상 값 설정
                    entries[index]; // 순서 처리

                if (entry == null || // 대상 조건 확인
                    !entry.Unlocked || // 대상 처리
                    entry.Weapon == null) // 포탑 처리
                {
                    continue; // 다음 반복 진행
                }

                if (MetaProgressionController.Instance != null && !MetaProgressionController.Instance.IsTurretUnlocked(entry.Weapon)) // 실제 포탑 해금 확인
                {
                    continue; // 미해금 포탑 제외
                }

                if (weaponManager != null && // 포탑·관리자 조건 확인
                    weaponManager.HasCompletedGradeFive( // 포탑·관리자 호출 시작
                        entry.Weapon // 포탑 처리
                    )) // 대상 처리
                {
                    continue; // 다음 반복 진행
                }

                if (!includedIds.Add(entry.Weapon.Id ?? string.Empty)) // 실제 동일 식별자 중복 확인
                {
                    continue; // 중복 포탑 후보 제외
                }

                result.Add(entry); // 결과·추가 기능 실행
            }

            return result; // 결과 결과 반환
        }
    }
}
