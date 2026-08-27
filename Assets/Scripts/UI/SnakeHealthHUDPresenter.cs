using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(310)]
    public sealed class SnakeHealthHUDPresenter : MonoBehaviour
    {
        [SerializeField] private SnakeHealth health;
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

        public void Bind(SnakeHealth snakeHealth, HUDController hud)
        {
            Unsubscribe();
            health = snakeHealth;
            hudController = hud;

            if (Application.isPlaying)
            {
                Subscribe();
            }

            Refresh();
        }

        private void Subscribe()
        {
            if (subscribed || health == null)
            {
                return;
            }

            health.HealthChanged += HandleHealthChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || health == null)
            {
                subscribed = false;
                return;
            }

            health.HealthChanged -= HandleHealthChanged;
            subscribed = false;
        }

        private void Refresh()
        {
            if (health == null || hudController == null)
            {
                return;
            }

            hudController.SetHealth(
                health.CurrentHealth,
                health.MaximumHealth
            );
        }

        private void HandleHealthChanged(int current, int maximum)
        {
            if (hudController != null)
            {
                hudController.SetHealth(current, maximum);
            }
        }
    }
}
