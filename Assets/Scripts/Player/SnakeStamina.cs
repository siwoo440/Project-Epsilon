using System;
using ProjectEpsilon.Core;
using UnityEngine;

namespace ProjectEpsilon.Player
{
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(PlayerInputReader))]
    public sealed class SnakeStamina : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private float maximumStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float boostMultiplier = 1.5f;
        [SerializeField] private float drainPerSecond = 25f;
        [SerializeField] private float recoveryPerSecond = 20f;
        [SerializeField] private float recoveryDelay = 1f;

        private float recoveryBlockedUntil;
        private bool exhaustedUntilRelease;
        private bool isBoosting;

        public event Action<int, int> StaminaChanged;
        public event Action<bool> BoostStateChanged;

        public float CurrentStamina => currentStamina;
        public float MaximumStamina => maximumStamina;
        public bool IsBoosting => isBoosting;
        public float CurrentSpeedMultiplier => isBoosting ? boostMultiplier : 1f;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            NormalizeValues();
        }

        private void Start()
        {
            NotifyStaminaChanged();
            SetBoosting(false);
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                SetBoosting(false);
                return;
            }

            bool boostHeld = inputReader != null && inputReader.BoostPressed;

            if (!boostHeld)
            {
                exhaustedUntilRelease = false;
            }

            bool canBoost =
                boostHeld &&
                !exhaustedUntilRelease &&
                currentStamina > 0.0001f;

            if (canBoost)
            {
                TickBoost();
                return;
            }

            SetBoosting(false);
            TickRecovery();
        }

        public void Configure(
            PlayerInputReader reader,
            float maximum,
            float multiplier,
            float drainRate,
            float recoveryRate,
            float delay
        )
        {
            inputReader = reader;
            maximumStamina = Mathf.Max(1f, maximum);
            boostMultiplier = Mathf.Max(1f, multiplier);
            drainPerSecond = Mathf.Max(0.01f, drainRate);
            recoveryPerSecond = Mathf.Max(0f, recoveryRate);
            recoveryDelay = Mathf.Max(0f, delay);

            ResetStamina();
        }

        public void ResetStamina()
        {
            NormalizeValues();
            currentStamina = maximumStamina;
            recoveryBlockedUntil = 0f;
            exhaustedUntilRelease = false;
            SetBoosting(false);
            NotifyStaminaChanged();
        }

        private void TickBoost()
        {
            SetBoosting(true);
            recoveryBlockedUntil = Time.time + recoveryDelay;

            float previous = currentStamina;
            currentStamina = Mathf.Max(
                0f,
                currentStamina - (drainPerSecond * Time.deltaTime)
            );

            if (!Mathf.Approximately(previous, currentStamina))
            {
                NotifyStaminaChanged();
            }

            if (currentStamina <= 0.0001f)
            {
                currentStamina = 0f;
                exhaustedUntilRelease = true;
                SetBoosting(false);
                NotifyStaminaChanged();
            }
        }

        private void TickRecovery()
        {
            if (Time.time < recoveryBlockedUntil)
            {
                return;
            }

            if (currentStamina >= maximumStamina || recoveryPerSecond <= 0f)
            {
                return;
            }

            float previous = currentStamina;
            currentStamina = Mathf.Min(
                maximumStamina,
                currentStamina + (recoveryPerSecond * Time.deltaTime)
            );

            if (!Mathf.Approximately(previous, currentStamina))
            {
                NotifyStaminaChanged();
            }
        }

        private void NormalizeValues()
        {
            maximumStamina = Mathf.Max(1f, maximumStamina);
            currentStamina = Mathf.Clamp(
                currentStamina,
                0f,
                maximumStamina
            );
            boostMultiplier = Mathf.Max(1f, boostMultiplier);
            drainPerSecond = Mathf.Max(0.01f, drainPerSecond);
            recoveryPerSecond = Mathf.Max(0f, recoveryPerSecond);
            recoveryDelay = Mathf.Max(0f, recoveryDelay);
        }

        private void SetBoosting(bool value)
        {
            if (isBoosting == value)
            {
                return;
            }

            isBoosting = value;
            BoostStateChanged?.Invoke(isBoosting);
        }

        private void NotifyStaminaChanged()
        {
            StaminaChanged?.Invoke(
                Mathf.RoundToInt(currentStamina),
                Mathf.RoundToInt(maximumStamina)
            );
        }
    }
}
