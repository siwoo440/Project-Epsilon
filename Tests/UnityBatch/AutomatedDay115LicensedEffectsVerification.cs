using System; // 시험 실패 처리
using System.Collections.Generic; // 실제 음원 중복 확인
using ProjectEpsilon.Audio; // 실제 전투 효과음 설정
using UnityEditor; // Unity 배치 실행
using UnityEngine; // 실제 음원 자료

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedDay115LicensedEffectsVerification // 실제 효과음 교체 시험
    { // 클래스 시작
        public static void Run() // 실제 Unity 효과음 가져오기 확인
        { // 함수 시작
            try // 실제 음원 검사
            { // 시험 시작
                GameplaySoundSettings settings = Resources.Load<GameplaySoundSettings>("Audio/GameplaySoundSettings"); // 실제 전투 효과음 설정
                Require(settings != null, "실제 전투 효과음 설정 누락"); // 실제 설정 확인
                AudioClip[] clips = { settings.Attack.Clip, settings.Hit.Clip, settings.BodyLoss.Clip, settings.LevelUp.Clip, settings.Merge.Clip, settings.Elite.Clip, settings.Boss.Clip }; // 실제 일곱 이벤트 음원
                HashSet<AudioClip> distinct = new HashSet<AudioClip>(); // 실제 중복 음원 확인
                foreach (AudioClip clip in clips) // 실제 이벤트 음원 순회
                { // 반복 시작
                    Require(clip != null && clip.length > 0f && clip.samples > 0, "가져온 효과음 또는 길이 누락"); // 실제 음원 가져오기 확인
                    Require(AssetDatabase.GetAssetPath(clip).Contains("/Licensed/") && distinct.Add(clip), "가져온 효과음 경로 또는 이벤트 구분 오류"); // 실제 교체와 중복 확인
                    Debug.Log("[Day115LicensedEffectsVerification] " + clip.name + " " + clip.length.ToString("F2") + "초"); // 실제 개별 재생 길이 기록
                } // 반복 종료
                Debug.Log("[Day115LicensedEffectsVerification] PASS: Seven imported effects are distinct and readable."); // 실제 성공 결과
                EditorApplication.Exit(0); // 성공 종료
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Debug.LogError("[Day115LicensedEffectsVerification] FAIL: " + error); // 실제 실패 결과
                EditorApplication.Exit(1); // 실패 종료
            } // 실패 종료
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 원인 전달
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
