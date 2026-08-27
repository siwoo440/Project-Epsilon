using ProjectEpsilon.Progression;
using UnityEngine;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(300)]
    public sealed class SnakeExperienceHUDPresenter : MonoBehaviour
    {
        [SerializeField] private SnakeExperience experience;
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

        public void Bind(
            SnakeExperience source,
            HUDController hud
        )
        {
            Unsubscribe();

            experience = source;
            hudController = hud;

            if (Application.isPlaying)
            {
                Subscribe();
            }

            Refresh();
        }

        private void Subscribe()
        {
            if (subscribed || experience == null)
            {
                return;
            }

            experience.ExperienceChanged +=
                HandleExperienceChanged;

            experience.LevelChanged +=
                HandleLevelChanged;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || experience == null)
            {
                subscribed = false;
                return;
            }

            experience.ExperienceChanged -=
                HandleExperienceChanged;

            experience.LevelChanged -=
                HandleLevelChanged;

            subscribed = false;
        }

        private void Refresh()
        {
            if (experience == null || hudController == null)
            {
                return;
            }

            hudController.SetExperience(
                experience.CurrentExperience,
                experience.RequiredExperience
            );

            hudController.SetLevel(
                experience.CurrentLevel
            );
        }

        private void HandleExperienceChanged(
            int current,
            int required
        )
        {
            if (hudController != null)
            {
                hudController.SetExperience(
                    current,
                    required
                );
            }
        }

        private void HandleLevelChanged(int level)
        {
            if (hudController != null)
            {
                hudController.SetLevel(level);
            }
        }
    }
}
