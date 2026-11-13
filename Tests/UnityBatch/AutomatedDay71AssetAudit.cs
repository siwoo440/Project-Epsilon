using System; // 예외 처리
using System.Collections.Generic; // 중복 검사
using System.IO; // 파일 목록
using ProjectEpsilon.Data; // 포탑과 캐릭터 데이터
using ProjectEpsilon.Progression; // 포탑 보상 목록
using ProjectEpsilon.World; // 맵 데이터
using UnityEditor; // Unity 자산 검사
using UnityEngine; // 스프라이트 검사

namespace ProjectEpsilon.Editor // 배치 검사 영역
{ // 영역 시작
    public static class AutomatedDay71AssetAudit // 자산 연결 검사
    { // 검사 시작
        public static void Run() // 배치 진입점
        { // 진입 시작
            try // 검사 실행
            { // 실행 시작
                HashSet<string> ids = new HashSet<string>(); // 포탑 식별자 집합
                HashSet<string> images = new HashSet<string>(); // 연결된 이미지 집합
                HashSet<WeaponData> weapons = new HashSet<WeaponData>(); // 포탑 데이터 집합
                foreach (string path in Directory.GetFiles("Assets/Data/Weapons", "*.asset", SearchOption.TopDirectoryOnly)) // 기본 포탑 파일 순회
                { // 순회 시작
                    WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path.Replace('\\', '/')); // 실제 포탑 데이터
                    Require(weapon != null && weapon.Artwork != null, "포탑 이미지 누락: " + path); // 이미지 연결 확인
                    Require(ids.Add(weapon.Id), "포탑 식별자 중복: " + weapon.Id); // 식별자 중복 확인
                    weapons.Add(weapon); // 포탑 수집
                    images.Add(AssetDatabase.GetAssetPath(weapon.Artwork)); // 실제 이미지 수집
                } // 순회 종료
                Require(weapons.Count == 48, "기본 포탑 수: " + weapons.Count); // 포탑 수 확인
                WeaponRewardPool pool = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 실제 보상 목록
                Require(pool != null, "보상 목록 누락"); // 목록 존재 확인
                HashSet<WeaponData> rewards = new HashSet<WeaponData>(); // 보상 중복 검사
                foreach (WeaponRewardEntry entry in pool.Entries) // 보상 항목 순회
                { // 순회 시작
                    Require(entry != null && entry.Weapon != null && rewards.Add(entry.Weapon), "보상 포탑 누락 또는 중복"); // 보상 항목 확인
                } // 순회 종료
                Require(rewards.SetEquals(weapons), "기본 포탑과 보상 목록 불일치"); // 전체 보상 연결 확인
                int characters = 0; // 캐릭터 수
                foreach (string guid in AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Data/Characters" })) // 캐릭터 데이터 순회
                { // 순회 시작
                    CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)); // 캐릭터 데이터
                    Require(character != null && character.StartingWeapons != null && character.StartingWeapons.Weapons.Length == 3, "시작 포탑 후보 누락"); // 시작 후보 확인
                    foreach (WeaponData candidate in character.StartingWeapons.Weapons) // 시작 후보 순회
                    { // 순회 시작
                        Require(candidate != null && candidate.Artwork != null, "시작 후보 이미지 누락"); // 시작 이미지 확인
                    } // 순회 종료
                    characters++; // 캐릭터 수 증가
                } // 순회 종료
                Require(characters == 6, "캐릭터 수: " + characters); // 캐릭터 수 확인
                int maps = 0; // 맵 수
                foreach (string guid in AssetDatabase.FindAssets("t:PrototypeMapProfile", new[] { "Assets/Data/World" })) // 맵 데이터 순회
                { // 순회 시작
                    PrototypeMapProfile profile = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>(AssetDatabase.GUIDToAssetPath(guid)); // 실제 맵 설정
                    Require(profile != null && profile.Ground != null, "맵 바닥 이미지 누락"); // 바닥 연결 확인
                    maps++; // 맵 수 증가
                } // 순회 종료
                Require(maps == 3, "맵 수: " + maps); // 맵 수 확인
                int elements = 0; // 환경 이미지 수
                foreach (string path in Directory.GetFiles("Assets/Resources/MapElements", "*.png", SearchOption.AllDirectories)) // 환경 파일 순회
                { // 순회 시작
                    string assetPath = path.Replace('\\', '/'); // Unity 경로 변환
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter; // 이미지 가져오기 설정
                    Require(importer != null && importer.textureType == TextureImporterType.Sprite && AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) != null, "환경 스프라이트 누락: " + path); // 실제 스프라이트 확인
                    elements++; // 환경 이미지 수 증가
                } // 순회 종료
                Require(elements == 33, "환경 이미지 수: " + elements); // 환경 이미지 수 확인
                Debug.Log("[Day71AssetAudit] PASS weapons=" + weapons.Count + " distinctWeaponImages=" + images.Count + " characters=" + characters + " maps=" + maps + " mapSprites=" + elements + " imagePaths=" + string.Join(",", images)); // 검사 결과 기록
                EditorApplication.Exit(0); // 정상 종료
            } // 실행 종료
            catch (Exception error) // 실패 처리
            { // 실패 시작
                Debug.LogError("[Day71AssetAudit] FAIL " + error); // 실패 원인 기록
                EditorApplication.Exit(1); // 실패 종료
            } // 실패 종료
        } // 진입 종료
        private static void Require(bool condition, string message) // 검사 조건 확인
        { // 확인 시작
            if (!condition) // 조건 실패 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 확인 종료
    } // 검사 종료
} // 영역 종료
