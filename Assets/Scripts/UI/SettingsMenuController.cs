using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class SettingsMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button turnLeftButton;
        [SerializeField] private Button turnRightButton;
        [SerializeField] private Button boostButton;
        [SerializeField] private Text turnLeftBindingText;
        [SerializeField] private Text turnRightBindingText;
        [SerializeField] private Text boostBindingText;
        [SerializeField] private Text statusText;

        private bool wasPlayingBeforeOpen;

        private void Start()
        {
            RegisterListeners();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (InputBindingManager.Instance != null)
            {
                InputBindingManager.Instance.BindingsChanged += RefreshBindingTexts;
            }

            RefreshBindingTexts();
            SetStatus(string.Empty);
        }

        private void OnDestroy()
        {
            UnregisterListeners();

            if (InputBindingManager.Instance != null)
            {
                InputBindingManager.Instance.BindingsChanged -= RefreshBindingTexts;
            }
        }

        public void Bind(
            GameObject panel,
            Button open,
            Button close,
            Button reset,
            Button left,
            Button right,
            Button boost,
            Text leftText,
            Text rightText,
            Text boostText,
            Text status
        )
        {
            settingsPanel = panel;
            openButton = open;
            closeButton = close;
            resetButton = reset;
            turnLeftButton = left;
            turnRightButton = right;
            boostButton = boost;
            turnLeftBindingText = leftText;
            turnRightBindingText = rightText;
            boostBindingText = boostText;
            statusText = status;
        }

        private void RegisterListeners()
        {
            openButton?.onClick.AddListener(OpenSettings);
            closeButton?.onClick.AddListener(CloseSettings);
            resetButton?.onClick.AddListener(ResetBindings);
            turnLeftButton?.onClick.AddListener(RebindTurnLeft);
            turnRightButton?.onClick.AddListener(RebindTurnRight);
            boostButton?.onClick.AddListener(RebindBoost);
        }

        private void UnregisterListeners()
        {
            openButton?.onClick.RemoveListener(OpenSettings);
            closeButton?.onClick.RemoveListener(CloseSettings);
            resetButton?.onClick.RemoveListener(ResetBindings);
            turnLeftButton?.onClick.RemoveListener(RebindTurnLeft);
            turnRightButton?.onClick.RemoveListener(RebindTurnRight);
            boostButton?.onClick.RemoveListener(RebindBoost);
        }

        private void OpenSettings()
        {
            if (settingsPanel == null)
            {
                return;
            }

            GameManager gameManager = GameManager.Instance;
            wasPlayingBeforeOpen = gameManager != null && gameManager.IsPlaying;

            if (wasPlayingBeforeOpen)
            {
                gameManager.PauseGame();
            }

            settingsPanel.SetActive(true);
            RefreshBindingTexts();
            SetStatus(string.Empty);
        }

        private void CloseSettings()
        {
            InputBindingManager.Instance?.CancelRebind();

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            GameManager gameManager = GameManager.Instance;

            if (wasPlayingBeforeOpen && gameManager != null && gameManager.IsPaused)
            {
                gameManager.ResumeGame();
            }

            wasPlayingBeforeOpen = false;
        }

        private void RebindTurnLeft()
        {
            BeginRebind(RebindableInput.TurnLeft);
        }

        private void RebindTurnRight()
        {
            BeginRebind(RebindableInput.TurnRight);
        }

        private void RebindBoost()
        {
            BeginRebind(RebindableInput.Boost);
        }

        private void BeginRebind(RebindableInput input)
        {
            InputBindingManager bindingManager = InputBindingManager.Instance;

            if (bindingManager == null)
            {
                SetStatus("입력 관리자를 찾을 수 없습니다.");
                return;
            }

            SetBindingButtonsInteractable(false);
            SetStatus("새 키를 누르세요. ESC로 취소합니다.");

            bindingManager.StartInteractiveRebind(
                input,
                success =>
                {
                    SetBindingButtonsInteractable(true);
                    RefreshBindingTexts();
                    SetStatus(success ? "키 설정이 저장되었습니다." : "키 변경을 취소했습니다.");
                }
            );
        }

        private void ResetBindings()
        {
            InputBindingManager bindingManager = InputBindingManager.Instance;

            if (bindingManager == null)
            {
                SetStatus("입력 관리자를 찾을 수 없습니다.");
                return;
            }

            bindingManager.ResetToDefaults();
            RefreshBindingTexts();
            SetStatus("기본 키 설정으로 복원했습니다.");
        }

        private void RefreshBindingTexts()
        {
            InputBindingManager bindingManager = InputBindingManager.Instance;

            if (bindingManager == null)
            {
                SetBindingText(turnLeftBindingText, "-");
                SetBindingText(turnRightBindingText, "-");
                SetBindingText(boostBindingText, "-");
                return;
            }

            SetBindingText(
                turnLeftBindingText,
                bindingManager.GetBindingDisplayName(RebindableInput.TurnLeft)
            );

            SetBindingText(
                turnRightBindingText,
                bindingManager.GetBindingDisplayName(RebindableInput.TurnRight)
            );

            SetBindingText(
                boostBindingText,
                bindingManager.GetBindingDisplayName(RebindableInput.Boost)
            );
        }

        private void SetBindingButtonsInteractable(bool interactable)
        {
            if (turnLeftButton != null)
            {
                turnLeftButton.interactable = interactable;
            }

            if (turnRightButton != null)
            {
                turnRightButton.interactable = interactable;
            }

            if (boostButton != null)
            {
                boostButton.interactable = interactable;
            }

            if (resetButton != null)
            {
                resetButton.interactable = interactable;
            }
        }

        private static void SetBindingText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
