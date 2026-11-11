using System; // 기본 형식
using System.Reflection; // 합성 API 검색
using ProjectEpsilon.Combat; // 무기 및 시너지
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Player; // 몸통 구성

internal static class Program // 미장착 보상 합성 시험
{
    private static int failures; // 실패 횟수
    private static readonly MethodInfo MergeMethod = typeof(SnakeWeaponManager).GetMethod("TryMergeReward", new[] { typeof(int), typeof(WeaponData), typeof(int), typeof(bool), typeof(WeaponMergeUpgrade) }); // 보상 합성 API 조회

    private static int Main() // 시험 시작
    {
        Check("장착 무기 강화 계승", () => VerifyInheritance(true)); // 장착 강화 계승 검사
        Check("보상 무기 강화 계승", () => VerifyInheritance(false)); // 보상 강화 계승 검사
        Check("빈 슬롯 없는 합성", VerifyFullSlots); // 가득 찬 슬롯 검사
        Check("무기와 등급 불일치 거절", VerifyInvalid); // 유효성 검사
        Check("기존 장착 무기 합성 유지", VerifyExistingMerge); // 기존 합성 회귀 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 상태
    }

    private static void Check(string name, Action action) // 개별 시험 실행
    {
        try // 시험 시도
        {
            action(); // 시험 실행
            Console.WriteLine($"PASS: {name}"); // 성공 출력
        }
        catch (Exception error) // 실패 포착
        {
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 출력
        }
    }

    private static (SnakeWeaponManager manager, WeaponAttributeSynergyManager synergy, WeaponData weapon, WeaponData other) Create() // 시험 객체 구성
    {
        SnakeBodyManager body = new SnakeBodyManager(); // 몸통 관리자
        for (int index = 0; index < 3; index++) // 몸통 세 칸 생성
        {
            body.BodySegments.Add(new SnakeSegment()); // 몸통 추가
        }
        SnakeWeaponManager manager = new SnakeWeaponManager(); // 실제 무기 관리자
        manager.Configure(body, null, null); // 슬롯 생성
        WeaponData weapon = new WeaponData { Attribute = WeaponAttribute.Fire }; // 합성 대상 무기
        WeaponData other = new WeaponData { Attribute = WeaponAttribute.Fire }; // 다른 화염 무기
        manager.TryEquipAt(0, other, 1); // 첫 슬롯에 다른 무기 장착
        manager.TryEquipAt(2, weapon, 2); // 끝 슬롯에 재료 장착
        manager.Slots[2].Equip(weapon, 2, 2, 3, 1); // 재료 강화 구성
        WeaponAttributeSynergyManager synergy = new WeaponAttributeSynergyManager(); // 실제 시너지 관리자
        synergy.Configure(manager); // 슬롯 변경 구독
        return (manager, synergy, weapon, other); // 시험 객체 반환
    }

    private static bool Merge(SnakeWeaponManager manager, int slot, WeaponData reward, int grade, bool inheritEquipped, WeaponMergeUpgrade upgrade) // 보상 합성 호출
    {
        Require(MergeMethod != null, "보상 합성 API 미구현"); // 신규 API 확인
        return (bool)MergeMethod.Invoke(manager, new object[] { slot, reward, grade, inheritEquipped, upgrade }); // 실제 관리자 호출
    }

    private static void VerifyInheritance(bool inheritEquipped) // 강화 계승 검사
    {
        var fixture = Create(); // 시험 무기 구성
        int notifications = 0; // 변경 알림 횟수
        fixture.manager.SlotsChanged += () => notifications++; // 슬롯 변경 기록
        Require(fixture.synergy.GetCount(WeaponAttribute.Fire) == 2 && fixture.synergy.GetStage(WeaponAttribute.Fire) == 2, "초기 시너지 오류"); // 초기 집계 확인
        Require(Merge(fixture.manager, 2, fixture.weapon, 2, inheritEquipped, WeaponMergeUpgrade.Range), "유효 보상 합성 실패"); // 합성 결과 확인
        Require(fixture.manager.OccupiedSlotCount == 2 && fixture.manager.SlotCount == 3, "장착 무기 또는 슬롯 수 변경"); // 무기 수 보존
        Require(fixture.manager.Slots[0].Weapon == fixture.weapon && fixture.manager.Slots[0].Grade == 3, "결과 무기 위치 또는 등급 오류"); // 머리 뒤 결과 확인
        Require(fixture.manager.Slots[1].Weapon == fixture.other, "다른 무기 손실"); // 나머지 무기 보존
        Require(fixture.manager.Slots[0].DamageUpgrades == (inheritEquipped ? 2 : 0), "공격 강화 계승 오류"); // 공격 강화 확인
        Require(fixture.manager.Slots[0].SpeedUpgrades == (inheritEquipped ? 3 : 0), "속도 강화 계승 오류"); // 속도 강화 확인
        Require(fixture.manager.Slots[0].RangeUpgrades == (inheritEquipped ? 2 : 1), "범위 강화 계승 오류"); // 계승 및 신규 강화 확인
        Require(notifications == 1 && fixture.synergy.GetCount(WeaponAttribute.Fire) == 2 && fixture.synergy.GetStage(WeaponAttribute.Fire) == 2, "시너지 유지 또는 변경 알림 오류"); // 즉시 집계 확인
    }

