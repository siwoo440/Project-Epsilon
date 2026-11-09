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
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDay46ZonesVerification // 이동 중 경험치 흡수 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay46ZonesVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay46ZonesVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 진행 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 감시 연결
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 시험 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 씬 열기
            SessionState.SetBool(Key, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 감시 제거
            EditorApplication.update += Tick; // 플레이 감시 연결
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 메서드 끝
        private static void Tick() // 실제 초기화 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 대기
            } // 조건 끝
            if (started <= 0d) // 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 저장
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 시험 제한 시간 확인
            { // 조건 시작
                Finish(1, "Timed out."); // 시간 초과 실패
                return; // 시험 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행 여부 확인
            { // 조건 시작
                return; // 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행 중복 방지
            player.gameObject.AddComponent<Day46ZonesVerificationRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day46ZonesVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day46ZonesVerificationRunner : MonoBehaviour // 실제 특수 지역 검증
    { // 클래스 시작
        private IEnumerator Start() // 실제 효과와 정지 시험
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 머리 이동
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            EarlyEnemyWaveSpawner wave = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Component map = UnityEngine.Object.FindFirstObjectByType(typeof(ProjectEpsilon.World.PrototypeChunkMap)) as Component; // 실제 청크 맵
            Type zoneType = typeof(SnakeMovement).Assembly.GetType("ProjectEpsilon.World.PrototypeSpecialZone"); // 실제 특수 지역 형식
            Type kindType = typeof(SnakeMovement).Assembly.GetType("ProjectEpsilon.World.PrototypeZoneKind"); // 실제 지역 종류 형식
            Type selectionType = typeof(SnakeMovement).Assembly.GetType("ProjectEpsilon.World.PrototypeMapSelection"); // 실제 맵 선택 형식
            Component selection = selectionType == null ? null : UnityEngine.Object.FindFirstObjectByType(selectionType) as Component; // 실제 맵 선택기
            Require(player != null && experience != null && health != null && wave != null && map != null && zoneType != null && kindType != null && selection != null, "Actual special zones or scene fixtures missing."); // 실제 제품 계약 확인
            MethodInfo create = zoneType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static); // 실제 지역 생성 경로
            object profile = selectionType.GetProperty("SelectedProfile").GetValue(selection); // 실제 선택 맵 데이터
            Require(create != null && profile != null, "Actual special zone configuration missing."); // 설정 연결 확인
            player.enabled = false; // 실제 이동 통제
            ((Behaviour)map).enabled = false; // 통제 중 자연 청크 변경 방지
            wave.enabled = false; // 실제 적 출현 중지
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 전투 간섭 차단
            } // 반복 끝
            player.transform.position = new Vector3(1000f, 1000f, 0f); // 기존 지역 밖 통제 위치
            experience.Configure(10000, 1f); // 시험 중 레벨업 차단
            experience.ResetProgression(); // 경험치 초기화
            float baseSpeed = player.CurrentMoveSpeed; // 실제 기본 속도
            Component slow = (Component)create.Invoke(null, new object[] { null, player.transform.position, Enum.Parse(kindType, "Slow"), profile }); // 실제 감속 지역 생성
            Require(slow != null && player.CurrentMoveSpeed < baseSpeed, "Slow zone did not affect actual movement speed."); // 실제 감속 확인
            slow.gameObject.SetActive(false); // 실제 감속 비활성
            Require(Mathf.Approximately(player.CurrentMoveSpeed, baseSpeed), "Slow zone effect remained after removal."); // 감속 제거 확인
            Component xp = (Component)create.Invoke(null, new object[] { null, player.transform.position, Enum.Parse(kindType, "Experience"), profile }); // 실제 경험치 지역 생성
            ExperiencePickup.Spawn(player.transform.position, 1, null, experience); // 실제 첫 소형 보석 생성
            yield return null; // 실제 첫 보석 수집 대기
            yield return null; // 실제 경험치 갱신 대기
            Require(experience.CurrentExperience == 1, "Fractional XP bonus was rounded up prematurely."); // 첫 경험치의 소수 보너스 보존 확인
            ExperiencePickup.Spawn(player.transform.position, 1, null, experience); // 실제 두 번째 소형 보석 생성
            yield return null; // 실제 두 번째 보석 수집 대기
            yield return null; // 실제 경험치 갱신 대기
            Require(experience.CurrentExperience == 3, "Fractional XP bonus did not accumulate to the configured multiplier."); // 두 경험치의 실제 배율 확인
            experience.ResetProgression(); // 실제 성장 초기화
            experience.AddExperience(1); // 초기화 후 첫 경험치
            experience.ResetProgression(); // 실제 보너스 소수 재초기화
            experience.AddExperience(1); // 다시 초기화한 첫 경험치
            Require(experience.CurrentExperience == 1, "Reset progression retained a prior fractional XP bonus."); // 초기화 후 이전 소수 제거 확인
            xp.gameObject.SetActive(false); // 경험치 지역 비활성
            int previousXp = experience.CurrentExperience; // 기존 경험치 기록
            experience.AddExperience(2); // 지역 밖 경험치 획득
            Require(experience.CurrentExperience == previousXp + 2, "XP bonus remained after zone removal."); // 지역 밖 기존 수량 확인
            health.ResetHealth(); // 실제 체력 초기화
            Component hazard = (Component)create.Invoke(null, new object[] { null, player.transform.position, Enum.Parse(kindType, "Danger"), profile }); // 실제 위험 지역 생성
            GameManager.Instance.PauseGame(); // 실제 게임 정지
            int pausedHealth = health.CurrentHealth; // 정지 중 실제 체력
            yield return new WaitForSecondsRealtime(2.3f); // 실제 피해 주기 대기
            Require(health.CurrentHealth == pausedHealth, "Danger zone damaged player while paused."); // 정지 중 피해 차단
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(2.3f); // 실제 피해 주기 대기
            Require(health.CurrentHealth < pausedHealth, "Danger zone did not damage the player."); // 실제 위험 피해 확인
            hazard.gameObject.SetActive(false); // 실제 위험 제거
            int injured = health.CurrentHealth; // 실제 피해 후 체력
            Component healing = (Component)create.Invoke(null, new object[] { null, player.transform.position, Enum.Parse(kindType, "Healing"), profile }); // 실제 회복 지역 생성
            yield return new WaitForSeconds(2.3f); // 실제 회복 주기 대기
            Require(health.CurrentHealth > injured, "Healing zone did not restore shared HP."); // 실제 회복 확인
            healing.gameObject.SetActive(false); // 시험 지역 비활성
            AutomatedDay46ZonesVerification.Finish(0, "Actual slow, experience, danger, healing, removal and pause passed."); // 실제 시험 성공
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay46ZonesVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
