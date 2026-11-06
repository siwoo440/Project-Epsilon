using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 시험 설정 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 출현 제어
using ProjectEpsilon.Player; // 플레이어 이동 제어
using ProjectEpsilon.Progression; // 실제 경험치와 보석
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 UI 버튼과 문구
using ProjectEpsilon.UI; // 실제 화면 구성

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 전환 연결
    public static class AutomatedBodyGrowthVerification // 실제 몸통 성장 통제 시험
    { // 클래스 시작
        private const string ActiveKey = "EpsilonBodyGrowthVerification"; // 배치 진행 표식
        private static double started; // 실시간 시작 시각
        private static bool injected; // 시험 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedBodyGrowthVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 진입 감시
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 저장
            EditorApplication.update -= Tick; // 중복 구독 방지
            EditorApplication.update += Tick; // 플레이 진입 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 시작 선택 및 제한 시간 확인
        { // 메서드 시작
            if (started <= 0d) // 실시간 시작 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for the actual Game scene test."); // 제한 시간 실패
                return; // 후속 처리 중지
            } // 조건 끝
            if (!EditorApplication.isPlaying || StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 버튼 처리
            { // 조건 시작
                return; // 게임 시작 대기
            } // 조건 끝
            if (injected || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 또는 결과 대기
            } // 조건 끝
            SnakeExperience experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 실제 경험치 관리자 조회
            if (experience == null) // 경험치 관리자 존재 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            experience.gameObject.AddComponent<BodyGrowthVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 결과와 종료 코드 기록
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 종료 방지
            } // 조건 끝
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log($"[BodyGrowthVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 결과 기록
            EditorApplication.Exit(code); // Unity 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class BodyGrowthVerificationRunner : MonoBehaviour // 실제 보석과 성장 UI 시험
    { // 클래스 시작
        private static readonly BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 화면 필드 조회
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeHealth health; // 실제 공용 체력
        private SnakeExperience experience; // 실제 경험치 관리자
        private SnakeLevelUpController level; // 실제 레벨업 처리기
        private LevelUpPanelController panel; // 실제 레벨업 화면
        private HUDController hud; // 실제 몸통 HUD

        private IEnumerator Start() // 실제 씬 통제 시험 순서
        { // 코루틴 시작
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            health = UnityEngine.Object.FindFirstObjectByType<SnakeHealth>(); // 실제 체력 조회
            experience = GetComponent<SnakeExperience>(); // 실제 경험치 조회
            level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업 조회
            panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 보상 패널 조회
            hud = UnityEngine.Object.FindFirstObjectByType<HUDController>(); // 실제 HUD 조회
            SnakeMovement movement = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 이동 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            Require(body != null && health != null && experience != null && level != null && panel != null && hud != null && movement != null && spawner != null, "Actual Game scene fixtures are missing."); // 필수 씬 구성 확인
            movement.enabled = false; // 시험 중 플레이어 이동 정지
            spawner.enabled = false; // 시험 중 자연 적 출현 정지
            SnakeSelfCollision self = movement.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            if (self != null) // 자기 충돌 존재 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 몸통 접촉 방지
            } // 조건 끝
            SnakeExternalCollision external = movement.GetComponent<SnakeExternalCollision>(); // 외부 충돌 조회
            if (external != null) // 외부 충돌 존재 확인
            { // 조건 시작
                external.enabled = false; // 시험 중 외부 피해 방지
            } // 조건 끝
            experience.Configure(10, 1f); // 일정한 임시 레벨업 기준
            experience.ResetProgression(); // 시험 레벨과 경험치 초기화
            Require(experience.RequiredExperience == 10, "Controlled experience requirement is not ten."); // 임시 요구량 확인
            Debug.Log("[BodyGrowthVerification] controlled experience requirement: base=10, multiplier=1.0, each level requires 10; three consecutive levels require 30."); // 임시 시험값 기록
            Require(body.MaximumBodyCount == 20 && body.CurrentBodyCount == 3 && body.EarnedBodyCount == 3 && health.CurrentHealth == health.MaximumHealth, "Unexpected starting body or health fixture."); // 실제 시작 상태 확인
            while (body.EarnedBodyCount < 6) // 손실 전 성장 상태 구성
            { // 반복 시작
                Require(body.TryGainBodyFromLevelUp(), "Could not prepare six earned bodies."); // 실제 몸통 성장 메서드
            } // 반복 끝
            Require(health.TakeDamage(health.MaximumHealth * 2 + 23), "Could not prepare controlled health and body loss."); // 실제 초과 피해 적용
            Require(body.CurrentBodyCount == 4 && body.EarnedBodyCount == 6 && health.CurrentHealth == health.MaximumHealth - 23, "Controlled loss did not preserve earned maximum."); // 현재 및 성장 최대 구분
            LogState("before lost-body level"); // 실제 수치와 HUD 기록
            yield return CollectExperience(10); // 실제 보석 수집과 레벨업
            RequireGrowth(2, 5, 7, health.MaximumHealth - 23); // 손실 뒤 현재와 성장 최대 동시 증가
            LogState("lost-body level"); // 실제 성장 수치와 UI 기록
            CompleteReward(); // 실제 후보 버튼으로 첫 보상 완료
            Require(!level.IsPresentingLevelUp && !experience.IsLevelUpPending && GameManager.Instance.IsPlaying, "First reward did not resume the game."); // 보상 완료와 재개 확인
            while (body.EarnedBodyCount < 20) // 절대 상한 근처 상태 구성
            { // 반복 시작
                Require(body.TryGainBodyFromLevelUp(), "Could not prepare earned maximum twenty."); // 실제 몸통 성장 메서드
            } // 반복 끝
            Require(body.CurrentBodyCount == 18 && body.TryRepairBody() && body.CurrentBodyCount == 19 && body.EarnedBodyCount == 20, "Could not prepare nineteen of twenty bodies."); // 상한 손실 상태 구성
            yield return CollectExperience(10); // 실제 보석 수집과 레벨업
            RequireGrowth(3, 20, 20, health.MaximumHealth - 23); // 현재 몸통만 복구 및 체력 보존
            LogState("repair to twenty"); // 복구 수치와 UI 기록
            CompleteReward(); // 실제 후보 버튼으로 둘째 보상 완료
            yield return CollectExperience(10); // 실제 보석 수집과 레벨업
            RequireGrowth(4, 20, 20, health.MaximumHealth); // 양쪽 최대 시 공용 체력 전회복
            LogState("full-body heal"); // 전회복 수치와 UI 기록
            CompleteReward(); // 실제 후보 버튼으로 셋째 보상 완료
            Require(health.TakeDamage(health.MaximumHealth + 30), "Could not prepare consecutive-level loss."); // 실제 초과 피해 적용
            Require(body.CurrentBodyCount == 19 && body.EarnedBodyCount == 20 && health.CurrentHealth == health.MaximumHealth - 30, "Consecutive-level fixture mismatch."); // 연속 레벨업 시작 상태
            yield return CollectExperience(30); // 실제 보석 한 개로 세 레벨 경험치
            RequireGrowth(5, 20, 20, health.MaximumHealth - 30); // 첫 연속 레벨의 몸통만 복구
            Require(experience.CurrentExperience == 20, "First pending reward did not retain excess experience."); // 잔여 경험치 확인
            LogState("consecutive level one"); // 첫 보상 수치 기록
            CompleteReward(); // 실제 후보 버튼으로 첫 보상 완료
            RequireGrowth(6, 20, 20, health.MaximumHealth); // 둘째 연속 레벨 체력 회복
            Require(experience.CurrentExperience == 10, "Second pending reward did not retain experience."); // 둘째 잔여 경험치 확인
            LogState("consecutive level two"); // 둘째 보상 수치 기록
            CompleteReward(); // 실제 후보 버튼으로 둘째 보상 완료
            RequireGrowth(7, 20, 20, health.MaximumHealth); // 셋째 연속 레벨 상태
            Require(experience.CurrentExperience == 0, "Third pending reward did not consume exact experience."); // 마지막 잔여 경험치 확인
            LogState("consecutive level three"); // 셋째 보상 수치 기록
            CompleteReward(); // 실제 후보 버튼으로 마지막 보상 완료
            Require(!experience.IsLevelUpPending && !level.IsPresentingLevelUp && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "Consecutive rewards did not finish and resume exactly once."); // 최종 대기 해제와 재개 확인
            AutomatedBodyGrowthVerification.Finish(0, "Actual gem collection, reward UI and consecutive levels preserved body and health rules under controlled conditions."); // 전체 시험 성공
        } // 코루틴 끝

        private IEnumerator CollectExperience(int amount) // 실제 보석 수집 경로
        { // 코루틴 시작
            Require(GameManager.Instance.IsPlaying && !experience.IsLevelUpPending, "Game was not ready to collect experience."); // 수집 전 게임 상태
            ExperiencePickup.Spawn(experience.transform.position, amount, null, experience); // 실제 플레이어 위치에 보석 생성
            float deadline = Time.realtimeSinceStartup + 3f; // 실제 수집 제한
            while (!experience.IsLevelUpPending && Time.realtimeSinceStartup < deadline) // 실제 수집 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            Require(experience.IsLevelUpPending, "Actual gem did not trigger a level-up."); // 실제 보석 레벨업 확인
        } // 코루틴 끝

        private void RequireGrowth(int expectedLevel, int current, int earned, int hp) // 실제 레벨업 결과 확인
        { // 메서드 시작
            Require(experience.CurrentLevel == expectedLevel && experience.RequiredExperience == 10 && body.CurrentBodyCount == current && body.EarnedBodyCount == earned && body.MaximumBodyCount == 20 && health.CurrentHealth == hp, "Actual level-up body, health or controlled XP requirement mismatch."); // 세 몸통 수와 체력 확인
            Require(experience.IsLevelUpPending && level.IsPresentingLevelUp && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && panel.gameObject.activeInHierarchy && level.CurrentCandidates.Count > 0, "Actual level-up reward UI did not pause and present candidates."); // 실제 보상 UI와 정지 확인
            string expectedHud = HUDTextFormatter.FormatBodyCount(current, earned, 20); // 세 몸통 수 표시 기준
            Require(ReadText(hud, "bodyCountText") == expectedHud, "Actual body HUD did not display current and both maxima."); // 실제 HUD 문구 확인
        } // 메서드 끝

        private void CompleteReward() // 실제 UI 버튼으로 한 보상만 완료
        { // 메서드 시작
            int selectedLevel = experience.CurrentLevel; // 현재 보상 레벨
            for (int attempts = 0; attempts < 4 && level.IsPresentingLevelUp && experience.CurrentLevel == selectedLevel; attempts++) // 현재 보상 화면 선택
            { // 반복 시작
                bool replacing = ReadFlag("selectingReplacement"); // 교체 화면 여부
                string field = replacing ? "candidateButton02" : "candidateButton01"; // 현재 화면의 실제 확정 버튼
                Button button = typeof(LevelUpPanelController).GetField(field, Fields)?.GetValue(panel) as Button; // 실제 직렬화 버튼 조회
                Require(button != null && button.gameObject.activeInHierarchy && button.interactable, "Actual reward choice button is unavailable."); // 실제 버튼 상태 확인
                button.onClick.Invoke(); // 실제 UI 선택 이벤트
            } // 반복 끝
            Require(!level.IsPresentingLevelUp || experience.CurrentLevel != selectedLevel, "Actual UI button did not finish this level reward."); // 현재 보상 한 개 완료 확인
        } // 메서드 끝

        private bool ReadFlag(string field) // 실제 보상 선택 단계 조회
        { // 메서드 시작
            FieldInfo info = typeof(SnakeLevelUpController).GetField(field, Fields); // 실제 단계 필드 조회
            Require(info != null, "Level-up selection state field is missing."); // 필드 존재 확인
            return (bool)info.GetValue(level); // 실제 선택 단계 반환
        } // 메서드 끝

        private void LogState(string stage) // 실제 수치와 UI 문구 기록
        { // 메서드 시작
            string hudText = ReadText(hud, "bodyCountText"); // 실제 몸통 HUD 문구
            string growthText = panel.gameObject.activeInHierarchy ? ReadText(panel, "growthText") : "hidden"; // 실제 레벨업 문구
            Debug.Log($"[BodyGrowthVerification] {stage}: level={experience.CurrentLevel}, current={body.CurrentBodyCount}, earned={body.EarnedBodyCount}, absolute={body.MaximumBodyCount}, hp={health.CurrentHealth}/{health.MaximumHealth}, xp={experience.CurrentExperience}, HUD='{hudText}', panel='{growthText}'."); // 실제 상태 기록
        } // 메서드 끝

        private static string ReadText(object owner, string field) // 실제 직렬화 텍스트 조회
        { // 메서드 시작
            FieldInfo info = owner.GetType().GetField(field, Fields); // 텍스트 필드 조회
            Text text = info?.GetValue(owner) as Text; // 실제 UI 텍스트
            Require(text != null, "Actual UI text field is missing: " + field); // 실제 UI 연결 확인
            return text.text; // 실제 문구 반환
        } // 메서드 끝

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedBodyGrowthVerification.Finish(1, message); // 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
