using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(300)]
    public sealed class SnakeBodyHUDPresenter : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
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

        public void Bind(SnakeBodyManager manager, HUDController hud)
        {
            Unsubscribe();
            bodyManager = manager;
            hudController = hud;

            if (Application.isPlaying)
            {
                Subscribe();
            }

            Refresh();
        }

        private void Subscribe()
        {
            if (subscribed || bodyManager == null)
            {
                return;
            }

            bodyManager.BodyCountChanged += HandleBodyCountChanged;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || bodyManager == null)
            {
                subscribed = false;
                return;
            }

            bodyManager.BodyCountChanged -= HandleBodyCountChanged;
            subscribed = false;
        }

        private void Refresh()
        {
            if (bodyManager == null || hudController == null)
            {
                return;
            }

            hudController.SetBodyCount(
                bodyManager.CurrentBodyCount,
                bodyManager.MaximumBodyCount
            );
        }

        private void HandleBodyCountChanged(int current, int maximum)
        {
            if (hudController != null)
            {
                hudController.SetBodyCount(current, maximum);
            }
        }
    }
}
