using System; // 실제 예외 형식
using System.Collections; // 실제 게임 프레임 대기
using System.Reflection; // 실제 기록 필드 조회
using ProjectEpsilon.Combat; // 실제 포탑 제어
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using ProjectEpsilon.Progression; // 실제 보석과 성장
using ProjectEpsilon.Recording; // 실제 선택적 기록
using UnityEditor; // Unity 배치 제어
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 플레이 진입 후 감시 복구
    public static class AutomatedGemAccountingVerification // 실제 보석 생성·병합·수집 기록 검증
    {
        private const string ActiveKey = "EpsilonGemAccountingVerification"; // 실제 시험 진행 표식
        private static bool injected; // 실제 실행기 생성 여부
        private static double started; // 실제 시작 시각

        static AutomatedGemAccountingVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 실제 시험 진행 상태 확인
            {
                EditorApplication.update += Observe; // 실제 전투 시작 관찰
            }
        }

        public static void Run() // 실제 게임 씬 시험 시작
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 실제 시험 진행 기록
            EditorApplication.update -= Observe; // 중복 관찰 해제
            EditorApplication.update += Observe; // 실제 시작 관찰 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }

        private static void Observe() // 실제 전투와 기록 초기화 확인
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
                Finish(false, "Timed out waiting for recording."); // 시간 초과 기록
                return; // 추가 처리 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작 선택과 실행 상태 확인
            {
                return; // 실제 전투와 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || RunTelemetry.Instance == null || RunTelemetry.Instance.Current == null || Time.time < 0.35f) // 실제 전투와 기록 준비 확인
            {
                return; // 실제 구성 초기화 대기
            }
            injected = true; // 실행기 중복 생성 차단
            player.gameObject.AddComponent<GemAccountingVerificationRunner>(); // 실제 보석 기록 시험 시작
        }

        public static void Finish(bool passed, string message) // 실제 시험 결과 기록과 종료
        {
            SessionState.SetBool(ActiveKey, false); // 실제 시험 표식 제거
            EditorApplication.update -= Observe; // 실제 관찰 연결 해제
            Debug.Log("[GemAccountingVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 실제 종료 코드 반환
        }
    }

    public sealed class GemAccountingVerificationRunner : MonoBehaviour // 실제 보석 기록 시험 실행기
    {
        private IEnumerator Start() // 실제 보석 생성·병합·수집 검증
        {
            RunTelemetry telemetry = RunTelemetry.Instance; // 실제 전투 기록기
            RunRecording recording = telemetry.Current; // 실제 전투 기록
            FieldInfo version = typeof(RunRecording).GetField("gemAccountingVersion"); // 실제 보석 집계 버전
            FieldInfo dropped = typeof(RunSnapshot).GetField("droppedGemExperience"); // 실제 생성 보석 경험치
            FieldInfo collected = typeof(RunSnapshot).GetField("collectedGemExperience"); // 실제 수집 보석 원본 경험치
            Require(version != null && dropped != null && collected != null && (int)version.GetValue(recording) == 1, "Actual gem accounting fields are missing."); // 실제 기록 형식 확인
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치 관리자
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            ExperienceGemMergeController merger = UnityEngine.Object.FindFirstObjectByType<ExperienceGemMergeController>(); // 실제 보석 병합기
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            Require(experience != null && spawner != null && merger != null && weapons != null, "Actual gem fixtures are missing."); // 실제 필수 구성 확인
            GetComponent<SnakeMovement>().enabled = false; // 실제 머리 이동 통제
            spawner.enabled = false; // 시험 중 실제 적 생성 차단
            weapons.enabled = false; // 시험 중 실제 자동 공격 차단
            Require(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 0, "Unexpected preexisting gems."); // 실제 이전 보석 부재 확인
            typeof(ExperienceGemMergeController).GetField("mergeThreshold", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(merger, 1); // 실제 병합 발동 개수 통제
            telemetry.Capture("gem_before"); // 실제 시험 전 집계 표본
            RunSnapshot before = recording.snapshots[recording.snapshots.Count - 1]; // 실제 이전 표본
            long startDropped = (long)dropped.GetValue(before); // 실제 기존 생성 경험치
            long startCollected = (long)collected.GetValue(before); // 실제 기존 수집 경험치
            Vector3 distant = transform.position + Vector3.right * 100f; // 실제 흡수 범위 밖 위치
            for (int index = 0; index < 5; index++) // 실제 소형 보석 다섯 개 생성
            {
                ExperiencePickup.Spawn(distant + Vector3.right * (index * 0.1f), 1, null, experience); // 실제 소형 보석 생성
            }
            ExperiencePickup large = ExperiencePickup.Spawn(distant + Vector3.right * 5f, 20, null, experience); // 실제 별도 대형 보석 생성
            Require(merger.ProcessMergePass() == 1, "Actual five-gem merge did not occur."); // 실제 자동 병합 확인
            telemetry.Capture("gem_after_merge"); // 실제 병합 이후 기록 표본
            RunSnapshot afterMerge = recording.snapshots[recording.snapshots.Count - 1]; // 실제 병합 후 표본
            Require((long)dropped.GetValue(afterMerge) - startDropped == 25 && (long)collected.GetValue(afterMerge) - startCollected == 0, "Merge duplicated or consumed actual gem accounting."); // 실제 병합 중 중복 집계 방지
            ExperiencePickup merged = null; // 실제 병합 결과 초기화
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 실제 활성 보석 순회
            {
                if (pickup != null && pickup.ExperienceValue == 5 && pickup.gameObject.activeInHierarchy) // 실제 중형 결과 확인
                {
                    merged = pickup; // 실제 병합 결과 저장
                }
            }
            Require(merged != null && large != null, "Actual merged or uncollected gem is missing."); // 실제 보석 결과 확인
            merged.transform.position = transform.position; // 실제 수집 위치로 이동
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 보석 수집 제한
            while (merged != null && Time.realtimeSinceStartup < deadline) // 실제 보석 수집 대기
            {
                yield return null; // 실제 보석 갱신 진행
            }
            telemetry.Capture("gem_after_collect"); // 실제 수집 이후 기록 표본
            RunSnapshot afterCollect = recording.snapshots[recording.snapshots.Count - 1]; // 실제 수집 후 표본
            Require(merged == null && large != null && (long)dropped.GetValue(afterCollect) - startDropped == 25 && (long)collected.GetValue(afterCollect) - startCollected == 5 && afterCollect.totalExperience >= 5, "Actual collected gem value or remaining gem accounting differs."); // 실제 수집량과 미수집 보석 확인
            int beforeLarge = recording.snapshots.Count; // 실제 대형 보석 이전 표본 수
            large.transform.position = transform.position; // 실제 대형 보석 수집 위치
            deadline = Time.realtimeSinceStartup + 5f; // 실제 추가 수집 제한
            while (large != null && Time.realtimeSinceStartup < deadline) // 실제 대형 보석 수집 대기
            {
                yield return null; // 실제 보석과 레벨업 갱신
            }
            telemetry.Capture("gem_after_level"); // 실제 레벨업 이후 기록 표본
            RunSnapshot afterLevel = recording.snapshots[recording.snapshots.Count - 1]; // 실제 레벨업 후 표본
            Require(large == null && experience.CurrentLevel >= 2 && experience.IsLevelUpPending && (long)dropped.GetValue(afterLevel) - startDropped == 25 && (long)collected.GetValue(afterLevel) - startCollected == 25 && afterLevel.totalExperience >= 25, "Actual level-up collection accounting differs."); // 실제 레벨업과 전체 수집량 확인
            for (int index = beforeLarge; index < recording.snapshots.Count; index++) // 실제 레벨업 중 생성된 표본 순회
            {
                RunSnapshot sample = recording.snapshots[index]; // 실제 현재 기록 표본
                Require((long)collected.GetValue(sample) - startCollected == 25, "A level-up snapshot missed the collected gem."); // 동기식 레벨업 표본의 원본 수집량 확인
            }
            AutomatedGemAccountingVerification.Finish(true, "Actual 25 dropped XP, five-gem merge, partial collection and level-up collection passed."); // 실제 전체 시험 성공
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedGemAccountingVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
