using System; // 예외 형식
using System.Collections; // 코루틴 형식
using System.Collections.Generic; // 실제 유예 몸통 조회
using System.Reflection; // 실제 보스 구성 조회
using ProjectEpsilon.Combat; // 실제 보스와 무기
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Enemies; // 실제 적과 보스
using ProjectEpsilon.Player; // 실제 몸통과 충돌
using UnityEditor; // 배치 실행
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Editor // 배치 시험 영역
{
    [InitializeOnLoad] // 도메인 재시작 연결
    public static class AutomatedBossPushVerification // 실제 보스 몸통 밀림 검증
    {
        private const string ActiveKey = "EpsilonBossPushVerification"; // 시험 진행 표식
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지
        private static double started; // 실제 시험 시작 시각
        static AutomatedBossPushVerification() // 도메인 재시작 처리
        {
            if (SessionState.GetBool(ActiveKey, false)) // 시험 진행 확인
            {
                EditorApplication.update += Tick; // 실제 시작 감시
            }
        }
        public static void Run() // 실제 씬 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 시험 진행 표식
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 모드 시작
        }
        private static void Tick() // 실제 시작과 결과 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 진입 확인
            {
                return; // 실제 진입 대기
            }
            if (started <= 0d) // 실제 시험 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 기록
            }
            if (EditorApplication.timeSinceStartup - started > 90d) // 실제 시험 시간 제한
            {
                Finish(1, "Timed out waiting for boss push test."); // 시간 초과 실패
                return; // 감시 종료
            }
            if (StartingWeaponBatchHelper.HandleStartingSelection() || injected) // 실제 시작과 실행 확인
            {
                return; // 실제 시작 또는 결과 대기
            }
            SnakeMovement player = UnityEngine.Object.FindFirstObjectByType<SnakeMovement>(); // 실제 플레이어 조회
            if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying || Time.time < 0.35f) // 실제 씬 초기화 확인
            {
                return; // 실제 초기화 대기
            }
            injected = true; // 실행기 중복 방지
            player.gameObject.AddComponent<BossPushVerificationRunner>(); // 실제 몸통 밀림 시험 실행
        }
        public static void Finish(int code, string message) // 실제 결과 기록과 종료
        {
            if (finished) // 중복 종료 확인
            {
                return; // 중복 처리 방지
            }
            finished = true; // 종료 상태 저장
            SessionState.SetBool(ActiveKey, false); // 시험 표식 해제
            EditorApplication.update -= Tick; // 실제 시작 감시 해제
            Debug.Log($"[BossPushVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(code); // 실제 프로세스 종료
        }
    }

    public sealed class BossPushContactProbe : MonoBehaviour // 실제 같은 스텝 접촉 관찰기
    {
        public SnakeMovement Player; // 실제 머리 참조
        public WeaponTarget Boss; // 실제 보스 참조
        public float HeadFixedTime = -1f; // 실제 머리 접촉 물리 시각
        public float BossFixedTime = -2f; // 실제 보스 접촉 물리 시각
        public int HeadStays; // 실제 머리 지속 접촉 수
        public int BossStays; // 실제 보스 지속 접촉 수
        public int HeadExits; // 실제 머리 접촉 종료 수
        private void OnTriggerEnter2D(Collider2D other) // 실제 몸통 진입 접촉
        {
            if (Player != null && other.transform == Player.transform) // 실제 머리 접촉 확인
            {
                HeadFixedTime = Time.fixedTime; // 실제 머리 접촉 시각 기록
            }
            if (Boss != null && other.GetComponent<WeaponTarget>() == Boss) // 실제 보스 접촉 확인
            {
                BossFixedTime = Time.fixedTime; // 실제 보스 접촉 시각 기록
            }
        }
        private void OnTriggerStay2D(Collider2D other) // 실제 몸통 지속 접촉 관찰
        {
            if (Player != null && other.transform == Player.transform) // 실제 머리 지속 접촉 확인
            {
                HeadStays++; // 실제 머리 지속 접촉 증가
            }
            if (Boss != null && other.GetComponent<WeaponTarget>() == Boss) // 실제 보스 지속 접촉 확인
            {
                BossStays++; // 실제 보스 지속 접촉 증가
            }
        }
        private void OnTriggerExit2D(Collider2D other) // 실제 머리 접촉 종료 관찰
        {
            if (Player != null && other.transform == Player.transform) // 실제 머리 접촉 종료 확인
            {
                HeadExits++; // 실제 머리 접촉 종료 기록
            }
        }
    }

    [DefaultExecutionOrder(250)] // 실제 경로 추적 이후 실행
    public sealed class BossPushPinProbe : MonoBehaviour // 실제 추적 중 시험 위치 고정
    {
        public SnakeMovement Player; // 실제 머리 참조
        public SnakeBodyManager Body; // 실제 몸통 관리자
        public SnakeSegment First; // 시험 중 첫 몸통
        public SnakeSegment Second; // 시험 중 둘째 몸통
        private void LateUpdate() // 실제 추적 후 시험 위치 구성
        {
            if (Player == null || Body == null) // 실제 시험 구성 확인
            {
                return; // 위치 구성 생략
            }
            for (int index = 0; index < Body.BodySegments.Count; index++) // 실제 몸통 순회
            {
                SnakeSegment segment = Body.BodySegments[index]; // 실제 현재 몸통
                if (segment == null) // 파괴된 몸통 확인
                {
                    continue; // 무효 몸통 제외
                }
                segment.transform.position = segment == First || segment == Second ? Player.transform.position : Player.transform.position + Vector3.right * (20f + index * 4f); // 실제 접촉과 비접촉 시험 위치
            }
        }
    }

    [DefaultExecutionOrder(1000)] // 자기 충돌 예약 이후 실행
    public sealed class BossPushSeparationProbe : MonoBehaviour // 실제 같은 스텝 분리 관찰기
    {
        public SnakeSegment Segment; // 실제 유예 몸통
        public SnakeSelfCollision Self; // 실제 자기 충돌 처리기
        public SnakeExternalCollision External; // 실제 물리 충돌 처리기
        public BossPushPinProbe Pin; // 실제 시험 위치 고정기
        public SnakeBodyFollower Follower; // 실제 몸통 경로 추적기
        public bool Armed; // 실제 분리 시험 준비
        public bool Moved; // 실제 분리 수행 여부
        private void FixedUpdate() // 예약 직후 같은 물리 스텝 분리
        {
            if (!Armed || Moved || Segment == null || Self == null || External == null || Segment.IsBossPushGraceActive) // 실제 분리 가능 상태 확인
            {
                return; // 실제 예약 대기
            }
            FieldInfo pending = typeof(SnakeExternalCollision).GetField("pendingSelfCollision", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 자기 충돌 예약 조회
            if (pending == null || !ReferenceEquals(pending.GetValue(External), Self)) // 실제 유예 종료 예약 확인
            {
                return; // 실제 예약 대기
            }
            Segment.transform.position += Vector3.right * 20f; // 물리 처리 전 실제 몸통 분리
            if (Pin != null) // 실제 위치 고정기 확인
            {
                Pin.enabled = false; // 이후 프레임의 재겹침 방지
            }
            if (Follower != null) // 실제 몸통 경로 추적기 확인
            {
                Follower.enabled = false; // 분리 검증 중 재배치 방지
            }
            Physics2D.SyncTransforms(); // 실제 분리 위치 반영
            Moved = true; // 실제 같은 스텝 분리 기록
        }
    }

    public sealed class BossPushVerificationRunner : MonoBehaviour // 실제 밀림과 자기 충돌 시나리오
    {
        private SnakeMovement player; // 실제 플레이어 머리
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeBodyFollower follower; // 실제 경로 추적기
        private SnakePathRecorder path; // 실제 머리 경로
        private SnakeSelfCollision self; // 실제 자기 충돌 처리기
        private WeaponTarget boss; // 실제 보스 대상
        private PropertyInfo grace; // 실제 몸통별 유예 상태
        private BossPushPinProbe pin; // 실제 추적 중 시험 위치 고정기

        private IEnumerator Start() // 실제 밀림과 유예 시험
        {
            player = GetComponent<SnakeMovement>(); // 실제 머리 이동 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            path = GetComponent<SnakePathRecorder>(); // 실제 머리 경로 조회
            self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌 처리기 조회
            EarlyEnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 일반 적 출현기
            PrototypeBossEncounter encounter = UnityEngine.Object.FindFirstObjectByType<PrototypeBossEncounter>(); // 실제 보스 관리자
            Require(player != null && body != null && follower != null && path != null && self != null && spawner != null && encounter != null, "Actual boss push scene fixtures missing."); // 실제 필수 구성 확인
            player.enabled = false; // 실제 플레이어 이동 통제
            self.enabled = false; // 초기 밀림 시험의 자기 충돌 방지
            spawner.enabled = false; // 실제 일반 적 출현 통제
            foreach (EarlyEnemyBehaviour existing in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 기존 적 순회
            {
                existing.gameObject.SetActive(false); // 실제 기존 적 차단
                Destroy(existing.gameObject); // 실제 기존 적 제거
            }
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 무기 슬롯 순회
            {
                slot.Clear(); // 시험 중 자동 공격 방지
            }
            MethodInfo spawn = typeof(PrototypeBossEncounter).GetMethod("TrySpawnBoss", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 보스 생성 경로
            Require(spawn != null, "Actual boss creation path missing."); // 실제 생성 경로 확인
            spawn.Invoke(encounter, null); // 시험용 실제 보스 생성
            boss = encounter.Boss; // 실제 생성 보스
            Type pushType = typeof(PrototypeBossEncounter).Assembly.GetType("ProjectEpsilon.Enemies.PrototypeBossBodyPush"); // 실제 몸통 밀림 형식 조회
            grace = typeof(SnakeSegment).GetProperty("IsBossPushGraceActive"); // 실제 몸통별 유예 조회
            Require(boss != null && pushType != null && boss.GetComponent(pushType) != null && grace != null, "Actual boss push or body-specific grace missing."); // 실제 밀림 구성 확인
            boss.GetComponent<EarlyEnemyBehaviour>().enabled = false; // 실제 사격과 추적 통제
            boss.GetComponent<PrototypeBossChargeAttack>().enabled = false; // 실제 돌진 통제
            pin = gameObject.AddComponent<BossPushPinProbe>(); // 실제 시험 위치 고정기
            pin.Player = player; // 실제 머리 참조
            pin.Body = body; // 실제 몸통 참조
            pin.enabled = false; // 초기 경로 밀림 시험 방해 방지
            yield return VerifyDisabledFollower(); // 실제 비활성 추적기 경계
            yield return VerifyPathPush(); // 실제 경로 밀림과 복귀
            yield return VerifySameStepGraceAndUnrelatedCollision(); // 실제 동시 접촉과 비보호 몸통
            yield return VerifyPersistentOverlapAfterGrace(); // 실제 유예 종료 후 접촉
            yield return VerifySeparationBeforeResolution(); // 실제 같은 스텝 분리 확인
            AutomatedBossPushVerification.Finish(0, "Actual push, body-specific grace and persistent-overlap resolution passed."); // 실제 전체 결과
        }

        private IEnumerator VerifyDisabledFollower() // 비활성 추적기의 밀림과 유예 차단
        {
            follower.enabled = false; // 실제 몸통 추적기 비활성화
            SnakeSegment segment = body.BodySegments[0]; // 실제 첫 몸통
            Vector3 initial = segment.transform.position; // 실제 초기 몸통 위치
            boss.transform.position = initial - Vector3.right * 0.1f; // 실제 몸통 옆 보스 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 보스와 몸통 접촉
            yield return null; // 실제 접촉 처리 대기
            Require(!(bool)grace.GetValue(segment) && (segment.transform.position - initial).sqrMagnitude < 0.0001f, "Disabled body follower received a boss push or grace."); // 실제 비활성 밀림과 유예 차단
            boss.transform.position = player.transform.position + Vector3.right * 50f; // 실제 보스 접촉 해제
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 접촉 해제 스텝
            yield return null; // 실제 접촉 종료 대기
            Debug.Log("[BossPushVerification] disabled body follower received neither push nor grace."); // 실제 비활성 경계 결과 기록
        }

        private IEnumerator VerifyPathPush() // 실제 몸통 경로 밀림과 복귀
        {
            follower.enabled = true; // 실제 몸통 경로 추적 허용
            follower.SnapToPath(); // 실제 몸통 경로 초기화
            SnakeSegment segment = body.BodySegments[0]; // 실제 첫 몸통
            Vector3 pathPosition = path.GetPositionAtDistance(follower.SegmentSpacing); // 실제 첫 몸통 경로 위치
            boss.transform.position = pathPosition - Vector3.right * 0.1f; // 실제 몸통 옆 보스 위치
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 보스와 몸통 접촉
            yield return null; // 실제 경로와 밀림 반영 대기
            Require((bool)grace.GetValue(segment) && (segment.transform.position - pathPosition).magnitude > 0.1f, "Actual boss contact did not displace the followed body and start grace."); // 실제 몸통 밀림과 유예 확인
            boss.transform.position = player.transform.position + Vector3.right * 50f; // 실제 보스 접촉 해제
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForSeconds(0.8f); // 임시 유예와 경로 복귀 대기
            yield return null; // 실제 경로 추적 완료 대기
            Require(!(bool)grace.GetValue(segment) && (segment.transform.position - pathPosition).magnitude < 0.05f, "Displaced body did not return to its recorded path."); // 실제 경로 복귀 확인
            Debug.Log("[BossPushVerification] actual boss-body contact displaced a followed body and the body returned to its path."); // 실제 밀림 결과 기록
        }

        private IEnumerator VerifySameStepGraceAndUnrelatedCollision() // 실제 같은 스텝과 다른 몸통 접촉
        {
            follower.enabled = true; // 실제 몸통 추적기 유지
            self.enabled = true; // 실제 자기 충돌 처리 허용
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            health.ResetHealth(); // 실제 시험 전 체력 복구
            int initialHealth = health.CurrentHealth; // 실제 유예 전 체력
            SnakeSegment protectedBody = body.BodySegments[0]; // 실제 보호 대상 몸통
            pin.First = protectedBody; // 실제 유예 몸통 위치 고정
            pin.Second = null; // 추가 접촉 몸통 초기화
            pin.enabled = true; // 실제 추적 후 시험 위치 유지
            MoveOtherBodiesAway(protectedBody); // 다른 몸통 접촉 방지
            protectedBody.transform.position = player.transform.position; // 실제 머리와 보호 몸통 겹침
            BossPushContactProbe probe = protectedBody.gameObject.AddComponent<BossPushContactProbe>(); // 실제 접촉 시각 관찰기
            probe.Player = player; // 실제 머리 참조
            probe.Boss = boss; // 실제 보스 참조
            boss.transform.position = player.transform.position - Vector3.right * 0.1f; // 실제 같은 스텝 보스 겹침
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 같은 스텝 접촉
            yield return null; // 실제 물리 우선순위 처리 대기
            Require(probe.HeadFixedTime >= 0f && Mathf.Approximately(probe.HeadFixedTime, probe.BossFixedTime), "Actual boss and head contacts were not in the same physics step."); // 실제 동시 접촉 확인
            Require((bool)grace.GetValue(protectedBody) && body.CurrentBodyCount == 3 && health.CurrentHealth == initialHealth, "Boss-pushed body caused immediate same-step self collision."); // 실제 몸통별 동시 유예 확인
            GameManager.Instance.PauseGame(); // 실제 게임 일시정지
            yield return new WaitForSecondsRealtime(0.3f); // 실제 정지 시간 대기
            Require((bool)grace.GetValue(protectedBody) && body.CurrentBodyCount == 3 && health.CurrentHealth == initialHealth, "Boss push grace expired or damaged the player during pause."); // 실제 정지 중 유예 유지 확인
            GameManager.Instance.ResumeGame(); // 실제 게임 재개
            boss.transform.position = player.transform.position + Vector3.right * 50f; // 실제 보스 접촉 해제
            SnakeSegment otherBody = body.BodySegments[1]; // 실제 비보호 몸통
            pin.Second = otherBody; // 실제 비보호 몸통 위치 유지
            otherBody.transform.position = player.transform.position; // 실제 비보호 머리 접촉
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 비보호 몸통 접촉
            yield return null; // 실제 자기 충돌 처리 대기
            Require(body.CurrentBodyCount == 3 && health.CurrentHealth == initialHealth - 25, "An unrelated unprotected body did not cause self-collision damage."); // 다른 몸통의 체력 피해 확인
            pin.enabled = false; // 다음 시험의 이전 위치 고정 해제
            Debug.Log($"[BossPushVerification] actual same physics step={probe.HeadFixedTime:F3}; protected body caused no damage; another body caused self-collision health damage."); // 실제 동시 접촉 결과 기록
        }

        private IEnumerator VerifyPersistentOverlapAfterGrace() // 실제 유예 후 지속 겹침 처리
        {
            self.enabled = false; // 실제 몸통 초기화 중 충돌 중지
            pin.enabled = false; // 이전 시험 위치 고정 해제
            body.ResetBody(); // 실제 시작 몸통 복구
            follower.enabled = true; // 실제 몸통 추적기 유지
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            health.ResetHealth(); // 실제 지속 접촉 시험 전 체력 복구
            int initialHealth = health.CurrentHealth; // 실제 유예 전 체력
            yield return null; // 이전 몸통 파괴 완료 대기
            SnakeSegment protectedBody = body.BodySegments[0]; // 실제 보호 대상 몸통
            pin.First = protectedBody; // 실제 유예 몸통 위치 고정
            pin.Second = null; // 추가 접촉 몸통 초기화
            pin.enabled = true; // 실제 추적 후 시험 위치 유지
            MoveOtherBodiesAway(protectedBody); // 다른 몸통 접촉 방지
            protectedBody.transform.position = player.transform.position; // 실제 머리와 몸통 겹침
            BossPushContactProbe probe = protectedBody.gameObject.AddComponent<BossPushContactProbe>(); // 실제 지속 접촉 관찰기
            probe.Player = player; // 실제 머리 참조
            probe.Boss = boss; // 실제 보스 참조
            boss.transform.position = player.transform.position - Vector3.right * 0.1f; // 실제 보스와 몸통 접촉
            self.enabled = true; // 실제 자기 충돌 처리 재개
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 보스와 자기 접촉
            yield return null; // 실제 같은 스텝 처리 대기
            Require((bool)grace.GetValue(protectedBody) && body.CurrentBodyCount == 3, "Actual persistent overlap was not initially protected."); // 실제 유예 시작 확인
            boss.transform.position = player.transform.position + Vector3.right * 50f; // 실제 보스 접촉 해제
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForSeconds(0.8f); // 실제 유예 종료 대기
            float deadline = Time.realtimeSinceStartup + 3f; // 실제 물리 재판정 제한 시각
            while (health.CurrentHealth == initialHealth && Time.realtimeSinceStartup < deadline) // 유예 종료 후 실제 체력 피해 대기
            {
                yield return new WaitForFixedUpdate(); // 실제 지속 접촉 물리 스텝 진행
                yield return null; // 실제 물리 우선순위 처리 대기
            }
            FieldInfo deferredField = typeof(SnakeSelfCollision).GetField("deferredBossPushContacts", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 지연 접촉 목록 조회
            HashSet<SnakeSegment> deferred = deferredField == null ? null : deferredField.GetValue(self) as HashSet<SnakeSegment>; // 실제 지연 접촉 목록
            Debug.Log($"[BossPushVerification] persistent diagnostic: bodies={body.CurrentBodyCount}, activeGrace={grace.GetValue(protectedBody)}, deferred={deferred != null && deferred.Contains(protectedBody)}, headStays={probe.HeadStays}, headExits={probe.HeadExits}, bossStays={probe.BossStays}, sleeping={GetComponent<Rigidbody2D>().IsSleeping()}, state={GameManager.Instance.CurrentState}, scale={Time.timeScale}, time={Time.time:F3}, fixed={Time.fixedTime:F3}, realtime={Time.realtimeSinceStartup:F3}, immune={GetComponent<SnakeInvulnerability>().IsInvulnerable}, head={player.transform.position}, body={protectedBody.transform.position}."); // 실제 지속 접촉과 유예 상태 기록
            Require(body.CurrentBodyCount == 3 && health.CurrentHealth == initialHealth - 25, "Persistent self overlap did not cause health damage after grace expired."); // 실제 유예 후 체력 피해 확인
            pin.enabled = false; // 다음 시험의 이전 위치 고정 해제
            Debug.Log("[BossPushVerification] actual persistent body overlap caused health damage after grace expired."); // 실제 유예 종료 결과 기록
        }

        private void MoveOtherBodiesAway(SnakeSegment kept) // 실제 비시험 몸통 접촉 차단
        {
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            {
                SnakeSegment segment = body.BodySegments[index]; // 실제 현재 몸통
                if (segment != kept) // 시험 몸통 외 확인
                {
                    segment.transform.position = player.transform.position + Vector3.right * (20f + index * 4f); // 실제 접촉 밖 위치
                }
            }
        }
        private IEnumerator VerifySeparationBeforeResolution() // 실제 유예 종료 스텝의 분리
        {
            self.enabled = false; // 실제 몸통 초기화 중 충돌 중지
            pin.enabled = false; // 이전 시험 위치 고정 해제
            body.ResetBody(); // 실제 시작 몸통 복구
            follower.enabled = true; // 실제 몸통 추적기 유지
            GetComponent<SnakeInvulnerability>().ClearInvulnerability(); // 실제 전체 무적 해제
            SnakeHealth health = GetComponent<SnakeHealth>(); // 실제 공용 체력
            health.ResetHealth(); // 실제 분리 시험 전 체력 복구
            int initialHealth = health.CurrentHealth; // 실제 분리 전 체력
            yield return null; // 이전 몸통 파괴 완료 대기
            SnakeSegment protectedBody = body.BodySegments[0]; // 실제 유예 대상 몸통
            pin.First = protectedBody; // 실제 유예 몸통 위치 고정
            pin.Second = null; // 추가 접촉 몸통 초기화
            pin.enabled = true; // 실제 추적 후 시험 위치 유지
            MoveOtherBodiesAway(protectedBody); // 다른 몸통 접촉 방지
            protectedBody.transform.position = player.transform.position; // 실제 머리와 몸통 겹침
            boss.transform.position = player.transform.position - Vector3.right * 0.1f; // 실제 보스와 몸통 접촉
            self.enabled = true; // 실제 자기 충돌 처리 재개
            Physics2D.SyncTransforms(); // 실제 물리 위치 반영
            yield return new WaitForFixedUpdate(); // 실제 보스와 자기 접촉
            yield return null; // 실제 같은 스텝 처리 대기
            Require((bool)grace.GetValue(protectedBody) && body.CurrentBodyCount == 3, "Actual separation fixture was not initially protected."); // 실제 유예 시작 확인
            boss.transform.position = player.transform.position + Vector3.right * 50f; // 실제 보스 접촉 해제
            Physics2D.SyncTransforms(); // 실제 보스 분리 위치 반영
            BossPushSeparationProbe probe = gameObject.AddComponent<BossPushSeparationProbe>(); // 실제 같은 스텝 분리 관찰기
            probe.Segment = protectedBody; // 실제 유예 몸통 연결
            probe.Self = self; // 실제 자기 충돌 연결
            probe.External = GetComponent<SnakeExternalCollision>(); // 실제 물리 처리기 연결
            probe.Pin = pin; // 실제 위치 고정기 연결
            probe.Follower = follower; // 실제 추적기 연결
            probe.Armed = true; // 실제 분리 시험 시작
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 분리 제한 시각
            while (!probe.Moved && Time.realtimeSinceStartup < deadline) // 실제 예약 후 분리 대기
            {
                yield return null; // 실제 물리 프레임 진행
            }
            Require(probe.Moved, "Actual deferred collision was not queued before the separation probe."); // 실제 같은 스텝 분리 확인
            yield return new WaitForFixedUpdate(); // 실제 물리 분리 처리
            yield return null; // 실제 물리 후 충돌 처리 대기
            Require(body.CurrentBodyCount == 3 && health.CurrentHealth == initialHealth, "Separated body caused a stale deferred self collision."); // 분리된 몸통의 잘못된 피해 방지
            Debug.Log("[BossPushVerification] actual deferred overlap was separated after queue and before physics; no stale self-collision damage."); // 실제 같은 스텝 분리 결과 기록
            Destroy(probe); // 실제 분리 관찰기 제거
        }
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        {
            if (!condition) // 실제 실패 조건 확인
            {
                AutomatedBossPushVerification.Finish(1, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            }
        }
    }
}
