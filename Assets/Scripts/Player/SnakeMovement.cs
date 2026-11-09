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

        [Header("Merge Movement")]
        [SerializeField] private bool mergeMovementMode;
        [Range(0.1f, 1f)]
        [SerializeField] private float mergeSpeedMultiplier = 0.7f;

        private PlayerInputReader inputReader;

        public float BaseMoveSpeed => moveSpeed; // 캐릭터 보정 전 기본 이동 속도
        public float MoveSpeed => moveSpeed * CharacterRunInitializer.MoveSpeedMultiplier; // 캐릭터 이동 배율 적용
        public float TurnSpeed => turnSpeed * CharacterRunInitializer.TurnSpeedMultiplier; // 캐릭터 회전 배율 적용
        public bool IsMergeMovementMode => mergeMovementMode;

        public float CurrentMoveSpeed =>
            MoveSpeed * // 캐릭터 이동 속도 적용
            ProjectEpsilon.World.PrototypeSpecialZone.SpeedMultiplierAt(transform.position) * // 현재 지역 감속 적용
            (stamina == null
                ? 1f
                : stamina.CurrentSpeedMultiplier); // 합성 감속은 전체 시간에만 적용

        private void Awake()
        {
            inputReader =
                GetComponent<PlayerInputReader>();

            if (stamina == null)
            {
                stamina =
                    GetComponent<SnakeStamina>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            if (!mergeMovementMode)
            {
                Rotate();
            }

            MoveForward();
        }

        public void BindStamina(
            SnakeStamina staminaController
        )
        {
            stamina =
                staminaController;
        }

        public void SetMergeMovementMode(
            bool active,
            float speedMultiplier = 0.7f
        )
        {
            mergeMovementMode = active;
            mergeSpeedMultiplier =
                Mathf.Clamp(
                    speedMultiplier,
                    0.1f,
                    1f
                );
        }

        private void Rotate()
        {
            float turnInput =
                inputReader.TurnInput;

            float rotationAmount =
                -turnInput *
                TurnSpeed * // 캐릭터 회전 속도 적용
                Time.deltaTime;

            transform.Rotate(
                0f,
                0f,
                rotationAmount
            );
        }

        private void MoveForward()
        {
            transform.position +=
                transform.up *
                (CurrentMoveSpeed *
                Time.deltaTime);
        }
    }
}
