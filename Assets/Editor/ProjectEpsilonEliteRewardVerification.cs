using System; // 예외 형식
using System.Collections.Generic; // 대상 목록
using System.Reflection; // 형식 조회
using ProjectEpsilon.Combat; // 전투 대상
using ProjectEpsilon.Enemies; // 적 출현기
using ProjectEpsilon.Progression; // 진행 시스템
using ProjectEpsilon.UI; // 설정 화면
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    public static class ProjectEpsilonEliteRewardVerification // 엘리트 보상 검증
    { // 클래스 시작
        public static void Run() // 검증 실행
        { // 메서드 시작
            Type controller = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.EliteRewardController"); // 보상 처리기 형식
            Require(controller != null, "엘리트 보상 처리기 누락"); // 처리기 존재 확인
            Require(controller.GetMethod("QueueReward", BindingFlags.Public | BindingFlags.Instance) != null, "처치 보상 대기열 누락"); // 보상 요청 확인
            Type rules = typeof(SnakeExperience).Assembly.GetType("ProjectEpsilon.Progression.EliteRewardRules"); // 보상 규칙 형식
            Require(rules != null, "보상 추첨 규칙 누락"); // 규칙 존재 확인
            MethodInfo select = rules.GetMethod("Select", BindingFlags.Public | BindingFlags.Static); // 후보 추첨 조회
            Require(select != null, "후보 추첨 메서드 누락"); // 메서드 존재 확인
            bool[] eligible = new bool[] // 무기만 선택 가능한 상태
            { // 목록 시작
                false, false, true, false // 보상별 사용 가능 여부
            }; // 목록 끝
            object[] arguments = new object[] // 추첨 인자 목록
            { // 목록 시작
                eligible, 3 // 가능 상태와 추첨값
            }; // 목록 끝
            int[] choices = (int[])select.Invoke(null, arguments); // 후보 조합 추첨
            Require(choices.Length == 3, "보상 후보 수 오류"); // 세 후보 확인
            Require(choices[0] != choices[1] && choices[0] != choices[2] && choices[1] != choices[2], "중복 보상 후보"); // 서로 다른 후보 확인
            Require(Array.IndexOf(choices, 2) >= 0, "선택 가능한 보상 누락"); // 막힌 선택 화면 방지
            VerifyDuplicateEliteDeath(); // 엘리트 중복 사망 검증
            VerifySettingsOrder(); // 설정 화면 표시 순서 검증
            Debug.Log("[Project Epsilon] Elite reward verification passed."); // 성공 기록
        } // 메서드 끝

        private static void VerifySettingsOrder() // 설정 화면 겹침 검증
        { // 메서드 시작
            SettingsMenuController menu = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>(); // 설정 관리자 조회
            Require(menu != null, "설정 관리자 누락"); // 설정 관리자 확인
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 비공개 멤버 조회 설정
            GameObject settings = (GameObject)typeof(SettingsMenuController).GetField("settingsPanel", flags).GetValue(menu); // 설정 화면 조회
            Require(settings != null && settings.transform.parent != null, "설정 화면 연결 누락"); // 화면 및 부모 확인
            int previousIndex = settings.transform.GetSiblingIndex(); // 기존 표시 순서 저장
            try // 화면 순서 복구 보장
            { // 보호 시작
                typeof(SettingsMenuController).GetMethod("OpenSettings", flags).Invoke(menu, null); // 실제 설정 열기
                Require(menu.IsOpen && settings.transform.GetSiblingIndex() == settings.transform.parent.childCount - 1, "설정 화면 최상위 표시 실패"); // 설정 화면 순서 확인
            } // 보호 끝
            finally // 설정 화면 복구
            { // 정리 시작
                typeof(SettingsMenuController).GetMethod("CloseSettings", flags).Invoke(menu, null); // 설정 화면 닫기
                settings.transform.SetSiblingIndex(previousIndex); // 원래 표시 순서 복구
            } // 정리 끝
        } // 메서드 끝

        private static void VerifyDuplicateEliteDeath() // 중복 사망 보상 검증
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 출현기 조회
            EliteRewardController controller = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 보상 처리기 조회
            Require(spawner != null && controller != null, "씬 보상 구성 누락"); // 씬 참조 확인
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 멤버 조회 설정
            FieldInfo rewards = typeof(EarlyEnemyWaveSpawner).GetField("eliteRewards", flags); // 보상 연결 필드 조회
            Require(ReferenceEquals(rewards.GetValue(spawner), controller), "출현기 보상 참조 누락"); // 직렬화 참조 확인
            GameObject subject = new GameObject("EliteRewardVerificationTarget"); // 임시 엘리트 객체
            GameObject survivor = new GameObject("EliteRewardVerificationSurvivor"); // 생존 적 객체
            try // 임시 객체 정리 보장
            { // 보호 시작
                WeaponTarget target = subject.AddComponent<WeaponTarget>(); // 전투 대상 생성
                List<WeaponTarget> targets = (List<WeaponTarget>)typeof(EarlyEnemyWaveSpawner).GetField("targets", flags).GetValue(spawner); // 출현 대상 목록
                targets.Add(target); // 시험 대상 등록
                WeaponTarget survivorTarget = survivor.AddComponent<WeaponTarget>(); // 생존 적 대상 생성
                targets.Add(survivorTarget); // 생존 적 등록
                HashSet<WeaponTarget> eliteTargets = (HashSet<WeaponTarget>)typeof(EarlyEnemyWaveSpawner).GetField("eliteTargets", flags).GetValue(spawner); // 엘리트 대상 집합
                eliteTargets.Add(target); // 시험 엘리트 등록
                typeof(EarlyEnemyWaveSpawner).GetField("activeEnemies", flags).SetValue(spawner, 2); // 시험 적 수 설정
                MethodInfo death = typeof(EarlyEnemyWaveSpawner).GetMethod("HandleEnemyDied", flags); // 사망 처리 조회
                int previous = controller.PendingRewardCount; // 기존 보상 수
                object[] deathArguments = new object[] // 사망 처리 인자
                { // 목록 시작
                    target // 시험 대상
                }; // 목록 끝
                death.Invoke(spawner, deathArguments); // 첫 사망 처리
                death.Invoke(spawner, deathArguments); // 중복 사망 처리
                Require(controller.PendingRewardCount == previous + 1 && spawner.ActiveEnemies == 1, "중복 사망 보상 또는 적 수 오류"); // 단일 보상과 생존 적 수 확인
                targets.Remove(survivorTarget); // 시험 생존 적 등록 해제
            } // 보호 끝
            finally // 시험 객체 정리
            { // 정리 시작
                UnityEngine.Object.DestroyImmediate(subject); // 임시 객체 제거
                UnityEngine.Object.DestroyImmediate(survivor); // 생존 적 객체 제거
            } // 정리 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 검증 조건
        { // 메서드 시작
            if (!condition) // 실패 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 보고
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
