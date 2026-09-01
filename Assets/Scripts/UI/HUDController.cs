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

        public void SetHealth(int current, int maximum, int shield) // 보호막 포함 체력 표시
        { // 메서드 시작
            if (healthText != null) // 체력 Text 확인
            { // 조건 시작
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum, shield); // 체력과 보호막 갱신
            } // 조건 끝
        } // 메서드 끝

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
