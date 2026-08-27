using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Core
{
    public sealed class InputBindingManager : MonoBehaviour
    {
        private const string BindingPreferencesKey = "ProjectEpsilon.InputBindings";

        public static InputBindingManager Instance { get; private set; }

        public event Action BindingsChanged;

        private InputActionAsset inputActions;
        private InputAction turnLeftAction;
        private InputAction turnRightAction;
        private InputAction boostAction;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;

        public bool TurnLeftPressed => turnLeftAction != null && turnLeftAction.IsPressed();
        public bool TurnRightPressed => turnRightAction != null && turnRightAction.IsPressed();
        public bool BoostPressed => boostAction != null && boostAction.IsPressed();
        public bool IsRebinding => rebindOperation != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            CreateInputActions();
            LoadBindingOverrides();
            inputActions.Enable();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            CancelRebind();
            inputActions?.Disable();

            if (inputActions != null)
            {
                Destroy(inputActions);
            }

            Instance = null;
        }

        public string GetBindingDisplayName(RebindableInput input)
        {
            InputAction action = GetAction(input);

            if (action == null || action.bindings.Count == 0)
            {
                return "-";
            }

            string effectivePath = action.bindings[0].effectivePath;

            if (string.IsNullOrEmpty(effectivePath))
            {
                return "-";
            }

            return InputControlPath.ToHumanReadableString(
                effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }

        public void StartInteractiveRebind(RebindableInput input, Action<bool> completed)
        {
            CancelRebind();

            InputAction action = GetAction(input);

            if (action == null)
            {
                completed?.Invoke(false);
                return;
            }

            bool wasEnabled = action.enabled;

            if (wasEnabled)
            {
                action.Disable();
            }

            rebindOperation = action
                .PerformInteractiveRebinding(0)
                .WithControlsHavingToMatchPath("<Keyboard>/*")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation =>
                {
                    FinishRebindOperation(operation, action, wasEnabled);
                    completed?.Invoke(false);
                })
                .OnComplete(operation =>
                {
                    FinishRebindOperation(operation, action, wasEnabled);
                    SaveBindingOverrides();
                    BindingsChanged?.Invoke();
                    completed?.Invoke(true);
                });

            rebindOperation.Start();
        }

        public void CancelRebind()
        {
            if (rebindOperation == null)
            {
                return;
            }

            rebindOperation.Cancel();
        }

        public void ResetToDefaults()
        {
            CancelRebind();

            foreach (InputAction action in inputActions)
            {
                action.RemoveAllBindingOverrides();
            }

            PlayerPrefs.DeleteKey(BindingPreferencesKey);
            PlayerPrefs.Save();
            BindingsChanged?.Invoke();
        }

        private void CreateInputActions()
        {
            inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap gameplayMap = inputActions.AddActionMap("Gameplay");

            turnLeftAction = gameplayMap.AddAction(
                "TurnLeft",
                InputActionType.Button,
                "<Keyboard>/a"
            );
            turnLeftAction.AddBinding("<Keyboard>/leftArrow");

            turnRightAction = gameplayMap.AddAction(
                "TurnRight",
                InputActionType.Button,
                "<Keyboard>/d"
            );
            turnRightAction.AddBinding("<Keyboard>/rightArrow");

            boostAction = gameplayMap.AddAction(
                "Boost",
                InputActionType.Button,
                "<Keyboard>/leftShift"
            );
            boostAction.AddBinding("<Keyboard>/rightShift");
        }

        private InputAction GetAction(RebindableInput input)
        {
            return input switch
            {
                RebindableInput.TurnLeft => turnLeftAction,
                RebindableInput.TurnRight => turnRightAction,
                RebindableInput.Boost => boostAction,
                _ => null
            };
        }

        private void LoadBindingOverrides()
        {
            if (!PlayerPrefs.HasKey(BindingPreferencesKey))
            {
                return;
            }

            string json = PlayerPrefs.GetString(BindingPreferencesKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            try
            {
                inputActions.LoadBindingOverridesFromJson(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[Project Epsilon] 저장된 키 설정을 불러오지 못했습니다. 기본값을 사용합니다.\n{exception.Message}"
                );

                PlayerPrefs.DeleteKey(BindingPreferencesKey);
                PlayerPrefs.Save();
            }
        }

        private void SaveBindingOverrides()
        {
            string json = inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(BindingPreferencesKey, json);
            PlayerPrefs.Save();
        }

        private void FinishRebindOperation(
            InputActionRebindingExtensions.RebindingOperation operation,
            InputAction action,
            bool wasEnabled
        )
        {
            operation.Dispose();
            rebindOperation = null;

            if (wasEnabled)
            {
                action.Enable();
            }
        }
    }
}
