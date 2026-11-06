using System; // 기본 형식
using System.Reflection; // 실제 공격 경로 호출
using ProjectEpsilon.Combat; // 무기 및 표적
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Player; // 몸통 구성
using UnityEngine; // 좌표와 시간

internal static class Program // 강철 검 등급 시험
{ // 클래스 시작
    private static int failures; // 실패 수
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 메서드 범위

    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("1·2등급 단일 대상", VerifyLowerGrades); // 낮은 등급 검사
        Check("3·5등급 최대 두 대상", VerifyHigherGrades); // 높은 등급 검사
        Check("빈 대상 및 범위 제한", VerifyRangeAndExclusions); // 유효 대상 검사
        Check("단일 공격 주기와 효과 알림", VerifyCooldown); // 공격 주기 검사
        Check("첫 명중 중 둘째 사망", VerifyReentrantDeath); // 피해 재진입 검사
        Check("첫 대상 사망 후 둘째 명중", VerifyFirstDeath); // 첫 대상 사망 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 전체 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝

    private static void Check(string name, Action action) // 개별 시험
    { // 메서드 시작
        try // 시험 실행
        { // 시도 시작
            action(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        } // 시도 끝
        catch (Exception error) // 실패 처리
        { // 예외 시작
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 기록
        } // 예외 끝
    } // 메서드 끝

    private static (SnakeWeaponManager manager, SnakeWeaponSlot slot, WeaponAttributeCombatEffects effects, WeaponGradeEffectHooks hooks, WeaponTarget first, WeaponTarget second, WeaponTarget third) Create(int grade, int targetLimit = 2) // 시험 구성
    { // 메서드 시작
        WeaponTarget.Active.Clear(); // 이전 대상 초기화
        Time.time = 0f; // 공격 시각 초기화
        SnakeSegment segment = new SnakeSegment(); // 몸통 생성
        segment.transform.position = new Vector3(10f, 0f, 0f); // 머리에서 떨어진 몸통 위치
        SnakeBodyManager body = new SnakeBodyManager(); // 몸통 관리자
        body.BodySegments.Add(segment); // 몸통 등록
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 무기 관리자
        manager.Configure(body, null, null); // 몸통 슬롯 구성
        WeaponData weapon = new WeaponData { Range = 2f, AttackInterval = 0.8f, BaseDamage = 10f, GradeThreeMeleeTargets = targetLimit }; // 강철 검 시험값
        manager.TryEquipAt(0, weapon, grade); // 몸통에 검 장착
        WeaponAttributeCombatEffects effects = new WeaponAttributeCombatEffects(); // 속성 명중 기록
        WeaponGradeEffectHooks hooks = new WeaponGradeEffectHooks(); // 공격 이벤트 기록
        manager.BindAttributeCombatEffects(effects); // 속성 효과 연결
        manager.BindGradeEffectHooks(hooks); // 등급 효과 연결
        WeaponTarget first = Target(10.5f); // 첫 번째 근접 적
        WeaponTarget second = Target(11f); // 두 번째 근접 적
        WeaponTarget third = Target(11.5f); // 세 번째 근접 적
        return (manager, manager.Slots[0], effects, hooks, first, second, third); // 시험 객체 반환
    } // 메서드 끝

    private static WeaponTarget Target(float x) // 시험 대상 생성
    { // 메서드 시작
        WeaponTarget target = new WeaponTarget(); // 적 생성
        target.transform.position = new Vector3(x, 0f, 0f); // 적 위치 설정
        WeaponTarget.Active.Add(target); // 활성 대상 등록
        return target; // 대상 반환
    } // 메서드 끝

    private static bool Attack(SnakeWeaponManager manager, SnakeWeaponSlot slot) // 실제 근접 공격 호출
    { // 메서드 시작
        return (bool)typeof(SnakeWeaponManager).GetMethod("TryMeleeAttack", Private).Invoke(manager, new object[] { slot, slot.Weapon }); // 실제 공격 결과
    } // 메서드 끝

    private static void VerifyLowerGrades() // 낮은 등급 단일 공격
    { // 메서드 시작
        for (int grade = 1; grade <= 2; grade++) // 하위 등급 순회
        { // 반복 시작
            var fixture = Create(grade); // 등급별 구성
            Require(Attack(fixture.manager, fixture.slot), "근접 공격 실패"); // 공격 성공 확인
            Require(fixture.first.Hits == 1 && fixture.second.Hits == 0 && fixture.third.Hits == 0, "하위 등급이 여러 대상 공격"); // 단일 대상 확인
            Require(fixture.effects.HitCount == 1 && fixture.effects.LastOrigin.x == 10f, "몸통 원점 또는 속성 효과 오류"); // 원점과 효과 확인
        } // 반복 끝
    } // 메서드 끝

    private static void VerifyHigherGrades() // 높은 등급 최대 두 대상
    { // 메서드 시작
        for (int grade = 3; grade <= 5; grade++) // 상위 등급 순회
        { // 반복 시작
            var fixture = Create(grade); // 등급별 구성
            Require(Attack(fixture.manager, fixture.slot), "상위 등급 공격 실패"); // 공격 성공 확인
            Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.third.Hits == 0, "상위 등급 두 대상 제한 오류"); // 최근접 두 대상 확인
            Require(fixture.effects.HitCount == 2 && fixture.effects.LastOrigin.x == 10f, "두 대상 속성 경로 또는 몸통 원점 오류"); // 속성 및 원점 확인
        } // 반복 끝
        var other = Create(3, 1); // 고유 기능 없는 다른 근접 무기
        Require(Attack(other.manager, other.slot) && other.first.Hits == 1 && other.second.Hits == 0, "다른 근접 무기까지 일괄 확대"); // 무기 데이터 구분 확인
    } // 메서드 끝

