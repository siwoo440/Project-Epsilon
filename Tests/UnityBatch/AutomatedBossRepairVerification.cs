using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 전환과 보상 상태 조회
using ProjectEpsilon.Combat; // 실제 장착 무기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 출현기와 보스
using ProjectEpsilon.Player; // 실제 플레이어 구성
using ProjectEpsilon.Progression; // 실제 경험치와 복구 아이템
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossRepairVerification // 실제 보스전 복구 제한 검증
    {
        private const string ActiveKey = "EpsilonBossRepairVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedBossRepairVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 제한 시간
            {
                Finish(1, "Timed out waiting for boss repair test."); // 시간 초과 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 확인
            {
                return; // 실제 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 실제 초기화 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossRepairVerificationRunner>(); // 실제 복구 제한 시험 실행
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
            Debug.Log($"[BossRepairVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }

    public sealed class BossRepairVerificationRunner : MonoBehaviour // 실제 보스전 복구 제한 시나리오
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 필드 범위
        private IEnumerator Start() // 실제 전환 전후 복구와 레벨업
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 전투 출현기
            EliteRewardController rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 보상
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자
            Require(player != null && health != null && body != null && follower != null && spawner != null && rewards != null && experience != null, "Actual boss repair scene fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 플레이어 이동 통제
            follower.enabled = false; // 실제 몸통 추적 통제
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌 조회
            if (self != null) // 실제 자기 충돌 확인
            {
                self.enabled = false; // 시험 중 자기 충돌 방지
            }
            if (external != null) // 실제 외부 충돌 확인
            {
                external.enabled = false; // 시험 중 외부 충돌 방지
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            {
                slot.Clear(); // 시험 중 자동 공격 방지
            }
            Require(body.RemoveBodies(1) == 1 && body.TryRepairBody() && body.CurrentBodyCount == 3, "Actual repair before boss did not work."); // 보스 이전 실제 복구 대조
            Require(body.RemoveBodies(1) == 1 && body.CurrentBodyCount == 2 && body.EarnedBodyCount == 3, "Actual missing-body fixture failed."); // 실제 몸통 손실 구성
            FieldInfo elapsed = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Private); // 실제 출현 경과 시간 조회
            Require(elapsed != null, "Actual transition clock fixture missing."); // 실제 시계 필드 확인
            elapsed.SetValue(spawner, 1199.99f); // 시험용 20분 직전 시간 주입
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 전환 제한 시각
            while (!spawner.HasEnteredBossTransition && Time.realtimeSinceStartup < deadline) // 실제 전환 대기
            {
                yield return null; // 실제 게임 프레임 진행
            }
            Require(spawner.HasEnteredBossTransition && body.CurrentBodyCount == 2, "Actual boss transition fixture failed."); // 실제 전환과 몸통 확인
            Require(!body.TryRepairBody() && body.CurrentBodyCount == 2, "Body repair API remained usable during the boss encounter."); // 실제 별도 몸통 복구 차단 확인
            MethodInfo availabilityMethod = typeof(EliteRewardController).GetMethod("GetAvailability", Private); // 실제 엘리트 후보 상태 조회
            Require(availabilityMethod != null, "Actual elite reward availability path missing."); // 실제 보상 상태 경로 확인
            bool[] available = (bool[])availabilityMethod.Invoke(rewards, null); // 실제 현재 보상 가능 상태
            Require(available.Length >= 2 && !available[1], "Elite repair reward remained available during the boss encounter."); // 실제 별도 복구 보상 차단
            RecoveryPickup repair = RecoveryPickup.Spawn(player.transform.position, player.transform, health, body, RecoveryPickupType.BodyRepair, 1, null); // 전환 이후 실제 복구 아이템
            yield return null; // 실제 복구 아이템 갱신
            yield return null; // 실제 수집과 지연 파괴 대기
            Require(repair != null && body.CurrentBodyCount == 2, "Post-transition repair pickup restored a body."); // 실제 복구 아이템 차단 확인
            int required = experience.RequiredExperience; // 실제 현재 레벨 요구 경험치
            experience.AddExperience(required); // 실제 레벨업 요청
            yield return null; // 실제 레벨업 성장 처리 대기
            Require(experience.IsLevelUpPending && body.CurrentBodyCount == 3 && body.EarnedBodyCount == 4, "Level-up body growth was incorrectly blocked in the boss encounter."); // 실제 레벨업 몸통 성장 유지 확인
            Debug.Log("[BossRepairVerification] actual pre-boss repair succeeded; boss repair API, reward availability and repair pickup blocked; actual XP level-up grew current 2->3 and earned 3->4; transition time injected."); // 실제 통제 조건과 결과 기록
            AutomatedBossRepairVerification.Finish(0, "Actual boss repair restriction and level-up growth passed."); // 실제 전체 결과
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedBossRepairVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
