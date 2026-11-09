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
    public static class AutomatedDay45PickupVerification // 이동 중 경험치 흡수 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay45PickupVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static double started; // 시험 시작 시각
        static AutomatedDay45PickupVerification() // 도메인 재시작 처리
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
            player.gameObject.AddComponent<Day45PickupVerificationRunner>(); // 실제 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 시험 종료
        { // 메서드 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 플레이 감시 제거
            Debug.Log($"[Day45PickupVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 시험 결과 기록
            EditorApplication.Exit(code); // 실제 종료 코드
        } // 메서드 끝
    } // 클래스 끝
    public sealed class Day45PickupVerificationRunner : MonoBehaviour // 실제 경험치 흡수 검증
    { // 클래스 시작
        private IEnumerator Start() // 실제 이동과 흡수 시험
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동기
            SnakeExperience experience = GetComponent<SnakeExperience>(); // 실제 경험치 수신기
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(player != null && experience != null && spawner != null, "Pickup fixtures missing."); // 실제 시험 구성 확인
            player.enabled = false; // 시험용 머리 위치 직접 조절
            player.transform.position = new Vector3(1000f, 1000f, 0f); // 기존 장애물에서 떨어진 시험 위치
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 몸통 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 통제 중 몸통 추적 방지
            } // 조건 끝
            spawner.enabled = false; // 자연 적 출현 중지
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌기
            if (self != null) // 자기 충돌 구성 확인
            { // 조건 시작
                self.enabled = false; // 통제 이동의 자기 충돌 차단
            } // 조건 끝
            ExperiencePickup gem = ExperiencePickup.Spawn(player.transform.position - Vector3.up * 2.7f, 1, null, experience); // 실제 흡수 범위 밖 보석
            Vector3 outside = gem.transform.position; // 실제 보석 초기 위치
            int initialExperience = experience.CurrentExperience; // 실제 초기 경험치
            yield return new WaitForSeconds(0.15f); // 실제 범위 밖 대기
            Require(gem != null && Vector3.Distance(gem.transform.position, outside) < 0.001f && experience.CurrentExperience == initialExperience, "Gem moved or collected outside the attraction radius."); // 범위 밖 보석 보존 확인
            gem.transform.position = player.transform.position - Vector3.up * 2.4f; // 실제 흡수 범위 안 배치
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            Vector3 pausedPosition = gem.transform.position; // 정지 중 실제 보석 위치
            yield return new WaitForSecondsRealtime(0.15f); // 실제 정지 시간 경과
            Require(gem != null && Vector3.Distance(gem.transform.position, pausedPosition) < 0.001f && experience.CurrentExperience == initialExperience, "Gem advanced while paused."); // 실제 정지 중 보석 유지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            float deadline = Time.time + 3f; // 실제 이동 중 흡수 제한 시간
            while (gem != null && Time.time < deadline) // 실제 보석 획득 대기
            { // 반복 시작
                player.transform.position += Vector3.up * (4.5f * Time.deltaTime); // 가속 상태와 같은 속도로 머리 이동
                yield return null; // 실제 보석 갱신 대기
            } // 반복 끝
            Require(gem == null && experience.CurrentExperience == initialExperience + 1, "Gem did not reach the moving head or grant exactly one XP."); // 이동하는 머리의 실제 단일 획득 확인
            AutomatedDay45PickupVerification.Finish(0, "Real gem stayed outside range, paused safely and reached a moving head for one XP."); // 전체 성공 기록
        } // 코루틴 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay45PickupVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
