using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text bodyCountText;
        [SerializeField] private Text experienceText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text staminaText;

        private void Start()
        {
            ShowDebugDefaults();
        }

        public void Bind(
            Text health,
            Text bodyCount,
            Text experience,
            Text level,
            Text stamina
        )
        {
            healthText = health;
            bodyCountText = bodyCount;
            experienceText = experience;
            levelText = level;
            staminaText = stamina;

            ShowDebugDefaults();
        }

        public void SetHealth(int current, int maximum)
        {
            if (healthText != null)
            {
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum);
            }
        }

        public void SetBodyCount(int current, int maximum)
        {
            if (bodyCountText != null)
            {
                bodyCountText.text = HUDTextFormatter.FormatBodyCount(current, maximum);
            }
        }

        public void SetExperience(int current, int required)
        {
            if (experienceText != null)
            {
                experienceText.text = HUDTextFormatter.FormatExperience(current, required);
            }
        }

        public void SetLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = HUDTextFormatter.FormatLevel(level);
            }
        }

        public void SetStamina(int current, int maximum)
        {
            if (staminaText != null)
            {
                staminaText.text = HUDTextFormatter.FormatStamina(current, maximum);
            }
        }

        private void ShowDebugDefaults()
        {
            SetHealth(100, 100);
            SetBodyCount(3, 20);
            SetExperience(0, 10);
            SetLevel(1);
            SetStamina(100, 100);
        }
    }
}
