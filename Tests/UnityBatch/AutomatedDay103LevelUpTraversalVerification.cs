using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험 속도와 실제 버튼 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어와 체력
using ProjectEpsilon.Progression; // 실제 경험치와 아이템
using ProjectEpsilon.UI; // 실제 레벨업 화면
using ProjectEpsilon.World; // 실제 맵 관리자
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능
using UnityEngine.UI; // 실제 보상 버튼

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay103LevelUpTraversalVerification // 레벨업 중 같은 이동 획득 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay103LevelUpTraversal"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay103LevelUpTraversalVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 실제 전투 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("Day103LevelUpTraversalRunner").AddComponent<Day103LevelUpTraversalRunner>(); // 실제 이동 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day103LevelUpTraversalVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day103LevelUpTraversalRunner : MonoBehaviour // 실제 레벨업 획득 시험 실행기
    { // 클래스 시작
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 비공개 시험 접근
        private IEnumerator Start() // 실제 자동 전진과 레벨업 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeExperience experience = player == null ? null : player.GetComponent<SnakeExperience>(); // 실제 경험치 수신자
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            Require(map != null && player != null && body != null && experience != null && health != null, "실제 플레이어·경험치 구성 누락"); // 실제 시험 구성 확인
            map.enabled = false; // 격리 시험 중 청크 이동 중지
            player.enabled = false; // 시험 준비 중 자동 이동 중지
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 시험 중 몸통 이동 중지
            } // 조건 종료
            SnakeSelfCollision self = player.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            if (self != null) // 실제 자기 충돌 처리기 확인
            { // 조건 시작
                self.enabled = false; // 격리 위치의 시험 간섭 방지
            } // 조건 종료
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            experience.Configure(10, 1f); // 실제 시험 레벨업 요구량
            experience.ResetProgression(); // 실제 경험치 초기화
            experience.AddExperience(9); // 실제 첫 레벨 직전 경험치
            Require(!experience.IsLevelUpPending && experience.CurrentExperience == 9, "레벨업 직전 경험치 구성 실패"); // 실제 시작 경험치 확인
            Require(health.TakeDamage(10), "실제 회복 필요 체력 구성 실패"); // 실제 공용 체력 피해
            int damagedHealth = health.CurrentHealth; // 실제 회복 전 체력
            Time.captureFramerate = 20; // 재현 가능한 시험 프레임 간격
            typeof(SnakeMovement).GetField("moveSpeed", Private).SetValue(player, 200f); // 한 프레임 통과용 시험 속도
            player.transform.rotation = Quaternion.Euler(0f, 0f, -90f); // 실제 오른쪽 자동 전진 방향
            yield return null; // 실제 시험 프레임 간격 적용 대기
            float step = player.CurrentMoveSpeed * Time.deltaTime; // 실제 한 프레임 자동 전진 거리
            Require(step > 6f, "다중 아이템 통과 시험 속도 부족"); // 실제 통과 시험 조건 확인
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            player.transform.position = center + Vector3.left * (step * 0.5f); // 실제 아이템 앞 시작 위치
            ExperiencePickup first = ExperiencePickup.Spawn(center + Vector3.left * 0.5f, 1, null, experience); // 실제 첫 경험치 보석
            ExperiencePickup second = ExperiencePickup.Spawn(center + Vector3.right * 0.5f, 1, null, experience); // 실제 둘째 경험치 보석
            RecoveryPickup heal = RecoveryPickup.Spawn(center, player.transform, health, body, RecoveryPickupType.Heal, 10, null); // 실제 같은 경로의 회복 아이템
            yield return null; // 실제 첫 아이템 상태 갱신
            player.enabled = true; // 실제 자동 전진 시작
            yield return null; // 실제 한 프레임 이동과 레벨업 대기
            player.enabled = false; // 시험 이동 종료
            yield return null; // 실제 아이템 제거 대기
            Require(GameManager.Instance.IsPaused && experience.IsLevelUpPending && experience.CurrentLevel == 2, "실제 첫 보석 레벨업 정지 실패"); // 실제 레벨업 정지 확인
            Require(first == null && second == null && experience.CurrentExperience == 1 && heal == null && health.CurrentHealth > damagedHealth, "레벨업 정지가 같은 이동 경로의 아이템 획득을 누락"); // 실제 같은 이동의 모든 획득 확인
            ExperiencePickup pausedGem = ExperiencePickup.Spawn(player.transform.position, 1, null, experience); // 정지 중 실제 머리 위치 보석
            yield return null; // 실제 정지 프레임 진행
            yield return null; // 실제 정지 보석 갱신 대기
            Require(pausedGem != null && experience.CurrentExperience == 1, "레벨업 정지 후 새 보석 획득"); // 이후 정지 프레임의 획득 차단 확인
            SnakeLevelUpController level = FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업 제어기
            LevelUpPanelController panel = FindFirstObjectByType<LevelUpPanelController>(); // 실제 보상 화면
            Require(level != null && panel != null, "실제 레벨업 화면 누락"); // 실제 보상 구성 확인
            for (int attempts = 0; attempts < 4 && level.IsPresentingLevelUp; attempts++) // 실제 보상 단계 순회
            { // 반복 시작
                bool replacing = (bool)typeof(SnakeLevelUpController).GetField("selectingReplacement", Private).GetValue(level); // 실제 교체 단계 확인
                string field = replacing ? "candidateButton02" : "candidateButton01"; // 실제 확정 버튼 이름
                Button button = typeof(LevelUpPanelController).GetField(field, Private).GetValue(panel) as Button; // 실제 보상 버튼
                Require(button != null && button.gameObject.activeInHierarchy && button.interactable, "실제 보상 선택 버튼 사용 불가"); // 실제 버튼 상태 확인
                button.onClick.Invoke(); // 실제 보상 선택
            } // 반복 종료
            Require(!experience.IsLevelUpPending && GameManager.Instance.IsPlaying, "실제 보상 선택 후 전투 재개 실패"); // 실제 재개 확인
            GameManager.Instance.PauseGame(); // 실제 일반 일시정지
            yield return null; // 실제 일반 정지 프레임 진행
            Require(pausedGem != null && experience.CurrentExperience == 1, "일반 일시정지 중 새 보석 획득"); // 일반 정지 획득 차단 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            Time.captureFramerate = 0; // 기존 프레임 시간 복원
            AutomatedDay103LevelUpTraversalVerification.Finish(true, "Same-movement pickups, level-up pause and ordinary pause passed."); // 실제 시험 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay103LevelUpTraversalVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
