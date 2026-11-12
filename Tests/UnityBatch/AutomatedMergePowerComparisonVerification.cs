using System; // 실제 예외 형식
using System.Collections; // 실제 시험 코루틴
using System.Reflection; // 실제 출현기 내부 조회
using ProjectEpsilon.Combat; // 실제 적 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적과 출현기
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 실제 Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 실제 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedMergePowerComparisonVerification // 구간별 실제 적 출현 검증
    {
        private const string ActiveKey = "EpsilonMergePowerComparisonVerification"; // 실제 시험 진행 표식
        private static bool injected; // 실제 실행기 생성 여부
        private static double started; // 실제 시험 시작 시각

        static AutomatedMergePowerComparisonVerification() // 실제 도메인 재시작 처리
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
            player.gameObject.AddComponent<MergePowerComparisonRunner>(); // 실제 구간 시험 실행
        }

        public static void Finish(int code, string message) // 실제 시험 결과와 종료
        {
            SessionState.SetBool(ActiveKey, false); // 실제 시험 표식 제거
            EditorApplication.update -= Tick; // 실제 감시 연결 해제
            Debug.Log($"[MergePowerComparisonVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드 반환
        }
    }

    public sealed class MergePowerComparisonRunner : MonoBehaviour // 실제 출현 구간 시험 실행기
    {
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 내부 필드 범위

        private IEnumerator Start() // 실제 합성 전후 단일 대상 화력 비교
        {
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            WeaponAttributeSynergyManager synergy = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 시너지
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 강철 근접 포탑
            Require(spawner != null && weapons != null && synergy != null && weapon != null, "Actual merge comparison dependencies missing."); // 실제 구성 확인
            spawner.enabled = false; // 자연 출현 중지
            GetComponent<SnakeMovement>().enabled = false; // 머리 이동 중지
            weapons.enabled = false; // 자동 발사 중지
            foreach (WeaponTarget existing in UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 표적 순회
            {
                existing.gameObject.SetActive(false); // 기존 적의 표적 제외
                Destroy(existing.gameObject); // 기존 적 제거
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 시작 슬롯 순회
            {
                slot.Clear(); // 기존 시험 포탑 해제
            }
            Require(weapons.TryEquipAt(0, weapon, 1) && weapons.TryEquipAt(1, weapon, 1), "Two matching grade-one turrets could not equip."); // 같은 포탑 두 개 장착
            Require(synergy.GetCount(WeaponAttribute.Physical) == 2 && synergy.GetStage(WeaponAttribute.Physical) == 2, "Physical synergy did not activate before merging."); // 합성 전 속성 단계 확인
            GameObject item = new GameObject("MergePowerTarget"); // 실제 단일 대상 생성
            item.transform.position = new Vector3(100f, 100f, 0f); // 기존 플레이어와 분리된 표적
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 피해 대상
            target.Configure(10000f); // 피해량 측정용 체력
            MethodInfo attack = typeof(SnakeWeaponManager).GetMethod("TryMeleeAttack", Private); // 실제 근접 공격 경로
            Require(attack != null, "Actual turret attack method missing."); // 실제 공격 경로 확인
            float before = target.CurrentHealth; // 합성 전 체력
            Require((bool)attack.Invoke(weapons, new object[] { weapons.Slots[0], weapon }) && (bool)attack.Invoke(weapons, new object[] { weapons.Slots[1], weapon }), "Actual pre-merge attacks failed."); // 두 포탑의 실제 공격
            float preDamage = before - target.CurrentHealth; // 두 공격의 실제 피해
            float preDps = preDamage / weapon.AttackInterval; // 같은 주기의 단일 대상 평균 화력
            Require(weapons.TryMergeSlots(0, 1, WeaponMergeUpgrade.Damage), "Actual matching turret merge failed."); // 실제 공격력 강화 합성
            Require(synergy.GetCount(WeaponAttribute.Physical) == 1 && synergy.GetStage(WeaponAttribute.Physical) == 0, "Physical synergy did not update immediately after merging."); // 합성 직후 시너지 감소 확인
            Require(weapons.Slots[0].Grade == 2 && weapons.Slots[1].IsEmpty, "Merge did not leave one grade-two turret and one empty slot."); // 실제 합성 결과 확인
            before = target.CurrentHealth; // 합성 후 체력
            Require((bool)attack.Invoke(weapons, new object[] { weapons.Slots[0], weapon }), "Actual post-merge attack failed."); // 합성 포탑의 실제 공격
            float postDamage = before - target.CurrentHealth; // 합성 후 실제 피해
            float postDps = postDamage / weapon.AttackInterval; // 같은 주기의 단일 대상 평균 화력
            Require(preDamage > 0f && postDamage > 0f && postDps < preDps, "Controlled single-target comparison did not expose the expected synergy tradeoff."); // 실제 화력 비교 확인
            Debug.Log($"[MergePowerComparisonVerification] controlled blade: before damage={preDamage:F3}, interval={weapon.AttackInterval:F3}, dps={preDps:F3}, physical=2; after damage={postDamage:F3}, dps={postDps:F3}, physical=1, grade=2, upgrade=damage. Actual hit samples; no crowd, survival or real play claim."); // 통제된 실측 조건과 결과 기록
            yield return null; // 실제 표시 프레임 대기
            AutomatedMergePowerComparisonVerification.Finish(0, "Actual merge, immediate synergy loss and controlled single-target damage comparison passed."); // 실제 검증 성공 기록
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedMergePowerComparisonVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 추가 처리 중단
            }
        }
    }
}
