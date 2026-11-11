using System; // 기본 형식
using System.Reflection; // 비공개 선택 호출
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 합성 제어기
using ProjectEpsilon.UI; // 합성 화면
using UnityEngine; // 프레임 상태
using UnityEngine.InputSystem; // 키 입력

internal static class Program // 재료 선택 시험
{
    private static int failures; // 실패 수
    private static readonly MethodInfo SelectMethod = typeof(SnakeWeaponMergeController).GetMethod("HandleCandidateSelected", BindingFlags.Instance | BindingFlags.NonPublic); // 선택 입력 조회

    private static int Main() // 시험 시작
    {
        Check("두 번째 재료 강화 계승", VerifySecondInheritance); // 계승 대상 검증
        Check("네 번째 슬롯 접근", VerifyFourthSlot); // 전체 슬롯 접근 검증
        Check("스무 번째 슬롯 접근", VerifyTwentiethSlot); // 최대 슬롯 접근 검증
        Check("외부 슬롯 변경 시 취소", VerifyExternalSlotChange); // 외부 변경 검증
        Check("재료 선택 중 취소", VerifyCancellation); // 취소 보존 검증
        Check("시너지 미리보기 경계", VerifySynergyPreview); // 시너지 단계 검증
        Check("미장착 보상 합성", VerifyRewardMerge); // 보상 재료 검사
        Check("합성 제어기 비활성 화면 정리", VerifyMergeDisable); // 비활성 화면 검사
        Check("합성 진입 프레임 입력 소비", VerifyOpeningFrameInput); // 같은 프레임 입력 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 코드
    }

    private static void Check(string name, Action test) // 개별 시험
    {
        try // 시험 실행
        {
            test(); // 시험 호출
            Console.WriteLine($"PASS: {name}"); // 성공 기록
        }
        catch (Exception error) // 시험 실패
        {
            failures++; // 실패 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 기록
        }
    }

