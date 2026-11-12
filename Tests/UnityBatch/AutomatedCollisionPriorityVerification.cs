using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 충돌 목록
using System.Reflection; // 비공개 콜백 조회
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Enemies; // 적 행동과 출현기
using ProjectEpsilon.Player; // 플레이어 구성
using UnityEditor; // 편집기 제어
using UnityEditor.SceneManagement; // 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.LowLevel; // 플레이어루프 조회

namespace ProjectEpsilon.Editor // 편집기 영역
{
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedCollisionPriorityVerification // 충돌 우선순위 배치 시험
    {
        private const string ActiveKey = "EpsilonCollisionPriorityVerification"; // 시험 진행 표식
        private static double started; // 대기 시작 시간
        private static bool injected; // 실행기 추가 여부

        static AutomatedCollisionPriorityVerification() // 도메인 연결
        {
            if (SessionState.GetBool(ActiveKey, false)) // 진행 여부 확인
            {
                EditorApplication.update += Tick; // 진입 감시 연결
            }
        }

        public static void Run() // 배치 실행 진입점
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 기록
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 진입 감시 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        }

        private static void Tick() // 플레이 진입 감시
        {
            if (!EditorApplication.isPlaying) // 진입 상태 확인
            {
                return; // 플레이 시작 대기
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 시작 무기 화면 처리
            {
                return; // 전투 시작 대기
            }
            if (started <= 0d) // 최초 시간 확인
            {
                started = EditorApplication.timeSinceStartup; // 대기 시간 저장
            }
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 실행 제한 확인
            {
                Finish(1, "Collision verification timed out."); // 시간 초과 기록
                return; // 추가 처리 중단
            }
            if (injected) // 실행기 동작 확인
            {
                return; // 실행기 완료 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || Time.time < 0.25f) // 초기화 완료 확인
            {
                return; // 필수 구성 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<CollisionVerificationRunner>(); // 실제 물리 검증 시작
        }

        public static void Finish(int code, string message) // 배치 종료 처리
        {
            SessionState.SetBool(ActiveKey, false); // 진행 표식 해제
            EditorApplication.update -= Tick; // 진입 감시 해제
            if (code == 0) // 성공 여부 확인
            {
                Debug.Log("[CollisionVerification] PASS: " + message); // 성공 기록
            }
            else // 실패 처리
            {
                Debug.LogError("[CollisionVerification] FAIL: " + message); // 실패 기록
            }
            EditorApplication.Exit(code); // 결과 코드 반환
        }
    }

    public sealed class CollisionVerificationProbe : MonoBehaviour // 실제 콜백 순서 기록
    {
        public readonly List<string> Entries = new List<string>(); // 실제 진입 목록
        public readonly List<float> Steps = new List<float>(); // 진입 물리 시간
        public bool PauseOnEntry; // 충돌 중 정지 여부
        public bool DestroyOnEntry; // 접촉 원본 제거 여부

        private void OnTriggerEnter2D(Collider2D other) // 물리 엔진 진입 콜백
        {
            SnakeContactHazard hazard = other.GetComponent<SnakeContactHazard>(); // 외부 위험 조회
            if (hazard != null) // 외부 위험 확인
            {
                Entries.Add(hazard.HazardType.ToString()); // 콜백 순서 기록
                Steps.Add(Time.fixedTime); // 물리 스텝 기록
                if (DestroyOnEntry) // 접촉 원본 제거 시험
                {
                    Destroy(other.gameObject); // 접촉 객체 지연 제거
                }
            }
            if (PauseOnEntry && GameManager.Instance != null) // 충돌 중 정지 시험
            {
                PauseOnEntry = false; // 중복 정지 방지
                GameManager.Instance.PauseGame(); // 동일 스텝 정지 요청
            }
        }

        public void Clear() // 시험 기록 초기화
        {
            Entries.Clear(); // 콜백 목록 초기화
            Steps.Clear(); // 물리 스텝 목록 초기화
            PauseOnEntry = false; // 정지 시험 해제
            DestroyOnEntry = false; // 원본 제거 시험 해제
        }
    }

    [DefaultExecutionOrder(-1000)] // 물리 시뮬레이션 전 실행
    public sealed class CollisionVerificationSequencer : MonoBehaviour // 물리 단계별 접촉 생성기
    {
        public CollisionVerificationRunner Runner; // 시험 실행기
        public int FirstFrame = -1; // 첫 물리 단계의 렌더 프레임
        public int SecondFrame = -1; // 둘째 물리 단계의 렌더 프레임
        public float FirstTime; // 첫 물리 시간
        public float SecondTime; // 둘째 물리 시간
        private int step; // 생성 단계

        private void FixedUpdate() // 물리 단계별 위험 생성
        {
            if (step == 0) // 첫 물리 단계 확인
            {
                FirstFrame = Time.frameCount; // 첫 프레임 기록
                FirstTime = Time.fixedTime; // 첫 물리 시간 기록
                Runner.SpawnHazard(SnakeContactHazardType.Obstacle, true); // 첫 스텝 장애물 진입
                Physics2D.SyncTransforms(); // 장애물 위치 동기화
                step = 1; // 둘째 단계 대기
                return; // 중복 처리 방지
            }
            if (step == 1) // 둘째 물리 단계 확인
            {
                SecondFrame = Time.frameCount; // 둘째 프레임 기록
                SecondTime = Time.fixedTime; // 둘째 물리 시간 기록
                Runner.SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 다음 스텝 적 진입
                Physics2D.SyncTransforms(); // 적 위치 동기화
                step = 2; // 시험 생성 완료
                enabled = false; // 추가 물리 단계 중단
            }
        }
    }

    public sealed class CollisionVerificationRunner : MonoBehaviour // 실제 충돌 시나리오 실행기
    {
        private SnakeMovement player; // 머리 이동 구성
        private SnakeBodyManager body; // 몸통 관리자
        private SnakeBodyFollower follower; // 몸통 위치 추적기
        private SnakeHealth health; // 공용 체력
        private SnakeInvulnerability invulnerability; // 전체 무적
        private SnakeExternalCollision external; // 외부 충돌 처리기
        private SnakeShieldController shield; // 보호막 관리자
        private CollisionVerificationProbe probe; // 물리 콜백 기록기
        private readonly List<GameObject> spawned = new List<GameObject>(); // 시험 객체 목록
        private readonly HashSet<string> observedOrders = new HashSet<string>(); // 실제 순서 종류
        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance; // 비공개 멤버 검색 조건

        private IEnumerator Start() // 시험 코루틴
        {
            player = GetComponent<SnakeMovement>(); // 머리 이동 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 몸통 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 몸통 추적기 조회
            health = GetComponent<SnakeHealth>(); // 체력 조회
            invulnerability = GetComponent<SnakeInvulnerability>(); // 무적 조회
            external = GetComponent<SnakeExternalCollision>(); // 외부 충돌 조회
            shield = health == null ? null : health.ShieldController; // 체력에 연결된 보호막 조회
            if (!Check(player != null && body != null && follower != null && health != null && invulnerability != null && external != null && shield != null, "Missing Game scene components.")) // 필수 참조 확인
            {
                yield break; // 실패 후 중단
            }
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 자연 출현기 조회
            if (spawner != null) // 출현기 연결 확인
            {
                spawner.enabled = false; // 자연 적 생성 중지
            }
            player.enabled = false; // 머리 자동 이동 중지
            follower.enabled = false; // 시험 몸통 위치 고정
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 기존 시험 적 순회
            {
                Destroy(enemy.gameObject); // 자연 생성 적 정리
            }
            probe = gameObject.AddComponent<CollisionVerificationProbe>(); // 머리 충돌 순서 기록기
            Debug.Log("[CollisionVerification] Conditions: real Game scene; player motion and wave spawning disabled; controlled colliders; fixed simulation."); // 시험 조건 기록
            yield return null; // 기존 적 제거 대기
            yield return ResetFixture(); // 초기 상태 준비
            yield return SimultaneousContacts(true); // 장애물 우선 생성 물리 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return SimultaneousContacts(false); // 적 우선 생성 물리 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return SyntheticOrder(true); // 장애물 선행 인위 콜백
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return SyntheticOrder(false); // 적 선행 인위 콜백
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return ExistingInvulnerability(); // 기존 전체 무적 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return BodyAndObstacle(); // 자기충돌 우선 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 긴 몸통 시험 초기화
            yield return PrepareLongBody(); // 실제 스무 칸 몸통 준비
            yield return LongBodySelfAndObstacle(); // 긴 몸통 자기 충돌 우선 확인
            if (!enabled) // 긴 몸통 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 적 포위 시험 초기화
            yield return PrepareLongBody(); // 실제 스무 칸 몸통 준비
            yield return LongBodySurrounded(); // 다수 적 동시 접촉 확인
            if (!enabled) // 적 포위 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 좁은 통로 시험 초기화
            yield return PrepareLongBody(); // 실제 스무 칸 몸통 준비
            yield return LongBodyCorridor(); // 직진과 벽 접촉 확인
            if (!enabled) // 좁은 통로 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 단일 장애물 이탈 시험 초기화
            yield return SingleObstacleNearestRecovery(); // 최소 이탈 거리 확인
            if (!enabled) // 최소 이탈 거리 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 이탈 완료 시험 초기화
            yield return ObstacleExitDuringInvulnerability(); // 무적 중 이미 벗어난 장애물 확인
            if (!enabled) // 장애물 이탈 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return ProjectileAndObstacle(false); // 장애물과 실제 투사체 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return ProjectileAndObstacle(true); // 보호막과 동일 피해 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return MeleeAndObstacle(); // 기존 근접 접촉과 신규 장애물 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return EliteCutAndObstacle(); // 엘리트 절단과 장애물 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return PersistentEnemyContact(); // 기존 적 접촉과 신규 장애물 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return DestroyedHazards(); // 접촉 직후 객체 제거 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return MultiplePhysicsSteps(); // 동일 렌더 프레임의 여러 물리 단계 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return DisabledPendingCancellation(); // 비활성화 시 절단 예약 취소 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return DispatcherLifecycle(); // 실행 순서와 재등록 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return PausedDuringContact(); // 충돌 중 일시정지 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            yield return ResetFixture(); // 상태 초기화
            yield return LastBodyDefeat(); // 마지막 몸통 패배 시험
            if (!enabled) // 이전 시험 실패 확인
            {
                yield break; // 남은 시험 중단
            }
            AutomatedCollisionPriorityVerification.Finish(0, "Actual 2D triggers, long-body self collision, surrounding enemies, narrow corridor, invulnerability, projectile, shield, melee, elite cut, persistent contact, cancellation, pause and defeat. Actual callback orders: " + string.Join(",", observedOrders)); // 전체 성공 기록
        }

        private IEnumerator ResetFixture() // 충돌 시험 초기화
        {
            SnakeSelfCollision self = player.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            self.enabled = false; // 몸통 초기화 중 의도하지 않은 자기 접촉 방지
            player.enabled = false; // 이전 통로 시험의 이동 정지
            foreach (GameObject item in spawned) // 시험 객체 순회
            {
                if (item != null) // 객체 존재 확인
                {
                    Collider2D collider = item.GetComponent<Collider2D>(); // 시험 충돌체 조회
                    if (collider != null) // 충돌체 존재 확인
                    {
                        collider.enabled = false; // 남은 충돌 차단
                    }
                    Destroy(item); // 시험 객체 제거
                }
            }
            spawned.Clear(); // 시험 객체 목록 초기화
            if (GameManager.Instance.IsPaused || GameManager.Instance.CurrentState == GameState.Defeated) // 진행 정지 여부
            {
                GameManager.Instance.StartGame(); // 다음 시험 진행 복구
            }
            invulnerability.ClearInvulnerability(); // 무적 초기화
            shield.Clear(); // 보호막 초기화
            player.GetComponent<SnakePathRecorder>().ResetHistory(); // 실제 머리 위치 기준 직선 경로 초기화
            body.ResetBody(); // 시작 몸통 복구
            health.ResetHealth(); // 공용 체력 복구
            external.Bind(body, health, invulnerability, 1, 1f, 25, 1.5f); // 외부 충돌 보호 초기화
            probe.Clear(); // 진입 기록 초기화
            Physics2D.SyncTransforms(); // 시험 위치 물리 동기화
            yield return null; // 지연 객체 제거 대기
            yield return new WaitForFixedUpdate(); // 충돌 해제 물리 스텝 대기
            self.enabled = true; // 실제 자기 충돌 재개
            yield return null; // 후속 플레이어루프 대기
        }

        private IEnumerator SimultaneousContacts(bool obstacleFirst) // 실제 동시 트리거 시험
        {
            SpawnHazard(obstacleFirst ? SnakeContactHazardType.Obstacle : SnakeContactHazardType.EnemyDirect, true); // 첫 위험 생성
            SpawnHazard(obstacleFirst ? SnakeContactHazardType.EnemyDirect : SnakeContactHazardType.Obstacle, true); // 둘째 위험 생성
            Physics2D.SyncTransforms(); // 새 충돌체 동기화
            yield return new WaitForFixedUpdate(); // 실제 물리 스텝 대기
            yield return null; // 물리 후 처리 대기
            if (!Check(probe.Entries.Count == 2 && probe.Steps.Count == 2 && Mathf.Approximately(probe.Steps[0], probe.Steps[1]), "Real hazards did not enter in one physical step.")) // 실제 동시 진입 확인
            {
                yield break; // 실패 처리
            }
            string order = string.Join("->", probe.Entries); // 실제 콜백 순서
            observedOrders.Add(order); // 관측 순서 저장
            Debug.Log("[CollisionVerification] Real callbacks: " + order); // 실제 순서 기록
            Check(body.CurrentBodyCount == 2 && health.CurrentHealth == 75 && external.EnemyContactProtected && invulnerability.IsInvulnerable, $"Real simultaneous contact mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}, direct={external.EnemyContactProtected}, full={invulnerability.IsInvulnerable}."); // 몸통 우선 및 보호 확인
        }

        private IEnumerator SyntheticOrder(bool obstacleFirst) // 인위적 순서 콜백 시험
        {
            Collider2D obstacle = SpawnHazard(SnakeContactHazardType.Obstacle, false); // 비활성 장애물
            Collider2D enemy = SpawnHazard(SnakeContactHazardType.EnemyDirect, false); // 비활성 적
            MethodInfo enter = typeof(SnakeExternalCollision).GetMethod("OnTriggerEnter2D", PrivateInstance); // 실제 콜백 조회
            enter.Invoke(external, new object[] { obstacleFirst ? obstacle : enemy }); // 첫 인위 콜백
            enter.Invoke(external, new object[] { obstacleFirst ? enemy : obstacle }); // 둘째 인위 콜백
            enter.Invoke(external, new object[] { obstacle }); // 중복 장애물 콜백
            enter.Invoke(external, new object[] { enemy }); // 중복 적 콜백
            yield return new WaitForFixedUpdate(); // 물리 후 처리 대기
            yield return null; // 후속 플레이어루프 대기
            Check(body.CurrentBodyCount == 2 && health.CurrentHealth == 75 && external.EnemyContactProtected && invulnerability.IsInvulnerable, $"Synthetic duplicate or priority mismatch: {(obstacleFirst ? "obstacle-first" : "enemy-first")}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 중복 및 순서 확인
            Debug.Log("[CollisionVerification] Synthetic callback order: " + (obstacleFirst ? "obstacle-first" : "enemy-first")); // 인위 시험 구분 기록
        }

        private IEnumerator ExistingInvulnerability() // 기존 전체 무적 시험
        {
            invulnerability.StartInvulnerability(5f); // 기존 무적 적용
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 장애물 접촉 생성
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 적 접촉 생성
            Physics2D.SyncTransforms(); // 충돌체 동기화
            yield return new WaitForFixedUpdate(); // 물리 처리 대기
            yield return null; // 후속 처리 대기
            Check(body.CurrentBodyCount == 3 && health.CurrentHealth == 100, "Existing full invulnerability did not block contacts."); // 기존 무적 우선 확인
        }

        private IEnumerator BodyAndObstacle() // 자기충돌과 장애물 시험
        {
            body.BodySegments[2].transform.position = player.transform.position; // 꼬리 몸통 머리 위치 이동
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 같은 위치 장애물 생성
            Physics2D.SyncTransforms(); // 움직인 몸통 물리 동기화
            yield return new WaitForFixedUpdate(); // 실제 접촉 대기
            yield return null; // 물리 후 해결 대기
            Check(body.CurrentBodyCount == 1 && health.CurrentHealth == 100 && invulnerability.IsInvulnerable, $"Self collision priority mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}, full={invulnerability.IsInvulnerable}."); // 몸통 손실 및 신규 무적 확인
        }

        private IEnumerator PrepareLongBody() // 실제 스무 칸 몸통 준비
        {
            SnakeSelfCollision self = player.GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기
            self.enabled = false; // 시험 배치 중 자기 충돌 차단
            while (body.CurrentBodyCount < 20) // 실제 절대 상한까지 성장
            {
                if (!Check(body.TryGainBodyFromLevelUp(), "Could not grow to twenty bodies.")) // 실제 성장 성공 확인
                {
                    yield break; // 성장 실패 시 중단
                }
            }
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            {
                body.BodySegments[index].transform.position = player.transform.position + new Vector3(30f + index * 2f, 30f, 0f); // 접촉 없는 몸통 위치
            }
            if (body.TailSegment != null) // 실제 꼬리 존재 확인
            {
                body.TailSegment.transform.position = player.transform.position + new Vector3(80f, 30f, 0f); // 꼬리 접촉 방지
            }
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 기존 물리 접촉 해제
            self.enabled = true; // 실제 자기 충돌 재개
            yield return null; // 실제 몸통과 충돌 준비 대기
        }

        private IEnumerator LongBodySelfAndObstacle() // 긴 몸통 자기 충돌과 장애물 동시 접촉
        {
            body.BodySegments[19].transform.position = player.transform.position; // 스무 번째 몸통과 머리 접촉
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 같은 스텝 장애물 접촉
            Physics2D.SyncTransforms(); // 실제 충돌체 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 물리 접촉 대기
            yield return null; // 실제 충돌 결과 대기
            Check(body.CurrentBodyCount == 18 && health.CurrentHealth == 100 && invulnerability.IsInvulnerable, $"Long body self collision mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 두 칸 손실과 무적 우선 확인
        }

        private IEnumerator LongBodySurrounded() // 여러 적의 동시 접촉
        {
            for (int index = 0; index < 4; index++) // 네 방향 적 접촉 구성
            {
                Collider2D enemy = SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 실제 적 직접 충돌체
                float angle = index * Mathf.PI * 0.5f; // 현재 방향 각도
                enemy.transform.position += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.08f; // 머리 주변 네 방향 배치
            }
            Physics2D.SyncTransforms(); // 실제 적 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 동시 접촉 대기
            yield return null; // 실제 충돌 결과 대기
            if (!Check(body.CurrentBodyCount == 19 && health.CurrentHealth == 100 && external.EnemyContactProtected, $"Surrounding enemies mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}.")) // 동시 직접 접촉 단일 손실 확인
            {
                yield break; // 실패 시 후속 검사 중단
            }
            yield return new WaitForFixedUpdate(); // 적 접촉 유지 물리 대기
            yield return null; // 실제 후속 처리 대기
            Check(body.CurrentBodyCount == 19 && health.CurrentHealth == 100, "Persistent surrounding enemies caused duplicate contact loss."); // 직접 접촉 보호 시간 확인
        }

        private IEnumerator LongBodyCorridor() // 스무 칸 직진 통로와 장애물 접촉
        {
            CircleCollider2D head = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌 범위
            if (!Check(head != null, "Player head collider missing.")) // 실제 충돌체 확인
            {
                yield break; // 구성 누락 시 중단
            }
            player.transform.rotation = Quaternion.identity; // 실제 위쪽 직진 방향
            float offset = head.bounds.extents.x + 0.16f; // 머리 크기와 좁은 여유 폭
            Vector3 start = player.transform.position; // 실제 통과 시작 위치
            BoxCollider2D left = SpawnCorridorWall(start + new Vector3(-offset, 1f, 0f)); // 실제 왼쪽 통로 벽
            SpawnCorridorWall(start + new Vector3(offset, 1f, 0f)); // 실제 오른쪽 통로 벽
            Physics2D.SyncTransforms(); // 실제 통로 충돌체 반영
            player.enabled = true; // 실제 자동 전진 시작
            yield return new WaitForSeconds(0.4f); // 실제 통로 이동 대기
            player.enabled = false; // 접촉 시험 전 이동 중단
            if (!Check(player.transform.position.y - start.y > 0.5f && body.CurrentBodyCount == 20 && health.CurrentHealth == 100, $"Narrow passage mismatch: moved={player.transform.position.y - start.y}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}.")) // 직진과 무피해 확인
            {
                yield break; // 통로 실패 시 중단
            }
            left.transform.position = player.transform.position; // 실제 벽과 머리 접촉
            Physics2D.SyncTransforms(); // 실제 접촉 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 장애물 접촉 대기
            yield return null; // 실제 피해 처리 대기
            if (!Check(body.CurrentBodyCount == 20 && health.CurrentHealth == 75 && invulnerability.IsInvulnerable, $"Corridor wall contact mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}.")) // 벽 피해와 무적 확인
            {
                yield break; // 접촉 실패 시 중단
            }
            Collider2D protectedEnemy = SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 무적 중 추가 적 접촉
            Physics2D.SyncTransforms(); // 추가 적 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 적 접촉 대기
            yield return null; // 실제 후속 처리 대기
            if (!Check(body.CurrentBodyCount == 20 && health.CurrentHealth == 75, "Corridor invulnerability did not block enemy contact.")) // 기존 무적의 적 접촉 차단 확인
            {
                yield break; // 무적 시험 실패 시 중단
            }
            protectedEnemy.enabled = false; // 무적 이후 적 접촉 차단
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 무적 종료 제한 시각
            while (invulnerability.IsInvulnerable && Time.realtimeSinceStartup < deadline) // 장애물 무적의 실제 종료 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 무적 시간 진행
            }
            yield return new WaitForFixedUpdate(); // 종료 후 물리 상태 반영
            yield return null; // 안전 위치 이동 처리 대기
            if (!Check(!invulnerability.IsInvulnerable && !head.Distance(left).isOverlapped && body.CurrentBodyCount == 20 && health.CurrentHealth == 75, "Player remained inside the obstacle after invulnerability ended.")) // 무적 종료 후 안전 위치 확인
            {
                yield break; // 안전 위치 시험 실패 시 중단
            }
            left.transform.position = player.transform.position; // 안전 위치 이동 직후 재접촉 구성
            Collider2D enemyDuringGrace = SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 유예 중 실제 적 직접 접촉
            Physics2D.SyncTransforms(); // 실제 재접촉 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 재접촉 물리 대기
            yield return null; // 실제 피해 처리 대기
            if (!Check(body.CurrentBodyCount == 19 && health.CurrentHealth == 75 && external.EnemyContactProtected, "Obstacle recovery grace did not block the wall or incorrectly blocked enemy contact.")) // 벽 유예와 적 직접 충돌 독립 확인
            {
                yield break; // 유예 시험 실패 시 중단
            }
            enemyDuringGrace.enabled = false; // 후속 적 접촉 차단
            player.GetComponent<SnakeSelfCollision>().enabled = false; // 시험용 몸통 재배치의 추가 자기 접촉 차단
            left.transform.position = player.transform.position + Vector3.left * 8f; // 장애물 접촉 해제
            Physics2D.SyncTransforms(); // 실제 분리 위치 반영
            yield return new WaitForSeconds(0.35f); // 장애물 재접촉 유예 종료 대기
            left.transform.position = player.transform.position; // 유예 종료 후 장애물 재접촉
            Physics2D.SyncTransforms(); // 실제 재접촉 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 장애물 재접촉 대기
            yield return null; // 실제 피해 처리 대기
            Check(body.CurrentBodyCount == 19 && health.CurrentHealth == 50 && invulnerability.IsInvulnerable, $"Obstacle re-entry mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}, invulnerable={invulnerability.IsInvulnerable}, overlap={head.Distance(left).isOverlapped}."); // 유예 종료 후 실제 피해 재개 확인
        }

        private BoxCollider2D SpawnCorridorWall(Vector3 position) // 시험용 통로 벽 생성
        {
            GameObject wall = new GameObject("CollisionVerificationCorridorWall"); // 실제 시험 벽 객체
            spawned.Add(wall); // 시험 종료 정리 대상
            wall.transform.position = position; // 실제 통로 벽 위치
            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>(); // 실제 벽 충돌체
            collider.size = new Vector2(0.2f, 3f); // 좁은 통로 벽 크기
            collider.isTrigger = true; // 실제 장애물 접촉 설정
            wall.AddComponent<SnakeContactHazard>().Configure(SnakeContactHazardType.Obstacle); // 기존 장애물 처리 연결
            return collider; // 생성한 실제 벽 반환
        }

        private IEnumerator ObstacleExitDuringInvulnerability() // 무적 종료 전 실제 장애물 이탈
        {
            Collider2D obstacle = SpawnHazard(SnakeContactHazardType.Obstacle, true); // 실제 장애물 접촉
            Physics2D.SyncTransforms(); // 실제 장애물 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 장애물 충돌 대기
            yield return null; // 실제 피해 처리 대기
            if (!Check(health.CurrentHealth == 75 && invulnerability.IsInvulnerable, "Initial obstacle hit failed.")) // 실제 장애물 피해와 무적 확인
            {
                yield break; // 초기 접촉 실패 시 중단
            }
            obstacle.enabled = false; // 무적 중 실제 장애물 이탈
            Vector3 position = player.transform.position; // 장애물 이탈 후 머리 위치
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 무적 종료 제한 시각
            while (invulnerability.IsInvulnerable && Time.realtimeSinceStartup < deadline) // 기존 무적 종료 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 무적 시간 진행
            }
            yield return new WaitForFixedUpdate(); // 실제 무적 종료 처리 대기
            yield return null; // 후속 위치 처리 대기
            if (!Check(!invulnerability.IsInvulnerable && Vector3.Distance(position, player.transform.position) < 0.001f, "Player moved after leaving the obstacle during invulnerability.")) // 이미 이탈한 머리 위치 보존 확인
            {
                yield break; // 위치 확인 실패 시 중단
            }
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 별도 장애물 즉시 접촉
            Physics2D.SyncTransforms(); // 실제 신규 장애물 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 신규 충돌 대기
            yield return null; // 실제 신규 피해 처리 대기
            Check(health.CurrentHealth == 50 && invulnerability.IsInvulnerable, "Leaving the obstacle incorrectly started a recovery grace period."); // 불필요한 재접촉 유예 방지
        }

        private IEnumerator SingleObstacleNearestRecovery() // 단일 장애물의 실제 최소 이탈 거리
        {
            BoxCollider2D wall = SpawnCorridorWall(player.transform.position + Vector3.right * 0.05f); // 실제 머리와 겹친 벽
            CircleCollider2D head = player.GetComponent<CircleCollider2D>(); // 실제 머리 충돌체
            Physics2D.SyncTransforms(); // 실제 벽과 머리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 장애물 접촉 대기
            yield return null; // 실제 피해 처리 대기
            if (!Check(health.CurrentHealth == 75 && invulnerability.IsInvulnerable, "Single obstacle hit failed.")) // 실제 장애물 피해 확인
            {
                yield break; // 초기 접촉 실패 시 중단
            }
            ColliderDistance2D separation = head.Distance(wall); // 실제 최소 분리 거리
            Vector2 expected = (Vector2)player.transform.position + separation.normal * (separation.distance - 0.01f); // 실제 최소 안전 위치
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 무적 종료 제한 시각
            while (invulnerability.IsInvulnerable && Time.realtimeSinceStartup < deadline) // 실제 무적 종료 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 무적 시간 진행
            }
            yield return new WaitForFixedUpdate(); // 실제 이탈 처리 대기
            yield return null; // 실제 이동 반영 대기
            Check(!head.Distance(wall).isOverlapped && Vector2.Distance(player.transform.position, expected) < 0.02f, "Single obstacle did not use its nearest safe boundary."); // 실제 최소 경계와 장애물 이탈 확인
        }

        private IEnumerator ProjectileAndObstacle(bool withShield) // 투사체 및 보호막 시험
        {
            if (withShield) // 보호막 시험 확인
            {
                shield.Apply(50, 10f, 2, 10f); // 공용 보호막 적용
            }
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 장애물 생성
            GameObject shot = new GameObject("CollisionVerificationProjectile"); // 실제 시험 투사체
            spawned.Add(shot); // 시험 객체 등록
            shot.transform.position = player.transform.position; // 머리 위치 배치
            EnemyTestProjectile projectile = shot.AddComponent<EnemyTestProjectile>(); // 실제 투사체 동작
            projectile.Configure(Vector2.right, 0f, 10f, 8, health, body, player, null); // 정지 명중 설정
            Physics2D.SyncTransforms(); // 충돌체 물리 동기화
            yield return new WaitForFixedUpdate(); // 실제 충돌 대기
            yield return null; // 투사체 제거와 해결 대기
            if (withShield) // 보호막 결과 확인
            {
                Check(body.CurrentBodyCount == 3 && health.CurrentHealth == 100 && shield.CurrentShield == 25 && invulnerability.IsInvulnerable, $"Shielded obstacle mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}, shield={shield.CurrentShield}."); // 보호막 흡수와 투사체 차단 확인
            }
            else // 보호막 없는 결과 확인
            {
                Check(body.CurrentBodyCount == 3 && health.CurrentHealth == 75 && invulnerability.IsInvulnerable, $"Obstacle/projectile mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 장애물 피해와 신규 무적 확인
            }
            Check(shot == null, "Projectile was not destroyed after contact."); // 투사체 파괴 확인
        }

        private IEnumerator PersistentEnemyContact() // 지속 접촉 재적용 방지 시험
        {
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 지속 적 접촉 생성
            Physics2D.SyncTransforms(); // 물리 동기화
            yield return new WaitForFixedUpdate(); // 첫 접촉 대기
            yield return null; // 직접 충돌 해결 대기
            if (!Check(body.CurrentBodyCount == 2, "Initial direct enemy contact did not remove one body.")) // 최초 적 피해 확인
            {
                yield break; // 실패 중단
            }
            yield return new WaitForSeconds(1.2f); // 직접 보호 종료 대기
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 기존 적 접촉 중 장애물 진입
            Physics2D.SyncTransforms(); // 장애물 물리 동기화
            yield return new WaitForFixedUpdate(); // 신규 장애물 접촉 대기
            yield return null; // 후속 해결 대기
            Check(body.CurrentBodyCount == 2 && health.CurrentHealth == 75, $"Persistent enemy overlap mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 기존 접촉 재적용 방지 확인
        }

        private IEnumerator MeleeAndObstacle() // 근접 피해와 장애물 시험
        {
            EarlyEnemyBehaviour enemy = SpawnStationaryEnemy(EarlyEnemyKind.Chaser); // 정지 추적자 생성
            Physics2D.SyncTransforms(); // 적 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 초기 적 진입 대기
            yield return null; // 첫 콜백 완료 대기
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", PrivateInstance).SetValue(enemy, 0f); // 다음 근접 공격 허용
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 같은 스텝 장애물 진입
            Physics2D.SyncTransforms(); // 장애물 위치 동기화
            yield return new WaitForFixedUpdate(); // 근접 유지와 장애물 물리 처리
            yield return null; // 물리 후 피해 해결 대기
            Check(body.CurrentBodyCount == 3 && health.CurrentHealth == 75 && invulnerability.IsInvulnerable, $"Melee/obstacle mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 장애물 우선 및 근접 차단 확인
        }

        private IEnumerator EliteCutAndObstacle() // 엘리트 돌진 절단과 장애물 시험
        {
            EarlyEnemyBehaviour enemy = SpawnStationaryEnemy(EarlyEnemyKind.Elite); // 정지 엘리트 생성
            Physics2D.SyncTransforms(); // 적 위치 동기화
            yield return new WaitForFixedUpdate(); // 초기 적 진입 대기
            yield return null; // 첫 콜백 완료 대기
            typeof(EarlyEnemyBehaviour).GetField("chargePhase", PrivateInstance).SetValue(enemy, 2); // 돌진 단계 설정
            typeof(EarlyEnemyBehaviour).GetField("chargePhaseEnd", PrivateInstance).SetValue(enemy, Time.time + 10f); // 돌진 지속 설정
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", PrivateInstance).SetValue(enemy, Time.time + 10f); // 일반 근접 공격 보류
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 같은 스텝 장애물 진입
            Physics2D.SyncTransforms(); // 장애물 위치 동기화
            yield return new WaitForFixedUpdate(); // 돌진 유지와 장애물 처리
            yield return null; // 물리 후 피해 해결 대기
            bool cutRecorded = (bool)typeof(EarlyEnemyBehaviour).GetField("chargeCut", PrivateInstance).GetValue(enemy); // 절단 완료 상태 조회
            Check(body.CurrentBodyCount == 2 && health.CurrentHealth == 75 && cutRecorded, $"Elite/obstacle mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}, cut={cutRecorded}."); // 절단 우선과 완료 확인
        }

        private EarlyEnemyBehaviour SpawnStationaryEnemy(EarlyEnemyKind kind) // 정지 적 접촉 생성
        {
            GameObject enemy = new GameObject("CollisionVerificationEnemy_" + kind); // 시험 적 객체
            spawned.Add(enemy); // 시험 객체 등록
            enemy.transform.position = player.transform.position; // 머리와 같은 위치
            CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>(); // 적 충돌체 생성
            collider.radius = 0.1f; // 시험 충돌 범위
            collider.isTrigger = true; // 트리거 판정 설정
            Rigidbody2D rigidbody = enemy.AddComponent<Rigidbody2D>(); // 적 물리체 생성
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 정지 물리체 설정
            rigidbody.useFullKinematicContacts = true; // 운동학 접촉 허용
            EarlyEnemySettings settings = new EarlyEnemySettings(); // 적 임시 설정
            settings.kind = kind; // 적 행동 종류
            settings.damage = 10; // 시험 근접 피해
            settings.speedRatio = 0f; // 시험 이동 속도
            settings.attackInterval = 10f; // 시험 공격 간격
            EarlyEnemyBehaviour behaviour = enemy.AddComponent<EarlyEnemyBehaviour>(); // 실제 적 행동 추가
            behaviour.Configure(settings, player, body, health, null, 4f, 5f, 3f, 1f, 0.6f, 3f); // 실제 적 동작 연결
            return behaviour; // 시험 적 반환
        }

        private IEnumerator PausedDuringContact() // 같은 물리 단계의 일시정지 시험
        {
            probe.PauseOnEntry = true; // 첫 물리 콜백 중 정지 요청
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 장애물 생성
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 직접 충돌 생성
            Physics2D.SyncTransforms(); // 충돌체 물리 동기화
            float deadline = Time.realtimeSinceStartup + 3f; // 접촉 대기 제한
            while (!GameManager.Instance.IsPaused && Time.realtimeSinceStartup < deadline) // 물리 콜백의 정지 대기
            {
                yield return null; // 렌더 프레임 대기
            }
            yield return null; // 정지 상태의 물리 후 해결 대기
            Check(GameManager.Instance.IsPaused && body.CurrentBodyCount == 2 && health.CurrentHealth == 75, $"Paused contact mismatch: paused={GameManager.Instance.IsPaused}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 정지 전 접촉 해결 확인
        }

        private IEnumerator DestroyedHazards() // 접촉 직후 제거되는 위험 시험
        {
            probe.DestroyOnEntry = true; // 접촉 원본의 지연 제거 요청
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 장애물 생성
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 직접 충돌 생성
            Physics2D.SyncTransforms(); // 충돌체 물리 동기화
            yield return new WaitForFixedUpdate(); // 실제 접촉 처리 대기
            yield return null; // 물리 해결과 객체 제거 대기
            Check(probe.Entries.Count == 2 && body.CurrentBodyCount == 2 && health.CurrentHealth == 75, $"Destroyed hazard mismatch: callbacks={probe.Entries.Count}, body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 수집된 접촉 보존 확인
        }

        private IEnumerator MultiplePhysicsSteps() // 같은 렌더 프레임의 물리 스텝 분리 시험
        {
            CollisionVerificationSequencer sequence = gameObject.AddComponent<CollisionVerificationSequencer>(); // 물리 단계 생성기
            sequence.Runner = this; // 시험 실행기 연결
            float previousMaximum = Time.maximumDeltaTime; // 최대 프레임 시간 보관
            Time.maximumDeltaTime = 0.5f; // 여러 물리 단계 허용
            System.Threading.Thread.Sleep(250); // 다음 프레임까지 물리 시간 누적
            yield return null; // 누적 물리 단계 처리 대기
            yield return null; // 후속 해결 대기
            Time.maximumDeltaTime = previousMaximum; // 최대 프레임 시간 복구
            bool distinctSteps = sequence.SecondTime > sequence.FirstTime; // 서로 다른 물리 단계 확인
            bool sameFrame = sequence.FirstFrame == sequence.SecondFrame && sequence.FirstFrame >= 0; // 같은 렌더 프레임 확인
            Debug.Log($"[CollisionVerification] Multiple fixed steps: frames={sequence.FirstFrame},{sequence.SecondFrame} times={sequence.FirstTime:F3},{sequence.SecondTime:F3}"); // 실제 스텝 기록
            Destroy(sequence); // 시험 생성기 제거
            if (!Check(distinctSteps && sameFrame, "The test did not observe two physical steps in one render frame.")) // 실제 시험 조건 확인
            {
                yield break; // 조건 실패 중단
            }
            Check(body.CurrentBodyCount == 3 && health.CurrentHealth == 75 && invulnerability.IsInvulnerable, $"Separated steps mismatch: body={body.CurrentBodyCount}, hp={health.CurrentHealth}."); // 첫 스텝 무적의 다음 스텝 적용 확인
        }

        private IEnumerator DisabledPendingCancellation() // 처리기 비활성화 예약 취소 시험
        {
            EarlyEnemyBehaviour enemy = SpawnStationaryEnemy(EarlyEnemyKind.Elite); // 시험 엘리트 생성
            enemy.GetComponent<Collider2D>().enabled = false; // 실제 충돌 대신 인위 콜백 선택
            typeof(EarlyEnemyBehaviour).GetField("chargePhase", PrivateInstance).SetValue(enemy, 2); // 돌진 단계 설정
            typeof(EarlyEnemyBehaviour).GetField("chargePhaseEnd", PrivateInstance).SetValue(enemy, Time.time + 10f); // 돌진 종료 시간 설정
            typeof(EarlyEnemyBehaviour).GetField("nextAttack", PrivateInstance).SetValue(enemy, Time.time + 10f); // 일반 공격 차단
            FieldInfo pending = typeof(EarlyEnemyBehaviour).GetField("chargeCutPending", PrivateInstance); // 절단 대기 필드 조회
            if (!Check(pending != null, "Elite queued-cut state is missing.")) // 대기 상태 확인
            {
                yield break; // 실패 중단
            }
            MethodInfo stay = typeof(EarlyEnemyBehaviour).GetMethod("OnTriggerStay2D", PrivateInstance); // 실제 적 유지 콜백 조회
            object[] arguments = new object[] { player.GetComponent<Collider2D>() }; // 머리 충돌 인자
            stay.Invoke(enemy, arguments); // 인위적인 돌진 접촉
            if (!Check((bool)pending.GetValue(enemy), "Elite cut was not queued before disabling.")) // 예약 상태 확인
            {
                yield break; // 실패 중단
            }
            external.enabled = false; // 대기 충돌 처리기 비활성화
            bool cut = (bool)typeof(EarlyEnemyBehaviour).GetField("chargeCut", PrivateInstance).GetValue(enemy); // 절단 성공 상태
            if (!Check(!(bool)pending.GetValue(enemy) && !cut && body.CurrentBodyCount == 3, "Disabling did not cancel the pending elite cut.")) // 취소와 몸통 유지 확인
            {
                yield break; // 실패 중단
            }
            external.enabled = true; // 처리기 재활성화
            stay.Invoke(enemy, arguments); // 동일 엘리트 접촉 재시도
            yield return new WaitForFixedUpdate(); // 다음 물리 단계 해결 대기
            yield return null; // 물리 후 콜백 대기
            cut = (bool)typeof(EarlyEnemyBehaviour).GetField("chargeCut", PrivateInstance).GetValue(enemy); // 절단 성공 상태 재조회
            Check(body.CurrentBodyCount == 2 && cut && !(bool)pending.GetValue(enemy), "Re-enabled handler did not resolve the next elite cut."); // 재활성화와 정상 절단 확인
        }

        private IEnumerator DispatcherLifecycle() // 물리 직후 실행 단계와 재등록 시험
        {
            Type dispatcher = typeof(SnakeExternalCollision).Assembly.GetType("ProjectEpsilon.Player.SnakePhysicsStepDispatcher"); // 물리 완료 처리기 형식
            if (!Check(dispatcher != null, "Physics dispatcher type is missing.")) // 처리기 존재 확인
            {
                yield break; // 실패 중단
            }
            Type marker = dispatcher.GetNestedType("AfterPhysics2D", BindingFlags.NonPublic); // 물리 직후 단계 표식
            if (!Check(marker != null && HasSinglePhysicsMarker(marker), "Physics dispatcher is missing, duplicated, or not immediately after Physics2DFixedUpdate.")) // 초기 플레이어루프 확인
            {
                yield break; // 실패 중단
            }
            MethodInfo reset = dispatcher.GetMethod("ResetState", BindingFlags.NonPublic | BindingFlags.Static); // 구독 세대 초기화 조회
            if (!Check(reset != null, "Dispatcher lifecycle reset is missing.")) // 재설정 메서드 확인
            {
                yield break; // 실패 중단
            }
            reset.Invoke(null, null); // 도메인 재사용 초기화 모사
            yield return new WaitForFixedUpdate(); // 처리기 재등록 물리 단계
            yield return null; // 물리 후 등록 대기
            if (!Check(HasSinglePhysicsMarker(marker), "Dispatcher was duplicated after lifecycle reset.")) // 재등록 단계 중복 확인
            {
                yield break; // 실패 중단
            }
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 재등록 이후 실제 적 진입
            Physics2D.SyncTransforms(); // 충돌체 동기화
            yield return new WaitForFixedUpdate(); // 실제 접촉 대기
            yield return null; // 물리 후 처리 대기
            Check(body.CurrentBodyCount == 2 && health.CurrentHealth == 100, "Dispatcher did not process contact after lifecycle reset."); // 재등록 후 실제 처리 확인
            Debug.Log("[CollisionVerification] Dispatcher marker and synthetic lifecycle reset verified."); // 인위적 수명주기 시험 기록
        }

        private static bool HasSinglePhysicsMarker(Type marker) // 물리 직후 단계 위치 확인
        {
            int count = 0; // 전체 표식 수
            bool correctlyPlaced = false; // 물리 직후 여부
            InspectLoop(PlayerLoop.GetCurrentPlayerLoop(), marker, ref count, ref correctlyPlaced); // 실행 순서 트리 검사
            return count == 1 && correctlyPlaced; // 유일성과 위치 반환
        }

        private static void InspectLoop(PlayerLoopSystem node, Type marker, ref int count, ref bool correctlyPlaced) // 실행 순서 재귀 검사
        {
            if (node.type == marker) // 현재 표식 확인
            {
                count++; // 표식 수 증가
            }
            if (node.subSystemList == null) // 하위 단계 부재 확인
            {
                return; // 탐색 종료
            }
            for (int index = 0; index < node.subSystemList.Length; index++) // 하위 단계 순회
            {
                PlayerLoopSystem child = node.subSystemList[index]; // 현재 하위 단계
                if (child.type == typeof(UnityEngine.PlayerLoop.FixedUpdate.Physics2DFixedUpdate) && index + 1 < node.subSystemList.Length && node.subSystemList[index + 1].type == marker) // 물리 직후 표식 확인
                {
                    correctlyPlaced = true; // 정상 위치 기록
                }
                InspectLoop(child, marker, ref count, ref correctlyPlaced); // 하위 트리 탐색
            }
        }

        private IEnumerator LastBodyDefeat() // 동시 충돌 패배 시험
        {
            body.RemoveBodies(2); // 마지막 몸통만 유지
            SpawnHazard(SnakeContactHazardType.Obstacle, true); // 장애물 생성
            SpawnHazard(SnakeContactHazardType.EnemyDirect, true); // 직접 충돌 생성
            Physics2D.SyncTransforms(); // 충돌체 동기화
            float deadline = Time.realtimeSinceStartup + 3f; // 패배 대기 제한
            while (GameManager.Instance.CurrentState != GameState.Defeated && Time.realtimeSinceStartup < deadline) // 실제 패배 대기
            {
                yield return null; // 렌더 프레임 대기
            }
            yield return null; // 패배 후 해결 대기
            Check(body.CurrentBodyCount == 0 && health.CurrentHealth == 0 && GameManager.Instance.CurrentState == GameState.Defeated, "Last body direct contact did not cause defeat."); // 마지막 몸통 우선 확인
        }

        public Collider2D SpawnHazard(SnakeContactHazardType type, bool enabledCollider) // 시험 위험 생성
        {
            GameObject item = new GameObject("CollisionVerification_" + type); // 시험 위험 객체
            spawned.Add(item); // 시험 객체 저장
            item.transform.position = player.transform.position; // 머리와 같은 위치
            CircleCollider2D collider = item.AddComponent<CircleCollider2D>(); // 위험 충돌체 생성
            collider.radius = 0.1f; // 시험 접촉 크기
            collider.isTrigger = true; // 트리거 판정 설정
            collider.enabled = enabledCollider; // 실제 또는 인위 콜백 선택
            SnakeContactHazard hazard = item.AddComponent<SnakeContactHazard>(); // 위험 종류 구성
            hazard.Configure(type); // 충돌 종류 설정
            return collider; // 생성 충돌체 반환
        }

        private bool Check(bool condition, string message) // 결과 확인
        {
            if (condition) // 성공 조건 확인
            {
                return true; // 정상 진행
            }
            enabled = false; // 남은 시험 중단
            AutomatedCollisionPriorityVerification.Finish(1, message); // 실패 종료 요청
            return false; // 검증 실패
        }
    }
}
