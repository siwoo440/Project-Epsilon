using System;
using System.Collections.Generic;
using ProjectEpsilon.Core;
using ProjectEpsilon.Data;
using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    [DefaultExecutionOrder(80)]
    public sealed class SnakeWeaponManager : MonoBehaviour
    {
        [SerializeField] private SnakeBodyManager bodyManager;
        [SerializeField] private WeaponData startingWeapon;
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private List<SnakeWeaponSlot> slots =
            new List<SnakeWeaponSlot>();

        private bool subscribed;

        public event Action SlotsChanged;

        public IReadOnlyList<SnakeWeaponSlot> Slots => slots;
        public int SlotCount => slots.Count;

        public int OccupiedSlotCount
        {
            get
            {
                int count = 0;

                for (int index = 0; index < slots.Count; index++)
                {
                    if (!slots[index].IsEmpty)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
            SynchronizeSlots();
            EnsureStartingWeapon();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                !GameManager.Instance.IsPlaying)
            {
                return;
            }

            TickAutoAttack();
        }

        public void Configure(
            SnakeBodyManager manager,
            WeaponData defaultWeapon,
            Sprite projectileVisual
        )
        {
            Unsubscribe();

            bodyManager = manager;
            startingWeapon = defaultWeapon;
            projectileSprite = projectileVisual;

            SynchronizeSlots();

            if (slots.Count > 0 &&
                slots[0].IsEmpty &&
                startingWeapon != null)
            {
                slots[0].Equip(startingWeapon, 1);
            }

            Subscribe();
            SlotsChanged?.Invoke();
        }

        public void SynchronizeSlots()
        {
            if (bodyManager == null)
            {
                slots.Clear();
                SlotsChanged?.Invoke();
                return;
            }

            List<SnakeWeaponSlot> previousSlots =
                new List<SnakeWeaponSlot>(slots);

            slots.Clear();

            IReadOnlyList<SnakeSegment> bodySegments =
                bodyManager.BodySegments;

            for (int bodyIndex = 0;
                bodyIndex < bodySegments.Count;
                bodyIndex++)
            {
                SnakeSegment segment = bodySegments[bodyIndex];
                SnakeWeaponSlot existing =
                    FindSlot(previousSlots, segment);

                if (existing != null)
                {
                    existing.SetOwner(segment);
                    slots.Add(existing);
                }
                else
                {
                    slots.Add(
                        new SnakeWeaponSlot(segment)
                    );
                }
            }

            SlotsChanged?.Invoke();
        }

        public bool TryEquipFirstEmpty(
            WeaponData weapon,
            int grade = 1
        )
        {
            if (weapon == null)
            {
                return false;
            }

            for (int index = 0; index < slots.Count; index++)
            {
                if (!slots[index].IsEmpty)
                {
                    continue;
                }

                slots[index].Equip(weapon, grade);
                SlotsChanged?.Invoke();
                return true;
            }

            return false;
        }

        public bool TryEquipAt(
            int slotIndex,
            WeaponData weapon,
            int grade = 1
        )
        {
            if (weapon == null ||
                slotIndex < 0 ||
                slotIndex >= slots.Count)
            {
                return false;
            }

            slots[slotIndex].Equip(weapon, grade);
            SlotsChanged?.Invoke();
            return true;
        }

        public bool AcquireWeapon(
            WeaponData weapon,
            int grade = 1
        )
        {
            if (weapon == null || slots.Count <= 0)
            {
                return false;
            }

            int emptyIndex =
                FindFirstEmptySlotIndex();

            int lastShiftIndex =
                emptyIndex >= 0
                    ? emptyIndex
                    : slots.Count - 1;

            for (int index = lastShiftIndex;
                index >= 1;
                index--)
            {
                CopySlotWeapon(
                    slots[index - 1],
                    slots[index]
                );
            }

            slots[0].Equip(
                weapon,
                Mathf.Clamp(grade, 1, 5)
            );

            SlotsChanged?.Invoke();
            return true;
        }

        public bool HasCompletedGradeFive(
            WeaponData weapon
        )
        {
            if (weapon == null)
            {
                return false;
            }

            for (int index = 0; index < slots.Count; index++)
            {
                SnakeWeaponSlot slot =
                    slots[index];

                if (slot == null ||
                    slot.IsEmpty ||
                    slot.Weapon != weapon)
                {
                    continue;
                }

                if (slot.Grade >= 5)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleBodyCountChanged(
            int current,
            int maximum
        )
        {
            SynchronizeSlots();
        }

        private void EnsureStartingWeapon()
        {
            if (startingWeapon == null ||
                slots.Count <= 0)
            {
                return;
            }

            if (OccupiedSlotCount > 0)
            {
                return;
            }

            slots[0].Equip(startingWeapon, 1);
            SlotsChanged?.Invoke();
        }

        private void TickAutoAttack()
        {
            float currentTime = Time.time;

            for (int index = 0; index < slots.Count; index++)
            {
                SnakeWeaponSlot slot = slots[index];

                if (slot == null ||
                    slot.IsEmpty ||
                    slot.Origin == null ||
                    !slot.IsReady(currentTime))
                {
                    continue;
                }

                if (!TryAttack(slot, slot.Weapon))
                {
                    continue;
                }

                slot.StartCooldown(currentTime);
            }
        }

        private bool TryAttack(
            SnakeWeaponSlot slot,
            WeaponData weapon
        )
        {
            switch (weapon.AttackType)
            {
                case WeaponAttackType.Melee:
                    return TryMeleeAttack(
                        slot,
                        weapon
                    );

                case WeaponAttackType.StraightProjectile:
                    return TryStraightProjectileAttack(
                        slot,
                        weapon
                    );

                case WeaponAttackType.Area:
                    return TryAreaAttack(
                        slot,
                        weapon
                    );

                default:
                    return false;
            }
        }

        private bool TryMeleeAttack(
            SnakeWeaponSlot slot,
            WeaponData weapon
        )
        {
            Vector3 origin =
                slot.Origin.position;

            WeaponTarget target =
                WeaponTarget.FindClosest(
                    origin,
                    weapon.Range
                );

            if (target == null)
            {
                return false;
            }

            target.TakeDamage(
                weapon.BaseDamage
            );

            SpawnAttackPulse(
                origin,
                weapon.Range,
                new Color(
                    1f,
                    0.65f,
                    0.25f,
                    0.75f
                ),
                0.12f
            );

            return true;
        }

        private bool TryStraightProjectileAttack(
            SnakeWeaponSlot slot,
            WeaponData weapon
        )
        {
            WeaponTarget target =
                WeaponTarget.FindClosest(
                    slot.Origin.position,
                    weapon.Range
                );

            if (target == null)
            {
                return false;
            }

            FireStraightProjectile(
                slot,
                weapon,
                target
            );

            return true;
        }

        private bool TryAreaAttack(
            SnakeWeaponSlot slot,
            WeaponData weapon
        )
        {
            Vector3 origin =
                slot.Origin.position;

            int hitCount =
                WeaponTarget.DamageAllInRange(
                    origin,
                    weapon.Range,
                    weapon.BaseDamage
                );

            if (hitCount <= 0)
            {
                return false;
            }

            SpawnAttackPulse(
                origin,
                weapon.Range,
                new Color(
                    0.45f,
                    0.8f,
                    1f,
                    0.65f
                ),
                0.2f
            );

            return true;
        }

        private void FireStraightProjectile(
            SnakeWeaponSlot slot,
            WeaponData weapon,
            WeaponTarget target
        )
        {
            Vector3 origin =
                slot.Origin.position;

            Vector2 direction =
                target.transform.position -
                origin;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction =
                    slot.Origin.up;
            }

            GameObject projectileObject =
                new GameObject(
                    $"{weapon.DisplayName}_Projectile"
                );

            projectileObject.transform.position =
                origin;

            StraightProjectile projectile =
                projectileObject.AddComponent<StraightProjectile>();

            projectile.Configure(
                direction,
                weapon.BaseDamage,
                weapon.ProjectileSpeed,
                weapon.ProjectileLifetime,
                projectileSprite
            );
        }

        private void SpawnAttackPulse(
            Vector3 origin,
            float radius,
            Color color,
            float duration
        )
        {
            GameObject pulseObject =
                new GameObject(
                    "Weapon_AttackPulse"
                );

            pulseObject.transform.position =
                origin;

            WeaponAttackPulse pulse =
                pulseObject.AddComponent<WeaponAttackPulse>();

            pulse.Configure(
                projectileSprite,
                radius,
                color,
                duration
            );
        }

        private int FindFirstEmptySlotIndex()
        {
            for (int index = 0; index < slots.Count; index++)
            {
                if (slots[index].IsEmpty)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void CopySlotWeapon(
            SnakeWeaponSlot source,
            SnakeWeaponSlot destination
        )
        {
            if (source == null ||
                destination == null)
            {
                return;
            }

            if (source.IsEmpty)
            {
                destination.Clear();
                return;
            }

            destination.Equip(
                source.Weapon,
                source.Grade
            );
        }

        private static SnakeWeaponSlot FindSlot(
            List<SnakeWeaponSlot> source,
            SnakeSegment owner
        )
        {
            for (int index = 0; index < source.Count; index++)
            {
                SnakeWeaponSlot slot =
                    source[index];

                if (slot != null &&
                    slot.Owner == owner)
                {
                    return slot;
                }
            }

            return null;
        }

        private void Subscribe()
        {
            if (subscribed ||
                bodyManager == null)
            {
                return;
            }

            bodyManager.BodyCountChanged +=
                HandleBodyCountChanged;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed ||
                bodyManager == null)
            {
                subscribed = false;
                return;
            }

            bodyManager.BodyCountChanged -=
                HandleBodyCountChanged;

            subscribed = false;
        }
    }
}
