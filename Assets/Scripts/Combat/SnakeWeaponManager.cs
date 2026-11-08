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

        [Header("Grade Effects")]
        [SerializeField] private WeaponGradeEffectHooks gradeEffectHooks;
        [SerializeField] private WeaponAttributeCombatEffects attributeCombatEffects; // 속성 전투 효과 참조

        [SerializeField] private List<SnakeWeaponSlot> slots =
            new List<SnakeWeaponSlot>();

        private bool subscribed;

        public event Action SlotsChanged;

        public IReadOnlyList<SnakeWeaponSlot> Slots => slots;
        public int SlotCount => slots.Count;
        public WeaponAttributeCombatEffects AttributeCombatEffects => attributeCombatEffects; // 속성 효과 반환

        public int OccupiedSlotCount
        {
            get
            {
                int count = 0;

                for (int index = 0;
                    index < slots.Count;
                    index++)
                {
                    if (!slots[index].IsEmpty)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private void Awake()
        {
            if (gradeEffectHooks == null)
            {
                gradeEffectHooks =
                    GetComponent<WeaponGradeEffectHooks>();
            }

            if (attributeCombatEffects == null) // 속성 효과 없음 확인
            { // 조건 시작
                attributeCombatEffects = GetComponent<WeaponAttributeCombatEffects>(); // 같은 오브젝트 효과 조회
            } // 조건 끝
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
                slots[0].Equip(
                    startingWeapon,
                    1
                );
            }

            Subscribe();
            SlotsChanged?.Invoke();
        }

        public void BindGradeEffectHooks(
            WeaponGradeEffectHooks hooks
        )
        {
            gradeEffectHooks = hooks;
        }

        public void BindAttributeCombatEffects(WeaponAttributeCombatEffects effects) // 속성 효과 연결
        { // 메서드 시작
            attributeCombatEffects = effects; // 속성 효과 저장
        } // 메서드 끝

        public void SynchronizeSlots()
        {
            if (bodyManager == null)
            {
                slots.Clear();
                SlotsChanged?.Invoke();
                return;
            }

            List<SnakeWeaponSlot> previousSlots =
                new List<SnakeWeaponSlot>(
                    slots
                );

            slots.Clear();

            IReadOnlyList<SnakeSegment> bodySegments =
                bodyManager.BodySegments;

            for (int bodyIndex = 0;
                bodyIndex < bodySegments.Count;
                bodyIndex++)
            {
                SnakeSegment segment =
                    bodySegments[bodyIndex];

                SnakeWeaponSlot existing =
                    FindSlot(
                        previousSlots,
                        segment
                    );

                if (existing != null)
                {
                    existing.SetOwner(
                        segment
                    );

                    slots.Add(existing);
                }
                else
                {
                    slots.Add(
                        new SnakeWeaponSlot(
                            segment
                        )
                    );
                }
            }

            SlotsChanged?.Invoke();
        }

        public bool ClearStartingLoadout() // 시작 선택 전 기존 무기 정리
        { // 메서드 시작
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작 상태 확인
            { // 조건 시작
                return false; // 진행 중 무기 초기화 방지
            } // 조건 끝
            foreach (SnakeWeaponSlot slot in slots) // 실제 몸통 슬롯 순회
            { // 반복 시작
                slot?.Clear(); // 시작 전 기존 무기 제거
            } // 반복 끝
            SlotsChanged?.Invoke(); // 무기와 시너지 갱신
            return true; // 시작 무기 정리 완료
        } // 메서드 끝

        public bool TryEquipFirstEmpty(
            WeaponData weapon,
            int grade = 1
        )
        {
            if (weapon == null)
            {
                return false;
            }

            for (int index = 0;
                index < slots.Count;
                index++)
            {
                if (!slots[index].IsEmpty)
                {
                    continue;
                }

                slots[index].Equip(
                    weapon,
                    grade
                );

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

            slots[slotIndex].Equip(
                weapon,
                grade
            );

            SlotsChanged?.Invoke();
            return true;
        }

        public bool AcquireWeapon(
            WeaponData weapon,
            int grade = 1
        )
        {
            if (weapon == null ||
                slots.Count <= 0)
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
                Mathf.Clamp(
                    grade,
                    1,
                    5
                )
            );

            NotifyGradeEffect(
                slots[0],
                WeaponGradeEffectTrigger.Acquired
            );

            SlotsChanged?.Invoke();
            return true;
        }

        public bool TryReplaceWeapon(int slotIndex, WeaponData weapon, int grade = 1) // 선택 무기 교체 및 머리 뒤 배치
        { // 메서드 시작
            if (weapon == null || slotIndex < 0 || slotIndex >= slots.Count) // 입력 범위 확인
            { // 조건 시작
                return false; // 잘못된 교체 차단
            } // 조건 끝
            for (int index = slotIndex; index > 0; index--) // 선택 위치까지 역순 이동
            { // 반복 시작
                CopySlotWeapon(slots[index - 1], slots[index]); // 앞 무기와 강화 보존
            } // 반복 끝
            slots[0].Equip(weapon, grade); // 신규 무기를 머리 뒤에 장착
            NotifyGradeEffect(slots[0], WeaponGradeEffectTrigger.Acquired); // 신규 무기 효과 알림
            SlotsChanged?.Invoke(); // 슬롯 및 시너지 갱신
            return true; // 교체 완료
        } // 메서드 끝

        public bool TryMergeSlots(
            int firstSlotIndex,
            int secondSlotIndex,
            WeaponMergeUpgrade upgrade = WeaponMergeUpgrade.None // 합성 강화 선택
        )
        {
            if (firstSlotIndex ==
                    secondSlotIndex ||
                firstSlotIndex < 0 ||
                secondSlotIndex < 0 ||
                firstSlotIndex >=
                    slots.Count ||
                secondSlotIndex >=
                    slots.Count)
            {
                return false;
            }

            SnakeWeaponSlot first =
                slots[firstSlotIndex];

            SnakeWeaponSlot second =
                slots[secondSlotIndex];

            if (first == null ||
                second == null ||
                first.IsEmpty ||
                second.IsEmpty ||
                first.Weapon != second.Weapon ||
                first.Grade != second.Grade ||
                first.Grade >= 5 ||
                first.Grade >=
                    first.Weapon.MaxGrade)
            {
                return false;
            }

            return CompleteMerge(first, firstSlotIndex, secondSlotIndex, upgrade, true); // 공통 합성 처리
        } // 메서드 끝

        public bool TryMergeReward(int slotIndex, WeaponData reward, int grade, bool inheritEquipped, WeaponMergeUpgrade upgrade) // 미장착 보상 합성 및 기본 강화 0회
        { // 메서드 시작
            if (reward == null || slotIndex < 0 || slotIndex >= slots.Count || grade < 1 || grade >= 5 || grade >= reward.MaxGrade) // 보상 및 범위 검사
            { // 조건 시작
                return false; // 잘못된 재료 거절
            } // 조건 끝
            SnakeWeaponSlot equipped = slots[slotIndex]; // 장착 재료 조회
            if (equipped == null || equipped.IsEmpty || equipped.Weapon != reward || equipped.Grade != grade) // 같은 무기 및 등급 검사
            { // 조건 시작
                return false; // 불일치 재료 거절
            } // 조건 끝
            if (upgrade < WeaponMergeUpgrade.Damage || upgrade > WeaponMergeUpgrade.Range) // 강화 선택 범위 검사
            { // 조건 시작
                return false; // 잘못된 강화 거절
            } // 조건 끝
            return CompleteMerge(equipped, slotIndex, -1, upgrade, inheritEquipped); // 보상 합성 처리
        } // 메서드 끝

        private bool CompleteMerge(SnakeWeaponSlot first, int firstSlotIndex, int secondSlotIndex, WeaponMergeUpgrade upgrade, bool inheritEquipped) // 공통 합성 결과 처리
        { // 메서드 시작
            WeaponData resultWeapon = first.Weapon; // 합성 결과 무기
            int resultGrade = Mathf.Clamp(first.Grade + 1, 1, 5); // 상위 등급 계산
            int damageBonus = inheritEquipped ? first.DamageUpgrades : 0; // 계승 공격력 강화
            int speedBonus = inheritEquipped ? first.SpeedUpgrades : 0; // 계승 공격 속도 강화
            int rangeBonus = inheritEquipped ? first.RangeUpgrades : 0; // 계승 사거리 강화
            List<WeaponLoadoutEntry> remaining = new List<WeaponLoadoutEntry>(); // 남은 무기 목록
            for (int index = 0; index < slots.Count; index++) // 모든 슬롯 순회
            { // 반복 시작
                if (index == firstSlotIndex || index == secondSlotIndex) // 합성 재료 위치 확인
                { // 조건 시작
                    continue; // 재료 보존 제외
                } // 조건 끝
                SnakeWeaponSlot slot = slots[index]; // 현재 슬롯 조회
                if (slot == null || slot.IsEmpty) // 빈 슬롯 확인
                { // 조건 시작
                    continue; // 빈 슬롯 제외
                } // 조건 끝
                remaining.Add(new WeaponLoadoutEntry(slot.Weapon, slot.Grade, slot.DamageUpgrades, slot.SpeedUpgrades, slot.RangeUpgrades)); // 나머지 무기와 강화 보존
            } // 반복 끝
            for (int index = 0; index < slots.Count; index++) // 슬롯 초기화 순회
            { // 반복 시작
                slots[index].Clear(); // 기존 장착 정보 제거
            } // 반복 끝
            slots[0].Equip(resultWeapon, resultGrade, damageBonus, speedBonus, rangeBonus); // 합성 무기와 계승 강화 장착
            slots[0].ApplyUpgrade(upgrade); // 선택한 공통 강화 적용
            int writeIndex = 1; // 나머지 무기 배치 위치
            for (int index = 0; index < remaining.Count && writeIndex < slots.Count; index++) // 보존 무기 순회
            { // 반복 시작
                WeaponLoadoutEntry entry = remaining[index]; // 보존 무기 조회
                slots[writeIndex].Equip(entry.Weapon, entry.Grade, entry.DamageUpgrades, entry.SpeedUpgrades, entry.RangeUpgrades); // 무기와 강화 복원
                writeIndex++; // 다음 슬롯 이동
            } // 반복 끝
            NotifyGradeEffect(slots[0], WeaponGradeEffectTrigger.Merged); // 합성 등급 효과 알림
            SlotsChanged?.Invoke(); // 무기 및 시너지 갱신 알림
            return true; // 합성 성공
        } // 메서드 끝

        public bool HasCompletedGradeFive(
            WeaponData weapon
        )
        {
            if (weapon == null)
            {
                return false;
            }

            for (int index = 0;
                index < slots.Count;
                index++)
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

            slots[0].Equip(
                startingWeapon,
                1
            );

            SlotsChanged?.Invoke();
        }

        private void TickAutoAttack()
        {
            float currentTime =
                Time.time;

            for (int index = 0;
                index < slots.Count;
                index++)
            {
                SnakeWeaponSlot slot =
                    slots[index];

                if (slot == null ||
                    slot.IsEmpty ||
                    slot.Origin == null ||
                    !slot.IsReady(
                        currentTime
                    ))
                {
                    continue;
                }

                if (!TryAttack(
                    slot,
                    slot.Weapon
                ))
                {
                    continue;
                }

                NotifyGradeEffect(
                    slot,
                    WeaponGradeEffectTrigger.Attack
                );

                slot.StartCooldown(
                    currentTime
                );
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
                    float.PositiveInfinity // 사거리와 관계없는 최근접 대상
                );

            if (target == null)
            {
                return false;
            }

            WeaponTarget second = null; // 추가 공격 대상 초기화
            if (weapon.GetMeleeTargetLimit(slot.Grade) >= 2) // 무기 등급별 두 대상 공격 확인
            { // 조건 시작
                second = WeaponTarget.FindClosest(origin, float.PositiveInfinity, target); // 첫 대상을 제외한 최근접 적
            } // 조건 끝
            WeaponTarget third = null; // 셋째 공격 대상 초기화
            if (weapon.GetMeleeTargetLimit(slot.Grade) >= 3) // 오 등급 추가 대상 확인
            { // 조건 시작
                third = WeaponTarget.FindClosest(origin, float.PositiveInfinity, target, second); // 첫째와 둘째를 제외한 최근접 적
            } // 조건 끝
            WeaponAttributeAttackSnapshot attack = CreateAttackSnapshot(slot, origin); // 몸통 원점 공격 정보
            ApplyWeaponHit(attack, target, target.transform.position); // 첫 대상 속성 명중
            if (second != null && second.isActiveAndEnabled && second.IsAlive) // 첫 명중 후 추가 대상 유효성 확인
            { // 조건 시작
                ApplyWeaponHit(attack, second, second.transform.position); // 둘째 대상 속성 명중
            } // 조건 끝
            if (third != null && third.isActiveAndEnabled && third.IsAlive) // 앞선 명중 후 셋째 대상 유효성 확인
            { // 조건 시작
                ApplyWeaponHit(attack, third, third.transform.position); // 셋째 대상 속성 명중
            } // 조건 끝

            SpawnAttackPulse(
                origin,
                slot.EffectiveRange,
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
                    float.PositiveInfinity // 사거리와 관계없는 최근접 대상
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

            WeaponTarget closest = WeaponTarget.FindClosest(origin, float.PositiveInfinity); // 사거리와 관계없는 최근접 대상
            if (closest == null) // 공격 대상 확인
            { // 조건 시작
                return false; // 적이 없으면 공격 대기
            } // 조건 끝
            float attackRange = slot.EffectiveRange * weapon.GetAreaRangeMultiplier(slot.Grade); // 등급별 실제 범위
            Vector3 center = (closest.transform.position - origin).sqrMagnitude > attackRange * attackRange ? closest.transform.position : origin; // 사거리 밖 대상 또는 기존 몸통 중심
            WeaponAttributeAttackSnapshot snapshot = CreateAttackSnapshot(slot, origin); // 실제 몸통 원점 공격 정보
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(snapshot.Weapon, snapshot.Attribute, snapshot.AttributeCount, snapshot.SynergyStage, snapshot.Grade, snapshot.Origin, snapshot.DirectDamage, center); // 실제 범위 중심 포함 공격 정보
            int hitCount = WeaponTarget.VisitAllInRange( // 범위 대상 방문
                center, // 실제 범위 중심 전달
                attackRange, // 등급별 공격 범위 전달
                target => ApplyWeaponHit(attack, target, target.transform.position) // 대상별 명중 적용
            ); // 방문 종료

            if (hitCount <= 0)
            {
                return false;
            }

            SpawnAttackPulse(
                center, // 실제 범위 중심 표시
                attackRange, // 등급별 범위 시각화
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

            if (direction.sqrMagnitude <=
                0.0001f)
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

            WeaponAttributeAttackSnapshot attack = CreateAttackSnapshot(slot, origin); // 발사 시점 정보 생성

            projectile.Configure(
                direction,
                attack, // 공격 정보 전달
                attributeCombatEffects, // 속성 효과 전달
                weapon.ProjectileSpeed,
                Mathf.Max(weapon.ProjectileLifetime, direction.magnitude / Mathf.Max(0.01f, weapon.ProjectileSpeed) + 0.5f), // 발사 시점 원거리 목표까지의 최소 수명
                projectileSprite
            );
        }

        private WeaponAttributeAttackSnapshot CreateAttackSnapshot(SnakeWeaponSlot slot, Vector3 origin) // 공격 정보 생성
        { // 메서드 시작
            float gradeDamage = CalculateSlotDamage(slot); // 등급 피해 계산

            if (attributeCombatEffects != null) // 속성 효과 연결 확인
            { // 조건 시작
                return attributeCombatEffects.CreateAttackSnapshot(slot.Weapon, slot.Grade, origin, gradeDamage); // 시너지 공격 정보 반환
            } // 조건 끝

            return new WeaponAttributeAttackSnapshot(slot.Weapon, slot.Weapon.Attribute, 0, 0, slot.Grade, origin, gradeDamage); // 기본 공격 정보 반환
        } // 메서드 끝

        private void ApplyWeaponHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 hitPosition) // 대상 명중 적용
        { // 메서드 시작
            if (attributeCombatEffects != null) // 속성 효과 연결 확인
            { // 조건 시작
                attributeCombatEffects.ApplyHit(attack, target, hitPosition); // 통합 속성 명중 적용
                return; // 대체 피해 방지
            } // 조건 끝

            target?.TakeDamage(attack.DirectDamage); // 기본 직접 피해 적용
        } // 메서드 끝

        private float CalculateSlotDamage(
            SnakeWeaponSlot slot
        )
        {
            if (slot == null ||
                slot.IsEmpty)
            {
                return 0f;
            }

            return WeaponGradeRules.CalculateDamage(
                slot.Weapon.BaseDamage,
                slot.Grade
            ) * slot.DamageMultiplier; // 합성 공격력 강화 적용
        }

        private void NotifyGradeEffect(
            SnakeWeaponSlot slot,
            WeaponGradeEffectTrigger trigger
        )
        {
            if (gradeEffectHooks == null ||
                slot == null ||
                slot.IsEmpty)
            {
                return;
            }

            Vector3 origin =
                slot.Origin == null
                    ? transform.position
                    : slot.Origin.position;

            gradeEffectHooks.Notify(
                new WeaponGradeEffectContext(
                    slot.Weapon,
                    slot.Grade,
                    origin,
                    CalculateSlotDamage(
                        slot
                    ),
                    trigger
                )
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
            for (int index = 0;
                index < slots.Count;
                index++)
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
                source.Grade,
                source.DamageUpgrades, // 공격력 강화 복사
                source.SpeedUpgrades, // 공격 속도 강화 복사
                source.RangeUpgrades // 사거리 강화 복사
            );
        }

        private static SnakeWeaponSlot FindSlot(
            List<SnakeWeaponSlot> source,
            SnakeSegment owner
        )
        {
            for (int index = 0;
                index < source.Count;
                index++)
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

        private readonly struct WeaponLoadoutEntry
        {
            public WeaponData Weapon { get; }
            public int Grade { get; }
            public int DamageUpgrades { get; } // 공격력 강화 횟수
            public int SpeedUpgrades { get; } // 공격 속도 강화 횟수
            public int RangeUpgrades { get; } // 사거리 강화 횟수

            public WeaponLoadoutEntry(
                WeaponData weapon,
                int grade,
                int damage, // 공격력 강화 횟수
                int speed, // 공격 속도 강화 횟수
                int range // 사거리 강화 횟수
            )
            {
                Weapon = weapon;
                Grade = grade;
                DamageUpgrades = damage; // 공격력 강화 저장
                SpeedUpgrades = speed; // 공격 속도 강화 저장
                RangeUpgrades = range; // 사거리 강화 저장
            }
        }
    }
}
