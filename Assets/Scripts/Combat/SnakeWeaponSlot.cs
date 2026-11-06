using System;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    public enum WeaponMergeUpgrade // 합성 공통 강화 종류
    { // 열거형 시작
        None, // 강화 없음
        Damage, // 공격력 강화
        AttackSpeed, // 공격 속도 강화
        Range // 사거리 및 범위 강화
    } // 열거형 끝

    [Serializable]
    public sealed class SnakeWeaponSlot
    {
        [SerializeField] private SnakeSegment owner;
        [SerializeField] private WeaponData weapon;
        [Range(1, 5)] [SerializeField] private int grade = 1;

        [SerializeField] private int damageUpgrades; // 공격력 강화 횟수
        [SerializeField] private int speedUpgrades; // 공격 속도 강화 횟수
        [SerializeField] private int rangeUpgrades; // 사거리 강화 횟수
        private const float UpgradeBonus = 0.05f; // 강화 1회당 임시 증가율
        private float nextAttackTime;

        public SnakeSegment Owner => owner;
        public Transform Origin => owner == null ? null : owner.transform;
        public WeaponData Weapon => weapon;
        public int Grade => grade;
        public bool IsEmpty => weapon == null;
        public int DamageUpgrades => damageUpgrades; // 공격력 강화 횟수 조회
        public int SpeedUpgrades => speedUpgrades; // 공격 속도 강화 횟수 조회
        public int RangeUpgrades => rangeUpgrades; // 사거리 강화 횟수 조회
        public float DamageMultiplier => 1f + damageUpgrades * UpgradeBonus; // 공격력 배율
        public float SpeedMultiplier => 1f + speedUpgrades * UpgradeBonus; // 공격 속도 배율
        public float EffectiveRange => weapon == null ? 0f : weapon.Range * (1f + rangeUpgrades * UpgradeBonus); // 강화 사거리

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

        public void Equip(WeaponData data, int weaponGrade = 1, int damageBonus = 0, int speedBonus = 0, int rangeBonus = 0) // 무기 및 강화 장착
        {
            weapon = data;
            grade = Mathf.Clamp(weaponGrade, 1, 5);
            damageUpgrades = Mathf.Max(0, damageBonus); // 공격력 강화 저장
            speedUpgrades = Mathf.Max(0, speedBonus); // 공격 속도 강화 저장
            rangeUpgrades = Mathf.Max(0, rangeBonus); // 사거리 강화 저장
            nextAttackTime = 0f;
        }

        public void Clear()
        {
            weapon = null;
            grade = 1;
            damageUpgrades = 0; // 공격력 강화 초기화
            speedUpgrades = 0; // 공격 속도 강화 초기화
            rangeUpgrades = 0; // 사거리 강화 초기화
            nextAttackTime = 0f;
        }

        public void ApplyUpgrade(WeaponMergeUpgrade upgrade) // 선택 강화 적용
        { // 메서드 시작
            switch (upgrade) // 강화 종류 분기
            { // 분기 시작
                case WeaponMergeUpgrade.Damage: // 공격력 선택
                    damageUpgrades++; // 공격력 강화 증가
                    break; // 분기 종료
                case WeaponMergeUpgrade.AttackSpeed: // 공격 속도 선택
                    speedUpgrades++; // 공격 속도 강화 증가
                    break; // 분기 종료
                case WeaponMergeUpgrade.Range: // 사거리 선택
                    rangeUpgrades++; // 사거리 강화 증가
                    break; // 분기 종료
            } // 분기 끝
        } // 메서드 끝

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

            nextAttackTime = currentTime + Mathf.Max(0.01f, weapon.AttackInterval / SpeedMultiplier); // 강화 공격 주기
        }
    }
}
