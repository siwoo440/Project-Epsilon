using System; // 예외 형식
using System.Reflection; // 실제 버튼 및 상태 조회
using ProjectEpsilon.Combat; // 무기 슬롯
using ProjectEpsilon.Core; // 게임 일시정지
using ProjectEpsilon.Data; // 무기 데이터
using ProjectEpsilon.Progression; // 경험치 및 레벨업
using ProjectEpsilon.UI; // 설정 및 보상 화면
using UnityEditor; // 배치 편집기
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 유니티 기능
using UnityEngine.InputSystem; // 가상 입력 장치
using UnityEngine.InputSystem.LowLevel; // 키보드 상태
using UnityEngine.UI; // 실제 버튼

namespace ProjectEpsilon.Editor // 편집기 영역
{ // 영역 시작
    [InitializeOnLoad] // 도메인 갱신 연결
    public static class AutomatedSettingsPriorityVerification // 설정 및 보상 입력 통합 검증
    { // 클래스 시작
        private const string ActiveKey = "EpsilonSettingsPriorityVerification"; // 진행 상태 표식
        private static readonly BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance; // 내부 상태 조회 설정
        private static double started; // 실제 시험 시작 시각
        private static int stage; // 현재 검증 단계
        private static int waitUntilFrame; // 다음 단계 실행 프레임
        private static Keyboard keyboard; // 시험용 가상 키보드
        private static InputAction escapeProbe; // ESC 전달 확인 액션
        private static InputAction digitProbe; // 숫자 1 전달 확인 액션
        private static int escapes; // 실제 ESC 입력 수
        private static int digits; // 실제 숫자 입력 수
        private static InputSettings originalSettings; // 원래 입력 설정
        private static InputSettings testSettings; // 저장되지 않는 입력 설정 사본
        private static bool originalBackground; // 원래 백그라운드 설정
        private static int occupiedBefore; // 레벨업 선택 전 무기 수
        private static GameManager game; // 게임 관리자
        private static SnakeExperience experience; // 경험치 관리자
        private static SnakeLevelUpController level; // 레벨업 처리기
        private static SnakeWeaponManager weapons; // 무기 관리자
        private static SettingsMenuController settings; // 설정 화면
        private static LevelUpPanelController panel; // 보상 화면

        static AutomatedSettingsPriorityVerification() // 도메인 연결
        { // 생성자 시작
            if (SessionState.GetBool(ActiveKey, false)) // 진행 표식 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 검증 갱신 연결
            } // 조건 끝
        } // 생성자 끝

        public static void Run() // 배치 진입점
        { // 메서드 시작
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(ActiveKey, true); // 진행 상태 저장
            EditorApplication.update -= Tick; // 중복 연결 해제
            EditorApplication.update += Tick; // 검증 갱신 연결
            EditorApplication.isPlaying = true; // 플레이 모드 진입
        } // 메서드 끝

