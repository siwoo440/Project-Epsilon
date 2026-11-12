using System; // 실제 예외 형식
using System.Collections.Generic; // 실제 식별자 집합
using ProjectEpsilon.Data; // 실제 캐릭터와 포탑 데이터
using ProjectEpsilon.Progression; // 실제 해금 설정과 저장
using UnityEditor; // 실제 에셋 검색
using UnityEngine; // 실제 저장 직렬화

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    public static class AutomatedDataIdentifierVerification // 데이터 식별자와 기존 저장 시험
    {
        public static void Run() // 실제 배치 진입점
        {
            try // 실제 시험 실패 처리
            {
                string[] characterGuids = AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Data/Characters" }); // 실제 캐릭터 목록
                string[] weaponGuids = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/Data/Weapons" }); // 실제 포탑 목록
                Require(characterGuids.Length == 6 && weaponGuids.Length == 56, "Unexpected data asset count."); // 실제 에셋 수 확인
                HashSet<string> characters = new HashSet<string>(); // 실제 캐릭터 식별자 집합
                HashSet<string> weapons = new HashSet<string>(); // 실제 포탑 식별자 집합
                foreach (string guid in characterGuids) // 실제 캐릭터 순회
                {
                    CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)); // 실제 캐릭터 데이터
                    Require(character != null && IsValid(character.Id) && characters.Add(character.Id), "Invalid or duplicate character ID."); // 실제 고유 식별자 확인
                }
                foreach (string guid in weaponGuids) // 실제 포탑 순회
                {
                    WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid)); // 실제 포탑 데이터
                    Require(weapon != null && IsValid(weapon.Id) && weapons.Add(weapon.Id), "Invalid or duplicate turret ID."); // 실제 고유 식별자 확인
                }
                MetaUnlockConfig config = AssetDatabase.LoadAssetAtPath<MetaUnlockConfig>("Assets/Data/Progression/PrototypeMetaUnlockConfig.asset"); // 실제 해금 설정
                Require(config != null && config.FirstBossCharacterId == "mobile" && characters.Contains(config.FirstBossCharacterId), "Character unlock ID mismatch."); // 실제 캐릭터 해금 연결 확인
                string[] unlocks = // 실제 속성별 해금 포탑
                {
                    "physical_javelin", "fire_burst", "cold_wave", "electric_wave", // 앞쪽 네 속성 포탑
                    "poison_wave", "explosion_wave", "holy_wave", "dark_wave" // 뒤쪽 네 속성 포탑
                };
                for (int index = 0; index < unlocks.Length; index++) // 실제 해금 항목 순회
                {
                    Require(weapons.Contains(unlocks[index]) && config.GetTurretAttribute(unlocks[index]) == index, "Turret unlock ID mismatch."); // 실제 포탑과 속성 연결 확인
                }
                MetaSaveData oldSave = new MetaSaveData // 기존 저장 형식의 해금 상태
                {
                    cores = 17, // 기존 진화 코어
                    finishedFirstRun = true, // 기존 첫 출격 완료
                    mapMask = 3, // 기존 맵 해금 상태
                    characterMask = 1, // 기존 캐릭터 해금 상태
                    turretAttributeMask = 255 // 기존 포탑 속성 해금 상태
                };
                IdentifierMemoryStorage storage = new IdentifierMemoryStorage // 실제 기존 형식 메모리 저장소
                {
                    Content = JsonUtility.ToJson(oldSave) // 기존 저장 형식 직렬화
                };
                MetaProgressionService restored = new MetaProgressionService(storage); // 실제 저장 복원
                Require(restored.IsAvailable && restored.Cores == 17 && restored.IsCharacterUnlocked(0) && restored.IsTurretAttributeUnlocked(0) && restored.IsTurretAttributeUnlocked(7), "Existing unlock save was not restored."); // 기존 해금 복원 확인
                Debug.Log("[DataIdentifierVerification] PASS: unique IDs, unlock references and existing save masks."); // 실제 시험 성공 기록
                EditorApplication.Exit(0); // 실제 성공 종료
            }
            catch (Exception error) // 실제 시험 실패 확인
            {
                Debug.LogError("[DataIdentifierVerification] FAIL: " + error); // 실제 실패 원인 기록
                EditorApplication.Exit(1); // 실제 실패 종료
            }
        }
        private static bool IsValid(string id) // 실제 식별자 유효성 확인
        {
            return !string.IsNullOrWhiteSpace(id) && id.IndexOf("prototype_", StringComparison.OrdinalIgnoreCase) < 0; // 이전 접두어 제외
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
        private sealed class IdentifierMemoryStorage : IMetaStorage // 기존 저장 내용 시험 저장소
        {
            public string Content; // 실제 저장 내용
            public bool TryRead(out string json) // 실제 저장 읽기
            {
                json = Content; // 실제 저장 내용 반환
                return true; // 실제 읽기 성공
            }
            public bool TryWrite(string json) // 실제 저장 기록
            {
                Content = json; // 실제 저장 내용 교체
                return true; // 실제 저장 성공
            }
        }
    }
}