    private static (SnakeWeaponMergeController controller, SnakeWeaponManager weapons) Create(int count) // 시험 구성
    {
        WeaponData weapon = new WeaponData(); // 공통 무기
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자
        for (int index = 0; index < count; index++) // 슬롯 생성
        {
            weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 2, DamageUpgrades = index }); // 서로 다른 강화 슬롯
        }
        SnakeWeaponMergeController controller = new SnakeWeaponMergeController(); // 합성 제어기
        controller.Configure(weapons, null, null, new WeaponMergePanelController(), null, null, 0.7f); // 참조 연결
        Require(controller.OpenMergePanel(), "합성 화면 열기 실패"); // 열기 확인
        return (controller, weapons); // 시험 객체 반환
    }

    private static void Select(SnakeWeaponMergeController controller, int choice) // 버튼 입력
    {
        SelectMethod.Invoke(controller, new object[] { choice }); // 선택 처리 호출
    }

    private static void VerifySecondInheritance() // 두 번째 무기 계승
    {
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
    }

    private static void VerifyFourthSlot() // 네 번째 재료 접근
    {
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
    }

    private static void VerifyTwentiethSlot() // 최대 슬롯 접근
    {
        var fixture = Create(20); // 스무 슬롯 준비
        Select(fixture.controller, 0); // 마지막 슬롯 역순 이동
        Select(fixture.controller, 1); // 첫 재료 확정
        Select(fixture.controller, 1); // 둘째 재료 확정
        Select(fixture.controller, 0); // 첫 재료 계승
        Select(fixture.controller, 0); // 공격 강화 확정
        Require(fixture.weapons.FirstIndex == 19 && fixture.weapons.SecondIndex == 0, "스무 번째 슬롯 선택 불가"); // 최대 위치 확인
    }

    private static void VerifyExternalSlotChange() // 외부 무기 변경 처리
    {
        var fixture = Create(3); // 세 슬롯 준비
        bool? closedAsMerged = null; // 종료 결과 기록
        fixture.controller.MergeClosed += merged => closedAsMerged = merged; // 종료 이벤트 기록
        Select(fixture.controller, 1); // 첫 재료 확정
        fixture.weapons.NotifySlotsChanged(); // 외부 슬롯 변경
        Require(!fixture.controller.IsMergeOpen && closedAsMerged == false, "외부 슬롯 변경 시 취소 누락"); // 선택 취소 확인
        Require(fixture.weapons.MergeCount == 0, "외부 변경으로 합성 발생"); // 재료 보존 확인
    }

    private static void VerifySynergyPreview() // 합성 후 시너지 예측
    {
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
    }

    private static void VerifyRewardMerge() // 미장착 보상과 장착 무기 합성
    {
        GameManager.Instance = new GameManager(); // 진행 중 게임 생성
        WeaponData weapon = new WeaponData(); // 공통 무기 생성
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        for (int index = 0; index < 3; index++) // 장착 슬롯 생성
        {
            weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 1 }); // 같은 무기 장착
        }
        SnakeExperience experience = new SnakeExperience { IsLevelUpPending = true }; // 레벨업 보상 대기
        SnakeLevelUpController level = new SnakeLevelUpController { IsPresentingLevelUp = true, IsAwaitingRewardMerge = true }; // 보상 합성 대기
        SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 실제 합성 제어기
        merge.Configure(weapons, null, null, new WeaponMergePanelController(), experience, level, 0.7f); // 구성 연결
        bool? closed = null; // 합성 결과 기록
        merge.MergeClosed += merged => closed = merged; // 결과 이벤트 구독
        Require(merge.OpenRewardMerge(new WeaponRewardCandidate(weapon, 1)), "보상 합성 열기 실패"); // 합성 화면 확인
        Require(WeaponMergePanelController.PreviewCount == 3 && WeaponMergePanelController.PreviewResultCount == 3, "보상 합성 시너지 예측 오류"); // 장착 수 유지 예측
        Select(merge, 2); // 두 번째 장착 재료
        Select(merge, 2); // 세 번째 장착 재료
        Select(merge, 1); // 세 번째 재료 확정
        Select(merge, 1); // 장착 무기 강화 계승
        Select(merge, 0); // 공격력 공통 강화
        Require(weapons.RewardMergeCount == 1 && weapons.RewardSlot == 2 && weapons.InheritEquipped, "보상 재료 또는 강화 계승 오류"); // 장착 재료 확인
        Require(weapons.RewardUpgrade == WeaponMergeUpgrade.Damage && closed == true, "보상 강화 또는 성공 알림 오류"); // 결과 확인
        GameManager.Instance = null; // 현재 게임 해제
    }

    private static void VerifyOpeningFrameInput() // 보상 합성 개시 프레임 입력
    {
        GameManager.Instance = new GameManager(); // 진행 중 게임 생성
        WeaponData weapon = new WeaponData(); // 공통 무기 생성
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 1 }); // 첫 장착 재료
        weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 1 }); // 둘째 장착 재료
        SnakeLevelUpController level = new SnakeLevelUpController { IsAwaitingRewardMerge = true }; // 보상 합성 대기
        SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 실제 합성 제어기
        merge.Configure(weapons, null, null, new WeaponMergePanelController(), new SnakeExperience { IsLevelUpPending = true }, level, 0.7f); // 합성 구성 연결
        Time.frameCount = 50; // 합성 진입 프레임
        Keyboard.current = new Keyboard(); // 가상 키보드 생성
        Keyboard.current.digit2Key.wasPressedThisFrame = true; // 보상 합성 선택 키
        Require(merge.OpenRewardMerge(new WeaponRewardCandidate(weapon, 1)), "보상 합성 열기 실패"); // 합성 진입 확인
        typeof(SnakeWeaponMergeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(merge, null); // 같은 프레임 입력 처리
        int stage = (int)typeof(SnakeWeaponMergeController).GetField("selectionStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(merge); // 선택 단계 조회
        Require(stage == 1, "보상 합성 선택 키가 재료까지 자동 확정"); // 같은 프레임 입력 차단
        Time.frameCount++; // 다음 프레임 진행
        typeof(SnakeWeaponMergeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(merge, null); // 다음 프레임 입력 처리
        stage = (int)typeof(SnakeWeaponMergeController).GetField("selectionStage", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(merge); // 선택 단계 재조회
        Require(stage == 2, "다음 프레임 재료 선택 차단"); // 정상 입력 복구
        Keyboard.current = null; // 가상 키보드 해제
        GameManager.Instance = null; // 게임 관리자 해제
    }

    private static void VerifyMergeDisable() // 합성 제어기 비활성 처리
    {
        WeaponData weapon = new WeaponData(); // 합성 무기 생성
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 1 }); // 첫 재료
        weapons.Slots.Add(new SnakeWeaponSlot { Weapon = weapon, Grade = 1 }); // 둘째 재료
        WeaponMergePanelController panel = new WeaponMergePanelController(); // 합성 화면
        SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 실제 합성 제어기
        merge.Configure(weapons, null, null, panel, null, null, 0.7f); // 구성 연결
        bool? result = null; // 종료 결과 기록
        merge.MergeClosed += merged => result = merged; // 종료 알림 연결
        Require(merge.OpenMergePanel() && panel.Visible, "합성 화면 열기 실패"); // 최초 화면 확인
        typeof(SnakeWeaponMergeController).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(merge, null); // 합성 제어기 비활성
        Require(!merge.IsMergeOpen && !panel.Visible && result == false, "비활성 시 합성 화면 또는 취소 알림 오류"); // 화면 종료 및 알림 확인
    }

    private static void VerifyCancellation() // 단계별 취소 보존
    {
        for (int step = 0; step < 4; step++) // 취소 단계 순회
        {
            var fixture = Create(3); // 무기 준비
            for (int index = 0; index < step; index++) // 선택 단계 진행
            {
                Select(fixture.controller, index < 2 ? 1 : 0); // 재료 및 계승 선택
            }
            fixture.controller.CloseMergePanel(); // 합성 취소
            Require(fixture.weapons.MergeCount == 0, $"단계 {step} 취소 전 합성 발생"); // 합성 방지 확인
            Require(fixture.weapons.Slots.Count == 3 && fixture.weapons.Slots[2].DamageUpgrades == 2, "취소 후 무기 변화"); // 무기 강화 보존 확인
            Require(!fixture.controller.IsMergeOpen, "취소 후 화면 유지"); // 화면 종료 확인
        }
    }

    private static void Require(bool condition, string message) // 조건 확인
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 전달
        }
    }
}
