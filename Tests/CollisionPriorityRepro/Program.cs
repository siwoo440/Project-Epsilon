using System; // 기본 형식
using System.Reflection; // 물리 콜백 호출
using ProjectEpsilon.Player; // 실제 충돌 처리
using UnityEngine; // 충돌체 대체

public static class Program // 충돌 순서 검증기
{
    public static int Main(string[] args) // 검증 시작
    {
        if (args.Length == 0) // 명시적 재현 시나리오 확인
        {
            Console.WriteLine("Specify a collision scenario, such as obstacle-first."); // 충돌 시나리오 안내
            return 0; // 자동 실행 종료
        }
        try // 실패 처리
        {
            string mode = args[0]; // 시험 시나리오
            Time.time = 10f; // 현재 물리 시간
            Trace.Events = ""; // 처리 순서 초기화
            SnakeBodyManager body = new SnakeBodyManager(); // 몸통 관리자
            SnakeHealth health = new SnakeHealth(); // 체력 관리자
            SnakeInvulnerability immunity = new SnakeInvulnerability(); // 전체 무적 상태
            health.Immunity = immunity; // 체력 무적 연결
            SnakeExternalCollision collision = new SnakeExternalCollision(); // 실제 외부 충돌 처리기
            collision.Bind(body, health, immunity, 1, 1f, 25, 1.5f); // 실제 충돌 설정
            typeof(SnakeExternalCollision).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(collision, null); // 물리 스텝 등록
            SnakeSelfCollision self = new SnakeSelfCollision(); // 실제 자기 충돌 처리기
            self.Bind(body, immunity, 2, 2f); // 자기 충돌 설정
            self.Component = collision; // 같은 플레이어의 충돌 관리자
            Collider2D enemy = NewHazard(SnakeContactHazardType.EnemyDirect); // 적 충돌체
            Collider2D obstacle = NewHazard(SnakeContactHazardType.Obstacle); // 장애물 충돌체
            MethodInfo enter = typeof(SnakeExternalCollision).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance); // 물리 콜백 조회
            MethodInfo selfEnter = typeof(SnakeSelfCollision).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance); // 자기 충돌 콜백 조회
            Collider2D ownBody = new Collider2D(); // 몸통 충돌체
            ownBody.Component = new SnakeSegment(); // 자기 몸통 정보
            if (mode == "boss-grace-before-self" || mode == "boss-grace-after-self") // 보스 유예와 자기 접촉 순서 확인
            {
                SnakeSegment protectedBody = (SnakeSegment)ownBody.Component; // 실제 보호 몸통 대체
                protectedBody.IsBossPushGraceActive = mode == "boss-grace-before-self"; // 먼저 시작된 유예 상태
                selfEnter.Invoke(self, new object[] { ownBody }); // 실제 자기 충돌 진입
                protectedBody.IsBossPushGraceActive = true; // 같은 스텝의 보스 유예 확정
                typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 실제 물리 스텝 해결
                Require(body.CurrentBodyCount == 5, "보스 밀림 유예 중 자기 충돌 적용"); // 두 콜백 순서의 유예 확인
                protectedBody.IsBossPushGraceActive = false; // 실제 유예 종료 대체
                typeof(SnakeSelfCollision).GetMethod("OnTriggerStay2D", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, new object[] { ownBody }); // 실제 지속 접촉 재판정
                typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 재판정 스텝 해결
                Require(body.CurrentBodyCount == 3, "유예 종료 후 지속 자기 충돌 미처리"); // 실제 지속 접촉 손실 확인
                Console.WriteLine("Collision priority passed: " + mode); // 실제 시험 성공 기록
                return 0; // 독립 시험 종료
            }
            if (mode == "boss-grace-other") // 보호 몸통과 다른 몸통 동시 접촉
            {
                ((SnakeSegment)ownBody.Component).IsBossPushGraceActive = true; // 첫 몸통 유예 대체
                selfEnter.Invoke(self, new object[] { ownBody }); // 유예 몸통 실제 진입
                Collider2D otherBody = new Collider2D(); // 비보호 몸통 충돌체
                otherBody.Component = new SnakeSegment(); // 별도 몸통 대체
                selfEnter.Invoke(self, new object[] { otherBody }); // 비보호 몸통 실제 진입
                typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 실제 같은 스텝 처리
                Require(body.CurrentBodyCount == 3, "비보호 몸통까지 보스 유예 적용"); // 다른 몸통 자기 충돌 확인
                Console.WriteLine("Collision priority passed: " + mode); // 실제 시험 성공 기록
                return 0; // 독립 시험 종료
            }
            if (mode == "disable") // 비활성화 중 절단 취소 확인
            {
                int callbacks = 0; // 절단 완료 알림 수
                bool result = true; // 절단 성공 초기값
                Action<bool> completed = applied => // 절단 결과 수신기
                {
                    callbacks++; // 알림 횟수 증가
                    result = applied; // 절단 결과 저장
                };
                collision.QueueEliteCut(completed); // 절단 결과 예약
                typeof(SnakeExternalCollision).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 처리기 비활성화
                Require(callbacks == 1 && !result && body.CurrentBodyCount == 5, "비활성화 절단 취소 누락"); // 취소 결과 확인
                Console.WriteLine("Collision priority passed: disable"); // 성공 기록
                return 0; // 성공 종료
            }
            if (mode == "disable-during") // 몸통 손실 중 처리기 비활성화 확인
            {
                int callbacks = 0; // 절단 완료 알림 수
                bool result = true; // 절단 성공 초기값
                Action<bool> completed = applied => // 절단 결과 수신기
                {
                    callbacks++; // 알림 횟수 증가
                    result = applied; // 절단 결과 저장
                };
                body.OnRemoved = () => // 몸통 손실 시 비활성화
                {
                    collision.isActiveAndEnabled = false; // 처리기 비활성 상태
                    typeof(SnakeExternalCollision).GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 실제 비활성 처리
                };
                collision.QueueEliteCut(completed); // 절단 결과 예약
                InvokeEnter(enter, collision, enemy); // 적 직접 충돌 예약
                InvokeEnter(enter, collision, obstacle); // 장애물 충돌 예약
                typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 같은 스텝 충돌 해결
                Require(callbacks == 1 && !result && body.CurrentBodyCount == 4 && health.CurrentHealth == 100, "비활성 중 남은 충돌 적용"); // 절단 취소와 피해 차단 확인
                Console.WriteLine("Collision priority passed: disable-during"); // 성공 기록
                return 0; // 성공 종료
            }
            if (mode == "generation") // 플레이 시작 재등록 확인
            {
                int registrations = SnakePhysicsStepDispatcher.RegisterCalls; // 기존 등록 횟수
                SnakePhysicsStepDispatcher.Generation++; // 새 플레이 등록 세대
                typeof(SnakeExternalCollision).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collision, null); // 물리 스텝 등록 점검
                Require(SnakePhysicsStepDispatcher.RegisterCalls == registrations + 1, "리로드 생략 후 재등록 누락"); // 새 세대 재등록 확인
                Console.WriteLine("Collision priority passed: generation"); // 성공 기록
                return 0; // 성공 종료
            }
            if (mode == "fallback") // 지원되지 않는 물리 설정 확인
            {
                SnakePhysicsStepDispatcher.CanQueue = false; // 자동 물리 사용 불가
                InvokeEnter(enter, collision, enemy); // 즉시 적 충돌 대체
                Require(body.CurrentBodyCount == 4 && Debug.ErrorCount == 1, "즉시 판정 대체 또는 오류 누락"); // 피해 보존과 오류 확인
                Console.WriteLine("Collision priority passed: fallback"); // 성공 기록
                return 0; // 성공 종료
            }
            if (mode == "existing") // 기존 무적 확인
            {
                immunity.StartInvulnerability(0.1f); // 짧은 기존 무적
            }
            if (mode == "self-first") // 자기 충돌 선행 확인
            {
                self.TryApplySelfCollisionPenalty(); // 자기 충돌 처리
            }
            if (mode == "damage-first") // 일반 피해 선행 확인
            {
                MethodInfo queueDamage = typeof(SnakeExternalCollision).GetMethod("QueueHealthDamage"); // 피해 큐 조회
                if (queueDamage == null) // 기존 즉시 피해 확인
                {
                    health.TakeDamage(10); // 기존 피해 처리
                }
                else // 새 피해 큐 확인
                {
                    queueDamage.Invoke(collision, new object[] { health, 10 }); // 대상과 일반 피해 예약
                }
            }
            if (mode == "self-callback-first") // 자기 충돌 선행 예약
            {
                selfEnter.Invoke(self, new object[] { ownBody }); // 실제 자기 충돌 콜백
            }
            bool obstacleFirst = mode != "enemy-first"; // 장애물 선행 여부
            InvokeEnter(enter, collision, obstacleFirst ? obstacle : enemy); // 첫 충돌 콜백
            InvokeEnter(enter, collision, obstacleFirst ? enemy : obstacle); // 둘째 충돌 콜백
            if (mode == "self-callback-last") // 자기 충돌 후행 예약
            {
                selfEnter.Invoke(self, new object[] { ownBody }); // 실제 자기 충돌 콜백
            }
            if (mode == "duplicates") // 중복 콜백 확인
            {
                InvokeEnter(enter, collision, obstacle); // 장애물 중복 콜백
                InvokeEnter(enter, collision, enemy); // 적 중복 콜백
            }
            typeof(SnakeExternalCollision).GetMethod("ResolvePendingContacts", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(collision, null); // 같은 물리 스텝 처리
            if (mode == "existing") // 기존 무적 결과 확인
            {
                Require(body.CurrentBodyCount == 5 && health.CurrentHealth == 100, "기존 무적 중 충돌 적용"); // 기존 무적 우선 확인
            }
            else if (mode == "self-first" || mode == "self-callback-first" || mode == "self-callback-last") // 자기 충돌 결과 확인
            {
                Require(body.CurrentBodyCount == 3 && health.CurrentHealth == 100, "자기 충돌 무적 중 피해 적용"); // 신규 전체 무적 확인
            }
            else if (mode == "damage-first") // 일반 피해 선행 결과 확인
            {
                Require(body.CurrentBodyCount == 4 && health.CurrentHealth == 75 && health.DamageCalls == 1, "장애물 무적 전 일반 피해 적용"); // 새 무적 우선 확인
                Require(Trace.Events == "BH", "장애물 전 일반 피해 처리"); // 몸통과 장애물 우선 확인
            }
            else // 일반 동시 충돌 확인
            {
                Require(body.CurrentBodyCount == 4 && health.CurrentHealth == 75 && health.DamageCalls == 1, "동시 충돌 수량 오류"); // 피해 수량 확인
                Require(Trace.Events == "BH" && immunity.IsInvulnerable && collision.EnemyContactProtected, "직접 충돌 우선 또는 무적 오류"); // 충돌 순서와 무적 확인
            }
            Console.WriteLine("Collision priority passed: " + mode); // 성공 기록
            return 0; // 성공 종료
        }
        catch (Exception exception) // 실패 처리
        {
            Console.Error.WriteLine(exception.GetBaseException().Message); // 실패 내용 출력
            return 1; // 실패 종료
        }
    }
    private static void InvokeEnter(MethodInfo method, SnakeExternalCollision collision, Collider2D other) // 충돌 콜백 실행
    {
        object[] arguments = new object[1]; // 콜백 인자
        arguments[0] = other; // 충돌체 설정
        method.Invoke(collision, arguments); // 실제 콜백 호출
    }
    private static Collider2D NewHazard(SnakeContactHazardType type) // 시험 충돌체 생성
    {
        SnakeContactHazard hazard = new SnakeContactHazard(); // 충돌 위험 생성
        hazard.HazardType = type; // 위험 종류 설정
        Collider2D collider = new Collider2D(); // 충돌체 생성
        collider.Component = hazard; // 위험 정보 연결
        return collider; // 충돌체 반환
    }
    private static void Require(bool condition, string message) // 검증 조건
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 보고
        }
    }
}
