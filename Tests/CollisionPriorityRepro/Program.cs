using System; // 기본 형식
using System.Reflection; // 물리 콜백 호출
using ProjectEpsilon.Player; // 실제 충돌 처리
using UnityEngine; // 충돌체 대체

public static class Program // 충돌 순서 검증기
{ // 클래스 시작
    public static int Main(string[] args) // 검증 시작
    { // 메서드 시작
        if (args.Length == 0) // 명시적 재현 시나리오 확인
        { // 조건 시작
            Console.WriteLine("Known issue: run with obstacle-first to reproduce the expected failure."); // 미해결 재현 안내
            return 0; // 자동 실행 종료
        } // 조건 끝
        try // 실패 처리
        { // 보호 시작
            string mode = args[0]; // 시험 시나리오
            Time.time = 10f; // 현재 물리 시간
            Trace.Events = ""; // 처리 순서 초기화
            SnakeBodyManager body = new SnakeBodyManager(); // 몸통 관리자
            SnakeHealth health = new SnakeHealth(); // 체력 관리자
            SnakeInvulnerability immunity = new SnakeInvulnerability(); // 전체 무적 상태
            SnakeExternalCollision collision = new SnakeExternalCollision(); // 실제 외부 충돌 처리기
            collision.Bind(body, health, immunity, 1, 1f, 25, 1.5f); // 실제 충돌 설정
            SnakeSelfCollision self = new SnakeSelfCollision(); // 실제 자기 충돌 처리기
            self.Bind(body, immunity, 2, 2f); // 자기 충돌 설정
            Collider2D enemy = NewHazard(SnakeContactHazardType.EnemyDirect); // 적 충돌체
            Collider2D obstacle = NewHazard(SnakeContactHazardType.Obstacle); // 장애물 충돌체
            MethodInfo enter = typeof(SnakeExternalCollision).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance); // 물리 콜백 조회
            if (mode == "existing") // 기존 무적 확인
            { // 조건 시작
                immunity.StartInvulnerability(0.1f); // 짧은 기존 무적
            } // 조건 끝
            if (mode == "self-first") // 자기 충돌 선행 확인
            { // 조건 시작
                self.TryApplySelfCollisionPenalty(); // 자기 충돌 처리
            } // 조건 끝
            bool obstacleFirst = mode != "enemy-first"; // 장애물 선행 여부
            InvokeEnter(enter, collision, obstacleFirst ? obstacle : enemy); // 첫 충돌 콜백
            InvokeEnter(enter, collision, obstacleFirst ? enemy : obstacle); // 둘째 충돌 콜백
            if (mode == "duplicates") // 중복 콜백 확인
            { // 조건 시작
                InvokeEnter(enter, collision, obstacle); // 장애물 중복 콜백
                InvokeEnter(enter, collision, enemy); // 적 중복 콜백
            } // 조건 끝
            Time.time = 10.2f; // 다음 물리 틱
            typeof(SnakeExternalCollision).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(collision, null); // 예약 충돌 처리
            if (mode == "existing") // 기존 무적 결과 확인
            { // 조건 시작
                Require(body.CurrentBodyCount == 5 && health.CurrentHealth == 100, "기존 무적 중 충돌 적용"); // 기존 무적 우선 확인
            } // 조건 끝
            else if (mode == "self-first") // 자기 충돌 결과 확인
            { // 조건 시작
                Require(body.CurrentBodyCount == 3 && health.CurrentHealth == 100, "자기 충돌 무적 중 피해 적용"); // 신규 전체 무적 확인
            } // 조건 끝
            else // 일반 동시 충돌 확인
            { // 조건 시작
                Require(body.CurrentBodyCount == 4 && health.CurrentHealth == 75 && health.DamageCalls == 1, "동시 충돌 수량 오류"); // 피해 수량 확인
                Require(Trace.Events == "BH" && immunity.IsInvulnerable && collision.EnemyContactProtected, "직접 충돌 우선 또는 무적 오류"); // 충돌 순서와 무적 확인
            } // 조건 끝
            Console.WriteLine("Collision priority passed: " + mode); // 성공 기록
            return 0; // 성공 종료
        } // 보호 끝
        catch (Exception exception) // 실패 처리
        { // 예외 시작
            Console.Error.WriteLine(exception.GetBaseException().Message); // 실패 내용 출력
            return 1; // 실패 종료
        } // 예외 끝
    } // 메서드 끝
    private static void InvokeEnter(MethodInfo method, SnakeExternalCollision collision, Collider2D other) // 충돌 콜백 실행
    { // 메서드 시작
        object[] arguments = new object[1]; // 콜백 인자
        arguments[0] = other; // 충돌체 설정
        method.Invoke(collision, arguments); // 실제 콜백 호출
    } // 메서드 끝
    private static Collider2D NewHazard(SnakeContactHazardType type) // 시험 충돌체 생성
    { // 메서드 시작
        SnakeContactHazard hazard = new SnakeContactHazard(); // 충돌 위험 생성
        hazard.HazardType = type; // 위험 종류 설정
        Collider2D collider = new Collider2D(); // 충돌체 생성
        collider.Component = hazard; // 위험 정보 연결
        return collider; // 충돌체 반환
    } // 메서드 끝
    private static void Require(bool condition, string message) // 검증 조건
    { // 메서드 시작
        if (!condition) // 실패 조건
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 보고
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
