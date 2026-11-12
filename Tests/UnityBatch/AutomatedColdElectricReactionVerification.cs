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
    public static class AutomatedColdElectricReactionVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonColdElectricReactionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedColdElectricReactionVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<ColdElectricReactionRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[ColdElectricReactionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class ColdElectricReactionRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 냉기와 전기 반응 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 처리기 조회
            Require(effects != null, "Actual attribute combat effects missing."); // 실제 처리기 확인
            CheckOrder(effects, WeaponAttribute.Cold, WeaponAttribute.Electric, false); // 일반 적의 냉기 다음 전기
            CheckOrder(effects, WeaponAttribute.Electric, WeaponAttribute.Cold, false); // 일반 적의 전기 다음 냉기
            CheckOrder(effects, WeaponAttribute.Cold, WeaponAttribute.Electric, true); // 보스 저항 대상 확인
            CheckElectricChain(effects); // 전기 연쇄의 추가 대상 확인
            yield return null; // 실제 상태 갱신 대기
            AutomatedColdElectricReactionVerification.Finish(0, "Both orders consumed cold and electric, dealt reaction damage and preserved freeze resistance."); // 실제 검증 성공 기록
        }

        private static void CheckOrder(WeaponAttributeCombatEffects effects, WeaponAttribute first, WeaponAttribute second, bool resistant) // 실제 반응과 저항 확인
        {
            WeaponTarget target = CreateTarget("ColdElectricReactionTarget", new Vector3(100f, 100f, 0f)); // 실제 생존 대상 생성
            WeaponTargetStatusController statuses = target.gameObject.AddComponent<WeaponTargetStatusController>(); // 실제 이동 상태 관리자
            statuses.ConfigureColdFreezeResistance(resistant, 0.5f); // 보스 빙결 저항 설정
            WeaponAttributeAttackSnapshot firstAttack = new WeaponAttributeAttackSnapshot(null, first, 2, 2, 1, target.transform.position, 1f); // 첫 속성 공격 구성
            WeaponAttributeAttackSnapshot secondAttack = new WeaponAttributeAttackSnapshot(null, second, 2, 2, 1, target.transform.position, 1f); // 둘째 속성 공격 구성
            effects.ApplyHit(firstAttack, target, target.transform.position); // 실제 첫 속성 명중
            effects.ApplyHit(secondAttack, target, target.transform.position); // 실제 둘째 속성 명중
            WeaponTargetColdStatus cold = target.GetComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태 조회
            WeaponTargetReactionMaterials materials = target.GetComponent<WeaponTargetReactionMaterials>(); // 실제 반응 재료 조회
            Require(cold != null && cold.RemainingDuration <= 0f && materials != null && !materials.State.IsActive(WeaponAttribute.Electric, Time.time), "Cold and electric materials were not consumed: " + first); // 냉기와 전기 재료 소비 확인
            Require(target.CurrentHealth < 998f && statuses.IsSlowActive && !statuses.IsStopActive, "Reaction damage or strong slow was not applied safely: " + first); // 추가 피해와 빙결 없는 감속 확인
        }
        private static void CheckElectricChain(WeaponAttributeCombatEffects effects) // 전기 연쇄 추가 명중의 반응 확인
        {
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeElectricBolt.asset"); // 실제 전기 포탑 데이터
            Require(weapon != null && weapon.Range > 1f, "Electric chain weapon data missing."); // 실제 전기 포탑 확인
            WeaponTarget primary = CreateTarget("ElectricChainPrimary", new Vector3(500f, 500f, 0f)); // 첫 전기 대상
            WeaponTarget secondary = CreateTarget("ElectricChainSecondary", new Vector3(501f, 500f, 0f)); // 연쇄 대상
            WeaponAttributeAttackSnapshot cold = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Cold, 2, 2, 1, secondary.transform.position, 1f); // 연쇄 대상 냉기 공격
            WeaponAttributeAttackSnapshot electric = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Electric, 2, 2, 1, primary.transform.position, 1f); // 실제 전기 연쇄 공격
            effects.ApplyHit(cold, secondary, secondary.transform.position); // 연쇄 대상 냉기 적용
            effects.ApplyHit(electric, primary, primary.transform.position); // 첫 대상의 실제 전기 연쇄
            WeaponTargetColdStatus status = secondary.GetComponent<WeaponTargetColdStatus>(); // 연쇄 대상 냉기 조회
            Require(status != null && status.RemainingDuration <= 0f && secondary.CurrentHealth < 998f, "Electric chain did not trigger the cold reaction on its secondary target."); // 연쇄 대상 반응과 재료 소비 확인
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
                AutomatedColdElectricReactionVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
