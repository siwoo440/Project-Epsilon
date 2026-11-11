using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 보스 위험 구역 설정 조회
using ProjectEpsilon.Combat; // 실제 보스와 무기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적과 보스
using ProjectEpsilon.Player; // 실제 몸통과 체력
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossHazardVerification // 실제 보스 위험 구역 검증
    {
        private const string ActiveKey = "EpsilonBossHazardVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedBossHazardVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시
            }
        }
        public static void Run() // 실제 씬 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 시작과 결과 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 실제 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            {
                Finish(1, "Timed out waiting for boss hazard test."); // 시간 초과 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            {
                return; // 실제 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 실제 초기화 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossHazardVerificationRunner>(); // 실제 위험 구역 시험 실행
        }
        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 시작 감시 해제
            Debug.Log($"[BossHazardVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }

    public sealed class BossHazardVerificationRunner : MonoBehaviour // 실제 보스 위험 구역 시나리오
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 설정 범위
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private WeaponTarget boss; // 실제 보스 대상
        private Component hazardSpawner; // 실제 위험 구역 출현기
        private PropertyInfo currentHazard; // 실제 위험 구역 조회
        private PropertyInfo warningState; // 실제 예고 상태 조회
        private PropertyInfo activeState; // 실제 위험 상태 조회
        private FieldInfo nextHazardAt; // 실제 다음 위험 구역 시각

        private IEnumerator Start() // 실제 예고와 위험 구역 피해
        {
            player = GetComponent<SnakeMovement>(); // 실제 머리 이동 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자 조회
            Require(player != null && health != null && body != null && follower != null && spawner != null && encounter != null, "Actual boss hazard fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 플레이어 이동 통제
            follower.enabled = false; // 실제 몸통 추적 통제
            spawner.enabled = false; // 실제 일반 적 출현 통제
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 실제 자기 충돌 확인
            {
                self.enabled = false; // 시험 중 자기 충돌 방지
            }
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 기존 적 순회
            {
                existing.gameObject.SetActive(false); // 실제 기존 적 접촉 차단
                Destroy(existing.gameObject); // 실제 기존 적 제거
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            {
                slot.Clear(); // 시험 중 자동 공격 방지
            }
            SetEncounter(encounter, "hazardInterval", 20f); // 실제 구역 간격 통제
            SetEncounter(encounter, "hazardWarningDuration", 0.35f); // 실제 예고 시간 통제
            SetEncounter(encounter, "hazardDuration", 1.2f); // 실제 위험 시간 통제
            SetEncounter(encounter, "hazardDamageInterval", 0.25f); // 실제 피해 간격 통제
            SetEncounter(encounter, "hazardRadius", 1.2f); // 실제 구역 반경 통제
            SetEncounter(encounter, "hazardDamage", 10); // 실제 구역 피해 통제
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private); // 실제 보스 생성 경로
            Require(spawn != null, "Actual boss creation path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 시험용 실제 보스 생성
            boss = encounter.Boss; // 실제 생성 보스
            Type spawnerType = typeof(PrototypeBossEncounter).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossHazardSpawner"); // 실제 위험 구역 출현기 형식
            Type hazardType = typeof(PrototypeBossEncounter).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossHazard"); // 실제 위험 구역 형식
            hazardSpawner = boss == null || spawnerType == null ? null : boss.GetComponent(spawnerType); // 실제 위험 구역 출현기
            currentHazard = spawnerType == null ? null : spawnerType.GetProperty("CurrentHazard"); // 실제 위험 구역 조회
            nextHazardAt = spawnerType == null ? null : spawnerType.GetField("nextHazardAt", Private); // 실제 다음 구역 시각
            warningState = hazardType == null ? null : hazardType.GetProperty("IsWarning"); // 실제 예고 상태
            activeState = hazardType == null ? null : hazardType.GetProperty("IsActive"); // 실제 위험 상태
            Require(hazardSpawner != null && currentHazard != null && nextHazardAt != null && warningState != null && activeState != null, "Actual boss hazard behaviour missing."); // 실제 위험 구역 구성 확인
            boss.GetComponent<EarlyEnemyBehaviour>().enabled = false; // 실제 보스 사격과 이동 통제
            boss.GetComponent<PrototypeBossChargeAttack>().enabled = false; // 실제 보스 돌진 통제
            yield return VerifyAvoidedHazard(); // 실제 예고 후 구역 회피
            yield return VerifyContactAndVictoryCleanup(); // 실제 지속 접촉 피해와 승리 정리
            AutomatedBossHazardVerification.Finish(0, "Actual warning, avoidance, periodic overlap damage and boss death cleanup passed."); // 실제 전체 결과
        }

        private IEnumerator VerifyAvoidedHazard() // 실제 예고 구역 회피
        {
            health.ResetHealth(); // 실제 공용 체력 초기화
            MoveBodiesAway(); // 실제 몸통 구역 접촉 방지
            nextHazardAt.SetValue(hazardSpawner, Time.time); // 실제 다음 구역 즉시 준비
            Component hazard = null; // 실제 위험 구역 초기화
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 시험 제한 시각
            while (hazard == null && Time.realtimeSinceStartup < deadline) // 실제 위험 구역 생성 대기
            {
                hazard = currentHazard.GetValue(hazardSpawner) as Component; // 실제 위험 구역 조회
                yield return null; // 실제 게임 프레임 진행
            }
            Require(hazard != null && (bool)warningState.GetValue(hazard) && !(bool)activeState.GetValue(hazard), "Actual hazard warning did not appear first."); // 실제 예고 우선 확인
            LineRenderer warning = hazard.GetComponent<LineRenderer>(); // 실제 구역 예고선 조회
            Require(warning != null && warning.enabled && warning.positionCount > 8 && health.CurrentHealth == health.MaximumHealth, "Actual hazard warning visual or safe period missing."); // 실제 예고선과 무피해 확인
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.2f); // 실제 정지 시간 대기
            Require((bool)warningState.GetValue(hazard) && health.CurrentHealth == health.MaximumHealth, "Actual hazard warning advanced during pause."); // 실제 정지 중 예고 유지
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            player.transform.position += Vector3.right * 5f; // 예고 구역 밖 실제 이동 통제
            Physics2D.SyncTransforms(); // 실제 머리 위치 반영
            while (hazard != null && !(bool)activeState.GetValue(hazard) && Time.realtimeSinceStartup < deadline) // 실제 위험 단계 대기
            {
                yield return null; // 실제 프레임 진행
            }
            Require(hazard != null && (bool)activeState.GetValue(hazard), "Actual hazard did not activate after warning."); // 실제 위험 단계 확인
            yield return new WaitForSeconds(0.4f); // 실제 위험 단계 경과
            Require(health.CurrentHealth == health.MaximumHealth, "Actual player outside the warned hazard took damage."); // 실제 구역 회피 확인
            while (hazard != null && Time.realtimeSinceStartup < deadline) // 실제 구역 종료 대기
            {
                yield return null; // 실제 프레임 진행
            }
            Require(hazard == null, "Actual temporary hazard did not expire."); // 실제 구역 제거 확인
            Debug.Log("[BossHazardVerification] actual warning paused correctly, player avoided the activated zone, and the temporary zone expired."); // 실제 회피 결과 기록
        }

        private IEnumerator VerifyContactAndVictoryCleanup() // 실제 중복 접촉 피해와 보스 사망 정리
        {
            health.ResetHealth(); // 실제 공용 체력 초기화
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            SnakeSegment segment = body.BodySegments[0]; // 실제 첫 몸통
            segment.transform.position = player.transform.position; // 실제 머리와 몸통 동시 접촉
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            nextHazardAt.SetValue(hazardSpawner, Time.time); // 실제 다음 구역 즉시 준비
            Component hazard = null; // 실제 위험 구역 초기화
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 시험 제한 시각
            while (hazard == null && Time.realtimeSinceStartup < deadline) // 실제 위험 구역 생성 대기
            {
                hazard = currentHazard.GetValue(hazardSpawner) as Component; // 실제 위험 구역 조회
                yield return null; // 실제 게임 프레임 진행
            }
            Require(hazard != null, "Actual contact hazard did not spawn."); // 실제 위험 구역 확인
            while (!(bool)activeState.GetValue(hazard) && Time.realtimeSinceStartup < deadline) // 실제 위험 단계 대기
            {
                yield return null; // 실제 게임 프레임 진행
            }
            yield return new WaitForFixedUpdate(); // 실제 물리 피해 처리 대기
            yield return null; // 실제 충돌 우선순위 처리 대기
            Require(health.CurrentHealth == health.MaximumHealth - 10, "Head and body overlap did not apply exactly one first hazard hit."); // 실제 동일 구역 중복 피해 방지 확인
            yield return new WaitForSeconds(0.35f); // 실제 다음 피해 주기 대기
            Require(health.CurrentHealth <= health.MaximumHealth - 20 && health.CurrentHealth >= health.MaximumHealth - 30, "Actual persistent overlap did not apply periodic hazard damage."); // 실제 지속 접촉 피해 확인
            boss.TakeDamage(boss.MaximumHealth); // 실제 보스 강제 처치
            Require(GameManager.Instance.CurrentState == GameState.Victory && Mathf.Approximately(Time.timeScale, 0f), "Actual boss death did not reach victory."); // 실제 보스 사망 승리 확인
            yield return null; // 실제 보스 사망 후 구역 갱신
            yield return null; // 실제 구역 지연 파괴 완료
            Require(hazard == null, "Actual hazard remained after boss death."); // 실제 보스 사망 구역 정리 확인
            Debug.Log($"[BossHazardVerification] actual head and body overlap produced one hit per interval; HP={health.CurrentHealth}; boss death removed the active hazard."); // 실제 통제 피해 결과
        }

        private void MoveBodiesAway() // 실제 몸통의 시험 구역 접촉 방지
        {
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            {
                body.BodySegments[index].transform.position = player.transform.position + Vector3.right * (20f + index * 4f); // 실제 구역 밖 위치
            }
            Physics2D.SyncTransforms(); // 실제 몸통 위치 반영
        }
        private static void SetEncounter(PrototypeBossEncounter encounter, string name, object value) // 실제 보스 시험 설정
        {
            FieldInfo field = typeof(PrototypeBossEncounter).GetField(name, Private); // 실제 설정 필드 조회
            Require(field != null, "Actual adjustable boss hazard settings missing: " + name); // 실제 설정 존재 확인
            field.SetValue(encounter, value); // 실제 시험 설정 주입
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedBossHazardVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
