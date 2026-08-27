using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeDamageDebugControls : MonoBehaviour
    {
        [SerializeField] private SnakeHealth health;
        [SerializeField] private SnakeSelfCollision selfCollision;
        [SerializeField] private bool debugControlsEnabled = true;

        public void Bind(SnakeHealth snakeHealth, SnakeSelfCollision collision)
        {
            health = snakeHealth;
            selfCollision = collision;
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

            if (keyboard.pKey.wasPressedThisFrame)
            {
                health?.TakeDamage(25);
            }

            if (keyboard.oKey.wasPressedThisFrame)
            {
                health?.TakeDamage(120);
            }

            if (keyboard.kKey.wasPressedThisFrame)
            {
                selfCollision?.TryApplySelfCollisionPenalty();
            }
        }
    }
}
