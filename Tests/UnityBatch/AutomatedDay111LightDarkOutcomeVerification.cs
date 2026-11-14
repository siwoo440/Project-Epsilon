using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 속성 반응
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 공격 속성
using ProjectEpsilon.Enemies; // 실제 적 출현기
using ProjectEpsilon.Player; // 실제 플레이어 체력
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay111LightDarkOutcomeVerification // 명암 반응의 승리 경계 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay111LightDarkOutcome"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay111LightDarkOutcomeVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunVictory() // 실제 보스 반응 시험
        { // 함수 시작
            Begin(false); // 실제 승리 경계 시험
        } // 함수 종료
        public static void RunNormal() // 실제 일반 반응 시험
        { // 함수 시작
            Begin(true); // 일반 전투 시험
        } // 함수 종료
        private static void Begin(bool normal) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetBool("EpsilonDay111Normal", normal); // 시험 종류 기록
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
            new GameObject("Day111LightDarkRunner").AddComponent<Day111LightDarkRunner>(); // 실제 반응 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day111LightDarkOutcomeVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day111LightDarkRunner : MonoBehaviour // 실제 명암 반응 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 명암 반응과 승리 경계 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects effects = weapons == null ? null : weapons.AttributeCombatEffects; // 실제 속성 전투 처리기
            SnakeHealth health = player == null ? null : player.GetComponent<SnakeHealth>(); // 실제 공용 체력
            Require(player != null && weapons != null && spawner != null && effects != null && health != null, "실제 반응 시험 구성 누락"); // 실제 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 시험 간섭 제거
            } // 반복 종료
            bool normal = SessionState.GetBool("EpsilonDay111Normal", false); // 일반 전투 시험 여부
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget victim = CreateTarget("Day111Victim", center, 4f); // 실제 반응 처치 대상
            WeaponTarget boss = normal ? CreateTarget("Day111DistantBoss", center + Vector3.right * 50f, 20f) : victim; // 실제 최종 보스
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            Hit(effects, victim, WeaponAttribute.Dark); // 실제 암흑 저주와 반응 재료 준비
            Require(victim.IsAlive, "반응 전 실제 대상 사망"); // 첫 명중 후 생존 확인
            health.ResetHealth(); // 실제 공용 체력 복구
            Require(health.TakeDamage(20), "실제 회복 시험 피해 실패"); // 실제 회복 여유 확보
            int before = health.CurrentHealth; // 실제 반응 전 체력
            Hit(effects, victim, WeaponAttribute.Holy); // 실제 신성 명중과 명암 반응
            Require(!victim.IsAlive, "명암 반응의 실제 대상 처치 실패"); // 실제 반응 피해 확인
            if (normal) // 일반 전투 시험 여부
            { // 조건 시작
                Require(GameManager.Instance.IsPlaying && health.CurrentHealth > before, "일반 전투의 실제 명암 회복 누락"); // 실제 일반 회복 확인
            } // 조건 종료
            else // 실제 보스 승리 경계 확인
            { // 대체 시작
                Require(GameManager.Instance.CurrentState == GameState.Victory, "실제 명암 반응의 보스 승리 실패"); // 실제 승리 확정 확인
                Require(health.CurrentHealth == before, "승리 확정 후 명암 반응 회복 적용"); // 승리 후 회복 차단 확인
            } // 대체 종료
            yield return null; // 실제 지연 파괴 대기
            AutomatedDay111LightDarkOutcomeVerification.Finish(true, "Actual light-dark reaction respected victory and normal combat."); // 실제 시험 결과
        } // 함수 종료
        private static void Hit(WeaponAttributeCombatEffects effects, WeaponTarget target, WeaponAttribute attribute) // 실제 속성 명중
        { // 함수 시작
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, attribute, 2, 2, 1, target.transform.position, 1f); // 실제 이 단계 공격
            effects.ApplyHit(attack, target, target.transform.position); // 실제 속성 전투 적용
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
                AutomatedDay111LightDarkOutcomeVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
