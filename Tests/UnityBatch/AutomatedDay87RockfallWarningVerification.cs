using System; // 실제 예외 처리
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 시험 수치 설정
using ProjectEpsilon.Combat; // 기존 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태와 카메라
using ProjectEpsilon.Enemies; // 기존 적 출현기
using ProjectEpsilon.Player; // 실제 뱀 이동
using ProjectEpsilon.World; // 실제 맵 위험
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능
using UnityEngine.UI; // 실제 경고 문구

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay87RockfallWarningVerification // 실제 낙석 방향 안내 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay87RockfallWarning"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay87RockfallWarningVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 전투 상태 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool(Key, true); // 시험 상태 기록
            EditorApplication.update += Tick; // 전투 상태 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 함수 종료
        private static void Tick() // 실제 전투 준비 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || StartingWeaponBatchHelper.HandleStartingSelection() || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.5f) // 실제 전투 준비 확인
            { // 조건 시작
                return; // 실제 시작 대기
            } // 조건 종료
            started = true; // 중복 시험 방지
            new GameObject("Day87RockfallWarningRunner").AddComponent<Day87RockfallWarningRunner>(); // 실제 프레임 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day87RockfallWarningVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day87RockfallWarningRunner : MonoBehaviour // 실제 낙석 안내 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 카메라 이동과 경고 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 위험 관리자
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 뱀 머리
            Camera camera = Camera.main; // 실제 전투 카메라
            Require(events != null && map.Profile != null && player != null && camera != null, "실제 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>(); // 실제 카메라 추적기
            if (follow != null) // 실제 추적기 확인
            { // 조건 시작
                follow.enabled = false; // 시험 중 카메라 수동 이동
            } // 조건 종료
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 기존 지역의 시험 간섭 방지
            } // 반복 종료
            foreach (SnakeContactHazard obstacle in map.GetComponentsInChildren<SnakeContactHazard>()) // 기존 장애물 순회
            { // 반복 시작
                obstacle.gameObject.SetActive(false); // 시험 지점의 배치 간섭 방지
            } // 반복 종료
            FieldInfo warningField = typeof(PrototypeMapProfile).GetField("worldEventWarning", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 예고 시간
            Require(warningField != null, "실제 예고 수치 누락"); // 실제 데이터 확인
            warningField.SetValue(map.Profile, 20f); // 방향 시험 중 예고 유지
            Vector3 cameraStart = camera.transform.position; // 실제 카메라 위치
            camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, cameraStart.z); // 플레이어 중심의 실제 화면
            Require(events.TryBeginRockfall(), "안전한 낙석 예고 시작 실패"); // 실제 낙석 예고
            Vector3 impact = (Vector3)typeof(WorldEventController).GetField("impact", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events); // 실제 낙석 착탄 지점
            camera.transform.position = new Vector3(impact.x, impact.y, cameraStart.z); // 착탄 지점을 화면 중앙으로 이동
            yield return null; // 실제 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Text label = GameObject.Find("WarningText")?.GetComponent<Text>(); // 실제 경고 문구
            Require(label != null && label.text == "낙석 주의", "화면 안의 기본 낙석 경고 오류"); // 실제 화면 안 기본 안내 확인
            Vector2[] cameraOffsets = { Vector2.right, Vector2.left, Vector2.up, Vector2.down, new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f) }; // 실제 화면 이동 방향
            string[] directions = { "왼쪽", "오른쪽", "아래", "위", "왼쪽 아래", "오른쪽 아래", "왼쪽 위", "오른쪽 위" }; // 실제 착탄 지점 방향
            for (int index = 0; index < cameraOffsets.Length; index++) // 실제 여덟 방향 순회
            { // 반복 시작
                Vector2 offset = cameraOffsets[index] * camera.orthographicSize * 4f; // 실제 화면 밖 이동 거리
                camera.transform.position = new Vector3(impact.x + offset.x, impact.y + offset.y, cameraStart.z); // 실제 카메라 이동
                yield return null; // 실제 경고 갱신 대기
                yield return null; // 실제 UI 적용 대기
                Require(label.text == "낙석 주의 · " + directions[index], "화면 밖 낙석 방향 오류: " + directions[index] + " / " + label.text); // 실제 화면 밖 경고 확인
            } // 반복 종료
            camera.transform.position = new Vector3(impact.x, impact.y, cameraStart.z); // 실제 착탄 지점 복귀
            yield return null; // 실제 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Require(label.text == "낙석 주의", "화면 복귀 후 낙석 방향 잔류"); // 실제 기본 문구 복원 확인
            float progress = events.WarningProgress; // 실제 예고 진행도
            GameManager.Instance.PauseGame(); // 실제 일시정지
            yield return new WaitForSecondsRealtime(0.2f); // 실제 정지 시간 대기
            Require(Mathf.Abs(events.WarningProgress - progress) < 0.01f, "일시정지 중 예고 진행"); // 실제 정지 상태 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            MethodInfo clear = typeof(WorldEventController).GetMethod("ClearEvent", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 위험 정리 경로
            Require(clear != null, "실제 위험 정리 경로 누락"); // 실제 경로 확인
            clear.Invoke(events, null); // 실제 낙석 예고 종료
            Require(events.TryBeginLocust(), "실제 황충 예고 시작 실패"); // 기존 황충 예고 확인
            yield return null; // 실제 황충 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Require(label.text == "황충떼 주의", "황충 경고에 낙석 방향 잔류"); // 기존 황충 경고 보존 확인
            AutomatedDay87RockfallWarningVerification.Finish(true, "Visible and eight offscreen directions, pause and locust warning passed."); // 실제 전체 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay87RockfallWarningVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
