using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.World; // 실제 맵 생성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMapCoverVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonMapCoverVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedMapCoverVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            {
                EditorApplication.update += Tick; // 플레이 감시 연결
            }
        }
        public static void Run() // 실제 씬 시험 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 초기화 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 플레이 대기
            }
            if (started <= 0d) // 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            {
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<MapCoverRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[MapCoverVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class MapCoverRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 엄폐물과 양쪽 투사체 검증
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            Require(player != null && health != null && body != null && map != null, "Actual cover test dependencies missing."); // 실제 구성 확인
            player.enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 포탑 공격 중지
            GameObject chunk = new GameObject("CoverFixtureChunk"); // 격리된 실제 청크
            chunk.transform.position = new Vector3(500f, 500f, 0f); // 실제 플레이어와 분리
            System.Reflection.MethodInfo create = typeof(PrototypeChunkMap).GetMethod("CreateObstacle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // 실제 장애물 생성 경로
            Require(create != null, "Actual obstacle creation method missing."); // 생성 경로 확인
            Collider2D cover = null; // 실제 엄폐 충돌체 초기화
            for (int seed = 1; seed <= 200 && cover == null; seed++) // 실제 엄폐 이미지 탐색
            {
                UnityEngine.Random.InitState(seed); // 재현 가능한 생성 시드
                create.Invoke(map, new object[] { chunk.transform, 0 }); // 실제 맵 장애물 생성
                Transform item = chunk.transform.GetChild(chunk.transform.childCount - 1); // 새 장애물 조회
                SpriteRenderer renderer = item.GetComponent<SpriteRenderer>(); // 실제 표시기 조회
                if (renderer != null && renderer.sprite != null && renderer.sprite.name == "바위_엄폐") // 실제 평원 엄폐 이미지 확인
                {
                    cover = item.GetComponent<Collider2D>(); // 실제 엄폐 충돌체 저장
                }
                else // 다른 장애물 확인
                {
                    item.gameObject.SetActive(false); // 시험 중 불필요한 충돌 차단
                    Destroy(item.gameObject); // 다른 장애물 제거
                }
            }
            Require(cover != null, "Could not generate actual plains rock cover."); // 실제 엄폐물 확인
            GameObject turretShot = new GameObject("CoverTurretShot"); // 실제 포탑 탄 객체
            turretShot.transform.position = cover.bounds.center; // 실제 엄폐 충돌 범위 안 배치
            StraightProjectile turret = turretShot.AddComponent<StraightProjectile>(); // 기존 포탑 투사체
            WeaponAttributeAttackSnapshot snapshot = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Physical, 0, 0, 1, turretShot.transform.position, 5f); // 실제 공격 정보
            turret.Configure(Vector2.right, snapshot, null, 1f, 3f, null); // 기존 포탑 탄 설정
            GameObject enemyShot = new GameObject("CoverEnemyShot"); // 실제 적 탄 객체
            enemyShot.transform.position = cover.bounds.center; // 실제 엄폐 충돌 범위 안 배치
            EnemyTestProjectile enemy = enemyShot.AddComponent<EnemyTestProjectile>(); // 기존 적 투사체
            enemy.Configure(Vector2.right, 1f, 3f, 5, health, body, player, null); // 기존 적 탄 설정
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 트리거 감지
            yield return null; // 실제 지연 파괴 처리
            Require(turretShot == null && enemyShot == null, "Actual player or enemy projectile passed through generated cover."); // 양쪽 투사체 차단 확인
            GameObject ordinary = new GameObject("OrdinaryObstacle"); // 일반 장애물 시험 객체
            ordinary.transform.position = new Vector3(700f, 700f, 0f); // 다른 충돌과 분리된 위치
            BoxCollider2D ordinaryCollider = ordinary.AddComponent<BoxCollider2D>(); // 실제 일반 충돌체
            ordinaryCollider.isTrigger = true; // 기존 장애물 접촉 방식
            ordinary.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 투사체를 막지 않는 일반 장애물
            GameObject ordinaryTurretShot = new GameObject("OrdinaryTurretShot"); // 일반 장애물의 포탑 탄
            ordinaryTurretShot.transform.position = ordinary.transform.position; // 실제 일반 장애물 안 배치
            ordinaryTurretShot.AddComponent<StraightProjectile>().Configure(Vector2.right, snapshot, null, 1f, 3f, null); // 기존 포탑 탄 설정
            GameObject ordinaryEnemyShot = new GameObject("OrdinaryEnemyShot"); // 일반 장애물의 적 탄
            ordinaryEnemyShot.transform.position = ordinary.transform.position; // 실제 일반 장애물 안 배치
            ordinaryEnemyShot.AddComponent<EnemyTestProjectile>().Configure(Vector2.right, 1f, 3f, 5, health, body, player, null); // 기존 적 탄 설정
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 트리거 감지
            yield return null; // 지연 파괴 여부 확인
            Require(ordinaryTurretShot != null && ordinaryEnemyShot != null && ordinaryTurretShot.activeInHierarchy && ordinaryEnemyShot.activeInHierarchy, "Ordinary obstacle unexpectedly blocked a projectile."); // 엄폐가 아닌 장애물의 기존 동작 유지
            AutomatedMapCoverVerification.Finish(0, "Actual generated cover blocked both turret and enemy projectiles."); // 실제 검증 성공 기록
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedMapCoverVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
