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
                Refresh(); // 재활성화 시 현재 몸통 반영
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
                bodyManager.EarnedBodyCount, // 성장 최대 몸통 수
                bodyManager.MaximumBodyCount
            );
        }

        private void HandleBodyCountChanged(int current, int maximum)
        {
            if (hudController != null && bodyManager != null) // 표시 및 몸통 연결 확인
            {
                hudController.SetBodyCount(current, bodyManager.EarnedBodyCount, maximum); // 현재 수와 두 상한 표시
            }
        }
    }
}
