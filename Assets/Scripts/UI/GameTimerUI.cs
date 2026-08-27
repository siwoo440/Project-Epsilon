using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private Text timerText;

        private float elapsedSeconds;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            elapsedSeconds += Time.deltaTime;
            Refresh();
        }

        public void Bind(Text nextTimerText)
        {
            timerText = nextTimerText;
            Refresh();
        }

        public void ResetTimer()
        {
            elapsedSeconds = 0f;
            Refresh();
        }

        private void Refresh()
        {
            if (timerText != null)
            {
                timerText.text = HUDTextFormatter.FormatTime(elapsedSeconds);
            }
        }
    }
}
