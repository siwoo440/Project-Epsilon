using System; // 기본 형식
using System.Reflection; // 입력 호출
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 정보
using ProjectEpsilon.Progression; // 실제 합성 제어기
using ProjectEpsilon.UI; // 설정 화면
using UnityEngine.InputSystem; // 키보드 상태

public static class Program // 회귀 검증기
{
    public static int Main() // 검증 시작
    {
        try // 검증 실패 처리
        {
            GameManager manager = new GameManager(); // 게임 관리자
            GameManager.Instance = manager; // 현재 관리자 등록
            InputBindingManager binding = new InputBindingManager(); // 키 설정 관리자
            InputBindingManager.Instance = binding; // 키 설정 등록
            SettingsMenuController settings = new SettingsMenuController(); // 설정 화면
            SettingsMenuController.Active = settings; // 설정 화면 등록
            Keyboard keyboard = new Keyboard(); // 키보드 상태
            Keyboard.current = keyboard; // 키보드 등록
            SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자
            WeaponData weapon = new WeaponData(); // 시험 무기
            SnakeWeaponSlot first = new SnakeWeaponSlot(); // 첫 합성 슬롯
            first.Weapon = weapon; // 첫 무기 설정
            weapons.Slots.Add(first); // 첫 슬롯 등록
            SnakeWeaponSlot second = new SnakeWeaponSlot(); // 둘째 합성 슬롯
            second.Weapon = weapon; // 둘째 무기 설정
            weapons.Slots.Add(second); // 둘째 슬롯 등록
            SnakeExperience experience = new SnakeExperience(); // 경험치 상태
            SnakeLevelUpController level = new SnakeLevelUpController(); // 레벨업 상태
            SnakeWeaponMergeController merge = new SnakeWeaponMergeController(); // 실제 합성 제어기
            merge.Configure(weapons, null, null, new WeaponMergePanelController(), experience, level, 0.7f); // 제어기 설정
            MethodInfo update = typeof(SnakeWeaponMergeController).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic); // 입력 처리 조회
            keyboard.escapeKey.wasPressedThisFrame = true; // ESC 입력
            update.Invoke(merge, null); // 일시정지 입력 처리
            Require(manager.IsPaused, "ESC 일시정지 실패"); // 일시정지 확인
            settings.IsOpen = true; // 설정 화면 열기
            update.Invoke(merge, null); // 설정 중 ESC 처리
            Require(manager.IsPaused && !manager.IsPlaying, "설정 중 ESC가 게임 재개"); // 정지 유지 확인
            settings.IsOpen = false; // 설정 화면 닫기
            update.Invoke(merge, null); // 일반 ESC 처리
            Require(manager.IsPlaying, "설정 종료 후 ESC 재개 실패"); // 재개 확인
            keyboard.escapeKey.wasPressedThisFrame = false; // ESC 해제
            keyboard.eKey.wasPressedThisFrame = true; // 합성 키 입력
            settings.IsOpen = true; // 설정 화면 열기
            update.Invoke(merge, null); // 설정 중 합성 입력
            Require(!merge.IsMergeOpen, "설정 중 합성 화면 열림"); // 합성 차단 확인
            settings.IsOpen = false; // 설정 화면 닫기
            binding.IsRebinding = true; // 키 재설정 시작
            update.Invoke(merge, null); // 키 재설정 중 합성 입력
            Require(!merge.IsMergeOpen, "키 재설정 중 합성 화면 열림"); // 재설정 우선 확인
            binding.IsRebinding = false; // 키 재설정 종료
            update.Invoke(merge, null); // 일반 합성 입력
            Require(merge.IsMergeOpen, "일반 합성 화면 열기 실패"); // 합성 화면 확인
            settings.IsOpen = true; // 합성 중 설정 화면 열기
            manager.PauseGame(); // 설정 화면의 일시정지
            keyboard.eKey.wasPressedThisFrame = false; // 합성 키 해제
            update.Invoke(merge, null); // 설정 화면 상태 갱신
            Require(merge.IsMergeOpen && manager.IsPaused, "설정 중 합성 화면 종료"); // 합성 및 일시정지 유지 확인
            manager.ResumeGame(); // 설정 닫기 후 게임 재개
            experience.IsLevelUpPending = true; // 레벨업 우선 상황
            update.Invoke(merge, null); // 레벨업 우선 처리
            Require(!merge.IsMergeOpen, "레벨업 우선 합성 종료 실패"); // 레벨업 우선 확인
            Console.WriteLine("Pause and settings priority passed."); // 성공 기록
            return 0; // 성공 종료
        }
        catch (Exception exception) // 실패 예외 처리
        {
            Console.Error.WriteLine(exception.GetBaseException().Message); // 실패 내용 기록
            return 1; // 실패 종료
        }
    }
    private static void Require(bool condition, string message) // 조건 검증
    {
        if (!condition) // 실패 조건
        {
            throw new InvalidOperationException(message); // 실패 보고
        }
    }
}
