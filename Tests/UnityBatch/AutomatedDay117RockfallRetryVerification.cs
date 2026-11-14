using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 일정 조회
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어와 장애물
using ProjectEpsilon.World; // 실제 맵과 위험 일정
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay117RockfallRetryVerification // 실제 낙석 일정 재시도 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay117RockfallRetry"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay117RockfallRetryVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 낙석 일정 시험
        { // 함수 시작
            string[] paths = { "Assets/Data/World/PrototypeMapPlains.asset", "Assets/Data/World/PrototypeMapRuins.asset", "Assets/Data/World/PrototypeMapSwamp.asset" }; // 실제 세 맵 데이터 경로
            foreach (string path in paths) // 실제 세 맵 데이터 순회
            { // 반복 시작
                PrototypeMapProfile profile = AssetDatabase.LoadAssetAtPath<PrototypeMapProfile>(path); // 실제 맵 설정
                if (profile == null || !Mathf.Approximately(profile.WorldEventRetryInterval, 3f)) // 실제 재시도 시험값 확인
                { // 조건 시작
                    Finish(false, "세 맵의 실제 재시도 설정 오류: " + path); // 실제 데이터 실패 기록
                    return; // 실제 전투 시험 중단
                } // 조건 종료
            } // 반복 종료
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
            new GameObject("Day117RockfallRetryRunner").AddComponent<Day117RockfallRetryRunner>(); // 실제 일정 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day117RockfallRetryVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day117RockfallRetryRunner : MonoBehaviour // 실제 낙석 일정 시험 실행기
    { // 클래스 시작
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 실제 일정 필드 조회
        private IEnumerator Start() // 실제 안전 후보 부재와 복구 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 맵
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 위험 일정
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            Camera camera = Camera.main; // 실제 전투 카메라
            Require(map != null && map.Profile != null && events != null && player != null && camera != null, "실제 낙석 일정 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 플레이어 이동 중지
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 실제 시험 위치 확보
            } // 반복 종료
            foreach (SnakeContactHazard obstacle in FindObjectsByType<SnakeContactHazard>(FindObjectsSortMode.None)) // 기존 접촉 객체 순회
            { // 반복 시작
                if (obstacle.HazardType == SnakeContactHazardType.Obstacle) // 실제 맵 장애물 확인
                { // 조건 시작
                    obstacle.gameObject.SetActive(false); // 실제 시험 위치 확보
                } // 조건 종료
            } // 반복 종료
            camera.orthographicSize = 30f; // 시험 후보 전체의 카메라 범위 확보
            camera.enabled = false; // 실제 안전한 화면 후보 부재 구성
            Set(events, "nextRockfall", true); // 실제 다음 낙석 선택
            Set(events, "nextEvent", (float)Get(events, "elapsed") + 0.05f); // 실제 경과 기준 다음 위험 시각 단축
            yield return new WaitForSeconds(0.2f); // 실제 자동 일정 실행 대기
            float elapsed = (float)Get(events, "elapsed"); // 실제 위험 경과 시간
            float next = (float)Get(events, "nextEvent"); // 실제 다음 재시도 시각
            Require(!events.IsWarning && !events.IsActive && (bool)Get(events, "nextRockfall"), "안전 후보 없이 위험이 시작되거나 종류 변경"); // 실제 안전 대기 확인
            Require(next - elapsed > 0f && next - elapsed <= 3.1f, "안전 후보 부재 후 전체 출현 간격으로 지연"); // 실제 짧은 재시도 확인
            camera.enabled = true; // 실제 안전한 화면 후보 복구
            yield return new WaitForSeconds(3.3f); // 실제 짧은 재시도 대기
            Require(events.IsWarning && !(bool)Get(events, "nextRockfall"), "안전 후보 복구 후 낙석 예고 또는 교대 실패"); // 실제 안전 낙석 예고 확인
            elapsed = (float)Get(events, "elapsed"); // 실제 위험 경과 시간
            next = (float)Get(events, "nextEvent"); // 실제 다음 정상 일정
            Require(next - elapsed > 80f, "낙석 성공 후 기존 정상 간격 미적용"); // 실제 성공 후 정상 간격 확인
            AutomatedDay117RockfallRetryVerification.Finish(true, "Actual unsafe rockfall retried promptly and kept the normal interval after success."); // 실제 시험 결과
        } // 함수 종료
        private static object Get(object target, string name) // 실제 일정 필드 조회
        { // 함수 시작
            FieldInfo field = target.GetType().GetField(name, Private); // 실제 일정 필드
            Require(field != null, "실제 일정 필드 누락: " + name); // 실제 필드 확인
            return field.GetValue(target); // 실제 일정 반환
        } // 함수 종료
        private static void Set(object target, string name, object value) // 실제 시험 일정 설정
        { // 함수 시작
            FieldInfo field = target.GetType().GetField(name, Private); // 실제 일정 필드
            Require(field != null, "실제 일정 필드 누락: " + name); // 실제 필드 확인
            field.SetValue(target, value); // 실제 시험 일정 반영
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay117RockfallRetryVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
