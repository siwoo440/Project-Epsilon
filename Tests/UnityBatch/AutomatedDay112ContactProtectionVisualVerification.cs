using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 보호 종료 시각 설정
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Combat; // 실제 포탑 관리자
using ProjectEpsilon.Player; // 실제 충돌과 표시
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay112ContactProtectionVisualVerification // 실제 충돌 보호 표시 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay112ContactProtectionVisual"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay112ContactProtectionVisualVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 충돌 보호 표시 시험
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
            new GameObject("Day112ContactVisualRunner").AddComponent<Day112ContactVisualRunner>(); // 실제 표시 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day112ContactProtectionVisualVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day112ContactVisualRunner : MonoBehaviour // 실제 보호 표시 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 보호 표시의 시작과 종료 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            SnakeExternalCollision collision = player == null ? null : player.GetComponent<SnakeExternalCollision>(); // 실제 적 직접 충돌
            SnakeInvulnerability invulnerability = player == null ? null : player.GetComponent<SnakeInvulnerability>(); // 실제 완전 무적
            Require(player != null && weapons != null && spawner != null && collision != null && invulnerability != null, "실제 충돌 보호 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            yield return null; // 실제 표시 초기화 대기
            Transform limited = player.transform.Find("PlayerContactProtectionVfx"); // 실제 제한 보호 표시
            Transform full = player.transform.Find("PlayerInvulnerabilityVfx"); // 실제 완전 무적 표시
            Require(limited != null && full != null && !limited.gameObject.activeSelf && !full.gameObject.activeSelf, "보호 전 표시 구성 오류"); // 기본 비활성 확인
            Require(collision.TryApplyEnemyDirectContact() && collision.EnemyContactProtected, "실제 적 직접 충돌 보호 시작 실패"); // 실제 몸통 손실과 보호 시작
            yield return null; // 실제 표시 갱신 대기
            SpriteRenderer limitedImage = limited.GetComponent<SpriteRenderer>(); // 실제 제한 보호 이미지
            Require(limited.gameObject.activeSelf && !full.gameObject.activeSelf && limitedImage != null && limitedImage.sprite != null, "제한 보호 표시 또는 완전 무적 구분 실패"); // 제한 보호 표시 확인
            invulnerability.StartInvulnerability(3f); // 실제 완전 무적 시작
            yield return null; // 실제 표시 갱신 대기
            Require(limited.gameObject.activeSelf && full.gameObject.activeSelf, "두 보호 상태의 독립 표시 실패"); // 실제 두 상태 중첩 확인
            FieldInfo ready = typeof(SnakeExternalCollision).GetField("nextEnemyContactAllowedTime", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 직접 충돌 보호 종료 시각
            Require(ready != null, "실제 직접 충돌 보호 시각 누락"); // 실제 필드 확인
            ready.SetValue(collision, Time.time - 1f); // 직접 충돌 보호 종료
            yield return null; // 실제 표시 갱신 대기
            Require(!limited.gameObject.activeSelf && full.gameObject.activeSelf, "제한 보호 종료 또는 완전 무적 유지 실패"); // 독립 종료 확인
            invulnerability.ClearInvulnerability(); // 실제 완전 무적 종료
            yield return null; // 실제 표시 갱신 대기
            Require(!limited.gameObject.activeSelf && !full.gameObject.activeSelf, "완전 무적 종료 표시 실패"); // 모든 보호 종료 확인
            AutomatedDay112ContactProtectionVisualVerification.Finish(true, "Actual direct-contact protection and full invulnerability displayed independently."); // 실제 시험 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay112ContactProtectionVisualVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