        private static void Tick() // 단계별 입력 검증
        { // 메서드 시작
            if (!EditorApplication.isPlaying) // 플레이 진입 확인
            { // 조건 시작
                return; // 플레이 시작 대기
            } // 조건 끝
            if (started == 0d) // 최초 갱신 확인
            { // 조건 시작
                started = EditorApplication.timeSinceStartup; // 실제 시작 시간 저장
            } // 조건 끝
            try // 실패 정리 보장
            { // 보호 시작
                if (EditorApplication.timeSinceStartup - started > 60d) // 실행 제한 확인
                { // 조건 시작
                    throw new InvalidOperationException("Settings priority test timed out at stage " + stage); // 시간 초과 보고
                } // 조건 끝
                if (Time.frameCount < waitUntilFrame) // 실제 게임 프레임 진행 확인
                { // 조건 시작
                    return; // 입력 처리 대기
                } // 조건 끝
                if (stage == 0) // 실제 씬 준비 단계
                { // 조건 시작
                    Prepare(); // 객체 및 시험 입력 준비
                    return; // 첫 입력 처리 대기
                } // 조건 끝
                if (stage == 1) // ESC 일시정지 확인
                { // 조건 시작
                    Require(escapes >= 1 && game.IsPaused && !settings.IsOpen, "ESC did not pause the running game."); // 실제 ESC와 정지 확인
                    Release(); // 가상 키 해제
                    Advance(2); // 키 해제 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 2) // 설정 중 ESC 입력
                { // 조건 시작
                    ClickSettings("openButton"); // 실제 설정 버튼 클릭
                    Require(settings.IsOpen && game.IsPaused, "Settings failed to open while paused."); // 설정 표시와 정지 확인
                    Press(Key.Escape); // 설정 위 ESC 입력
                    Advance(3); // 입력 처리 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 3) // 설정 중 ESC 차단 확인
                { // 조건 시작
                    Require(escapes >= 2 && settings.IsOpen && game.IsPaused, "ESC resumed gameplay under settings."); // ESC 전달 및 게임 정지 유지 확인
                    Debug.Log("[SettingsPriority] ESC reached input system; settings stayed open and game stayed paused."); // 실제 입력 결과 기록
                    Release(); // 가상 키 해제
                    Advance(4); // 키 해제 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 4) // 설정 종료 후 ESC 입력
                { // 조건 시작
                    ClickSettings("closeButton"); // 실제 설정 닫기 버튼 클릭
                    Require(!settings.IsOpen && game.IsPaused, "Closing settings changed prior pause state."); // 기존 일시정지 보존 확인
                    Press(Key.Escape); // 일시정지 해제 입력
                    Advance(5); // 입력 처리 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 5) // 정상 재개 확인
                { // 조건 시작
                    Require(escapes >= 3 && game.IsPlaying, "ESC did not resume after settings closed."); // ESC 재개 확인
                    Release(); // 가상 키 해제
                    Advance(6); // 키 해제 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 6) // 레벨업 선택 및 설정 열기
                { // 조건 시작
                    GrantLevelExperience(); // 실제 경험치 주입 및 레벨업 시작
                    occupiedBefore = weapons.OccupiedSlotCount; // 선택 전 무기 수 기록
                    ClickSettings("openButton"); // 레벨업 중 설정 열기
                    Require(settings.IsOpen && game.IsPaused && level.IsPresentingLevelUp, "Settings did not open over level-up."); // 설정과 레벨업 중첩 확인
                    Press(Key.Digit1); // 보상 첫 숫자 입력
                    Advance(7); // 입력 처리 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 7) // 숫자 및 뒤쪽 버튼 차단 확인
                { // 조건 시작
                    Require(digits >= 1 && settings.IsOpen && level.IsPresentingLevelUp && experience.IsLevelUpPending && game.IsPaused, "Numeric selection leaked through settings."); // 숫자 전달 및 선택 차단 확인
                    Require(weapons.OccupiedSlotCount == occupiedBefore, "Numeric selection changed weapons under settings."); // 무기 변경 차단 확인
                    ClickCandidate(0); // 설정 뒤 실제 보상 버튼 이벤트 호출
                    Require(level.IsPresentingLevelUp && experience.IsLevelUpPending && game.IsPaused && weapons.OccupiedSlotCount == occupiedBefore, "Button selection leaked through settings."); // 클릭 이벤트 차단 확인
                    Debug.Log("[SettingsPriority] Actual digit 1 and underlying button event were blocked while settings remained open."); // 차단 결과 기록
                    Release(); // 숫자 키 해제
                    Advance(8); // 키 해제 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 8) // 설정 종료 후 보상 선택
                { // 조건 시작
                    ClickSettings("closeButton"); // 설정 닫기
                    Require(!settings.IsOpen && level.IsPresentingLevelUp && game.IsPaused, "Settings close changed pending level-up."); // 대기 보상 유지 확인
                    ClickCandidate(0); // 실제 첫 보상 버튼 선택
                    Require(!level.IsPresentingLevelUp && !experience.IsLevelUpPending && game.IsPlaying, "Weapon selection did not resume gameplay."); // 보상 완료와 재개 확인
                    Require(weapons.OccupiedSlotCount == occupiedBefore + 1, "Weapon selection did not equip reward."); // 무기 장착 확인
                    Debug.Log("[SettingsPriority] Closing settings allowed actual reward button and resumed gameplay."); // 정상 선택 기록
                    Advance(9); // 다음 프레임 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 9) // 만석 교체 중 설정 열기
                { // 조건 시작
                    WeaponData weapon = weapons.Slots[0].Weapon; // 기존 무기 조회
                    Require(weapon != null, "No weapon exists for replacement test."); // 시험용 무기 확인
                    GrantLevelExperience(); // 추가 경험치 주입 및 레벨업 시작
                    for (int index = 0; index < weapons.SlotCount; index++) // 모든 슬롯 순회
                    { // 반복 시작
                        Require(weapons.TryEquipAt(index, weapon, 1), "Could not prepare full weapon slots."); // 만석 시험 조건 설정
                    } // 반복 끝
                    ClickCandidate(0); // 실제 무기 보상 선택
                    Require(IsSelectingReplacement() && game.IsPaused, "Full slots did not enter replacement selection."); // 만석 교체 단계 확인
                    ClickSettings("openButton"); // 교체 중 설정 열기
                    Require(settings.IsOpen, "Settings did not open over replacement."); // 설정 표시 확인
                    Press(Key.Escape); // 설정 위 ESC 입력
                    Advance(10); // 입력 처리 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 10) // 만석 교체 ESC 차단
                { // 조건 시작
                    Require(escapes >= 4 && settings.IsOpen && IsSelectingReplacement() && level.IsPresentingLevelUp && game.IsPaused, "ESC rejected a replacement under settings."); // ESC 전달과 교체 대기 유지 확인
                    Release(); // ESC 키 해제
                    Advance(11); // 키 해제 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 11) // 설정 종료 후 교체 거절
                { // 조건 시작
                    ClickSettings("closeButton"); // 설정 닫기
                    Require(!settings.IsOpen && IsSelectingReplacement() && game.IsPaused, "Closing settings changed replacement state."); // 교체 대기 유지 확인
                    Press(Key.Escape); // 실제 교체 거절 입력
                    Advance(12); // 입력 처리 대기
                    return; // 다음 단계 대기
                } // 조건 끝
                if (stage == 12) // 교체 거절 후 재개 확인
                { // 조건 시작
                    Require(escapes >= 5 && !level.IsPresentingLevelUp && !experience.IsLevelUpPending && game.IsPlaying, "Replacement cancel did not resume correctly."); // 중복 ESC 없이 재개 확인
                    Debug.Log("[SettingsPriority] PASS: pause/settings ESC, digit and actual button blocking, normal selection, full-slot ESC priority."); // 실제 씬 통합 검증 성공
                    Finish(0); // 성공 종료
                } // 조건 끝
            } // 보호 끝
            catch (Exception exception) // 검증 예외 확인
            { // 예외 시작
                Debug.LogError("[SettingsPriority] stage=" + stage + " " + exception); // 실패 상세 기록
                Finish(1); // 실패 종료
            } // 예외 끝
        } // 메서드 끝

        private static void Prepare() // 실제 씬 구성 및 가상 키보드 준비
        { // 메서드 시작
            game = GameManager.Instance; // 게임 관리자 조회
            experience = UnityEngine.Object.FindFirstObjectByType<SnakeExperience>(); // 경험치 관리자 조회
            level = UnityEngine.Object.FindFirstObjectByType<SnakeLevelUpController>(); // 레벨업 처리기 조회
            weapons = UnityEngine.Object.FindFirstObjectByType<SnakeWeaponManager>(); // 무기 관리자 조회
            settings = UnityEngine.Object.FindFirstObjectByType<SettingsMenuController>(); // 설정 화면 관리자 조회
            panel = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 비활성 보상 화면 포함 조회
            if (game == null || experience == null || level == null || weapons == null || settings == null || panel == null || !game.IsPlaying || Time.time < 0.35f) // 씬 초기화 확인
            { // 조건 시작
                return; // 초기화 및 첫 물리 단계 대기
            } // 조건 끝
            originalSettings = InputSystem.settings; // 원래 입력 설정 저장
            testSettings = UnityEngine.Object.Instantiate(originalSettings); // 저장되지 않는 설정 사본 생성
            testSettings.hideFlags = HideFlags.HideAndDontSave; // 설정 사본 저장 방지
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; // 배치 키보드 게임 전달
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; // 배치 포커스 무시
            InputSystem.settings = testSettings; // 시험 설정 메모리 적용
            originalBackground = Application.runInBackground; // 원래 백그라운드 상태 저장
            Application.runInBackground = true; // 비활성 배치 입력 처리
            keyboard = InputSystem.AddDevice<Keyboard>(); // 가상 키보드 생성
            escapeProbe = new InputAction("EscapeProbe", InputActionType.Button, "<Keyboard>/escape"); // ESC 전달 확인 액션
            digitProbe = new InputAction("DigitProbe", InputActionType.Button, "<Keyboard>/1"); // 숫자 1 전달 확인 액션
            escapeProbe.performed += context => escapes++; // 실제 ESC 이벤트 수집
            digitProbe.performed += context => digits++; // 실제 숫자 이벤트 수집
            escapeProbe.Enable(); // ESC 진단 활성화
            digitProbe.Enable(); // 숫자 진단 활성화
            Debug.Log("[SettingsPriority] Test conditions: cloned InputSettings for unfocused batch, virtual keyboard, real button events, injected XP and forced full slots; PlayerPrefs unchanged."); // 시험 조건 기록
            Press(Key.Escape); // 실제 ESC 일시정지 입력
            Advance(1); // 입력 처리 대기
        } // 메서드 끝

        private static void GrantLevelExperience() // 실제 레벨업 경로 실행
        { // 메서드 시작
            int needed = Mathf.Max(1, experience.RequiredExperience - experience.CurrentExperience); // 현재 레벨 부족 경험치
            experience.AddExperience(needed); // 시험용 경험치 주입
            Require(level.IsPresentingLevelUp && experience.IsLevelUpPending && game.IsPaused, "Injected experience did not start paused level-up."); // 레벨업 화면과 정지 확인
        } // 메서드 끝

        private static void ClickSettings(string field) // 실제 설정 버튼 클릭
        { // 메서드 시작
            Button button = (Button)typeof(SettingsMenuController).GetField(field, PrivateInstance).GetValue(settings); // 직렬화 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Settings button unavailable: " + field); // 버튼 사용 가능 확인
            button.onClick.Invoke(); // 실제 설정 버튼 이벤트
        } // 메서드 끝

        private static void ClickCandidate(int index) // 실제 보상 버튼 클릭
        { // 메서드 시작
            string[] fields = { "candidateButton01", "candidateButton02", "candidateButton03" }; // 직렬화 후보 필드
            Button button = (Button)typeof(LevelUpPanelController).GetField(fields[index], PrivateInstance).GetValue(panel); // 실제 후보 버튼 조회
            Require(button != null && button.interactable && button.gameObject.activeInHierarchy, "Reward button unavailable: " + index); // 버튼 사용 가능 확인
            button.onClick.Invoke(); // 실제 보상 버튼 이벤트
        } // 메서드 끝

        private static bool IsSelectingReplacement() // 만석 슬롯 선택 상태
        { // 메서드 시작
            return (bool)typeof(SnakeLevelUpController).GetField("selectingReplacement", PrivateInstance).GetValue(level); // 실제 선택 단계 조회
        } // 메서드 끝

        private static void Press(Key key) // 가상 키 입력
        { // 메서드 시작
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); // 가상 키보드 상태 전달
        } // 메서드 끝

        private static void Release() // 가상 키 해제
        { // 메서드 시작
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); // 모든 키 해제 전달
        } // 메서드 끝

        private static void Advance(int nextStage) // 실제 프레임 대기 설정
        { // 메서드 시작
            stage = nextStage; // 다음 검증 단계 저장
            waitUntilFrame = Time.frameCount + 4; // 입력 처리를 위한 프레임 대기
        } // 메서드 끝

        private static void Require(bool condition, string message) // 검증 조건 확인
        { // 메서드 시작
            if (!condition) // 실패 조건 확인
            { // 조건 시작
                throw new InvalidOperationException(message); // 실패 상세 전달
            } // 조건 끝
        } // 메서드 끝

        private static void Finish(int code) // 시험 정리와 종료
        { // 메서드 시작
            escapeProbe?.Dispose(); // ESC 진단 해제
            digitProbe?.Dispose(); // 숫자 진단 해제
            if (keyboard != null) // 가상 키보드 확인
            { // 조건 시작
                InputSystem.RemoveDevice(keyboard); // 시험 장치 제거
            } // 조건 끝
            if (originalSettings != null) // 원래 설정 확인
            { // 조건 시작
                InputSystem.settings = originalSettings; // 입력 설정 복구
                Application.runInBackground = originalBackground; // 백그라운드 설정 복구
            } // 조건 끝
            if (testSettings != null) // 시험 설정 확인
            { // 조건 시작
                UnityEngine.Object.Destroy(testSettings); // 설정 사본 제거
            } // 조건 끝
            SessionState.SetBool(ActiveKey, false); // 진행 표식 해제
            EditorApplication.update -= Tick; // 검증 갱신 해제
            EditorApplication.Exit(code); // 종료 결과 반환
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
