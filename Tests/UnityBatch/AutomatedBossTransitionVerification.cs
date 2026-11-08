using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 제품 상태 조회
using ProjectEpsilon.Combat; // 실제 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적 출현과 투사체
using ProjectEpsilon.Player; // 플레이어 구성
using ProjectEpsilon.Progression; // 실제 보석과 회복 아이템
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossTransitionVerification // 실제 보스 전환 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonBossTransitionVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 시험 시작 시각

        static AutomatedBossTransitionVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 진행 시험 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 플레이 시작 감시
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // Unity 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 플레이 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 시작
        } // 메서드 끝

        private static void Tick() // 실제 시작 화면과 시험 실행 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (started <= 0d) // 최초 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시험 시작 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 180d) // 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for boss transition."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 시작 선택과 실행기 확인
            { // 조건 시작
                return; // 실제 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossTransitionVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[BossTransitionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class BossTransitionVerificationRunner : MonoBehaviour // 실제 전환 시나리오
    { // 클래스 시작
        private EarlyEnemyWaveSpawner spawner; // 실제 출현기
        private FieldInfo elapsedField; // 실제 출현 시간 필드
        private PropertyInfo transitioned; // 실제 전환 상태
        private int transitions; // 실제 전환 알림 수

        private IEnumerator Start() // 실제 누적 시간과 전환 관찰
        { // 코루틴 시작
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기 조회
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 머리 조회
            SnakeBodyManager body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 조회
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 체력 조회
            Require(spawner != null && player != null && body != null && health != null, "Missing actual scene fixtures."); // 실제 씬 구성 확인
            elapsedField = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 누적 시간 조회
            transitioned = typeof(EarlyEnemyWaveSpawner).GetProperty("HasEnteredBossTransition"); // 실제 전환 상태 조회
            EventInfo transitionEvent = typeof(EarlyEnemyWaveSpawner).GetEvent("BossTransitionStarted"); // 실제 전환 이벤트 조회
            Require(elapsedField != null && transitioned != null && transitionEvent != null, "Boss transition clock, state, or event missing."); // 실제 제품 계약 확인
            transitionEvent.AddEventHandler(spawner, new Action(ObserveTransition)); // 실제 전환 이벤트 구독
            player.enabled = false; // 플레이어 이동 통제
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            if (follower != null) // 몸통 추적 확인
            { // 조건 시작
                follower.enabled = false; // 몸통 이동 통제
            } // 조건 끝
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌 조회
            if (self != null) // 자기 충돌 확인
            { // 조건 시작
                self.enabled = false; // 시험 중 자기 충돌 방지
            } // 조건 끝
            if (external != null) // 외부 충돌 확인
            { // 조건 시작
                external.enabled = false; // 시험 중 외부 충돌 방지
            } // 조건 끝
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 조회
            var startingWeapon = weapons.Slots[0].Weapon; // 실제 시작 무기 보존
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 장착 무기 순회
            { // 반복 시작
                slot.Clear(); // 시험 중 자동 처치 방지
            } // 반복 끝
            Time.timeScale = 32f; // 실제 게임 시간 가속
            float deadline = Time.realtimeSinceStartup + 140f; // 실제 시험 제한 시각
            while (Elapsed() < 1190f && Time.realtimeSinceStartup < deadline) // 실제 누적 경계 대기
            { // 반복 시작
                DisableEnemyActions(); // 자연 적 행동 통제
                yield return null; // 실제 게임 프레임 진행
            } // 반복 끝
            Require(Elapsed() >= 1190f && !(bool)transitioned.GetValue(spawner) && transitions == 0, "Transition began before 1200 seconds or clock did not advance."); // 전환 전 실제 상태 확인
            Vector3 distant = player.transform.position + Vector3.right * 2000f; // 자동 수집 밖 위치
            ExperiencePickup gem = ExperiencePickup.Spawn(distant, 5, null, null); // 실제 보존 경험치 보석
            RecoveryPickup heal = RecoveryPickup.Spawn(distant, player.transform, health, body, RecoveryPickupType.Heal, 15, null); // 실제 보존 회복 아이템
            RecoveryPickup repair = RecoveryPickup.Spawn(distant, player.transform, health, body, RecoveryPickupType.BodyRepair, 1, null); // 실제 제거 몸통 복구
            GameObject shotObject = new GameObject("TransitionTestProjectile"); // 실제 적 투사체 객체
            shotObject.transform.position = distant; // 실제 투사체 위치
            EnemyTestProjectile shot = shotObject.AddComponent<EnemyTestProjectile>(); // 실제 적 투사체 추가
            shot.Configure(Vector2.right, 0f, 100f, 8, health, body, player, null); // 실제 투사체 구성
            int before = spawner.ActiveEnemies; // 전환 전 등록 적 수
            EliteRewardController rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 보상 조회
            int pendingRewards = rewards == null ? 0 : rewards.PendingRewardCount; // 기존 보상 대기 수
            int gemCount = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 기존 경험치 보석 수
            Require(before > 0, "No actual spawned enemies available for cleanup."); // 실제 정리 대상 확인
            while (Elapsed() < 1198.5f && Time.realtimeSinceStartup < deadline) // 합성 경계 직전 대기
            { // 반복 시작
                DisableEnemyActions(); // 자연 적 행동 통제
                yield return null; // 실제 게임 시간 진행
            } // 반복 끝
            Require(Elapsed() < 1200f && startingWeapon != null && weapons.TryEquipAt(0, startingWeapon, 1) && weapons.TryEquipAt(1, startingWeapon, 1), "Could not prepare actual merge materials before transition."); // 실제 미확정 합성 재료 준비
            SnakeWeaponMergeController merge = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponMergeController>(); // 실제 합성 화면 조회
            Require(merge != null && merge.OpenMergePanel() && merge.IsMergeOpen && Mathf.Approximately(Time.timeScale, 0.7f), "Actual merge did not open at 70 percent speed."); // 실제 합성과 속도 확인
            while (!(bool)transitioned.GetValue(spawner) && Time.realtimeSinceStartup < deadline) // 실제 20분 전환 대기
            { // 반복 시작
                DisableEnemyActions(); // 자연 적 행동 통제
                yield return null; // 실제 게임 시간 진행
            } // 반복 끝
            yield return null; // 실제 지연 파괴 완료 대기
            Require(Elapsed() >= 1200f && transitions == 1 && spawner.ActiveEnemies == 0, "Transition did not occur once or managed enemies remain."); // 실제 전환과 무보상 제거 확인
            Require(gem != null && heal != null && repair == null && shot == null, "Transition cleanup or pickup preservation failed."); // 실제 제거와 보존 확인
            Require((rewards == null || rewards.PendingRewardCount == pendingRewards) && UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == gemCount, "Cleanup incorrectly created experience or elite rewards."); // 정리 무보상 확인
            Require(!merge.IsMergeOpen && weapons.OccupiedSlotCount == 2 && weapons.Slots[0].Weapon == startingWeapon && weapons.Slots[1].Weapon == startingWeapon && weapons.Slots[0].Grade == 1 && weapons.Slots[1].Grade == 1, "Pending merge or its materials were not preserved at transition."); // 미확정 합성과 실제 재료 보존 확인
            Require(merge.OpenMergePanel() && merge.IsMergeOpen, "Pending merge could not be opened again after transition."); // 보스 전환 후 합성 재진입 확인
            merge.CloseMergePanel(); // 검증 후 합성 화면 닫기
            yield return null; // 중복 전환 확인 프레임
            Require(transitions == 1 && spawner.ActiveEnemies == 0 && GameManager.Instance.IsPlaying, "Transition repeated, waves resumed, or play stopped."); // 실제 전환 후 진행 확인
            Type encounterType = typeof(EarlyEnemyWaveSpawner).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossEncounter"); // 실제 시험 보스 형식 조회
            Component encounter = encounterType == null ? null : spawner.GetComponent(encounterType); // 실제 씬 보스 출현기 조회
            PropertyInfo bossProperty = encounterType == null ? null : encounterType.GetProperty("Boss"); // 실제 보스 대상 조회
            PropertyInfo phaseProperty = encounterType == null ? null : encounterType.GetProperty("IsPhaseTwo"); // 실제 보스 단계 조회
            Require(encounter != null && bossProperty != null && phaseProperty != null, "Actual scene boss encounter is missing."); // 실제 보스 구성 확인
            WeaponTarget boss = bossProperty.GetValue(encounter) as WeaponTarget; // 실제 출현 보스
            Require(boss != null && boss.IsAlive && !(bool)phaseProperty.GetValue(encounter), "Actual live first-phase boss did not spawn."); // 실제 보스 생존 확인
            boss.TakeDamage(boss.MaximumHealth * 0.5f); // 실제 보스 피해 적용
            yield return null; // 실제 단계 관찰 프레임
            Require(boss.IsAlive && (bool)phaseProperty.GetValue(encounter) && GameManager.Instance.IsPlaying, "Actual surviving boss did not enter phase two without victory."); // 실제 절반 체력 단계 확인
            boss.TakeDamage(boss.MaximumHealth); // 실제 보스 치명 피해 적용
            Require(GameManager.Instance.CurrentState.ToString() == "Victory" && Time.timeScale == 0f, "Actual boss death did not trigger victory and time stop."); // 실제 보스 사망 승리 확인
            Debug.Log($"[BossTransitionVerification] elapsed={Elapsed():F3}, initial managed enemies={before}, transitions={transitions}, XP and healing preserved, body repair and enemy projectile removed, actual boss phase two and death victory."); // 실제 경계 결과 기록
            AutomatedBossTransitionVerification.Finish(0, "Actual accelerated 0-to-1200-second transition, cleanup and forced boss victory passed."); // 실제 전체 결과
        } // 코루틴 끝

        private float Elapsed() // 실제 출현 경과 시간
        { // 메서드 시작
            return (float)elapsedField.GetValue(spawner); // 실제 누적 시간 반환
        } // 메서드 끝

        private void ObserveTransition() // 실제 전환 알림 기록
        { // 메서드 시작
            transitions++; // 전환 횟수 증가
        } // 메서드 끝

        private static void DisableEnemyActions() // 실제 자연 적 행동 통제
        { // 메서드 시작
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 적 순회
            { // 반복 시작
                enemy.enabled = false; // 피해와 이동 방지
            } // 반복 끝
        } // 메서드 끝

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedBossTransitionVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
