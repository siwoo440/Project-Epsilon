using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Collections.Generic; // 실제 폭발 대상 순서
using ProjectEpsilon.Combat; // 실제 화상과 처치 폭발
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 화염 포탑 데이터
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어 이동
using UnityEditor; // 배치 실행과 포탑 데이터
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay106BurnDeathVictoryVerification // 화상 폭발의 승리 경계 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay106BurnDeathVictory"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay106BurnDeathVictoryVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void RunBossBurn() // 화상 보스 사망 시험
        { // 함수 시작
            Begin(0); // 실제 보스 화상 폭발 시험 시작
        } // 함수 종료
        public static void RunBossInBlast() // 화상 폭발 중 보스 사망 시험
        { // 함수 시작
            Begin(1); // 실제 폭발 대상 순서 시험 시작
        } // 함수 종료
        public static void RunNormal() // 일반 화상 폭발 유지 시험
        { // 함수 시작
            Begin(2); // 실제 일반 폭발 시험 시작
        } // 함수 종료
        private static void Begin(int mode) // 실제 게임 씬 시험 준비
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬
            SessionState.SetInt("EpsilonDay106Mode", mode); // 실제 시험 종류 저장
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
            new GameObject("Day106BurnDeathVictoryRunner").AddComponent<Day106BurnDeathVictoryRunner>(); // 실제 사망 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day106BurnDeathVictoryVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day106BurnDeathVictoryRunner : MonoBehaviour // 실제 화상 사망 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 화상 폭발과 승리 경계 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(player != null && weapons != null && spawner != null, "실제 플레이어·전투 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 자동 공격 간섭 방지
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 폭발 간섭 제거
            } // 반복 종료
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlaster.asset"); // 실제 화상 폭발 포탑 데이터
            Require(weapon != null && weapon.GradeFiveBurnExplosionRadius > 1f && weapon.GradeFiveBurnExplosionDamageMultiplier > 0f, "실제 화상 폭발 데이터 누락"); // 실제 시험 데이터 확인
            int mode = SessionState.GetInt("EpsilonDay106Mode", 0); // 실제 시험 종류
            Vector3 center = new Vector3(500f, 500f, 0f); // 다른 전투와 분리된 위치
            WeaponTarget source = CreateTarget("Day106Source", center, 1f); // 실제 화상 처치 대상
            WeaponTarget a = CreateTarget("Day106CandidateA", center + Vector3.right * 0.5f, 100f); // 실제 첫 폭발 후보
            WeaponTarget b = CreateTarget("Day106CandidateB", center + Vector3.left * 0.5f, 100f); // 실제 둘째 폭발 후보
            WeaponTarget boss = mode == 0 ? source : mode == 2 ? CreateTarget("Day106DistantBoss", center + Vector3.right * 50f, 1f) : null; // 실제 보스 배치
            WeaponTarget observed = a; // 실제 후속 피해 확인 대상
            if (mode == 1) // 실제 폭발 중 보스 처치 시험 확인
            { // 조건 시작
                List<WeaponTarget> order = new List<WeaponTarget>(); // 실제 폭발 대상 순서
                foreach (WeaponTarget item in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 실제 폭발과 같은 대상 조회
                { // 반복 시작
                    if (item == a || item == b) // 실제 시험 후보 확인
                    { // 조건 시작
                        order.Add(item); // 실제 폭발 처리 순서 저장
                    } // 조건 종료
                } // 반복 종료
                Require(order.Count == 2, "실제 화상 폭발 대상 순서 확인 실패"); // 실제 후보 순서 확인
                boss = order[0]; // 실제 폭발의 첫 대상 보스
                boss.Configure(1f); // 실제 한 번에 처치할 보스 체력
                observed = order[1]; // 실제 폭발의 다음 대상
            } // 조건 종료
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            WeaponTargetBurnStatus burn = source.gameObject.AddComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태
            Require(burn.Apply(source, 0, 3f, 1f), "실제 화상 적용 실패"); // 실제 화상 조건 구성
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Fire, 0, 0, 5, center, 10f); // 실제 오 등급 화염 공격
            WeaponBurnDeathExplosion.Attach(source, attack, null); // 실제 처치 폭발 연결
            source.TakeDamage(10f); // 실제 화상 대상 처치
            if (mode == 2) // 실제 일반 폭발 시험 확인
            { // 조건 시작
                Require(GameManager.Instance.IsPlaying && observed.CurrentHealth < 100f, "일반 전투 중 실제 화상 폭발 피해 누락"); // 실제 일반 폭발 유지 확인
            } // 조건 종료
            else // 실제 보스 승리 경계 확인
            { // 대체 시작
                Require(GameManager.Instance.CurrentState == GameState.Victory && !boss.IsAlive, "실제 보스 처치와 승리 실패"); // 실제 승리 확정 확인
                Require(Mathf.Approximately(observed.CurrentHealth, 100f), "승리 확정 후 화상 처치 폭발이 다른 적에게 피해 적용"); // 실제 후속 피해 차단 확인
            } // 대체 종료
            yield return null; // 실제 지연 파괴 완료 대기
            AutomatedDay106BurnDeathVictoryVerification.Finish(true, "Actual burn death explosion respected victory and normal combat."); // 실제 시험 결과
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
                AutomatedDay106BurnDeathVictoryVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
