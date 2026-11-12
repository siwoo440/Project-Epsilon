using System; // 실제 예외 형식
using System.Collections; // 실제 게임 프레임 대기
using System.Collections.Generic; // 실제 후보와 식별자 목록
using System.Reflection; // 실제 조정 확률 조회
using ProjectEpsilon.Combat; // 실제 장착 포탑
using ProjectEpsilon.Core; // 실제 전투 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Player; // 실제 플레이어
using ProjectEpsilon.Progression; // 실제 보상 풀
using UnityEditor; // 실제 Unity 배치와 에셋 조회
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 실제 플레이 진입 후 감시 복구
    public static class AutomatedFocusedRewardVerification // 실제 포탑 집중 후보 검증
    {
        private const string ActiveKey = "EpsilonFocusedRewardVerification"; // 실제 시험 진행 표식
        private static bool injected; // 실제 실행기 생성 여부
        private static double started; // 실제 시험 시작 시각

        static AutomatedFocusedRewardVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 시험 진행 확인
            {
                EditorApplication.update += Observe; // 실제 전투 시작 관찰
            }
        }

        public static void Run() // 실제 게임 씬 시험 시작
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 기록
            EditorApplication.update -= Observe; // 기존 관찰 연결 해제
            EditorApplication.update += Observe; // 실제 시작 관찰 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }

        private static void Observe() // 실제 시작 선택과 실행기 준비
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 플레이 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            {
                Finish(false, "Timed out before reward test."); // 실제 시간 초과 기록
                return; // 추가 처리 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 선택과 실행 여부 확인
            {
                return; // 실제 전투와 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 전투 초기화 확인
            {
                return; // 실제 전투 시작 대기
            }
            injected = true; // 실행기 중복 생성 차단
            player.gameObject.AddComponent<FocusedRewardVerificationRunner>(); // 실제 보상 후보 시험 시작
        }

        public static void Finish(bool passed, string message) // 실제 시험 결과와 종료
        {
            SessionState.SetBool(ActiveKey, false); // 실제 시험 진행 표식 제거
            EditorApplication.update -= Observe; // 실제 감시 연결 해제
            Debug.Log("[FocusedRewardVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 실제 종료 코드 반환
        }
    }

    public sealed class FocusedRewardVerificationRunner : MonoBehaviour // 실제 게임 보상 후보 시험 실행기
    {
        private IEnumerator Start() // 실제 후보 풀과 슬롯 검증
        {
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 장착 관리자
            WeaponRewardPool original = AssetDatabase.LoadAssetAtPath<WeaponRewardPool>("Assets/Data/Progression/DebugWeaponRewardPool.asset"); // 실제 48종 후보 데이터
            WeaponData a = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 첫 포탑
            WeaponData b = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugNeedle.asset"); // 실제 두 번째 포탑
            WeaponData c = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 세 번째 포탑
            Require(weapons != null && weapons.Slots.Count >= 3 && original != null && original.Entries.Count == 48 && a != null && b != null && c != null, "Actual 48-entry reward fixtures are missing."); // 실제 에셋과 슬롯 확인
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal); // 실제 전체 포탑 ID 집합
            foreach (WeaponRewardEntry entry in original.Entries) // 실제 48개 후보 순회
            {
                Require(entry != null && entry.Weapon != null && !string.IsNullOrEmpty(entry.Weapon.Id) && ids.Add(entry.Weapon.Id), "Actual 48-entry pool contains a missing or duplicate ID."); // 실제 후보 고유 식별자 확인
            }
            FieldInfo chance = typeof(WeaponRewardPool).GetField("focusedCandidateChance", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 조정 가능한 집중 확률
            Require(chance != null && Mathf.Approximately((float)chance.GetValue(original), 0.4f), "Actual focus probability differs from 40 percent."); // 실제 데이터의 40퍼센트 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 플레이어 이동 통제
            weapons.enabled = false; // 실제 포탑 자동 공격 통제
            WeaponRewardPool pool = UnityEngine.Object.Instantiate(original); // 실제 보상 풀의 메모리 사본
            WeaponData sameA = UnityEngine.Object.Instantiate(a); // 실제 동일 ID의 별도 데이터 사본
            pool.Configure(new[] { a, b, c }); // 실제 세 종류의 통제된 후보 풀
            weapons.Slots[0].Equip(a, 1); // 실제 첫 포탑 1등급 장착
            weapons.Slots[1].Equip(sameA, 2); // 같은 ID 포탑의 다른 등급 장착
            weapons.Slots[2].Equip(b, 1); // 실제 다른 포탑 장착
            UnityEngine.Random.InitState(6201); // 실제 확률 시험 시드
            int[] locations = new int[3]; // 실제 최다 포탑 카드별 횟수
            const int attempts = 12000; // 실제 후보 추첨 횟수
            for (int iteration = 0; iteration < attempts; iteration++) // 실제 세 장 후보 반복 추첨
            {
                List<WeaponRewardCandidate> candidates = pool.BuildCandidates(1, weapons, 3); // 실제 첫 카드 집중 후보
                Require(candidates.Count == 3 && Unique(candidates), "Actual candidate IDs duplicated."); // 실제 세 카드 중복 방지 확인
                for (int index = 0; index < candidates.Count; index++) // 실제 세 카드 순회
                {
                    locations[index] += candidates[index].Weapon.Id == a.Id ? 1 : 0; // 실제 최다 포탑 위치 집계
                    Require(candidates[index].Grade == 1, "Actual early candidate grade differs."); // 실제 초반 등급 확인
                }
            }
            Require(locations[0] > attempts * 0.57f && locations[0] < attempts * 0.63f && locations[1] > attempts * 0.17f && locations[1] < attempts * 0.23f && locations[2] > attempts * 0.17f && locations[2] < attempts * 0.23f, "Actual first-card 40 percent focus distribution differs."); // 실제 첫 카드와 나머지 확률 확인
            weapons.Slots[0].Equip(a, 5); // 실제 최다 포탑의 5등급 장착
            weapons.Slots[1].Clear(); // 실제 중복 포탑 제거
            weapons.Slots[2].Equip(b, 1); // 실제 다른 포탑 유지
            pool.Configure(new[] { a, sameA, b, c }); // 실제 동일 ID 중복 후보 구성
            Require(weapons.HasCompletedGradeFive(sameA), "Actual grade-five lookup did not match the same weapon ID."); // 실제 같은 ID 5등급 확인
            for (int iteration = 0; iteration < 100; iteration++) // 실제 완료 포탑 제외 반복
            {
                List<WeaponRewardCandidate> candidates = pool.BuildCandidates(20, weapons, 4); // 실제 5등급 제외 후보
                Require(candidates.Count == 2 && Unique(candidates) && candidates[0].Weapon.Id != a.Id && candidates[1].Weapon.Id != a.Id, "Actual grade-five ID was not excluded."); // 실제 완성 ID 전체 제외 확인
            }
            weapons.Slots[0].Clear(); // 실제 5등급 포탑 제거
            List<WeaponRewardCandidate> restored = pool.BuildCandidates(20, weapons, 4); // 실제 포탑 후보 복귀
            Require(restored.Count == 3 && Unique(restored) && restored.Exists(item => item.Weapon.Id == a.Id), "Actual removed grade-five ID did not return once."); // 실제 후보 복귀와 중복 제거 확인
            UnityEngine.Object.Destroy(pool); // 실제 시험용 풀 사본 제거
            UnityEngine.Object.Destroy(sameA); // 실제 시험용 포탑 사본 제거
            yield return null; // 실제 사본 제거 대기
            AutomatedFocusedRewardVerification.Finish(true, "Actual 48 unique IDs, first-card focus, distinct cards and grade-five ID exclusion passed."); // 실제 전체 시험 성공
        }

        private static bool Unique(List<WeaponRewardCandidate> candidates) // 실제 후보 ID 중복 확인
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal); // 실제 후보 식별자 집합
            foreach (WeaponRewardCandidate candidate in candidates) // 실제 후보 순회
            {
                if (candidate.Weapon == null || !ids.Add(candidate.Weapon.Id)) // 실제 중복 또는 누락 확인
                {
                    return false; // 실제 중복 후보 반환
                }
            }
            return true; // 실제 모든 후보 고유 확인
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedFocusedRewardVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
