using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Linq; // 실제 발사 방향 정렬
using System.Reflection; // 실제 사격 상태 조회
using ProjectEpsilon.Combat; // 실제 공격 대상과 무기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 사수와 보스
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossFanVerification // 실제 부채꼴 사격 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonBossFanVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedBossFanVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 결과 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (started <= 0d) // 실제 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for boss fan test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 선택과 실행 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 중복 실행 방지
            player.gameObject.AddComponent<BossFanVerificationRunner>(); // 실제 사격 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 시작 감시 해제
            Debug.Log($"[BossFanVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class BossFanVerificationRunner : MonoBehaviour // 실제 보스 사격 시나리오
    { // 클래스 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 상태 범위
        private IEnumerator Start() // 실제 보스와 투사체 시험
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 시험 보스 관리자
            Require(player != null && health != null && body != null && follower != null && spawner != null && encounter != null, "Actual boss scene fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 플레이어 이동 통제
            follower.enabled = false; // 실제 몸통 위치 통제
            spawner.enabled = false; // 자연 적 출현 통제
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            { // 반복 시작
                existing.gameObject.SetActive(false); // 기존 적의 접촉과 행동 차단
                Destroy(existing.gameObject); // 기존 자연 적 제거
            } // 반복 끝
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 자기 충돌 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 자기 충돌 방지
            } // 조건 끝
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 장착 무기 조회
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 장착 무기 순회
            { // 반복 시작
                slot.Clear(); // 시험 중 자동 공격 중지
            } // 반복 끝
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private); // 실제 보스 생성 경로 조회
            Require(spawn != null, "Actual boss creation path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 시험을 위한 실제 보스 강제 생성
            WeaponTarget boss = encounter.Boss; // 실제 생성 보스
            Require(boss != null && boss.IsAlive && !encounter.IsPhaseTwo, "Actual first-phase boss missing."); // 실제 첫 단계 확인
            EarlyEnemyBehaviour behaviour = boss.GetComponent<EarlyEnemyBehaviour>(); // 실제 보스 사격 행동
            Require(behaviour != null, "Actual boss shooting behaviour missing."); // 실제 행동 구성 확인
            behaviour.enabled = false; // 자동 사격과 이동 통제
            boss.transform.position = player.transform.position + Vector3.up * 30f; // 실제 접촉 밖 보스 위치
            FieldInfo directionField = typeof(EnemyTestProjectile).GetField("direction", Private); // 실제 발사 방향 조회
            FieldInfo speedField = typeof(EnemyTestProjectile).GetField("speed", Private); // 실제 투사체 속도 조회
            MethodInfo fire = typeof(EarlyEnemyBehaviour).GetMethod("FireProjectile", Private); // 실제 사격 경로 조회
            Require(directionField != null && speedField != null && fire != null, "Actual projectile inspection paths missing."); // 실제 시험 경로 확인
            GameObject shooterObject = new GameObject("ControlledOrdinaryShooter"); // 실제 일반 사수 객체
            shooterObject.AddComponent<Rigidbody2D>(); // 실제 일반 사수 물리체
            EarlyEnemyBehaviour shooter = shooterObject.AddComponent<EarlyEnemyBehaviour>(); // 실제 일반 사수 행동
            EarlyEnemySettings ordinary = new EarlyEnemySettings // 실제 일반 사수 설정
            { // 설정 시작
                kind = EarlyEnemyKind.Shooter, // 일반 사수 행동
                attackInterval = 2f, // 시험 사격 간격
                damage = 8 // 일반 사수 피해
            }; // 설정 끝
            shooter.Configure(ordinary, player, body, health, null, 4f, 5f, 3f, 1f, 0.6f, 3f); // 실제 일반 사수 구성
            shooter.enabled = false; // 시험 중 자동 사격 방지
            fire.Invoke(shooter, new object[] { Vector2.right }); // 실제 일반 사수 단발
            EnemyTestProjectile[] ordinaryShots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 일반 사격 조회
            VerifyFan(ordinaryShots, directionField, 1, 0f); // 기존 단발과 방향 확인
            ordinaryShots[0].gameObject.SetActive(false); // 다음 시험 전 기존 투사체 차단
            Destroy(ordinaryShots[0].gameObject); // 일반 시험 투사체 제거
            Destroy(shooterObject); // 일반 시험 사수 제거
            fire.Invoke(behaviour, new object[] { Vector2.right }); // 실제 첫 단계 사격
            EnemyTestProjectile[] first = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 생성 투사체
            VerifyFan(first, directionField, 3, 40f); // 실제 첫 단계 부채꼴 확인
            EnemyTestProjectile center = first.OrderBy(item => Mathf.Abs(((Vector2)directionField.GetValue(item)).y)).First(); // 실제 중앙 투사체 선택
            foreach (EnemyTestProjectile item in first) // 실제 첫 단계 투사체 순회
            { // 반복 시작
                if (item != center) // 중앙 투사체 외 확인
                { // 조건 시작
                    item.gameObject.SetActive(false); // 다른 투사체 접촉 차단
                    Destroy(item.gameObject); // 다른 시험 투사체 제거
                } // 조건 끝
            } // 반복 끝
            SnakeSegment segment = body.BodySegments[0]; // 실제 첫 몸통 선택
            segment.transform.position = player.transform.position + Vector3.right * 20f; // 실제 접촉 시험 위치
            health.ResetHealth(); // 실제 공용 체력 초기화
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            if (health.ShieldController != null) // 실제 보호막 확인
            { // 조건 시작
                health.ShieldController.Clear(); // 보호막 간섭 제거
            } // 조건 끝
            speedField.SetValue(center, 0f); // 실제 중앙 투사체 이동 정지
            center.transform.position = segment.transform.position; // 실제 몸통 접촉 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 물리 접촉 진행
            yield return null; // 물리 직후 피해 해결 대기
            yield return null; // 실제 지연 파괴 완료 대기
            Require(health.CurrentHealth == health.MaximumHealth - 15 && center == null, "Actual boss projectile contact did not apply one 15 HP hit."); // 실제 접촉 피해 확인
            boss.TakeDamage(boss.MaximumHealth * 0.5f); // 실제 두 번째 단계 피해
            yield return null; // 실제 체력 단계 갱신 대기
            Require(encounter.IsPhaseTwo && boss.IsAlive, "Actual boss did not enter the second phase."); // 실제 두 번째 단계 확인
            fire.Invoke(behaviour, new object[] { Vector2.right }); // 실제 두 번째 단계 사격
            EnemyTestProjectile[] second = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 두 번째 사격 투사체
            VerifyFan(second, directionField, 5, 40f); // 실제 다섯 방향 부채꼴 확인
            Debug.Log("[BossFanVerification] actual first phase=3, second phase=5, spread=40 degrees, real central projectile contact=15 HP; boss spawn and firing forced for testing."); // 실제 통제 조건 기록
            AutomatedBossFanVerification.Finish(0, "Actual boss fan spawning, phase change and projectile contact passed."); // 실제 전체 결과
        } // 코루틴 끝
        private static void VerifyFan(EnemyTestProjectile[] items, FieldInfo directionField, int count, float degrees) // 실제 대칭 사격 확인
        { // 메서드 시작
            Require(items.Length == count, $"Expected {count} actual shots, got {items.Length}."); // 실제 투사체 수 확인
            float[] angles = items.Select(item => Mathf.Atan2(((Vector2)directionField.GetValue(item)).y, ((Vector2)directionField.GetValue(item)).x) * Mathf.Rad2Deg).OrderBy(value => value).ToArray(); // 실제 발사 각도 정렬
            for (int index = 0; index < count; index++) // 실제 각도 순회
            { // 반복 시작
                float expected = count == 1 ? 0f : -degrees * 0.5f + degrees * index / (count - 1); // 기대 대칭 각도
                Require(Mathf.Abs(angles[index] - expected) < 0.1f, "Actual fan angle mismatch."); // 실제 균일 각도 확인
            } // 반복 끝
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedBossFanVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
