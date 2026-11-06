using System; // 기본 형식
using System.Reflection; // 생명주기 호출
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 상태
using ProjectEpsilon.Progression; // 레벨업 및 합성
using ProjectEpsilon.UI; // 레벨업 화면
using UnityEngine; // 구성 대체

internal static class Program // 보상 합성 흐름 시험
{ // 클래스 시작
    private static int failures; // 실패 횟수

    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("보상 합성 취소 보존", VerifyCancel); // 취소 복귀 검사
        Check("합성 성공 중복 완료 방지", VerifySuccess); // 성공 및 중복 방지 검사
        Check("합성 뒤 연속 레벨업", VerifyQueuedLevel); // 연속 선택 검사
        Check("합성 중 패배", VerifyDefeat); // 패배 상태 검사
        Check("초과 피해로 인한 패배 정리", VerifyOverflowDefeat); // 연속 피해 패배 검사
        Check("제어기 비활성 후 복구", VerifyDisableRestore); // 비활성 복구 검사
        Check("외부 정지 뒤 보상 복구", VerifyExternalPause); // 정지 소유 검사
        Check("처리 방식 비활성 복구", () => VerifyChoiceDisable("action")); // 처리 방식 복구 검사
        Check("보상 후보 비활성 복구", () => VerifyChoiceDisable("reward")); // 후보 복구 검사
        Check("교체 화면 비활성 복구", () => VerifyChoiceDisable("replacement")); // 교체 복구 검사
        Check("마지막 호환 재료 손실 안내", VerifyNoMaterial); // 재료 소실 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과 출력
        return failures == 0 ? 0 : 1; // 종료 결과
    } // 메서드 끝

    private static void Check(string name, Action action) // 개별 흐름 검사
    { // 메서드 시작
        try // 시험 시도
        { // 시도 시작
            action(); // 시험 실행
            Console.WriteLine($"PASS: {name}"); // 성공 출력
        } // 시도 끝
        catch (Exception error) // 실패 처리
        { // 예외 시작
            failures++; // 실패 횟수 증가
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 출력
        } // 예외 끝
    } // 메서드 끝

    private static (SnakeLevelUpController level, SnakeWeaponMergeController merge, SnakeExperience xp, SnakeWeaponManager weapons, LevelUpPanelController panel, GameManager game, SnakeHealth health) Create(bool enterMerge = true, bool full = false) // 시험 구성
    { // 메서드 시작
        GameManager game = new GameManager(); // 게임 관리자 생성
        GameManager.Instance = game; // 현재 게임 등록
        InputBindingManager.Instance = new InputBindingManager(); // 키 설정 등록
        SettingsMenuController.Active = new SettingsMenuController(); // 설정 화면 등록
        SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 합성 제어기 생성
        MonoBehaviour.Component = merge; // 합성 구성 제공
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        weapons.Slots.Add(new SnakeWeaponSlot { Weapon = WeaponRewardPool.Reward, Grade = 1 }); // 같은 무기 장착
        weapons.Slots.Add(full ? new SnakeWeaponSlot { Weapon = WeaponRewardPool.Reward, Grade = 1 } : new SnakeWeaponSlot()); // 빈 슬롯 또는 무기 구성
        weapons.SlotCount = 2; // 슬롯 수 설정
        weapons.OccupiedSlotCount = full ? 2 : 1; // 장착 수 설정
        SnakeExperience xp = new SnakeExperience(); // 경험치 구성
        LevelUpPanelController panel = new LevelUpPanelController(); // 보상 화면 생성
        SnakeLevelUpController level = new SnakeLevelUpController(); // 실제 레벨업 제어기
        SnakeHealth health = new SnakeHealth(); // 실제 소진 알림 대체
        level.Configure(xp, new SnakeBodyManager(), health, weapons, new WeaponRewardPool(), panel); // 참조 연결
        xp.Request(2); // 레벨업 요청
        Require(game.IsPaused && level.IsPresentingLevelUp, "레벨업 정지 실패"); // 최초 정지 확인
        panel.Click(0); // 무기 보상 선택
        Require(game.IsPaused && level.IsPresentingLevelUp && panel.Mode == "action", "보상 처리 방법 선택 누락"); // 처리 방식 표시
        Require(weapons.AcquisitionCount == 0 && xp.CompletionCount == 0, "처리 방식 선택 전 보상 소비"); // 조기 확정 방지
        if (enterMerge) // 보상 합성 진입 여부
        { // 조건 시작
            panel.Click(1); // 미장착 보상 합성 선택
            Require(merge.IsMergeOpen && game.IsPlaying && xp.CompletionCount == 0, "합성 중 플레이 또는 보상 대기 오류"); // 합성 진입 확인
        } // 조건 끝
        return (level, merge, xp, weapons, panel, game, health); // 시험 객체 반환
    } // 메서드 끝

    private static void VerifyCancel() // 취소 뒤 보상 복귀
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.merge.CloseMergePanel(false); // 합성 취소
        Require(fixture.game.IsPaused && fixture.level.IsPresentingLevelUp && fixture.panel.Mode == "action", "취소 뒤 보상 화면 복귀 실패"); // 화면 및 정지 복귀
        Require(fixture.xp.CompletionCount == 0 && fixture.weapons.AcquisitionCount == 0, "취소 시 보상 소비"); // 미확정 보존
        fixture.panel.Click(1); // 같은 보상으로 합성 재시도
        Require(fixture.merge.IsMergeOpen && fixture.game.IsPlaying, "취소 뒤 합성 재시도 실패"); // 재시도 확인
    } // 메서드 끝

    private static void VerifySuccess() // 합성 성공 뒤 완료
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.merge.CloseMergePanel(true); // 합성 성공 알림
        Require(fixture.xp.CompletionCount == 1 && fixture.game.IsPlaying && !fixture.level.IsPresentingLevelUp, "합성 성공 뒤 레벨업 완료 오류"); // 단일 완료 확인
        fixture.merge.CloseMergePanel(true); // 중복 결과 알림
        Require(fixture.xp.CompletionCount == 1, "합성 결과 중복 소비"); // 중복 완료 방지
    } // 메서드 끝

    private static void VerifyQueuedLevel() // 연속 레벨업 재진입
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.xp.RequestNext = true; // 다음 레벨업 예약
        fixture.merge.CloseMergePanel(true); // 합성 성공
        Require(fixture.xp.CompletionCount == 1 && fixture.level.IsPresentingLevelUp && fixture.game.IsPaused, "연속 레벨업 정지 누락"); // 다음 보상 정지 확인
        Require(fixture.panel.Mode == "reward", "다음 무기 후보 표시 누락"); // 새 보상 화면 확인
    } // 메서드 끝

    private static void VerifyDefeat() // 합성 중 패배 정리
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.game.Defeat(); // 외부 패배 상태
        fixture.merge.CloseMergePanel(false); // 합성 취소 알림
        Require(!fixture.game.IsPlaying && !fixture.game.IsPaused && !fixture.level.IsPresentingLevelUp, "패배 후 게임 재개 또는 화면 복귀"); // 패배 유지 확인
        Require(fixture.xp.CompletionCount == 0, "패배 시 보상 완료"); // 미완료 보존
    } // 메서드 끝

    private static void VerifyOverflowDefeat() // 재료 손실 직후 패배
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.merge.CloseMergePanel(false); // 첫 몸통 손실로 합성 취소
        Require(fixture.game.IsPaused && fixture.panel.Mode == "action", "첫 손실 후 보상 정지 실패"); // 임시 복구 확인
        fixture.health.Deplete(); // 같은 피해의 남은 몸통 소진
        fixture.game.Defeat(); // 패배 상태 전환
        Require(!fixture.game.IsPlaying && !fixture.game.IsPaused && !fixture.level.IsPresentingLevelUp && fixture.panel.Mode == "hidden", "패배 후 보상 화면 잔존"); // 패배 화면 정리 확인
        Require(fixture.xp.CompletionCount == 0, "패배 중 미확정 보상 소비"); // 보상 미완료 확인
    } // 메서드 끝

    private static void VerifyDisableRestore() // 비활성 후 같은 보상 복구
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        typeof(SnakeLevelUpController).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.level, null); // 레벨업 제어기 비활성
        Require(!fixture.merge.IsMergeOpen && fixture.game.IsPlaying && fixture.xp.CompletionCount == 0, "비활성 중 합성 또는 보상 정리 오류"); // 진행 상태 확인
        typeof(SnakeLevelUpController).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.level, null); // 레벨업 제어기 복구
        Require(fixture.game.IsPaused && fixture.level.IsPresentingLevelUp && fixture.panel.Mode == "action", "비활성 후 같은 보상 복구 실패"); // 보상 복구 확인
    } // 메서드 끝

    private static void VerifyNoMaterial() // 호환 재료 손실 후 복귀
    { // 메서드 시작
        var fixture = Create(); // 보상 합성 구성
        fixture.weapons.Slots[0].Weapon = null; // 마지막 호환 재료 손실
        fixture.merge.CloseMergePanel(false); // 슬롯 손실로 합성 취소
        Require(fixture.game.IsPaused && fixture.panel.Mode == "action", "재료 손실 뒤 선택 복귀 실패"); // 보상 선택 복귀 확인
        Require(!fixture.panel.MergeAvailable, "재료 없는 합성 버튼 활성"); // 합성 불가 안내 확인
        Require(fixture.xp.CompletionCount == 0, "재료 손실 시 보상 소비"); // 보상 보존 확인
    } // 메서드 끝

    private static void VerifyChoiceDisable(string phase) // 레벨업 선택 중 비활성 복구
    { // 메서드 시작
        var fixture = Create(false, phase == "replacement"); // 선택 화면 구성
        if (phase == "reward") // 후보 화면 준비
        { // 조건 시작
            fixture.panel.Click(2); // 기존 후보로 돌아가기
        } // 조건 끝
        if (phase == "replacement") // 교체 화면 준비
        { // 조건 시작
            fixture.panel.Click(0); // 만석 무기 교체 선택
        } // 조건 끝
        Require(fixture.panel.Mode == phase, "시험 화면 준비 실패"); // 최초 단계 확인
        typeof(SnakeLevelUpController).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.level, null); // 레벨업 제어기 비활성
        Require(fixture.game.IsPlaying && fixture.panel.Mode == "hidden" && fixture.xp.CompletionCount == 0, "비활성 시 화면 또는 정지 정리 실패"); // 비활성 정리 확인
        typeof(SnakeLevelUpController).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.level, null); // 제어기 복구
        Require(fixture.game.IsPaused && fixture.level.IsPresentingLevelUp && fixture.panel.Mode == phase, "비활성 전 보상 화면 복구 실패"); // 기존 단계 복구 확인
        Require(fixture.xp.CompletionCount == 0, "비활성 복구 중 보상 소비"); // 미확정 보상 확인
    } // 메서드 끝

    private static void VerifyExternalPause() // 외부 정지 중 합성 취소
    { // 메서드 시작
        var fixture = Create(); // 시험 구성
        fixture.game.PauseGame(); // 외부 일시정지
        fixture.merge.CloseMergePanel(false); // 정지 중 합성 취소
        Require(fixture.game.IsPaused && fixture.xp.CompletionCount == 0, "외부 정지 중 게임 재개 또는 보상 소비"); // 외부 상태 유지
        fixture.game.ResumeGame(); // 외부 정지 해제
        typeof(SnakeLevelUpController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.level, null); // 보상 복구 시도
        Require(fixture.game.IsPaused && fixture.level.IsPresentingLevelUp && fixture.panel.Mode == "action", "외부 정지 뒤 보상 복구 실패"); // 보상 정지 확인
    } // 메서드 끝

    private static void Require(bool condition, string message) // 조건 검사
    { // 메서드 시작
        if (!condition) // 실패 조건
        { // 조건 시작
            throw new InvalidOperationException(message); // 실패 전달
        } // 조건 끝
    } // 메서드 끝
} // 클래스 끝