    private static void VerifyFullSlots() // 빈 슬롯 없는 경우
    {
        var fixture = Create(); // 시험 무기 구성
        fixture.manager.TryEquipAt(1, fixture.other, 1); // 마지막 빈 슬롯 채움
        Require(Merge(fixture.manager, 2, fixture.weapon, 2, true, WeaponMergeUpgrade.Damage), "가득 찬 슬롯의 합성 실패"); // 합성 결과 확인
        Require(fixture.manager.OccupiedSlotCount == 3 && fixture.manager.SlotCount == 3, "가득 찬 슬롯의 무기 손실"); // 무기 수 보존
        Require(fixture.synergy.GetCount(WeaponAttribute.Fire) == 3 && fixture.synergy.GetStage(WeaponAttribute.Fire) == 2, "가득 찬 슬롯의 시너지 오류"); // 속성 유지 확인
    }

    private static void VerifyExistingMerge() // 기존 두 장착 무기 합성
    {
        var fixture = Create(); // 시험 무기 구성
        fixture.manager.TryEquipAt(0, fixture.weapon, 2); // 같은 무기 및 등급 준비
        int notifications = 0; // 슬롯 변경 횟수
        fixture.manager.SlotsChanged += () => notifications++; // 변경 이벤트 기록
        Require(fixture.manager.TryMergeSlots(2, 0, WeaponMergeUpgrade.Damage), "기존 합성 실패"); // 기존 합성 호출
        Require(fixture.manager.OccupiedSlotCount == 1 && fixture.manager.Slots[0].Grade == 3, "기존 합성 수량 또는 등급 오류"); // 합성 결과 확인
        Require(fixture.manager.Slots[0].DamageUpgrades == 3 && fixture.manager.Slots[0].SpeedUpgrades == 3 && fixture.manager.Slots[0].RangeUpgrades == 1, "기존 합성 강화 계승 오류"); // 기존 강화 확인
        Require(notifications == 1 && fixture.synergy.GetCount(WeaponAttribute.Fire) == 1 && fixture.synergy.GetStage(WeaponAttribute.Fire) == 0, "기존 합성 시너지 오류"); // 이벤트와 시너지 확인
    }

    private static void VerifyInvalid() // 잘못된 재료 차단
    {
        var fixture = Create(); // 시험 무기 구성
        int notifications = 0; // 변경 알림 횟수
        fixture.manager.SlotsChanged += () => notifications++; // 변경 알림 기록
        Require(!Merge(fixture.manager, 2, null, 2, true, WeaponMergeUpgrade.Damage), "빈 보상 허용"); // 보상 존재 확인
        Require(!Merge(fixture.manager, 2, fixture.other, 2, true, WeaponMergeUpgrade.Damage), "다른 무기 허용"); // 무기 일치 확인
        Require(!Merge(fixture.manager, 2, fixture.weapon, 1, true, WeaponMergeUpgrade.Damage), "다른 등급 허용"); // 등급 일치 확인
        Require(!Merge(fixture.manager, 2, fixture.weapon, 0, true, WeaponMergeUpgrade.Damage), "잘못된 보상 등급 허용"); // 보상 등급 확인
        Require(!Merge(fixture.manager, 1, fixture.weapon, 2, true, WeaponMergeUpgrade.Damage), "빈 슬롯 허용"); // 빈 슬롯 차단
        Require(!Merge(fixture.manager, -1, fixture.weapon, 2, true, WeaponMergeUpgrade.Damage), "잘못된 슬롯 허용"); // 슬롯 범위 확인
        Require(!Merge(fixture.manager, 2, fixture.weapon, 2, true, WeaponMergeUpgrade.None), "강화 없는 합성 허용"); // 공통 강화 필수 확인
        Require(!Merge(fixture.manager, 2, fixture.weapon, 2, true, (WeaponMergeUpgrade)99), "알 수 없는 강화 허용"); // 강화 범위 확인
        fixture.weapon.MaxGrade = 2; // 무기별 최대 등급 설정
        Require(!Merge(fixture.manager, 2, fixture.weapon, 2, true, WeaponMergeUpgrade.Damage), "무기별 최대 등급 합성 허용"); // 무기 최대 등급 차단
        fixture.weapon.MaxGrade = 5; // 최대 등급 복구
        fixture.manager.Slots[2].Equip(fixture.weapon, 5); // 최고 등급 구성
        Require(!Merge(fixture.manager, 2, fixture.weapon, 5, true, WeaponMergeUpgrade.Damage), "최고 등급 합성 허용"); // 최고 등급 차단
        Require(notifications == 0 && fixture.manager.OccupiedSlotCount == 2 && fixture.synergy.GetCount(WeaponAttribute.Fire) == 2, "실패 시 상태 변경"); // 실패 시 무기 및 시너지 보존
    }

    private static void Require(bool condition, string message) // 조건 검사
    {
        if (!condition) // 실패 조건 확인
        {
            throw new InvalidOperationException(message); // 실패 전달
        }
    }
}
