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
    public static class AutomatedDay88LocustWarningVerification // 황충 진행 방향 안내 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay88LocustWarning"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay88LocustWarningVerification() // 시험 복구
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
            new GameObject("Day88LocustWarningRunner").AddComponent<Day88LocustWarningRunner>(); // 실제 프레임 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day88LocustWarningVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day88LocustWarningRunner : MonoBehaviour // 실제 황충 안내 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 카메라 방향과 경고 확인
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
                follow.enabled = false; // 시험 중 카메라 수동 제어
            } // 조건 종료
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            FieldInfo duration = typeof(PrototypeMapProfile).GetField("worldEventWarning", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 예고 시간
            FieldInfo direction = typeof(WorldEventController).GetField("direction", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 황충 이동 방향
            Require(duration != null && direction != null, "실제 황충 시험 수치 누락"); // 실제 데이터 확인
            duration.SetValue(map.Profile, 20f); // 방향 시험 중 예고 유지
            Require(events.TryBeginLocust(), "실제 황충 예고 시작 실패"); // 실제 황충 예고 시작
            yield return null; // 실제 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Text label = GameObject.Find("WarningText")?.GetComponent<Text>(); // 실제 경고 문구
            Require(label != null, "실제 황충 경고판 누락"); // 실제 경고판 확인
            Require(label.resizeTextForBestFit && label.resizeTextMinSize > 0 && label.resizeTextMaxSize >= label.resizeTextMinSize, "긴 방향 경고 글자 크기 조정 누락"); // 실제 경고 글자의 자동 크기 조정 확인
            Vector3[] vectors = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.right, Vector3.up, Vector3.up }; // 실제 황충 진행 방향
            float[] rotations = { 0f, 0f, 0f, 0f, 90f, 90f, 45f }; // 실제 전투 카메라 회전
            string[] labels = { "오른쪽", "왼쪽", "위", "아래", "아래", "오른쪽", "오른쪽 위" }; // 실제 화면상 진행 방향
            for (int index = 0; index < vectors.Length; index++) // 실제 일곱 방향 조건 순회
            { // 반복 시작
                camera.transform.rotation = Quaternion.Euler(0f, 0f, rotations[index]); // 실제 카메라 방향 설정
                direction.SetValue(events, vectors[index]); // 실제 황충 진행 방향 설정
                yield return null; // 실제 경고 갱신 대기
                yield return null; // 실제 UI 적용 대기
                Require(label.text == "황충떼 주의 · " + labels[index] + " 이동", "황충 진행 방향 오류: " + labels[index] + " / " + label.text); // 실제 화면 기준 방향 확인
            } // 반복 종료
            Require(events.IsWarning, "방향 안내 중 황충 예고 종료"); // 실제 경고 진행 보존 확인
            direction.SetValue(events, Vector3.zero); // 실제 이동 방향 부재 설정
            yield return null; // 실제 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Require(label.text == "황충떼 주의", "이동 방향 부재 시 기본 경고 누락"); // 실제 안전 대체 경고 확인
            direction.SetValue(events, Vector3.right); // 실제 황충 이동 방향 복원
            camera.tag = "Untagged"; // 실제 주 카메라 부재 설정
            yield return null; // 실제 경고 갱신 대기
            yield return null; // 실제 UI 적용 대기
            Require(label.text == "황충떼 주의", "카메라 부재 시 기본 경고 누락"); // 실제 카메라 부재 대체 확인
            camera.tag = "MainCamera"; // 실제 주 카메라 태그 복원
            AutomatedDay88LocustWarningVerification.Finish(true, "Cardinal, rotated camera, diagonal and fallback locust warnings passed."); // 실제 전체 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay88LocustWarningVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
