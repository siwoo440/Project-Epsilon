using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Combat; // 실제 신성 공격과 효과
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 공격 속성
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어와 보호막
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay110HolyHitOutcomeVerification // 신성 명중의 승패 경계 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay110HolyHitOutcome"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay110HolyHitOutcomeVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunVictory() // 신성 보스 처치 시험
        { // 함수 시작
            Begin(0); // 실제 승리 경계 시험 시작
        } // 함수 종료
        public static void RunNormal() // 일반 신성 명중 시험
        { // 함수 시작
            Begin(1); // 실제 일반 전투 시험 시작
        } // 함수 종료
        public static void RunDefeat() // 패배 뒤 지연 신성 명중 시험
        { // 함수 시작
            Begin(2); // 실제 패배 경계 시험 시작
        } // 함수 종료
        private static void Begin(int mode) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetInt("EpsilonDay110Mode", mode); // 실제 시험 종류 저장
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
            new GameObject("Day110HolyHitOutcomeRunner").AddComponent<Day110HolyHitOutcomeRunner>(); // 실제 신성 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day110HolyHitOutcomeVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day110HolyHitOutcomeRunner : MonoBehaviour // 실제 신성 명중 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 신성 명중과 승패 경계 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            WeaponAttributeCombatEffects combat = weapons == null ? null : weapons.AttributeCombatEffects; // 실제 속성 전투 처리기
            WeaponAttributePlayerEffects effects = combat == null ? null : combat.PlayerEffects; // 실제 신성 효과 관리자
            SnakeHealth health = effects == null ? null : effects.SnakeHealth; // 실제 공용 체력
            SnakeShieldController shield = effects == null ? null : effects.ShieldController; // 실제 공용 보호막
            Require(player != null && weapons != null && spawner != null && combat != null && effects != null && health != null && shield != null, "실제 플레이어·신성 전투 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 시험 간섭 제거
            } // 반복 종료
            int mode = SessionState.GetInt("EpsilonDay110Mode", 0); // 실제 시험 종류
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget victim = CreateTarget("Day110Victim", center, 1f); // 실제 신성 명중 대상
            WeaponTarget boss = mode == 0 ? victim : CreateTarget("Day110DistantBoss", center + Vector3.right * 50f, 1f); // 실제 최종 보스 위치
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            shield.Clear(); // 기존 보호막 초기화
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Holy, 6, 6, 1, center, 10f); // 실제 육 단계 신성 공격
            for (int index = 0; index < 4; index++) // 보호막 이전 실제 신성 명중 순회
            { // 반복 시작
                effects.HandleHolyHit(attack); // 실제 신성 명중 횟수 누적
            } // 반복 종료
            Require(shield.CurrentShield == 0, "보호막이 다섯 번째 명중 전 발동"); // 실제 보호막 대기 상태 확인
            health.ResetHealth(); // 실제 공용 체력 복구
            if (mode == 2) // 실제 패배 경계 시험 확인
            { // 조건 시작
                Require(health.TakeDamage(10000) && GameManager.Instance.CurrentState == GameState.Defeated, "실제 플레이어 패배 실패"); // 실제 패배 확정
                effects.HandleHolyHit(attack); // 패배 직후 지연된 실제 신성 명중 효과
                Require(shield.CurrentShield == 0, "패배 확정 후 신성 보호막 적용"); // 실제 패배 후 보호막 차단 확인
            } // 조건 종료
            else // 실제 신성 공격 시험 확인
            { // 대체 시작
                Require(health.TakeDamage(20), "실제 회복 필요 체력 구성 실패"); // 실제 공용 체력 피해
                int before = health.CurrentHealth; // 실제 공격 전 체력
                combat.ApplyHit(attack, victim, victim.transform.position); // 실제 신성 명중과 처치
                if (mode == 0) // 실제 보스 승리 시험 확인
                { // 조건 시작
                    Require(GameManager.Instance.CurrentState == GameState.Victory && !boss.IsAlive, "실제 신성 보스 처치 실패"); // 실제 승리 확인
                    Require(shield.CurrentShield == 0 && health.CurrentHealth == before, "승리 확정 후 신성 회복 또는 보호막 적용"); // 실제 승리 후 효과 차단 확인
                } // 조건 종료
                else // 실제 일반 전투 시험 확인
                { // 대체 시작
                    Require(GameManager.Instance.IsPlaying && shield.CurrentShield > 0, "일반 전투의 실제 신성 보호막 발동 실패"); // 실제 일반 신성 효과 확인
                } // 대체 종료
            } // 대체 종료
            yield return null; // 실제 지연 파괴 완료 대기
            AutomatedDay110HolyHitOutcomeVerification.Finish(true, "Actual holy hit respected victory, defeat and normal combat."); // 실제 시험 결과
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
                AutomatedDay110HolyHitOutcomeVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
