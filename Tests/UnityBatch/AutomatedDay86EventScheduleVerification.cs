using System; // 실제 예외 처리
using System.Collections; // 실제 프레임 대기
using System.Reflection; // 실제 시험 수치 설정
using ProjectEpsilon.Combat; // 기존 포탑 관리자
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 기존 적 출현기
using ProjectEpsilon.Player; // 실제 뱀 이동
using ProjectEpsilon.World; // 실제 맵 위험
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬 열기
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay86EventScheduleVerification // 실제 자동 위험 일정 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay86EventSchedule"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay86EventScheduleVerification() // 시험 복구
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
            new GameObject("Day86EventScheduleRunner").AddComponent<Day86EventScheduleRunner>(); // 실제 일정 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 배치 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 감시 해제
            Debug.Log("[Day86EventScheduleVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day86EventScheduleRunner : MonoBehaviour // 실제 자동 위험 일정 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 자동 출현 확인
        { // 함수 시작
            PrototypeChunkMap map = FindFirstObjectByType<PrototypeChunkMap>(); // 실제 청크 관리자
            WorldEventController events = map == null ? null : map.GetComponent<WorldEventController>(); // 실제 맵 위험 관리자
            Require(events != null && map.Profile != null, "실제 맵 위험 구성 누락"); // 실제 구성 확인
            FindFirstObjectByType<SnakeMovement>().enabled = false; // 시험 중 머리 이동 중지
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            foreach (PrototypeSpecialZone zone in FindObjectsByType<PrototypeSpecialZone>(FindObjectsSortMode.None)) // 기존 특수 지역 순회
            { // 반복 시작
                zone.gameObject.SetActive(false); // 기존 지역의 시험 간섭 방지
            } // 반복 종료
            SetFloat(map.Profile, "worldEventWarning", 0.4f); // 빠른 시험용 예고 시간
            SetFloat(map.Profile, "worldEventInterval", 2.5f); // 빠른 시험용 위험 간격
            SetFloat(map.Profile, "locustSpeed", 30f); // 빠른 시험용 황충 속도
            typeof(PrototypeMapProfile).GetField("locustMinimumWaves", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map.Profile, 1); // 기존 단일 대열 일정 대조
            typeof(PrototypeMapProfile).GetField("locustMaximumWaves", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(map.Profile, 1); // 기존 단일 대열 일정 대조
            SetFloat(events, "elapsed", 0f); // 실제 일정 경과 시간 초기화
            SetFloat(events, "nextEvent", 0.4f); // 실제 첫 위험 시간 단축
            yield return new WaitForSeconds(0.5f); // 실제 첫 위험 일정 경과
            Require(events.IsWarning && GameObject.Find("WorldHazardVisual") != null, "설정된 시간의 자동 황충 예고 누락"); // 실제 자동 황충 예고 확인
            yield return new WaitForSeconds(2.5f); // 실제 두 번째 위험 일정 경과
            Require(events.IsWarning && GameObject.Find("WorldHazardVisual") != null, "설정된 간격의 자동 낙석 예고 누락"); // 실제 자동 낙석 예고 확인
            AutomatedDay86EventScheduleVerification.Finish(true, "Automatic alternating events used configured times."); // 실제 전체 결과
        } // 함수 종료
        private static void SetFloat(object target, string name, float value) // 독립 시험 수치 적용
        { // 함수 시작
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); // 실제 맵 수치 필드
            Require(field != null, "맵 위험 수치 누락: " + name); // 실제 데이터 확인
            field.SetValue(target, value); // 메모리의 시험값 적용
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedDay86EventScheduleVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
