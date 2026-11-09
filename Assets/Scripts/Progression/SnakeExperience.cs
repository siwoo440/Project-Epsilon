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
        [SerializeField] private ExperiencePickupSettings pickupSettings; // 보석 흡수 설정
        private double zoneExperienceBonusRemainder; // 지역 경험치 소수 보너스

        public static SnakeExperience Current { get; private set; }

        public event Action<int, int> ExperienceChanged;
        public event Action<int> LevelChanged;
        public event Action<int> LevelUpRequested;

        public int CurrentLevel => currentLevel;
        public int CurrentExperience => currentExperience;
        public ExperiencePickupSettings PickupSettings => pickupSettings; // 새 보석의 흡수 설정

        public void ConfigurePickupSettings(ExperiencePickupSettings settings) // 새 보석 설정 연결
        { // 메서드 시작
            pickupSettings = settings; // 흡수 설정 저장
        } // 메서드 끝
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
            zoneExperienceBonusRemainder = 0d; // 새 성장의 지역 소수 초기화
            currentLevel = 1;
            currentExperience = 0;
            levelUpPending = false;

            NotifyLevelChanged();
            NotifyExperienceChanged();
        }

        public void ResetExperience()
        {
            zoneExperienceBonusRemainder = 0d; // 경험치 초기화의 소수 제거
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

            float zoneMultiplier = ProjectEpsilon.World.PrototypeSpecialZone.ExperienceMultiplierAt(transform.position); // 현재 지역 경험치 배율
            double zoneBonus = amount * ((double)zoneMultiplier - 1d) + zoneExperienceBonusRemainder; // 지역 소수 보너스 누적
            int wholeBonus = zoneBonus >= int.MaxValue ? int.MaxValue : (int)System.Math.Floor(zoneBonus); // 정수 범위 내 추가 경험치
            zoneExperienceBonusRemainder = zoneBonus >= int.MaxValue ? 0d : zoneBonus - wholeBonus; // 다음 획득의 소수 보존
            amount = wholeBonus > int.MaxValue - amount ? int.MaxValue : amount + wholeBonus; // 정수 범위 내 최종 획득량

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
                Mathf.RoundToInt((float)calculated) // 기획 기준 반올림
            );
        }
    }
}
