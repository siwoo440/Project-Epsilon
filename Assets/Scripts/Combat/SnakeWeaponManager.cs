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
                    slots.Add(new SnakeWeaponSlot(segment));
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

        private void HandleBodyCountChanged(
            int current,
            int maximum
        )
        {
            SynchronizeSlots();
        }

        private void EnsureStartingWeapon()
        {
            if (startingWeapon == null || slots.Count <= 0)
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

                WeaponData weapon = slot.Weapon;

                if (weapon.AttackType !=
                    WeaponAttackType.StraightProjectile)
                {
                    continue;
                }

                WeaponTarget target =
                    WeaponTarget.FindClosest(
                        slot.Origin.position,
                        weapon.Range
                    );

                if (target == null)
                {
                    continue;
                }

                FireStraightProjectile(
                    slot,
                    weapon,
                    target
                );

                slot.StartCooldown(currentTime);
            }
        }

        private void FireStraightProjectile(
            SnakeWeaponSlot slot,
            WeaponData weapon,
            WeaponTarget target
        )
        {
            Vector3 origin = slot.Origin.position;
            Vector2 direction =
                target.transform.position - origin;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = slot.Origin.up;
            }

            GameObject projectileObject =
                new GameObject(
                    $"{weapon.DisplayName}_Projectile"
                );

            projectileObject.transform.position = origin;

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

        private static SnakeWeaponSlot FindSlot(
            List<SnakeWeaponSlot> source,
            SnakeSegment owner
        )
        {
            for (int index = 0; index < source.Count; index++)
            {
                SnakeWeaponSlot slot = source[index];

                if (slot != null && slot.Owner == owner)
                {
                    return slot;
                }
            }

            return null;
        }

        private void Subscribe()
        {
            if (subscribed || bodyManager == null)
            {
                return;
            }

            bodyManager.BodyCountChanged +=
                HandleBodyCountChanged;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || bodyManager == null)
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
