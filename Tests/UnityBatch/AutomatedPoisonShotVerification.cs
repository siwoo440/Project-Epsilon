using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 사격 경로 조회
using ProjectEpsilon.Combat; // 실제 독 상태와 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 사수와 투사체
using ProjectEpsilon.Player; // 실제 플레이어 구성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedPoisonShotVerification // 실제 독 약화 사격 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonPoisonShotVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedPoisonShotVerification() // 도메인 재시작 처리
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
        private static void Tick() // 실제 시작과 시험 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for poison shot test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            { // 조건 시작
                return; // 실제 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<PoisonShotVerificationRunner>(); // 실제 독 사격 시험 실행
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
            Debug.Log($"[PoisonShotVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class PoisonShotVerificationRunner : MonoBehaviour // 실제 독 사격 시나리오
    { // 클래스 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 범위
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeSegment segment; // 실제 접촉 몸통
        private MethodInfo fire; // 실제 사수 발사 경로
        private FieldInfo shotSpeed; // 실제 투사체 속도
        private FieldInfo shotDamage; // 실제 발사 피해 스냅샷

        private IEnumerator Start() // 실제 사격과 접촉 사례
        { // 코루틴 시작
            player = GetComponent<SnakeMovement>(); // 실제 머리 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            fire = typeof(EarlyEnemyBehaviour).GetMethod("FireProjectile", Private); // 실제 사격 경로 조회
            shotSpeed = typeof(EnemyTestProjectile).GetField("speed", Private); // 실제 투사체 속도 조회
            shotDamage = typeof(EnemyTestProjectile).GetField("damage", Private); // 실제 투사체 피해 조회
            Require(player != null && health != null && body != null && follower != null && spawner != null && fire != null && shotSpeed != null && shotDamage != null, "Actual poison shot fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 머리 이동 통제
            follower.enabled = false; // 실제 몸통 추적 통제
            spawner.enabled = false; // 실제 일반 적 출현 통제
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 실제 자기 충돌 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 자기 충돌 방지
            } // 조건 끝
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 기존 적 순회
            { // 반복 시작
                existing.gameObject.SetActive(false); // 기존 적의 접촉 차단
                Destroy(existing.gameObject); // 기존 적 제거
            } // 반복 끝
            segment = body.BodySegments[0]; // 실제 첫 몸통 선택
            segment.transform.position = player.transform.position + Vector3.right * 20f; // 실제 접촉 시험 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return VerifyShot(21, false, false, false, 21); // 독 없는 실제 기준 피해
            yield return VerifyShot(21, true, false, false, 20); // 발사 전 독 약화 피해
            yield return VerifyShot(21, true, true, false, 20); // 발사 후 독 제거 스냅샷
            yield return VerifyShot(21, false, false, true, 21); // 발사 후 독 추가 스냅샷
            yield return VerifyShot(8, true, false, false, 8); // 기본 피해의 기존 올림 유지
            AutomatedPoisonShotVerification.Finish(0, "Actual shooter poison snapshot and physical contact cases passed."); // 실제 전체 결과
        } // 코루틴 끝

        private IEnumerator VerifyShot(int baseDamage, bool poisonedAtLaunch, bool clearAfterLaunch, bool poisonAfterLaunch, int expectedDamage) // 실제 발사와 접촉 시험
        { // 코루틴 시작
            health.ResetHealth(); // 실제 공용 체력 초기화
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            if (health.ShieldController != null) // 실제 보호막 확인
            { // 조건 시작
                health.ShieldController.Clear(); // 실제 보호막 간섭 제거
            } // 조건 끝
            GameObject shooterObject = new GameObject("PoisonTestShooter"); // 실제 시험 사수 객체
            shooterObject.transform.position = player.transform.position + Vector3.up * 30f; // 실제 접촉 밖 사수 위치
            shooterObject.AddComponent<Rigidbody2D>(); // 실제 사수 물리체
            WeaponTarget target = shooterObject.AddComponent<WeaponTarget>(); // 실제 독 적용 대상
            target.Configure(100f); // 실제 대상 체력 설정
            EarlyEnemyBehaviour shooter = shooterObject.AddComponent<EarlyEnemyBehaviour>(); // 실제 사수 행동
            EarlyEnemySettings settings = new EarlyEnemySettings // 실제 사수 설정
            { // 설정 시작
                kind = EarlyEnemyKind.Shooter, // 실제 사격 행동
                damage = baseDamage, // 실제 시험 기본 피해
                attackInterval = 2f // 실제 시험 사격 간격
            }; // 설정 끝
            shooter.Configure(settings, player, body, health, null, 4f, 5f, 3f, 1f, 0.6f, 3f); // 실제 사수 참조 구성
            shooter.enabled = false; // 시험 중 자동 사격과 이동 중지
            WeaponTargetPoisonStatus poison = null; // 실제 독 상태 초기화
            if (poisonedAtLaunch) // 발사 전 독 적용 확인
            { // 조건 시작
                poison = shooterObject.AddComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태 생성
                Require(poison.Apply(target, 2), "Could not apply actual poison before launch."); // 실제 독 시너지 적용
            } // 조건 끝
            fire.Invoke(shooter, new object[] { Vector2.right }); // 실제 사수 발사 경로 실행
            EnemyTestProjectile[] shots = UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None); // 실제 생성 투사체 조회
            Require(shots.Length == 1, "Actual ordinary shooter did not fire exactly one projectile."); // 실제 단발 확인
            EnemyTestProjectile shot = shots[0]; // 실제 단일 투사체
            int capturedDamage = (int)shotDamage.GetValue(shot); // 실제 발사 피해 스냅샷
            if (clearAfterLaunch) // 발사 후 독 제거 확인
            { // 조건 시작
                poison.Clear(); // 실제 독 상태 제거
            } // 조건 끝
            if (poisonAfterLaunch) // 발사 후 독 추가 확인
            { // 조건 시작
                poison = shooterObject.AddComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태 생성
                Require(poison.Apply(target, 2), "Could not apply actual poison after launch."); // 실제 독 시너지 적용
            } // 조건 끝
            shotSpeed.SetValue(shot, 0f); // 실제 투사체 이동 중지
            shot.transform.position = segment.transform.position; // 실제 몸통 접촉 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 물리 접촉 진행
            yield return null; // 실제 피해 처리 대기
            yield return null; // 실제 지연 파괴 완료 대기
            Require(capturedDamage == expectedDamage && health.CurrentHealth == health.MaximumHealth - expectedDamage && shot == null, $"Actual shot mismatch: base={baseDamage}, poisonedAtLaunch={poisonedAtLaunch}, captured={capturedDamage}, HP={health.CurrentHealth}, expectedDamage={expectedDamage}."); // 실제 피해와 발사 시점 스냅샷 확인
            Debug.Log($"[PoisonShotVerification] actual base={baseDamage}, poisonedAtLaunch={poisonedAtLaunch}, clearAfterLaunch={clearAfterLaunch}, poisonAfterLaunch={poisonAfterLaunch}, captured={capturedDamage}, HP={health.CurrentHealth}."); // 실제 통제 조건과 피해 기록
            shooterObject.SetActive(false); // 실제 시험 사수 차단
            Destroy(shooterObject); // 실제 시험 사수 제거
            yield return null; // 실제 지연 파괴 완료 대기
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedPoisonShotVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
