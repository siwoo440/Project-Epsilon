using ProjectEpsilon.Data;

namespace ProjectEpsilon.Progression
{
    public readonly struct WeaponMergeCandidate
    {
        public WeaponData Weapon { get; }
        public int CurrentGrade { get; }
        public int ResultGrade { get; }
        public int FirstSlotIndex { get; }
        public int SecondSlotIndex { get; }

        public bool IsValid =>
            Weapon != null &&
            CurrentGrade >= 1 &&
            CurrentGrade < 5 &&
            ResultGrade == CurrentGrade + 1 &&
            FirstSlotIndex >= 0 &&
            SecondSlotIndex >= 0 &&
            FirstSlotIndex != SecondSlotIndex;

        public WeaponMergeCandidate(
            WeaponData weapon,
            int currentGrade,
            int firstSlotIndex,
            int secondSlotIndex
        )
        {
            Weapon = weapon;
            CurrentGrade = currentGrade;
            ResultGrade = currentGrade + 1;
            FirstSlotIndex = firstSlotIndex;
            SecondSlotIndex = secondSlotIndex;
        }
    }
}
