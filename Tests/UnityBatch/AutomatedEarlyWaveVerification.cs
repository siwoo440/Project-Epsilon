using System; // 기본 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 시험 기록 목록
using System.Reflection; // 임시 시험 설정 조회
using ProjectEpsilon.Combat; // 무기와 공격 대상
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Enemies; // 자연 적 출현 제어
using ProjectEpsilon.Player; // 몸통과 이동
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 보상 버튼
using ProjectEpsilon.UI; // 실제 레벨업 화면
using ProjectEpsilon.Progression; // 실제 보석과 보상

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedEarlyWaveVerification // 초반 출현과 엘리트 실제 씬 검증
    {
        private const string ActiveKey = "EpsilonEarlyWaveVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedEarlyWaveVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            {
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 플레이 시작 감시
            }
        }

        public static void Run() // Unity 배치 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 플레이 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 시작 선택과 플레이 진입 감시
        {
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            {
                return; // 진입 대기
            }
            if (started <= 0d) // 최초 갱신 확인
            {
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            {
                Finish(1, "Timed out waiting for controlled early-wave test."); // 제한 시간 실패
                return; // 감시 중단
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 무기 첫 후보 선택
            {
                return; // 실제 게임 시작 대기
            }
            if (injected) // 시험 실행기 존재 확인
            {
                return; // 시험 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 초기화 대기
            }
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<EarlyWaveVerificationRunner>(); // 실제 씬 시험 실행
        }

        public static void Finish(int code, string message) // 결과 기록과 프로세스 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[EarlyWaveVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }

    public sealed class EarlyWaveVerificationRunner : MonoBehaviour // 실제 출현과 보상 연결 시험
    {
        private static readonly BindingFlags PrivateFields = BindingFlags.NonPublic | BindingFlags.Instance; // 실제 씬 설정 조회
        private readonly HashSet<int> observedIds = new HashSet<int>(); // 실제 출현 고유 번호
        private readonly int[] counts = new int[4]; // 실제 적 종류별 출현 수
        private readonly float[] firstTimes = { -1f, -1f, -1f, -1f }; // 실제 종류별 첫 출현 시간
        private EarlyEnemyWaveSpawner spawner; // 실제 게임 출현기
        private SnakeMovement player; // 실제 플레이어 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeHealth health; // 실제 공용 체력
        private SnakeExperience experience; // 실제 경험치 수신자
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private SnakeLevelUpController level; // 실제 레벨업 처리기
        private EliteRewardController eliteReward; // 실제 엘리트 보상 처리기
        private LevelUpPanelController panel; // 실제 공용 보상 패널
        private WeaponTarget elite; // 자동 출현한 실제 엘리트
        private FieldInfo elapsedField; // 출현기 실제 경과 시간
        private FieldInfo valueField; // 실제 보석 경험치 값

        private IEnumerator Start() // 실제 씬 출현 및 보상 검증
        {
            Prepare(); // 실제 씬과 통제 조건 준비
            float realStart = Time.realtimeSinceStartup; // 가속 시험 실제 시작 시각
            while (elite == null && Elapsed() < 245f && Time.realtimeSinceStartup - realStart < 120f) // 실제 4분 출현 대기
            {
                ObserveWaves(); // 실제 자동 출현 관찰과 통제
                yield return null; // 실제 Update 프레임 진행
            }
            ObserveWaves(); // 마지막 출현 프레임 관찰
            Require(elite != null && counts[3] == 1 && firstTimes[3] >= 240f && firstTimes[3] <= 240.5f, "Actual elite did not spawn once at the four-minute boundary."); // 실제 4분 출현 확인
            Require(counts[0] > 0 && counts[1] > 0 && counts[2] > 0 && firstTimes[0] < 60f && firstTimes[1] >= 60f && firstTimes[2] >= 120f, "Actual three early enemy kinds were not observed in their unlocked periods."); // 실제 종류별 해금과 출현 확인
            Debug.Log($"[EarlyWaveVerification] actual spawner elapsed 0->{Elapsed():F3}; first chaser={firstTimes[0]:F3}, predator={firstTimes[1]:F3}, shooter={firstTimes[2]:F3}, elite={firstTimes[3]:F3}; counts={counts[0]}/{counts[1]}/{counts[2]}/{counts[3]}, actualSeconds={Time.realtimeSinceStartup - realStart:F2}."); // 실제 출현과 가속 시간 기록
            float observeUntil = Elapsed() + 3.2f; // 중복 엘리트 감시 시간
            while (Elapsed() < observeUntil) // 실제 추가 출현 간격 진행
            {
                ObserveWaves(); // 실제 자동 출현 관찰
                yield return null; // 실제 게임 프레임 진행
            }
            Require(counts[3] == 1 && GameManager.Instance.IsPlaying, "Elite duplicated or gameplay stopped before controlled defeat."); // 실제 단회 출현 확인
            Time.timeScale = 1f; // 보상 경합 전 정상 시험 속도
            yield return VerifyEliteCharge(); // 실제 예고와 돌진 전환 확인
            Require(health.TakeDamage(health.MaximumHealth + 1), "Could not prepare controlled repair and heal reward availability."); // 회복 후보 준비용 통제 피해
            Require(body.CurrentBodyCount == 2 && body.EarnedBodyCount == 3 && health.CurrentHealth == health.MaximumHealth - 1, $"Controlled pre-reward fixture mismatch: body={body.CurrentBodyCount}, earned={body.EarnedBodyCount}, health={health.CurrentHealth}/{health.MaximumHealth}."); // 보상 전 몸통과 체력 확인
            Collider2D eliteCollider = elite.GetComponent<Collider2D>(); // 실제 엘리트 충돌체 조회
            if (eliteCollider != null) // 실제 충돌체 존재 확인
            {
                eliteCollider.enabled = false; // 강제 처치 전 접촉 방지
            }
            EnemyDropController dropController = elite.GetComponent<EnemyDropController>(); // 실제 엘리트 보석 드롭 처리기
            FieldInfo healChance = typeof(EnemyDropController).GetField("healDropChance", PrivateFields); // 실제 회복 드롭 확률 필드
            FieldInfo repairChance = typeof(EnemyDropController).GetField("bodyRepairDropChance", PrivateFields); // 실제 몸통 드롭 확률 필드
            Require(dropController != null && healChance != null && repairChance != null, "Actual elite drop configuration is missing."); // 실제 드롭 구성 확인
            healChance.SetValue(dropController, 0f); // 경합 시험 중 임의 체력 보석 차단
            repairChance.SetValue(dropController, 0f); // 경합 시험 중 임의 몸통 보석 차단
            elite.transform.position = experience.transform.position; // 실제 대형 보석 수집 위치 통제
            Require(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 0, "Unexpected preexisting gem prevents isolated reward accounting."); // 실제 시험 전 보석 부재 확인
            int deathEvents = 0; // 실제 엘리트 사망 횟수
            elite.Died += ignored => // 실제 엘리트 사망 관찰
            {
                deathEvents++; // 실제 사망 이벤트 횟수 증가
            };
            elite.TakeDamage(10000f); // 실제 자동 출현 엘리트 강제 처치
            ExperiencePickup[] dropped = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 드롭된 활성 보석
            Require(deathEvents == 1 && dropped.Length == 1 && ReadPickupValue(dropped[0]) == 20 && eliteReward.PendingRewardCount == 1, "Actual elite death did not create exactly one 20-XP gem and one queued reward."); // 실제 사망과 별도 보상 확인
            Debug.Log("[EarlyWaveVerification] forced actual elite death: exactly one Died event, one actual 20-XP gem and one pending elite reward; random recovery and repair drops disabled for this fixture."); // 강제 처치와 실제 드롭 기록
            float collectDeadline = Time.realtimeSinceStartup + 5f; // 실제 보석 수집 제한
            while (!experience.IsLevelUpPending && Time.realtimeSinceStartup < collectDeadline) // 실제 수집과 레벨업 대기
            {
                yield return null; // 실제 보석 Update 진행
            }
            RequireLevel(2, 10, 3, 4); // 첫 실제 레벨업 및 잔여 XP
            Require(eliteReward.PendingRewardCount == 1 && !eliteReward.IsPresentingReward, "Elite reward was not deferred behind the first level-up."); // 실제 엘리트 보상 대기 확인
            if (dropped[0] != null) // 같은 프레임 지연 파괴 확인
            {
                FieldInfo collectedField = typeof(ExperiencePickup).GetField("collected", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 획득 상태 조회
                FieldInfo destructionField = typeof(ExperiencePickup).GetField("destructionScheduled", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 파괴 예약 상태 조회
                Require(collectedField != null && destructionField != null && (bool)collectedField.GetValue(dropped[0]) && (bool)destructionField.GetValue(dropped[0]), "The still-present elite gem was not collected and scheduled for destruction."); // 실제 중복 획득 방지 확인
                Debug.Log("[EarlyWaveVerification] collected elite gem is already consumed and destruction is scheduled in the same frame."); // 실제 지연 파괴 상태 기록
                yield return null; // 실제 프레임 종료의 지연 파괴 대기
            }
            Require(UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 0, "The actual elite gem remained active after collection."); // 보석 한 번 수집 확인
            Debug.Log("[EarlyWaveVerification] actual 20-XP gem collected: level=2, remaining XP=10, first level reward open, elite reward pending=1."); // 실제 첫 보상 순서 기록
            CompleteLevelReward(); // 실제 버튼으로 첫 레벨 보상 완료
            RequireLevel(3, 0, 4, 5); // 연속 둘째 레벨과 정확한 경험치 소모
            Require(eliteReward.PendingRewardCount == 1 && !eliteReward.IsPresentingReward, "Elite reward interrupted the consecutive level-up."); // 연속 레벨 중 엘리트 대기 확인
            Debug.Log("[EarlyWaveVerification] actual first UI reward consumed once: level=3, remaining XP=0, second level reward open, elite reward pending=1."); // 두 번째 보상 순서 기록
            CompleteLevelReward(); // 실제 버튼으로 둘째 레벨 보상 완료
            Require(!experience.IsLevelUpPending && !level.IsPresentingLevelUp, "Second level reward did not complete exactly once."); // 연속 레벨 완료 확인
            float rewardDeadline = Time.realtimeSinceStartup + 5f; // 엘리트 화면 대기 제한
            while (!eliteReward.IsPresentingReward && Time.realtimeSinceStartup < rewardDeadline) // 실제 엘리트 보상 표시 대기
            {
                yield return null; // 실제 보상 Update 진행
            }
            Require(eliteReward.IsPresentingReward && eliteReward.PendingRewardCount == 1 && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && panel.gameObject.activeInHierarchy, "Actual elite reward did not present after consecutive levels."); // 실제 엘리트 보상 및 정지 확인
            Require(eliteReward.Candidates.Count == 3 && eliteReward.Candidates[0] != eliteReward.Candidates[1] && eliteReward.Candidates[1] != eliteReward.Candidates[2] && eliteReward.Candidates[0] != eliteReward.Candidates[2], "Actual elite reward did not present three distinct candidates."); // 실제 서로 다른 후보 세 개
            ClickUsableEliteReward(); // 실제 공용 UI 버튼으로 한 보상 선택
            Require(eliteReward.PendingRewardCount == 0 && !eliteReward.IsPresentingReward && !experience.IsLevelUpPending && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f), "Actual elite reward was not consumed once or gameplay did not resume."); // 실제 보상 소비와 시간 복구 확인
            yield return null; // 실제 다음 프레임 관찰
            Require(eliteReward.PendingRewardCount == 0 && !eliteReward.IsPresentingReward && experience.CurrentLevel == 3 && experience.CurrentExperience == 0 && GameManager.Instance.IsPlaying, "An extra reward or level-up appeared after completion."); // 중복 보상과 레벨업 방지
            AutomatedEarlyWaveVerification.Finish(0, "Actual accelerated four-minute spawn, warned charge, forced elite death, 20-XP gem, two level rewards and one elite reward completed under controlled conditions."); // 실제 통제 시험 성공
        }

        private IEnumerator VerifyEliteCharge() // 실제 엘리트 예고와 돌진 확인
        {
            spawner.enabled = false; // 추가 일반 적 출현 차단
            EarlyEnemyBehaviour behaviour = elite.GetComponent<EarlyEnemyBehaviour>(); // 실제 출현 엘리트 행동
            FieldInfo phase = typeof(EarlyEnemyBehaviour).GetField("chargePhase", PrivateFields); // 실제 돌진 단계 필드
            FieldInfo direction = typeof(EarlyEnemyBehaviour).GetField("chargeDirection", PrivateFields); // 실제 고정 방향 필드
            FieldInfo next = typeof(EarlyEnemyBehaviour).GetField("nextAttack", PrivateFields); // 실제 다음 공격 시각
            Require(behaviour != null && phase != null && direction != null && next != null, "Elite charge fields are missing."); // 실제 공격 구성 확인
            elite.transform.position = player.transform.position + Vector3.left * 10f; // 실제 충돌 밖 출현 위치
            Physics2D.SyncTransforms(); // 물리 위치 동기화
            next.SetValue(behaviour, Time.time); // 공격 주기 시작 시각 통제
            behaviour.enabled = true; // 실제 엘리트 행동 재개
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 예고 대기 제한
            while ((int)phase.GetValue(behaviour) != 1 && Time.realtimeSinceStartup < deadline) // 예고 단계 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 물리 단계 진행
            }
            LineRenderer warning = elite.GetComponent<LineRenderer>(); // 실제 예고선 조회
            Require((int)phase.GetValue(behaviour) == 1 && warning != null && warning.enabled, "Elite warning did not start."); // 예고 단계와 선 확인
            Vector2 locked = (Vector2)direction.GetValue(behaviour); // 실제 고정 방향 저장
            player.transform.position += Vector3.up * 4f; // 예고 후 목표 위치 변경
            Physics2D.SyncTransforms(); // 변경된 물리 위치 반영
            deadline = Time.realtimeSinceStartup + 5f; // 실제 돌진 대기 제한
            while ((int)phase.GetValue(behaviour) == 1 && Time.realtimeSinceStartup < deadline) // 예고 종료 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 물리 단계 진행
            }
            Require((int)phase.GetValue(behaviour) == 2 && !warning.enabled && Vector2.Dot(locked, (Vector2)direction.GetValue(behaviour)) > 0.99f, "Elite did not charge in the warned direction."); // 실제 돌진 전환과 방향 고정 확인
            Vector2 start = elite.transform.position; // 실제 돌진 시작 위치
            for (int index = 0; index < 5; index++) // 실제 돌진 물리 단계 반복
            {
                yield return new WaitForFixedUpdate(); // 실제 돌진 이동 대기
            }
            Vector2 movement = (Vector2)elite.transform.position - start; // 실제 돌진 이동량
            Require(movement.magnitude > 0.01f && Vector2.Dot(movement.normalized, locked) > 0.95f, "Elite charge did not move in its locked direction."); // 실제 이동 거리와 방향 확인
            deadline = Time.realtimeSinceStartup + 5f; // 실제 돌진 종료 제한
            while ((int)phase.GetValue(behaviour) == 2 && Time.realtimeSinceStartup < deadline) // 실제 돌진 종료 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 물리 단계 진행
            }
            Require((int)phase.GetValue(behaviour) == 0, "Elite charge did not finish."); // 실제 대기 단계 복귀 확인
            behaviour.enabled = false; // 강제 처치 전 엘리트 이동 정지
            Debug.Log("[EarlyWaveVerification] controlled elite attack: warning, locked direction, movement and recovery observed."); // 실제 통제 공격 기록
        }

        private void Prepare() // 실제 씬 확인과 시간 가속 통제
        {
            player = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            experience = GetComponent<SnakeExperience>(); // 실제 경험치 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업 처리기 조회
            eliteReward = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 보상 처리기 조회
            panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 공용 보상 패널 조회
            Require(player != null && body != null && health != null && experience != null && weapons != null && spawner != null && level != null && eliteReward != null && panel != null && GameManager.Instance.IsPlaying, "Actual Game scene fixtures are missing."); // 필수 씬 구성 확인
            elapsedField = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", PrivateFields); // 실제 출현 경과 시간 필드
            valueField = typeof(ExperiencePickup).GetField("experienceValue", PrivateFields); // 실제 보석 값 필드
            FieldInfo enemiesField = typeof(EarlyEnemyWaveSpawner).GetField("enemies", PrivateFields); // 실제 직렬화 출현 설정 필드
            FieldInfo intervalField = typeof(EarlyEnemyWaveSpawner).GetField("spawnInterval", PrivateFields); // 실제 직렬화 출현 간격
            Require(elapsedField != null && valueField != null && enemiesField != null && intervalField != null, "Actual spawn and gem configuration fields are missing."); // 실제 설정 연결 확인
            EarlyEnemySettings[] settings = enemiesField.GetValue(spawner) as EarlyEnemySettings[]; // 실제 씬 적 설정
            Require(settings != null && settings.Length >= 4 && settings[0].kind == EarlyEnemyKind.Chaser && settings[1].kind == EarlyEnemyKind.Predator && settings[2].kind == EarlyEnemyKind.Shooter && settings[3].kind == EarlyEnemyKind.Elite && Mathf.Approximately(settings[0].firstAppearance, 0f) && Mathf.Approximately(settings[1].firstAppearance, 60f) && Mathf.Approximately(settings[2].firstAppearance, 120f) && Mathf.Approximately(settings[3].firstAppearance, 240f), "Actual scene appearance configuration differs from the early-wave specification."); // 실제 출현 시간 대조
            Require(Elapsed() >= 0f && Elapsed() < 1.5f, "Actual spawn timer was not near its genuine game start."); // 실제 출현 타이머 시작 확인
            Require(experience.CurrentLevel == 1 && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth, "Unexpected initial player fixture."); // 실제 시작 상태 확인
            player.enabled = false; // 시험 중 플레이어 이동 정지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            if (follower != null) // 몸통 추적기 존재 확인
            {
                follower.enabled = false; // 시험 중 몸통 위치 고정
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌기 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌기 조회
            if (self != null) // 자기 충돌기 존재 확인
            {
                self.enabled = false; // 시험 중 자기 접촉 제외
            }
            if (external != null) // 외부 충돌기 존재 확인
            {
                external.enabled = false; // 시험 중 외부 접촉 제외
            }
            Collider2D headCollider = player.GetComponent<Collider2D>(); // 실제 머리 충돌체
            if (headCollider != null) // 머리 충돌체 존재 확인
            {
                headCollider.enabled = false; // 통제 시험 중 투사체 접촉 차단
            }
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 충돌체 순회
            {
                Collider2D segmentCollider = segment == null ? null : segment.GetComponent<Collider2D>(); // 각 몸통 충돌체
                if (segmentCollider != null) // 몸통 충돌체 존재 확인
                {
                    segmentCollider.enabled = false; // 통제 시험 중 몸통 접촉 차단
                }
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 몸통 무기 슬롯 순회
            {
                slot.Clear(); // 통제 중 자연 자동 처치 방지
            }
            experience.Configure(10, 1f); // 시험용 경험치 기준과 배율
            experience.ResetProgression(); // 시험 경험치와 레벨 초기화
            Require(experience.RequiredExperience == 10, "Controlled experience requirement is not ten."); // 실제 통제 요구량 확인
            UnityEngine.Random.InitState(3191); // 출현 추첨 시험 시드
            Time.timeScale = 8f; // 제품 수정 없는 임시 게임 시간 가속
            Debug.Log($"[EarlyWaveVerification] actual spawner start={Elapsed():F3}, configured interval={intervalField.GetValue(spawner)}, appearance=0/60/120/240; accelerated scale=8; seed=3191; no spawn clock injection; movement, player colliders, combat and normal enemies controlled; XP base=10 multiplier=1.0."); // 실제 설정과 통제 조건 기록
        }

        private void ObserveWaves() // 실제 출현 적 관찰과 일반 적 정리
        {
            Require(GameManager.Instance.IsPlaying, "Game stopped during accelerated spawn observation."); // 실제 전투 진행 상태 확인
            float elapsed = Elapsed(); // 실제 출현기 경과 시간
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 활성 출현 적 순회
            {
                int id = enemy.GetInstanceID(); // 실제 출현 적 고유 번호
                if (!observedIds.Add(id)) // 이미 관측한 적 확인
                {
                    continue; // 중복 관측 제외
                }
                int kind = ResolveKind(enemy.gameObject.name); // 실제 출현 객체 종류 판별
                Require(kind >= 0, "Unexpected actual enemy name: " + enemy.gameObject.name); // 실제 출현 종류 확인
                float unlock = kind == 0 ? 0f : kind == 1 ? 60f : kind == 2 ? 120f : 240f; // 기획 등장 해금 시각
                Require(elapsed >= unlock, "Enemy spawned before its configured unlock time."); // 실제 조기 출현 방지
                counts[kind]++; // 실제 종류별 생성 횟수
                if (firstTimes[kind] < 0f) // 해당 종류 최초 출현 확인
                {
                    firstTimes[kind] = elapsed; // 실제 최초 관측 시각 저장
                }
                if (kind == 3) // 실제 엘리트 확인
                {
                    elite = enemy.GetComponent<WeaponTarget>(); // 실제 자동 출현 엘리트 저장
                    Require(elite != null, "Actual spawned elite has no weapon target."); // 실제 전투 대상 확인
                    enemy.enabled = false; // 통제 중 엘리트 행동 정지
                    continue; // 보상 시험을 위해 대상 보존
                }
                enemy.enabled = false; // 통제 중 일반 적 행동 정지
                enemy.gameObject.SetActive(false); // 동시 상한 관찰을 막는 일반 적 제외
                Destroy(enemy.gameObject); // 보상 없이 시험용 일반 적 정리
            }
        }

        private static int ResolveKind(string name) // 실제 출현 객체의 적 종류
        {
            if (name == "EarlyEnemy_Chaser") // 실제 추적자 확인
            {
                return 0; // 추적자 번호 반환
            }
            if (name == "EarlyEnemy_Predator") // 실제 측면 포식자 확인
            {
                return 1; // 포식자 번호 반환
            }
            if (name == "EarlyEnemy_Shooter") // 실제 사수 확인
            {
                return 2; // 사수 번호 반환
            }
            if (name == "EarlyEnemy_Elite") // 실제 엘리트 확인
            {
                return 3; // 엘리트 번호 반환
            }
            return -1; // 알 수 없는 적
        }

        private float Elapsed() // 실제 출현기의 현재 경과 시간
        {
            return (float)elapsedField.GetValue(spawner); // 실제 누적 시간 반환
        }

        private int ReadPickupValue(ExperiencePickup pickup) // 실제 보석 경험치 값
        {
            return (int)valueField.GetValue(pickup); // 실제 보석 값 반환
        }

        private void RequireLevel(int expectedLevel, int remainingXp, int currentBody, int earnedBody) // 실제 연속 레벨업 상태
        {
            Require(experience.CurrentLevel == expectedLevel && experience.CurrentExperience == remainingXp && experience.RequiredExperience == 10 && experience.IsLevelUpPending && body.CurrentBodyCount == currentBody && body.EarnedBodyCount == earnedBody && level.IsPresentingLevelUp && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && panel.gameObject.activeInHierarchy, "Actual gem-to-level reward state or controlled XP accounting mismatch."); // 실제 레벨과 보상 정지 확인
        }

        private void CompleteLevelReward() // 실제 UI 버튼으로 현재 레벨 보상 완료
        {
            int originalLevel = experience.CurrentLevel; // 현재 레벨 기록
            for (int attempts = 0; attempts < 4 && level.IsPresentingLevelUp && experience.CurrentLevel == originalLevel; attempts++) // 실제 보상 화면 순회
            {
                FieldInfo replacement = typeof(SnakeLevelUpController).GetField("selectingReplacement", PrivateFields); // 실제 교체 선택 상태 필드
                Require(replacement != null, "Actual level reward selection state is missing."); // 실제 상태 필드 확인
                int index = (bool)replacement.GetValue(level) ? 1 : 0; // 실제 교체 확정 또는 첫 후보
                ClickButton(index); // 실제 UI 버튼 이벤트
            }
            Require(!level.IsPresentingLevelUp || experience.CurrentLevel != originalLevel, "Actual level reward UI did not complete exactly one level."); // 현재 레벨 보상 완료 확인
        }

        private void ClickUsableEliteReward() // 실제 엘리트 후보 중 통제 가능한 보상 선택
        {
            for (int index = 0; index < eliteReward.Candidates.Count; index++) // 실제 보상 후보 순회
            {
                int kind = eliteReward.Candidates[index]; // 실제 표시 보상 종류
                if (kind == 0 || kind == 1 || kind == 2) // 실제 회복 또는 무기 선택 확인
                {
                    Button button = GetButton(index); // 실제 후보 버튼 조회
                    if (button != null && button.gameObject.activeInHierarchy && button.interactable) // 실제 선택 가능 상태 확인
                    {
                        button.onClick.Invoke(); // 실제 엘리트 보상 버튼 이벤트
                        Debug.Log($"[EarlyWaveVerification] actual elite UI choice index={index}, kind={kind}; reward consumed once."); // 실제 보상 선택 기록
                        return; // 하나의 보상만 선택
                    }
                }
            }
            Require(false, "No actual usable heal, repair or weapon elite reward was presented."); // 실제 보상 선택 불가 기록
        }

        private void ClickButton(int index) // 실제 공용 패널 버튼 클릭
        {
            Button button = GetButton(index); // 실제 버튼 조회
            Require(button != null && button.gameObject.activeInHierarchy && button.interactable, "Actual reward UI button is unavailable."); // 실제 버튼 선택 가능 확인
            button.onClick.Invoke(); // 실제 버튼 이벤트 실행
        }

        private Button GetButton(int index) // 실제 공용 UI 버튼 조회
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 실제 버튼 필드 이름
            FieldInfo field = typeof(LevelUpPanelController).GetField(names[index], PrivateFields); // 실제 직렬화 버튼 필드
            return field == null ? null : field.GetValue(panel) as Button; // 실제 버튼 반환
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedEarlyWaveVerification.Finish(1, message); // 실제 실패 기록 및 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
