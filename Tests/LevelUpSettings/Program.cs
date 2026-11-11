using System; // 기본 형식
using System.Reflection; // 입력 처리 호출
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Player; // 플레이어 상태
using ProjectEpsilon.Progression; // 실제 레벨업 제어기
using ProjectEpsilon.UI; // 보상 화면
using UnityEngine.InputSystem; // 키보드 상태

public static class Program // 회귀 검증기
{
    public static int Main(string[] args) // 검증 시작
    {
        try // 검증 실패 처리
        {
            bool mouse = args.Length > 0 && args[0] == "mouse"; // 마우스 검증 여부
            bool replacement = args.Length > 0 && args[0] == "replacement"; // 교체 검증 여부
            GameManager manager = new GameManager(); // 게임 관리자
            GameManager.Instance = manager; // 현재 관리자 등록
            InputBindingManager binding = new InputBindingManager(); // 키 설정 관리자
            InputBindingManager.Instance = binding; // 현재 입력 관리자 등록
            SettingsMenuController settings = new SettingsMenuController(); // 설정 화면
            SettingsMenuController.Active = settings; // 현재 설정 등록
            Keyboard keyboard = new Keyboard(); // 키보드 상태
            Keyboard.current = keyboard; // 키보드 등록
            SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자
            weapons.OccupiedSlotCount = replacement ? 2 : 0; // 만석 여부 설정
            SnakeExperience experience = new SnakeExperience(); // 경험치 상태
            LevelUpPanelController panel = new LevelUpPanelController(); // 레벨업 화면
            SnakeLevelUpController level = new SnakeLevelUpController(); // 실제 레벨업 제어기
            level.Configure(experience, new SnakeBodyManager(), new SnakeHealth(), weapons, new WeaponRewardPool(), panel); // 레벨업 구성
            experience.Request(2); // 레벨업 요청
            Require(level.IsPresentingLevelUp && manager.IsPaused, "레벨업 정지 실패"); // 보상 선택 정지 확인
            if (replacement) // 교체 화면 시험 확인
            {
                panel.Click(0); // 무기 후보 선택
                keyboard.escapeKey.wasPressedThisFrame = true; // 교체 취소 입력
            }
            else // 일반 선택 시험
            {
                keyboard.digit1Key.wasPressedThisFrame = true; // 첫 보상 선택 입력
            }
            settings.IsOpen = true; // 설정 화면 열기
            if (mouse) // 마우스 선택 확인
            {
                panel.Click(0); // 설정 중 마우스 선택
            }
            else // 키보드 선택 확인
            {
                typeof(SnakeLevelUpController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(level, null); // 설정 중 키 입력 처리
            }
            Require(level.IsPresentingLevelUp && manager.IsPaused && weapons.AcquisitionCount == 0 && experience.CompletionCount == 0, "설정 중 레벨업 보상 소비 또는 게임 재개"); // 보상과 일시정지 유지 확인
            settings.IsOpen = false; // 설정 화면 닫기
            binding.IsRebinding = true; // 키 재설정 시작
            panel.Click(0); // 재설정 중 마우스 선택
            typeof(SnakeLevelUpController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(level, null); // 재설정 중 키 입력 처리
            Require(level.IsPresentingLevelUp && weapons.AcquisitionCount == 0, "키 재설정 중 보상 선택 허용"); // 입력 재설정 우선 확인
            binding.IsRebinding = false; // 키 재설정 종료
            if (!replacement) // 일반 보상 확인
            {
                panel.Click(0); // 정상 마우스 선택
                Require(!level.IsPresentingLevelUp && manager.IsPlaying && experience.CompletionCount == 1 && weapons.AcquisitionCount == 1, "설정 종료 후 정상 선택 실패"); // 정상 처리 확인
            }
            Console.WriteLine("Level-up settings priority passed."); // 성공 기록
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
