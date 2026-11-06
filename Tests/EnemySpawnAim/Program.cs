using System; // 기본 형식
using System.Linq; // 생성 객체 조회
using System.Reflection; // 실제 내부 경로 호출
using ProjectEpsilon.Combat; // 전투 상태 대체
using ProjectEpsilon.Enemies; // 실제 적 코드
using ProjectEpsilon.Player; // 플레이어 상태 대체
using UnityEngine; // 위치와 카메라 대체

internal static class Program // 출현 및 조준 시험
{ // 클래스 시작
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 비공개 범위
    private static int failures; // 실패 횟수
    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("사수의 최근접 몸통 발사", VerifyShooterBody); // 몸통 조준 검사
        Check("몸통 부재 시 머리 발사", VerifyShooterFallback); // 머리 대체 검사
        Check("일반 적 화면 밖 출현", () => VerifySpawn(EarlyEnemyKind.Chaser, 16f / 9f, new Vector3(), true)); // 일반 출현 검사
        Check("엘리트 화면 밖 출현", () => VerifySpawn(EarlyEnemyKind.Elite, 16f / 9f, new Vector3(), true)); // 엘리트 출현 검사
        Check("이동된 넓은 카메라 밖 출현", () => VerifySpawn(EarlyEnemyKind.Chaser, 2.4f, new Vector3(2f, 1f, 0f), true)); // 이동 카메라 검사
        Check("카메라 없는 최소 거리 출현", () => VerifySpawn(EarlyEnemyKind.Chaser, 16f / 9f, new Vector3(), false)); // 카메라 대체 검사
        Check("초광폭 화면 밖 출현 후 생존", VerifyWideSpawnSurvivesCleanup); // 생성 직후 거리 정리 검사
        Check("시간대별 출현과 간격", VerifySchedule); // 출현 순서 검사
        Check("만석 시 4분 엘리트", VerifyEliteAtCapacity); // 상한 유지 검사
        Check("다방향 화면 여유와 거리 정리", VerifyViewportSweep); // 종횡비와 이동 카메라 검사
        Check("멀리 벗어난 적 정리", VerifyFarCleanup); // 거리 정리 유지 검사
        Check("프레임 후반 카메라 이동", VerifyCameraLateMovement); // 카메라 추적 이후 출현 검사
        Check("일시정지 중 시간과 출현 정지", VerifyPause); // 일시정지 출현 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝
    private static void Check(string label, Action action) // 개별 시험 실행
    { // 메서드 시작
        try { action(); Console.WriteLine($"PASS: {label}"); } // 성공 기록
        catch (Exception error) { failures++; Console.WriteLine($"FAIL: {label}: {error.GetBaseException().Message}"); } // 실패 기록
    } // 메서드 끝
    private static object Invoke(object instance, string name, params object[] args) => instance.GetType().GetMethod(name, Private).Invoke(instance, args); // 실제 내부 메서드 실행
    private static void Set(object instance, string name, object value) => instance.GetType().GetField(name, Private).SetValue(instance, value); // 직렬화 값 설정
    private static void Require(bool condition, string message) { if (!condition) { throw new InvalidOperationException(message); } } // 검증 조건
    private static (SnakeMovement player, SnakeBodyManager body, SnakeHealth health) Player() // 플레이어 구성
    { // 메서드 시작
        GameObject.Objects.Clear(); // 기존 객체 초기화
        Camera.main = null; // 카메라 초기화
        UnityEngine.Random.Selected = 0; // 추첨 순번 초기화
        GameObject root = new GameObject("Player"); // 플레이어 객체
        root.transform.position = new Vector3(5f, 0f, 0f); // 머리 위치
        return (root.AddComponent<SnakeMovement>(), root.AddComponent<SnakeBodyManager>(), root.AddComponent<SnakeHealth>()); // 플레이어 구성 반환
    } // 메서드 끝
    private static void VerifyShooterBody() // 사수 몸통 조준 확인
    { // 메서드 시작
        var fixture = Player(); // 플레이어 구성
        GameObject near = new GameObject("NearBody"); // 가까운 몸통
        near.transform.position = new Vector3(0f, 3f, 0f); // 가까운 몸통 위치
        fixture.body.BodySegments.Add(null); // 소멸한 몸통 대체
        GameObject inactive = new GameObject("InactiveBody"); // 비활성 몸통
        inactive.transform.position = new Vector3(0f, 1f, 0f); // 가까운 비활성 위치
        inactive.activeInHierarchy = false; // 몸통 객체 비활성
        fixture.body.BodySegments.Add(inactive.AddComponent<SnakeSegment>()); // 비활성 몸통 등록
        GameObject overlap = new GameObject("OverlapBody"); // 사수와 겹친 몸통
        overlap.transform.position = new Vector3(); // 유효 방향 없는 위치
        fixture.body.BodySegments.Add(overlap.AddComponent<SnakeSegment>()); // 겹친 몸통 등록
        fixture.body.BodySegments.Add(near.AddComponent<SnakeSegment>()); // 유효 몸통 등록
        GameObject tied = new GameObject("TiedBody" ); // 같은 거리의 둘째 몸통
        tied.transform.position = new Vector3(3f, 0f, 0f); // 정확한 동률 위치
        fixture.body.BodySegments.Add(tied.AddComponent<SnakeSegment>()); // 동률 몸통 등록
        GameObject far = new GameObject("FarBody"); // 먼 몸통
        far.transform.position = new Vector3(-7f, 0f, 0f); // 먼 몸통 위치
        fixture.body.BodySegments.Add(far.AddComponent<SnakeSegment>()); // 먼 몸통 등록
        Vector2 aim = Fire(fixture.player, fixture.body, fixture.health); // 실제 사수 발사
        Require(MathF.Abs(aim.x) < 0.001f && aim.y > 0.999f, "사수가 가장 가까운 몸통 대신 머리를 조준"); // 몸통 조준 확인
    } // 메서드 끝
    private static void VerifyShooterFallback() // 몸통 부재 시 머리 대체 확인
    { // 메서드 시작
        var fixture = Player(); // 몸통 없는 플레이어
        Vector2 aim = Fire(fixture.player, fixture.body, fixture.health); // 실제 사수 발사
        Require(aim.x > 0.999f && MathF.Abs(aim.y) < 0.001f, "몸통 부재 시 머리 조준 누락"); // 머리 대체 확인
    } // 메서드 끝
    private static Vector2 Fire(SnakeMovement player, SnakeBodyManager body, SnakeHealth health) // 실제 사수 이동과 발사
    { // 메서드 시작
        GameObject enemy = new GameObject("Shooter"); // 사수 객체
        enemy.AddComponent<Rigidbody2D>(); // 물리체 연결
        enemy.AddComponent<WeaponTargetStatusController>(); // 상태 연결
        enemy.AddComponent<WeaponTargetKnockbackController>(); // 넉백 연결
        EarlyEnemyBehaviour behaviour = enemy.AddComponent<EarlyEnemyBehaviour>(); // 실제 적 행동
        EarlyEnemySettings settings = new EarlyEnemySettings { kind = EarlyEnemyKind.Shooter, attackInterval = 1f, speedRatio = 0.7f, damage = 8 }; // 사수 설정
        Time.time = 0f; // 설정 시각
        behaviour.Configure(settings, player, body, health, null, 4f, 5f, 3f, 1f, 0.6f, 3f); // 실제 구성
        Time.time = 2f; // 발사 시각
        Invoke(behaviour, "FixedUpdate"); // 실제 물리 갱신
        Rigidbody2D movement = enemy.GetComponent<Rigidbody2D>(); // 실제 이동 물리체
        Require(movement.position.x > 0f && MathF.Abs(movement.position.y) < 0.001f, "사수의 머리 기준 이동 방향 변경" ); // 기존 이동 방향 확인
        return EnemyTestProjectile.LastDirection; // 투사체 발사 방향
    } // 메서드 끝
    private static void VerifyPause() // 출현 갱신의 일시정지 확인
    { // 메서드 시작
        Player(); // 플레이어 구성
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        Set(spawner, "elapsed", 239.99f); // 엘리트 직전 시각
        ProjectEpsilon.Core.GameManager.Instance.IsPlaying = false; // 게임 일시정지
        Time.deltaTime = 0.02f; // 비정상 시간값 대조
        Invoke(spawner, "LateUpdate"); // 정지 중 갱신
        float elapsed = (float)spawner.GetType().GetField("elapsed", Private).GetValue(spawner); // 출현 경과 시간
        Require(spawner.ActiveEnemies == 0 && MathF.Abs(elapsed - 239.99f) < 0.001f, "일시정지 중 출현 또는 시간 증가"); // 정지 상태 확인
        ProjectEpsilon.Core.GameManager.Instance.IsPlaying = true; // 게임 재개
        Invoke(spawner, "LateUpdate"); // 재개 후 갱신
        Require(GameObject.Objects.Count(value => value.name == "EarlyEnemy_Elite") == 1, "재개 후 엘리트 출현 누락"); // 재개 출현 확인
    } // 메서드 끝
    private static void VerifyCameraLateMovement() // 카메라 추적 후 출현 확인
    { // 메서드 시작
        var fixture = Player(); // 플레이어 구성
        fixture.player.transform.position = new Vector3(); // 플레이어 기준점
        GameObject cameraObject = new GameObject("Camera"); // 카메라 객체
        Camera.main = cameraObject.AddComponent<Camera>(); // 주 카메라 지정
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        UnityEngine.Random.insideUnitCircle = new Vector2(1f, 0f); // 오른쪽 출현 방향
        Time.deltaTime = 0.02f; // 프레임 시간
        spawner.GetType().GetMethod("Update", Private)?.Invoke(spawner, null); // 카메라 이전 갱신
        cameraObject.transform.position = new Vector3(10f, 0f, 0f); // 같은 프레임 카메라 이동
        spawner.GetType().GetMethod("LateUpdate", Private)?.Invoke(spawner, null); // 카메라 이후 갱신
        GameObject enemy = GameObject.Objects.Last(value => value.name == "EarlyEnemy_Chaser"); // 생성된 적 조회
        Vector3 viewport = Camera.main.WorldToViewportPoint(enemy.transform.position); // 최종 카메라의 위치
        float horizontal = 0.5f / (2f * Camera.main.orthographicSize * Camera.main.aspect); // 가로 화면 여유
        float vertical = 0.5f / (2f * Camera.main.orthographicSize); // 세로 화면 여유
        Require(viewport.x < -horizontal || viewport.x > 1f + horizontal || viewport.y < -vertical || viewport.y > 1f + vertical, "카메라 이동 뒤 화면 내부 출현"); // 최종 화면 여유 확인
    } // 메서드 끝
    private static void VerifyViewportSweep() // 다양한 카메라와 방향 확인
    { // 메서드 시작
        float[] aspects = { 0.5625f, 16f / 9f, 2.4f, 6f }; // 세로와 가로 화면 비율
        foreach (float aspect in aspects) // 화면 비율 순회
        { // 반복 시작
            var fixture = Player(); // 플레이어 구성
            fixture.player.transform.position = new Vector3(); // 플레이어 기준점
            GameObject cameraObject = new GameObject("Camera"); // 카메라 객체
            cameraObject.transform.position = new Vector3(15f, 8f, 0f); // 플레이어에서 이동한 카메라
            Camera.main = cameraObject.AddComponent<Camera>(); // 주 카메라 지정
            Camera.main.aspect = aspect; // 화면 비율 설정
            GameObject owner = new GameObject("Spawner"); // 출현기 객체
            EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
            Invoke(spawner, "Start"); // 참조 초기화
            for (int index = 0; index < 16; index++) // 원주 방향 검사
            { // 반복 시작
                float angle = index * MathF.PI / 8f; // 출현 각도
                UnityEngine.Random.insideUnitCircle = new Vector2(MathF.Cos(angle), MathF.Sin(angle)); // 시험 방향
                Invoke(spawner, "Spawn", new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 25f, experience = 1, attackInterval = 1f }); // 실제 적 생성
                GameObject enemy = GameObject.Objects.Last(value => value.name == "EarlyEnemy_Chaser"); // 생성 적 조회
                Vector3 point = enemy.transform.position; // 생성 위치
                Vector3 viewport = Camera.main.WorldToViewportPoint(point); // 생성 뷰포트 좌표
                float horizontal = 0.5f / (2f * Camera.main.orthographicSize * aspect); // 가로 화면 여유
                float vertical = 0.5f / (2f * Camera.main.orthographicSize); // 세로 화면 여유
                Require(viewport.x < -horizontal || viewport.x > 1f + horizontal || viewport.y < -vertical || viewport.y > 1f + vertical, "화면 여유 내부 생성"); // 화면 밖 확인
                Require(point.sqrMagnitude >= 80.999f, "최소 출현 거리 미달"); // 최소 거리 확인
                int before = spawner.ActiveEnemies; // 정리 전 생존 수
                Invoke(spawner, "CleanupDistantEnemies"); // 실제 거리 정리
                Require(spawner.ActiveEnemies == before, "화면 밖 출현 적 즉시 정리"); // 생성 직후 보존 확인
            } // 반복 끝
        } // 반복 끝
    } // 메서드 끝
    private static void VerifyFarCleanup() // 카메라 부재와 기본 거리 정리 확인
    { // 메서드 시작
        Player(); // 플레이어 구성
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        Invoke(spawner, "Spawn", new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 25f, experience = 1, attackInterval = 1f }); // 일반 적 생성
        GameObject enemy = GameObject.Objects.Last(value => value.name == "EarlyEnemy_Chaser"); // 생성 적 조회
        enemy.transform.position = new Vector3(100f, 100f, 0f); // 먼 적 위치
        Invoke(spawner, "CleanupDistantEnemies"); // 거리 정리 실행
        Require(spawner.ActiveEnemies == 0, "카메라 부재 시 먼 적 정리 누락"); // 거리 정리 확인
    } // 메서드 끝
    private static void VerifySchedule() // 시간대별 적 종류와 간격 확인
    { // 메서드 시작
        Player(); // 플레이어 구성
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        Time.deltaTime = 0.02f; // 프레임 시간
        Invoke(spawner, "LateUpdate"); // 초기 추적자 출현
        Require(spawner.ActiveEnemies == 1 && GameObject.Objects.Any(value => value.name == "EarlyEnemy_Chaser"), "초기 추적자 누락"); // 초기 출현 확인
        Invoke(spawner, "LateUpdate"); // 출현 간격 전 갱신
        Require(spawner.ActiveEnemies == 1, "출현 간격 전에 적 추가"); // 출현 간격 확인
        Set(spawner, "elapsed", 59.99f); // 1분 경계
        UnityEngine.Random.Selected = 1; // 포식자 선택
        Invoke(spawner, "LateUpdate"); // 1분 출현
        Require(GameObject.Objects.Any(value => value.name == "EarlyEnemy_Predator"), "1분 포식자 출현 누락"); // 포식자 확인
        Set(spawner, "elapsed", 119.99f); // 2분 경계
        UnityEngine.Random.Selected = 2; // 사수 선택
        Invoke(spawner, "LateUpdate"); // 2분 출현
        Require(GameObject.Objects.Any(value => value.name == "EarlyEnemy_Shooter"), "2분 사수 출현 누락"); // 사수 확인
        Set(spawner, "elapsed", 239.99f); // 4분 경계
        Invoke(spawner, "LateUpdate"); // 4분 출현
        Require(GameObject.Objects.Count(value => value.name == "EarlyEnemy_Elite") == 1, "4분 엘리트 출현 누락"); // 엘리트 확인
        Invoke(spawner, "LateUpdate"); // 다음 갱신
        Require(GameObject.Objects.Count(value => value.name == "EarlyEnemy_Elite") == 1, "엘리트 중복 출현"); // 단회 출현 확인
    } // 메서드 끝
    private static void VerifyEliteAtCapacity() // 만석에서도 엘리트 자리 확보 확인
    { // 메서드 시작
        Player(); // 플레이어 구성
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        EarlyEnemySettings setting = new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 25f, experience = 1, attackInterval = 1f }; // 일반 적 설정
        for (int index = 0; index < 40; index++) { Invoke(spawner, "Spawn", setting); } // 일반 적 만석 구성
        Set(spawner, "elapsed", 239.99f); // 4분 경계
        Time.deltaTime = 0.02f; // 프레임 시간
        Invoke(spawner, "LateUpdate"); // 엘리트 출현 갱신
        Require(spawner.ActiveEnemies == 40 && GameObject.Objects.Count(value => value.name == "EarlyEnemy_Elite") == 1, "만석 엘리트 또는 적 상한 오류"); // 적 수와 엘리트 확인
    } // 메서드 끝
    private static void VerifyWideSpawnSurvivesCleanup() // 넓은 화면의 생성 직후 정리 확인
    { // 메서드 시작
        var fixture = Player(); // 플레이어 구성
        fixture.player.transform.position = new Vector3(); // 플레이어 원점
        GameObject cameraObject = new GameObject("Camera"); // 카메라 객체
        Camera.main = cameraObject.AddComponent<Camera>(); // 주 카메라 지정
        Camera.main.aspect = 6f; // 초광폭 화면 비율
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        UnityEngine.Random.insideUnitCircle = new Vector2(1f, 0f); // 화면 오른쪽 방향
        Invoke(spawner, "Spawn", new EarlyEnemySettings { kind = EarlyEnemyKind.Chaser, health = 25f, experience = 1, attackInterval = 1f }); // 실제 적 생성
        Invoke(spawner, "CleanupDistantEnemies"); // 다음 갱신 거리 정리
        Require(spawner.ActiveEnemies == 1, "화면 밖에 생성한 적이 즉시 거리 정리됨"); // 생성 직후 생존 확인
    } // 메서드 끝
    private static void VerifySpawn(EarlyEnemyKind kind, float aspect, Vector3 cameraPosition, bool cameraPresent) // 실제 출현 경로 확인
    { // 메서드 시작
        var fixture = Player(); // 플레이어 구성
        fixture.player.transform.position = new Vector3(); // 출현 기준 원점
        if (cameraPresent) // 카메라 설정 여부
        { // 조건 시작
            GameObject cameraObject = new GameObject("Camera"); // 카메라 객체
            cameraObject.transform.position = cameraPosition; // 이동 카메라 위치
            Camera.main = cameraObject.AddComponent<Camera>(); // 주 카메라 지정
            Camera.main.aspect = aspect; // 화면 종횡비
        } // 조건 끝
        GameObject owner = new GameObject("Spawner"); // 출현기 객체
        EarlyEnemyWaveSpawner spawner = owner.AddComponent<EarlyEnemyWaveSpawner>(); // 실제 출현기
        Invoke(spawner, "Start"); // 참조 초기화
        UnityEngine.Random.insideUnitCircle = new Vector2(8f, 4.12f); // 화면 내부 경계 방향
        Invoke(spawner, "Spawn", new EarlyEnemySettings { kind = kind, health = 25f, experience = 1, attackInterval = 1f }); // 실제 적 생성
        GameObject enemy = GameObject.Objects.Last(value => value.name == "EarlyEnemy_" + kind); // 생성 적 조회
        Vector3 point = enemy.transform.position; // 출현 위치
        Require(point.sqrMagnitude >= 80.999f, "기존 최소 출현 거리 미달"); // 최소 거리 확인
        if (cameraPresent) // 카메라 존재 확인
        { // 조건 시작
            Vector3 viewport = Camera.main.WorldToViewportPoint(point); // 출현 뷰포트 위치
            float horizontal = 0.5f / (2f * Camera.main.orthographicSize * Camera.main.aspect); // 임시 가로 여유
            float vertical = 0.5f / (2f * Camera.main.orthographicSize); // 임시 세로 여유
            Require(viewport.x < -horizontal || viewport.x > 1f + horizontal || viewport.y < -vertical || viewport.y > 1f + vertical, $"화면 여유 내부 출현: ({viewport.x:F3}, {viewport.y:F3})"); // 여유 밖 출현 확인
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
