using ProjectEpsilon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEpsilon.Progression
{
    public sealed class SnakeProgressionDebugControls : MonoBehaviour
    {
        [SerializeField] private SnakeExperience experience;
        [SerializeField] private int debugExperienceAmount = 10;

        public void Configure(
            SnakeExperience experienceSource,
            int amount
        )
        {
            experience = experienceSource;
            debugExperienceAmount =
                Mathf.Max(1, amount);
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null ||
                !keyboard.lKey.wasPressedThisFrame)
            {
                return;
            }

            if (experience == null)
            {
                experience =
                    GetComponent<SnakeExperience>();
            }

            experience?.AddExperience(
                Mathf.Max(1, debugExperienceAmount)
            );
        }
    }
}
