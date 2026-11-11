using System.Reflection; // 실제 적 생성 설정 조회
using System; // 예외 형식
using System.Collections; // 코루틴 형식
using ProjectEpsilon.Combat; // 실제 속성 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEnemyHalfSpeedVerification // 실제 적 이동 속도 시험
    {
        private const string Key = "EpsilonEnemyHalfSpeedVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedEnemyHalfSpeedVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<EnemyHalfSpeedRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[EnemyHalfSpeedVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class EnemyHalfSpeedRunner : MonoBehaviour // 실제 적 속도 시험 실행기
    {
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 경로 범위
        private IEnumerator Start() // 실제 일반 적과 보스 속도 시험
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동 조회
            player.enabled = false; // 실제 플레이어 이동 중지
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            spawner.enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 실제 자동 공격 중지
            foreach (WeaponTarget existing in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 전투 대상 순회
            {
                existing.gameObject.SetActive(false); // 시험 외 대상 비활성화
            }
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private).GetValue(spawner); // 실제 적 설정 조회
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 경로
            WeaponTarget[] targets = new WeaponTarget[4]; // 실제 네 적 대상
            for (int index = 0; index < targets.Length; index++) // 실제 적 종류 순회
            {
                targets[index] = (WeaponTarget)spawn.Invoke(spawner, new object[] { settings[index], new Vector3(20f, index * 10f, 0f), false }); // 실제 일반 적 또는 엘리트 생성
            }
            yield return new WaitForFixedUpdate(); // 실제 초기 물리 이동 대기
            Vector3[] positions = new Vector3[targets.Length]; // 적별 초기 위치
            for (int index = 0; index < targets.Length; index++) // 실제 적 위치 순회
            {
                positions[index] = targets[index].transform.position; // 실제 초기 위치 저장
            }
            float start = Time.fixedTime; // 실제 물리 시작 시각
            for (int index = 0; index < 10; index++) // 실제 물리 프레임 순회
            {
                yield return new WaitForFixedUpdate(); // 실제 물리 이동 대기
            }
            float elapsed = Time.fixedTime - start; // 실제 물리 경과 시간
            for (int index = 0; index < targets.Length; index++) // 실제 적 속도 순회
            {
                float speed = Vector3.Distance(positions[index], targets[index].transform.position) / elapsed; // 실제 측정 이동 속도
                float expected = player.BaseMoveSpeed * settings[index].speedRatio * 0.5f; // 기획 데이터 대비 절반 속도
                Require(Mathf.Abs(speed - expected) < 0.16f, "Enemy speed mismatch for " + settings[index].kind + ": " + speed); // 일반 적과 엘리트 실제 속도 확인
            }
            EarlyEnemyBehaviour elite = targets[3].GetComponent<EarlyEnemyBehaviour>(); // 실제 엘리트 행동
            typeof(EarlyEnemyBehaviour).GetField("chargePhase", Private).SetValue(elite, 2); // 실제 돌진 단계 설정
            typeof(EarlyEnemyBehaviour).GetField("chargePhaseEnd", Private).SetValue(elite, Time.time + 2f); // 실제 돌진 종료 시각 설정
            typeof(EarlyEnemyBehaviour).GetField("chargeDirection", Private).SetValue(elite, Vector2.left); // 실제 돌진 방향 설정
            float multiplier = (float)typeof(EarlyEnemyBehaviour).GetField("eliteChargeMultiplier", Private).GetValue(elite); // 기존 돌진 속도 배율
            yield return new WaitForFixedUpdate(); // 실제 돌진 시작 대기
            Vector3 eliteStart = elite.transform.position; // 실제 돌진 시작 위치
            start = Time.fixedTime; // 실제 물리 시작 시각
            for (int index = 0; index < 8; index++) // 실제 돌진 프레임 순회
            {
                yield return new WaitForFixedUpdate(); // 실제 돌진 이동 대기
            }
            float eliteSpeed = Vector3.Distance(eliteStart, elite.transform.position) / (Time.fixedTime - start); // 실제 엘리트 돌진 속도
            Require(Mathf.Abs(eliteSpeed - player.BaseMoveSpeed * settings[3].speedRatio * multiplier * 0.5f) < 0.2f, "Elite charge speed mismatch: " + eliteSpeed); // 실제 엘리트 돌진 절반 속도 확인
            GameObject generic = new GameObject("HalfSpeedGeneric"); // 기존 데이터 기반 적 객체
            generic.transform.position = new Vector3(20f, -40f, 0f); // 다른 대상과 떨어진 위치
            generic.AddComponent<WeaponTarget>().Configure(100f); // 실제 공격 대상 생성
            generic.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic; // 실제 물리체 생성
            EnemyMovementController legacy = generic.AddComponent<EnemyMovementController>(); // 기존 데이터 기반 이동기 생성
            legacy.Configure(null, player.transform, null, generic.GetComponent<Rigidbody2D>()); // 기존 기본 이동 설정
            Require(Mathf.Abs(legacy.CurrentMoveSpeed - legacy.BaseMoveSpeed * 0.5f) < 0.001f, "Data-driven enemy speed mismatch."); // 기존 적 이동 절반 확인
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", Private).Invoke(encounter, null); // 실제 보스 강제 생성
            WeaponTarget boss = encounter.Boss; // 실제 보스 대상
            Require(boss != null, "Boss did not spawn."); // 실제 보스 생성 확인
            boss.transform.position = new Vector3(100f, 0f, 0f); // 다른 적과 떨어진 보스 위치
            PrototypeBossChargeAttack charge = boss.GetComponent<PrototypeBossChargeAttack>(); // 실제 보스 돌진기
            typeof(PrototypeBossChargeAttack).GetMethod("BeginWarning", Private).Invoke(charge, null); // 실제 보스 예고 시작
            LineRenderer warning = boss.GetComponent<LineRenderer>(); // 실제 보스 예고선
            float chargeSpeed = (float)typeof(PrototypeBossChargeAttack).GetField("chargeSpeed", Private).GetValue(charge); // 기존 보스 돌진 속도
            float duration = (float)typeof(PrototypeBossChargeAttack).GetField("chargeDuration", Private).GetValue(charge); // 실제 보스 돌진 시간
            Require(warning != null && Mathf.Abs(Vector3.Distance(warning.GetPosition(0), warning.GetPosition(1)) - chargeSpeed * duration * 0.5f) < 0.01f, "Boss warning distance mismatch."); // 보스 절반 속도 예고 거리 확인
            typeof(PrototypeBossChargeAttack).GetField("phaseEndsAt", Private).SetValue(charge, Time.time - 1f); // 실제 예고 종료 시각
            while (!charge.IsCharging) // 실제 보스 돌진 시작 확인
            {
                yield return new WaitForFixedUpdate(); // 실제 보스 돌진 시작 대기
            }
            yield return new WaitForFixedUpdate(); // 실제 첫 돌진 물리 대기
            Vector3 bossStart = boss.transform.position; // 실제 보스 돌진 시작 위치
            start = Time.fixedTime; // 실제 물리 시작 시각
            for (int index = 0; index < 8; index++) // 실제 보스 돌진 프레임 순회
            {
                yield return new WaitForFixedUpdate(); // 실제 보스 이동 대기
            }
            float bossSpeed = Vector3.Distance(bossStart, boss.transform.position) / (Time.fixedTime - start); // 실제 보스 돌진 속도
            Require(Mathf.Abs(bossSpeed - chargeSpeed * 0.5f) < 0.2f, "Boss charge speed mismatch: " + bossSpeed); // 실제 보스 돌진 절반 속도 확인
            AutomatedEnemyHalfSpeedVerification.Finish(0, "Actual enemies, elite charge, data-driven movement and boss warning/charge use half speed."); // 전체 성공 기록
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedEnemyHalfSpeedVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
