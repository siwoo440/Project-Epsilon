using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class LevelUpPanelController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text growthText;
        [SerializeField] private Button continueButton;

        private bool buttonBound;

        public event Action ContinueRequested;

        private void Awake()
        {
            BindButton();
        }

        private void OnEnable()
        {
            BindButton();
        }

        private void OnDisable()
        {
            UnbindButton();
        }

        private void OnDestroy()
        {
            UnbindButton();
        }

        public void Configure(
            Text title,
            Text level,
            Text growth,
            Button continueControl
        )
        {
            UnbindButton();

            titleText = title;
            levelText = level;
            growthText = growth;
            continueButton = continueControl;

            if (Application.isPlaying)
            {
                BindButton();
            }
        }

        public void Show(
            int level,
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored
        )
        {
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "LEVEL UP!";
            }

            if (levelText != null)
            {
                levelText.text =
                    $"Lv. {Mathf.Max(1, level)}";
            }

            if (growthText != null)
            {
                growthText.text = BuildGrowthText(
                    bodyGrew,
                    currentBodyCount,
                    maximumBodyCount,
                    healthRestored
                );
            }

            BindButton();
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void HandleContinueClicked()
        {
            ContinueRequested?.Invoke();
        }

        private void BindButton()
        {
            if (buttonBound || continueButton == null)
            {
                return;
            }

            continueButton.onClick.AddListener(
                HandleContinueClicked
            );

            buttonBound = true;
        }

        private void UnbindButton()
        {
            if (!buttonBound || continueButton == null)
            {
                buttonBound = false;
                return;
            }

            continueButton.onClick.RemoveListener(
                HandleContinueClicked
            );

            buttonBound = false;
        }

        private static string BuildGrowthText(
            bool bodyGrew,
            int currentBodyCount,
            int maximumBodyCount,
            bool healthRestored
        )
        {
            int safeCurrent =
                Mathf.Max(0, currentBodyCount);

            int safeMaximum =
                Mathf.Max(1, maximumBodyCount);

            if (bodyGrew)
            {
                int previous =
                    Mathf.Max(0, safeCurrent - 1);

                return
                    $"Body {previous} → {safeCurrent}\n" +
                    "Empty Weapon Slot +1";
            }

            if (healthRestored)
            {
                return
                    $"Body MAX {safeMaximum}\n" +
                    "HP FULL RESTORE";
            }

            return
                $"Body MAX {safeMaximum}";
        }
    }
}
