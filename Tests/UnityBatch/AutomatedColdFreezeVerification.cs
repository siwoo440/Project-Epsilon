using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Reflection; // 실제 보스 생성 경로 조회
using ProjectEpsilon.Combat; // 실제 냉기와 전투 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 무기 속성
using ProjectEpsilon.Enemies; // 실제 적과 보스
using ProjectEpsilon.Player; // 실제 플레이어 구성
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedColdFreezeVerification // 실제 냉기 빙결 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonColdFreezeVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedColdFreezeVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 시작 감시
            } // 조건 끝
        } // 생성자 끝
        public static void Run() // 실제 씬 배치 진입
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        } // 메서드 끝
        private static void Tick() // 실제 시작과 결과 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            { // 조건 시작
                return; // 실제 진입 대기
            } // 조건 끝
            if (started <= 0d) // 실제 시험 시작 시각 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for cold freeze test."); // 시간 초과 실패
                return; // 감시 종료
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            { // 조건 시작
                return; // 실제 시작 또는 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 실제 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<ColdFreezeVerificationRunner>(); // 실제 냉기 시험 실행
        } // 메서드 끝
        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 시작 감시 해제
            Debug.Log($"[ColdFreezeVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        } // 메서드 끝
    } // 클래스 끝

    public sealed class ColdFreezeVerificationRunner : MonoBehaviour // 실제 냉기 상태 시나리오
    { // 클래스 시작
        private IEnumerator Start() // 실제 냉기 명중과 보스 저항
        { // 코루틴 시작
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            WeaponAttributeCombatEffects effects = UnityEngine.Object.FindFirstObjectByType<WeaponAttributeCombatEffects>(); // 실제 속성 명중 처리기
            Require(player != null && spawner != null && encounter != null && effects != null, "Actual cold freeze fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 플레이어 이동 통제
            spawner.enabled = false; // 실제 적 출현 통제
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 기존 적 순회
            { // 반복 시작
                existing.gameObject.SetActive(false); // 기존 적 접촉 차단
                Destroy(existing.gameObject); // 기존 적 제거
            } // 반복 끝
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 시험 중 자동 공격 방지
            } // 반복 끝
            WeaponTarget low = CreateTarget("ColdLowStage"); // 실제 낮은 단계 일반 적
            WeaponTarget normal = CreateTarget("ColdNormal"); // 실제 고단계 일반 적
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성 경로
            Require(spawn != null, "Actual boss creation path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 시험용 실제 보스 생성
            WeaponTarget boss = encounter.Boss; // 실제 생성 보스
            Require(boss != null && boss.IsAlive, "Actual boss target missing."); // 실제 보스 확인
            boss.GetComponent<EarlyEnemyBehaviour>().enabled = false; // 실제 보스 사격과 이동 통제
            boss.GetComponent<PrototypeBossChargeAttack>().enabled = false; // 실제 보스 돌진 통제
            WeaponTarget baseTarget = CreateTarget("ColdBaseStacks"); // 실제 기본 냉기 대조 대상
            Hit(effects, baseTarget, 0, 1); // 실제 기본 냉기 첫 명중
            WeaponTargetStatusController baseStatus = baseTarget.GetComponent<WeaponTargetStatusController>(); // 실제 기본 감속 상태
            Require(baseStatus != null && Mathf.Abs(baseStatus.CurrentMovementMultiplier - 0.9f) < 0.001f, "Actual base cold first stack was not ten percent."); // 실제 기본 첫 감속 확인
            Hit(effects, baseTarget, 0, 1); // 실제 기본 냉기 두 번째 명중
            Require(Mathf.Abs(baseStatus.CurrentMovementMultiplier - 0.8f) < 0.001f, "Actual base cold second stack was not twenty percent."); // 실제 기본 두 번째 감속 확인
            Hit(effects, baseTarget, 0, 1); // 실제 기본 냉기 세 번째 명중
            Require(Mathf.Abs(baseStatus.CurrentMovementMultiplier - 0.7f) < 0.001f, "Actual base cold third stack was not thirty percent."); // 실제 기본 세 번째 감속 확인
            Hit(effects, baseTarget, 0, 1); // 실제 기본 최대 누적 이후 명중
            Require(Mathf.Abs(baseStatus.CurrentMovementMultiplier - 0.7f) < 0.001f && !baseStatus.IsStopActive, "Actual base cold exceeded its cap or froze without synergy."); // 실제 기본 누적 상한 확인
            WeaponTarget boosted = CreateTarget("ColdBoostedStacks"); // 실제 시너지 냉기 대조 대상
            Hit(effects, boosted, 2, 1); // 실제 시너지 첫 명중
            WeaponTargetStatusController boostedStatus = boosted.GetComponent<WeaponTargetStatusController>(); // 실제 시너지 감속 상태
            Require(boostedStatus != null && Mathf.Abs(boostedStatus.CurrentMovementMultiplier - 0.885f) < 0.001f, "Actual cold synergy first stack mismatch."); // 실제 시너지 첫 감속 확인
            Hit(effects, boosted, 2, 1); // 실제 시너지 두 번째 명중
            Require(Mathf.Abs(boostedStatus.CurrentMovementMultiplier - 0.77f) < 0.001f, "Actual cold synergy second stack mismatch."); // 실제 시너지 두 번째 감속 확인
            Hit(effects, boosted, 2, 1); // 실제 시너지 세 번째 명중
            Require(Mathf.Abs(boostedStatus.CurrentMovementMultiplier - 0.655f) < 0.001f, "Actual cold synergy third stack mismatch."); // 실제 시너지 세 번째 감속 확인
            Hit(effects, boosted, 2, 1); // 실제 시너지 최대 누적 이후 명중
            Require(Mathf.Abs(boostedStatus.CurrentMovementMultiplier - 0.655f) < 0.001f && !boostedStatus.IsStopActive, "Actual cold synergy exceeded its cap or froze below stage six."); // 실제 시너지 누적 상한 확인
            Hit(effects, low, 2, 4); // 실제 낮은 시너지 냉기 명중
            WeaponTargetStatusController lowStatus = low.GetComponent<WeaponTargetStatusController>(); // 실제 낮은 단계 상태
            Require(lowStatus != null && !lowStatus.IsStopActive && lowStatus.CurrentMovementMultiplier > 0f && lowStatus.CurrentMovementMultiplier < 1f, "Low cold synergy unexpectedly froze or failed to slow a normal target."); // 실제 낮은 단계 감속 확인
            Require(Mathf.Abs(lowStatus.SlowRemaining - 3f) < 0.001f, "Actual stage-two cold duration mismatch."); // 실제 낮은 단계 지속 시간 확인
            Hit(effects, normal, 6, 3); // 실제 세 번의 냉기 누적
            WeaponTargetStatusController normalStatus = normal.GetComponent<WeaponTargetStatusController>(); // 실제 일반 적 상태
            WeaponTargetColdStatus normalCold = normal.GetComponent<WeaponTargetColdStatus>(); // 실제 일반 적 냉기 누적
            Require(normalStatus != null && normalCold != null && normalCold.StackCount == 3 && !normalStatus.IsStopActive, "Actual normal target froze before the additional hit."); // 실제 최대 누적 경계 확인
            Require(Mathf.Abs(normalStatus.SlowRemaining - 3.75f) < 0.001f, "Actual higher-stage cold duration did not increase by 25 percent."); // 실제 고단계 지속 시간 증가 확인
            Hit(effects, normal, 6, 1); // 실제 최대 누적 이후 추가 냉기 명중
            Require(normalStatus.IsStopActive && Mathf.Approximately(normalStatus.CurrentMovementMultiplier, 0f), "Actual normal target did not freeze after the additional cold hit."); // 실제 일반 적 빙결 확인
            Hit(effects, boss, 6, 4); // 실제 보스의 같은 냉기 누적과 추가 명중
            WeaponTargetStatusController bossStatus = boss.GetComponent<WeaponTargetStatusController>(); // 실제 보스 상태
            Require(bossStatus != null && !bossStatus.IsStopActive && bossStatus.CurrentMovementMultiplier > 0f && bossStatus.CurrentMovementMultiplier < 0.85f, "Actual boss was not strongly slowed instead of frozen."); // 실제 보스 빙결 저항 확인
            Require(Mathf.Abs(bossStatus.CurrentMovementMultiplier - 0.5f) < 0.001f, "Actual boss temporary strong-slow multiplier mismatch."); // 실제 보스 임시 감속 수치 확인
            FieldInfo additionalHits = typeof(WeaponAttributeCombatEffects).GetField("coldFreezeAdditionalHits", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 추가 명중 설정 조회
            FieldInfo freezeDuration = typeof(WeaponAttributeCombatEffects).GetField("coldFreezeDuration", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 빙결 시간 설정 조회
            Require(additionalHits != null && freezeDuration != null, "Actual adjustable cold freeze settings missing."); // 실제 조정 설정 확인
            additionalHits.SetValue(effects, 2); // 시험용 추가 명중 두 번
            freezeDuration.SetValue(effects, 0.2f); // 시험용 빙결 시간 설정
            WeaponTarget configured = CreateTarget("ColdConfigured"); // 실제 설정 대조 대상
            Hit(effects, configured, 6, 4); // 최대 누적과 첫 추가 명중
            WeaponTargetStatusController configuredStatus = configured.GetComponent<WeaponTargetStatusController>(); // 실제 설정 대조 상태
            Require(configuredStatus != null && !configuredStatus.IsStopActive, "Configured cold freeze ignored its additional-hit threshold."); // 실제 조정 명중 경계 확인
            Hit(effects, configured, 6, 1); // 실제 둘째 추가 명중
            Require(configuredStatus.IsStopActive && Mathf.Abs(configuredStatus.StopRemaining - 0.2f) < 0.001f, "Configured cold freeze duration or threshold did not apply."); // 실제 조정 빙결 시간 확인
            EarlyEnemyBehaviour bossMovement = boss.GetComponent<EarlyEnemyBehaviour>(); // 실제 보스 이동 행동
            FieldInfo nextAttack = typeof(EarlyEnemyBehaviour).GetField("nextAttack", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 사격 시각 조회
            Rigidbody2D bossBody = boss.GetComponent<Rigidbody2D>(); // 실제 보스 물리체
            Require(bossMovement != null && nextAttack != null && bossBody != null, "Actual boss movement fixtures missing."); // 실제 이동 구성 확인
            boss.transform.position = player.transform.position + Vector3.up * 10f; // 실제 보스 추적 시작 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            nextAttack.SetValue(bossMovement, float.MaxValue); // 시험 중 실제 사격 방지
            float beforeMove = bossBody.position.y; // 실제 이동 전 세로 위치
            bossMovement.enabled = true; // 실제 보스 추적 이동 허용
            yield return new WaitForFixedUpdate(); // 실제 물리 이동 진행
            yield return null; // 실제 물리 위치 갱신 대기
            float movedDistance = beforeMove - bossBody.position.y; // 실제 감속 상태 이동 거리
            Require(movedDistance > 0f && bossStatus.CurrentMovementMultiplier > 0f && bossStatus.CurrentMovementMultiplier < 0.85f, "Cold-resistant boss did not actually move while strongly slowed."); // 실제 보스 감속 이동 확인
            bossMovement.enabled = false; // 이후 정지 시간 시험 통제
            float frozenRemaining = normalStatus.StopRemaining; // 실제 일반 적 빙결 남은 시간
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.25f); // 실제 정지 시간 대기
            Require(normalStatus.IsStopActive && Mathf.Abs(normalStatus.StopRemaining - frozenRemaining) < 0.001f && bossStatus.CurrentMovementMultiplier < 0.85f, "Actual cold states advanced during pause."); // 실제 정지 중 냉기 유지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            yield return new WaitForSeconds(1.25f); // 임시 빙결 지속 시간 경과
            Require(!normalStatus.IsStopActive && normalStatus.CurrentMovementMultiplier > 0f && normalStatus.CurrentMovementMultiplier < 1f, "Normal freeze did not expire into its existing slow."); // 실제 빙결 후 감속 복귀
            Require(!bossStatus.IsStopActive && Mathf.Abs(bossStatus.CurrentMovementMultiplier - 0.655f) < 0.001f, "Boss strong slow did not expire into its existing slow."); // 실제 보스 강한 감속 종료 확인
            Debug.Log($"[ColdFreezeVerification] actual cold hits: stage2 slow only; stage6 three stacks plus one additional hit froze ordinary target and strongly slowed actual boss; boss moved {movedDistance:F4}; configurable threshold and duration, pause and expiry passed."); // 실제 통제 조건과 결과 기록
            AutomatedColdFreezeVerification.Finish(0, "Actual cold freeze, boss resistance, pause and expiry passed."); // 실제 전체 결과
        } // 코루틴 끝
        private static WeaponTarget CreateTarget(string name) // 실제 일반 전투 대상 생성
        { // 메서드 시작
            GameObject item = new GameObject(name); // 실제 일반 적 대체 객체
            item.transform.position = Vector3.right * 100f; // 실제 플레이어 접촉 밖 위치
            WeaponTarget target = item.AddComponent<WeaponTarget>(); // 실제 무기 공격 대상
            target.Configure(100f); // 실제 체력 설정
            return target; // 실제 전투 대상 반환
        } // 메서드 끝
        private static void Hit(WeaponAttributeCombatEffects effects, WeaponTarget target, int stage, int count) // 실제 냉기 명중 경로 실행
        { // 메서드 시작
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(null, WeaponAttribute.Cold, stage, stage, 1, Vector3.zero, 0f); // 실제 냉기 명중 정보
            for (int index = 0; index < count; index++) // 실제 냉기 명중 반복
            { // 반복 시작
                effects.ApplyHit(attack, target, target.transform.position); // 실제 속성 명중 처리
            } // 반복 끝
        } // 메서드 끝
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedColdFreezeVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
