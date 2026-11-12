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
    public static class AutomatedRemainingReactionVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonRemainingReactionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedRemainingReactionVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<RemainingReactionRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[RemainingReactionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class RemainingReactionRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 남은 세 가지 실제 반응 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 처리기 조회
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            Require(effects != null && health != null, "Actual attribute or health dependencies missing."); // 실제 구성 확인
            CheckSacred(effects, WeaponAttribute.Fire, WeaponAttribute.Holy, 300f); // 화염 다음 신성
            CheckSacred(effects, WeaponAttribute.Holy, WeaponAttribute.Fire, 400f); // 신성 다음 화염
            WeaponTarget expiring = CheckCorrosiveCurse(effects, WeaponAttribute.Dark, WeaponAttribute.Poison, 500f); // 암흑 다음 독
            CheckCorrosiveCurse(effects, WeaponAttribute.Poison, WeaponAttribute.Dark, 600f); // 독 다음 암흑
            CheckLightDark(effects, health, WeaponAttribute.Holy, WeaponAttribute.Dark, 700f); // 신성 다음 암흑
            CheckLightDark(effects, health, WeaponAttribute.Dark, WeaponAttribute.Holy, 800f); // 암흑 다음 신성
            yield return new WaitForSeconds(3.2f); // 반응 약화 만료까지 실제 전투 시간 진행
            Require(Mathf.Approximately(expiring.OutgoingDamageMultiplier, 1f) && Mathf.Approximately(expiring.IncomingDamageMultiplier, 1f), "Reaction weakening did not expire."); // 반응 약화 만료 확인
            AutomatedRemainingReactionVerification.Finish(0, "Actual sacred fire, dark poison and holy dark reactions passed in both orders."); // 실제 검증 성공 기록
        }

        private static void CheckSacred(WeaponAttributeCombatEffects effects, WeaponAttribute first, WeaponAttribute second, float x) // 성화 폭발 양방향 확인
        {
            WeaponTarget target = CreateTarget("SacredTarget", new Vector3(x, 100f, 0f)); // 실제 반응 대상
            WeaponTarget neighbor = CreateTarget("SacredNeighbor", new Vector3(x + 1f, 100f, 0f)); // 반응 범위 대상
            Hit(effects, target, first); // 첫 속성 명중
            float previous = neighbor.CurrentHealth; // 주변 체력 저장
            Hit(effects, target, second); // 둘째 속성 명중
            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 조회
            Require(burn != null && burn.RemainingDuration <= 0f && neighbor.CurrentHealth < previous, "Sacred fire did not consume burn or damage the neighbor: " + first); // 화상 소비와 범위 피해 확인
        }

        private static WeaponTarget CheckCorrosiveCurse(WeaponAttributeCombatEffects effects, WeaponAttribute first, WeaponAttribute second, float x) // 부식 저주 양방향 확인
        {
            WeaponTarget target = CreateTarget("CorrosiveCurseTarget", new Vector3(x, 100f, 0f)); // 실제 반응 대상
            Hit(effects, target, first); // 첫 속성 명중
            Hit(effects, target, second); // 둘째 속성 명중
            WeaponTargetPoisonStatus poison = target.GetComponent<WeaponTargetPoisonStatus>(); // 기존 독 상태 조회
            WeaponTargetDarkCurseStatus curse = target.GetComponent<WeaponTargetDarkCurseStatus>(); // 기존 암흑 저주 조회
            WeaponTargetCorrosiveCurseStatus result = target.GetComponent<WeaponTargetCorrosiveCurseStatus>(); // 반응 전용 약화 조회
            Require(poison != null && !poison.IsActive && curse != null && !curse.IsActive && result != null, "Dark poison did not consume both statuses: " + first); // 기존 두 상태 소비 확인
            Require(target.OutgoingDamageMultiplier < 1f && target.IncomingDamageMultiplier > 1f, "Dark poison did not apply separate attack and defense weakening: " + first); // 반응 전용 약화 확인
            return target; // 실제 반응 대상 반환
        }

        private static void CheckLightDark(WeaponAttributeCombatEffects effects, SnakeHealth health, WeaponAttribute first, WeaponAttribute second, float x) // 명암 상쇄 양방향 확인
        {
            health.Heal(health.MaximumHealth); // 이전 시험 체력 복구
            Require(health.TakeDamage(20), "Could not prepare player health."); // 실제 회복 여유 확보
            int previousHealth = health.CurrentHealth; // 회복 전 공용 체력
            WeaponTarget target = CreateTarget("LightDarkTarget", new Vector3(x, 100f, 0f)); // 실제 반응 대상
            Hit(effects, target, first); // 첫 속성 명중
            float before = target.CurrentHealth; // 반응 전 대상 체력
            Hit(effects, target, second); // 둘째 속성 명중
            WeaponTargetDarkCurseStatus curse = target.GetComponent<WeaponTargetDarkCurseStatus>(); // 실제 저주 조회
            Require(curse != null && !curse.IsActive && before - target.CurrentHealth > 1f && health.CurrentHealth > previousHealth, "Holy dark did not consume curse, deal damage and heal: " + first); // 저주 소비와 반응 피해 및 회복 확인
        }

        private static void Hit(WeaponAttributeCombatEffects effects, WeaponTarget target, WeaponAttribute attribute) // 실제 속성 명중
        {
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, attribute, 2, 2, 1, target.transform.position, 1f); // 실제 공격 구성
            effects.ApplyHit(attack, target, target.transform.position); // 실제 전투 명중 적용
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedRemainingReactionVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
