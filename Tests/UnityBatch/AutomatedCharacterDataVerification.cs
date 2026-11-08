using System; // 예외 형식
using System.Reflection; // 캐릭터 설정 조회
using System.Collections.Generic; // 패시브 중복 검사
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Progression; // 시작 무기 후보 데이터
using UnityEditor; // 유니티 배치 실행
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedCharacterDataVerification // 여섯 캐릭터 데이터 시험
    { // 클래스 시작
        public static void Run() // 배치 시험 진입
        { // 메서드 시작
            try // 시험 실패 처리
            { // 예외 처리 시작
                string[] names = { "Balanced", "Mobile", "Heavy", "Fusion", "Holy", "Dark" }; // 실제 캐릭터 에셋 이름
                int[] bodies = { 3, 3, 4, 3, 3, 3 }; // 기획 시작 몸통 수
                int[] health = { 100, 85, 125, 95, 100, 90 }; // 기획 공용 체력
                PropertyInfo optionsProperty = typeof(CharacterData).GetProperty("StartingWeapons"); // 시작 후보 설정 조회
                PropertyInfo passiveProperty = typeof(CharacterData).GetProperty("Passive"); // 패시브 설정 조회
                HashSet<CharacterPassive> passives = new HashSet<CharacterPassive>(); // 실제 패시브 중복 방지
                for (int index = 0; index < names.Length; index++) // 실제 여섯 캐릭터 순회
                { // 반복 시작
                    CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Data/Characters/Prototype" + names[index] + ".asset"); // 실제 캐릭터 에셋 조회
                    Require(data != null, "Character asset missing: " + names[index]); // 캐릭터 에셋 존재 확인
                    Require(data.StartingBodyCount == bodies[index] && Mathf.Approximately(data.BodyMaxHealth, health[index]), "Character base stats mismatch: " + names[index]); // 기획 시작 능력치 확인
                    Require(data.MoveSpeedMultiplier >= 0.9f && data.MoveSpeedMultiplier <= 1.1f && data.TurnPowerMultiplier >= 0.85f && data.TurnPowerMultiplier <= 1.15f, "Character movement modifiers out of bounds: " + names[index]); // 기획 이동 보정 범위 확인
                    Require(optionsProperty != null && passiveProperty != null, "Starting weapon or passive configuration missing."); // 후보와 패시브 구성 확인
                    StartingWeaponOptions options = optionsProperty.GetValue(data) as StartingWeaponOptions; // 실제 시작 후보 데이터
                    Require(options != null && options.Weapons != null && options.Weapons.Length == 3, "Character starting options missing: " + names[index]); // 시작 후보 개수 확인
                    Require(options.Weapons[0] != null && options.Weapons[1] != null && options.Weapons[2] != null && options.Weapons[0] != options.Weapons[1] && options.Weapons[0] != options.Weapons[2] && options.Weapons[1] != options.Weapons[2], "Character starting options invalid: " + names[index]); // 실제 무기와 중복 확인
                    Require(passiveProperty.GetValue(data) != null && passives.Add(data.Passive), "Character passive missing or duplicated: " + names[index]); // 실제 패시브 종류와 중복 확인
                } // 반복 끝
                Debug.Log("[CharacterDataVerification] PASS: Six character stats and distinct starting weapon choices."); // 전체 성공 기록
                EditorApplication.Exit(0); // 성공 종료 코드
            } // 예외 처리 끝
            catch (Exception exception) // 실제 실패 확인
            { // 예외 처리 시작
                Debug.LogError("[CharacterDataVerification] FAIL: " + exception.Message); // 실패 원인 기록
                EditorApplication.Exit(1); // 실패 종료 코드
            } // 예외 처리 끝
        } // 메서드 끝
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
