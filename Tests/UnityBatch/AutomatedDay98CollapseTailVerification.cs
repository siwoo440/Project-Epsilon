using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험용 예고 시간 설정
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 뱀과 몸통
using ProjectEpsilon.World; // 실제 맵과 붕괴 지역
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay98CollapseTailVerification // 붕괴 잔해 꼬리 보호 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay98CollapseTail"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay98CollapseTailVerification() // 시험 복구
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
            new GameObject("Day98CollapseTailRunner").AddComponent<Day98CollapseTailRunner>(); // 실제 안전 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day98CollapseTailVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day98CollapseTailRunner : MonoBehaviour // 실제 붕괴 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 꼬리와 붕괴 생성 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 머리
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            Require(map != null && map.Profile != null && player != null && body != null && body.TailSegment != null, "실제 맵 또는 꼬리 구성 누락"); // 실제 시험 구성 확인
            map.enabled = false; // 격리 시험 중 청크 이동 중지
            player.enabled = false; // 시험 중 머리 이동 중지
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
            typeof(PrototypeMapProfile).GetField("collapseWarning", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map.Profile, 0.2f); // 시험용 짧은 붕괴 예고
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 지형과 분리된 붕괴 중심
            Vector3 safe = center + Vector3.right * 20f; // 실제 붕괴 구역 밖 위치
            GameObject root = new GameObject("Day98IsolatedChunk"); // 격리된 실제 지역 부모
            PrototypeSpecialZone zone = PrototypeSpecialZone.Create(root.transform, center, PrototypeZoneKind.Collapse, map.Profile); // 실제 붕괴 지역 생성
            player.transform.position = center; // 실제 머리의 지역 진입
            MoveBody(body, safe); // 실제 몸통과 꼬리 이탈
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            yield return new WaitForFixedUpdate(); // 실제 붕괴 예고 시작 대기
            yield return new WaitForFixedUpdate(); // 실제 물리 처리 완료 대기
            Require(zone.IsWarning, "실제 붕괴 예고 시작 실패"); // 실제 예고 확인
            player.transform.position = safe; // 실제 머리 이탈
            body.TailSegment.transform.position = center; // 실제 꼬리만 붕괴 구역에 배치
            Physics2D.SyncTransforms(); // 실제 꼬리 위치 갱신
            yield return new WaitForSeconds(0.35f); // 실제 예고 종료 대기
            Require(zone.transform.Find("TemporaryCollapseObstacle") == null, "꼬리 위에 붕괴 장애물 생성"); // 실제 꼬리 보호 확인
            body.TailSegment.transform.position = safe; // 실제 꼬리 이탈
            player.transform.position = center; // 실제 붕괴 지역 재진입
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            yield return new WaitForFixedUpdate(); // 실제 두 번째 예고 시작 대기
            yield return new WaitForFixedUpdate(); // 실제 물리 처리 완료 대기
            Require(zone.IsWarning, "안전 위치의 두 번째 붕괴 예고 실패"); // 실제 재진입 확인
            player.transform.position = safe; // 실제 머리 이탈
            Physics2D.SyncTransforms(); // 실제 물리 위치 갱신
            yield return new WaitForSeconds(0.35f); // 실제 예고 종료 대기
            Require(zone.transform.Find("TemporaryCollapseObstacle") != null, "안전한 위치의 붕괴 장애물 생성 실패"); // 기존 정상 생성 확인
            AutomatedDay98CollapseTailVerification.Finish(true, "Tail overlap blocked and safe collapse preserved."); // 실제 시험 결과
        } // 함수 종료
        private static void MoveBody(SnakeBodyManager body, Vector3 position) // 실제 몸통과 꼬리 이동
        { // 함수 시작
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = position; // 실제 몸통 안전 위치
            } // 반복 종료
            body.TailSegment.transform.position = position; // 실제 꼬리 안전 위치
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay98CollapseTailVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
