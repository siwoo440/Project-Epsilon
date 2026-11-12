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
    public static class AutomatedPhysicalExplosionReactionVerification // 실제 신성과 암흑 시험
    {
        private const string Key = "EpsilonPhysicalExplosionReactionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedPhysicalExplosionReactionVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<PhysicalExplosionReactionRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[PhysicalExplosionReactionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class PhysicalExplosionReactionRunner : MonoBehaviour // 실제 신성 및 암흑 시험 실행기
    {
        private IEnumerator Start() // 실제 물리와 폭발 반응 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 중지
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 처리기 조회
            Require(effects != null, "Actual attribute combat effects missing."); // 실제 처리기 확인
            CheckOrder(effects, WeaponAttribute.Physical, WeaponAttribute.Explosion, 500f); // 물리 다음 폭발 확인
            CheckOrder(effects, WeaponAttribute.Explosion, WeaponAttribute.Physical, 700f); // 폭발 다음 물리 확인
            yield return null; // 실제 상태 갱신 대기
            AutomatedPhysicalExplosionReactionVerification.Finish(0, "Both orders consumed markers and applied area damage without bypassing boss knockback immunity."); // 실제 검증 성공 기록
        }

        private static void CheckOrder(WeaponAttributeCombatEffects effects, WeaponAttribute first, WeaponAttribute second, float x) // 실제 반응과 면역 확인
        {
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/PrototypeExplosionBolt.asset"); // 실제 폭발 포탑 데이터
            Require(weapon != null && weapon.Range > 1f, "Actual explosion weapon missing."); // 실제 폭발 포탑 확인
            WeaponTarget target = CreateTarget("PhysicalExplosionTarget", new Vector3(x, 500f, 0f)); // 실제 생존 대상 생성
            WeaponTarget neighbor = CreateTarget("PhysicalExplosionNeighbor", new Vector3(x + 1f, 500f, 0f)); // 반응 범위 내 대상
            WeaponTargetKnockbackController immune = neighbor.gameObject.AddComponent<WeaponTargetKnockbackController>(); // 실제 넉백 관리자
            immune.Configure(true); // 보스 넉백 면역 설정
            WeaponAttributeAttackSnapshot firstAttack = new WeaponAttributeAttackSnapshot(first == WeaponAttribute.Explosion ? weapon : null, first, 2, 2, 1, target.transform.position, 1f); // 첫 속성 공격 구성
            WeaponAttributeAttackSnapshot secondAttack = new WeaponAttributeAttackSnapshot(second == WeaponAttribute.Explosion ? weapon : null, second, 2, 2, 1, target.transform.position, 1f); // 둘째 속성 공격 구성
            effects.ApplyHit(firstAttack, target, target.transform.position); // 실제 첫 속성 명중
            float previousNeighborHealth = neighbor.CurrentHealth; // 두 번째 명중 전 주변 체력
            effects.ApplyHit(secondAttack, target, target.transform.position); // 실제 둘째 속성 명중
            WeaponTargetReactionMaterials materials = target.GetComponent<WeaponTargetReactionMaterials>(); // 실제 반응 재료 조회
            Require(materials != null && !materials.State.IsActive(WeaponAttribute.Physical, Time.time) && !materials.State.IsActive(WeaponAttribute.Explosion, Time.time), "Physical and explosion materials were not consumed: " + first); // 두 표식 소비 확인
            Require(previousNeighborHealth - neighbor.CurrentHealth > 1.01f && !immune.IsActive, "Reaction area damage missing or boss knockback immunity bypassed: " + first); // 추가 범위 피해와 넉백 면역 확인
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
                AutomatedPhysicalExplosionReactionVerification.Finish(1, message); // 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
