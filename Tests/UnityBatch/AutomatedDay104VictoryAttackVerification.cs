using System; // 시험 실패 처리
using System.Collections; // 프레임 대기
using System.Reflection; // 실제 자동 공격 호출
using ProjectEpsilon.Combat; // 실제 포탑과 적
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 포탑 데이터
using ProjectEpsilon.Enemies; // 자연 적 생성기
using ProjectEpsilon.Player; // 실제 플레이어와 몸통
using UnityEditor; // 배치 실행과 포탑 데이터
using UnityEditor.SceneManagement; // 실제 게임 씬
using UnityEngine; // 엔진 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay104VictoryAttackVerification // 승리 중 후속 공격 시험
    { // 클래스 시작
        private const string Key = "EpsilonDay104VictoryAttack"; // 시험 상태 표식
        private static bool started; // 중복 실행 방지
        static AutomatedDay104VictoryAttackVerification() // 시험 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 시험 진행 여부
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 전투 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 진입점
        { // 함수 시작
            SessionState.SetBool("EpsilonDay104Multi", false); // 두 포탑 시험 설정
            SessionState.SetBool("EpsilonDay104Hook", false); // 일반 직접 공격 시험 설정
            Begin(); // 실제 시험 시작
        } // 함수 종료
        public static void RunMulti() // 다중 대상 공격 배치 진입점
        { // 함수 시작
            SessionState.SetBool("EpsilonDay104Multi", true); // 한 포탑 다중 대상 시험 설정
            SessionState.SetBool("EpsilonDay104Hook", false); // 일반 직접 공격 시험 설정
            Begin(); // 실제 시험 시작
        } // 함수 종료
        public static void RunHook() // 공격 후 등급 효과 배치 진입점
        { // 함수 시작
            SessionState.SetBool("EpsilonDay104Multi", false); // 실제 두 포탑 시험 설정
            SessionState.SetBool("EpsilonDay104Hook", true); // 실제 등급 효과 승리 시험 설정
            Begin(); // 실제 시험 시작
        } // 함수 종료
        private static void Begin() // 실제 게임 씬 시험 준비
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
            new GameObject("Day104VictoryAttackRunner").AddComponent<Day104VictoryAttackRunner>(); // 실제 공격 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day104VictoryAttackVerification] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 결과 기록
            EditorApplication.Exit(passed ? 0 : 1); // 배치 종료
        } // 함수 종료
    } // 클래스 종료
    public sealed class Day104VictoryAttackRunner : MonoBehaviour // 실제 포탑 공격 시험 실행기
    { // 클래스 시작
        private IEnumerator Start() // 실제 보스 승리와 후속 공격 확인
        { // 함수 시작
            SnakeMovement player = FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 이동
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SnakeWeaponManager weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 관리자
            EarlyEnemyWaveSpawner spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            Require(player != null && body != null && weapons != null && spawner != null && weapons.Slots.Count >= 2, "실제 플레이어·두 포탑 구성 누락"); // 실제 시험 구성 확인
            player.enabled = false; // 시험 중 머리 이동 중지
            SnakeBodyFollower follower = body.GetComponent<SnakeBodyFollower>(); // 실제 몸통 추적기
            if (follower != null) // 실제 추적기 확인
            { // 조건 시작
                follower.enabled = false; // 시험 중 몸통 이동 중지
            } // 조건 종료
            spawner.enabled = false; // 자연 적 출현 중지
            weapons.enabled = false; // 시험 준비 중 자동 공격 중지
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 장착 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 기존 포탑 간섭 제거
            } // 반복 종료
            foreach (WeaponTarget target in FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None)) // 기존 실제 적 순회
            { // 반복 시작
                target.gameObject.SetActive(false); // 기존 적의 조준 간섭 제거
            } // 반복 종료
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/DebugBlade.asset"); // 실제 근접 포탑 데이터
            bool multi = SessionState.GetBool("EpsilonDay104Multi", false); // 실제 다중 대상 시험 여부
            bool hook = SessionState.GetBool("EpsilonDay104Hook", false); // 실제 등급 효과 시험 여부
            Require(weapon != null && weapons.TryEquipAt(0, weapon, multi ? 3 : 1) && (multi || weapons.TryEquipAt(1, weapon)), "실제 시험 포탑 장착 실패"); // 실제 포탑 장착 확인
            Vector3 firstOrigin = new Vector3(500f, 500f, 0f); // 실제 첫 몸통 위치
            Vector3 secondOrigin = firstOrigin + Vector3.right * (multi ? 1f : 20f); // 실제 둘째 대상의 기준 위치
            weapons.Slots[0].Origin.position = firstOrigin; // 실제 첫 몸통 발사 위치
            weapons.Slots[1].Origin.position = secondOrigin; // 실제 둘째 몸통 발사 위치
            WeaponTarget boss = CreateTarget("Day104Boss", firstOrigin + Vector3.right * (hook ? 10f : 1f), 1f); // 실제 시험 보스
            WeaponTarget other = CreateTarget("Day104Other", secondOrigin + Vector3.right, 100f); // 실제 둘째 포탑의 최근접 적
            Require(GameManager.Instance.RegisterFinalBoss(boss), "실제 최종 보스 등록 실패"); // 실제 승리 조건 연결
            if (hook) // 실제 공격 후 등급 효과 시험 확인
            { // 조건 시작
                CreateTarget("Day104FirstTarget", firstOrigin + Vector3.right * 0.5f, 100f); // 첫 포탑의 실제 최근접 적
                WeaponGradeEffectHooks hooks = weapons.GetComponent<WeaponGradeEffectHooks>(); // 실제 등급 효과 알림기
                Require(hooks != null, "실제 등급 효과 알림기 누락"); // 실제 이벤트 구성 확인
                hooks.EffectTriggered += context => // 실제 공격 후 등급 효과 구독
                { // 알림 시작
                    if (context.Trigger == WeaponGradeEffectTrigger.Attack && boss != null && boss.IsAlive) // 실제 공격과 보스 생존 확인
                    { // 조건 시작
                        boss.TakeDamage(boss.MaximumHealth); // 실제 등급 효과 중 최종 보스 처치
                    } // 조건 종료
                }; // 알림 종료
            } // 조건 종료
            weapons.enabled = true; // 실제 포탑 공격 활성화
            typeof(SnakeWeaponManager).GetMethod("TickAutoAttack", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(weapons, null); // 실제 한 프레임 포탑 공격
            Require(GameManager.Instance.CurrentState == GameState.Victory && !boss.IsAlive, "첫 포탑의 실제 보스 처치와 승리 실패"); // 실제 승리 확정 확인
            Require(Mathf.Approximately(other.CurrentHealth, 100f), "승리 확정 후 다른 적에게 후속 공격 적용"); // 실제 승리 후 추가 피해 차단 확인
            yield return null; // 실제 지연 파괴 완료 대기
            AutomatedDay104VictoryAttackVerification.Finish(true, "Actual nearest boss kill stopped later turret or multi-target attacks."); // 실제 시험 결과
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
                AutomatedDay104VictoryAttackVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
