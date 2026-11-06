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
    public static class AutomatedShooterSpawnVerification // 사수 조준과 화면 밖 출현 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonShooterSpawnVerification"; // 배치 진행 표식
        private static double started; // 플레이 시작 시각
        private static bool injected; // 실행기 생성 여부
        private static bool finished; // 중복 종료 방지

        static AutomatedShooterSpawnVerification() // 도메인 재시작 처리
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
                Finish(1, "Timed out waiting for controlled shooter and spawn test."); // 제한 시간 실패
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
            player.gameObject.AddComponent<ShooterSpawnVerificationRunner>(); // 실제 씬 시험 실행
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
            Debug.Log($"[ShooterSpawnVerification] {(code == 0 ? "PASS" : "FAIL")}: {message}"); // 실제 배치 결과 기록
            EditorApplication.Exit(code); // 종료 코드 반환
        } // 메서드 끝
    } // 클래스 끝

    [DefaultExecutionOrder(32000)] // 실제 카메라와 출현 처리 뒤 관찰
    public sealed class ShooterSpawnRenderProbe : MonoBehaviour // 렌더 직전 실제 출현 위치 관찰
    { // 클래스 시작
        public Camera ObservedCamera; // 실제 이동 카메라
        public EarlyEnemyBehaviour SpawnedEnemy; // 실제 해당 프레임 출현 적
        public Vector3 BeforeCamera; // 프레임 전 카메라 위치
        public Vector3 BeforeViewport; // 출현기 Update 직후 좌표
        public Vector3 AfterViewport; // 모든 LateUpdate 뒤 좌표
        public bool SawBefore; // 카메라 이동 전 출현 관찰 여부
        public float CameraMovement; // 같은 프레임 카메라 이동량
        public bool Observed; // 실제 렌더 직전 위치 기록 여부

        private void Update() // 실제 LateUpdate 전 위치 기록
        { // 메서드 시작
            if (Observed || ObservedCamera == null) // 관찰 상태 확인
            { // 조건 시작
                return; // 중복 기록 생략
            } // 조건 끝
            BeforeCamera = ObservedCamera.transform.position; // 실제 카메라 이동 전 위치
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 출현 적 순회
            { // 반복 시작
                if (enemy.gameObject.name.StartsWith("EarlyEnemy_")) // 실제 출현기 생성 적 확인
                { // 조건 시작
                    SawBefore = true; // 이동 전 출현 관찰 저장
                    BeforeViewport = ObservedCamera.WorldToViewportPoint(enemy.transform.position); // 이동 전 뷰포트 좌표
                    break; // 첫 대상 관찰 종료
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝

        private void LateUpdate() // 실제 카메라 갱신 뒤 출현 관찰
        { // 메서드 시작
            if (Observed || ObservedCamera == null) // 관찰 상태 확인
            { // 조건 시작
                return; // 중복 기록 생략
            } // 조건 끝
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 출현 적 순회
            { // 반복 시작
                if (!enemy.gameObject.name.StartsWith("EarlyEnemy_")) // 실제 출현기 생성 적 확인
                { // 조건 시작
                    continue; // 다른 적 제외
                } // 조건 끝
                SpawnedEnemy = enemy; // 실제 생성 적 저장
                CameraMovement = Vector3.Distance(BeforeCamera, ObservedCamera.transform.position); // 같은 프레임 실제 카메라 이동
                AfterViewport = ObservedCamera.WorldToViewportPoint(enemy.transform.position); // 렌더 직전 뷰포트 좌표
                enemy.enabled = false; // 실제 관찰 대상의 후속 이동 정지
                Observed = true; // 실제 관찰 완료
                break; // 첫 대상 기록 종료
            } // 반복 끝
        } // 메서드 끝
    } // 클래스 끝

    [DefaultExecutionOrder(32000)] // 출현 직후 같은 프레임 관찰
    public sealed class ShooterSpawnBatchProbe : MonoBehaviour // 실제 일반 적 생성 위치 관찰
    { // 클래스 시작
        public Camera ObservedCamera; // 실제 탑뷰 카메라
        public Transform Player; // 실제 머리 위치
        public readonly HashSet<int> Seen = new HashSet<int>(); // 실제 생성 적 고유 번호
        public readonly List<WeaponTarget> Targets = new List<WeaponTarget>(); // 실제 생성 적 목록
        public int Outside; // 여유 밖 실제 생성 수
        public int Minimum; // 최소 거리 충족 수
        public int BeyondOldCleanup; // 기존 정리 거리 밖 수

        private void LateUpdate() // 실제 생성 직후 렌더 전 관찰
        { // 메서드 시작
            if (ObservedCamera == null || Player == null || Seen.Count >= 16) // 필수 참조와 표본 수 확인
            { // 조건 시작
                return; // 관찰 대기 또는 종료
            } // 조건 끝
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 생성 적 순회
            { // 반복 시작
                if (!enemy.gameObject.name.StartsWith("EarlyEnemy_") || !Seen.Add(enemy.GetInstanceID())) // 출현기 적과 중복 확인
                { // 조건 시작
                    continue; // 다른 적 또는 중복 제외
                } // 조건 끝
                enemy.enabled = false; // 다음 물리 스텝의 이동 방지
                WeaponTarget target = enemy.GetComponent<WeaponTarget>(); // 실제 적 전투 대상
                if (target != null) // 실제 대상 존재 확인
                { // 조건 시작
                    Targets.Add(target); // 실제 생성 대상 기록
                } // 조건 끝
                float distance = Vector2.Distance(enemy.transform.position, Player.position); // 생성 프레임의 실제 머리 거리
                if (ShooterSpawnVerificationRunner.IsOutsideMargin(ObservedCamera, enemy.transform.position, 0.5f)) // 생성 프레임 화면 여유 확인
                { // 조건 시작
                    Outside++; // 실제 화면 밖 생성 수 증가
                } // 조건 끝
                if (distance >= 8.999f) // 생성 프레임의 실제 최소 거리 확인
                { // 조건 시작
                    Minimum++; // 실제 최소 거리 충족 증가
                } // 조건 끝
                if (distance > 22f) // 기존 정리 거리 초과 확인
                { // 조건 시작
                    BeyondOldCleanup++; // 기존 거리 밖 표본 증가
                } // 조건 끝
            } // 반복 끝
        } // 메서드 끝
    } // 클래스 끝

    [DefaultExecutionOrder(32000)] // 출현 직후 같은 프레임 관찰
    public sealed class ShooterSpawnPositionProbe : MonoBehaviour // 카메라 부재 출현 위치 관찰
    { // 클래스 시작
        public Transform Player; // 실제 플레이어 위치
        public EarlyEnemyBehaviour SpawnedEnemy; // 실제 출현 적
        public float SpawnDistance; // 실제 생성 프레임 거리

        private void LateUpdate() // 실제 출현 직후 렌더 전 관찰
        { // 메서드 시작
            if (SpawnedEnemy != null || Player == null) // 관찰 완료 확인
            { // 조건 시작
                return; // 중복 관찰 방지
            } // 조건 끝
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 실제 출현 적 순회
            { // 반복 시작
                if (!enemy.gameObject.name.StartsWith("EarlyEnemy_")) // 실제 출현기 생성 적 확인
                { // 조건 시작
                    continue; // 다른 적 제외
                } // 조건 끝
                SpawnedEnemy = enemy; // 실제 생성 적 저장
                SpawnDistance = Vector2.Distance(enemy.transform.position, Player.position); // 생성 프레임의 실제 거리
                enemy.enabled = false; // 다음 물리 스텝 이동 방지
                break; // 실제 첫 대상 기록 종료
            } // 반복 끝
        } // 메서드 끝
    } // 클래스 끝

    public sealed class ShooterSpawnVerificationRunner : MonoBehaviour // 실제 사수와 출현 통제 시험
    { // 클래스 시작
        private static readonly BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance; // 실제 내부 설정 조회
        private readonly List<GameObject> spawned = new List<GameObject>(); // 통제 객체 목록
        private SnakeMovement player; // 실제 플레이어 이동
        private SnakeBodyManager body; // 실제 몸통 관리자
        private SnakeHealth health; // 실제 공용 체력
        private EarlyEnemyWaveSpawner spawner; // 실제 적 출현기
        private Camera camera; // 실제 탑뷰 카메라
        private int failures; // 독립 사례 실패 수

        private IEnumerator Start() // 실제 씬 검증 순서
        { // 코루틴 시작
            Prepare(); // 실제 씬과 통제 조건 준비
            yield return null; // 기존 적 제거 대기
            yield return VerifyShooterAim(0); // 실제 같은 거리 몸통 우선순위
            yield return VerifyShooterAim(1); // 실제 몸통 부재 머리 발사
            yield return VerifyShooterAim(2); // 실제 다른 거리 최근접 몸통 발사
            yield return VerifyShooterMovement(8f, -1); // 머리에서 먼 사수의 접근
            yield return VerifyShooterMovement(3.5f, 0); // 머리 기준 사거리 유지
            yield return VerifyShooterMovement(2.5f, 1); // 머리 근접 사수의 후퇴
            yield return VerifyAutomaticSpawns(); // 실제 일반 적의 넓은 화면 밖 출현
            yield return VerifyEliteSpawn(); // 통제 시각의 실제 엘리트 출현
            yield return VerifyNoCamera(); // 실제 카메라 부재 대체 경로
            yield return VerifyMovingCamera(); // 실제 같은 프레임 카메라 이동 후 출현
            AutomatedShooterSpawnVerification.Finish(failures == 0 ? 0 : 1, $"Actual shooter aiming, head-distance movement and controlled spawn cases completed; failures={failures}."); // 전체 시험 결과
        } // 코루틴 끝

        private void Prepare() // 실제 씬 구성과 위치 통제
        { // 메서드 시작
            player = GetComponent<SnakeMovement>(); // 실제 플레이어 조회
            body = UnityEngine.Object.FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자 조회
            health = GetComponent<SnakeHealth>(); // 실제 공용 체력 조회
            spawner = UnityEngine.Object.FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 적 출현기 조회
            camera = Camera.main; // 실제 주 카메라 조회
            SnakeWeaponManager weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 실제 무기 관리자 조회
            Require(player != null && body != null && health != null && spawner != null && camera != null && camera.orthographic && weapons != null && body.CurrentBodyCount == 3, "Actual Game scene fixtures are missing."); // 필수 씬 구성 확인
            spawner.enabled = false; // 직접 통제 전 자연 출현 중지
            player.enabled = false; // 실제 플레이어 이동 정지
            player.transform.position = Vector3.zero; // 동률을 정확하게 비교할 정수 좌표
            Physics2D.SyncTransforms(); // 실제 머리 위치 물리 동기화
            SnakeBodyFollower follower = UnityEngine.Object.FindFirstObjectByType<SnakeBodyFollower>(); // 실제 몸통 추적기 조회
            if (follower != null) // 몸통 추적기 존재 확인
            { // 조건 시작
                follower.enabled = false; // 실제 몸통 위치 고정
            } // 조건 끝
            SnakeSelfCollision self = GetComponent<SnakeSelfCollision>(); // 실제 자기 충돌기 조회
            SnakeExternalCollision external = GetComponent<SnakeExternalCollision>(); // 실제 외부 충돌기 조회
            if (self != null) // 자기 충돌기 존재 확인
            { // 조건 시작
                self.enabled = false; // 통제 중 자기 충돌 방지
            } // 조건 끝
            if (external != null) // 외부 충돌기 존재 확인
            { // 조건 시작
                external.enabled = false; // 통제 중 외부 접촉 방지
            } // 조건 끝
            foreach (SnakeWeaponSlot slot in weapons.Slots) // 실제 몸통 무기 슬롯 순회
            { // 반복 시작
                slot.Clear(); // 통제 중 플레이어 자동 공격 방지
            } // 반복 끝
            foreach (EarlyEnemyBehaviour enemy in UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None)) // 초기 자연 출현 적 순회
            { // 반복 시작
                enemy.gameObject.SetActive(false); // 기존 적 즉시 제외
                Destroy(enemy.gameObject); // 기존 적 제거
            } // 반복 끝
            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>(); // 실제 카메라 추적기 조회
            if (follow != null) // 카메라 추적기 존재 확인
            { // 조건 시작
                follow.enabled = false; // 임시 카메라 위치 직접 통제
            } // 조건 끝
            Debug.Log("[ShooterSpawnVerification] Actual Game scene and fixed-step shooter firing; movement, body following and automatic weapons controlled; camera position/aspect and select spawner times changed only in memory."); // 실제 시험 통제 조건 기록
        } // 메서드 끝

        private IEnumerator VerifyShooterAim(int scenario) // 실제 사수의 발사 방향
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 실제 대상 정리
            Vector3 head = player.transform.position; // 실제 플레이어 머리 위치
            body.BodySegments[0].transform.position = head + new Vector3(7f, 1f); // 가까운 첫 몸통
            body.BodySegments[1].transform.position = head + new Vector3(7f, scenario == 2 ? -0.5f : -1f); // 같은 거리 또는 더 가까운 둘째 몸통
            body.BodySegments[2].transform.position = head + new Vector3(3f, 5f); // 더 먼 몸통
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.gameObject.SetActive(scenario != 1); // 몸통 존재 또는 비활성 대체 통제
            } // 반복 끝
            EarlyEnemyBehaviour enemy = CreateShooter(head + Vector3.right * 8f, 0f, 0f); // 실제 사수와 정지 발사 설정
            float firstSquared = ((Vector2)(body.BodySegments[0].transform.position - enemy.transform.position)).sqrMagnitude; // 첫 몸통 실제 거리 제곱
            float secondSquared = ((Vector2)(body.BodySegments[1].transform.position - enemy.transform.position)).sqrMagnitude; // 둘째 몸통 실제 거리 제곱
            if (scenario == 0) // 실제 동률 시험 확인
            { // 조건 시작
                Require(firstSquared == secondSquared, "Controlled body tie is not exactly equal."); // 정확한 실제 동률 검증
            } // 조건 끝
            if (scenario == 2) // 실제 비동률 시험 확인
            { // 조건 시작
                Require(secondSquared < firstSquared, "Controlled nearest body is not strictly closer."); // 실제 최근접 거리 검증
            } // 조건 끝
            Physics2D.SyncTransforms(); // 실제 몸통 위치 동기화
            yield return new WaitForFixedUpdate(); // 실제 사수 물리 발사 스텝
            yield return null; // 실제 투사체 생성 대기
            EnemyTestProjectile projectile = UnityEngine.Object.FindFirstObjectByType<EnemyTestProjectile>(); // 실제 적 투사체 조회
            Require(projectile != null, "Actual shooter did not create a projectile."); // 실제 발사 확인
            FieldInfo directionField = typeof(EnemyTestProjectile).GetField("direction", Fields); // 실제 투사체 방향 필드
            Require(directionField != null, "Actual projectile direction field is missing."); // 방향 관찰 필드 확인
            Vector2 direction = (Vector2)directionField.GetValue(projectile); // 실제 발사 방향
            Vector2 target = scenario == 1 ? (Vector2)head : (Vector2)body.BodySegments[scenario == 2 ? 1 : 0].transform.position; // 실제 머리 대체 또는 대상 몸통
            Vector2 expected = (target - (Vector2)projectile.transform.position).normalized; // 실제 발사 위치 기준 기대 방향
            float dot = Vector2.Dot(direction.normalized, expected); // 실제 방향 일치도
            string name = scenario == 1 ? "shooter no-body head fallback" : scenario == 2 ? "shooter strictly nearest body" : "shooter nearest-body exact tie"; // 실제 조준 시험 이름
            Record(name, dot > 0.999f, $"dot={dot:F4}, actual={direction}, expected={expected}, bodyCount={body.CurrentBodyCount}, firstSquared={firstSquared:F5}, secondSquared={secondSquared:F5}"); // 실제 사수 발사 결과
            enemy.enabled = false; // 추가 발사 방지
            foreach (SnakeSegment segment in body.BodySegments) // 실제 몸통 순회
            { // 반복 시작
                segment.gameObject.SetActive(true); // 실제 몸통 활성 상태 복구
            } // 반복 끝
        } // 코루틴 끝

        private IEnumerator VerifyShooterMovement(float headDistance, int direction) // 실제 머리 기준 사수 거리 유지
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 실제 대상 정리
            Vector3 head = player.transform.position; // 실제 머리 위치
            for (int index = 0; index < body.BodySegments.Count; index++) // 실제 몸통 순회
            { // 반복 시작
                body.BodySegments[index].transform.position = head + new Vector3(headDistance, 3f + index); // 머리와 다른 몸통 방향
            } // 반복 끝
            EarlyEnemyBehaviour enemy = CreateShooter(head + Vector3.right * headDistance, 0.7f, float.MaxValue); // 실제 이동하는 사수
            float before = enemy.transform.position.x; // 실제 초기 가로 위치
            yield return new WaitForSeconds(0.18f); // 여러 실제 물리 단계 진행
            float after = enemy.transform.position.x; // 실제 이동 후 가로 위치
            float delta = after - before; // 실제 머리 기준 이동량
            bool success = direction < 0 ? delta < -0.02f : direction > 0 ? delta > 0.02f : Mathf.Abs(delta) < 0.001f; // 접근·유지·후퇴 확인
            Record("shooter head-distance movement", success, $"headDistance={headDistance:F1}, deltaX={delta:F3}, expectedDirection={direction}"); // 실제 이동 결과
        } // 코루틴 끝

        private IEnumerator VerifyAutomaticSpawns() // 실제 넓은 화면 밖 일반 적 출현
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 시험 객체 정리
            ConfigureCamera(new Vector2(6f, 3f)); // 실제 이동한 초광폭 카메라 구성
            GameObject observer = new GameObject("ShooterSpawnVerification_BatchProbe"); // 실제 생성 프레임 관찰 객체
            spawned.Add(observer); // 시험 객체 기록
            ShooterSpawnBatchProbe probe = observer.AddComponent<ShooterSpawnBatchProbe>(); // 실제 생성 프레임 관찰기
            probe.ObservedCamera = camera; // 실제 뷰포트 카메라 연결
            probe.Player = player.transform; // 실제 머리 위치 연결
            SetSpawnerField("nextSpawn", 0f); // 다음 실제 Update부터 출현 허용
            UnityEngine.Random.InitState(3281); // 출현 방향 재현 시드
            Time.timeScale = 8f; // 실제 출현 간격 관찰을 위한 시험 가속
            spawner.enabled = true; // 실제 자연 출현 Update 활성화
            float deadline = Time.realtimeSinceStartup + 12f; // 실제 출현 제한 시간
            while (probe.Seen.Count < 16 && Time.realtimeSinceStartup < deadline) // 실제 일반 적 생성 대기
            { // 반복 시작
                yield return null; // 실제 출현 Update 진행
            } // 반복 끝
            Require(probe.Seen.Count == 16, "Actual spawner did not create sixteen controlled normal enemies."); // 실제 생성 표본 수 확인
            yield return null; // 실제 다음 정리 Update 진행
            int alive = 0; // 다음 정리 후 실제 생존 수
            foreach (WeaponTarget target in probe.Targets) // 실제 생성 적 순회
            { // 반복 시작
                if (target != null && target.gameObject.activeInHierarchy) // 실제 잔존 적 확인
                { // 조건 시작
                    alive++; // 실제 잔존 적 증가
                } // 조건 끝
            } // 반복 끝
            Record("wide moved-camera automatic spawn", probe.Outside == 16 && probe.Minimum == 16 && probe.BeyondOldCleanup > 0 && alive == 16, $"outside={probe.Outside}/16, minimum={probe.Minimum}/16, beyondOld22={probe.BeyondOldCleanup}, aliveAfterCleanup={alive}/16, aspect={camera.aspect:F3}, size={camera.orthographicSize:F1}"); // 실제 뷰포트와 정리 결과
            spawner.enabled = false; // 다음 통제 전 실제 출현 정지
            Time.timeScale = 1f; // 시험 시간 배율 복구
            foreach (WeaponTarget target in probe.Targets) // 실제 생성 적 정리
            { // 반복 시작
                if (target != null) // 실제 적 존재 확인
                { // 조건 시작
                    target.gameObject.SetActive(false); // 다음 시험 접촉 제외
                    Destroy(target.gameObject); // 실제 적 제거
                } // 조건 끝
            } // 반복 끝
            yield return null; // 지연 파괴 완료 대기
        } // 코루틴 끝

        private IEnumerator VerifyEliteSpawn() // 실제 4분 엘리트 화면 밖 출현
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 표적 정리
            ConfigureCamera(new Vector2(-7f, 4f)); // 실제 다른 위치의 초광폭 카메라
            SetSpawnerField("elapsed", 240f); // 시험용 엘리트 경계 시간 주입
            SetSpawnerField("nextSpawn", float.MaxValue); // 일반 출현만 잠시 보류
            SetSpawnerField("nextEliteRound", 0); // 시험용 첫 엘리트 회차
            UnityEngine.Random.InitState(3299); // 엘리트 출현 방향 재현 시드
            spawner.enabled = true; // 실제 출현 Update 활성화
            yield return null; // 실제 엘리트 출현 Update
            EarlyEnemyBehaviour[] found = UnityEngine.Object.FindObjectsByType<EarlyEnemyBehaviour>(FindObjectsSortMode.None); // 실제 적 행동 조회
            EarlyEnemyBehaviour elite = null; // 실제 엘리트 초기화
            int elites = 0; // 실제 엘리트 수
            foreach (EarlyEnemyBehaviour enemy in found) // 실제 적 순회
            { // 반복 시작
                if (enemy.gameObject.name == "EarlyEnemy_Elite") // 실제 엘리트 종류 확인
                { // 조건 시작
                    elite = enemy; // 실제 엘리트 저장
                    elites++; // 실제 엘리트 수 증가
                } // 조건 끝
            } // 반복 끝
            Require(elite != null && elites == 1, "Actual forced-boundary elite did not spawn exactly once."); // 실제 엘리트 생성 확인
            elite.enabled = false; // 시험 중 실제 엘리트 이동 정지
            bool outside = IsOutsideMargin(camera, elite.transform.position, 0.5f); // 임시 여유 포함 화면 밖 여부
            float distance = Vector2.Distance(elite.transform.position, player.transform.position); // 실제 머리와 생성 거리
            yield return null; // 실제 다음 정리 Update
            Record("elite at forced four-minute boundary", outside && distance >= 8.999f && elite != null, $"outside={outside}, distance={distance:F3}, aliveAfterCleanup={elite != null}, aspect={camera.aspect:F3}"); // 실제 엘리트 화면 밖 및 유지 확인
            spawner.enabled = false; // 다음 시험 전 출현 중지
            if (elite != null) // 실제 엘리트 존재 확인
            { // 조건 시작
                elite.gameObject.SetActive(false); // 실제 접촉 방지
                Destroy(elite.gameObject); // 실제 시험 엘리트 제거
            } // 조건 끝
            yield return null; // 지연 파괴 완료 대기
        } // 코루틴 끝

        private IEnumerator VerifyNoCamera() // 실제 카메라 부재 시 출현 대체
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 시험 객체 정리
            camera.gameObject.SetActive(false); // 실제 주 카메라 비활성화
            Require(Camera.main == null, "Actual main camera was not absent for fallback test."); // 실제 카메라 부재 확인
            GameObject observer = new GameObject("ShooterSpawnVerification_PositionProbe"); // 실제 생성 프레임 관찰 객체
            spawned.Add(observer); // 시험 객체 기록
            ShooterSpawnPositionProbe probe = observer.AddComponent<ShooterSpawnPositionProbe>(); // 실제 LateUpdate 위치 관찰기
            probe.Player = player.transform; // 실제 머리 위치 연결
            SetSpawnerField("nextSpawn", 0f); // 다음 실제 Update 출현 허용
            UnityEngine.Random.InitState(3301); // 실제 출현 방향 재현 시드
            spawner.enabled = true; // 실제 출현 Update 재개
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 생성 제한 시각
            while (probe.SpawnedEnemy == null && Time.realtimeSinceStartup < deadline) // 실제 생성 프레임 관찰 대기
            { // 반복 시작
                yield return null; // 실제 게임 프레임 진행
            } // 반복 끝
            EarlyEnemyBehaviour enemy = probe.SpawnedEnemy; // 실제 생성 프레임의 출현 적
            Require(enemy != null, "Actual no-camera fallback did not produce a normal enemy."); // 실제 일반 적 생성 확인
            float distance = probe.SpawnDistance; // 이동 전 실제 출현 거리
            yield return null; // 실제 다음 정리 Update
            Record("no-camera spawn fallback", Mathf.Abs(distance - 9f) < 0.01f && enemy != null, $"distance={distance:F3}, aliveAfterCleanup={enemy != null}"); // 기존 거리 대체와 정리 확인
            spawner.enabled = false; // 시험 후 출현 중지
            camera.gameObject.SetActive(true); // 실제 주 카메라 복구
            if (enemy != null) // 실제 적 존재 확인
            { // 조건 시작
                Destroy(enemy.gameObject); // 시험 적 제거
            } // 조건 끝
        } // 코루틴 끝

        private IEnumerator VerifyMovingCamera() // 같은 프레임 실제 카메라 이동 후 뷰포트
        { // 코루틴 시작
            yield return ClearFixture(); // 이전 시험 객체 정리
            ConfigureCamera(new Vector2(25f, 0f)); // 실제 플레이어와 떨어진 초광폭 카메라
            CameraFollow2D follow = camera.GetComponent<CameraFollow2D>(); // 실제 카메라 추적기 조회
            Require(follow != null, "Actual CameraFollow2D is missing."); // 실제 카메라 추적 확인
            SetField(follow, "smoothTime", 0.001f); // 같은 프레임 충분한 카메라 이동 통제
            follow.enabled = true; // 실제 카메라 LateUpdate 활성화
            GameObject observer = new GameObject("ShooterSpawnVerification_RenderProbe"); // 실제 렌더 직전 관찰 객체
            spawned.Add(observer); // 시험 객체 기록
            ShooterSpawnRenderProbe probe = observer.AddComponent<ShooterSpawnRenderProbe>(); // 실제 LateUpdate 관찰기
            probe.ObservedCamera = camera; // 실제 이동 카메라 연결
            SetSpawnerField("elapsed", 0f); // 독립 통제용 초기 출현 시간
            SetSpawnerField("nextSpawn", 0f); // 다음 실제 Update 출현 허용
            UnityEngine.Random.InitState(3317); // 이동 카메라 출현 방향 재현
            Time.timeScale = 8f; // 실제 프레임 카메라 이동 통제
            spawner.enabled = true; // 실제 출현기 활성화
            float deadline = Time.realtimeSinceStartup + 5f; // 실제 출현 제한 시간
            while (!probe.Observed && Time.realtimeSinceStartup < deadline) // 실제 카메라 갱신과 출현 대기
            { // 반복 시작
                yield return null; // 실제 프레임 진행
            } // 반복 끝
            Require(probe.Observed && probe.SpawnedEnemy != null, "Actual moving-camera spawn was not observed after LateUpdate."); // 실제 렌더 직전 관찰 확인
            float horizontal = 0.5f / (2f * camera.orthographicSize * camera.aspect); // 임시 가로 화면 여유
            float vertical = 0.5f / (2f * camera.orthographicSize); // 임시 세로 화면 여유
            Vector3 point = probe.AfterViewport; // 실제 렌더 직전 뷰포트 좌표
            bool outside = point.x <= -horizontal + 0.001f || point.x >= 1f + horizontal - 0.001f || point.y <= -vertical + 0.001f || point.y >= 1f + vertical - 0.001f; // 렌더 직전 여유 밖 여부
            Record("same-frame moving camera render position", probe.CameraMovement >= 0.5f && outside, $"cameraMovement={probe.CameraMovement:F3}, beforeObserved={probe.SawBefore}, beforeViewport={probe.BeforeViewport}, afterViewport={point}, outside={outside}"); // 실제 같은 프레임 이동 결과
            spawner.enabled = false; // 시험 후 자동 출현 정지
            Time.timeScale = 1f; // 시험 속도 복구
            follow.enabled = false; // 임시 카메라 추적 정지
            if (probe.SpawnedEnemy != null) // 실제 출현 적 존재 확인
            { // 조건 시작
                Destroy(probe.SpawnedEnemy.gameObject); // 실제 시험 적 제거
            } // 조건 끝
        } // 코루틴 끝

        private EarlyEnemyBehaviour CreateShooter(Vector3 position, float speedRatio, float readyAt) // 실제 사수 객체 구성
        { // 메서드 시작
            GameObject item = new GameObject("ShooterSpawnVerification_ControlledShooter"); // 통제 사수 객체
            spawned.Add(item); // 실제 시험 객체 기록
            item.transform.position = position; // 실제 사수 시작 위치
            Rigidbody2D rigidbody = item.AddComponent<Rigidbody2D>(); // 실제 사수 물리체
            rigidbody.bodyType = RigidbodyType2D.Kinematic; // 실제 사수 이동 방식
            EarlyEnemyBehaviour enemy = item.AddComponent<EarlyEnemyBehaviour>(); // 실제 적 행동 컴포넌트
            EarlyEnemySettings setting = new EarlyEnemySettings // 실제 시험용 사수 설정
            { // 설정 시작
                kind = EarlyEnemyKind.Shooter, // 실제 사수 종류
                health = 20f, // 임시 사수 체력
                damage = 8, // 임시 사수 피해
                speedRatio = speedRatio, // 통제 이동 속도
                attackInterval = 100f // 추가 발사 방지 간격
            }; // 설정 끝
            enemy.Configure(setting, player, body, health, null, 4f, 0f, 10f, 1f, 0.6f, 3f); // 실제 사수 설정 및 몸통 참조
            SetField(enemy, "nextAttack", readyAt); // 실제 발사 시각 통제
            return enemy; // 실제 사수 반환
        } // 메서드 끝

        private IEnumerator ClearFixture() // 이전 실제 통제 객체 정리
        { // 코루틴 시작
            foreach (GameObject item in spawned) // 시험 객체 순회
            { // 반복 시작
                if (item != null) // 객체 존재 확인
                { // 조건 시작
                    item.SetActive(false); // 실제 대상 즉시 제외
                    Destroy(item); // 지연 파괴 예약
                } // 조건 끝
            } // 반복 끝
            spawned.Clear(); // 이전 시험 객체 기록 초기화
            foreach (EnemyTestProjectile shot in UnityEngine.Object.FindObjectsByType<EnemyTestProjectile>(FindObjectsSortMode.None)) // 이전 실제 적 투사체 순회
            { // 반복 시작
                shot.gameObject.SetActive(false); // 실제 투사체 즉시 제외
                Destroy(shot.gameObject); // 실제 투사체 제거
            } // 반복 끝
            yield return null; // 지연 파괴 완료 대기
        } // 코루틴 끝

        private void ConfigureCamera(Vector2 offset) // 실제 카메라 뷰포트 통제
        { // 메서드 시작
            camera.gameObject.SetActive(true); // 실제 주 카메라 활성화
            camera.orthographicSize = 12f; // 세로 뷰포트 크기 통제
            camera.aspect = 32f / 9f; // 실제 16대9 외 초광폭 종횡비
            Vector3 head = player.transform.position; // 실제 머리 위치
            camera.transform.position = new Vector3(head.x + offset.x, head.y + offset.y, -10f); // 실제 카메라 이동과 오프셋
        } // 메서드 끝

        public static bool IsOutsideMargin(Camera current, Vector3 position, float worldMargin) // 실제 카메라 여유 바깥 확인
        { // 메서드 시작
            Vector3 point = current.WorldToViewportPoint(position); // 실제 월드 좌표 투영
            float horizontal = worldMargin / (2f * current.orthographicSize * current.aspect); // 월드 여유의 가로 비율
            float vertical = worldMargin / (2f * current.orthographicSize); // 월드 여유의 세로 비율
            return point.x <= -horizontal + 0.001f || point.x >= 1f + horizontal - 0.001f || point.y <= -vertical + 0.001f || point.y >= 1f + vertical - 0.001f; // 네 방향 중 화면 여유 밖 여부
        } // 메서드 끝

        private void SetSpawnerField(string name, object value) // 실제 출현기 통제 설정
        { // 메서드 시작
            SetField(spawner, name, value); // 실제 출현기 필드 변경
        } // 메서드 끝

        private static void SetField(object owner, string name, object value) // 실제 비공개 필드 통제
        { // 메서드 시작
            FieldInfo field = owner.GetType().GetField(name, Fields); // 실제 필드 조회
            Require(field != null, "Actual control field is missing: " + name); // 필수 설정 확인
            field.SetValue(owner, value); // 메모리 내 시험 설정
        } // 메서드 끝

        private void Record(string name, bool success, string detail) // 실제 독립 사례 결과 기록
        { // 메서드 시작
            if (!success) // 실제 사례 실패 확인
            { // 조건 시작
                failures++; // 실패 수 증가
            } // 조건 끝
            Debug.Log($"[ShooterSpawnVerification] {(success ? "PASS" : "FAIL")} {name}: {detail}."); // 실제 통제 결과 기록
        } // 메서드 끝

        private static void Require(bool condition, string message) // 필수 실제 시험 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                AutomatedShooterSpawnVerification.Finish(1, message); // 실제 실패 기록과 종료
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
