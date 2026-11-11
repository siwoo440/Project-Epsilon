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
    public static class AutomatedDay40StatusVerification // 실제 독과 폭발 시험
    {
        private const string Key = "EpsilonDay40StatusVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay40StatusVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<Day40StatusRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day40StatusVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class Day40StatusRunner : MonoBehaviour // 실제 속성 효과 시험 실행기
    {
        private IEnumerator Start() // 실제 독과 폭발 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 효과 조회
            WeaponData poisonWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Day14/DebugPoison.asset"); // 기존 독 시험 무기
            WeaponData explosionWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Day14/DebugExplosion.asset"); // 기존 폭발 시험 무기
            Require(effects != null && poisonWeapon != null && explosionWeapon != null, "Status test dependencies missing."); // 시험 구성 확인
            Require(Mathf.Abs(WeaponAttributeExplosionRules.GetKnockbackDistance(4) - 0.9f) < 0.001f, "Explosion stage four rule mismatch."); // 사 단계 넉백 증가 확인
            WeaponTarget poisoned = CreateTarget("PoisonTarget", new Vector3(100f, 100f, 0f)); // 실제 독 표적
            WeaponAttributeAttackSnapshot poison = new WeaponAttributeAttackSnapshot(poisonWeapon, WeaponAttribute.Poison, 2, 2, 1, poisoned.transform.position, 1f); // 실제 독 명중 정보
            for (int index = 0; index < 8; index++) // 최대 중첩 초과 명중 순회
            {
                effects.ApplyHit(poison, poisoned, poisoned.transform.position); // 실제 독 명중 처리
            }
            WeaponTargetPoisonStatus status = poisoned.GetComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태 조회
            Require(status != null && status.StackCount == 5 && status.IsActive, "Poison did not cap at five stacks."); // 독 최대 다섯 중첩 확인
            WeaponTarget low = CreateTarget("ExplosionLow", new Vector3(200f, 100f, 0f)); // 실제 낮은 단계 표적
            WeaponTarget high = CreateTarget("ExplosionHigh", new Vector3(300f, 100f, 0f)); // 실제 높은 단계 표적
            WeaponTarget immune = CreateTarget("ExplosionImmune", new Vector3(400f, 100f, 0f)); // 실제 면역 표적
            immune.gameObject.AddComponent<WeaponTargetKnockbackController>().Configure(true); // 보스 넉백 면역 설정
            HitExplosion(effects, explosionWeapon, low, 2); // 낮은 단계 실제 폭발
            HitExplosion(effects, explosionWeapon, high, 4); // 높은 단계 실제 폭발
            HitExplosion(effects, explosionWeapon, immune, 4); // 면역 표적 실제 폭발
            WeaponTargetKnockbackController lowPush = low.GetComponent<WeaponTargetKnockbackController>(); // 낮은 단계 넉백
            WeaponTargetKnockbackController highPush = high.GetComponent<WeaponTargetKnockbackController>(); // 높은 단계 넉백
            Require(lowPush != null && Mathf.Abs(lowPush.RemainingDistance - 0.75f) < 0.001f, "Explosion base knockback mismatch."); // 기존 임시 넉백 확인
            Require(highPush != null && Mathf.Abs(highPush.RemainingDistance - 0.9f) < 0.001f, "Explosion stage four knockback mismatch."); // 사 단계 넉백 증가 확인
            Require(!immune.GetComponent<WeaponTargetKnockbackController>().IsActive, "Explosion ignored boss immunity."); // 보스 넉백 면역 확인
            yield return null; // 실제 프레임 대기
            AutomatedDay40StatusVerification.Finish(0, "Actual poison stack cap and explosion stage four knockback with boss immunity."); // 전체 성공 기록
        }
        private static WeaponTarget CreateTarget(string name, Vector3 position) // 실제 공격 대상 생성
        {
            GameObject item = new GameObject(name); // 통제 표적 객체
            item.transform.position = position; // 다른 적과 떨어진 표적 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(1000f); // 실제 표적 체력
            return target; // 실제 표적 반환
        }
        private static void HitExplosion(WeaponAttributeCombatEffects effects, WeaponData weapon, WeaponTarget target, int stage) // 실제 폭발 명중
        {
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Explosion, stage, stage, 1, target.transform.position, 10f); // 실제 폭발 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 폭발과 넉백 처리
        }
        private static void Require(bool condition, string message) // 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedDay40StatusVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
