using System; // 예외 처리
using System.Reflection; // 실제 생성 경로 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 뱀 구성
using ProjectEpsilon.World; // 청크 생성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay84ObstacleVerification // 실제 장애물 겹침 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay84Obstacle"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay84ObstacleVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시작 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 시작 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            try // 실제 장애물 생성 시험
            { // 시험 시작
                Verify(); // 몸통과 꼬리 보호 확인
                Finish(0, "Body and tail clearance and unobstructed placement passed."); // 성공 결과
            } // 시험 종료
            catch (Exception error) // 실패 확인
            { // 실패 시작
                Finish(1, error.ToString()); // 실패 원인
            } // 실패 종료
        } // 함수 종료
        private static void Verify() // 실제 청크 생성 경로 확인
        { // 함수 시작
            PrototypeChunkMap map = UnityEngine.Object.FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 머리
            Require(map != null && map.Profile != null && body != null && body.BodySegments.Count > 0 && body.TailSegment != null && player != null, "시험 구성 누락"); // 실제 구성 확인
            MethodInfo create = typeof(PrototypeChunkMap).GetMethod("CreateObstacle", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 장애물 생성 함수
            Require(create != null, "장애물 생성 경로 누락"); // 실제 경로 확인
            player.transform.position = Vector3.zero; // 머리 안전 위치
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.transform.position = Vector3.zero; // 몸통 초기 위치
            } // 반복 종료
            body.TailSegment.transform.position = Vector3.zero; // 꼬리 초기 위치
            GameObject chunk = new GameObject("Day84ObstacleFixture"); // 독립 시험 청크
            chunk.transform.position = new Vector3(200f, 200f, 0f); // 머리와 떨어진 위치
            float edge = map.Profile.SafeCorridorWidth * 0.5f + 1.2f; // 실제 배치 경계
            float inner = map.ChunkSize * 0.5f - edge; // 실제 사분면 내부 경계
            Require(inner > edge, "시험 배치 공간 부족"); // 실제 생성 가능 여부
            UnityEngine.Random.InitState(84); // 배치 난수 고정
            Vector3 candidate = chunk.transform.position + new Vector3(UnityEngine.Random.Range(edge, inner), UnityEngine.Random.Range(edge, inner), 0f); // 실제 장애물 후보 위치
            body.BodySegments[0].transform.position = candidate; // 후보 위치에 몸통 배치
            UnityEngine.Random.InitState(84); // 동일한 후보 복원
            create.Invoke(map, new object[] { chunk.transform, 0 }); // 실제 장애물 생성
            Require(chunk.GetComponentsInChildren<SnakeContactHazard>().Length == 0, "몸통 위에 장애물 생성"); // 몸통 보호 확인
            body.BodySegments[0].transform.position = Vector3.zero; // 몸통 위치 복원
            body.TailSegment.transform.position = candidate; // 후보 위치에 꼬리 배치
            UnityEngine.Random.InitState(84); // 동일한 후보 복원
            create.Invoke(map, new object[] { chunk.transform, 0 }); // 실제 장애물 생성
            Require(chunk.GetComponentsInChildren<SnakeContactHazard>().Length == 0, "꼬리 위에 장애물 생성"); // 꼬리 보호 확인
            body.TailSegment.transform.position = Vector3.zero; // 꼬리 위치 복원
            UnityEngine.Random.InitState(84); // 동일한 후보 복원
            create.Invoke(map, new object[] { chunk.transform, 0 }); // 빈 위치의 실제 생성
            Require(chunk.GetComponentsInChildren<SnakeContactHazard>().Length == 1, "안전한 위치의 장애물 누락"); // 정상 생성 확인
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실제 실패 전달
            } // 조건 종료
        } // 함수 종료
        private static void Finish(int code, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day84ObstacleVerification] " + (code == 0 ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(code); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
