using System; // 기본 형식
using System.Reflection; // 비공개 선택 호출
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 합성 제어기
using ProjectEpsilon.UI; // 합성 화면

internal static class Program // 재료 선택 시험
{ // 클래스 시작
    private static int failures; // 실패 수
    private static readonly MethodInfo SelectMethod = typeof(SnakeWeaponMergeController).GetMethod("HandleCandidateSelected", BindingFlags.Instance | BindingFlags.NonPublic); // 선택 입력 조회

    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("두 번째 재료 강화 계승", VerifySecondInheritance); // 계승 대상 검증
        Check("네 번째 슬롯 접근", VerifyFourthSlot); // 전체 슬롯 접근 검증
        Check("스무 번째 슬롯 접근", VerifyTwentiethSlot); // 최대 슬롯 접근 검증
        Check("외부 슬롯 변경 시 취소", VerifyExternalSlotChange); // 외부 변경 검증
        Check("재료 선택 중 취소", VerifyCancellation); // 취소 보존 검증
        Check("시너지 미리보기 경계", VerifySynergyPreview); // 시너지 단계 검증
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    } // 메서드 끝

    private static void Check(string name, Action test) // 개별 시험
    { // 메서드 시작
        try // 시험 실행
        { // 시도 시작
            test(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        } // 시도 끝
        catch (Exception error) // 시험 실패
        { // 예외 시작
            failures++; // 실패 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 기록
        } // 예외 끝
    } // 메서드 끝

    private static (SnakeWeaponMergeController controller, SnakeWeaponManager weapons) Create(int count) // 시험 구성
    { // 메서드 시작
        WeaponData weapon = new WeaponData(); // 공통 무기
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자
        for (int index = 0; index < count; index++) // 슬롯 생성
        { // 반복 시작
            weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 2, DamageUpgrades = index }); // 서로 다른 강화 슬롯
        } // 반복 끝
        SnakeWeaponMergeController controller = new SnakeWeaponMergeController(); // 합성 제어기
        controller.Configure(weapons, null, null, new WeaponMergePanelController(), null, null, 0.7f); // 참조 연결
        Require(controller.OpenMergePanel(), "합성 화면 열기 실패"); // 열기 확인
        return (controller, weapons); // 시험 객체 반환
    } // 메서드 끝

    private static void Select(SnakeWeaponMergeController controller, int choice) // 버튼 입력
    { // 메서드 시작
        SelectMethod.Invoke(controller, new object[] { choice }); // 선택 처리 호출
    } // 메서드 끝

    private static void VerifySecondInheritance() // 두 번째 무기 계승
    { // 메서드 시작
        var fixture = Create(3); // 세 슬롯 준비
        bool? closedAsMerged = null; // 종료 결과 기록
        fixture.controller.MergeClosed += merged => closedAsMerged = merged; // 결과 이벤트 기록
        Select(fixture.controller, 2); // 첫 재료 다음 슬롯
        Select(fixture.controller, 1); // 첫 재료 선택
        Require(fixture.weapons.MergeCount == 0, "첫 재료 선택 후 합성 발생"); // 미확정 확인
        Select(fixture.controller, 2); // 둘째 재료 다음 슬롯
        Select(fixture.controller, 1); // 둘째 재료 선택
        Require(fixture.weapons.MergeCount == 0, "둘째 재료 선택 후 합성 발생"); // 계승 선택 확인
        Select(fixture.controller, 1); // 둘째 재료 계승 선택
        Require(fixture.weapons.MergeCount == 0, "강화 선택 전 합성 발생"); // 강화 선택 확인
        Select(fixture.controller, 0); // 공격력 강화 선택
        Require(fixture.weapons.MergeCount == 1, "합성 누락"); // 단일 합성 확인
        Require(closedAsMerged == true, "슬롯 변경 이벤트가 합성 성공을 취소 처리"); // 성공 알림 확인
        Require(fixture.weapons.FirstIndex == 2 && fixture.weapons.SecondIndex == 1, "둘째 재료 계승 위치 오류"); // 계승 순서 확인
        Require(fixture.weapons.ChosenUpgrade == WeaponMergeUpgrade.Damage, "강화 선택 오류"); // 강화 종류 확인
    } // 메서드 끝

    private static void VerifyFourthSlot() // 네 번째 재료 접근
    { // 메서드 시작
        var fixture = Create(4); // 네 슬롯 준비
        Select(fixture.controller, 2); // 둘째 슬롯 이동
        Select(fixture.controller, 2); // 셋째 슬롯 이동
        Select(fixture.controller, 2); // 넷째 슬롯 이동
        Select(fixture.controller, 1); // 첫 재료 확정
        Select(fixture.controller, 1); // 둘째 재료 확정
        Select(fixture.controller, 0); // 첫 재료 강화 계승
        Select(fixture.controller, 2); // 범위 강화 확정
        Require(fixture.weapons.MergeCount == 1, "합성 누락"); // 합성 결과 확인
        Require(fixture.weapons.FirstIndex == 3 && fixture.weapons.SecondIndex == 0, "네 번째 슬롯 선택 불가"); // 재료 위치 확인
        Require(fixture.weapons.ChosenUpgrade == WeaponMergeUpgrade.Range, "범위 강화 누락"); // 강화 종류 확인
    } // 메서드 끝

    private static void VerifyTwentiethSlot() // 최대 슬롯 접근
    { // 메서드 시작
        var fixture = Create(20); // 스무 슬롯 준비
        Select(fixture.controller, 0); // 마지막 슬롯 역순 이동
        Select(fixture.controller, 1); // 첫 재료 확정
        Select(fixture.controller, 1); // 둘째 재료 확정
        Select(fixture.controller, 0); // 첫 재료 계승
        Select(fixture.controller, 0); // 공격 강화 확정
        Require(fixture.weapons.FirstIndex == 19 && fixture.weapons.SecondIndex == 0, "스무 번째 슬롯 선택 불가"); // 최대 위치 확인
    } // 메서드 끝

    private static void VerifyExternalSlotChange() // 외부 무기 변경 처리
    { // 메서드 시작
        var fixture = Create(3); // 세 슬롯 준비
        bool? closedAsMerged = null; // 종료 결과 기록
        fixture.controller.MergeClosed += merged => closedAsMerged = merged; // 종료 이벤트 기록
        Select(fixture.controller, 1); // 첫 재료 확정
        fixture.weapons.NotifySlotsChanged(); // 외부 슬롯 변경
        Require(!fixture.controller.IsMergeOpen && closedAsMerged == false, "외부 슬롯 변경 시 취소 누락"); // 선택 취소 확인
        Require(fixture.weapons.MergeCount == 0, "외부 변경으로 합성 발생"); // 재료 보존 확인
    } // 메서드 끝

    private static void VerifySynergyPreview() // 합성 후 시너지 예측
    { // 메서드 시작
        WeaponMergePanelController.PreviewStage = -1; // 이전 예측 초기화
        var fixture = Create(4); // 네 속성 무기 준비
        Require(WeaponMergePanelController.PreviewCount == 4 && WeaponMergePanelController.PreviewResultCount == 3, "속성 수 미리보기 오류"); // 합성 전후 개수 확인
        Require(WeaponMergePanelController.PreviewStage == 4 && WeaponMergePanelController.PreviewResultStage == 2, "시너지 단계 미리보기 오류"); // 단계 경계 확인
        Select(fixture.controller, 1); // 첫 재료 확정
        Select(fixture.controller, 1); // 둘째 재료 확정
        Require(WeaponMergePanelController.PreviewStage == 4 && WeaponMergePanelController.PreviewResultStage == 2, "계승 화면 미리보기 오류"); // 계승 화면 확인
        Select(fixture.controller, 0); // 첫 재료 계승
        Require(WeaponMergePanelController.PreviewStage == 4 && WeaponMergePanelController.PreviewResultStage == 2, "강화 화면 미리보기 오류"); // 강화 화면 확인
        fixture.controller.CloseMergePanel(); // 합성 취소
        Require(fixture.weapons.MergeCount == 0 && fixture.weapons.Slots.Count == 4, "미리보기 취소로 무기 변경"); // 취소 보존 확인
        fixture = Create(2); // 두 속성 무기 준비
        Require(WeaponMergePanelController.PreviewStage == 2 && WeaponMergePanelController.PreviewResultStage == 0, "시너지 비활성 경계 오류"); // 비활성 경계 확인
    } // 메서드 끝

    private static void VerifyCancellation() // 단계별 취소 보존
    { // 메서드 시작
        for (int step = 0; step < 4; step++) // 취소 단계 순회
        { // 반복 시작
            var fixture = Create(3); // 무기 준비
            for (int index = 0; index < step; index++) // 선택 단계 진행
            { // 반복 시작
                Select(fixture.controller, index < 2 ? 1 : 0); // 재료 및 계승 선택
            } // 반복 끝
            fixture.controller.CloseMergePanel(); // 합성 취소
            Require(fixture.weapons.MergeCount == 0, $"단계 {step} 취소 전 합성 발생"); // 합성 방지 확인
            Require(fixture.weapons.Slots.Count == 3 && fixture.weapons.Slots[2].DamageUpgrades == 2, "취소 후 무기 변화"); // 무기 강화 보존 확인
            Require(!fixture.controller.IsMergeOpen, "취소 후 화면 유지"); // 화면 종료 확인
        } // 반복 끝
    } // 메서드 끝

    private static void Require(bool condition, string message) // 조건 확인
    { // 메서드 시작
        if (!condition) // 실패 조건
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 전달
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
