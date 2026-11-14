using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험용 이동 속도 설정
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using ProjectEpsilon.Progression; // 실제 보석과 경험치
using ProjectEpsilon.World; // 실제 맵 관리자
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay101ExperienceTraversalVerification // 보석 통과 획득 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay101ExperienceTraversal"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay101ExperienceTraversalVerification() // 시험 복구
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
            new GameObject("Day101ExperienceTraversalRunner").AddComponent<Day101ExperienceTraversalRunner>(); // 실제 이동 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day101ExperienceTraversalVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day101ExperienceTraversalRunner : MonoBehaviour // 실제 보석 통과 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 자동 전진과 보석 획득 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeExperience experience = player == null ? null : player.GetComponent<SnakeExperience>(); // 실제 경험치 수신자
            Require(map != null && player != null && body != null && experience != null, "실제 맵·플레이어·경험치 구성 누락"); // 실제 시험 구성 확인
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
            foreach (ExperiencePickup pickup in FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 기존 경험치 보석 순회
            { // 반복 시작
                pickup.gameObject.SetActive(false); // 기존 보석의 시험 간섭 방지
            } // 반복 종료
            Time.captureFramerate = 20; // 재현 가능한 시험 프레임 간격
            typeof(SnakeMovement).GetField("moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, 200f); // 한 프레임 통과용 시험 속도
            player.transform.rotation = Quaternion.Euler(0f, 0f, -90f); // 실제 오른쪽 자동 전진 방향
            yield return null; // 실제 시험 프레임 간격 적용 대기
            float step = player.CurrentMoveSpeed * Time.deltaTime; // 실제 한 프레임 자동 전진 거리
            Require(step > 6f, "보석 양쪽 흡수 범위를 넘는 시험 속도 부족"); // 실제 통과 시험 조건 확인
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 보석 위치
            player.transform.position = center + Vector3.left * (step * 0.5f); // 실제 보석 앞 시작 위치
            ExperiencePickup gem = ExperiencePickup.Spawn(center, 1, null, experience); // 실제 경험치 보석
            ExperiencePickup outside = ExperiencePickup.Spawn(center + Vector3.up, 1, null, experience); // 실제 이동 경로 밖 보석
            int gained = 0; // 실제 경험치 알림 횟수
            experience.ExperienceGained += amount => gained++; // 실제 경험치 획득 기록
            yield return null; // 실제 첫 보석 상태 갱신
            Require(gem != null && gained == 0, "자동 전진 전 보석이 먼저 획득됨"); // 실제 시작 조건 확인
            player.enabled = true; // 실제 자동 전진 시작
            yield return null; // 실제 한 프레임 이동과 획득 대기
            player.enabled = false; // 시험 이동 종료
            Require(player.transform.position.x > center.x + 2.6f, "실제 자동 전진이 보석을 통과하지 못함"); // 실제 통과 경로 확인
            yield return null; // 실제 보석 제거와 경험치 처리 대기
            Require(gem == null && outside != null && gained == 1, "자동 전진 경로의 획득 또는 범위 밖 보석 판정 실패"); // 실제 단일 지급과 범위 밖 보석 확인
            Vector3 teleportedCenter = center + Vector3.up * 30f; // 실제 이동 경로와 분리된 보석 위치
            player.transform.position = teleportedCenter + Vector3.left * 5f; // 실제 강제 이동 전 위치
            ExperiencePickup untouched = ExperiencePickup.Spawn(teleportedCenter, 1, null, experience); // 실제 강제 이동 시험 보석
            yield return null; // 실제 보석 갱신 대기
            player.transform.position = teleportedCenter + Vector3.right * 5f; // 자동 전진 없이 실제 위치 변경
            yield return null; // 실제 보석 갱신 대기
            Require(untouched != null && gained == 1, "강제 위치 변경 경로의 보석을 잘못 획득"); // 실제 비이동 경로 제외 확인
            Time.captureFramerate = 0; // 기존 프레임 시간 복원
            AutomatedDay101ExperienceTraversalVerification.Finish(true, "Actual fast forward pickup and teleport exclusion passed."); // 실제 시험 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay101ExperienceTraversalVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
