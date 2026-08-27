using System;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(120)]
    public sealed class SnakeExperience : MonoBehaviour
    {
        [SerializeField] private int currentExperience;
        [SerializeField] private int previewRequiredExperience = 10;

        public static SnakeExperience Current { get; private set; }

        public event Action<int, int> ExperienceChanged;

        public int CurrentExperience => currentExperience;
        public int PreviewRequiredExperience => previewRequiredExperience;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Current = this;
            }
        }

        private void Start()
        {
            NotifyChanged();
        }

        private void OnDisable()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public void Configure(int previewRequirement)
        {
            previewRequiredExperience = Mathf.Max(1, previewRequirement);
            currentExperience = Mathf.Max(0, currentExperience);
            NotifyChanged();
        }

        public void ResetExperience()
        {
            currentExperience = 0;
            NotifyChanged();
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (currentExperience > int.MaxValue - amount)
            {
                currentExperience = int.MaxValue;
            }
            else
            {
                currentExperience += amount;
            }

            NotifyChanged();
        }

        private void NotifyChanged()
        {
            ExperienceChanged?.Invoke(
                currentExperience,
                previewRequiredExperience
            );
        }
    }
}
