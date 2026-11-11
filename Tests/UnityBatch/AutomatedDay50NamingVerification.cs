using System; // 실제 예외 형식
using System.Collections.Generic; // 실제 이름 집합
using System.Reflection; // 실제 표시 문구 조회
using System.Text.RegularExpressions; // 실제 영문 검사
using ProjectEpsilon.Combat; // 실제 포탑 슬롯
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Progression; // 실제 보상 후보
using ProjectEpsilon.UI; // 실제 포탑 선택 화면
using UnityEditor; // 실제 Unity 에셋 조회
using UnityEngine; // 실제 Unity 객체

namespace ProjectEpsilon.Editor // 실제 시험 영역
{
    public static class AutomatedDay50NamingVerification // 실제 포탑·아이템 표시 시험
    {
        public static void Run() // 실제 Unity 배치 진입
        {
            GameObject holder = null; // 실제 보상 시험 객체
            try // 실제 이름 검증
            {
                string[] assets = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/Data/Weapons" }); // 실제 포탑 에셋 목록
                Require(assets.Length == 56, "Weapon asset count changed."); // 실제 전체 포탑 수 확인
                HashSet<string> playableNames = new HashSet<string>(); // 실제 기본 포탑 이름 집합
                int playable = 0; // 실제 기본 포탑 수
                MethodInfo candidateLabel = typeof(LevelUpPanelController).GetMethod("BuildCandidateLabel", BindingFlags.Static | BindingFlags.NonPublic); // 실제 포탑 카드 문구
                MethodInfo slotLabel = typeof(WeaponMergePanelController).GetMethod("FormatSlot", BindingFlags.Static | BindingFlags.NonPublic); // 실제 합성 포탑 문구
                Require(candidateLabel != null && slotLabel != null, "Weapon label functions missing."); // 실제 화면 문구 구성 확인
                foreach (string guid in assets) // 실제 모든 포탑 순회
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid); // 실제 에셋 경로
                    WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path); // 실제 포탑 데이터
                    Require(weapon != null && !string.IsNullOrWhiteSpace(weapon.Id) && IsKoreanName(weapon.DisplayName), "Invalid visible weapon name: " + path); // 실제 한글 이름과 식별자 확인
                    if (!path.Contains("/Day14/")) // 실제 기본 포탑 확인
                    {
                        playable++; // 실제 기본 포탑 수 증가
                        Require(playableNames.Add(weapon.DisplayName), "Duplicate visible weapon name: " + weapon.DisplayName); // 실제 포탑 이름 중복 방지
                    }
                    string card = (string)candidateLabel.Invoke(null, new object[] { new WeaponRewardCandidate(weapon, 1) }); // 실제 선택 카드 문구
                    Require(IsKoreanName(card), "English or temporary text in weapon card: " + path); // 실제 한글 포탑 카드 확인
                    SnakeWeaponSlot slot = new SnakeWeaponSlot(null); // 실제 포탑 슬롯 생성
                    slot.Equip(weapon); // 실제 포탑 장착
                    string merge = (string)slotLabel.Invoke(null, new object[] { slot, 0 }); // 실제 합성 재료 문구
                    Require(IsKoreanName(merge), "English or temporary text in merge slot: " + path); // 실제 한글 합성 문구 확인
                }
                Require(playable == 48, "Playable weapon count changed."); // 실제 기본 포탑 수 확인
                holder = new GameObject("NamingVerification"); // 실제 보상 시험 객체
                EliteRewardController rewards = holder.AddComponent<EliteRewardController>(); // 실제 보상 제어기
                MethodInfo build = typeof(EliteRewardController).GetMethod("BuildLabel", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보상 이름 생성기
                Require(build != null, "Reward label function missing."); // 실제 아이템 보상 이름 경로 확인
                for (int kind = 0; kind < 4; kind++) // 실제 보상 종류 순회
                {
                    foreach (bool available in new[] { true, false }) // 실제 가능·불가 상태 순회
                    {
                        string label = (string)build.Invoke(rewards, new object[] { kind, available }); // 실제 보상 표시 문구
                        Require(IsKoreanName(label), "English or temporary reward name: " + kind); // 실제 한글 보상 확인
                    }
                }
                Debug.Log("[Day50NamingVerification] PASS: 56 asset names, 48 distinct playable names, weapon cards, merge slots and reward names."); // 실제 시험 결과
                UnityEngine.Object.DestroyImmediate(holder); // 실제 시험 객체 정리
                EditorApplication.Exit(0); // 실제 성공 종료
            }
            catch (Exception error) // 실제 시험 오류 확인
            {
                if (holder != null) // 실제 시험 객체 확인
                {
                    UnityEngine.Object.DestroyImmediate(holder); // 실제 시험 객체 정리
                }
                Debug.LogError("[Day50NamingVerification] FAIL: " + error); // 실제 실패 기록
                EditorApplication.Exit(1); // 실제 실패 종료
            }
        }
        private static bool IsKoreanName(string text) // 실제 한국어 이름 검증
        {
            return !string.IsNullOrWhiteSpace(text) && !text.Contains("시험용") && !Regex.IsMatch(text, "[A-Za-z]"); // 시험용과 영문 제외
        }
        private static void Require(bool condition, string message) // 실제 시험 조건
        {
            if (!condition) // 실제 조건 실패 확인
            {
                throw new InvalidOperationException(message); // 실제 시험 중단
            }
        }
    }
}
