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
    public static class AutomatedFirePoisonReactionVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonFirePoisonReactionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedFirePoisonReactionVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<FirePoisonReactionRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[FirePoisonReactionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class FirePoisonReactionRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 화염과 독 반응 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 처리기 조회
            Require(effects != null, "Actual attribute combat effects missing."); // 실제 처리기 확인
            WeaponTarget firstTarget = CheckOrder(effects, WeaponAttribute.Fire, WeaponAttribute.Poison); // 화염 다음 독 확인
            WeaponTarget secondTarget = CheckOrder(effects, WeaponAttribute.Poison, WeaponAttribute.Fire); // 독 다음 화염 확인
            float firstHealth = firstTarget.CurrentHealth; // 첫 대상의 반응 직후 체력
            float secondHealth = secondTarget.CurrentHealth; // 둘째 대상의 반응 직후 체력
            yield return new WaitForSeconds(1.1f); // 실제 전투 시간 진행
            Require(firstTarget.CurrentHealth < firstHealth && secondTarget.CurrentHealth < secondHealth, "Reaction damage did not advance over time."); // 양방향 반응 지속 피해 확인
            Require(firstTarget.GetComponent<WeaponTargetBurnStatus>() == null && firstTarget.GetComponent<WeaponTargetPoisonStatus>() == null, "Reaction damage reapplied consumed statuses."); // 지속 피해의 원래 상태 재적용 방지
            CheckInactivePoison(effects); // 독 효과가 없는 명중 확인
            CheckNoDeathChain(effects); // 반응 피해의 파생 폭발 차단
            AutomatedFirePoisonReactionVerification.Finish(0, "Both orders consumed statuses, applied reaction damage and suppressed death explosions."); // 실제 검증 성공 기록
        }

        private static WeaponTarget CheckOrder(WeaponAttributeCombatEffects effects, WeaponAttribute first, WeaponAttribute second) // 실제 반응 양방향 확인
        {
            WeaponTarget target = CreateTarget("FirePoisonReactionTarget", new Vector3(100f, 100f, 0f)); // 실제 생존 대상 생성
            WeaponAttributeAttackSnapshot firstAttack = new WeaponAttributeAttackSnapshot(null, first, 2, 2, 1, target.transform.position, 1f); // 첫 속성 공격 구성
            WeaponAttributeAttackSnapshot secondAttack = new WeaponAttributeAttackSnapshot(null, second, 2, 2, 1, target.transform.position, 1f); // 둘째 속성 공격 구성
            effects.ApplyHit(firstAttack, target, target.transform.position); // 실제 첫 속성 명중
            effects.ApplyHit(secondAttack, target, target.transform.position); // 실제 둘째 속성 명중
            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태 조회
            WeaponTargetPoisonStatus poison = target.GetComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태 조회
            Require((burn == null || burn.RemainingDuration <= 0f) && (poison == null || !poison.IsActive), "Fire and poison statuses were not consumed: " + first); // 반응 재료의 즉시 소비 확인
            Require(target.GetComponent("WeaponTargetCorrosiveBurnStatus") != null, "Reaction damage-over-time status was not applied: " + first); // 독립 반응 지속 피해 확인
            effects.ApplyHit(firstAttack, target, target.transform.position); // 같은 프레임의 첫 재료 재적용
            effects.ApplyHit(secondAttack, target, target.transform.position); // 같은 프레임의 둘째 재료 재적용
            Require((burn == null || burn.RemainingDuration <= 0f) && (poison == null || !poison.IsActive) && target.GetComponents<WeaponTargetCorrosiveBurnStatus>().Length == 1, "Same-frame reaction refresh duplicated or retained materials."); // 재발동 시 단일 결과와 재료 소비 확인
            return target; // 반응 대상 반환
        }
        private static void CheckInactivePoison(WeaponAttributeCombatEffects effects) // 독 효과가 없는 명중 확인
        {
            WeaponTarget target = CreateTarget("InactivePoisonTarget", new Vector3(200f, 200f, 0f)); // 독립 표적 생성
            WeaponAttributeAttackSnapshot fire = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Fire, 2, 2, 1, target.transform.position, 1f); // 실제 화상 공격
            WeaponAttributeAttackSnapshot poison = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Poison, 0, 0, 1, target.transform.position, 1f); // 비활성 독 공격
            effects.ApplyHit(fire, target, target.transform.position); // 실제 화상 적용
            effects.ApplyHit(poison, target, target.transform.position); // 독 약화 없는 명중
            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 조회
            Require(burn != null && burn.RemainingDuration > 0f && target.GetComponent<WeaponTargetCorrosiveBurnStatus>() == null, "Inactive poison incorrectly consumed fire."); // 독 부재 시 화상 보존 확인
        }
        private static void CheckNoDeathChain(WeaponAttributeCombatEffects effects) // 반응 피해 사망 연쇄 차단 확인
        {
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화염 폭발 포탑 데이터
            Require(weapon != null && weapon.GradeFiveBurnExplosionRadius > 0f, "Grade-five fire explosion data missing."); // 실제 폭발 데이터 확인
            WeaponTarget victim = CreateTarget("ReactionVictim", new Vector3(300f, 300f, 0f)); // 반응 피해 사망 대상
            WeaponTarget nearby = CreateTarget("ReactionNearby", new Vector3(301f, 300f, 0f)); // 폭발 반경 내 주변 대상
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Fire, 2, 2, 5, victim.transform.position, 1f); // 오 등급 화염 공격 구성
            effects.ApplyHit(attack, victim, victim.transform.position); // 실제 화상과 처치 폭발 연결
            Require(victim.GetComponent<WeaponBurnDeathExplosion>() != null, "Fire death explosion was not attached."); // 폭발 효과 연결 확인
            victim.Configure(1f); // 반응 피해의 처치 체력 설정
            int deaths = 0; // 실제 사망 이벤트 횟수
            victim.Died += ignored => deaths++; // 기존 사망 이벤트 확인
            float nearbyHealth = nearby.CurrentHealth; // 주변 대상의 기존 체력
            victim.TakeReactionDamage(2f); // 실제 반응 피해로 처치
            Require(deaths == 1 && Mathf.Approximately(nearby.CurrentHealth, nearbyHealth), "Reaction kill triggered a death explosion or lost its death event."); // 보상 이벤트 유지와 연쇄 차단 확인
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
                AutomatedFirePoisonReactionVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
