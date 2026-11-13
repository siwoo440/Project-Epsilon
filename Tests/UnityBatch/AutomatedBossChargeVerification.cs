using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 보스 상태 조회
using ProjectEpsilon.Combat; // 실제 보스와 장착 무기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 보스와 적
using ProjectEpsilon.Player; // 실제 플레이어 구성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossChargeVerification // 실제 보스 돌진 검증
    {
        private const string ActiveKey = "EpsilonBossChargeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시작 시각
        static AutomatedBossChargeVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 시험 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시
            }
        }
        public static void Run() // 실제 씬 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        }
        private static void Tick() // 실제 시작과 결과 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 시간 제한
            {
                Finish(1, "Timed out waiting for boss charge test."); // 제한 시간 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 상태 확인
            {
                return; // 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossChargeVerificationRunner>(); // 실제 돌진 시험 실행
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
            Debug.Log($"[BossChargeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }

    public sealed class BossChargeContactProbe : MonoBehaviour // 실제 돌진 물리 접촉 관찰기
    {
        public SnakeMovement Player; // 실제 플레이어 머리
        public SnakeBodyManager Body; // 실제 몸통 관리자
        public int HeadEntries; // 실제 머리 진입 수
        public int BodyEntries; // 실제 몸통 진입 수
        public bool BodyEnteredFirst; // 머리 이전 몸통 진입 여부
        private void OnTriggerEnter2D(Collider2D other) // 실제 물리 진입 관찰
        {
            if (Player != null && other.transform == Player.transform) // 실제 머리 접촉 확인
            {
                HeadEntries++; // 실제 머리 진입 기록
                return; // 몸통 판정 생략
            }
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 접촉 몸통 조회
            if (segment != null && Body != null && segment.transform.IsChildOf(Body.transform)) // 실제 플레이어 몸통 확인
            {
                BodyEnteredFirst |= BodyEntries == 0 && HeadEntries == 0; // 몸통 선행 접촉 기록
                BodyEntries++; // 실제 몸통 진입 기록
            }
        }
    }

    public sealed class BossChargeVerificationRunner : MonoBehaviour // 실제 보스 돌진 시나리오
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 상태 범위
        private SnakeMovement player; // 실제 플레이어 이동
        private SnakeHealth health; // 실제 공용 체력
        private SnakeBodyManager body; // 실제 몸통 관리자
        private PrototypeBossEncounter encounter; // 실제 보스 관리자
        private WeaponTarget boss; // 실제 보스 대상
        private Component charge; // 실제 돌진 행동
        private PropertyInfo warningState; // 실제 예고 상태
        private PropertyInfo chargeState; // 실제 돌진 상태
        private FieldInfo nextCharge; // 실제 다음 돌진 시각
        private BossChargeContactProbe contactProbe; // 실제 물리 접촉 관찰기

        private IEnumerator Start() // 실제 예고와 충돌 실행
        {
            player = GetComponent<SnakeMovement>(); // 실제 머리 이동 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기 조회
            Require(player != null && health != null && body != null && follower != null && spawner != null && encounter != null, "Actual boss charge fixtures missing."); // 실제 구성 확인
            player.enabled = false; // 플레이어 이동 통제
            follower.enabled = false; // 몸통 추적 통제
            spawner.enabled = false; // 일반 적 출현 통제
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 실제 자기 충돌 확인
            {
                self.enabled = false; // 시험 중 자기 충돌 방지
            }
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 자연 적 순회
            {
                existing.gameObject.SetActive(false); // 기존 적 접촉 차단
                Destroy(existing.gameObject); // 기존 적 제거
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 조회
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 슬롯 순회
            {
                slot.Clear(); // 시험 중 자동 공격 중지
            }
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private); // 실제 보스 생성 경로 조회
            Require(spawn != null, "Actual boss spawn path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 보스 강제 생성
            boss = encounter.Boss; // 실제 보스 대상
            if (boss != null && boss.GetComponent<EarlyEnemyBehaviour>() != null) // 실제 일반 사격 행동 확인
            {
                boss.GetComponent<EarlyEnemyBehaviour>().enabled = false; // 통제 중 자동 사격과 추적 중지
            }
            Type chargeType = typeof(PrototypeBossEncounter).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossChargeAttack"); // 실제 돌진 행동 형식 조회
            charge = boss == null || chargeType == null ? null : boss.GetComponent(chargeType); // 실제 보스 돌진 행동 조회
            warningState = chargeType == null ? null : chargeType.GetProperty("IsWarning"); // 실제 예고 상태 조회
            chargeState = chargeType == null ? null : chargeType.GetProperty("IsCharging"); // 실제 돌진 상태 조회
            nextCharge = chargeType == null ? null : chargeType.GetField("nextChargeAt", Private); // 실제 돌진 시각 조회
            Require(charge != null && warningState != null && chargeState != null && nextCharge != null, "Actual boss warning and charge behaviour missing."); // 실제 돌진 구성 확인
            contactProbe = boss.gameObject.AddComponent<BossChargeContactProbe>(); // 실제 물리 접촉 관찰기
            contactProbe.Player = player; // 실제 머리 참조
            contactProbe.Body = body; // 실제 몸통 참조
            FieldInfo warningDuration = chargeType.GetField("warningDuration", Private); // 실제 예고 시간 조회
            FieldInfo chargeDuration = chargeType.GetField("chargeDuration", Private); // 실제 돌진 시간 조회
            FieldInfo chargeSpeed = chargeType.GetField("chargeSpeed", Private); // 실제 돌진 속도 조회
            Require(warningDuration != null && chargeDuration != null && chargeSpeed != null, "Actual boss charge settings missing."); // 실제 임시 설정 확인
            warningDuration.SetValue(charge, 0.35f); // 실제 예고 시간 통제
            chargeDuration.SetValue(charge, 1f); // 실제 돌진 시간 통제
            chargeSpeed.SetValue(charge, 8f); // 실제 돌진 속도 통제
            yield return TestCharge(false, false, false); // 실제 첫 단계 HP 피해 시험
            boss.TakeDamage(boss.MaximumHealth * 0.5f); // 실제 두 번째 단계 체력 변경
            yield return null; // 실제 단계 갱신 대기
            Require(encounter.IsPhaseTwo && boss.IsAlive, "Actual boss second phase missing."); // 실제 두 번째 단계 확인
            yield return TestCharge(true, false, false); // 실제 머리 접촉 절단 시험
            yield return TestCharge(true, false, true); // 실제 몸통과 머리 연속 접촉 시험
            yield return TestCharge(true, true, false); // 실제 전체 무적 중 절단 방지 시험
            AutomatedBossChargeVerification.Finish(0, "Actual warning pause, phase-one HP hit, phase-two body cut and invulnerability passed."); // 실제 전체 결과
        }

        private IEnumerator TestCharge(bool phaseTwo, bool invulnerable, bool bodyContact) // 실제 예고와 물리 충돌
        {
            boss.transform.position = player.transform.position + Vector3.up * 30f; // 이전 접촉을 분리할 보스 위치
            Physics2D.SyncTransforms(); // 분리 위치의 물리 반영
            yield return new WaitForFixedUpdate(); // 이전 접촉 종료 대기
            body.ResetBody(); // 실제 시험 몸통 초기화
            health.ResetHealth(); // 실제 공용 체력 초기화
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            if (invulnerable) // 실제 무적 대조 확인
            {
                GetComponent<SnakeInvulnerability>().StartInvulnerability(5f); // 실제 전체 무적 적용
            }
            if (health.ShieldController != null) // 실제 보호막 확인
            {
                health.ShieldController.Clear(); // 실제 보호막 간섭 제거
            }
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 위치 순회
            {
                body.BodySegments[index].transform.position = player.transform.position + Vector3.right * (20f + index * 4f); // 실제 머리 접촉만 허용
            }
            if (bodyContact) // 실제 몸통 선행 접촉 확인
            {
                body.BodySegments[0].transform.position = player.transform.position + Vector3.up * 1.5f; // 돌진 경로의 실제 첫 몸통
            }
            boss.transform.position = player.transform.position + Vector3.up * 3f; // 실제 돌진 시작 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            contactProbe.HeadEntries = 0; // 이전 머리 접촉 기록 초기화
            contactProbe.BodyEntries = 0; // 이전 몸통 접촉 기록 초기화
            contactProbe.BodyEnteredFirst = false; // 이전 접촉 순서 초기화
            int initialBodies = body.CurrentBodyCount; // 실제 초기 몸통 수
            nextCharge.SetValue(charge, Time.time); // 실제 다음 돌진 즉시 준비
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 시험 제한 시각
            while (!(bool)warningState.GetValue(charge) && Time.realtimeSinceStartup < deadline) // 실제 예고 시작 대기
            {
                yield return null; // 실제 프레임 진행
            }
            LineRenderer warning = boss.GetComponent<LineRenderer>(); // 실제 예고선 조회
            Require(warning != null && warning.sortingOrder > 30, "Boss charge warning can be obscured by mounted turrets."); // 포탑 위 예고 순서 확인
            Require(warning != null && warning.enabled && !(bool)chargeState.GetValue(charge), "Actual warning did not appear before charge."); // 실제 예고 우선 확인
            Require(health.CurrentHealth == health.MaximumHealth && body.CurrentBodyCount == initialBodies, "Warning alone caused damage or cutting."); // 예고 중 무피해 확인
            if (!phaseTwo) // 첫 단계 정지 시험 확인
            {
                Vector3 beforePause = boss.transform.position; // 실제 예고 중 위치 보존
                GameManager.Instance.PauseGame(); // 실제 게임 일시정지
                yield return new WaitForSecondsRealtime(0.25f); // 실제 정지 시간 대기
                Require((bool)warningState.GetValue(charge) && warning.enabled && (boss.transform.position - beforePause).sqrMagnitude < 0.0001f, "Actual paused warning advanced or moved."); // 실제 정지 중 예고 유지 확인
                GameManager.Instance.ResumeGame(); // 실제 게임 재개
            }
            while ((bool)warningState.GetValue(charge) && Time.realtimeSinceStartup < deadline) // 실제 예고 유지 대기
            {
                yield return null; // 실제 예고 프레임 진행
            }
            Require((bool)chargeState.GetValue(charge) && !warning.enabled, "Actual warning did not lead to charge."); // 실제 예고 후 돌진 확인
            while ((bool)chargeState.GetValue(charge) && Time.realtimeSinceStartup < deadline) // 실제 돌진 완료 대기
            {
                yield return null; // 실제 물리 프레임 진행
            }
            yield return null; // 실제 물리 우선순위 해결 대기
            Require(!(bool)chargeState.GetValue(charge), "Actual charge did not finish."); // 실제 돌진 종료 확인
            if (bodyContact) // 실제 몸통 선행 시험 확인
            {
                Debug.Log($"[BossChargeVerification] contact positions: boss={boss.transform.position}, head={player.transform.position}, bodies={body.CurrentBodyCount}, bodyEntries={contactProbe.BodyEntries}, headEntries={contactProbe.HeadEntries}."); // 실제 접촉 위치 진단
                Require(contactProbe.BodyEntries > 0 && contactProbe.HeadEntries > 0 && contactProbe.BodyEnteredFirst, "Actual sequential body and head contacts were not observed."); // 실제 접촉 순서 확인
            }
            if (phaseTwo && invulnerable) // 전체 무적 중 절단 확인
            {
                Require(body.CurrentBodyCount == initialBodies && health.CurrentHealth == health.MaximumHealth, "Invulnerable player lost a body or HP to boss charge."); // 실제 무적 보호 확인
            }
            else if (phaseTwo) // 두 번째 단계 확인
            {
                Require(body.CurrentBodyCount == initialBodies - 1 && health.CurrentHealth == health.MaximumHealth, "Phase-two actual contact did not cut exactly one body without HP damage."); // 실제 절단과 HP 보존 확인
            }
            else // 첫 번째 단계 확인
            {
                Require(body.CurrentBodyCount == initialBodies && health.CurrentHealth == health.MaximumHealth - 15, "Phase-one actual contact did not apply one 15 HP hit."); // 실제 공용 HP 피해 확인
            }
            Debug.Log($"[BossChargeVerification] actual phaseTwo={phaseTwo}, invulnerable={invulnerable}, bodyContact={bodyContact}, bodyEntries={contactProbe.BodyEntries}, headEntries={contactProbe.HeadEntries}, warning=0.35, speed=8, duration=1, HP={health.CurrentHealth}, bodies={body.CurrentBodyCount}."); // 실제 통제 조건과 결과 기록
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedBossChargeVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
