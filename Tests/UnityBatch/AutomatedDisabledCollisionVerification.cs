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

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedDisabledCollisionVerification // 비활성 물리 충돌 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonDisabledCollisionVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedDisabledCollisionVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 배치 진행 확인
            { // 조건 시작
                EditorApplication.update -= Tick; // 중복 연결 제거
                EditorApplication.update += Tick; // 플레이 시작 감시
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // Unity 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 배치 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 플레이 시작 감시
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 시작 선택과 플레이 진입 감시
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 진입 대기
            } // 조건 끝
            if (started <= 0d) // 최초 갱신 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 시작 시각 기록
            } // 조건 끝
            if (EditorApplication.timeSinceStartup - started > 120d) // 전체 시험 제한
            { // 조건 시작
                Finish(1, "Timed out waiting for controlled disabled collision test."); // 제한 시간 실패
                return; // 감시 중단
            } // 조건 끝
            if (StartingWeaponBatchHelper.HandleStartingSelection()) // 실제 시작 무기 첫 후보 선택
            { // 조건 시작
                return; // 실제 게임 시작 대기
            } // 조건 끝
            if (injected) // 시험 실행기 존재 확인
            { // 조건 시작
                return; // 시험 결과 대기
            } // 조건 끝
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 대기
            } // 조건 끝
            injected = true; // 실행기 중복 생성 방지
            player.gameObject.AddComponent<DisabledCollisionVerificationRunner>(); // 실제 씬 시험 실행
        } // 메서드 끝

        public static void Finish(int code, string message) // 결과 기록과 프로세스 종료
        { // 메서드 시작
            if (finished) // 중복 종료 확인
            { // 조건 시작
                return; // 중복 처리 방지
            } // 조건 끝
            finished = true; // 종료 상태 기록
            SessionState.SetBool(ActiveKey, false); // 진행 표식 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log($"[DisabledCollisionVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        } // 메서드 끝
    } // 클래스 끝

    [DefaultExecutionOrder(1000)] // 실제 콜백 뒤 상태 관찰 순서
    public sealed class DisabledCollisionProbe : MonoBehaviour // 실제 물리 콜백 관찰기
    { // 클래스 시작
        public SnakeSelfCollision Self; // 실제 자기 충돌 처리기
        public SnakeExternalCollision External; // 실제 외부 충돌 처리기
        public bool ReenableSelfOnEntry; // 같은 물리 스텝 재활성 여부
        public bool PendingBeforeReenable; // 재활성 전 실제 예약 여부
        public int HazardEntries; // 위험 물리 진입 횟수
        public int SegmentEntries; // 몸통 물리 진입 횟수
        public int EnemyEntries; // 적 물리 진입 횟수
        public int EnemyStays; // 적 물리 지속 횟수
        public int ProjectileEntries; // 적 투사체 물리 진입 횟수
        public float CallbackFixedTime; // 실제 물리 콜백 시각

        private void OnTriggerEnter2D(Collider2D other) // 실제 물리 진입 관찰
        { // 메서드 시작
            CallbackFixedTime = Time.fixedTime; // 실제 물리 시각 기록
            if (other.GetComponent<SnakeContactHazard>() != null) // 실제 위험 충돌체 확인
            { // 조건 시작
                HazardEntries++; // 위험 접촉 횟수 증가
            } // 조건 끝
            if (other.GetComponent<EarlyEnemyBehaviour>() != null) // 실제 적 충돌체 확인
            { // 조건 시작
                EnemyEntries++; // 적 진입 횟수 증가
            } // 조건 끝
            if (other.GetComponent<EnemyTestProjectile>() != null) // 실제 적 투사체 확인
            { // 조건 시작
                ProjectileEntries++; // 투사체 진입 횟수 증가
            } // 조건 끝
            SnakeSegment segment = other.GetComponent<SnakeSegment>(); // 실제 몸통 조회
            if (segment == null || !segment.IsBody) // 몸통 여부 확인
            { // 조건 시작
                return; // 다른 접촉 무시
            } // 조건 끝
            SegmentEntries++; // 실제 몸통 접촉 기록
            if (ReenableSelfOnEntry && Self != null && External != null) // 같은 물리 스텝 재활성 시험
            { // 조건 시작
                FieldInfo pending = typeof(SnakeExternalCollision).GetField("pendingSelfCollision", BindingFlags.NonPublic | BindingFlags.Instance); // 실제 예약 필드 조회
                PendingBeforeReenable = pending != null && ReferenceEquals(pending.GetValue(External), Self); // 재활성 전 예약 상태 기록
                ReenableSelfOnEntry = false; // 재활성 중복 방지
                Self.enabled = true; // 같은 물리 콜백 중 재활성화
            } // 조건 끝
        } // 메서드 끝

        private void OnTriggerStay2D(Collider2D other) // 실제 지속 접촉 관찰
        { // 메서드 시작
            if (other.GetComponent<EarlyEnemyBehaviour>() != null) // 실제 적 충돌체 확인
            { // 조건 시작
                EnemyStays++; // 실제 적 지속 접촉 기록
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝

    public sealed class DisabledCollisionVerificationRunner : MonoBehaviour // 실제 비활성 물리 시험
    { // 클래스 시작
        private static readonly BindingFlags PrivateFields = BindingFlags.NonPublic | BindingFlags.Instance; // 실제 내부 상태 조회
        private readonly List<GameObject> spawned = new List<GameObject>(); // 통제 객체 목록
        private SnakeMovement player; // 실제 머리 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeBodyFollower follower; // 실제 몸통 추적
        private SnakeHealth health; // 실제 공용 체력
        private SnakeInvulnerability invulnerability; // 실제 무적 상태
        private SnakeExternalCollision external; // 실제 외부 충돌 처리
        private SnakeSelfCollision self; // 실제 자기 충돌 처리
        private SnakeShieldController shield; // 실제 공용 보호막
        private DisabledCollisionProbe headProbe; // 머리 실제 접촉 관찰
        private int failures; // 독립 사례 실패 수

        private IEnumerator Start() // 실제 씬 사례 실행 순서
        { // 코루틴 시작
            player = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            invulnerability = GetComponent<SnakeInvulnerability>(); // 실제 무적 조회
            external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌기 조회
            self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌기 조회
            shield = health == null ? null : health.ShieldController; // 실제 보호막 조회
            Require(player != null && body != null && follower != null && health != null && invulnerability != null && external != null && self != null && shield != null, "Actual Game scene collision fixtures are missing."); // 필수 씬 구성 확인
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 자연 출현기 조회
            if (spawner != null) // 자연 출현기 존재 확인
            { // 조건 시작
                spawner.enabled = false; // 통제 시험 중 적 출현 중지
            } // 조건 끝
            player.enabled = false; // 통제 시험 중 머리 이동 정지
            follower.enabled = false; // 시험 중 몸통 위치 고정
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 자연 생성 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 기존 적 즉시 제외
                Destroy(enemy.gameObject); // 기존 적 제거
            } // 반복 끝
            headProbe = gameObject.AddComponent<DisabledCollisionProbe>(); // 실제 머리 접촉 관찰기 추가
            headProbe.External = external; // 실제 외부 충돌기 연결
            headProbe.Self = self; // 실제 자기 충돌기 연결
            Debug.Log("[DisabledCollisionVerification] Actual Game scene and real Physics2D callbacks; player motion, body following and natural spawning disabled; controlled contacts and shield."); // 실제 시험 조건 기록
            yield return ResetFixture(); // 외부 충돌 시험 초기화
            yield return VerifyExternal(); // 외부 충돌기 비활성 및 재접촉
            yield return ResetFixture(); // 자기 충돌 시험 초기화
            yield return VerifySelf(); // 자기 충돌 비활성 및 같은 스텝 재활성
            yield return ResetFixture(); // 근접 적 시험 초기화
            yield return VerifyEnemy(); // 적 행동 비활성 및 재접촉
            yield return ResetFixture(); // 적 투사체 시험 초기화
            yield return VerifyEnemyProjectile(); // 적 투사체 비활성 및 재접촉
            yield return ResetFixture(); // 우선순위 시험 초기화
            yield return VerifyPriorityShield(); // 실제 동시 접촉과 보호막
            AutomatedDisabledCollisionVerification.Finish(failures == 0 ? 0 : 1, $"Actual independent cases completed; failures={failures}."); // 전체 시험 결과
        } // 코루틴 끝

        private IEnumerator ResetFixture() // 실제 충돌 사례 초기화
        { // 코루틴 시작
            foreach (GameObject item in spawned) // 이전 통제 객체 순회
            { // 반복 시작
                if (item != null) // 객체 존재 확인
                { // 조건 시작
                    item.SetActive(false); // 실제 충돌 즉시 차단
                    Destroy(item); // 이전 객체 지연 파괴
                } // 조건 끝
            } // 반복 끝
            spawned.Clear(); // 이전 통제 객체 기록 초기화
            external.enabled = true; // 실제 물리 처리기 재활성
            self.enabled = false; // 통제 중 자기 충돌 중지
            invulnerability.ClearInvulnerability(); // 실제 전체 무적 해제
            shield.Clear(); // 실제 보호막 제거
            body.ResetBody(); // 실제 시작 몸통 복구
            health.ResetHealth(); // 실제 공용 체력 복구
            external.Bind(body, health, invulnerability, 1, 1f, 25, 1.5f); // 충돌 설정 및 보호 시간 초기화
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            { // 반복 시작
                body.BodySegments[index].transform.position = player.transform.position + Vector3.right * (20f + index * 5f); // 충돌 없이 멀리 배치
            } // 반복 끝
            headProbe.HazardEntries = 0; // 위험 접촉 기록 초기화
            headProbe.SegmentEntries = 0; // 자기 접촉 기록 초기화
            headProbe.ProjectileEntries = 0; // 투사체 접촉 기록 초기화
            headProbe.ReenableSelfOnEntry = false; // 재활성 시험 초기화
            headProbe.PendingBeforeReenable = false; // 물리 예약 기록 초기화
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return null; // 지연 파괴 완료 대기
            yield return new WaitForFixedUpdate(); // 실제 접촉 해제 스텝
            yield return null; // 물리 후 처리 대기
        } // 코루틴 끝

        private IEnumerator VerifyExternal() // 비활성 외부 충돌기와 실제 재접촉
        { // 코루틴 시작
            external.enabled = false; // 실제 외부 충돌 컴포넌트 비활성화
            GameObject hazard = SpawnHazard(SnakeContactHazardType.Obstacle); // 실제 장애물 생성
            Physics2D.SyncTransforms(); // 실제 물리 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 진입 스텝
            yield return null; // 물리 직후 판정 대기
            bool ignored = headProbe.HazardEntries > 0 && health.CurrentHealth == 100 && !invulnerability.IsInvulnerable; // 실제 비활성 접촉 피해 확인
            Record("disabled external", ignored, $"entries={headProbe.HazardEntries}, hp={health.CurrentHealth}, invulnerable={invulnerability.IsInvulnerable}"); // 독립 결과 기록
            hazard.transform.position = player.transform.position + Vector3.up * 5f; // 기존 장애물과 분리
            Physics2D.SyncTransforms(); // 실제 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 분리 스텝
            invulnerability.ClearInvulnerability(); // 이전 피해 무적 초기화
            health.ResetHealth(); // 이전 피해 체력 초기화
            external.enabled = true; // 같은 외부 충돌기 재활성화
            external.Bind(body, health, invulnerability, 1, 1f, 25, 1.5f); // 보호 시간 초기화
            hazard.transform.position = player.transform.position; // 같은 장애물 재접촉
            Physics2D.SyncTransforms(); // 실제 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 재진입 스텝
            yield return null; // 물리 직후 해결 대기
            Record("reenabled external", headProbe.HazardEntries >= 2 && health.CurrentHealth == 75 && invulnerability.IsInvulnerable, $"entries={headProbe.HazardEntries}, hp={health.CurrentHealth}, invulnerable={invulnerability.IsInvulnerable}"); // 재접촉 정상 판정 확인
        } // 코루틴 끝

        private IEnumerator VerifySelf() // 비활성 자기 충돌과 같은 스텝 재활성
        { // 코루틴 시작
            self.enabled = false; // 실제 자기 충돌기 비활성화
            headProbe.ReenableSelfOnEntry = true; // 같은 물리 콜백 중 재활성 예약
            body.BodySegments[2].transform.position = player.transform.position; // 실제 몸통을 머리에 접촉
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 자기 접촉 스텝
            yield return null; // 물리 직후 충돌 해결 대기
            Record("disabled self same-step reenable", headProbe.SegmentEntries > 0 && self.enabled && body.CurrentBodyCount == 3 && !invulnerability.IsInvulnerable, $"entries={headProbe.SegmentEntries}, pendingBeforeReenable={headProbe.PendingBeforeReenable}, callbackFixedTime={headProbe.CallbackFixedTime:F3}, body={body.CurrentBodyCount}, invulnerable={invulnerability.IsInvulnerable}"); // 실제 예약과 피해 분리 기록
        } // 코루틴 끝

        private IEnumerator VerifyEnemy() // 비활성 적 행동과 실제 지속 접촉
        { // 코루틴 시작
            SnakeSegment segment = body.BodySegments[0]; // 실제 머리에서 떨어진 몸통
            DisabledCollisionProbe probe = segment.gameObject.AddComponent<DisabledCollisionProbe>(); // 실제 몸통 접촉 관찰기
            GameObject enemyObject = new GameObject("DisabledCollisionEnemy"); // 실제 통제 적 객체
            spawned.Add(enemyObject); // 통제 객체 등록
            enemyObject.transform.position = segment.transform.position; // 실제 몸통 접촉 배치
            CircleCollider2D collider = enemyObject.AddComponent<CircleCollider2D>(); // 실제 적 트리거
            collider.isTrigger = true; // 접촉 이벤트 설정
            collider.radius = 0.1f; // 통제 충돌 반경
            Rigidbody2D rigidbody = enemyObject.AddComponent<Rigidbody2D>(); // 실제 적 물리체
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 운동학 물리 설정
            rigidbody.useFullKinematicContacts = true; // 실제 운동학 접촉 허용
            EarlyEnemyBehaviour enemy = enemyObject.AddComponent<EarlyEnemyBehaviour>(); // 실제 적 행동
            EarlyEnemySettings settings = new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 100f, damage = 10, speedRatio = 0f, attackInterval = 100f }; // 통제 근접 적 설정
            enemy.Configure(settings, player, body, health, null, 4f, 5f, 3f, 1f, 0.6f, 3f); // 실제 적 참조 설정
            SetEnemyAttackReady(enemy); // 실제 근접 공격 준비
            enemy.enabled = false; // 적 행동 컴포넌트만 비활성화
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 진입 스텝
            yield return new WaitForFixedUpdate(); // 실제 지속 접촉 스텝
            yield return null; // 물리 직후 피해 해결 대기
            Record("disabled enemy behaviour", probe.EnemyStays > 0 && health.CurrentHealth == 100, $"entries={probe.EnemyEntries}, stays={probe.EnemyStays}, hp={health.CurrentHealth}"); // 비활성 적 실제 접촉 결과
            enemyObject.transform.position += Vector3.up * 5f; // 실제 적 접촉 해제
            Physics2D.SyncTransforms(); // 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 분리 스텝
            health.ResetHealth(); // 이전 피해 초기화
            enemy.enabled = true; // 같은 적 행동 재활성화
            SetEnemyAttackReady(enemy); // 실제 근접 공격 준비
            enemyObject.transform.position = segment.transform.position; // 실제 몸통 재접촉
            Physics2D.SyncTransforms(); // 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 진입 스텝
            yield return new WaitForFixedUpdate(); // 실제 지속 접촉 스텝
            yield return null; // 물리 직후 피해 해결 대기
            Record("reenabled enemy behaviour", health.CurrentHealth == 90 && probe.EnemyStays > 0, $"entries={probe.EnemyEntries}, stays={probe.EnemyStays}, hp={health.CurrentHealth}"); // 재활성 실제 근접 피해 확인
            Destroy(probe); // 시험용 몸통 관찰기 제거
        } // 코루틴 끝

        private IEnumerator VerifyEnemyProjectile() // 비활성 적 투사체와 실제 재접촉
        { // 코루틴 시작
            SnakeSegment segment = body.BodySegments[0]; // 실제 머리에서 떨어진 몸통
            DisabledCollisionProbe probe = segment.gameObject.AddComponent<DisabledCollisionProbe>(); // 실제 몸통 접촉 관찰기
            EnemyTestProjectile projectile = SpawnProjectile(segment.transform.position); // 실제 정지 적 투사체 생성
            projectile.enabled = false; // 투사체 컴포넌트만 비활성화
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 진입 스텝
            yield return null; // 물리 직후 피해와 파괴 대기
            bool intact = projectile != null; // 실제 투사체 생존 확인
            Record("disabled enemy projectile", probe.ProjectileEntries > 0 && health.CurrentHealth == 100 && intact, $"entries={probe.ProjectileEntries}, hp={health.CurrentHealth}, intact={intact}"); // 비활성 실제 접촉 결과
            if (projectile == null) // 기존 결함으로 파괴된 경우
            { // 조건 시작
                projectile = SpawnProjectile(segment.transform.position + Vector3.up * 5f); // 활성 대조용 투사체 생성
            } // 조건 끝
            else // 비활성 투사체가 남은 경우
            { // 대안 시작
                projectile.transform.position = segment.transform.position + Vector3.up * 5f; // 같은 투사체 접촉 해제
            } // 조건 끝
            Physics2D.SyncTransforms(); // 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 접촉 해제 스텝
            health.ResetHealth(); // 이전 피해 초기화
            projectile.enabled = true; // 같은 투사체 또는 대조 투사체 활성화
            projectile.transform.position = segment.transform.position; // 실제 몸통 재접촉
            Physics2D.SyncTransforms(); // 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 재진입 스텝
            yield return null; // 물리 직후 피해 해결 대기
            Record("reenabled enemy projectile", health.CurrentHealth == 92, $"entries={probe.ProjectileEntries}, hp={health.CurrentHealth}, sameProjectile={intact}"); // 실제 적 투사체 피해 확인
            Destroy(probe); // 시험용 몸통 관찰기 제거
        } // 코루틴 끝

        private IEnumerator VerifyPriorityShield() // 실제 동시 접촉과 보호막 회귀
        { // 코루틴 시작
            Require(shield.Apply(50, 10f, 2, 10f), "Could not prepare actual shared shield."); // 실제 보호막 구성
            SpawnHazard(SnakeContactHazardType.Obstacle); // 실제 장애물 접촉 생성
            SpawnProjectile(player.transform.position); // 실제 적 투사체 접촉 생성
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 동시 물리 스텝
            yield return null; // 물리 직후 우선순위 해결 대기
            Record("active collision priority and shield", headProbe.HazardEntries > 0 && health.CurrentHealth == 100 && shield.CurrentShield == 25 && invulnerability.IsInvulnerable, $"hazardEntries={headProbe.HazardEntries}, projectileEntries={headProbe.ProjectileEntries}, hp={health.CurrentHealth}, shield={shield.CurrentShield}, invulnerable={invulnerability.IsInvulnerable}"); // 실제 우선순위와 보호막 확인
        } // 코루틴 끝

        private GameObject SpawnHazard(SnakeContactHazardType type) // 실제 머리 접촉 위험 생성
        { // 메서드 시작
            GameObject item = new GameObject("DisabledCollisionHazard"); // 통제 위험 객체
            spawned.Add(item); // 통제 객체 기록
            item.transform.position = player.transform.position; // 실제 머리 위치
            CircleCollider2D collider = item.AddComponent<CircleCollider2D>(); // 실제 위험 충돌체
            collider.radius = 0.1f; // 실제 접촉 반경
            collider.isTrigger = true; // 실제 트리거 설정
            item.AddComponent<SnakeContactHazard>().Configure(type); // 실제 위험 종류 설정
            return item; // 생성 객체 반환
        } // 메서드 끝

        private EnemyTestProjectile SpawnProjectile(Vector3 position) // 실제 적 투사체 생성
        { // 메서드 시작
            GameObject item = new GameObject("DisabledCollisionProjectile"); // 실제 통제 투사체 객체
            spawned.Add(item); // 통제 객체 기록
            item.transform.position = position; // 실제 시험 위치
            EnemyTestProjectile projectile = item.AddComponent<EnemyTestProjectile>(); // 실제 적 투사체 컴포넌트
            projectile.Configure(Vector2.right, 0f, 10f, 8, health, body, player, null); // 정지 투사체와 실제 대상 설정
            return projectile; // 실제 투사체 반환
        } // 메서드 끝

        private static void SetEnemyAttackReady(EarlyEnemyBehaviour enemy) // 실제 적 공격 대기 시간 통제
        { // 메서드 시작
            FieldInfo field = typeof(EarlyEnemyBehaviour).GetField("nextAttack", PrivateFields); // 실제 공격 시각 조회
            Require(field != null, "Enemy attack-time fixture is unavailable."); // 필드 존재 확인
            field.SetValue(enemy, 0f); // 즉시 근접 공격 가능 상태
        } // 메서드 끝

        private void Record(string name, bool success, string detail) // 각 독립 사례 결과 기록
        { // 메서드 시작
            if (!success) // 사례 실패 확인
            { // 조건 시작
                failures++; // 실제 실패 횟수 증가
            } // 조건 끝
            Debug.Log($"[DisabledCollisionVerification] {(success ? "PASS" : "FAIL")} {name}: {detail}."); // 개별 실제 결과 기록
        } // 메서드 끝

        private static void Require(bool condition, string message) // 필수 시험 구성 확인
        { // 메서드 시작
            if (!condition) // 필수 조건 실패 확인
            { // 조건 시작
                AutomatedDisabledCollisionVerification.Finish(1, message); // 즉시 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
