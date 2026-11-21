using System; // 실제 예외 형식
using System.Collections.Generic;
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
        private static bool diagnosed; // 시험 대기 상태 기록 여부

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
            if (!diagnosed && EditorApplication.timeSinceStartup - started > 5d) // 초기화 지연 진단 시점
            { // 조건 시작
                diagnosed = true; // 중복 진단 방지
                var selection = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.Progression.StartingWeaponSelection>(); // 시작 선택기 조회
                var frontend = ProjectEpsilon.UI.RunFrontendController.Instance; // 시작 화면 조회
                var panel = UnityEngine.Object.FindFirstObjectByType<ProjectEpsilon.UI.LevelUpPanelController>(FindObjectsInactive.Include); // 포탑 선택 화면 조회
                var field = typeof(ProjectEpsilon.UI.LevelUpPanelController).GetField("candidateButton01", BindingFlags.Instance | BindingFlags.NonPublic); // 첫 후보 버튼 필드 조회
                var button = field == null || panel == null ? null : field.GetValue(panel) as UnityEngine.UI.Button; // 실제 첫 후보 버튼 조회
                Debug.Log($"[EnemyWaveStartDiagnostic] state={GameManager.Instance?.CurrentState}, frontend={frontend?.Stage}, selecting={selection?.IsSelecting}, button={button != null}, active={button?.IsActive()}, interactable={button?.interactable}, injected={injected}, time={Time.time:F2}"); // 실제 시작 상태 기록
            } // 조건 종료
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
            float[] times = { 0f, 299.99f, 300f, 599.99f, 600f, 899.99f, 900f, 1080f, 1140f, 1200f }; // 실제 구간 경계 시각
            float[] intervals = { 1.3f, 0.94f, 0.85f, 0.57f, 0.5f, 0.356f, 0.32f, 0.24f, 0.24f, 0.24f }; // 실제 기대 생성 간격
            int[] limits = { 40, 88, 100, 196, 220, 332, 360, 500, 500, 500 }; // 실제 일반 적 최대 상한
            float[] health = { 1f, 1.233544f, 1.3f, 1.686579f, 1.8f, 2.341027f, 2.5f, 3.1f, 3.340659f, 3.6f }; // 실제 체력 배율
            float[] damage = { 1f, 1.047719f, 1.06f, 1.123531f, 1.14f, 1.219322f, 1.24f, 1.32f, 1.359412f, 1.4f }; // 실제 공격 배율
            EnemyWaveSettings wave = data as EnemyWaveSettings; // 실제 웨이브 설정 조회
            Require(wave != null, "Actual wave settings have an unexpected type."); // 실제 설정 형식 확인
            EnemyWavePacing? firstMinute = wave.SampleMinute(0); // 시작 분의 실제 설정
            Require(firstMinute.HasValue, "Starting wave pacing is missing."); // 시작 설정 존재 확인
            float previousHealth = firstMinute.Value.HealthMultiplier; // 이전 분 체력 배율
            float previousDamage = firstMinute.Value.DamageMultiplier; // 이전 분 공격력 배율
            float previousHealthGain = 0f; // 이전 분 체력 증가폭
            float previousDamageGain = 0f; // 이전 분 공격력 증가폭
            for (int minute = 1; minute <= 20; minute++) // 모든 경과 분 순회
            { // 반복 시작
                EnemyWavePacing? sample = wave.SampleMinute(minute); // 실제 분별 설정 조회
                Require(sample.HasValue, "Missing minute pacing: " + minute); // 분별 설정 존재 확인
                float healthGain = sample.Value.HealthMultiplier - previousHealth; // 현재 분 체력 증가폭
                float damageGain = sample.Value.DamageMultiplier - previousDamage; // 현재 분 공격력 증가폭
                Require(healthGain > 0f && damageGain > 0f && healthGain + 0.0001f >= previousHealthGain && damageGain + 0.0001f >= previousDamageGain, "Enemy growth did not accelerate at minute " + minute); // 매분 상승과 증가폭 확인
                previousHealth = sample.Value.HealthMultiplier; // 현재 체력 배율 저장
                previousDamage = sample.Value.DamageMultiplier; // 현재 공격력 배율 저장
                previousHealthGain = healthGain; // 현재 체력 증가폭 저장
                previousDamageGain = damageGain; // 현재 공격력 증가폭 저장
            } // 반복 종료
            Require(Mathf.CeilToInt(25f * wave.SampleMinute(19).Value.HealthMultiplier) == 84, "Minute 19 soldier health should be 84."); // 병정개미 기준 체력 확인
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
            elapsed.SetValue(spawner, 900f);
            WeaponTarget elite = spawn.Invoke(spawner, new object[] { enemies[3], new Vector3(150f, 100f, 0f), false }) as WeaponTarget; // 실제 엘리트 생성
            Require(elite != null && Mathf.Approximately(elite.MaximumHealth, Mathf.Ceil(enemies[3].health * 2.5f)), "Elite health did not follow elapsed time."); // 엘리트 체력 배율 확인
            elite.TakeDamage(elite.MaximumHealth * 0.5f); // 엘리트 체력 절반 감소
            elapsed.SetValue(spawner, 960f); // 다음 분 경계 이동
            MethodInfo refresh = typeof(EarlyEnemyWaveSpawner).GetMethod("RefreshEnemyScaling", Private); // 실제 생존 적 갱신 경로
            Require(refresh != null, "Retroactive minute scaling is missing."); // 분 단위 소급 갱신 존재 확인
            refresh.Invoke(spawner, null); // 실제 생존 적 능력치 갱신
            Require(Mathf.Approximately(elite.MaximumHealth, Mathf.Ceil(enemies[3].health * 2.685843f)) && Mathf.Approximately(elite.CurrentHealth, elite.MaximumHealth * 0.5f), "Retroactive elite scaling did not preserve health ratio."); // 기존 체력 비율과 증가 확인
            elapsed.SetValue(spawner, 0f); // 신규 적의 시작 시각 설정
            WeaponTarget normal = spawn.Invoke(spawner, new object[] { enemies[0], new Vector3(160f, 100f, 0f), false }) as WeaponTarget; // 시작 시각의 일반 적 생성
            normal.TakeDamage(normal.MaximumHealth * 0.4f); // 기존 일반 적의 체력 감소
            refresh.Invoke(spawner, null); // 시작 분 적용
            elapsed.SetValue(spawner, 60f); // 첫 분 경계 이동
            refresh.Invoke(spawner, null); // 생존 적 소급 갱신
            float minuteHealth = Mathf.Ceil(enemies[0].health * 1.053874f); // 첫 분의 일반 적 최대 체력
            int minuteDamage = Mathf.CeilToInt(enemies[0].damage * 1.011722f); // 첫 분의 일반 적 공격력
            EarlyEnemySettings normalRuntime = typeof(EarlyEnemyBehaviour).GetField("settings", Private).GetValue(normal.GetComponent<EarlyEnemyBehaviour>()) as EarlyEnemySettings; // 생존 적의 실제 공격 설정
            Require(Mathf.Approximately(normal.MaximumHealth, minuteHealth) && Mathf.Approximately(normal.CurrentHealth, minuteHealth * 0.6f), "Retroactive normal scaling lost its health ratio."); // 일반 적 체력 비율 확인
            Require(normalRuntime != null && normalRuntime.damage == minuteDamage && Mathf.Approximately(normalRuntime.speedRatio, enemies[0].speedRatio), "Retroactive damage or movement speed mismatch."); // 기존 적 공격력과 속도 확인
            WeaponTarget newNormal = spawn.Invoke(spawner, new object[] { enemies[0], new Vector3(165f, 100f, 0f), false }) as WeaponTarget; // 같은 분의 신규 일반 적 생성
            Require(Mathf.Approximately(newNormal.MaximumHealth, normal.MaximumHealth), "New and existing normal enemies use different scaling."); // 기존 적과 신규 적 배율 일치 확인
            normal.TakeDamage(normal.MaximumHealth * 10f); // 기존 일반 적 처치
            elapsed.SetValue(spawner, 120f); // 다음 분 경계 이동
            refresh.Invoke(spawner, null); // 사망 이후 재갱신
            Require(!normal.IsAlive && Mathf.Approximately(enemies[0].health, 25f) && enemies[0].damage == 10, "Scaling revived a dead enemy or changed base data."); // 사망 상태와 원본 보존 확인
            Debug.Log("[EnemyWaveCheckpoint] 분별 성장과 체력 비율 확인 완료"); // 성장 검증 도달 기록
            yield return null; // 실제 제거 프레임 대기
            elapsed.SetValue(spawner, 1080f);
            Debug.Log("[EnemyWaveCheckpoint] 500마리 생성 시작"); // 대량 생성 시작 기록
            VerifySharedCapacity(spawner);
            Debug.Log($"[EnemyWaveCheckpoint] 500마리와 정예 대기 확인 완료: {spawner.ActiveEnemies}"); // 상한 검증 완료 기록
            SnakeInvulnerability protection = GetComponent<SnakeInvulnerability>(); // 스트레스 시험용 무적 조회
            if (protection != null) // 무적 구성 확인
            { // 조건 시작
                protection.StartInvulnerability(120f, false); // 측정 중 체력 손실 방지
            } // 조건 종료
            const int sampleCount = 30; // 실제 프레임 표본 수
            double[] frameMilliseconds = new double[sampleCount]; // 실제 프레임 간격 보관
            int minimumEnemies = spawner.ActiveEnemies; // 측정 중 최소 적 수
            int maximumEnemies = spawner.ActiveEnemies; // 측정 중 최대 적 수
            double previous = Time.realtimeSinceStartupAsDouble; // 첫 실시간 기준
            for (int frame = 0; frame < sampleCount; frame++) // 실제 프레임 측정 반복
            { // 반복 시작
                yield return null; // 실제 적 행동 프레임 진행
                double current = Time.realtimeSinceStartupAsDouble; // 현재 실시간 조회
                frameMilliseconds[frame] = (current - previous) * 1000d; // 프레임 간격 밀리초 기록
                previous = current; // 다음 프레임 기준 갱신
                minimumEnemies = Mathf.Min(minimumEnemies, spawner.ActiveEnemies); // 실제 최소 적 수 갱신
                maximumEnemies = Mathf.Max(maximumEnemies, spawner.ActiveEnemies); // 실제 최대 적 수 갱신
                if ((frame + 1) % 15 == 0) // 측정 진행 상태 확인
                { // 조건 시작
                    Debug.Log($"[EnemyWaveCheckpoint] 측정 프레임 {frame + 1}/{sampleCount}, 상태={GameManager.Instance?.CurrentState}"); // 장시간 측정 진행 기록
                } // 조건 종료
            } // 반복 종료
            Array.Sort(frameMilliseconds); // 프레임 간격 백분위 정렬
            Require(maximumEnemies <= 500, "The actual stress scenario exceeded 500 enemies."); // 측정 중 전체 상한 확인
            double median = (frameMilliseconds[(sampleCount - 1) / 2] + frameMilliseconds[sampleCount / 2]) / 2d; // 짝수 표본 중앙값
            double percentile95 = frameMilliseconds[Mathf.CeilToInt(sampleCount * 0.95f) - 1]; // 최근접 순위 95백분위
            Debug.Log($"[Enemy500FrameSample] frames={sampleCount}, enemies={minimumEnemies}-{maximumEnemies}, p50={median:F2}ms, p95={percentile95:F2}ms, graphics=headless, weapons=disabled, player=stationary"); // 비대화형 프레임 측정 결과
            AutomatedEnemyWaveStagesVerification.Finish(0, "Actual adjustable wave boundaries, spawn settings, minute scaling and retroactive elite health passed."); // 실제 전체 성공 기록
        }

        private static void VerifySharedCapacity(EarlyEnemyWaveSpawner spawner)
        {
            // 이전 시험에서 지연 파괴한 오브젝트를 실제 출현기 집계에서 정리한다.
            typeof(EarlyEnemyWaveSpawner).GetMethod("CleanupDistantEnemies", Private).Invoke(spawner, null);
            Vector3 position = spawner.transform.position + Vector3.right * 10f;
            int remaining = 499 - spawner.ActiveEnemies;
            for (int index = 0; index < remaining; index++)
            {
                float angle = index * Mathf.PI * 2f / Mathf.Max(1, remaining); // 실제 적 분산 각도
                Vector3 spread = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 10f; // 플레이어 주변 분산 위치
                Require(spawner.SpawnMapChaser(spawner.transform.position + spread) != null, "Map wave could not reach the late-game limit."); // 분산된 실제 적 생성 확인
            }
            WeaponTarget last = spawner.SpawnEliteSummon(position);
            Require(last != null && spawner.ActiveEnemies == 500, "The last shared slot was unavailable.");
            Require(spawner.SpawnMapChaser(position) == null && spawner.SpawnEliteSummon(position) == null, "A map or elite summon exceeded 500.");

            MethodInfo schedule = typeof(EarlyEnemyWaveSpawner).GetMethod("ProcessEliteSchedule", Private);
            var pending = (Queue<int>)typeof(EarlyEnemyWaveSpawner).GetField("pendingEliteRounds", Private).GetValue(spawner);
            schedule.Invoke(spawner, null);
            int queuedAtCapacity = pending.Count;
            schedule.Invoke(spawner, null);
            Require(queuedAtCapacity > 0 && pending.Count == queuedAtCapacity && spawner.ActiveEnemies == 500, "A full field consumed a scheduled elite.");
            spawner.RemoveBossSummon(last);
            schedule.Invoke(spawner, null);
            Require(spawner.ActiveEnemies == 500 && pending.Count == queuedAtCapacity - 1 && spawner.SpawnMapChaser(position) == null, "A deferred elite failed to use the freed slot.");
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