    private static void VerifyRangeAndExclusions() // 범위와 생존 대상 확인
    { // 메서드 시작
        var fixture = Create(3); // 상위 등급 구성
        fixture.second.isActiveAndEnabled = false; // 둘째 대상 비활성
        fixture.third.transform.position = new Vector3(12.1f, 0f, 0f); // 셋째 대상 범위 밖
        Require(Attack(fixture.manager, fixture.slot), "단일 유효 대상 공격 실패"); // 한 대상 공격 확인
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 0 && fixture.third.Hits == 0, "무효 또는 범위 밖 대상 공격"); // 유효성 확인
        fixture.first.Health = 0f; // 마지막 대상 사망
        Require(!Attack(fixture.manager, fixture.slot), "유효 대상 없는 공격 성공"); // 빈 범위 확인
    } // 메서드 끝

    private static void VerifyCooldown() // 두 명중의 단일 공격 주기
    { // 메서드 시작
        var fixture = Create(3); // 상위 등급 구성
        MethodInfo tick = typeof(SnakeWeaponManager).GetMethod("TickAutoAttack", Private); // 실제 자동 공격 조회
        tick.Invoke(fixture.manager, null); // 첫 공격 처리
        tick.Invoke(fixture.manager, null); // 같은 시각 재시도
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.hooks.Notifications == 1, "두 명중 또는 쿨다운 이벤트 오류"); // 단일 공격 확인
        Time.time = 0.8f; // 다음 공격 시각
        tick.Invoke(fixture.manager, null); // 두 번째 공격 처리
        Require(fixture.first.Hits == 2 && fixture.second.Hits == 2 && fixture.hooks.Notifications == 2, "공격 주기 복구 오류"); // 정상 주기 확인
    } // 메서드 끝

    private static void VerifyReentrantDeath() // 첫 명중으로 둘째 대상 무효화
    { // 메서드 시작
        var fixture = Create(3); // 상위 등급 구성
        fixture.first.OnHit = () => fixture.second.Health = 0f; // 첫 명중 중 둘째 사망
        Require(Attack(fixture.manager, fixture.slot), "첫 대상 공격 실패"); // 공격 성공 확인
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 0 && fixture.third.Hits == 0, "명중 중 사망한 둘째 대상 공격"); // 재진입 유효성 확인
    } // 메서드 끝

    private static void VerifyFirstDeath() // 첫 대상 사망 시 후속 명중
    { // 메서드 시작
        var fixture = Create(3); // 상위 등급 구성
        fixture.first.Health = 5f; // 첫 대상 사망 직전 체력
        Require(Attack(fixture.manager, fixture.slot), "첫 대상 사망 공격 실패"); // 공격 성공 확인
        Require(!fixture.first.IsAlive && fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.third.Hits == 0, "첫 대상 사망 시 둘째 명중 오류"); // 후속 명중 확인
        Require(fixture.effects.HitCount == 2, "첫 대상 사망 시 속성 효과 누락"); // 속성 효과 횟수 확인
    } // 메서드 끝

    private static void Require(bool condition, string message) // 조건 검사
    { // 메서드 시작
        if (!condition) // 실패 조건
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 전달
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
