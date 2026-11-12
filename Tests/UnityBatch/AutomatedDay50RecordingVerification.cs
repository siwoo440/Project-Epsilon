using System; // 실제 시간과 예외
using System.Collections; // 실제 프레임 대기
using System.IO; // 실제 기록 파일 읽기
using System.Reflection; // 실제 적과 물리 처리 조회
using ProjectEpsilon.Combat; // 실제 포탑과 적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 체력과 충돌
using ProjectEpsilon.Progression; // 실제 경험치와 성장
using ProjectEpsilon.Recording; // 실제 전투 기록
using UnityEditor; // 비대화형 시험 제어
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 실제 Unity 객체

namespace ProjectEpsilon.Editor // 시험 기능 영역
{
    [InitializeOnLoad] // 도메인 재시작 감시
    public static class AutomatedDay50RecordingVerification // 전투 기록 실제 씬 시험
    {
        private const string Key = "EpsilonDay50Recording"; // 실제 시험 상태 키
        private static double started; // 실제 제한 시간 기준
        private static bool injected; // 실제 시험 실행기 여부
        static AutomatedDay50RecordingVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(Key, false)) // 진행 중인 시험 확인
            {
                EditorApplication.update += Tick; // 실제 시험 감시 등록
            }
        }
        public static void Run() // 비대화형 시험 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 실제 시험 감시 등록
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 시작 상태 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 플레이 진입 대기
            }
            if (started <= 0d) // 제한 시간 시작 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 시험 제한 시간 확인
            {
                Finish(1, "Timed out."); // 실제 시간 초과 표시
                return; // 이후 처리 중지
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 메뉴와 시작 포탑 진행
            {
                return; // 실제 전투 시작 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying || player == null || RunTelemetry.Instance == null) // 실제 전투 구성 확인
            {
                return; // 실제 구성 대기
            }
            injected = true; // 실제 중복 실행 방지
            player.gameObject.AddComponent<Day50RecordingRunner>(); // 실제 기록 검증 시작
        }
        public static void Finish(int code, string message) // 실제 시험 결과 처리
        {
            SessionState.SetBool(Key, false); // 실제 시험 상태 해제
            EditorApplication.update -= Tick; // 실제 감시 해제
            Debug.Log($"[Day50RecordingVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day50RecordingRunner : MonoBehaviour // 실제 전투 기록 시나리오
    {
        private IEnumerator Start() // 실제 통제 전투 실행
        {
            Require(Application.productName == "Project-Epsilon-Day48-Isolated", "Batch project is not isolated."); // 실제 사용자 데이터 격리 확인
            RunTelemetry telemetry = RunTelemetry.Instance; // 실제 전투 기록기
            RunRecording recording = telemetry.Current; // 실제 현재 런 기록
            Require(recording != null && recording.seedProvided && recording.seed == 13579 && recording.snapshots.Count > 0, "Run seed or initial sample missing."); // 실제 기록 시작 확인
            UnityEngine.Random.State savedRandom = UnityEngine.Random.state; // 실제 게임 난수 상태 보존
            UnityEngine.Random.InitState(13579); // 실제 동일 시드 구성
            float expectedProbe = UnityEngine.Random.value; // 실제 첫 난수 표본
            UnityEngine.Random.state = savedRandom; // 실제 게임 난수 상태 복원
            Require(Mathf.Approximately(recording.randomProbe, expectedProbe) && Mathf.Approximately(recording.fixedDeltaTime, Time.fixedDeltaTime), "Seed or physics configuration mismatch."); // 실제 재현 조건 확인
            Require(recording.mapIndex == 0 && recording.characterId == "balanced" && recording.enemies.Count == 4 && recording.bossTransitionTime == 1200f, "Actual initial configuration mismatch."); // 실제 기존 게임 설정 확인
            RunSnapshot initial = recording.snapshots[0]; // 실제 첫 상태 표본
            Require(initial.reason == "start" && initial.currentBodies == 3 && initial.earnedBodies == 3 && initial.bodyLimit == 20 && initial.health == 100 && initial.weapons.Count == 3 && !string.IsNullOrEmpty(initial.weapons[0].weaponId), "Initial body, health or turret snapshot incorrect."); // 실제 시작 상태 확인
            EarlyEnemyWaveSpawner wave = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 보상 처치 집계
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeInvulnerability invulnerability = GetComponent<SnakeInvulnerability>(); // 실제 무적 판정
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌
            SnakeExternalCollision collision = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌
            Require(wave != null && meta != null && experience != null && weapons != null && body != null && health != null && invulnerability != null && self != null && collision != null, "Actual combat fixtures missing."); // 실제 씬 구성 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 이동 통제
            SnakeBodyFollower follower = FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            {
                follower.enabled = false; // 실제 몸통 이동 통제
            }
            wave.enabled = false; // 실제 자연 출현 통제
            weapons.enabled = false; // 실제 자동 공격 통제
            experience.Configure(10000, 1f); // 실제 시험 경험치 요구량
            experience.ResetProgression(); // 실제 경험치 초기화
            experience.AddExperience(42); // 실제 경험치 획득
            Require(weapons.TryEquipAt(1, weapons.Slots[0].Weapon), "Could not change actual loadout."); // 실제 포탑 상태 변경
            telemetry.Capture("controlled"); // 실제 통제 상태 표본
            RunSnapshot changed = recording.snapshots[recording.snapshots.Count - 1]; // 실제 최신 상태
            Require(changed.totalExperience == 42 && changed.currentExperience == 42 && changed.weapons.Count == 3 && !string.IsNullOrEmpty(changed.weapons[1].weaponId), "Actual XP or loadout snapshot mismatch."); // 실제 누적 XP와 장착 확인
            MethodInfo spawn = typeof(EarlyEnemyWaveSpawner).GetMethod("SpawnAt", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 적 생성 경로
            EarlyEnemySettings[] settings = (EarlyEnemySettings[])typeof(EarlyEnemyWaveSpawner).GetField("enemies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wave); // 실제 적 설정
            WeaponTarget normal = (WeaponTarget)spawn.Invoke(wave, new object[] { settings[0], new Vector3(500f, 500f, 0f), false }); // 실제 보상 대상 적
            WeaponTarget summoned = (WeaponTarget)spawn.Invoke(wave, new object[] { settings[0], new Vector3(520f, 500f, 0f), true }); // 실제 무보상 소환 적
            normal.TakeDamage(10000f); // 실제 일반 적 처치
            summoned.TakeDamage(10000f); // 실제 소환 적 처치
            telemetry.Capture("kills"); // 실제 처치 상태 표본
            RunSnapshot kills = recording.snapshots[recording.snapshots.Count - 1]; // 실제 처치 표본
            Require(kills.kills == 2 && kills.rewardableKills == 1 && kills.summonedKills == 1 && meta.Kills == 1, "Actual total and rewardable kills differ."); // 실제 소환 적 집계 분리 확인
            invulnerability.StartInvulnerability(1f); // 실제 완전 무적 적용
            Require(!health.TakeDamage(10, DamageOrigin.EnemyMelee) && recording.damages.Count == 0, "Invulnerable hit was recorded as damage."); // 실제 무적 피해 제외 확인
            invulnerability.ClearInvulnerability(); // 실제 무적 해제
            Require(health.ShieldController != null && health.ShieldController.Apply(5, 10f, 2, 1f), "Shield setup failed."); // 실제 보호막 구성
            Require(health.TakeDamage(4, DamageOrigin.EnemyProjectile) && recording.damages.Count == 1 && recording.damages[0].shieldDamage == 4 && recording.damages[0].healthDamage == 0, "Shield-only damage recording failed."); // 실제 보호막 흡수 기록 확인
            Require(health.TakeDamage(10, DamageOrigin.EnemyMelee) && recording.damages.Count == 2 && recording.damages[1].shieldDamage == 1 && recording.damages[1].healthDamage == 9, "Partial shield or health damage recording failed."); // 실제 부분 보호막 피해 확인
            collision.QueueHealthDamage(health, 20, DamageOrigin.BossHazard); // 실제 물리 우선순위 피해 예약
            typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(collision, null); // 실제 예약 피해 처리
            Require(recording.damages.Count == 3 && recording.damages[2].origin == "BossHazard" && recording.damages[2].healthDamage == 20, "Deferred damage origin was lost."); // 실제 예약 피해 원인 확인
            Require(self.TryApplySelfCollisionPenalty() && body.CurrentBodyCount == 1 && recording.damages.Count == 4 && recording.damages[3].origin == "SelfCollision" && recording.damages[3].bodyLoss == 2, "Actual self collision recording failed."); // 실제 자기 충돌 손실 확인
            invulnerability.ClearInvulnerability(); // 실제 마지막 절단 무적 해제
            bool cutApplied = false; // 실제 절단 결과
            collision.QueueEliteCut(applied => cutApplied = applied, DamageOrigin.BossCut); // 실제 보스 절단 예약
            typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(collision, null); // 실제 절단 우선순위 처리
            Require(cutApplied && GameManager.Instance.CurrentState == GameState.Defeated && !string.IsNullOrEmpty(telemetry.LastSavedPath) && File.Exists(telemetry.LastSavedPath), "Defeat did not save an actual record."); // 실제 패배와 파일 저장 확인
            RunRecording saved = JsonUtility.FromJson<RunRecording>(File.ReadAllText(telemetry.LastSavedPath)); // 실제 저장 파일 복원
            Require(saved != null && saved.outcome == "Defeated" && saved.seed == 13579 && saved.lastDamageOrigin == "BossCut" && saved.damages.Count == 5 && saved.snapshots[saved.snapshots.Count - 1].currentBodies == 0 && saved.snapshots[saved.snapshots.Count - 1].totalExperience == 42, "Saved final state or lethal cause mismatch."); // 실제 최종 상태와 치명 원인 확인
            Require(saved.snapshots[saved.snapshots.Count - 1].weapons.Count == 0, "Final weapon slots outlive the remaining bodies."); // 실제 남은 몸통과 포탑 슬롯 일치 확인
            Require(Path.GetFullPath(telemetry.LastSavedPath).StartsWith(Path.GetFullPath(Application.persistentDataPath), StringComparison.OrdinalIgnoreCase), "Record escaped the isolated profile."); // 실제 격리 저장 위치 확인
            Debug.Log("[Day50RecordingVerification] FILE " + telemetry.LastSavedPath); // 실제 기록 파일 경로
            AutomatedDay50RecordingVerification.Finish(0, "Actual seed, configuration, XP, kills, shield, deferred damage, body loss and JSON result passed."); // 실제 전체 시험 성공
            yield return null; // 실제 종료 프레임 대기
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 조건 실패 확인
            {
                AutomatedDay50RecordingVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 실제 후속 시험 중단
            }
        }
    }
}
