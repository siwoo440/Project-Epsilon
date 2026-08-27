using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class SnakeMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float turnSpeed = 145f;
        [SerializeField] private SnakeStamina stamina;

        private PlayerInputReader inputReader;

        public float MoveSpeed => moveSpeed;
        public float TurnSpeed => turnSpeed;
        public float CurrentMoveSpeed =>
            moveSpeed * (stamina == null ? 1f : stamina.CurrentSpeedMultiplier);

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();

            if (stamina == null)
            {
                stamina = GetComponent<SnakeStamina>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Rotate();
            MoveForward();
        }

        public void BindStamina(SnakeStamina staminaController)
        {
            stamina = staminaController;
        }

        private void Rotate()
        {
            float turnInput = inputReader.TurnInput;
            float rotationAmount = -turnInput * turnSpeed * Time.deltaTime;

            transform.Rotate(0f, 0f, rotationAmount);
        }

        private void MoveForward()
        {
            transform.position +=
                transform.up * (CurrentMoveSpeed * Time.deltaTime);
        }
    }
}
