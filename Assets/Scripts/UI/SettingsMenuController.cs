using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.UI; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    public sealed class SettingsMenuController : MonoBehaviour // 설정·제어기 자료 구조
    {
        [SerializeField] private GameObject settingsPanel; // 필드·게임 설정
        [SerializeField] private Button openButton; // 필드·버튼 설정
        [SerializeField] private Button closeButton; // 필드·버튼 설정
        [SerializeField] private Button resetButton; // 필드·버튼 설정
        [SerializeField] private Button turnLeftButton; // 필드·버튼 설정
        [SerializeField] private Button turnRightButton; // 필드·버튼 설정
        [SerializeField] private Button boostButton; // 필드·버튼 설정
        [SerializeField] private Text turnLeftBindingText; // 필드·문구 설정
        [SerializeField] private Text turnRightBindingText; // 필드·문구 설정
        [SerializeField] private Text boostBindingText; // 필드·문구 설정
        [SerializeField] private Text statusText; // 필드·문구 설정

        private bool wasPlayingBeforeOpen; // 열기 구성 선언
        public bool IsOpen => settingsPanel != null && settingsPanel.activeInHierarchy; // 설정 화면 상태

        private void Start() // 시작 기능 선언
        {
            RegisterListeners(); // 등록 기능 실행

            if (settingsPanel != null) // 설정·화면 조건 확인
            {
                settingsPanel.SetActive(false); // 설정·화면 기능 실행
            }

            if (InputBindingManager.Instance != null) // 입력·관리자 조건 확인
            {
                InputBindingManager.Instance.BindingsChanged += RefreshBindingTexts; // 입력·관리자 값 설정
            }

            RefreshBindingTexts(); // 갱신 기능 실행
            SetStatus(string.Empty); // 설정·상태 기능 실행
        }

        private void OnDestroy() // 제거 기능 선언
        {
            UnregisterListeners(); // 등록 해제 기능 실행

            if (InputBindingManager.Instance != null) // 입력·관리자 조건 확인
            {
                InputBindingManager.Instance.BindingsChanged -= RefreshBindingTexts; // 입력·관리자 값 설정
            }
        }

        public void Bind( // 연결 호출 시작
            GameObject panel, // 게임·객체 항목 전달
            Button open, // 버튼·열기 항목 전달
            Button close, // 버튼·닫기 항목 전달
            Button reset, // 버튼·초기화 항목 전달
            Button left, // 버튼 항목 전달
            Button right, // 버튼 항목 전달
            Button boost, // 버튼 항목 전달
            Text leftText, // 문구 항목 전달
            Text rightText, // 문구 항목 전달
            Text boostText, // 문구 항목 전달
            Text status // 문구·상태 처리
        ) // 앞선 항목 종료
        {
            settingsPanel = panel; // 설정·화면 값 설정
            openButton = open; // 열기·버튼 값 설정
            closeButton = close; // 닫기·버튼 값 설정
            resetButton = reset; // 초기화·버튼 값 설정
            turnLeftButton = left; // 버튼 값 설정
            turnRightButton = right; // 버튼 값 설정
            boostButton = boost; // 버튼 값 설정
            turnLeftBindingText = leftText; // 문구 값 설정
            turnRightBindingText = rightText; // 문구 값 설정
            boostBindingText = boostText; // 문구 값 설정
            statusText = status; // 상태·문구 값 설정
        }

        private void RegisterListeners() // 등록 기능 선언
        {
            openButton?.onClick.AddListener(OpenSettings); // 열기·버튼 기능 실행
            closeButton?.onClick.AddListener(CloseSettings); // 닫기·버튼 기능 실행
            resetButton?.onClick.AddListener(ResetBindings); // 초기화·버튼 기능 실행
            turnLeftButton?.onClick.AddListener(RebindTurnLeft); // 버튼·추가 기능 실행
            turnRightButton?.onClick.AddListener(RebindTurnRight); // 버튼·추가 기능 실행
            boostButton?.onClick.AddListener(RebindBoost); // 버튼·추가 기능 실행
        }

        private void UnregisterListeners() // 등록 해제 기능 선언
        {
            openButton?.onClick.RemoveListener(OpenSettings); // 열기·버튼 기능 실행
            closeButton?.onClick.RemoveListener(CloseSettings); // 닫기·버튼 기능 실행
            resetButton?.onClick.RemoveListener(ResetBindings); // 초기화·버튼 기능 실행
            turnLeftButton?.onClick.RemoveListener(RebindTurnLeft); // 버튼·제거 기능 실행
            turnRightButton?.onClick.RemoveListener(RebindTurnRight); // 버튼·제거 기능 실행
            boostButton?.onClick.RemoveListener(RebindBoost); // 버튼·제거 기능 실행
        }

        public void OpenSettings() // 실제 메뉴에서 설정 열기
        {
            if (settingsPanel == null) // 설정·화면 조건 확인
            {
                return; // 현재 기능 종료
            }

            GameManager gameManager = GameManager.Instance; // 게임·관리자 값 설정
            wasPlayingBeforeOpen = gameManager != null && gameManager.IsPlaying; // 열기·게임 처리

            if (wasPlayingBeforeOpen) // 열기 조건 확인
            {
                gameManager.PauseGame(); // 게임·관리자 기능 실행
            }

            settingsPanel.transform.SetAsLastSibling(); // 설정 화면 표시 순서 우선
            settingsPanel.SetActive(true); // 설정·화면 기능 실행
            RefreshBindingTexts(); // 갱신 기능 실행
            SetStatus(string.Empty); // 설정·상태 기능 실행
        }

        public void CloseSettings() // 실제 메뉴에서 설정 닫기
        {
            InputBindingManager.Instance?.CancelRebind(); // 입력·관리자 기능 실행

            if (settingsPanel != null) // 설정·화면 조건 확인
            {
                settingsPanel.SetActive(false); // 설정·화면 기능 실행
            }

            GameManager gameManager = GameManager.Instance; // 게임·관리자 값 설정

            if (wasPlayingBeforeOpen && gameManager != null && gameManager.IsPaused) // 열기·게임 조건 확인
            {
                gameManager.ResumeGame(); // 게임·관리자 기능 실행
            }

            wasPlayingBeforeOpen = false; // 열기 값 설정
        }

        private void RebindTurnLeft() // 대상 기능 선언
        {
            BeginRebind(RebindableInput.TurnLeft); // 시작·입력 기능 실행
        }

        private void RebindTurnRight() // 대상 기능 선언
        {
            BeginRebind(RebindableInput.TurnRight); // 시작·입력 기능 실행
        }

        private void RebindBoost() // 대상 기능 선언
        {
            BeginRebind(RebindableInput.Boost); // 시작·입력 기능 실행
        }

        private void BeginRebind(RebindableInput input) // 시작·입력 기능 선언
        {
            InputBindingManager bindingManager = InputBindingManager.Instance; // 입력·관리자 값 설정

            if (bindingManager == null) // 관리자 조건 확인
            {
                SetStatus("입력 관리자를 찾을 수 없습니다."); // 설정·상태 기능 실행
                return; // 현재 기능 종료
            }

            SetBindingButtonsInteractable(false); // 설정 기능 실행
            SetStatus("새 키를 누르세요. ESC로 취소합니다."); // 설정·상태 기능 실행

            bindingManager.StartInteractiveRebind( // 관리자·시작 호출 시작
                input, // 입력 항목 전달
                success => // 성공 처리
                {
                    SetBindingButtonsInteractable(true); // 설정 기능 실행
                    RefreshBindingTexts(); // 갱신 기능 실행
                    SetStatus(success ? "키 설정이 저장되었습니다." : "키 변경을 취소했습니다."); // 설정·상태 기능 실행
                }
            ); // 앞선 항목 종료
        }

        private void ResetBindings() // 초기화 기능 선언
        {
            InputBindingManager bindingManager = InputBindingManager.Instance; // 입력·관리자 값 설정

            if (bindingManager == null) // 관리자 조건 확인
            {
                SetStatus("입력 관리자를 찾을 수 없습니다."); // 설정·상태 기능 실행
                return; // 현재 기능 종료
            }

            bindingManager.ResetToDefaults(); // 관리자·초기화 기능 실행
            RefreshBindingTexts(); // 갱신 기능 실행
            SetStatus("기본 키 설정으로 복원했습니다."); // 설정·상태 기능 실행
        }

        private void RefreshBindingTexts() // 갱신 기능 선언
        {
            InputBindingManager bindingManager = InputBindingManager.Instance; // 입력·관리자 값 설정

            if (bindingManager == null) // 관리자 조건 확인
            {
                SetBindingText(turnLeftBindingText, "-"); // 설정·문구 기능 실행
                SetBindingText(turnRightBindingText, "-"); // 설정·문구 기능 실행
                SetBindingText(boostBindingText, "-"); // 설정·문구 기능 실행
                return; // 현재 기능 종료
            }

            SetBindingText( // 설정·문구 호출 시작
                turnLeftBindingText, // 문구 항목 전달
                bindingManager.GetBindingDisplayName(RebindableInput.TurnLeft) // 관리자·조회 기능 실행
            ); // 앞선 항목 종료

            SetBindingText( // 설정·문구 호출 시작
                turnRightBindingText, // 문구 항목 전달
                bindingManager.GetBindingDisplayName(RebindableInput.TurnRight) // 관리자·조회 기능 실행
            ); // 앞선 항목 종료

            SetBindingText( // 설정·문구 호출 시작
                boostBindingText, // 문구 항목 전달
                bindingManager.GetBindingDisplayName(RebindableInput.Boost) // 관리자·조회 기능 실행
            ); // 앞선 항목 종료
        }

        private void SetBindingButtonsInteractable(bool interactable) // 설정 기능 선언
        {
            if (turnLeftButton != null) // 버튼 조건 확인
            {
                turnLeftButton.interactable = interactable; // 버튼 값 설정
            }

            if (turnRightButton != null) // 버튼 조건 확인
            {
                turnRightButton.interactable = interactable; // 버튼 값 설정
            }

            if (boostButton != null) // 버튼 조건 확인
            {
                boostButton.interactable = interactable; // 버튼 값 설정
            }

            if (resetButton != null) // 초기화·버튼 조건 확인
            {
                resetButton.interactable = interactable; // 초기화·버튼 값 설정
            }
        }

        private static void SetBindingText(Text text, string value) // 설정·문구 기능 선언
        {
            if (text != null) // 문구 조건 확인
            {
                text.text = value; // 문구·값 값 설정
            }
        }

        private void SetStatus(string message) // 설정·상태 기능 선언
        {
            if (statusText != null) // 상태·문구 조건 확인
            {
                statusText.text = message; // 상태·문구 값 설정
            }
        }
    }
}
