using System; // 기본 형식
using System.Reflection; // 물리 콜백 호출
using ProjectEpsilon.Player; // 실제 충돌 처리기
using UnityEngine; // 충돌체 대체

internal static class Program // 비활성 충돌 시험
{
    private static int failures; // 실패 횟수
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 메서드 범위

    private static int Main() // 시험 시작
    {
        Check("비활성 외부 충돌 무시", VerifyExternal); // 외부 충돌 검사
        Check("비활성 자기 충돌 예약 차단", VerifySelfQueued); // 자기 충돌 예약 검사
        Check("비활성 자기 충돌 대체 경로 차단", VerifySelfFallback); // 즉시 대체 경로 검사
        Check("공개 직접 판정 유지", VerifyPublicCalls); // 기존 직접 API 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    }

    private static void Check(string name, Action action) // 개별 시험
    {
        try // 시험 실행
        {
            action(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        }
        catch (Exception error) // 실패 처리
        {
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 기록
        }
    }

    private static (SnakeExternalCollision collision, SnakeSelfCollision self, SnakeBodyManager body, SnakeHealth health) Create() // 시험 구성
    {
        Time.time = 10f; // 현재 시간
        Debug.ErrorCount = 0; // 오류 기록 초기화
        SnakeBodyManager body = new SnakeBodyManager(); // 몸통 관리자
        SnakeHealth health = new SnakeHealth(); // 공용 체력
        SnakeInvulnerability immunity = new SnakeInvulnerability(); // 무적 상태
        health.Immunity = immunity; // 무적 연결
        SnakeExternalCollision collision = new SnakeExternalCollision(); // 실제 외부 충돌
        collision.Bind(body, health, immunity, 1, 1f, 25, 1.5f); // 충돌 설정
        Call(collision, "OnEnable"); // 물리 처리 등록
        SnakeSelfCollision self = new SnakeSelfCollision(); // 실제 자기 충돌
        self.Bind(body, immunity, 2, 2f); // 자기 충돌 설정
        self.Component = collision; // 외부 처리기 연결
        return (collision, self, body, health); // 시험 객체 반환
    }

    private static void Call(object instance, string name, params object[] arguments) // 내부 콜백 호출
    {
        instance.GetType().GetMethod(name, Private).Invoke(instance, arguments); // 실제 콜백 실행
    }

    private static Collider2D Hazard(SnakeContactHazardType type) // 시험용 충돌 대상
    {
        return new Collider2D { Component = new SnakeContactHazard { HazardType = type } }; // 충돌체 구성
    }

    private static void VerifyExternal() // 비활성 외부 충돌과 재활성
    {
        var fixture = Create(); // 시험 구성
        fixture.collision.isActiveAndEnabled = false; // 외부 충돌 비활성
        Call(fixture.collision, "OnDisable"); // 스텝 구독 해제
        Call(fixture.collision, "OnTriggerEnter2D", Hazard(SnakeContactHazardType.EnemyDirect)); // 비활성 적 충돌
        Call(fixture.collision, "OnTriggerEnter2D", Hazard(SnakeContactHazardType.Obstacle)); // 비활성 장애물 충돌
        Require(fixture.body.CurrentBodyCount == 5 && fixture.health.CurrentHealth == 100 && Debug.ErrorCount == 0, "비활성 외부 충돌이 피해 또는 대체 오류 발생"); // 비활성 상태 확인
        fixture.collision.isActiveAndEnabled = true; // 충돌 처리 재활성
        Call(fixture.collision, "OnEnable"); // 스텝 구독 복구
        Call(fixture.collision, "OnTriggerEnter2D", Hazard(SnakeContactHazardType.EnemyDirect)); // 재활성 적 충돌
        Call(fixture.collision, "ResolvePendingContacts"); // 물리 스텝 완료
        Require(fixture.body.CurrentBodyCount == 4, "재활성 적 충돌 누락"); // 정상 피해 복구 확인
    }

    private static void VerifySelfQueued() // 비활성 자기 충돌이 나중에 적용되는 경계
    {
        var fixture = Create(); // 시험 구성
        fixture.self.isActiveAndEnabled = false; // 자기 충돌 비활성
        Call(fixture.self, "OnTriggerEnter2D", new Collider2D { Component = new SnakeSegment() }); // 비활성 몸통 접촉
        fixture.self.isActiveAndEnabled = true; // 스텝 완료 전 재활성
        Call(fixture.collision, "ResolvePendingContacts"); // 물리 스텝 완료
        Require(fixture.body.CurrentBodyCount == 5, "비활성 중 수집된 자기 충돌이 재활성 뒤 적용"); // 예약 차단 확인
    }

    private static void VerifySelfFallback() // 처리기 없는 비활성 자기 충돌
    {
        var fixture = Create(); // 시험 구성
        fixture.self.isActiveAndEnabled = false; // 자기 충돌 비활성
        fixture.self.Component = null; // 외부 처리기 부재
        Call(fixture.self, "OnTriggerEnter2D", new Collider2D { Component = new SnakeSegment() }); // 비활성 접촉
        Require(fixture.body.CurrentBodyCount == 5 && Debug.ErrorCount == 0, "비활성 자기 충돌이 직접 피해 또는 오류 발생"); // 대체 경로 차단 확인
    }

    private static void VerifyPublicCalls() // 공개 즉시 API 호환
    {
        var fixture = Create(); // 시험 구성
        fixture.collision.isActiveAndEnabled = false; // 외부 처리 비활성
        Require(fixture.collision.TryApplyEnemyDirectContact() && fixture.body.CurrentBodyCount == 4, "공개 즉시 충돌 API 변경"); // 기존 직접 호출 유지
        var selfFixture = Create(); // 자기 충돌 시험 구성
        selfFixture.self.isActiveAndEnabled = false; // 자기 충돌 처리 비활성
        Require(selfFixture.self.TryApplySelfCollisionPenalty() && selfFixture.body.CurrentBodyCount == 3, "공개 자기 충돌 API 변경" ); // 기존 자기 충돌 직접 호출 유지
    }

    private static void Require(bool condition, string message) // 조건 확인
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 전달
        }
    }
}
