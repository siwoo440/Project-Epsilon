using System; // 시험 실패 처리
using System.Collections.Generic; // 실제 음원 중복 확인
using ProjectEpsilon.Audio; // 실제 배경음 설정
using UnityEditor; // Unity 배치 실행
using UnityEngine; // 실제 음원 자료

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    public static class AutomatedDay114LicensedMusicVerification // 실제 배경음 교체 시험
    { // 클래스 시작
        public static void Run() // 실제 Unity 음원 가져오기 확인
        { // 함수 시작
            try // 실제 음원 검사
            { // 시험 시작
                BackgroundMusicSettings settings = Resources.Load<BackgroundMusicSettings>("Music/BackgroundMusicSettings"); // 실제 장면별 배경음 설정
                Require(settings != null, "실제 배경음 설정 누락"); // 실제 설정 확인
                AudioClip[] clips = { settings.Menu, settings.Map(0), settings.Map(1), settings.Map(2), settings.Boss }; // 실제 다섯 장면 음원
                HashSet<AudioClip> distinct = new HashSet<AudioClip>(); // 실제 중복 음원 확인
                foreach (AudioClip clip in clips) // 실제 장면별 음원 순회
                { // 반복 시작
                    Require(clip != null && clip.length > 5f && clip.samples > 0, "가져온 배경음 또는 길이 누락"); // 실제 음원 가져오기 확인
                    Require(AssetDatabase.GetAssetPath(clip).Contains("/Licensed/") && distinct.Add(clip), "가져온 음원 경로 또는 장면별 구분 오류"); // 실제 교체와 중복 확인
                } // 반복 종료
                Debug.Log("[Day114LicensedMusicVerification] PASS: Five imported music clips are distinct and readable."); // 실제 성공 결과
                EditorApplication.Exit(0); // 성공 종료
            } // 시험 종료
            catch (Exception error) // 실제 실패 확인
            { // 실패 시작
                Debug.LogError("[Day114LicensedMusicVerification] FAIL: " + error); // 실제 실패 결과
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
