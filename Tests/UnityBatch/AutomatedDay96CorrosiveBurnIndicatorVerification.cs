using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 시험용 남은 시간 조정
using ProjectEpsilon.Combat; // 실제 대상과 속성 반응
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 속성 구분
using ProjectEpsilon.Enemies; // 자연 적 생성기
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay96CorrosiveBurnIndicatorVerification // 부식 연소 표시 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay96CorrosiveBurnIndicator"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay96CorrosiveBurnIndicatorVerification() // 시험 복구
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
            new GameObject("Day96CorrosiveBurnRunner").AddComponent<Day96CorrosiveBurnRunner>(); // 실제 표시 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day96CorrosiveBurnIndicatorVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day96CorrosiveBurnRunner : MonoBehaviour // 실제 반응 표시 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 두 공격 순서와 표시 확인
        { // 함수 시작
            FindFirstObjectByType<EarlyEnemyWaveSpawner>().enabled = false; // 자연 적 출현 중지
            FindFirstObjectByType<SnakeWeaponManager>().enabled = false; // 자동 공격 간섭 방지
            WeaponAttributeCombatEffects effects = FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 전투 처리기
            Require(effects != null, "실제 속성 처리기 누락"); // 실제 전투 구성 확인
            WeaponTarget first = CreateTarget("Day96FireFirst"); // 화염 우선 시험 대상
            WeaponTarget second = CreateTarget("Day96PoisonFirst"); // 독 우선 시험 대상
            Hit(effects, first, WeaponAttribute.Fire); // 실제 첫 화염 공격
            Hit(effects, first, WeaponAttribute.Poison); // 실제 독 공격과 반응
            Hit(effects, second, WeaponAttribute.Poison); // 실제 첫 독 공격
            Hit(effects, second, WeaponAttribute.Fire); // 실제 화염 공격과 반응
            yield return null; // 실제 반응 표시 갱신 대기
            yield return null; // 지연 표시 완료 대기
            Transform firstIcon = first.transform.Find("StatusCorrosiveBurnIndicator"); // 화염 우선 실제 표시
            Transform secondIcon = second.transform.Find("StatusCorrosiveBurnIndicator"); // 독 우선 실제 표시
            Require(firstIcon != null && firstIcon.gameObject.activeSelf && secondIcon != null && secondIcon.gameObject.activeSelf, "화염·독 반응의 부식 연소 표시 누락"); // 실제 양방향 표시 확인
            Require(firstIcon.GetComponent<SpriteRenderer>().sprite != null && secondIcon.GetComponent<SpriteRenderer>().sprite != null, "부식 연소 반응 이미지 누락"); // 실제 반응 이미지 확인
            Require(first.GetComponent<WeaponTargetPoisonStatus>() == null || !first.GetComponent<WeaponTargetPoisonStatus>().IsActive, "반응 후 독 상태 미소비"); // 기존 반응 재료 확인
            float health = first.CurrentHealth; // 실제 정지 전 체력
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.1f); // 실제 정지 시간 대기
            Require(Mathf.Approximately(first.CurrentHealth, health) && firstIcon.gameObject.activeSelf, "정지 중 연소 피해 또는 표시 변경"); // 실제 정지 상태 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            WeaponTargetCorrosiveBurnStatus burning = first.GetComponent<WeaponTargetCorrosiveBurnStatus>(); // 실제 반응 지속 피해
            Require(burning != null, "실제 부식 연소 상태 누락"); // 실제 상태 확인
            typeof(WeaponTargetCorrosiveBurnStatus).GetField("remainingDuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(burning, 0.05f); // 짧은 만료 시험 시간
            yield return new WaitForSeconds(0.2f); // 실제 반응 만료 대기
            Require(first.transform.Find("StatusCorrosiveBurnIndicator") == null && secondIcon.gameObject.activeSelf, "개별 연소 만료의 표시 정리 실패"); // 실제 개별 만료 확인
            AutomatedDay96CorrosiveBurnIndicatorVerification.Finish(true, "Both reaction orders, pause and independent expiry passed."); // 실제 시험 결과
        } // 함수 종료
        private static WeaponTarget CreateTarget(string name) // 실제 격리 대상 생성
        { // 함수 시작
            WeaponTarget target = new GameObject(name).AddComponent<WeaponTarget>(); // 실제 공격 대상
            target.Configure(100f); // 실제 시험 체력
            return target; // 실제 대상 반환
        } // 함수 종료
        private static void Hit(WeaponAttributeCombatEffects effects, WeaponTarget target, WeaponAttribute attribute) // 실제 속성 공격
        { // 함수 시작
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, attribute, 2, 2, 1, target.transform.position, 1f); // 실제 공격 정보
            effects.ApplyHit(attack, target, target.transform.position); // 실제 전투 명중
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay96CorrosiveBurnIndicatorVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
