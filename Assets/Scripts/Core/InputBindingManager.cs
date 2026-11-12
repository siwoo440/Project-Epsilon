using System; // C# 기본 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Core // 프로젝트 기능 영역
{
    public sealed class InputBindingManager : MonoBehaviour // 입력·관리자 자료 구조
    {
        private const string BindingPreferencesKey = "ProjectEpsilon.InputBindings"; // 키·프로젝트 구성 선언

        public static InputBindingManager Instance { get; private set; } // 입력·관리자 구성 선언

        public event Action BindingsChanged; // 이벤트·동작 구성 선언

        private InputActionAsset inputActions; // 입력·동작 구성 선언
        private InputAction turnLeftAction; // 입력·동작 구성 선언
        private InputAction turnRightAction; // 입력·동작 구성 선언
        private InputAction boostAction; // 입력·동작 구성 선언
        private InputActionRebindingExtensions.RebindingOperation rebindOperation; // 입력·동작 구성 선언

        public bool TurnLeftPressed => turnLeftAction != null && turnLeftAction.IsPressed(); // 입력·동작 기능 선언
        public bool TurnRightPressed => turnRightAction != null && turnRightAction.IsPressed(); // 입력·동작 기능 선언
        public bool BoostPressed => boostAction != null && boostAction.IsPressed(); // 입력·동작 기능 선언
        public bool IsRebinding => rebindOperation != null; // 대상 값 반환

        private void Awake() // 초기화 기능 선언
        {
            if (Instance != null && Instance != this) // 인스턴스 조건 확인
            {
                Destroy(this); // 제거 기능 실행
                return; // 현재 기능 종료
            }

            Instance = this; // 인스턴스 값 설정
            CreateInputActions(); // 생성·입력 기능 실행
            LoadBindingOverrides(); // 불러오기 기능 실행
            inputActions.Enable(); // 입력·활성 기능 실행
        }

        private void OnDestroy() // 제거 기능 선언
        {
            if (Instance != this) // 인스턴스 조건 확인
            {
                return; // 현재 기능 종료
            }

            CancelRebind(); // 취소 기능 실행
            inputActions?.Disable(); // 입력·비활성 기능 실행

            if (inputActions != null) // 입력 조건 확인
            {
                Destroy(inputActions); // 제거·입력 기능 실행
            }

            Instance = null; // 인스턴스 값 설정
        }

        public string GetBindingDisplayName(RebindableInput input) // 조회·이름 기능 선언
        {
            InputAction action = GetAction(input); // 입력·동작 값 설정

            if (action == null || action.bindings.Count == 0) // 동작·개수 조건 확인
            {
                return "-"; // 대상 결과 반환
            }

            string effectivePath = action.bindings[0].effectivePath; // 경로·동작 값 설정

            if (string.IsNullOrEmpty(effectivePath)) // 빈 상태·경로 조건 확인
            {
                return "-"; // 대상 결과 반환
            }

            string displayName = InputControlPath.ToHumanReadableString( // 실제 키 표시 이름
                effectivePath, // 경로 항목 전달
                InputControlPath.HumanReadableStringOptions.OmitDevice // 입력·경로 처리
            ); // 앞선 항목 종료
            return LocalizeBindingName(displayName, effectivePath); // 실제 키 이름의 한국어 표시
        }

        private static string LocalizeBindingName(string value, string path) // 실제 키보드 표시 이름 정리
        {
            const string keypadPrefix = "<Keyboard>/numpad"; // 실제 숫자 키패드 경로
            if (!string.IsNullOrEmpty(path) && path.StartsWith(keypadPrefix, StringComparison.OrdinalIgnoreCase)) // 실제 숫자 키패드 확인
            {
                string keypad = path.Substring(keypadPrefix.Length).ToLowerInvariant(); // 실제 키패드 세부 키
                switch (keypad) // 실제 키패드 이름 확인
                {
                    case "enter": return "숫자 키패드 엔터"; // 실제 키패드 엔터
                    case "plus": return "숫자 키패드 +"; // 실제 키패드 더하기
                    case "minus": return "숫자 키패드 -"; // 실제 키패드 빼기
                    case "multiply": return "숫자 키패드 ×"; // 실제 키패드 곱하기
                    case "divide": return "숫자 키패드 ÷"; // 실제 키패드 나누기
                    case "period": return "숫자 키패드 ."; // 실제 키패드 소수점
                    case "comma": return "숫자 키패드 ,"; // 실제 키패드 쉼표
                    case "equals": return "숫자 키패드 ="; // 실제 키패드 등호
                    default: return "숫자 키패드 " + keypad; // 실제 키패드 숫자 보존
                }
            }
            switch (path == null ? "" : path.ToLowerInvariant()) // 실제 특수 키 경로 확인
            {
                case "<keyboard>/altgr": return "오른쪽 알트"; // 실제 보조 알트 키
                case "<keyboard>/leftmeta": // 실제 왼쪽 운영체제 키 별칭
                case "<keyboard>/leftwindows": // 실제 왼쪽 윈도 키 별칭
                case "<keyboard>/leftapple": // 실제 왼쪽 애플 키 별칭
                case "<keyboard>/leftcommand": return "왼쪽 운영체제 키"; // 실제 왼쪽 운영체제 키
                case "<keyboard>/rightmeta": // 실제 오른쪽 운영체제 키 별칭
                case "<keyboard>/rightwindows": // 실제 오른쪽 윈도 키 별칭
                case "<keyboard>/rightapple": // 실제 오른쪽 애플 키 별칭
                case "<keyboard>/rightcommand": return "오른쪽 운영체제 키"; // 실제 오른쪽 운영체제 키
                case "<keyboard>/contextmenu": return "메뉴 키"; // 실제 문맥 메뉴 키
                case "<keyboard>/imeselected": return "입력기 전환 키"; // 실제 입력기 전환 키
                case "<keyboard>/mediaplaypause": return "재생·일시정지 키"; // 실제 미디어 재생 키
                case "<keyboard>/mediarewind": return "되감기 키"; // 실제 미디어 되감기 키
                case "<keyboard>/mediaforward": return "빨리감기 키"; // 실제 미디어 빨리감기 키
                case "<keyboard>/oem1": return "특수 키 1"; // 실제 제조사 특수 키
                case "<keyboard>/oem2": return "특수 키 2"; // 실제 제조사 특수 키
                case "<keyboard>/oem3": return "특수 키 3"; // 실제 제조사 특수 키
                case "<keyboard>/oem4": return "특수 키 4"; // 실제 제조사 특수 키
                case "<keyboard>/oem5": return "특수 키 5"; // 실제 제조사 특수 키
            }
            switch (value) // 실제 일반 키 이름 확인
            {
                case "Space": return "스페이스"; // 실제 스페이스 키
                case "Spacebar": return "스페이스"; // 실제 스페이스 키 별칭
                case "Left Arrow": return "왼쪽 방향키"; // 실제 왼쪽 방향키
                case "Right Arrow": return "오른쪽 방향키"; // 실제 오른쪽 방향키
                case "Up Arrow": return "위쪽 방향키"; // 실제 위쪽 방향키
                case "Down Arrow": return "아래쪽 방향키"; // 실제 아래쪽 방향키
                case "Left Shift": return "왼쪽 시프트"; // 실제 왼쪽 시프트 키
                case "Right Shift": return "오른쪽 시프트"; // 실제 오른쪽 시프트 키
                case "Left Control": // 실제 왼쪽 컨트롤 별칭
                case "Left Ctrl": return "왼쪽 컨트롤"; // 실제 왼쪽 컨트롤 키
                case "Right Control": // 실제 오른쪽 컨트롤 별칭
                case "Right Ctrl": return "오른쪽 컨트롤"; // 실제 오른쪽 컨트롤 키
                case "Left Alt": return "왼쪽 알트"; // 실제 왼쪽 알트 키
                case "Right Alt": return "오른쪽 알트"; // 실제 오른쪽 알트 키
                case "Enter": return "엔터"; // 실제 엔터 키
                case "Tab": return "탭"; // 실제 탭 키
                case "Backspace": return "백스페이스"; // 실제 백스페이스 키
                case "Delete": return "삭제 키"; // 실제 삭제 키
                case "Insert": return "삽입 키"; // 실제 삽입 키
                case "Home": return "홈 키"; // 실제 홈 키
                case "End": return "끝 키"; // 실제 끝 키
                case "Page Up": return "이전 페이지 키"; // 실제 이전 페이지 키
                case "Page Down": return "다음 페이지 키"; // 실제 다음 페이지 키
                case "Escape": return "이스케이프"; // 실제 취소 키
                case "Caps Lock": return "대문자 잠금"; // 실제 대문자 잠금 키
                case "Num Lock": return "숫자 잠금"; // 실제 숫자 잠금 키
                case "Scroll Lock": return "스크롤 잠금"; // 실제 스크롤 잠금 키
                case "Print Screen": return "화면 캡처 키"; // 실제 화면 캡처 키
                case "Pause": return "일시정지 키"; // 실제 키보드 일시정지 키
                case "Left Bracket": return "["; // 실제 왼쪽 대괄호 키
                case "Right Bracket": return "]"; // 실제 오른쪽 대괄호 키
                case "Semicolon": return ";"; // 실제 세미콜론 키
                case "Quote": return "'"; // 실제 따옴표 키
                case "Comma": return ","; // 실제 쉼표 키
                case "Period": return "."; // 실제 마침표 키
                case "Slash": return "/"; // 실제 슬래시 키
                case "Backslash": return "\\"; // 실제 역슬래시 키
                case "Minus": return "-"; // 실제 빼기 키
                case "Equals": return "="; // 실제 등호 키
                case "Backquote": return "`"; // 실제 백틱 키
                default: return value; // 실제 문자·숫자와 기타 키 보존
            }
        }

        public void StartInteractiveRebind(RebindableInput input, Action<bool> completed) // 시작·입력 기능 선언
        {
            CancelRebind(); // 취소 기능 실행

            InputAction action = GetAction(input); // 입력·동작 값 설정

            if (action == null) // 동작 조건 확인
            {
                completed?.Invoke(false); // 대상 기능 실행
                return; // 현재 기능 종료
            }

            bool wasEnabled = action.enabled; // 활성·동작 값 설정

            if (wasEnabled) // 활성 조건 확인
            {
                action.Disable(); // 동작·비활성 기능 실행
            }

            rebindOperation = action // 동작 값 설정
                .PerformInteractiveRebinding(0) // 대상 기능 실행
                .WithControlsHavingToMatchPath("<Keyboard>/*") // 경로·키보드 기능 실행
                .WithCancelingThrough("<Keyboard>/escape") // 키보드 기능 실행
                .OnCancel(operation => // 취소 기능 실행
                {
                    FinishRebindOperation(operation, action, wasEnabled); // 종료·동작 기능 실행
                    completed?.Invoke(false); // 대상 기능 실행
                }) // 대상 처리
                .OnComplete(operation => // 완료 기능 실행
                {
                    FinishRebindOperation(operation, action, wasEnabled); // 종료·동작 기능 실행
                    SaveBindingOverrides(); // 저장 기능 실행
                    BindingsChanged?.Invoke(); // 대상 기능 실행
                    completed?.Invoke(true); // 대상 기능 실행
                }); // 대상 처리

            rebindOperation.Start(); // 시작 기능 실행
        }

        public void CancelRebind() // 취소 기능 선언
        {
            if (rebindOperation == null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            rebindOperation.Cancel(); // 취소 기능 실행
        }

        public void ResetToDefaults() // 초기화 기능 선언
        {
            CancelRebind(); // 취소 기능 실행

            foreach (InputAction action in inputActions) // 입력·동작 항목 순회
            {
                action.RemoveAllBindingOverrides(); // 동작·제거 기능 실행
            }

            PlayerPrefs.DeleteKey(BindingPreferencesKey); // 플레이어·키 기능 실행
            PlayerPrefs.Save(); // 플레이어·저장 기능 실행
            BindingsChanged?.Invoke(); // 대상 기능 실행
        }

        private void CreateInputActions() // 생성·입력 기능 선언
        {
            inputActions = ScriptableObject.CreateInstance<InputActionAsset>(); // 입력·객체 값 설정
            InputActionMap gameplayMap = inputActions.AddActionMap("Gameplay"); // 입력·동작 값 설정

            turnLeftAction = gameplayMap.AddAction( // 동작·맵 호출 시작
                "TurnLeft", // 대상 경로 또는 문구 값
                InputActionType.Button, // 입력·동작 항목 전달
                "<Keyboard>/a" // 키보드 경로 또는 문구 값
            ); // 앞선 항목 종료
            turnLeftAction.AddBinding("<Keyboard>/leftArrow"); // 동작·추가 기능 실행

            turnRightAction = gameplayMap.AddAction( // 동작·맵 호출 시작
                "TurnRight", // 대상 경로 또는 문구 값
                InputActionType.Button, // 입력·동작 항목 전달
                "<Keyboard>/d" // 키보드 경로 또는 문구 값
            ); // 앞선 항목 종료
            turnRightAction.AddBinding("<Keyboard>/rightArrow"); // 동작·추가 기능 실행

            boostAction = gameplayMap.AddAction( // 동작·맵 호출 시작
                "Boost", // 대상 경로 또는 문구 값
                InputActionType.Button, // 입력·동작 항목 전달
                "<Keyboard>/space" // 기본 가속 키
            ); // 앞선 항목 종료
        }

        private InputAction GetAction(RebindableInput input) // 입력·동작 기능 선언
        {
            return input switch // 입력 결과 반환
            {
                RebindableInput.TurnLeft => turnLeftAction, // 입력·동작 항목 전달
                RebindableInput.TurnRight => turnRightAction, // 입력·동작 항목 전달
                RebindableInput.Boost => boostAction, // 입력·동작 항목 전달
                _ => null // 대상 처리
            };
        }

        private void LoadBindingOverrides() // 불러오기 기능 선언
        {
            if (!PlayerPrefs.HasKey(BindingPreferencesKey)) // 플레이어·존재 여부 조건 확인
            {
                return; // 현재 기능 종료
            }

            string json = PlayerPrefs.GetString(BindingPreferencesKey, string.Empty); // 플레이어·조회 값 설정

            if (string.IsNullOrWhiteSpace(json)) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            try // 오류 발생 가능 작업 시작
            {
                inputActions.LoadBindingOverridesFromJson(json); // 입력·불러오기 기능 실행
            }
            catch (Exception exception) // 대상 오류 처리
            {
                Debug.LogWarning( // 대상 호출 시작
                    $"[Project Epsilon] 저장된 키 설정을 불러오지 못했습니다. 기본값을 사용합니다.\n{exception.Message}" // 프로젝트 처리
                ); // 앞선 항목 종료

                PlayerPrefs.DeleteKey(BindingPreferencesKey); // 플레이어·키 기능 실행
                PlayerPrefs.Save(); // 플레이어·저장 기능 실행
            }
        }

        private void SaveBindingOverrides() // 저장 기능 선언
        {
            string json = inputActions.SaveBindingOverridesAsJson(); // 입력·저장 값 설정
            PlayerPrefs.SetString(BindingPreferencesKey, json); // 플레이어·설정 기능 실행
            PlayerPrefs.Save(); // 플레이어·저장 기능 실행
        }

        private void FinishRebindOperation( // 종료 호출 시작
            InputActionRebindingExtensions.RebindingOperation operation, // 입력·동작 항목 전달
            InputAction action, // 입력·동작 항목 전달
            bool wasEnabled // 활성 처리
        ) // 앞선 항목 종료
        {
            operation.Dispose(); // 대상 기능 실행
            rebindOperation = null; // 대상 값 설정

            if (wasEnabled) // 활성 조건 확인
            {
                action.Enable(); // 동작·활성 기능 실행
            }
        }
    }
}
