using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(320)]
    public sealed class SnakeStaminaHUDPresenter : MonoBehaviour
    {
        [SerializeField] private SnakeStamina stamina;
        [SerializeField] private HUDController hudController;

        private bool subscribed;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Bind(SnakeStamina snakeStamina, HUDController hud)
        {
            Unsubscribe();
            stamina = snakeStamina;
            hudController = hud;

            if (Application.isPlaying)
            {
                Subscribe();
            }

            Refresh();
        }

        private void Subscribe()
        {
            if (subscribed || stamina == null)
            {
                return;
            }

            stamina.StaminaChanged += HandleStaminaChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || stamina == null)
            {
                subscribed = false;
                return;
            }

            stamina.StaminaChanged -= HandleStaminaChanged;
            subscribed = false;
        }

        private void Refresh()
        {
            if (stamina == null || hudController == null)
            {
                return;
            }

            hudController.SetStamina(
                Mathf.RoundToInt(stamina.CurrentStamina),
                Mathf.RoundToInt(stamina.MaximumStamina)
            );
        }

        private void HandleStaminaChanged(int current, int maximum)
        {
            if (hudController != null)
            {
                hudController.SetStamina(current, maximum);
            }
        }
    }
}
