using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Player
{
    [RequireComponent(typeof(SnakeBodyManager))]
    public sealed class SnakeBodyDebugControls : MonoBehaviour
    {
        [SerializeField] private bool debugControlsEnabled = true;

        private SnakeBodyManager bodyManager;

        private void Awake()
        {
            bodyManager = GetComponent<SnakeBodyManager>();
        }

        private void Update()
        {
            if (!debugControlsEnabled)
            {
                return;
            }

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame)
            {
                bodyManager.TryAddBody();
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame)
            {
                bodyManager.TryRemoveBody();
            }
        }
    }
}
