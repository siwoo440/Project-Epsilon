using System;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    [Serializable]
    public sealed class SnakeWeaponSlot
    {
        [SerializeField] private SnakeSegment owner;
        [SerializeField] private WeaponData weapon;
        [Range(1, 5)] [SerializeField] private int grade = 1;

        private float nextAttackTime;

        public SnakeSegment Owner => owner;
        public Transform Origin => owner == null ? null : owner.transform;
        public WeaponData Weapon => weapon;
        public int Grade => grade;
        public bool IsEmpty => weapon == null;

        public SnakeWeaponSlot(SnakeSegment segment)
        {
            owner = segment;
            grade = 1;
            nextAttackTime = 0f;
        }

        public void SetOwner(SnakeSegment segment)
        {
            owner = segment;
        }

        public void Equip(WeaponData data, int weaponGrade = 1)
        {
            weapon = data;
            grade = Mathf.Clamp(weaponGrade, 1, 5);
            nextAttackTime = 0f;
        }

        public void Clear()
        {
            weapon = null;
            grade = 1;
            nextAttackTime = 0f;
        }

        public bool IsReady(float currentTime)
        {
            return weapon != null && currentTime >= nextAttackTime;
        }

        public void StartCooldown(float currentTime)
        {
            if (weapon == null)
            {
                nextAttackTime = currentTime;
                return;
            }

            nextAttackTime = currentTime + Mathf.Max(0.01f, weapon.AttackInterval);
        }
    }
}
