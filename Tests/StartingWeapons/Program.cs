using System; // 기본 형식
using System.Reflection; // 생명주기 호출
using ProjectEpsilon.Combat; // 무기 관리자
using ProjectEpsilon.Core; // 게임 상태
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 시작 선택
using ProjectEpsilon.UI; // 후보 화면
using UnityEngine; // 프레임 환경
using UnityEngine.InputSystem; // 키 입력

internal static class Program // 시작 무기 선택 시험
{
    private static int failures; // 실패 횟수
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic; // 내부 메서드 범위

    private static int Main() // 시험 시작
    {
        Check("선택 전 진행 정지와 단일 장착", VerifySelection); // 시작 선택 검사
        Check("후보 누락 시 진행 차단", VerifyInvalidOptions); // 후보 누락 검사
        Check("중복 후보 시 진행 차단", VerifyDuplicateOptions); // 후보 중복 검사
        Check("진행 중 후보 설정 오류", VerifyInvalidReconfiguration); // 재설정 오류 검사
        Check("비활성 복구와 중복 구독", VerifyDisableAndSubscription); // 복구 및 구독 검사
        Check("설정 및 재설정 입력 우선", VerifyInputPriority); // 입력 우선순위 검사
        Check("선택기 없는 기존 씬 시작", VerifyLegacyScene); // 기존 씬 호환 검사
        Check("비활성 선택기 대기 및 복구", VerifyInitiallyDisabled); // 초기 비활성 검사
        Check("비활성 선택기 재설정", VerifyDisabledReconfiguration); // 비활성 재설정 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과
        return failures == 0 ? 0 : 1; // 종료 결과
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
            Console.WriteLine($"FAIL: {name}: {error.GetBaseException().Message}"); // 실패 출력
        }
    }

    private static void Call(object instance, string method) // 내부 생명주기 호출
    {
        instance.GetType().GetMethod(method, Private).Invoke(instance, null); // 지정 동작 실행
    }

    private static (GameManager game, StartingWeaponSelection selection, LevelUpPanelController panel, SnakeWeaponManager weapons, WeaponData[] options) Create(bool missing = false, bool duplicate = false) // 시험 구성
    {
        typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null); // 이전 게임 초기화
        InputBindingManager.Instance = new InputBindingManager(); // 입력 설정 생성
        SettingsMenuController.Active = new SettingsMenuController(); // 설정 화면 생성
        Keyboard.current = new Keyboard(); // 가상 키보드 생성
        WeaponData[] options = { new WeaponData { Name = "A" }, new WeaponData { Name = "B" }, missing ? null : new WeaponData { Name = "C" } }; // 임시 후보 구성
        if (duplicate) // 중복 후보 시험 확인
        {
            options[2] = options[0]; // 동일 무기 중복 배치
        }
        StartingWeaponOptions data = new StartingWeaponOptions(); // 후보 데이터 생성
        data.Configure(options); // 후보 데이터 저장
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        LevelUpPanelController panel = new LevelUpPanelController(); // 후보 화면 생성
        StartingWeaponSelection selection = new StartingWeaponSelection(); // 시작 선택기 생성
        selection.Configure(weapons, panel, data); // 선택 구성 연결
        MonoBehaviour.Component = selection; // 씬 선택기 등록
        GameManager game = new GameManager(); // 실제 게임 관리자
        Call(game, "Awake"); // 관리자 초기화
        Call(selection, "OnEnable"); // 선택기 활성화
        Call(game, "Start"); // 시작 상태 결정
        Call(selection, "Start"); // 후보 화면 준비
        return (game, selection, panel, weapons, options); // 시험 객체 반환
    }

    private static void VerifySelection() // 시작 선택 및 단일 확정
    {
        var fixture = Create(); // 세 후보 준비
        Require(!fixture.game.IsPlaying && Time.timeScale == 0f && fixture.selection.IsSelecting && fixture.panel.Visible, "선택 전 게임 또는 시간 진행"); // 정지와 화면 확인
        fixture.panel.Click(2); // 세 번째 후보 선택
        Require(fixture.weapons.EquipCount == 1 && fixture.weapons.Weapon == fixture.options[2], "선택 무기 장착 오류"); // 첫 몸통 무기 확인
        Require(fixture.game.IsPlaying && Time.timeScale == 1f && !fixture.panel.Visible, "선택 후 게임 시작 오류"); // 진행 복구 확인
        fixture.panel.Click(0); // 중복 입력
        Require(fixture.weapons.EquipCount == 1, "시작 무기 중복 확정"); // 중복 선택 방지
    }

    private static void VerifyInvalidOptions() // 잘못된 후보 데이터
    {
        Debug.ErrorCount = 0; // 오류 기록 초기화
        var fixture = Create(true); // 누락 후보 준비
        Require(!fixture.game.IsPlaying && Time.timeScale == 0f && Debug.ErrorCount > 0, "후보 누락 시 오류 또는 시작 차단 실패"); // 구성 오류 확인
        fixture.panel.Click(0); // 불완전 후보 선택 시도
        Require(fixture.weapons.EquipCount == 0 && !fixture.game.IsPlaying, "누락 후보로 게임 시작"); // 불완전 구성 차단
    }

    private static void VerifyInvalidReconfiguration() // 후보 표시 중 잘못된 재설정
    {
        var fixture = Create(); // 정상 후보 표시
        StartingWeaponOptions broken = new StartingWeaponOptions(); // 잘못된 후보 설정
        broken.Configure(new[] { fixture.options[0], fixture.options[1], null }); // 마지막 후보 누락
        fixture.selection.Configure(fixture.weapons, fixture.panel, broken); // 후보 구성 교체
        Require(!fixture.selection.IsSelecting && !fixture.panel.Visible && !fixture.game.IsPlaying, "잘못된 재설정 후 이전 후보 화면 잔존"); // 화면 및 시작 차단 확인
        fixture.panel.Click(0); // 이전 후보 입력 시도
        Require(fixture.weapons.EquipCount == 0, "잘못된 재설정 후 무기 확정"); // 이전 후보 확정 방지
    }

    private static void VerifyDuplicateOptions() // 중복 시작 후보 차단
    {
        Debug.ErrorCount = 0; // 오류 기록 초기화
        var fixture = Create(false, true); // 중복 후보 준비
        Require(!fixture.game.IsPlaying && Time.timeScale == 0f && Debug.ErrorCount > 0, "중복 후보 오류 또는 시작 차단 실패"); // 중복 구성 확인
        fixture.panel.Click(0); // 중복 구성 선택 시도
        Require(fixture.weapons.EquipCount == 0, "중복 후보로 무기 장착"); // 불완전 선택 차단
    }

    private static void VerifyDisableAndSubscription() // 비활성 및 중복 구독
    {
        var fixture = Create(); // 세 후보 준비
        Call(fixture.selection, "OnEnable"); // 중복 활성 호출
        Call(fixture.selection, "OnDisable"); // 선택기 비활성
        Require(!fixture.panel.Visible && !fixture.game.IsPlaying && Time.timeScale == 0f, "선택기 비활성 시 화면 또는 시간 오류"); // 비활성 정리
        Call(fixture.selection, "OnEnable"); // 선택기 복구
        Require(fixture.panel.Visible && fixture.selection.IsSelecting, "선택기 재활성 시 후보 복구 실패"); // 선택 화면 복구
        fixture.panel.Click(1); // 두 번째 후보 선택
        Require(fixture.weapons.EquipCount == 1 && fixture.weapons.Weapon == fixture.options[1], "중복 구독으로 무기 중복 장착"); // 단일 선택 확인
    }

    private static void VerifyInputPriority() // 설정 중 선택 방지
    {
        var fixture = Create(); // 세 후보 준비
        SettingsMenuController.Active.IsOpen = true; // 설정 화면 열기
        fixture.panel.Click(0); // 설정 중 버튼 입력
        Keyboard.current.digit2Key.wasPressedThisFrame = true; // 설정 중 숫자 입력
        Call(fixture.selection, "Update"); // 입력 처리
        Require(fixture.weapons.EquipCount == 0 && !fixture.game.IsPlaying, "설정 중 시작 무기 확정"); // 설정 입력 우선
        SettingsMenuController.Active.IsOpen = false; // 설정 화면 닫기
        InputBindingManager.Instance.IsRebinding = true; // 입력 재설정 시작
        fixture.panel.Click(0); // 재설정 중 버튼 입력
        Call(fixture.selection, "Update"); // 재설정 중 숫자 입력
        Require(fixture.weapons.EquipCount == 0 && !fixture.game.IsPlaying, "키 재설정 중 시작 무기 확정"); // 재설정 입력 우선
        InputBindingManager.Instance.IsRebinding = false; // 재설정 종료
        Call(fixture.selection, "Update"); // 정상 숫자 입력
        Require(fixture.weapons.Weapon == fixture.options[1] && fixture.game.IsPlaying, "설정 종료 뒤 정상 선택 실패"); // 정상 선택 확인
    }

    private static void VerifyDisabledReconfiguration() // 비활성 중 선택기 재설정
    {
        var fixture = Create(); // 시작 후보 표시
        fixture.selection.isActiveAndEnabled = false; // 선택기 비활성
        Call(fixture.selection, "OnDisable"); // 비활성 생명주기 호출
        StartingWeaponOptions data = new StartingWeaponOptions(); // 교체 후보 데이터
        data.Configure(fixture.options); // 기존 세 후보 저장
        fixture.selection.Configure(fixture.weapons, fixture.panel, data); // 비활성 중 구성 변경
        Require(!fixture.selection.IsSelecting && !fixture.panel.Visible && !fixture.game.IsPlaying, "비활성 재설정이 선택 화면을 다시 표시"); // 비활성 상태 유지 확인
        fixture.panel.Click(0); // 비활성 중 선택 시도
        Require(fixture.weapons.EquipCount == 0, "비활성 중 시작 무기 확정"); // 잘못된 장착 방지
        fixture.selection.isActiveAndEnabled = true; // 선택기 재활성
        Call(fixture.selection, "OnEnable"); // 선택기 복구
        Require(fixture.selection.IsSelecting && fixture.panel.Visible, "재활성 후 선택 화면 누락"); // 정상 복구 확인
    }

    private static void VerifyInitiallyDisabled() // 시작 전 선택기 비활성
    {
        typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null); // 이전 게임 초기화
        WeaponData[] choices = { new WeaponData(), new WeaponData(), new WeaponData() }; // 세 후보 준비
        StartingWeaponOptions data = new StartingWeaponOptions(); // 후보 데이터 생성
        data.Configure(choices); // 세 후보 저장
        SnakeWeaponManager weapons = new SnakeWeaponManager(); // 무기 관리자 생성
        LevelUpPanelController panel = new LevelUpPanelController(); // 시작 선택 화면
        StartingWeaponSelection selection = new StartingWeaponSelection { isActiveAndEnabled = false }; // 비활성 선택기
        selection.Configure(weapons, panel, data); // 선택 구성 연결
        MonoBehaviour.Component = selection; // 씬 선택기 등록
        GameManager game = new GameManager(); // 게임 관리자 생성
        Call(game, "Awake"); // 게임 초기화
        Call(game, "Start"); // 비활성 선택기 상태로 시작
        Require(!game.IsPlaying && Time.timeScale == 0f && weapons.EquipCount == 0, "비활성 선택기를 건너뛰고 게임 시작"); // 시작 대기 확인
        selection.isActiveAndEnabled = true; // 선택기 재활성
        Call(selection, "OnEnable"); // 활성 처리
        Call(selection, "Start"); // 최초 선택 화면 표시
        Require(selection.IsSelecting && panel.Visible && !game.IsPlaying, "재활성 후 시작 선택 복구 실패"); // 선택 화면 확인
        panel.Click(1); // 둘째 무기 선택
        Require(game.IsPlaying && weapons.Weapon == choices[1] && weapons.EquipCount == 1, "재활성 뒤 정상 선택 실패"); // 시작 결과 확인
    }

    private static void VerifyLegacyScene() // 선택기 없는 기존 씬
    {
        typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null); // 이전 게임 초기화
        MonoBehaviour.Component = null; // 선택기 없는 씬
        GameManager game = new GameManager(); // 실제 게임 관리자
        Call(game, "Awake"); // 관리자 초기화
        Call(game, "Start"); // 기존 시작 처리
        Require(game.IsPlaying && Time.timeScale == 1f, "기존 씬 자동 시작 실패"); // 호환 상태 확인
    }

    private static void Require(bool condition, string message) // 조건 검사
    {
        if (!condition) // 실패 조건 확인
        {
            throw new InvalidOperationException(message); // 실패 전달
        }
    }
}
