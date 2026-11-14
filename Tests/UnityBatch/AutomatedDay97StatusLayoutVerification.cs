using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 대상과 상태
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay97StatusLayoutVerification // 중첩 상태 배치 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay97StatusLayout"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay97StatusLayoutVerification() // 시험 복구
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
            new GameObject("Day97StatusLayoutRunner").AddComponent<Day97StatusLayoutRunner>(); // 실제 배치 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day97StatusLayoutVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day97StatusLayoutRunner : MonoBehaviour // 실제 배치 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 상태별 표시 배치 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            GameObject enemy = new GameObject("Day97Target"); // 격리된 실제 적
            WeaponTarget target = enemy.AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 시험 체력 설정
            WeaponTargetColdStatus cold = enemy.AddComponent<WeaponTargetColdStatus>(); // 실제 냉기 상태
            Require(cold.Apply(target, 2), "실제 냉기 적용 실패"); // 실제 냉기 적용
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 표시 완료 대기
            Transform coldIcon = enemy.transform.Find("StatusColdIndicator"); // 실제 냉기 표시
            Require(coldIcon != null && Mathf.Approximately(coldIcon.localPosition.x, 0f), "상태 한 개의 가운데 정렬 실패"); // 실제 한 개 정렬 확인
            WeaponTargetPoisonStatus poison = enemy.AddComponent<WeaponTargetPoisonStatus>(); // 실제 독 상태
            Require(poison.Apply(target, 2), "실제 독 적용 실패"); // 실제 독 적용
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 표시 완료 대기
            Transform poisonIcon = enemy.transform.Find("StatusPoisonIndicator"); // 실제 독 표시
            Require(poisonIcon != null && Mathf.Approximately(coldIcon.localPosition.x + poisonIcon.localPosition.x, 0f) && coldIcon.localPosition.y == poisonIcon.localPosition.y, "상태 두 개의 가운데 정렬 실패"); // 실제 두 개 정렬 확인
            WeaponTargetDarkCurseStatus dark = enemy.AddComponent<WeaponTargetDarkCurseStatus>(); // 실제 암흑 저주
            WeaponTargetCorrosiveCurseStatus curse = enemy.AddComponent<WeaponTargetCorrosiveCurseStatus>(); // 실제 부식 저주
            WeaponTargetCorrosiveBurnStatus burn = enemy.AddComponent<WeaponTargetCorrosiveBurnStatus>(); // 실제 부식 연소
            Require(dark.Apply(2, 1) > 0, "실제 암흑 저주 적용 실패"); // 실제 암흑 저주 적용
            curse.Apply(3f, 0.8f, 1.1f); // 실제 부식 저주 적용
            burn.Apply(target, 3f, 1f); // 실제 부식 연소 적용
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 표시 완료 대기
            Transform darkIcon = enemy.transform.Find("StatusDarkIndicator"); // 실제 암흑 표시
            Transform curseIcon = enemy.transform.Find("StatusCorrosiveCurseIndicator"); // 실제 저주 반응 표시
            Transform burnIcon = enemy.transform.Find("StatusCorrosiveBurnIndicator"); // 실제 연소 반응 표시
            Require(darkIcon != null && curseIcon != null && burnIcon != null, "실제 다섯 상태 표시 누락"); // 실제 표시 존재 확인
            Require(coldIcon.localPosition.y == poisonIcon.localPosition.y && poisonIcon.localPosition.y == darkIcon.localPosition.y && curseIcon.localPosition.y == burnIcon.localPosition.y && curseIcon.localPosition.y > darkIcon.localPosition.y, "다섯 상태의 두 줄 배치 실패"); // 실제 두 줄 배치 확인
            Require(Mathf.Approximately(coldIcon.localPosition.x + poisonIcon.localPosition.x + darkIcon.localPosition.x, 0f) && Mathf.Approximately(curseIcon.localPosition.x + burnIcon.localPosition.x, 0f), "중첩 상태의 줄별 가운데 정렬 실패"); // 실제 줄별 정렬 확인
            poison.Clear(); // 실제 독 해제
            dark.Clear(); // 실제 암흑 저주 해제
            curse.Apply(0f, 1f, 1f); // 실제 부식 저주 해제
            burn.Apply(target, 0f, 0f); // 실제 부식 연소 해제
            yield return null; // 실제 표시 갱신 대기
            yield return null; // 지연 표시 완료 대기
            Require(coldIcon.gameObject.activeSelf && Mathf.Approximately(coldIcon.localPosition.x, 0f), "상태 해제 후 남은 표시 가운데 정렬 실패"); // 실제 재배치 확인
            AutomatedDay97StatusLayoutVerification.Finish(true, "Single, pair, five indicators and reflow passed."); // 실제 시험 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay97StatusLayoutVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
