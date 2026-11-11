using System; // 기본 예외와 인수
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 제품 계약 조회
using ProjectEpsilon.Combat; // 실제 보스 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 보스와 적
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.World; // 실제 청크 맵
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 로드
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedDay46MapsVerification // 실제 맵과 보스 검증
    {
        private const string Key = "EpsilonDay46Maps"; // 시험 진행 표식
        private static bool prepared; // 시작 맵 선택 완료
        private static bool injected; // 실행기 중복 방지
        private static int selected; // 선택할 맵 번호
        private static double started; // 실제 시험 시작
        static AutomatedDay46MapsVerification() // 도메인 갱신 복구
        {
            if (SessionState.GetBool(Key, false)) // 활성 시험 확인
            {
                EditorApplication.update += Tick; // 시험 감시 연결
            }
        }
        public static void Run() // 실제 배치 진입
        {
            string[] args = Environment.GetCommandLineArgs(); // 배치 인수 조회
            for (int i = 0; i + 1 < args.Length; i++) // 배치 인수 순회
            {
                if (args[i] == "-epsilonMapIndex") // 맵 번호 인수 확인
                {
                    int.TryParse(args[i + 1], out selected); // 시험 맵 번호 저장
                }
            }
            SessionState.SetInt("EpsilonDay46MapIndex", selected); // 도메인 갱신 간 맵 번호 보존
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 진행 표식
            EditorApplication.update += Tick; // 시험 감시 시작
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 시작 상태 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (started <= 0d) // 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 확인
            {
                Finish(1, "Timed out."); // 제한 시간 실패
                return; // 추가 처리 중단
            }
            if (!prepared) // 시작 전 맵 구성 확인
            {
                Type type = typeof(PrototypeChunkMap).Assembly.GetType("ProjectEpsilon.World.PrototypeMapSelection"); // 실제 맵 선택 형식
                Component selection = type == null ? null : UnityEngine.Object.FindFirstObjectByType(type) as Component; // 실제 선택 구성
                if (selection == null) // 맵 선택 부재 확인
                {
                    Finish(1, "Actual map selection is missing."); // 선행 실패 기록
                    return; // 선택 불가 처리
                }
                PropertyInfo count = type.GetProperty("ProfileCount"); // 실제 맵 수
                MethodInfo choose = type.GetMethod("SelectMap"); // 실제 맵 선택
                if (count == null || choose == null || (int)count.GetValue(selection) != 3) // 세 맵 계약 확인
                {
                    Finish(1, "Three map profiles or selection path are missing."); // 맵 구성 실패
                    return; // 선택 불가 처리
                }
                selected = SessionState.GetInt("EpsilonDay46MapIndex", 0); // 저장된 맵 번호
                if (!(bool)choose.Invoke(selection, new object[] { selected })) // 실제 선택 적용
                {
                    Finish(1, "Selected map was rejected before the run."); // 선택 실패
                    return; // 추가 처리 중단
                }
                prepared = true; // 실제 맵 선택 완료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 후보 처리
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 시작 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 중복 실행 방지
            player.gameObject.AddComponent<Day46MapsRunner>(); // 실제 시나리오 시작
        }
        public static void Finish(int code, string message) // 실제 결과 기록
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시험 감시 해제
            Debug.Log($"[Day46MapsVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day46MapsRunner : MonoBehaviour // 실제 맵별 전투 시나리오
    {
        private IEnumerator Start() // 실제 씬 검증
        {
            int index = SessionState.GetInt("EpsilonDay46MapIndex", 0); // 선택 맵 번호
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(map != null && encounter != null && wave != null, "Actual scene fixtures missing."); // 실제 필수 구성 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 이동 통제
            wave.enabled = false; // 자연 적 출현 통제
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            {
                existing.gameObject.SetActive(false); // 시험 중 자연 적 전투 차단
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            if (weapons != null) // 무기 관리자 확인
            {
                weapons.enabled = false; // 통제 시험의 자동 공격 차단
            }
            yield return null; // 실제 청크 생성 대기
            Require(map.ActiveCount == 9, "Selected map did not create nine chunks."); // 세 맵의 실제 청크 수
            GameObject center = map.GetChunk(Vector2Int.zero); // 실제 중앙 청크
            Require(center != null && center.transform.Find("Ground") != null, "Selected map ground missing."); // 실제 바닥 확인
            SpriteRenderer ground = center.transform.Find("Ground").GetComponent<SpriteRenderer>(); // 실제 바닥 렌더러
            string[] names = { "PlainsGround_Casual", "RuinsGround_Prototype", "SwampGround_Prototype" }; // 맵별 실제 바닥 이름
            Require(ground != null && ground.sprite != null && ground.sprite.name == names[index], "Selected map ground does not match its profile."); // 실제 맵 배경 연결
            FieldInfo obstacleChance = map.Profile.GetType().GetField("obstacleChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 장애물 확률 필드
            FieldInfo zoneChance = map.Profile.GetType().GetField("specialZoneChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 지역 확률 필드
            Require(obstacleChance != null && zoneChance != null, "Selected map generation data missing."); // 실제 맵 생성 설정 확인
            obstacleChance.SetValue(map.Profile, 1f); // 시험용 장애물 생성 확정
            zoneChance.SetValue(map.Profile, 1f); // 시험용 지역 생성 확정
            transform.position = new Vector3(map.ChunkSize * 30f, map.ChunkSize * 30f, 0f); // 실제 새 청크 위치
            yield return null; // 실제 새 청크 생성 대기
            yield return null; // 실제 지연 제거 대기
            GameObject generated = map.GetChunk(new Vector2Int(30, 30)); // 실제 맵별 청크
            SnakeContactHazard[] obstacles = generated == null ? null : generated.GetComponentsInChildren<SnakeContactHazard>(); // 실제 장애물 패턴
            PrototypeSpecialZone[] zones = generated == null ? null : generated.GetComponentsInChildren<PrototypeSpecialZone>(); // 실제 특수 지역
            Require(obstacles != null && obstacles.Length == 2 && zones != null && zones.Length == 1, "Selected map did not generate its obstacle pattern and special zone."); // 실제 맵별 확률 적용 확인
            Vector3 difference = obstacles[0].transform.position - obstacles[1].transform.position; // 사전 패턴 간 방향
            Require(index == 1 ? Mathf.Abs(difference.y) > Mathf.Abs(difference.x) : Mathf.Abs(difference.x) > Mathf.Abs(difference.y), "Selected map obstacle pattern orientation is wrong."); // 맵별 실제 배치 차이 확인
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성
            Require(spawn != null, "Boss spawn path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 실제 보스 강제 생성
            WeaponTarget boss = encounter.Boss; // 실제 보스 조회
            Require(boss != null && boss.IsAlive && boss.GetComponent<EarlyEnemyBehaviour>() != null, "Selected map boss did not spawn with combat."); // 실제 보스 전투 구성
            float[] expectedHealth = { 600f, 700f, 650f }; // 맵별 실제 임시 체력
            int[] expectedFirstShots = { 3, 3, 4 }; // 맵별 첫 단계 발사 수
            int[] expectedSecondShots = { 5, 5, 6 }; // 맵별 두 번째 발사 수
            Require(Mathf.Approximately(boss.MaximumHealth, expectedHealth[index]), "Selected map boss health does not match its data."); // 실제 맵별 보스 데이터 확인
            EarlyEnemyBehaviour behavior = boss.GetComponent<EarlyEnemyBehaviour>(); // 실제 보스 사격 행동
            behavior.enabled = false; // 통제 중 자동 사격 중지
            MethodInfo fire = typeof(EarlyEnemyBehaviour).GetMethod("FireProjectile", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 부채꼴 사격 경로
            Require(fire != null, "Actual boss firing path missing."); // 실제 사격 경로 확인
            fire.Invoke(behavior, new object[] { Vector2.right }); // 실제 첫 단계 사격
            EnemyTestProjectile[] firstShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 첫 단계 투사체
            Require(firstShots.Length == expectedFirstShots[index], "Selected map boss first-phase firing count differs from its data."); // 실제 첫 단계 발사 확인
            foreach (EnemyTestProjectile shot in firstShots) // 실제 첫 단계 투사체 순회
            {
                shot.gameObject.SetActive(false); // 다음 단계 시험 간섭 차단
            }
            boss.TakeDamage(boss.MaximumHealth * 0.5f); // 실제 두 번째 단계 경계
            yield return null; // 실제 보스 단계 갱신
            Require(encounter.IsPhaseTwo, "Selected map boss did not enter its second phase."); // 실제 두 번째 단계 확인
            fire.Invoke(behavior, new object[] { Vector2.right }); // 실제 두 번째 단계 사격
            EnemyTestProjectile[] secondShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 두 번째 단계 투사체
            Require(secondShots.Length == expectedSecondShots[index], "Selected map boss second-phase firing count differs from its data."); // 실제 두 번째 단계 발사 확인
            AutomatedDay46MapsVerification.Finish(0, $"Map {index}: actual profile, nine chunks, ground, boss HP, both firing phases and transition passed."); // 맵별 시험 성공
        }
        private static void Require(bool condition, string message) // 실제 시험 조건
        {
            if (!condition) // 실제 실패 확인
            {
                AutomatedDay46MapsVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 잘못된 후속 시험 중단
            }
        }
    }
}
