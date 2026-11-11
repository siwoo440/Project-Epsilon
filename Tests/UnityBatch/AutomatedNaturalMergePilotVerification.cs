using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 HUD 필드 조회
using ProjectEpsilon.Combat; // 무기와 표적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 데이터
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치
using ProjectEpsilon.UI; // 실제 HUD
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 HUD 텍스트

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedNaturalMergePilotVerification // 자동 조종기 합성 선택 시험
    {
        private const string Key = "EpsilonNaturalMergePilotVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedNaturalMergePilotVerification() // 도메인 재시작 처리
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
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
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
            player.gameObject.AddComponent<NaturalMergePilotVerificationRunner>(); // 실제 시험 실행
        }
        public static void Finish(int code, string message) // 실제 시험 종료
        {
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[NaturalMergePilotVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        }
    }
    public sealed class NaturalMergePilotVerificationRunner : MonoBehaviour // 실제 합성 자동 선택 실행기
    {
        private IEnumerator Start() // 실제 합성 흐름 시험
        {
            GetComponent<SnakeMovement>().enabled = false; // 시험 중 플레이어 이동 중지
            UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 관리자
            weapons.enabled = false; // 시험 중 자동 공격 중지
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 시작 포탑 데이터
            Require(weapon != null && merge != null && weapons.TryEquipAt(0, weapon) && weapons.TryEquipAt(1, weapon), "Merge pilot setup failed."); // 실제 동일 포탑 장착 확인
            Require(merge.OpenMergePanel(), "Actual merge panel did not open."); // 실제 합성 화면 열기
            MethodInfo click = typeof(AutomatedNaturalCombatVerification).GetMethod("ClickFirstAvailableMergeButton", BindingFlags.Static | BindingFlags.NonPublic); // 실제 자연 전투 조종기 선택 메서드
            Require(click != null, "Natural merge pilot handler missing."); // 실제 조종기 확인
            for (int index = 0; index < 5 && merge.IsMergeOpen; index++) // 실제 합성 단계 순회
            {
                click.Invoke(null, null); // 실제 자연 전투 조종기의 버튼 선택
                yield return null; // 실제 화면 단계 갱신 대기
            }
            Require(!merge.IsMergeOpen && weapons.Slots[0].Weapon == weapon && weapons.Slots[0].Grade == 2 && weapons.OccupiedSlotCount == 1, "Natural pilot did not complete the real merge."); // 실제 재료 선택과 합성 완료 확인
            Require(Mathf.Abs(Time.timeScale - 1f) < 0.001f, "Merge time scale was not restored."); // 실제 합성 시간 복구 확인
            AutomatedNaturalMergePilotVerification.Finish(0, "Natural pilot selected real materials, inherited enhancement and completed merge."); // 전체 성공 기록
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedNaturalMergePilotVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
