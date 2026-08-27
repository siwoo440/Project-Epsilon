using System;
using UnityEngine;

namespace ProjectEpsilon.Progression
{
    [DefaultExecutionOrder(120)]
    public sealed class SnakeExperience : MonoBehaviour
    {
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExperience;
        [SerializeField] private int baseRequiredExperience = 10;
        [SerializeField] private float experienceGrowthMultiplier = 1.12f;
        [SerializeField] private bool levelUpPending;

        public static SnakeExperience Current { get; private set; }

        public event Action<int, int> ExperienceChanged;
        public event Action<int> LevelChanged;
        public event Action<int> LevelUpRequested;

        public int CurrentLevel => currentLevel;
        public int CurrentExperience => currentExperience;
        public int RequiredExperience =>
            CalculateRequiredExperience(
                baseRequiredExperience,
                experienceGrowthMultiplier,
                currentLevel
            );

        // Day09 코드와의 호환용 별칭이다.
        public int PreviewRequiredExperience => RequiredExperience;
        public bool IsLevelUpPending => levelUpPending;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Current = this;
            }
        }

        private void Start()
        {
            NormalizeValues();
            NotifyLevelChanged();
            NotifyExperienceChanged();
        }

        private void OnDisable()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        public void Configure(
            int baseRequirement,
            float growthMultiplier
        )
        {
            baseRequiredExperience = Mathf.Max(1, baseRequirement);
            experienceGrowthMultiplier = Mathf.Max(1f, growthMultiplier);

            NormalizeValues();
            NotifyLevelChanged();
            NotifyExperienceChanged();
        }

        // Day09 Setup 또는 외부 코드가 기존 시그니처를 사용해도 깨지지 않게 유지한다.
        public void Configure(int baseRequirement)
        {
            Configure(baseRequirement, 1.12f);
        }

        public void ResetProgression()
        {
            currentLevel = 1;
            currentExperience = 0;
            levelUpPending = false;

            NotifyLevelChanged();
            NotifyExperienceChanged();
        }

        public void ResetExperience()
        {
            currentExperience = 0;
            levelUpPending = false;
            NotifyExperienceChanged();
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

            TryRequestNextLevelUp();
            NotifyExperienceChanged();
        }

        public bool CompletePendingLevelUp()
        {
            if (!levelUpPending)
            {
                return false;
            }

            levelUpPending = false;

            bool requestedNext =
                TryRequestNextLevelUp();

            NotifyExperienceChanged();
            return requestedNext;
        }

        private bool TryRequestNextLevelUp()
        {
            if (levelUpPending)
            {
                return false;
            }

            int required = RequiredExperience;

            if (currentExperience < required)
            {
                return false;
            }

            currentExperience -= required;
            currentLevel++;
            levelUpPending = true;

            NotifyLevelChanged();
            LevelUpRequested?.Invoke(currentLevel);

            return true;
        }

        private void NormalizeValues()
        {
            currentLevel = Mathf.Max(1, currentLevel);
            currentExperience = Mathf.Max(0, currentExperience);
            baseRequiredExperience =
                Mathf.Max(1, baseRequiredExperience);
            experienceGrowthMultiplier =
                Mathf.Max(1f, experienceGrowthMultiplier);
        }

        private void NotifyExperienceChanged()
        {
            ExperienceChanged?.Invoke(
                currentExperience,
                RequiredExperience
            );
        }

        private void NotifyLevelChanged()
        {
            LevelChanged?.Invoke(currentLevel);
        }

        public static int CalculateRequiredExperience(
            int baseRequirement,
            float growthMultiplier,
            int level
        )
        {
            int safeBase = Mathf.Max(1, baseRequirement);
            float safeGrowth = Mathf.Max(1f, growthMultiplier);
            int safeLevel = Mathf.Max(1, level);

            double calculated =
                safeBase *
                Math.Pow(
                    safeGrowth,
                    safeLevel - 1
                );

            if (calculated >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Mathf.Max(
                1,
                Mathf.CeilToInt((float)calculated)
            );
        }
    }
}
