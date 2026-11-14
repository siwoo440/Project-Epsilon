using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 신성 효과와 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어 체력과 보호막
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay108ShieldOutcomeVerification // 신성 보호막의 승패 경계 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay108ShieldOutcome"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay108ShieldOutcomeVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunVictory() // 보호막 폭발의 보스 처치 시험
        { // 함수 시작
            Begin(0); // 실제 승리 경계 시험 시작
        } // 함수 종료
        public static void RunDefeat() // 플레이어 패배 뒤 보호막 폭발 시험
        { // 함수 시작
            Begin(1); // 실제 패배 경계 시험 시작
        } // 함수 종료
        public static void RunNormal() // 일반 보호막 폭발 유지 시험
        { // 함수 시작
            Begin(2); // 실제 일반 전투 시험 시작
        } // 함수 종료
        private static void Begin(int mode) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetInt("EpsilonDay108Mode", mode); // 실제 시험 종류 저장
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
            new GameObject("Day108ShieldOutcomeRunner").AddComponent<Day108ShieldOutcomeRunner>(); // 실제 보호막 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day108ShieldOutcomeVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day108ShieldOutcomeRunner : MonoBehaviour // 실제 보호막 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 보호막 폭발과 승패 경계 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributePlayerEffects effects = weapons == null || weapons.AttributeCombatEffects == null ? null : weapons.AttributeCombatEffects.PlayerEffects; // 실제 포탑에 연결된 신성 효과 관리자
            SnakeHealth health = effects == null ? null : effects.SnakeHealth; // 실제 효과에 연결된 공용 체력
            SnakeShieldController shield = effects == null ? null : effects.ShieldController; // 실제 효과에 연결된 공용 보호막
            Require(player != null && weapons != null && spawner != null && effects != null && effects.IsConfigured && health != null && shield != null, "실제 플레이어·신성 보호막 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 폭발 간섭 제거
            } // 반복 종료
            int mode = SessionState.GetInt("EpsilonDay108Mode", 0); // 실제 시험 종류
            Vector3 center = effects.EffectOrigin.position; // 실제 보호막 폭발 중심
            WeaponTarget other = CreateTarget("Day108Other", center + Vector3.right * 0.5f, 100f); // 실제 역순 폭발의 후속 대상
            WeaponTarget boss = CreateTarget("Day108Boss", center + Vector3.left * (mode == 0 ? 0.5f : 50f), 1f); // 실제 최종 보스 위치
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            shield.Clear(); // 기존 보호막 초기화
            health.ResetHealth(); // 실제 공용 체력 복구
            Require(shield.Apply(1, 3f, 8, 10f), "실제 신성 보호막 적용 실패"); // 실제 팔 단계 보호막 적용
            Require(health.TakeDamage(mode == 1 ? 10000 : 1), "실제 보호막 파괴 피해 실패"); // 실제 피해와 보호막 종료
            if (mode == 0) // 실제 보스 승리 시험 확인
            { // 조건 시작
                Require(GameManager.Instance.CurrentState == GameState.Victory && !boss.IsAlive, "실제 보호막 폭발의 보스 처치 실패"); // 실제 승리 확인
                Require(Mathf.Approximately(other.CurrentHealth, 100f), "승리 확정 후 보호막 폭발이 다른 적에게 피해 적용"); // 실제 승리 후 피해 차단 확인
            } // 조건 종료
            else if (mode == 1) // 실제 플레이어 패배 시험 확인
            { // 조건 시작
                Require(GameManager.Instance.CurrentState == GameState.Defeated, "실제 플레이어 패배 실패"); // 실제 패배 확인
                Require(Mathf.Approximately(other.CurrentHealth, 100f), "패배 확정 후 보호막 폭발 피해 적용"); // 실제 패배 후 폭발 차단 확인
            } // 조건 종료
            else // 실제 일반 전투 시험 확인
            { // 대체 시작
                Require(GameManager.Instance.IsPlaying && other.CurrentHealth < 100f, "일반 전투의 실제 보호막 폭발 피해 누락"); // 실제 일반 폭발 확인
            } // 대체 종료
            yield return null; // 실제 지연 파괴 완료 대기
            AutomatedDay108ShieldOutcomeVerification.Finish(true, "Actual shield burst respected victory, defeat and normal combat."); // 실제 시험 결과
        } // 함수 종료
        private static WeaponTarget CreateTarget(string name, Vector3 position, float health) // 실제 공격 대상 생성
        { // 함수 시작
            GameObject item = new GameObject(name); // 실제 격리 대상
            item.transform.position = position; // 실제 대상 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 공격 대상 구성
            target.Configure(health); // 실제 대상 체력
            return target; // 실제 공격 대상 반환
        } // 함수 종료
        private static void Require(bool condition, string message) // 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건
            { // 조건 시작
                AutomatedDay108ShieldOutcomeVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
