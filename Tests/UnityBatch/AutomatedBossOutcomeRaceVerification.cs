using System; // 실제 예외와 콜백
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 충돌 예약 처리 조회
using ProjectEpsilon.Combat; // 실제 보스 공격 대상
using ProjectEpsilon.Core; // 실제 승패 상태
using ProjectEpsilon.Enemies; // 실제 보스와 출현기
using ProjectEpsilon.Player; // 실제 몸통과 충돌
using UnityEditor; // Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 플레이 진입 후 관찰 복구
    public static class AutomatedBossOutcomeRaceVerification // 보스 처치와 플레이어 피해 경합 검증
    {
        private const string ActiveKey = "EpsilonBossOutcomeRaceVerification"; // 시험 진행 표식
        private const string ModeKey = "EpsilonBossOutcomeRaceMode"; // 실제 종료 순서 구분
        private static bool injected; // 실제 실행기 생성 여부
        private static double started; // 실제 시작 시각

        static AutomatedBossOutcomeRaceVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 상태 확인
            {
                EditorApplication.update += Observe; // 실제 전투 시작 관찰
            }
        }

        public static void RunVictory() // 보스 처치 우선 경계 시험
        {
            Start(false); // 실제 승리 우선 조건 실행
        }

        public static void RunDefeat() // 플레이어 패배 우선 경계 시험
        {
            Start(true); // 실제 패배 우선 조건 실행
        }

        private static void Start(bool defeatFirst) // 실제 게임 씬과 조건 준비
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 기록
            SessionState.SetBool(ModeKey, defeatFirst); // 실제 종료 순서 기록
            EditorApplication.update -= Observe; // 중복 관찰 해제
            EditorApplication.update += Observe; // 실제 플레이 시작 관찰
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }

        private static void Observe() // 실제 시작 선택과 실행기 준비
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 실제 플레이 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 제한 확인
            {
                Finish(false, "Timed out before outcome race."); // 시간 초과 기록
                return; // 추가 처리 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 선택과 실행 여부 확인
            {
                return; // 실제 전투와 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 초기화 확인
            {
                return; // 실제 게임 시작 대기
            }
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<BossOutcomeRaceVerificationRunner>(); // 실제 충돌 경합 시험 시작
        }

        public static bool DefeatFirst => SessionState.GetBool(ModeKey, false); // 실제 종료 순서 조회

        public static void Finish(bool passed, string message) // 실제 결과 기록과 종료
        {
            SessionState.SetBool(ActiveKey, false); // 시험 진행 표식 제거
            EditorApplication.update -= Observe; // 실제 관찰 연결 해제
            Debug.Log("[BossOutcomeRaceVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과
            EditorApplication.Exit(passed ? 0 : 1); // 실제 종료 코드 반환
        }
    }

    public sealed class BossOutcomeRaceVerificationRunner : MonoBehaviour // 실제 보스와 충돌 경합 실행기
    {
        private IEnumerator Start() // 실제 보스 전환과 종료 경계 검증
        {
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeExternalCollision resolver = GetComponent<SnakeExternalCollision>(); // 실제 물리 충돌 예약기
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 출현기
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 전환 시간
            MethodInfo resolve = typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 물리 직후 충돌 처리
            Require(game != null && player != null && health != null && resolver != null && body != null && weapons != null && spawner != null && encounter != null && elapsed != null && resolve != null, "Actual game fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 시험 중 실제 플레이어 이동 정지
            weapons.enabled = false; // 시험 중 자동 공격 정지
            elapsed.SetValue(spawner, 1199.95f); // 실제 보스 전환 직전 시간 통제
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 전환 대기 제한
            while ((!game.IsFinalBossPhase || encounter.Boss == null) && Time.realtimeSinceStartup < deadline) // 실제 보스 등록 대기
            {
                yield return null; // 실제 게임 프레임 진행
            }
            WeaponTarget boss = encounter.Boss; // 실제 등록된 최종 보스
            Require(boss != null && boss.IsAlive && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth, "Actual controlled boss state missing."); // 실제 시험 시작 상태 확인
            if (AutomatedBossOutcomeRaceVerification.DefeatFirst) // 실제 패배 우선 조건 확인
            {
                Require(health.TakeDamage(10000), "Actual lethal player damage failed."); // 실제 공용 체력 소진
                Require(game.CurrentState == GameState.Defeated, "Actual player defeat was not recorded."); // 실제 패배 확인
                boss.TakeDamage(boss.MaximumHealth); // 패배 이후 지연된 보스 피해
                game.SetState(GameState.Victory); // 종료 이후 직접 승리 변경 시도
                FieldInfo bossDefeated = typeof(GameManager).GetField("actualBossDefeated", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 처치 성과 상태
                Require(game.CurrentState == GameState.Defeated && bossDefeated != null && !(bool)bossDefeated.GetValue(game), "Defeat or boss achievement changed after run ended."); // 실제 패배와 성과 보존
                AutomatedBossOutcomeRaceVerification.Finish(true, "First defeat and its achievement state remained unchanged after later boss damage."); // 실제 패배 경계 성공
                yield break; // 실제 시험 종료
            }
            bool cutCompleted = false; // 실제 절단 결과 수신 여부
            bool cutApplied = true; // 실제 절단 적용 결과
            resolver.QueueEliteCut(applied => // 실제 물리 스텝의 몸통 절단 예약
            {
                cutCompleted = true; // 실제 절단 결과 수신
                cutApplied = applied; // 실제 절단 결과 저장
            });
            resolver.QueueHealthDamage(health, 10000); // 같은 물리 스텝의 치명 피해 예약
            boss.TakeDamage(boss.MaximumHealth); // 예약 처리 전 실제 최종 보스 처치
            Require(game.CurrentState == GameState.Victory, "Actual boss defeat did not produce victory."); // 실제 승리 전환 확인
            resolve.Invoke(resolver, null); // 실제 물리 직후 충돌 예약 처리
            Require(cutCompleted && !cutApplied && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth && game.CurrentState == GameState.Victory, "Pending collisions changed player or outcome after actual victory."); // 실제 예약 취소와 승리 보존
            bool lateDamage = health.TakeDamage(10000); // 종료 이후 직접 피해 시도
            game.SetState(GameState.Defeated); // 종료 이후 직접 패배 변경 시도
            Require(!lateDamage && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth && game.CurrentState == GameState.Victory, "Late damage or state change overwrote actual victory."); // 실제 종료 상태와 플레이어 보존
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            Require(self != null, "Actual self-collision fixture missing."); // 실제 자기 충돌 구성 확인
            bool lateContact = resolver.TryApplyEnemyDirectContact(); // 종료 이후 즉시 적 접촉 시도
            bool lateCut = resolver.TryApplyEliteCut(); // 종료 이후 즉시 몸통 절단 시도
            bool lateSelf = self.TryApplySelfCollisionPenalty(); // 종료 이후 즉시 자기 충돌 시도
            Require(!lateContact && !lateCut && !lateSelf && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth, "Immediate collision fallback changed player after actual victory."); // 즉시 대체 판정의 종료 보호 확인
            AutomatedBossOutcomeRaceVerification.Finish(true, "Actual victory canceled pending collisions and ignored later damage or outcome changes."); // 실제 승리 경계 성공
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedBossOutcomeRaceVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
