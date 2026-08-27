using ProjectEpsilon.Data;

namespace ProjectEpsilon.Progression
{
    public readonly struct WeaponRewardCandidate
    {
        public WeaponData Weapon { get; }
        public int Grade { get; }

        public bool IsValid => Weapon != null;

        public WeaponRewardCandidate(
            WeaponData weapon,
            int grade
        )
        {
            Weapon = weapon;
            Grade = grade;
        }
    }
}
