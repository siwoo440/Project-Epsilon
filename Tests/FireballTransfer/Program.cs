using System; // 기본 형식
using System.Reflection; // 충돌 콜백 호출
using ProjectEpsilon.Combat; // 투사체 및 표적
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using UnityEngine; // 충돌과 좌표

internal static class Program // 화염탄 등급 전달 시험
{
    private static int failures; // 실패 횟수
    private static readonly MethodInfo Trigger = typeof(StraightProjectile).GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic); // 실제 충돌 콜백

    private static int Main() // 시험 시작
    {
        Check("하위 등급과 다른 무기", VerifyLowerAndOtherWeapons); // 기존 공격 회귀
        Check("상위 등급 추가 한 명과 스냅샷", VerifyTransferAndSnapshot); // 등급 전달 검증
        Check("후보 부재와 범위 제한", VerifyNoCandidate); // 후보 조건 검증
        Check("최초 사망 중 둘째 무효화", VerifyTargetInvalidation); // 사망 이벤트 경계
        Check("무효화 후 직접 충돌 복구", VerifyInvalidatedDirectHit); // 실제 미명중 대상 검사
        Check("관통과 전달 대상 중복 방지", VerifyPenetration); // 관통 상호작용 검사
        Check("동기 재진입 명중 한도", VerifyReentrantHit); // 재진입 검사
        Check("효과 관리자 부재 대체 피해", VerifyFallback); // 기존 대체 경로
        Check("발사 후 데이터 변경 보호", VerifyDataSnapshot); // 임시 수치 스냅샷 검사
        Check("명중 중 재설정 보호", VerifyReconfigureDuringHit); // 투사체 재사용 경계
        Check("명중 중 비활성 보호", VerifyDisableDuringHit); // 비활성 경계
        Check("전달 명중 중 재설정 보호", VerifyReconfigureDuringTransfer); // 전달 콜백 경계
        Check("비활성 투사체 충돌 차단", VerifyInactiveTrigger); // 비활성 물리 콜백 경계
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 전체 결과
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

    private static (StraightProjectile projectile, WeaponData weapon, WeaponAttributeCombatEffects effects, WeaponTarget first, WeaponTarget second, WeaponTarget third) Create(int grade, float radius = 2f, float multiplier = 0.5f, int hitLimit = 1, bool useEffects = true) // 시험 구성
    {
        WeaponTarget.Active.Clear(); // 이전 표적 초기화
        GameManager.Instance = new GameManager(); // 진행 중 게임
        WeaponData weapon = new WeaponData { GradeThreeTransferRange = radius, GradeThreeTransferDamageMultiplier = multiplier, ProjectileHitLimit = hitLimit }; // 임시 무기 설정
        WeaponAttributeCombatEffects effects = useEffects ? new WeaponAttributeCombatEffects() : null; // 명중 효과 기록
        WeaponAttributeAttackSnapshot snapshot = new WeaponAttributeAttackSnapshot(weapon, WeaponAttribute.Fire, 4, 4, grade, new Vector3(100f, 2f, 0f), 14f); // 몸통 원점과 등급 피해
        StraightProjectile projectile = new StraightProjectile(); // 실제 투사체
        projectile.Configure(Vector2.up, snapshot, effects, 8f, 3f, new Sprite()); // 발사 정보 설정
        WeaponTarget first = Target(10f); // 최초 명중 대상
        WeaponTarget second = Target(10.5f); // 가장 가까운 전달 대상
        WeaponTarget third = Target(11f); // 추가 후보
        return (projectile, weapon, effects, first, second, third); // 시험 객체 반환
    }

    private static WeaponTarget Target(float x) // 시험 표적 생성
    {
        WeaponTarget target = new WeaponTarget(); // 표적 생성
        target.transform.position = new Vector3(x, 0f, 0f); // 표적 위치 저장
        WeaponTarget.Active.Add(target); // 활성 목록 등록
        return target; // 표적 반환
    }

    private static void Hit(StraightProjectile projectile, WeaponTarget target) // 실제 투사체 충돌
    {
        Trigger.Invoke(projectile, new object[] { new Collider2D { Target = target } }); // 실제 충돌 콜백 호출
    }

    private static void VerifyLowerAndOtherWeapons() // 기본 등급 및 다른 무기 유지
    {
        for (int grade = 1; grade <= 2; grade++) // 하위 등급 순회
        {
            var fixture = Create(grade); // 등급별 시험 구성
            Hit(fixture.projectile, fixture.first); // 최초 명중
            Require(fixture.first.Hits == 1 && fixture.second.Hits == 0 && fixture.third.Hits == 0, "하위 등급이 추가 대상 공격"); // 단일 명중 확인
            Require(fixture.first.Burns == 1 && fixture.projectile.gameObject.PendingDestroy, "기존 화상 또는 투사체 종료 오류"); // 기본 경로 확인
        }
        var other = Create(3, 0f); // 전달 비활성 다른 무기
        Hit(other.projectile, other.first); // 최초 명중
        Require(other.first.Hits == 1 && other.second.Hits == 0, "다른 무기의 추가 전달 발생"); // 무기별 설정 확인
    }

    private static void VerifyTransferAndSnapshot() // 상위 등급 추가 전달과 피해 정보
    {
        for (int grade = 3; grade <= 5; grade++) // 상위 등급 순회
        {
            var fixture = Create(grade); // 등급별 시험 구성
            Hit(fixture.projectile, fixture.first); // 최초 명중
            Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.third.Hits == 0, "최근접 추가 대상 한 명 제한 오류"); // 대상 수 확인
            Require(Math.Abs(fixture.first.Health - 86f) < 0.001f && Math.Abs(fixture.second.Health - 93f) < 0.001f, "기본 등급 피해 또는 전달 배율 오류"); // 피해량 확인
            Require(fixture.first.Burns == 1 && fixture.second.Burns == 1 && fixture.effects.Hits.Count == 2, "기존 화상 명중 경로 누락"); // 화상 경로 확인
            WeaponAttributeAttackSnapshot transfer = fixture.effects.Hits[1]; // 전달 명중 정보
            Require(transfer.Weapon == fixture.weapon && transfer.Grade == grade && transfer.AttributeCount == 4 && transfer.SynergyStage == 4 && transfer.Origin.x == 100f && transfer.Origin.y == 2f, "발사 원점 또는 등급·시너지 변경"); // 스냅샷 보존 확인
            Require(fixture.projectile.gameObject.PendingDestroy, "한도 도달 투사체 미파괴"); // 투사체 종료 확인
        }
    }

    private static void VerifyNoCandidate() // 후보 부재 및 거리 경계
    {
        var fixture = Create(3); // 상위 등급 구성
        fixture.second.isActiveAndEnabled = false; // 가까운 후보 비활성
        fixture.third.transform.position = new Vector3(12.1f, 0f, 0f); // 다른 후보 범위 밖
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 0 && fixture.third.Hits == 0, "비활성 또는 범위 밖 대상 전달"); // 후보 제한 확인
        fixture = Create(3); // 동일 무기 재구성
        WeaponTarget.Active.Remove(fixture.second); // 둘째 후보 제거
        WeaponTarget.Active.Remove(fixture.third); // 셋째 후보 제거
        Hit(fixture.projectile, fixture.first); // 유일 대상 명중
        Require(fixture.first.Hits == 1 && fixture.effects.Hits.Count == 1, "최초 대상 재선택 또는 중복 피해"); // 최초 대상 제외 확인
    }

    private static void VerifyTargetInvalidation() // 최초 사망 중 전달 대상 소실
    {
        var fixture = Create(3); // 상위 등급 구성
        fixture.first.Health = 5f; // 최초 적 사망 체력
        fixture.first.OnHit = () => fixture.second.isActiveAndEnabled = false; // 사망 중 둘째 대상 비활성
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(!fixture.first.IsAlive && fixture.second.Hits == 0 && fixture.third.Hits == 0, "무효화된 전달 대상 또는 대체 대상 공격"); // 재진입 유효성 확인
        fixture = Create(3); // 사망 경계 재구성
        fixture.first.OnHit = () => fixture.second.Health = 0f; // 최초 명중 중 둘째 사망
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(fixture.second.Hits == 0 && fixture.third.Hits == 0, "사망한 전달 대상 또는 대체 대상 공격"); // 사망 대상 제외 확인
    }

    private static void VerifyInvalidatedDirectHit() // 전달 전 비활성된 대상의 후속 직접 명중
    {
        var fixture = Create(3, 2f, 1f, 2); // 두 번 관통 허용
        fixture.first.OnHit = () => fixture.second.isActiveAndEnabled = false; // 최초 명중 중 전달 대상 비활성
        Hit(fixture.projectile, fixture.first); // 최초 명중과 전달 취소
        Require(fixture.second.Hits == 0, "비활성 대상에 전달 피해"); // 전달 취소 확인
        fixture.second.isActiveAndEnabled = true; // 대상 재활성
        Hit(fixture.projectile, fixture.second); // 같은 투사체의 후속 직접 명중
        Require(fixture.second.Hits == 1 && fixture.projectile.gameObject.PendingDestroy, "실제 명중하지 않은 대상의 직접 충돌 차단"); // 미명중 대상 재진입 확인
    }

    private static void VerifyPenetration() // 관통 중 전달 대상 재명중 방지
    {
        var fixture = Create(3, 1f, 1f, 2); // 관통 두 번 허용
        fixture.third.transform.position = new Vector3(14f, 0f, 0f); // 다른 최초 명중 위치
        WeaponTarget fourth = Target(14.5f); // 다른 명중의 주변 후보
        Hit(fixture.projectile, fixture.first); // 첫 명중과 전달
        Hit(fixture.projectile, fixture.second); // 이미 전달된 대상과 충돌
        Require(!fixture.projectile.gameObject.PendingDestroy && fixture.second.Hits == 1, "전달 대상 재명중 또는 관통 횟수 소비"); // 중복 명중 차단
        Hit(fixture.projectile, fixture.third); // 둘째 관통 명중
        Require(fixture.third.Hits == 1 && fourth.Hits == 0 && fixture.projectile.gameObject.PendingDestroy, "관통 중 전달 재발동 또는 파괴 오류"); // 투사체당 한 번 전달 확인
    }

    private static void VerifyReentrantHit() // 최초 명중 중 동일 투사체 재진입
    {
        var fixture = Create(3); // 단일 관통 시험 구성
        fixture.first.OnHit = () => Hit(fixture.projectile, fixture.third); // 명중 도중 다른 충돌 재진입
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.third.Hits == 0, "명중 한도 확정 전 재진입 공격"); // 조기 명중 한도 확인
    }

    private static void VerifyDataSnapshot() // 발사 후 에셋 수정 경계
    {
        var fixture = Create(3, 2f, 0.5f); // 전달 반경과 피해 설정
        fixture.weapon.GradeThreeTransferRange = 0f; // 발사 후 에셋 반경 변경
        fixture.weapon.GradeThreeTransferDamageMultiplier = 2f; // 발사 후 에셋 배율 변경
        Hit(fixture.projectile, fixture.first); // 기존 발사체 명중
        Require(fixture.second.Hits == 1 && Math.Abs(fixture.second.Health - 93f) < 0.001f, "발사 후 에셋 변경이 기존 투사체에 반영"); // 발사 시점 수치 확인
    }

    private static void VerifyReconfigureDuringHit() // 최초 명중 중 투사체 재설정
    {
        var fixture = Create(3); // 상위 등급 투사체 구성
        WeaponAttributeAttackSnapshot replacement = new WeaponAttributeAttackSnapshot(fixture.weapon, WeaponAttribute.Fire, 1, 0, 1, new Vector3(200f, 0f, 0f), 3f); // 새 발사 정보
        fixture.first.OnHit = () => fixture.projectile.Configure(Vector2.up, replacement, fixture.effects, 8f, 3f, new Sprite()); // 최초 명중 중 재설정
        Hit(fixture.projectile, fixture.first); // 이전 발사체 명중
        Require(fixture.second.Hits == 0 && !fixture.projectile.gameObject.PendingDestroy, "재설정 뒤 이전 발사의 전달 또는 파괴 실행"); // 이전 발사 처리 종료 확인
        Hit(fixture.projectile, fixture.second); // 새 발사체 직접 명중
        Require(fixture.second.Hits == 1 && Math.Abs(fixture.second.Health - 97f) < 0.001f && fixture.projectile.gameObject.PendingDestroy, "재설정된 발사체 동작 오류"); // 새 발사 상태 확인
    }

    private static void VerifyDisableDuringHit() // 최초 명중 중 투사체 비활성
    {
        var fixture = Create(3); // 상위 등급 투사체 구성
        fixture.first.OnHit = () => fixture.projectile.isActiveAndEnabled = false; // 최초 명중 중 비활성
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(fixture.second.Hits == 0 && !fixture.projectile.gameObject.PendingDestroy, "비활성 투사체의 후속 전달 또는 파괴 실행"); // 수명주기 변경 확인
    }

    private static void VerifyReconfigureDuringTransfer() // 추가 대상 명중 중 재설정
    {
        var fixture = Create(3); // 상위 등급 투사체 구성
        WeaponAttributeAttackSnapshot replacement = new WeaponAttributeAttackSnapshot(fixture.weapon, WeaponAttribute.Fire, 1, 0, 1, new Vector3(200f, 0f, 0f), 3f); // 새 발사 정보
        fixture.second.OnHit = () => fixture.projectile.Configure(Vector2.up, replacement, fixture.effects, 8f, 3f, new Sprite()); // 전달 명중 중 재설정
        Hit(fixture.projectile, fixture.first); // 최초 및 추가 명중
        Require(fixture.second.Hits == 1 && !fixture.projectile.gameObject.PendingDestroy, "전달 중 재설정 후 이전 발사의 파괴 실행"); // 이전 발사 종료 확인
        Hit(fixture.projectile, fixture.third); // 새 발사체 직접 명중
        Require(fixture.third.Hits == 1 && fixture.projectile.gameObject.PendingDestroy, "전달 중 재설정된 발사체 동작 오류"); // 새 발사 상태 확인
    }

    private static void VerifyInactiveTrigger() // 비활성 구성 요소의 물리 콜백
    {
        var fixture = Create(3); // 상위 등급 구성
        fixture.projectile.isActiveAndEnabled = false; // 투사체 구성 비활성
        Hit(fixture.projectile, fixture.first); // 비활성 상태의 충돌 콜백
        Require(fixture.first.Hits == 0 && fixture.second.Hits == 0 && !fixture.projectile.gameObject.PendingDestroy, "비활성 투사체가 피해 또는 파괴 실행"); // 비활성 상태 보호 확인
        fixture.projectile.isActiveAndEnabled = true; // 투사체 재활성
        Hit(fixture.projectile, fixture.first); // 재활성 뒤 같은 대상 명중
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.projectile.gameObject.PendingDestroy, "비활성 콜백이 재활성 후 명중 횟수 소비"); // 명중 횟수 보존 확인
    }

    private static void VerifyFallback() // 속성 관리자 없는 기본 피해
    {
        var fixture = Create(3, 2f, 1f, 1, false); // 효과 관리자 없는 구성
        Hit(fixture.projectile, fixture.first); // 최초 명중
        Require(fixture.first.Hits == 1 && fixture.second.Hits == 1 && fixture.third.Hits == 0, "대체 피해 경로의 전달 실패"); // 직접 피해 대체 확인
    }

    private static void Require(bool condition, string message) // 조건 확인
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 보고
        }
    }
}
