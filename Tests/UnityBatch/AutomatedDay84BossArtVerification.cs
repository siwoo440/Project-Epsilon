using System; // 예외 처리
using System.Reflection; // 실제 보스 설정 조회
using ProjectEpsilon.Combat; // 실제 보스 대상
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 보스 출현기
using ProjectEpsilon.World; // 맵 데이터
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay84BossArtVerification // 맵별 보스 그림 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay84BossArt"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay84BossArtVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시작 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            int mapIndex = 0; // 기본 평원 시험
            string[] args = Environment.GetCommandLineArgs(); // 실제 실행 인수
            for (int index = 0; index + 1 < args.Length; index++) // 실행 인수 순회
            { // 반복 시작
                if (args[index] == "-epsilonMapIndex") // 시험 맵 인수 확인
                { // 조건 시작
                    int.TryParse(args[index + 1], out mapIndex); // 실제 맵 번호 저장
                } // 조건 종료
            } // 반복 종료
            SessionState.SetInt(Key + "Map", mapIndex); // 맵 번호 보존
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 시작 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 보스 그림 시험
            { // 시험 시작
                Verify(SessionState.GetInt(Key + "Map", 0)); // 선택 맵 보스 확인
                Finish(0, "Map boss artwork, fallback and actual spawned sprite passed."); // 성공 결과
            } // 시험 종료
            catch (Exception error) // 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인
            } // 실패 종료
        } // 함수 종료
        private static void Verify(int index) // 실제 맵별 보스 확인
        { // 함수 시작
            string[] profiles = { "Plains", "Ruins", "Swamp" }; // 실제 맵 데이터 이름
            string[] artwork = { "BossPlains_Casual", "BossRuins_Casual", "BossSwamp_Casual" }; // 실제 맵별 보스 그림 이름
            Require(index >= 0 && index < profiles.Length, "잘못된 맵 번호"); // 시험 범위 확인
            PrototypeMapProfile profile = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>("Assets/Data/World/PrototypeMap" + profiles[index] + ".asset"); // 실제 맵 데이터
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            PropertyInfo image = typeof(PrototypeMapProfile).GetProperty("BossArtwork"); // 맵별 그림 조회
            FieldInfo visual = typeof(PrototypeBossEncounter).GetField("bossVisual", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 그림 설정
            FieldInfo projectile = typeof(PrototypeBossEncounter).GetField("projectileVisual", BindingFlags.Instance | BindingFlags.NonPublic); // 기존 보스 탄환 그림
            Require(profile != null && encounter != null && image != null && visual != null && projectile != null, "맵별 보스 그림 연결 누락"); // 실제 연결 확인
            Sprite expected = image.GetValue(profile) as Sprite; // 실제 맵별 그림
            Require(expected != null && expected.name == artwork[index], "선택 맵의 보스 그림 오류"); // 기획서 그림 확인
            Sprite original = visual.GetValue(encounter) as Sprite; // 씬의 기본 보스 그림
            object originalProjectile = projectile.GetValue(encounter); // 기존 탄환 그림 보존
            Require(encounter.ApplyProfile(profile), "맵 보스 설정 실패"); // 실제 맵 설정 적용
            Require((Sprite)visual.GetValue(encounter) == expected && projectile.GetValue(encounter) == originalProjectile, "맵 그림 적용 또는 탄환 그림 보존 실패"); // 실제 그림 선택 확인
            PrototypeMapProfile fallback = ScriptableObject.CreateInstance<PrototypeMapProfile>(); // 그림 없는 맵 시험
            Require(encounter.ApplyProfile(fallback) && (Sprite)visual.GetValue(encounter) == original, "기본 보스 그림 복원 실패"); // 이전 맵 그림 잔류 방지
            Require(encounter.ApplyProfile(profile), "맵 보스 재설정 실패"); // 선택 맵 그림 복원
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성 경로
            Require(spawn != null, "보스 생성 경로 누락"); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 실제 보스 생성
            WeaponTarget boss = encounter.Boss; // 실제 생성된 보스
            SpriteRenderer[] renderers = boss == null ? Array.Empty<SpriteRenderer>() : boss.GetComponentsInChildren<SpriteRenderer>(true); // 실제 보스 표시기
            bool found = false; // 선택 그림 사용 상태
            foreach (SpriteRenderer renderer in renderers) // 실제 보스 표시기 순회
            { // 반복 시작
                found |= renderer.sprite == expected && renderer.color == Color.white; // 실제 원본 그림과 색상 확인
            } // 반복 종료
            Require(found, "실제 보스에 선택 맵 그림 또는 원본 색상 누락"); // 실제 생성 결과 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day84BossArtVerification] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
