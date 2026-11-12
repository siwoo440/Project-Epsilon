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
    public static class AutomatedEliteScheduleVerification // 엘리트 일정과 보상 실제 씬 검증
    {
        private const string ActiveKey = "EpsilonEliteScheduleVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedEliteScheduleVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            {
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 플레이 시작 감시
            }
        }

        public static void Run() // Unity 배치 진입점
        {
            SessionState.SetBool("EpsilonEliteExternalDestroy", false); // 일정 통합 시험 선택
            RunScenario(); // 실제 게임 씬 시작
        }

        public static void RunExternalDestroy() // 실제 외부 파괴 경계 진입점
        {
            SessionState.SetBool("EpsilonEliteExternalDestroy", true); // 외부 파괴 시험 선택
            RunScenario(); // 실제 게임 씬 시작
        }

        private static void RunScenario() // 선택한 시험의 실제 씬 진입
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
            if (EditorApplication.timeSinceStartup - started > 240d) // 전체 시험 제한
            {
                Finish(1, "Timed out waiting for controlled elite schedule test."); // 제한 시간 실패
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
            if (SessionState.GetBool("EpsilonEliteExternalDestroy", false)) // 외부 파괴 시험 확인
            {
                player.gameObject.AddComponent<EliteExternalDestroyVerificationRunner>(); // 실제 외부 파괴 시험 실행
            }
            else // 전체 일정 시험 확인
            {
                player.gameObject.AddComponent<EliteScheduleVerificationRunner>(); // 실제 씬 일정 시험 실행
            }
        }

        public static void Finish(int code, string message) // 결과 기록과 프로세스 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            SessionState.SetBool("EpsilonEliteExternalDestroy", false); // 시험 종류 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[EliteScheduleVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        }
    }

    public sealed class EliteExternalDestroyVerificationRunner : MonoBehaviour // 실제 외부 엘리트 파괴 시험
    {
        private static readonly BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance; // 실제 출현 시간 조회
        private readonly HashSet<int> seen = new HashSet<int>(); // 실제 출현 적 고유 번호
        private readonly List<WeaponTarget> elites = new List<WeaponTarget>(); // 실제 엘리트 출현 목록
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private EliteRewardController rewards; // 실제 엘리트 보상 관리자
        private FieldInfo elapsedField; // 실제 출현 누적 시간
        private int deathEvents; // 실제 사망 이벤트 수

        private IEnumerator Start() // 실제 외부 파괴와 대기 회차 시험
        {
            SnakeMovement player = GetComponent<SnakeMovement>(); // 실제 플레이어 이동 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 보상 관리자 조회
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            Require(player != null && spawner != null && rewards != null && weapons != null && GameManager.Instance.IsPlaying, "Actual external-destruction fixtures are missing."); // 실제 필수 구성 확인
            elapsedField = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Fields); // 실제 출현 시간 조회
            Require(elapsedField != null && Elapsed() < 1.5f, "Actual spawn clock did not start near zero."); // 실제 출현 시간 초기 상태
            player.enabled = false; // 플레이어 이동 정지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            if (follower != null) // 몸통 추적 구성 확인
            {
                follower.enabled = false; // 몸통 위치 고정
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌 조회
            if (self != null) // 자기 충돌 구성 확인
            {
                self.enabled = false; // 통제 중 자기 충돌 방지
            }
            if (external != null) // 외부 충돌 구성 확인
            {
                external.enabled = false; // 통제 중 외부 충돌 방지
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 몸통 무기 순회
            {
                slot.Clear(); // 통제 중 자동 처치 방지
            }
            rewards.enabled = false; // 실제 엘리트 보상 표시 보류
            UnityEngine.Random.InitState(3392); // 실제 출현 방향 재현 시드
            Time.timeScale = 32f; // 실제 출현 누적 시간 가속
            Debug.Log("[EliteScheduleVerification] external destroy case: scale=32, no spawn clock injection, normal enemies retained and enemy actions disabled."); // 실제 시험 조건 기록
            float deadline = Time.realtimeSinceStartup + 120f; // 실제 시험 제한 시간
            while (Elapsed() < 481f && Time.realtimeSinceStartup < deadline) // 실제 8분 경계 대기
            {
                Observe(); // 실제 엘리트와 일반 적 관찰
                yield return null; // 실제 게임 프레임 진행
            }
            Observe(); // 실제 마지막 출현 관찰
            Require(Elapsed() >= 481f && elites.Count == 1 && elites[0] != null && elites[0].IsAlive && spawner.CurrentNormalEnemyCount == 70 && spawner.ActiveEnemies == 71, "Actual eight-minute occurrence did not wait behind one surviving elite and 70 normal enemies."); // 실제 일반 적 만석과 엘리트 대기 확인
            WeaponTarget first = elites[0]; // 실제 미처치 엘리트
            first.Died += ignored => // 실제 사망 이벤트 관찰
            {
                deathEvents++; // 실제 사망 이벤트 수 증가
            };
            Destroy(first.gameObject); // 실제 Unity 외부 지연 파괴 요청
            yield return null; // 실제 파괴 완료 프레임
            Require(first == null && deathEvents == 0, "Actual externally destroyed elite did not reach Unity fake-null without a death."); // 실제 Unity 파괴 완료 확인
            while (elites.Count < 2 && Elapsed() < 487f) // 실제 대기 회차 재개 대기
            {
                Observe(); // 실제 출현 적 관찰
                yield return null; // 실제 게임 프레임 진행
            }
            Observe(); // 실제 재출현 관찰
            Require(elites.Count == 2 && elites[1] != null && elites[1].IsAlive && spawner.CurrentNormalEnemyCount == 70 && spawner.ActiveEnemies == 71 && rewards.PendingRewardCount == 0 && UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length == 0, "External destruction did not release its count and deferred occurrence without rewards."); // 실제 생존 수와 무보상 재출현 확인
            Debug.Log($"[EliteScheduleVerification] actual external destruction: first target became Unity fake-null, Died={deathEvents}, active={spawner.ActiveEnemies}, deferred elite alive, XP gems=0, rewards=0, elapsed={Elapsed():F3}."); // 실제 외부 파괴 결과
            AutomatedEliteScheduleVerification.Finish(0, "Actual externally destroyed elite released capacity and resumed one deferred occurrence without rewards."); // 실제 경계 성공 기록
        }

        private void Observe() // 실제 적 출현 관찰과 행동 중지
        {
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 활성 적 순회
            {
                if (!seen.Add(enemy.GetInstanceID())) // 이전 관찰 여부 확인
                {
                    continue; // 중복 기록 제외
                }
                enemy.enabled = false; // 실제 적 이동과 공격 중지
                if (enemy.gameObject.name == "EarlyEnemy_Elite") // 실제 엘리트 종류 확인
                {
                    WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 엘리트 대상 조회
                    Require(target != null && target.IsAlive, "Actual spawned elite is invalid."); // 실제 생존 대상 확인
                    elites.Add(target); // 실제 출현 순서 저장
                }
            }
        }

        private float Elapsed() // 실제 출현기의 경과 시간
        {
            return (float)elapsedField.GetValue(spawner); // 실제 누적 시간 반환
        }

        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedEliteScheduleVerification.Finish(1, message); // 실제 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }

    public sealed class EliteScheduleVerificationRunner : MonoBehaviour // 실제 엘리트 일정과 보상 검증
    {
        private static readonly BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance; // 실제 내부 설정 조회
        private readonly HashSet<int> seen = new HashSet<int>(); // 실제 출현 적 고유 번호
        private readonly List<WeaponTarget> spawnedElites = new List<WeaponTarget>(); // 실제 출현 엘리트 순서
        private readonly List<float> eliteTimes = new List<float>(); // 실제 출현 관찰 시각
        private SnakeMovement player; // 실제 플레이어 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeHealth health; // 실제 공용 체력
        private SnakeExperience experience; // 실제 경험치 관리자
        private SnakeWeaponManager weapons; // 실제 무기 관리자
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private EliteRewardController rewards; // 실제 엘리트 보상 관리자
        private SnakeLevelUpController level; // 실제 레벨업 관리자
        private LevelUpPanelController panel; // 실제 공용 보상 화면
        private FieldInfo elapsedField; // 실제 출현기 경과 시간
        private FieldInfo pickupValueField; // 실제 보석 경험치 값
        private int deathEvents; // 실제 엘리트 사망 이벤트 수

        private IEnumerator Start() // 실제 씬 일정과 보상 검증
        {
            Prepare(); // 실제 씬과 시험 통제 준비
            yield return UntilElapsed(239f); // 실제 누적 시간으로 첫 경계 접근
            WeaponTarget farthest = FindFarthestNormal(); // 만석에서 가장 먼 일반 적 기록
            int countBefore = spawner.ActiveEnemies; // 실제 전체 활성 적 수
            Require(countBefore == 40 && farthest != null, "Actual normal-enemy capacity fixture is not full before four minutes."); // 실제 일반 적 만석 확인
            yield return UntilEliteCount(1, 245f); // 실제 4분 엘리트 출현 대기
            Require(eliteTimes[0] >= 240f && eliteTimes[0] < 241f && spawner.CurrentNormalEnemyCount == 40 && spawner.ActiveEnemies == 41 && IsAlive(farthest), "First elite did not spawn separately while preserving all 40 normal enemies."); // 첫 엘리트와 일반 적 보존 확인
            Require(CountPickups() == 0 && rewards.PendingRewardCount == 0, "Elite arrival produced an unintended reward."); // 엘리트 출현 시 보상 부재
            Debug.Log($"[EliteScheduleVerification] first elite at {eliteTimes[0]:F3}; normal={spawner.CurrentNormalEnemyCount}, total={spawner.ActiveEnemies}; existing normals preserved."); // 실제 첫 출현 결과
            yield return UntilElapsed(481f); // 실제 8분 경계와 대기 확인
            Require(spawnedElites.Count == 1 && IsAlive(spawnedElites[0]), "The eight-minute elite did not wait for the existing early elite."); // 초반 동시 한 마리와 FIFO 대기
            Debug.Log($"[EliteScheduleVerification] 8-minute occurrence deferred while first elite remains alive; elapsed={Elapsed():F3}."); // 실제 연기 상태 기록
            KillElite(spawnedElites[0], 1); // 실제 첫 엘리트 사망과 중복 처리 확인
            yield return UntilEliteCount(2, 487f); // 대기 중 8분 엘리트 출현
            Require(IsAlive(spawnedElites[1]) && spawnedElites.Count == 2 && spawner.CurrentNormalEnemyCount == 70 && spawner.ActiveEnemies == 71, "Deferred eight-minute elite did not spawn separately from 70 normal enemies."); // 실제 FIFO 재개 확인
            Debug.Log($"[EliteScheduleVerification] deferred 8-minute occurrence spawned at {eliteTimes[1]:F3}; pending rewards={rewards.PendingRewardCount}."); // 지연 출현 실제 기록
            yield return UntilEliteCount(3, 725f); // 실제 12분 엘리트 출현 대기
            Require(eliteTimes[2] >= 720f && eliteTimes[2] < 721f && IsAlive(spawnedElites[1]) && IsAlive(spawnedElites[2]) && spawner.CurrentNormalEnemyCount <= 110 && spawner.ActiveEnemies == spawner.CurrentNormalEnemyCount + 2, "Twelve-minute elites did not coexist separately from normal enemies."); // 12분 동시 두 엘리트 확인
            Debug.Log($"[EliteScheduleVerification] 12-minute occurrence at {eliteTimes[2]:F3}; 8-minute and 12-minute elites alive; capacity={spawner.ActiveEnemies}."); // 실제 공존 결과 기록
            yield return UntilElapsed(961f); // 실제 16분 경계와 대기 확인
            Require(spawnedElites.Count == 3 && IsAlive(spawnedElites[1]) && IsAlive(spawnedElites[2]), "Sixteen-minute elite failed to wait for the two live elites."); // 최대 두 마리와 다음 회차 대기
            KillElite(spawnedElites[1], 2); // 실제 두 번째 엘리트 사망
            yield return UntilEliteCount(4, 967f); // 대기 중 16분 엘리트 출현
            Require(IsAlive(spawnedElites[2]) && IsAlive(spawnedElites[3]) && spawner.CurrentNormalEnemyCount <= 125 && spawner.ActiveEnemies == spawner.CurrentNormalEnemyCount + 2, "Deferred sixteen-minute elite did not coexist with the surviving earlier elite."); // 실제 FIFO 및 기존 엘리트 보존
            Debug.Log($"[EliteScheduleVerification] deferred 16-minute occurrence spawned at {eliteTimes[3]:F3}; surviving 12-minute elite preserved."); // 지연된 네 번째 출현 기록
            yield return UntilElapsed(Elapsed() + 3.5f); // 실제 다음 출현 간격 진행
            Require(spawnedElites.Count == 4, "An elite occurrence spawned more than once."); // 네 회차 중복 생성 방지
            spawner.enabled = false; // 실제 보상 시험 중 추가 일반 적 출현 정지
            KillElite(spawnedElites[2], 3); // 실제 세 번째 엘리트 사망
            KillElite(spawnedElites[3], 4); // 실제 네 번째 엘리트 사망
            Require(deathEvents == 4 && rewards.PendingRewardCount == 4 && CountPickups() == 4 && CountExperience() == 80, "Actual elite deaths did not produce exactly four reward requests and four 20-XP gems."); // 실제 사망과 보상 총량 확인
            Debug.Log("[EliteScheduleVerification] four controlled elite defeats: four Died events, four pending rewards, four actual gems worth 80 XP."); // 실제 보상 생성 기록
            Time.timeScale = 1f; // 보상 경합 전 정상 시험 배율
            Require(health.TakeDamage(health.MaximumHealth + 1), "Could not prepare usable repair and heal rewards."); // 보상 선택용 통제 피해
            Require(body.CurrentBodyCount == 2 && body.EarnedBodyCount == 3, "Controlled reward fixture body state is incorrect."); // 실제 몸통 통제 상태
            ExperiencePickup[] gems = UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None); // 실제 대형 보석 조회
            Require(gems.Length == 4, "Actual elite gems are missing before collection."); // 실제 보석 수 확인
            gems[0].transform.position = experience.transform.position; // 실제 보석 하나의 수집 위치 통제
            rewards.enabled = true; // 실제 대기 엘리트 보상 재개
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 레벨업 제한 시각
            while (!experience.IsLevelUpPending && Time.realtimeSinceStartup < deadline) // 실제 보석 수집 대기
            {
                yield return null; // 실제 보석 및 UI Update 진행
            }
            RequireLevel(2, 10); // 실제 대형 보석의 첫 레벨업
            Require(rewards.PendingRewardCount == 4 && !rewards.IsPresentingReward, "Elite rewards did not wait for the first actual level-up."); // 실제 보상 우선순위 확인
            CompleteLevelReward(); // 실제 UI로 첫 레벨업 보상 완료
            RequireLevel(3, 0); // 실제 대형 보석의 연속 레벨업
            Require(rewards.PendingRewardCount == 4 && !rewards.IsPresentingReward, "Elite rewards interrupted the consecutive level-up."); // 실제 연속 레벨 우선 확인
            CompleteLevelReward(); // 실제 UI로 둘째 레벨업 보상 완료
            for (int remaining = 4; remaining > 0; remaining--) // 실제 대기 보상 각각 선택
            {
                yield return SelectOneEliteReward(remaining); // 실제 보상 한 개 선택과 소비 확인
            }
            yield return null; // 실제 다음 프레임 진행
            int remainingExperience = CountExperience(); // 실제 남은 보석 경험치 합계
            string finalState = $"pending={rewards.PendingRewardCount}, presenting={rewards.IsPresentingReward}, levelPending={experience.IsLevelUpPending}, playing={GameManager.Instance.IsPlaying}, scale={Time.timeScale:F2}, level={experience.CurrentLevel}, xp={experience.CurrentExperience}, gemsXp={remainingExperience}"; // 실제 완료 상태 진단
            Debug.Log("[EliteScheduleVerification] final reward state: " + finalState); // 실제 보상 종료 상태 기록
            Require(rewards.PendingRewardCount == 0 && !rewards.IsPresentingReward && !experience.IsLevelUpPending && GameManager.Instance.IsPlaying && Mathf.Approximately(Time.timeScale, 1f) && experience.CurrentLevel == 3 && experience.CurrentExperience == 0 && remainingExperience == 60, "Actual rewards, level-ups or remaining gems were duplicated or lost: " + finalState); // 실제 완료 상태와 미수집 보석 확인
            AutomatedEliteScheduleVerification.Finish(0, "Actual accelerated 4/8/12/16-minute elite schedule, FIFO, capacity, four deaths, XP gems and reward ordering passed under controlled conditions."); // 실제 통제 시험 성공
        }

        private void Prepare() // 실제 씬 초기화와 시간 가속 통제
        {
            player = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            experience = GetComponent<SnakeExperience>(); // 실제 경험치 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기 조회
            rewards = UnityEngine.Object.FindFirstObjectByType<EliteRewardController>(); // 실제 엘리트 보상 관리자 조회
            level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 실제 레벨업 관리자 조회
            panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 보상 패널 조회
            Require(player != null && body != null && health != null && experience != null && weapons != null && spawner != null && rewards != null && level != null && panel != null && GameManager.Instance.IsPlaying, "Actual Game scene fixtures are missing."); // 실제 필수 구성 확인
            elapsedField = typeof(EarlyEnemyWaveSpawner).GetField("elapsed", Fields); // 실제 출현 누적 시간 필드
            pickupValueField = typeof(ExperiencePickup).GetField("experienceValue", Fields); // 실제 보석 값 필드
            Require(elapsedField != null && pickupValueField != null && Elapsed() >= 0f && Elapsed() < 1.5f, "Actual spawn clock was not near its genuine game start."); // 실제 출현 시계 시작 확인
            player.enabled = false; // 실제 머리 이동 정지
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            if (follower != null) // 실제 몸통 추적 존재 확인
            {
                follower.enabled = false; // 실제 몸통 위치 고정
            }
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌 조회
            if (self != null) // 자기 충돌 존재 확인
            {
                self.enabled = false; // 통제 중 자기 충돌 제외
            }
            if (external != null) // 외부 충돌 존재 확인
            {
                external.enabled = false; // 통제 중 외부 충돌 제외
            }
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 모든 무기 슬롯 순회
            {
                slot.Clear(); // 시험 중 자연 자동 처치 방지
            }
            weapons.enabled = false; // 보상으로 새 포탑을 얻은 뒤에도 자동 공격 차단
            rewards.enabled = false; // 여러 실제 보상을 후반 경합까지 대기
            experience.Configure(10, 1f); // 시험용 레벨업 요구량
            experience.ResetProgression(); // 실제 시험 경험치 초기화
            Require(experience.CurrentLevel == 1 && experience.CurrentExperience == 0 && experience.RequiredExperience == 10 && body.CurrentBodyCount == 3 && health.CurrentHealth == health.MaximumHealth, "Actual controlled start state is incorrect."); // 실제 시작 상태 확인
            UnityEngine.Random.InitState(3391); // 출현과 보상 재현 시드
            Time.timeScale = 32f; // 실제 출현 시계의 시험 가속
            Debug.Log($"[EliteScheduleVerification] actual initial spawner time={Elapsed():F3}; scale=32, seed=3391, no clock injection; player/attacks and spawned enemy actions disabled; early cap=1, late cap=2, deferred FIFO is a provisional assumption; XP base=10 multiplier=1.0."); // 실제 통제와 기획 가정 기록
        }

        private IEnumerator UntilElapsed(float required) // 실제 출현기 시각 대기
        {
            float realDeadline = Time.realtimeSinceStartup + 150f; // 실제 시간 제한
            while (Elapsed() < required && Time.realtimeSinceStartup < realDeadline) // 실제 누적 시간 대기
            {
                ObserveEnemies(); // 실제 자동 출현 적 기록
                yield return null; // 실제 게임 프레임 진행
            }
            ObserveEnemies(); // 마지막 출현 프레임 관찰
            Require(Elapsed() >= required && GameManager.Instance.IsPlaying, "Actual spawner did not reach the requested elapsed time while playing."); // 실제 시간 진행 확인
        }

        private IEnumerator UntilEliteCount(int count, float latest) // 실제 엘리트 출현 수 대기
        {
            while (spawnedElites.Count < count && Elapsed() < latest) // 실제 출현과 시한 확인
            {
                ObserveEnemies(); // 실제 자동 출현 관찰
                yield return null; // 실제 프레임 진행
            }
            ObserveEnemies(); // 마지막 실제 출현 관찰
            Require(spawnedElites.Count == count, "Expected actual elite occurrence did not spawn by controlled deadline."); // 실제 회차 출현 확인
        }

        private void ObserveEnemies() // 실제 출현 적 관찰과 행동 통제
        {
            Require(GameManager.Instance.IsPlaying, "Game stopped during the accelerated schedule observation."); // 실제 게임 진행 상태 확인
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 활성 적 순회
            {
                if (!seen.Add(enemy.GetInstanceID())) // 이미 관찰한 적 확인
                {
                    continue; // 중복 관찰 방지
                }
                enemy.enabled = false; // 실제 적의 이동과 공격 정지
                if (enemy.gameObject.name != "EarlyEnemy_Elite") // 실제 엘리트 종류 확인
                {
                    continue; // 일반 적은 만석 상태로 유지
                }
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 엘리트 전투 대상
                Require(target != null && target.IsAlive, "Actual spawned elite has no living target."); // 실제 엘리트 생존 확인
                target.Died += ignored => // 실제 사망 이벤트 구독
                {
                    deathEvents++; // 실제 엘리트 사망 수 증가
                };
                spawnedElites.Add(target); // 실제 출현 순서 저장
                eliteTimes.Add(Elapsed()); // 실제 출현 관찰 시각 저장
            }
        }

        private WeaponTarget FindFarthestNormal() // 실제 만석의 가장 먼 일반 적
        {
            WeaponTarget farthest = null; // 실제 가장 먼 적 초기화
            float greatest = -1f; // 실제 최대 거리 제곱
            FieldInfo targetsField = typeof(EarlyEnemyWaveSpawner).GetField("targets", Fields); // 실제 출현기 관리 대상 필드
            Require(targetsField != null, "Actual spawner target list is unavailable."); // 실제 관리 목록 확인
            List<WeaponTarget> targets = targetsField.GetValue(spawner) as List<WeaponTarget>; // 실제 출현 순서의 대상 목록
            Require(targets != null && targets.Count == 40, "Actual spawner target list is not at full capacity."); // 실제 관리 대상 만석 확인
            foreach (WeaponTarget target in targets) // 실제 출현기 순서로 대상 순회
            {
                if (target == null || target.gameObject.name == "EarlyEnemy_Elite") // 실제 엘리트 및 제거 대상 제외
                {
                    continue; // 다음 일반 적 확인
                }
                float distance = (target.transform.position - player.transform.position).sqrMagnitude; // 실제 머리 거리 제곱
                if (target.IsAlive && distance > greatest) // 실제 가장 먼 생존 일반 적 확인
                {
                    farthest = target; // 실제 제거 예상 대상
                    greatest = distance; // 실제 최대 거리 갱신
                }
            }
            return farthest; // 실제 예상 대상 반환
        }

        private void KillElite(WeaponTarget target, int expectedRewards) // 실제 엘리트 사망과 중복 방지
        {
            Require(IsAlive(target), "Expected elite was not alive before the controlled defeat."); // 실제 처치 전 생존 확인
            EnemyDropController drop = target.GetComponent<EnemyDropController>(); // 실제 보상 드롭 관리자
            FieldInfo healChance = typeof(EnemyDropController).GetField("healDropChance", Fields); // 실제 회복 드롭 확률
            FieldInfo repairChance = typeof(EnemyDropController).GetField("bodyRepairDropChance", Fields); // 실제 몸통 드롭 확률
            Require(drop != null && healChance != null && repairChance != null, "Actual elite drop configuration is missing."); // 실제 드롭 구성 확인
            healChance.SetValue(drop, 0f); // 시험 중 무작위 회복 보석 제외
            repairChance.SetValue(drop, 0f); // 시험 중 무작위 복구 보석 제외
            int beforeEvents = deathEvents; // 기존 실제 사망 이벤트 수
            target.TakeDamage(10000f); // 실제 엘리트 강제 처치
            target.TakeDamage(10000f); // 같은 대상 중복 피해 시도
            Require(deathEvents == beforeEvents + 1 && rewards.PendingRewardCount == expectedRewards && CountPickups() == expectedRewards && CountExperience() == expectedRewards * 20, "Elite death produced a missing or duplicate actual reward or gem."); // 실제 한 회 사망과 보상 확인
        }

        private int CountPickups() // 실제 활성 경험치 보석 수
        {
            return UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None).Length; // 실제 보석 수 반환
        }

        private int CountExperience() // 실제 활성 보석의 경험치 총량
        {
            int total = 0; // 실제 경험치 합계
            foreach (ExperiencePickup pickup in UnityEngine.Object.FindObjectsByType<ExperiencePickup>(FindObjectsSortMode.None)) // 실제 활성 보석 순회
            {
                total += (int)pickupValueField.GetValue(pickup); // 실제 보석 값 누적
            }
            return total; // 실제 보석 경험치 반환
        }

        private float Elapsed() // 실제 출현기의 누적 시간
        {
            return (float)elapsedField.GetValue(spawner); // 실제 누적 시간 반환
        }

        private static bool IsAlive(WeaponTarget target) // 실제 대상 생존 확인
        {
            return target != null && target.isActiveAndEnabled && target.IsAlive; // 실제 생존 및 활성 여부
        }

        private void RequireLevel(int expectedLevel, int remainingXp) // 실제 레벨업 상태 확인
        {
            Require(experience.CurrentLevel == expectedLevel && experience.CurrentExperience == remainingXp && experience.IsLevelUpPending && experience.RequiredExperience == 10 && level.IsPresentingLevelUp && GameManager.Instance.IsPaused && Mathf.Approximately(Time.timeScale, 0f), "Actual elite gem-to-level UI state mismatch."); // 실제 경험치와 정지 확인
        }

        private void CompleteLevelReward() // 실제 UI로 한 레벨 보상 완료
        {
            int selectedLevel = experience.CurrentLevel; // 현재 실제 레벨
            for (int attempts = 0; attempts < 4 && level.IsPresentingLevelUp && experience.CurrentLevel == selectedLevel; attempts++) // 실제 보상 단계 순회
            {
                FieldInfo replacement = typeof(SnakeLevelUpController).GetField("selectingReplacement", Fields); // 실제 교체 화면 상태
                Require(replacement != null, "Actual level selection state is missing."); // 실제 단계 필드 확인
                ClickButton((bool)replacement.GetValue(level) ? 1 : 0); // 실제 공용 UI 선택
            }
            Require(!level.IsPresentingLevelUp || experience.CurrentLevel != selectedLevel, "Actual level reward was not completed once."); // 실제 보상 완료 확인
        }

        private IEnumerator SelectOneEliteReward(int expectedPending) // 실제 엘리트 보상 하나 선택
        {
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 보상 화면 제한
            while (!rewards.IsPresentingReward && Time.realtimeSinceStartup < deadline) // 실제 보상 표시 대기
            {
                yield return null; // 실제 보상 Update 진행
            }
            Require(rewards.IsPresentingReward && rewards.PendingRewardCount == expectedPending && GameManager.Instance.IsPaused && rewards.Candidates.Count == 3, "Actual queued elite reward did not present once."); // 실제 보상 대기와 정지 확인
            if (health.CurrentHealth == health.MaximumHealth) // 실제 체력 회복 후보 준비 확인
            {
                Require(health.TakeDamage(1), "Could not maintain a usable controlled heal candidate."); // 시험용 체력 1 감소
                yield return null; // 실제 보상 선택 가능 상태 갱신
            }
            int choice = -1; // 실제 선택 가능한 후보 번호
            for (int index = 0; index < rewards.Candidates.Count; index++) // 실제 후보 세 개 순회
            {
                int kind = rewards.Candidates[index]; // 실제 보상 종류
                Button button = GetButton(index); // 실제 UI 버튼 조회
                if ((kind == 0 || kind == 2) && button != null && button.gameObject.activeInHierarchy && button.interactable) // 실제 회복 또는 무기 선택 확인
                {
                    choice = index; // 실제 선택 후보 저장
                    break; // 첫 선택 가능 후보 사용
                }
            }
            Require(choice >= 0, "Actual elite reward has no controlled usable heal or weapon choice."); // 실제 후보 존재 확인
            ClickButton(choice); // 실제 엘리트 보상 버튼 이벤트
            FieldInfo replacing = typeof(EliteRewardController).GetField("selectingReplacement", Fields); // 실제 교체 화면 상태
            Require(replacing != null, "Actual elite replacement state is missing."); // 교체 단계 필드 확인
            if (rewards.IsPresentingReward && (bool)replacing.GetValue(rewards)) // 실제 무기 교체 단계 확인
            {
                ClickButton(1); // 실제 교체 확정 버튼
            }
            Require(rewards.PendingRewardCount == expectedPending - 1 && !rewards.IsPresentingReward && GameManager.Instance.IsPlaying, "Actual elite reward was not consumed exactly once."); // 실제 단일 보상 소비 확인
            Debug.Log($"[EliteScheduleVerification] actual elite UI consumed one reward: {expectedPending}->{rewards.PendingRewardCount}."); // 실제 보상 소비 기록
        }

        private void ClickButton(int index) // 실제 공용 보상 버튼 선택
        {
            Button button = GetButton(index); // 실제 UI 버튼 조회
            Require(button != null && button.gameObject.activeInHierarchy && button.interactable, "Actual reward UI button is unavailable."); // 실제 선택 가능 확인
            button.onClick.Invoke(); // 실제 UI 버튼 이벤트
        }

        private Button GetButton(int index) // 실제 공용 보상 버튼 조회
        {
            string[] names = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 실제 버튼 필드 이름
            FieldInfo field = typeof(LevelUpPanelController).GetField(names[index], Fields); // 실제 직렬화 버튼 필드
            return field == null ? null : field.GetValue(panel) as Button; // 실제 버튼 반환
        }

        private static void Require(bool condition, string message) // 실제 시험 필수 조건 확인
        {
            if (!condition) // 실패 조건 확인
            {
                AutomatedEliteScheduleVerification.Finish(1, message); // 실제 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
