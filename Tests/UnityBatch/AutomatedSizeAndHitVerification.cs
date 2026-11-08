using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 생성 경로 조회
using ProjectEpsilon.Combat; // 적 피해 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 생성
using ProjectEpsilon.Player; // 플레이어와 몸통
using ProjectEpsilon.Progression; // 경험치 보석
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedSizeAndHitVerification // 실제 크기와 피격 시험
    { // 클래스 시작
        private const string Key = "EpsilonSizeAndHitVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 대기 시작 시각
        static AutomatedSizeAndHitVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 연결 방지
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 추가 처리 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<SizeAndHitRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 연결 제거
            Debug.Log($"[SizeAndHitVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class SizeAndHitRunner : MonoBehaviour // 실제 장면 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 생성 경로 범위
        private IEnumerator Start() // 실제 크기와 피격 흐름
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 간격 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 생성기 조회
            PrototypeBossEncounter bossEncounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 생성기 조회
            player.enabled = false; // 시험 중 플레이어 이동 중지
            spawner.enabled = false; // 시험 외 적 생성 중지
            Require(Mathf.Abs(player.transform.lossyScale.x - 1.44f) < 0.001f, "Player head scale mismatch."); // 머리 두 배 크기 확인
            Require(body.BodySegments.Count == 3 && Mathf.Abs(body.BodySegments[0].transform.lossyScale.x - 0.62f) < 0.001f && Mathf.Abs(body.TailSegment.transform.lossyScale.x - 0.48f) < 0.001f, "Player body or tail scale mismatch."); // 축소된 몸통과 꼬리 크기 확인
            Require(Mathf.Abs(follower.SegmentSpacing - 0.58f) < 0.001f, "Body spacing mismatch."); // 축소된 몸통 간격 확인
            int[] values = { 1, 5, 20 }; // 경험치 등급 값
            float[] sizes = { 0.54f, 0.78f, 1.02f }; // 보석 세 배 크기
            for (int i = 0; i < values.Length; i++) // 경험치 등급 순회
            { // 반복 시작
                ExperiencePickup gem = ExperiencePickup.Spawn(Vector3.one * 50f, values[i], null, null); // 실제 경험치 보석 생성
                Require(gem.ExperienceValue == values[i] && Mathf.Abs(gem.transform.localScale.x - sizes[i]) < 0.001f, "Gem size or XP mismatch."); // 보석 크기와 경험치 보존 확인
                Destroy(gem.gameObject); // 시험 보석 제거
            } // 반복 끝
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 적 설정 조회
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 메서드 조회
            WeaponTarget normal = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[0], Vector3.one * 40f, false }); // 실제 일반 적 생성
            WeaponTarget elite = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[3], Vector3.one * 45f, false }); // 실제 엘리트 생성
            Require(Mathf.Abs(normal.transform.lossyScale.x - 1f) < 0.001f && Mathf.Abs(elite.transform.lossyScale.x - 1.6f) < 0.001f, "Enemy or elite scale mismatch."); // 실제 적 두 배 크기 확인
            MethodInfo contactDistance = typeof(AutomatedNaturalCombatVerification).GetMethod("ContactDistance", BindingFlags.Static | BindingFlags.NonPublic); // 자동 조종 충돌 거리 조회
            Require(contactDistance != null, "Natural pilot does not use actual collider distances."); // 크기 대응 경로 확인
            CircleCollider2D headCollider = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            CircleCollider2D enemyCollider = normal.GetComponent<CircleCollider2D>(); // 실제 일반 적 충돌체
            float expectedContact = headCollider.radius * Mathf.Abs(headCollider.transform.lossyScale.x) + enemyCollider.radius * Mathf.Abs(enemyCollider.transform.lossyScale.x); // 실제 크기 반영 접촉 거리
            float actualContact = (float)contactDistance.Invoke(null, new object[] { headCollider, enemyCollider }); // 자동 조종 접촉 거리
            Require(Mathf.Abs(actualContact - expectedContact) < 0.001f, "Natural pilot collider distance mismatch."); // 실제 접촉 거리 대조
            typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private).Invoke(bossEncounter, null); // 실제 보스 강제 생성
            WeaponTarget boss = GameObject.Find("PrototypeFinalBoss").GetComponent<WeaponTarget>(); // 실제 보스 대상 조회
            Require(Mathf.Abs(boss.transform.lossyScale.x - 2.8f) < 0.001f, "Boss scale mismatch."); // 보스 두 배 크기 확인
            WeaponTarget[] enemies = { normal, elite, boss }; // 실제 적 종류 목록
            Color[] originals = new Color[enemies.Length]; // 원래 적 색상 목록
            for (int i = 0; i < enemies.Length; i++) // 실제 적 종류 순회
            { // 반복 시작
                SpriteRenderer renderer = enemies[i].GetComponent<SpriteRenderer>(); // 실제 적 표시기
                originals[i] = renderer.color; // 기존 적 색상 기록
                enemies[i].TakeDamage(0f); // 무피해 입력
                Require(renderer.color == originals[i], "Zero damage changed color."); // 무피해 점멸 방지 확인
                enemies[i].TakeDamage(1f); // 실제 적 피해 적용
                Require(renderer.color.r > renderer.color.g && renderer.color.r > renderer.color.b, "Enemy did not flash red."); // 실제 빨간 피격 표시 확인
            } // 반복 끝
            yield return new WaitForSeconds(0.12f); // 첫 피격 표시 경과 대기
            normal.TakeDamage(1f); // 표시 중 반복 피해 적용
            yield return new WaitForSeconds(0.1f); // 첫 표시 종료 시각 경과 대기
            Color repeatedColor = normal.GetComponent<SpriteRenderer>().color; // 반복 피해 후 실제 색상
            Require(repeatedColor.r > repeatedColor.g && repeatedColor.r > repeatedColor.b, "Repeated hit did not extend the flash."); // 반복 피해 표시 연장 확인
            yield return new WaitForSeconds(0.2f); // 실제 피격 표시 종료 대기
            for (int i = 0; i < enemies.Length; i++) // 적 색상 복귀 순회
            { // 반복 시작
                Require(enemies[i] != null && enemies[i].GetComponent<SpriteRenderer>().color == originals[i], "Enemy color was not restored."); // 원래 색상 복구 확인
            } // 반복 끝
            AutomatedSizeAndHitVerification.Finish(0, "Player, enemy, elite and boss scale; gem sizes and XP; real hit flash and restoration."); // 전체 성공 기록
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedSizeAndHitVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
