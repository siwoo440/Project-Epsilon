using System; // 실제 예외 형식
using System.Collections; // 실제 시험 코루틴
using System.Reflection; // 실제 출현기 내부 조회
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적과 출현기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 실제 Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEnemyWaveStagesVerification // 구간별 실제 적 출현 검증
    {
        private const string ActiveKey = "EpsilonEnemyWaveStagesVerification"; // 실제 시험 진행 표식
        private static bool injected; // 실제 실행기 생성 여부
        private static double started; // 실제 시험 시작 시각

        static AutomatedEnemyWaveStagesVerification() // 실제 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 진행 상태 확인
            {
                EditorApplication.update += Tick; // 실제 플레이 감시 연결
            }
        }

        public static void Run() // 실제 Unity 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }

        private static void Tick() // 실제 게임 초기화 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 플레이 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 실제 시험 시간 제한
            {
                Finish(1, "Timed out."); // 시간 초과 기록
                return; // 추가 처리 차단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 선택과 실행 여부 확인
            {
                return; // 실제 전투와 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 초기화 확인
            {
                return; // 실제 씬 구성 대기
            }
            injected = true; // 중복 실행 차단
            player.gameObject.AddComponent<EnemyWaveStagesVerificationRunner>(); // 실제 구간 시험 실행
        }

        public static void Finish(int code, string message) // 실제 시험 결과와 종료
        {
            SessionState.SetBool(ActiveKey, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 연결 해제
            Debug.Log($"[EnemyWaveStagesVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드 반환
        }
    }

    public sealed class EnemyWaveStagesVerificationRunner : MonoBehaviour // 실제 출현 구간 시험 실행기
    {
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 필드 범위

        private IEnumerator Start() // 실제 출현 데이터와 경계 검증
        {
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            ScriptableObject data = Resources.Load<ScriptableObject>("EnemyWaveSettings"); // 실제 구간별 데이터 에셋
            Require(spawner != null && data != null, "Adjustable enemy wave settings are missing."); // 실제 출현기와 데이터 확인
            spawner.enabled = false; // 자연 적 출현 중지
            GetComponent<SnakeMovement>().enabled = false; // 실제 머리 이동 중지
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            if (weapons != null) // 실제 포탑 관리자 확인
            {
                weapons.enabled = false; // 시험 적 자동 처치 차단
            }
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 출현 경과 시간 필드
            FieldInfo enemyField = typeof(EarlyEnemyWaveSpawner).GetField("enemies", Private); // 실제 원본 적 설정
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", Private); // 실제 적 생성 경로
            Require(elapsed != null && enemyField != null && spawn != null, "Actual wave fixtures missing."); // 실제 시험 구성 확인
            EarlyEnemySettings[] enemies = enemyField.GetValue(spawner) as EarlyEnemySettings[]; // 실제 원본 적 목록
            Require(enemies != null && enemies.Length >= 4 && enemies[0] != null && enemies[3] != null, "Actual base enemies missing."); // 실제 일반 적과 엘리트 확인
            float[] times = { 0f, 299.99f, 300f, 599.99f, 600f, 899.99f, 900f }; // 실제 구간 경계 시각
            float[] intervals = { 1.3f, 1.3f, 1.1f, 1.1f, 0.9f, 0.9f, 0.7f }; // 실제 기대 생성 간격
            int[] limits = { 40, 40, 70, 70, 110, 110, 125 }; // 실제 일반 적 최대 상한
            float[] health = { 1f, 1f, 1.12f, 1.12f, 1.25f, 1.25f, 1.4f }; // 실제 체력 배율
            float[] damage = { 1f, 1f, 1.03f, 1.03f, 1.06f, 1.06f, 1.09f }; // 실제 공격 배율
            for (int index = 0; index < times.Length; index++) // 실제 구간 경계 순회
            {
                elapsed.SetValue(spawner, times[index]); // 실제 경계 시각 통제
                Require(Mathf.Abs(ReadFloat(spawner, "CurrentSpawnInterval") - intervals[index]) < 0.001f && ReadInt(spawner, "CurrentNormalEnemyLimit") == limits[index], "Spawn interval or cap mismatch at " + times[index]); // 실제 구간별 간격과 상한 확인
                Require(Mathf.Abs(ReadFloat(spawner, "CurrentNormalHealthMultiplier") - health[index]) < 0.001f && Mathf.Abs(ReadFloat(spawner, "CurrentNormalDamageMultiplier") - damage[index]) < 0.001f, "Enemy multipliers mismatch at " + times[index]); // 실제 구간별 배율 확인
                WeaponTarget target = spawn.Invoke(spawner, new object[] { enemies[0], new Vector3(100f + index * 3f, 100f, 0f), false }) as WeaponTarget; // 실제 일반 적 생성
                Require(target != null && Mathf.Approximately(target.MaximumHealth, Mathf.Ceil(enemies[0].health * health[index])), "Actual spawned enemy health mismatch at " + times[index]); // 실제 체력 배율과 올림 확인
                FieldInfo behaviourSettings = typeof(EarlyEnemyBehaviour).GetField("settings", Private); // 실제 생성 적 행동 설정
                EarlyEnemySettings actual = behaviourSettings == null ? null : behaviourSettings.GetValue(target.GetComponent<EarlyEnemyBehaviour>()) as EarlyEnemySettings; // 실제 생성 적 능력치
                Require(actual != null && actual.damage == Mathf.CeilToInt(enemies[0].damage * damage[index]), "Actual spawned enemy damage mismatch at " + times[index]); // 실제 공격 배율과 올림 확인
                target.gameObject.SetActive(false); // 시험 적 후속 충돌 차단
                Destroy(target.gameObject); // 실제 시험 적 제거
            }
            Require(Mathf.Approximately(enemies[0].health, 25f) && enemies[0].damage == 10, "Wave scaling mutated the shared base enemy settings."); // 원본 능력치 보존 확인
            WeaponTarget elite = spawn.Invoke(spawner, new object[] { enemies[3], new Vector3(150f, 100f, 0f), false }) as WeaponTarget; // 실제 엘리트 생성
            Require(elite != null && Mathf.Approximately(elite.MaximumHealth, enemies[3].health), "Normal wave scaling changed elite health."); // 일반 적 배율의 엘리트 영향 차단
            yield return null; // 실제 제거 프레임 대기
            AutomatedEnemyWaveStagesVerification.Finish(0, "Actual adjustable wave boundaries, spawn settings, scaled normal enemies and unchanged elite passed."); // 실제 전체 성공 기록
        }

        private static float ReadFloat(object target, string name) // 실제 출현 구간 실수 값 조회
        {
            PropertyInfo property = target.GetType().GetProperty(name); // 실제 공개 속성 조회
            Require(property != null, "Missing wave property " + name); // 실제 속성 존재 확인
            return (float)property.GetValue(target); // 실제 실수 값 반환
        }

        private static int ReadInt(object target, string name) // 실제 출현 구간 정수 값 조회
        {
            PropertyInfo property = target.GetType().GetProperty(name); // 실제 공개 속성 조회
            Require(property != null, "Missing wave property " + name); // 실제 속성 존재 확인
            return (int)property.GetValue(target); // 실제 정수 값 반환
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedEnemyWaveStagesVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 추가 처리 중단
            }
        }
    }
}
