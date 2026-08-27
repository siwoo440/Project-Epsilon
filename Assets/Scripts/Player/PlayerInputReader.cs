using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        public float TurnInput
        {
            get
            {
                InputBindingManager bindingManager = InputBindingManager.Instance;

                if (bindingManager != null)
                {
                    float left = bindingManager.TurnLeftPressed ? -1f : 0f;
                    float right = bindingManager.TurnRightPressed ? 1f : 0f;

                    return Mathf.Clamp(left + right, -1f, 1f);
                }

                return GetFallbackTurnInput();
            }
        }

        public bool BoostPressed
        {
            get
            {
                InputBindingManager bindingManager = InputBindingManager.Instance;

                if (bindingManager != null)
                {
                    return bindingManager.BoostPressed;
                }

                Keyboard keyboard = Keyboard.current;

                return keyboard != null &&
                    (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
            }
        }

        private static float GetFallbackTurnInput()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return 0f;
            }

            float left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f;
            float right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f;

            return Mathf.Clamp(left + right, -1f, 1f);
        }
    }
}
