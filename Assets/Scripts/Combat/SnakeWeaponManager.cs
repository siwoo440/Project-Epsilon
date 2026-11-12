using System; // C# 기본 기능 연결
using System.Collections.Generic; // C# 기본 기능 연결
using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(80)] // 대상 설정
    public sealed class SnakeWeaponManager : MonoBehaviour // 포탑·관리자 자료 구조
    {
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private WeaponData startingWeapon; // 필드·포탑 설정
        [SerializeField] private Sprite projectileSprite; // 필드·이미지 설정
        [SerializeField] private Sprite turretProjectileSprite; // 실제 포탑 탄 이미지

        [Header("Grade Effects")] // 등급 설정
        [SerializeField] private WeaponGradeEffectHooks gradeEffectHooks; // 필드·포탑 설정
        [SerializeField] private WeaponAttributeCombatEffects attributeCombatEffects; // 속성 전투 효과 참조

        [SerializeField] private List<SnakeWeaponSlot> slots = // 필드·목록 설정
            new List<SnakeWeaponSlot>(); // 목록·포탑 객체 생성

        private bool subscribed; // 대상 구성 선언

        public event Action SlotsChanged; // 이벤트·동작 구성 선언

        public IReadOnlyList<SnakeWeaponSlot> Slots => slots; // 목록·포탑 값 반환
        public int SlotCount => slots.Count; // 슬롯·개수 값 반환
        public WeaponAttributeCombatEffects AttributeCombatEffects => attributeCombatEffects; // 속성 효과 반환

        public int OccupiedSlotCount // 장착·슬롯 구성 선언
        {
            get // 조회 처리
            {
                int count = 0; // 개수 값 설정

                for (int index = 0; // 순서 반복 처리
                    index < slots.Count; // 순서·슬롯 처리
                    index++) // 순서 처리
                {
                    if (!slots[index].IsEmpty) // 슬롯·순서 조건 확인
                    {
                        count++; // 개수 처리
                    }
                }

                return count; // 개수 결과 반환
            }
        }

        private void Awake() // 초기화 기능 선언
        {
            if (gradeEffectHooks == null) // 등급·효과 조건 확인
            {
                gradeEffectHooks = // 등급·효과 값 설정
                    GetComponent<WeaponGradeEffectHooks>(); // 조회·구성 요소 기능 실행
            }

            if (attributeCombatEffects == null) // 속성 효과 없음 확인
            {
                attributeCombatEffects = GetComponent<WeaponAttributeCombatEffects>(); // 같은 오브젝트 효과 조회
            }
        }

        private void OnEnable() // 활성 기능 선언
        {
            Subscribe(); // 구독 기능 실행
        }

        private void Start() // 시작 기능 선언
        {
            Subscribe(); // 구독 기능 실행
            SynchronizeSlots(); // 슬롯 기능 실행
            EnsureStartingWeapon(); // 확인·포탑 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            TickAutoAttack(); // 공격 기능 실행
        }

        public void Configure( // 구성 호출 시작
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            WeaponData defaultWeapon, // 포탑·데이터 항목 전달
            Sprite projectileVisual // 이미지·투사체 처리
        ) // 앞선 항목 종료
        {
            Unsubscribe(); // 구독 해제 기능 실행

            bodyManager = manager; // 몸통·관리자 값 설정
            startingWeapon = defaultWeapon; // 포탑 값 설정
            projectileSprite = projectileVisual; // 투사체·이미지 값 설정

            SynchronizeSlots(); // 슬롯 기능 실행

            if (slots.Count > 0 && // 슬롯·개수 조건 확인
                slots[0].IsEmpty && // 슬롯·빈 상태 처리
                startingWeapon != null) // 포탑 처리
            {
                slots[0].Equip( // 슬롯 호출 시작
                    startingWeapon, // 포탑 항목 전달
                    1 // 대상 처리
                ); // 앞선 항목 종료
            }

            Subscribe(); // 구독 기능 실행
            SlotsChanged?.Invoke(); // 슬롯 기능 실행
        }

        public void BindGradeEffectHooks( // 연결·등급 호출 시작
            WeaponGradeEffectHooks hooks // 포탑·등급 처리
        ) // 앞선 항목 종료
        {
            gradeEffectHooks = hooks; // 등급·효과 값 설정
        }

        public void BindAttributeCombatEffects(WeaponAttributeCombatEffects effects) // 속성 효과 연결
        {
            attributeCombatEffects = effects; // 속성 효과 저장
        }

        public void SynchronizeSlots() // 슬롯 기능 선언
        {
            if (bodyManager == null) // 몸통·관리자 조건 확인
            {
                slots.Clear(); // 슬롯 기능 실행
                SlotsChanged?.Invoke(); // 슬롯 기능 실행
                return; // 현재 기능 종료
            }

            List<SnakeWeaponSlot> previousSlots = // 목록·포탑 값 설정
                new List<SnakeWeaponSlot>( // 목록·포탑 객체 생성
                    slots // 슬롯 처리
                ); // 앞선 항목 종료

            slots.Clear(); // 슬롯 기능 실행

            IReadOnlyList<SnakeSegment> bodySegments = // 목록·몸통 값 설정
                bodyManager.BodySegments; // 몸통·관리자 처리

            for (int bodyIndex = 0; // 몸통·순서 반복 처리
                bodyIndex < bodySegments.Count; // 몸통·순서 처리
                bodyIndex++) // 몸통·순서 처리
            {
                SnakeSegment segment = // 몸통 값 설정
                    bodySegments[bodyIndex]; // 몸통·순서 처리

                SnakeWeaponSlot existing = // 포탑·슬롯 값 설정
                    FindSlot( // 검색·슬롯 호출 시작
                        previousSlots, // 이전 값·슬롯 항목 전달
                        segment // 몸통 처리
                    ); // 앞선 항목 종료

                if (existing != null) // 대상 조건 확인
                {
                    existing.SetOwner( // 설정·소유자 호출 시작
                        segment // 몸통 처리
                    ); // 앞선 항목 종료

                    slots.Add(existing); // 슬롯·추가 기능 실행
                }
                else // 다른 조건 처리
                {
                    slots.Add( // 슬롯·추가 호출 시작
                        new SnakeWeaponSlot( // 포탑·슬롯 객체 생성
                            segment // 몸통 처리
                        ) // 앞선 항목 종료
                    ); // 앞선 항목 종료
                }
            }

            SlotsChanged?.Invoke(); // 슬롯 기능 실행
        }

        public bool ClearStartingLoadout() // 시작 선택 전 기존 무기 정리
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작 상태 확인
            {
                return false; // 진행 중 무기 초기화 방지
            }
            foreach (SnakeWeaponSlot slot in slots) // 실제 몸통 슬롯 순회
            {
                slot?.Clear(); // 시작 전 기존 무기 제거
            }
            SlotsChanged?.Invoke(); // 무기와 시너지 갱신
            return true; // 시작 무기 정리 완료
        }

        public bool TryEquipFirstEmpty( // 시도·빈 상태 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade = 1 // 등급 값 설정
        ) // 앞선 항목 종료
        {
            if (weapon == null) // 포탑 조건 확인
            {
                return false; // 실패 결과 반환
            }

            for (int index = 0; // 순서 반복 처리
                index < slots.Count; // 순서·슬롯 처리
                index++) // 순서 처리
            {
                if (!slots[index].IsEmpty) // 슬롯·순서 조건 확인
                {
                    continue; // 다음 반복 진행
                }

                slots[index].Equip( // 슬롯·순서 호출 시작
                    weapon, // 포탑 항목 전달
                    grade // 등급 처리
                ); // 앞선 항목 종료

                SlotsChanged?.Invoke(); // 슬롯 기능 실행
                return true; // 성공 결과 반환
            }

            return false; // 실패 결과 반환
        }

        public bool TryEquipAt( // 시도 호출 시작
            int slotIndex, // 슬롯·순서 항목 전달
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade = 1 // 등급 값 설정
        ) // 앞선 항목 종료
        {
            if (weapon == null || // 포탑 조건 확인
                slotIndex < 0 || // 슬롯·순서 처리
                slotIndex >= slots.Count) // 슬롯·순서 처리
            {
                return false; // 실패 결과 반환
            }

            slots[slotIndex].Equip( // 슬롯·순서 호출 시작
                weapon, // 포탑 항목 전달
                grade // 등급 처리
            ); // 앞선 항목 종료

            SlotsChanged?.Invoke(); // 슬롯 기능 실행
            return true; // 성공 결과 반환
        }

        public bool AcquireWeapon( // 포탑 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade = 1 // 등급 값 설정
        ) // 앞선 항목 종료
        {
            if (weapon == null || // 포탑 조건 확인
                slots.Count <= 0) // 슬롯·개수 처리
            {
                return false; // 실패 결과 반환
            }

            int emptyIndex = // 빈 상태·순서 값 설정
                FindFirstEmptySlotIndex(); // 검색·빈 상태 기능 실행

            int lastShiftIndex = // 순서 값 설정
                emptyIndex >= 0 // 빈 상태·순서 처리
                    ? emptyIndex // 빈 상태·순서 처리
                    : slots.Count - 1; // 슬롯·개수 처리

            for (int index = lastShiftIndex; // 순서 반복 처리
                index >= 1; // 순서 처리
                index--) // 순서 처리
            {
                CopySlotWeapon( // 슬롯·포탑 호출 시작
                    slots[index - 1], // 슬롯·순서 항목 전달
                    slots[index] // 슬롯·순서 처리
                ); // 앞선 항목 종료
            }

            slots[0].Equip( // 슬롯 호출 시작
                weapon, // 포탑 항목 전달
                Mathf.Clamp( // 대상 호출 시작
                    grade, // 등급 항목 전달
                    1, // 대상 항목 전달
                    5 // 대상 처리
                ) // 앞선 항목 종료
            ); // 앞선 항목 종료

            NotifyGradeEffect( // 알림·등급 호출 시작
                slots[0], // 슬롯 항목 전달
                WeaponGradeEffectTrigger.Acquired // 포탑·등급 처리
            ); // 앞선 항목 종료

            SlotsChanged?.Invoke(); // 슬롯 기능 실행
            return true; // 성공 결과 반환
        }

        public bool TryReplaceWeapon(int slotIndex, WeaponData weapon, int grade = 1) // 선택 무기 교체 및 머리 뒤 배치
        {
            if (weapon == null || slotIndex < 0 || slotIndex >= slots.Count) // 입력 범위 확인
            {
                return false; // 잘못된 교체 차단
            }
            for (int index = slotIndex; index > 0; index--) // 선택 위치까지 역순 이동
            {
                CopySlotWeapon(slots[index - 1], slots[index]); // 앞 무기와 강화 보존
            }
            slots[0].Equip(weapon, grade); // 신규 무기를 머리 뒤에 장착
            NotifyGradeEffect(slots[0], WeaponGradeEffectTrigger.Acquired); // 신규 무기 효과 알림
            SlotsChanged?.Invoke(); // 슬롯 및 시너지 갱신
            return true; // 교체 완료
        }

        public bool TryMergeSlots( // 시도·합성 호출 시작
            int firstSlotIndex, // 슬롯·순서 항목 전달
            int secondSlotIndex, // 슬롯·순서 항목 전달
            WeaponMergeUpgrade upgrade = WeaponMergeUpgrade.None // 합성 강화 선택
        ) // 앞선 항목 종료
        {
            if (firstSlotIndex == // 슬롯·순서 조건 확인
                    secondSlotIndex || // 슬롯·순서 처리
                firstSlotIndex < 0 || // 슬롯·순서 처리
                secondSlotIndex < 0 || // 슬롯·순서 처리
                firstSlotIndex >= // 슬롯·순서 처리
                    slots.Count || // 슬롯·개수 처리
                secondSlotIndex >= // 슬롯·순서 처리
                    slots.Count) // 슬롯·개수 처리
            {
                return false; // 실패 결과 반환
            }

            SnakeWeaponSlot first = // 포탑·슬롯 값 설정
                slots[firstSlotIndex]; // 슬롯·순서 처리

            SnakeWeaponSlot second = // 포탑·슬롯 값 설정
                slots[secondSlotIndex]; // 슬롯·순서 처리

            if (first == null || // 대상 조건 확인
                second == null || // 대상 처리
                first.IsEmpty || // 빈 상태 처리
                second.IsEmpty || // 빈 상태 처리
                first.Weapon != second.Weapon || // 포탑 처리
                first.Grade != second.Grade || // 등급 처리
                first.Grade >= 5 || // 등급 처리
                first.Grade >= // 등급 처리
                    first.Weapon.MaxGrade) // 포탑·최대 처리
            {
                return false; // 실패 결과 반환
            }

            return CompleteMerge(first, firstSlotIndex, secondSlotIndex, upgrade, true); // 공통 합성 처리
        }

        public bool TryMergeReward(int slotIndex, WeaponData reward, int grade, bool inheritEquipped, WeaponMergeUpgrade upgrade) // 미장착 보상 합성 및 기본 강화 0회
        {
            if (reward == null || slotIndex < 0 || slotIndex >= slots.Count || grade < 1 || grade >= 5 || grade >= reward.MaxGrade) // 보상 및 범위 검사
            {
                return false; // 잘못된 재료 거절
            }
            SnakeWeaponSlot equipped = slots[slotIndex]; // 장착 재료 조회
            if (equipped == null || equipped.IsEmpty || equipped.Weapon != reward || equipped.Grade != grade) // 같은 무기 및 등급 검사
            {
                return false; // 불일치 재료 거절
            }
            if (upgrade < WeaponMergeUpgrade.Damage || upgrade > WeaponMergeUpgrade.Range) // 강화 선택 범위 검사
            {
                return false; // 잘못된 강화 거절
            }
            return CompleteMerge(equipped, slotIndex, -1, upgrade, inheritEquipped); // 보상 합성 처리
        }

        private bool CompleteMerge(SnakeWeaponSlot first, int firstSlotIndex, int secondSlotIndex, WeaponMergeUpgrade upgrade, bool inheritEquipped) // 공통 합성 결과 처리
        {
            WeaponData resultWeapon = first.Weapon; // 합성 결과 무기
            int resultGrade = Mathf.Clamp(first.Grade + 1, 1, 5); // 상위 등급 계산
            int damageBonus = inheritEquipped ? first.DamageUpgrades : 0; // 계승 공격력 강화
            int speedBonus = inheritEquipped ? first.SpeedUpgrades : 0; // 계승 공격 속도 강화
            int rangeBonus = inheritEquipped ? first.RangeUpgrades : 0; // 계승 사거리 강화
            List<WeaponLoadoutEntry> remaining = new List<WeaponLoadoutEntry>(); // 남은 무기 목록
            for (int index = 0; index < slots.Count; index++) // 모든 슬롯 순회
            {
                if (index == firstSlotIndex || index == secondSlotIndex) // 합성 재료 위치 확인
                {
                    continue; // 재료 보존 제외
                }
                SnakeWeaponSlot slot = slots[index]; // 현재 슬롯 조회
                if (slot == null || slot.IsEmpty) // 빈 슬롯 확인
                {
                    continue; // 빈 슬롯 제외
                }
                remaining.Add(new WeaponLoadoutEntry(slot.Weapon, slot.Grade, slot.DamageUpgrades, slot.SpeedUpgrades, slot.RangeUpgrades)); // 나머지 무기와 강화 보존
            }
            for (int index = 0; index < slots.Count; index++) // 슬롯 초기화 순회
            {
                slots[index].Clear(); // 기존 장착 정보 제거
            }
            slots[0].Equip(resultWeapon, resultGrade, damageBonus, speedBonus, rangeBonus); // 합성 무기와 계승 강화 장착
            slots[0].ApplyUpgrade(upgrade); // 선택한 공통 강화 적용
            int writeIndex = 1; // 나머지 무기 배치 위치
            for (int index = 0; index < remaining.Count && writeIndex < slots.Count; index++) // 보존 무기 순회
            {
                WeaponLoadoutEntry entry = remaining[index]; // 보존 무기 조회
                slots[writeIndex].Equip(entry.Weapon, entry.Grade, entry.DamageUpgrades, entry.SpeedUpgrades, entry.RangeUpgrades); // 무기와 강화 복원
                writeIndex++; // 다음 슬롯 이동
            }
            NotifyGradeEffect(slots[0], WeaponGradeEffectTrigger.Merged); // 합성 등급 효과 알림
            SlotsChanged?.Invoke(); // 무기 및 시너지 갱신 알림
            return true; // 합성 성공
        }

        public bool HasCompletedGradeFive( // 존재 여부·등급 호출 시작
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            if (weapon == null) // 포탑 조건 확인
            {
                return false; // 실패 결과 반환
            }

            for (int index = 0; // 순서 반복 처리
                index < slots.Count; // 순서·슬롯 처리
                index++) // 순서 처리
            {
                SnakeWeaponSlot slot = // 포탑·슬롯 값 설정
                    slots[index]; // 슬롯·순서 처리

                if (slot == null || // 슬롯 조건 확인
                    slot.IsEmpty || // 슬롯·빈 상태 처리
                    !string.Equals(slot.Weapon.Id, weapon.Id, StringComparison.Ordinal)) // 동일 식별자 포탑 확인
                {
                    continue; // 다음 반복 진행
                }

                if (slot.Grade >= 5) // 슬롯·등급 조건 확인
                {
                    return true; // 성공 결과 반환
                }
            }

            return false; // 실패 결과 반환
        }

        private void HandleBodyCountChanged( // 몸통·개수 호출 시작
            int current, // 현재 항목 전달
            int maximum // 최대 처리
        ) // 앞선 항목 종료
        {
            SynchronizeSlots(); // 슬롯 기능 실행
        }

        private void EnsureStartingWeapon() // 확인·포탑 기능 선언
        {
            if (startingWeapon == null || // 포탑 조건 확인
                slots.Count <= 0) // 슬롯·개수 처리
            {
                return; // 현재 기능 종료
            }

            if (OccupiedSlotCount > 0) // 장착·슬롯 조건 확인
            {
                return; // 현재 기능 종료
            }

            slots[0].Equip( // 슬롯 호출 시작
                startingWeapon, // 포탑 항목 전달
                1 // 대상 처리
            ); // 앞선 항목 종료

            SlotsChanged?.Invoke(); // 슬롯 기능 실행
        }

        private void TickAutoAttack() // 공격 기능 선언
        {
            float currentTime = // 현재·시간 값 설정
                Time.time; // 시간 처리

            for (int index = 0; // 순서 반복 처리
                index < slots.Count; // 순서·슬롯 처리
                index++) // 순서 처리
            {
                SnakeWeaponSlot slot = // 포탑·슬롯 값 설정
                    slots[index]; // 슬롯·순서 처리

                if (slot == null || // 슬롯 조건 확인
                    slot.IsEmpty || // 슬롯·빈 상태 처리
                    slot.Origin == null || // 슬롯·발사 위치 처리
                    !slot.IsReady( // 슬롯 호출 시작
                        currentTime // 현재·시간 처리
                    )) // 대상 처리
                {
                    continue; // 다음 반복 진행
                }

                if (!TryAttack( // 시도·공격 조건 확인
                    slot, // 슬롯 항목 전달
                    slot.Weapon // 슬롯·포탑 처리
                )) // 대상 처리
                {
                    continue; // 다음 반복 진행
                }

                NotifyGradeEffect( // 알림·등급 호출 시작
                    slot, // 슬롯 항목 전달
                    WeaponGradeEffectTrigger.Attack // 포탑·등급 처리
                ); // 앞선 항목 종료

                slot.StartCooldown( // 슬롯·시작 호출 시작
                    currentTime // 현재·시간 처리
                ); // 앞선 항목 종료
            }
        }

        private bool TryAttack( // 시도·공격 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            switch (weapon.AttackType) // 포탑·공격 종류 분기
            {
                case WeaponAttackType.Melee: // 포탑·공격 경우 선택
                    return TryMeleeAttack( // 시도·공격 결과 반환
                        slot, // 슬롯 항목 전달
                        weapon // 포탑 처리
                    ); // 앞선 항목 종료

                case WeaponAttackType.StraightProjectile: // 포탑·공격 경우 선택
                    return TryStraightProjectileAttack( // 시도·투사체 결과 반환
                        slot, // 슬롯 항목 전달
                        weapon // 포탑 처리
                    ); // 앞선 항목 종료

                case WeaponAttackType.Area: // 포탑·공격 경우 선택
                    return TryAreaAttack( // 시도·공격 결과 반환
                        slot, // 슬롯 항목 전달
                        weapon // 포탑 처리
                    ); // 앞선 항목 종료

                default: // 기본 경우 선택
                    return false; // 실패 결과 반환
            }
        }

        private bool TryMeleeAttack( // 시도·공격 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            Vector3 origin = // 벡터·발사 위치 값 설정
                slot.Origin.position; // 슬롯·발사 위치 처리

            WeaponTarget target = // 포탑·대상 값 설정
                WeaponTarget.FindClosest( // 포탑·대상 호출 시작
                    origin, // 발사 위치 항목 전달
                    float.PositiveInfinity // 사거리와 관계없는 최근접 대상
                ); // 앞선 항목 종료

            if (target == null) // 대상 조건 확인
            {
                return false; // 실패 결과 반환
            }

            TurretAttackTracer.Spawn(origin, target.transform.position, turretProjectileSprite, weapon.ArtworkTint); // 몸통에서 최근접 적까지 발사 표시
            if (slot.Grade >= 5 && weapon.GradeFiveWideStrikeRadius > 0f && slot.CountWideStrike(weapon.GradeFiveWideStrikeInterval)) // 실제 주기 강타 확인
            {
                Vector3 center = target.transform.position; // 최근접 대상의 실제 위치
                float rangeScale = weapon.Range > 0f ? slot.EffectiveRange / weapon.Range : 1f; // 합성 범위 강화 배율
                float radius = weapon.GradeFiveWideStrikeRadius * rangeScale; // 실제 강타 범위
                WeaponAttributeAttackSnapshot normal = CreateAttackSnapshot(slot, origin); // 몸통 원점 공격 정보
                WeaponAttributeAttackSnapshot wide = new WeaponAttributeAttackSnapshot(normal.Weapon, normal.Attribute, normal.AttributeCount, normal.SynergyStage, normal.Grade, normal.Origin, normal.DirectDamage * weapon.GradeFiveWideStrikeDamageMultiplier); // 실제 강타 피해 정보
                WeaponTarget[] targets = UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None); // 강타 전 실제 대상 목록
                foreach (WeaponTarget candidate in targets) // 강타 대상 순회
                {
                    if (candidate != null && candidate.isActiveAndEnabled && candidate.IsAlive && (candidate.transform.position - center).sqrMagnitude <= radius * radius) // 범위 안 생존 대상 확인
                    {
                        ApplyWeaponHit(wide, candidate, candidate.transform.position); // 실제 강타 피해 적용
                    }
                }
                SpawnAttackPulse(center, radius, new Color(1f, 0.65f, 0.25f, 0.75f), 0.2f); // 실제 강타 범위 표시
                return true; // 강타 공격 완료
            }
            WeaponTarget second = null; // 추가 공격 대상 초기화
            if (weapon.GetMeleeTargetLimit(slot.Grade) >= 2) // 무기 등급별 두 대상 공격 확인
            {
                second = WeaponTarget.FindClosest(origin, float.PositiveInfinity, target); // 첫 대상을 제외한 최근접 적
            }
            WeaponTarget third = null; // 셋째 공격 대상 초기화
            if (weapon.GetMeleeTargetLimit(slot.Grade) >= 3) // 오 등급 추가 대상 확인
            {
                third = WeaponTarget.FindClosest(origin, float.PositiveInfinity, target, second); // 첫째와 둘째를 제외한 최근접 적
            }
            WeaponAttributeAttackSnapshot attack = CreateAttackSnapshot(slot, origin); // 몸통 원점 공격 정보
            ApplyWeaponHit(attack, target, target.transform.position); // 첫 대상 속성 명중
            if (second != null && second.isActiveAndEnabled && second.IsAlive) // 첫 명중 후 추가 대상 유효성 확인
            {
                ApplyWeaponHit(attack, second, second.transform.position); // 둘째 대상 속성 명중
            }
            if (third != null && third.isActiveAndEnabled && third.IsAlive) // 앞선 명중 후 셋째 대상 유효성 확인
            {
                ApplyWeaponHit(attack, third, third.transform.position); // 셋째 대상 속성 명중
            }

            SpawnAttackPulse( // 생성·공격 호출 시작
                origin, // 발사 위치 항목 전달
                slot.EffectiveRange, // 슬롯·범위 항목 전달
                new Color( // 색상 객체 생성
                    1f, // 대상 항목 전달
                    0.65f, // 대상 항목 전달
                    0.25f, // 대상 항목 전달
                    0.75f // 대상 처리
                ), // 대상 항목 전달
                0.12f // 대상 처리
            ); // 앞선 항목 종료

            return true; // 성공 결과 반환
        }

        private bool TryStraightProjectileAttack( // 시도·투사체 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            WeaponTarget target = // 포탑·대상 값 설정
                WeaponTarget.FindClosest( // 포탑·대상 호출 시작
                    slot.Origin.position, // 슬롯·발사 위치 항목 전달
                    float.PositiveInfinity // 사거리와 관계없는 최근접 대상
                ); // 앞선 항목 종료

            if (target == null) // 대상 조건 확인
            {
                return false; // 실패 결과 반환
            }

            FireStraightProjectile( // 화염·투사체 호출 시작
                slot, // 슬롯 항목 전달
                weapon, // 포탑 항목 전달
                target // 대상 처리
            ); // 앞선 항목 종료

            return true; // 성공 결과 반환
        }

        private bool TryAreaAttack( // 시도·공격 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponData weapon // 포탑·데이터 처리
        ) // 앞선 항목 종료
        {
            Vector3 origin = // 벡터·발사 위치 값 설정
                slot.Origin.position; // 슬롯·발사 위치 처리

            WeaponTarget closest = WeaponTarget.FindClosest(origin, float.PositiveInfinity); // 사거리와 관계없는 최근접 대상
            if (closest == null) // 공격 대상 확인
            {
                return false; // 적이 없으면 공격 대기
            }
            TurretAttackTracer.Spawn(origin, closest.transform.position, turretProjectileSprite, weapon.ArtworkTint); // 몸통에서 범위 공격 대상까지 표시
            float attackRange = slot.EffectiveRange * weapon.GetAreaRangeMultiplier(slot.Grade); // 등급별 실제 범위
            Vector3 center = (closest.transform.position - origin).sqrMagnitude > attackRange * attackRange ? closest.transform.position : origin; // 사거리 밖 대상 또는 기존 몸통 중심
            WeaponAttributeAttackSnapshot snapshot = CreateAttackSnapshot(slot, origin); // 실제 몸통 원점 공격 정보
            WeaponAttributeAttackSnapshot attack = new WeaponAttributeAttackSnapshot(snapshot.Weapon, snapshot.Attribute, snapshot.AttributeCount, snapshot.SynergyStage, snapshot.Grade, snapshot.Origin, snapshot.DirectDamage, center); // 실제 범위 중심 포함 공격 정보
            int hitCount = WeaponTarget.VisitAllInRange( // 범위 대상 방문
                center, // 실제 범위 중심 전달
                attackRange, // 등급별 공격 범위 전달
                target => ApplyWeaponHit(attack, target, target.transform.position) // 대상별 명중 적용
            ); // 방문 종료

            if (hitCount <= 0) // 명중·개수 조건 확인
            {
                return false; // 실패 결과 반환
            }

            SpawnAttackPulse( // 생성·공격 호출 시작
                center, // 실제 범위 중심 표시
                attackRange, // 등급별 범위 시각화
                new Color( // 색상 객체 생성
                    0.45f, // 대상 항목 전달
                    0.8f, // 대상 항목 전달
                    1f, // 대상 항목 전달
                    0.65f // 대상 처리
                ), // 대상 항목 전달
                0.2f // 대상 처리
            ); // 앞선 항목 종료

            return true; // 성공 결과 반환
        }

        private void FireStraightProjectile( // 화염·투사체 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponData weapon, // 포탑·데이터 항목 전달
            WeaponTarget target // 포탑·대상 처리
        ) // 앞선 항목 종료
        {
            Vector3 origin = // 벡터·발사 위치 값 설정
                slot.Origin.position; // 슬롯·발사 위치 처리

            Vector2 direction = // 벡터·방향 값 설정
                target.transform.position - // 대상·위치 처리
                origin; // 발사 위치 처리

            if (direction.sqrMagnitude <= // 방향 조건 확인
                0.0001f) // 대상 처리
            {
                direction = // 방향 값 설정
                    slot.Origin.up; // 슬롯·발사 위치 처리
            }

            GameObject projectileObject = // 게임·객체 값 설정
                new GameObject( // 게임·객체 객체 생성
                    $"{weapon.DisplayName}_Projectile" // 포탑·이름 처리
                ); // 앞선 항목 종료

            projectileObject.transform.position = // 투사체·객체 값 설정
                origin; // 발사 위치 처리

            StraightProjectile projectile = // 투사체 값 설정
                projectileObject.AddComponent<StraightProjectile>(); // 투사체·객체 기능 실행

            WeaponAttributeAttackSnapshot attack = CreateAttackSnapshot(slot, origin); // 발사 시점 정보 생성

            projectile.Configure( // 투사체·구성 호출 시작
                direction, // 방향 항목 전달
                attack, // 공격 정보 전달
                attributeCombatEffects, // 속성 효과 전달
                weapon.ProjectileSpeed, // 포탑·투사체 항목 전달
                Mathf.Max(weapon.ProjectileLifetime, direction.magnitude / Mathf.Max(0.01f, weapon.ProjectileSpeed) + 0.5f), // 발사 시점 원거리 목표까지의 최소 수명
                projectileSprite // 투사체·이미지 처리
            ); // 앞선 항목 종료
            projectile.SetArtwork(turretProjectileSprite, weapon.ArtworkTint); // 실제 판정과 분리된 탄 이미지
        }

        private WeaponAttributeAttackSnapshot CreateAttackSnapshot(SnakeWeaponSlot slot, Vector3 origin) // 공격 정보 생성
        {
            float gradeDamage = CalculateSlotDamage(slot); // 등급 피해 계산

            if (attributeCombatEffects != null) // 속성 효과 연결 확인
            {
                return attributeCombatEffects.CreateAttackSnapshot(slot.Weapon, slot.Grade, origin, gradeDamage); // 시너지 공격 정보 반환
            }

            return new WeaponAttributeAttackSnapshot(slot.Weapon, slot.Weapon.Attribute, 0, 0, slot.Grade, origin, gradeDamage); // 기본 공격 정보 반환
        }

        private void ApplyWeaponHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 hitPosition) // 대상 명중 적용
        {
            if (attributeCombatEffects != null) // 속성 효과 연결 확인
            {
                attributeCombatEffects.ApplyHit(attack, target, hitPosition); // 통합 속성 명중 적용
                return; // 대체 피해 방지
            }

            target?.TakeDamage(attack.DirectDamage); // 기본 직접 피해 적용
        }

        private float CalculateSlotDamage( // 계산·슬롯 호출 시작
            SnakeWeaponSlot slot // 포탑·슬롯 처리
        ) // 앞선 항목 종료
        {
            if (slot == null || // 슬롯 조건 확인
                slot.IsEmpty) // 슬롯·빈 상태 처리
            {
                return 0f; // 대상 결과 반환
            }

            return WeaponGradeRules.CalculateDamage( // 포탑·등급 결과 반환
                slot.Weapon.BaseDamage, // 슬롯·포탑 항목 전달
                slot.Grade // 슬롯·등급 처리
            ) * slot.DamageMultiplier * ProjectEpsilon.Progression.MetaModifiers.DamageMultiplier(slot.Grade); // 합성과 영구 성장 피해 적용
        }

        private void NotifyGradeEffect( // 알림·등급 호출 시작
            SnakeWeaponSlot slot, // 포탑·슬롯 항목 전달
            WeaponGradeEffectTrigger trigger // 포탑·등급 처리
        ) // 앞선 항목 종료
        {
            if (gradeEffectHooks == null || // 등급·효과 조건 확인
                slot == null || // 슬롯 처리
                slot.IsEmpty) // 슬롯·빈 상태 처리
            {
                return; // 현재 기능 종료
            }

            Vector3 origin = // 벡터·발사 위치 값 설정
                slot.Origin == null // 슬롯·발사 위치 처리
                    ? transform.position // 위치 처리
                    : slot.Origin.position; // 슬롯·발사 위치 처리

            gradeEffectHooks.Notify( // 등급·효과 호출 시작
                new WeaponGradeEffectContext( // 포탑·등급 객체 생성
                    slot.Weapon, // 슬롯·포탑 항목 전달
                    slot.Grade, // 슬롯·등급 항목 전달
                    origin, // 발사 위치 항목 전달
                    CalculateSlotDamage( // 계산·슬롯 호출 시작
                        slot // 슬롯 처리
                    ), // 대상 항목 전달
                    trigger // 접촉 처리
                ) // 앞선 항목 종료
            ); // 앞선 항목 종료
        }

        private void SpawnAttackPulse( // 생성·공격 호출 시작
            Vector3 origin, // 벡터·발사 위치 항목 전달
            float radius, // 반경 항목 전달
            Color color, // 색상 항목 전달
            float duration // 지속 시간 처리
        ) // 앞선 항목 종료
        {
            GameObject pulseObject = // 게임·객체 값 설정
                new GameObject( // 게임·객체 객체 생성
                    "Weapon_AttackPulse" // 포탑·공격 경로 또는 문구 값
                ); // 앞선 항목 종료

            pulseObject.transform.position = // 객체·위치 값 설정
                origin; // 발사 위치 처리

            WeaponAttackPulse pulse = // 포탑·공격 값 설정
                pulseObject.AddComponent<WeaponAttackPulse>(); // 객체·추가 기능 실행

            pulse.Configure( // 구성 호출 시작
                projectileSprite, // 투사체·이미지 항목 전달
                radius, // 반경 항목 전달
                color, // 색상 항목 전달
                duration // 지속 시간 처리
            ); // 앞선 항목 종료
        }

        private int FindFirstEmptySlotIndex() // 검색·빈 상태 기능 선언
        {
            for (int index = 0; // 순서 반복 처리
                index < slots.Count; // 순서·슬롯 처리
                index++) // 순서 처리
            {
                if (slots[index].IsEmpty) // 슬롯·순서 조건 확인
                {
                    return index; // 순서 결과 반환
                }
            }

            return -1; // 대상 결과 반환
        }

        private static void CopySlotWeapon( // 슬롯·포탑 호출 시작
            SnakeWeaponSlot source, // 포탑·슬롯 항목 전달
            SnakeWeaponSlot destination // 포탑·슬롯 처리
        ) // 앞선 항목 종료
        {
            if (source == null || // 원본 조건 확인
                destination == null) // 대상 처리
            {
                return; // 현재 기능 종료
            }

            if (source.IsEmpty) // 원본·빈 상태 조건 확인
            {
                destination.Clear(); // 대상 기능 실행
                return; // 현재 기능 종료
            }

            destination.Equip( // 대상 호출 시작
                source.Weapon, // 원본·포탑 항목 전달
                source.Grade, // 원본·등급 항목 전달
                source.DamageUpgrades, // 공격력 강화 복사
                source.SpeedUpgrades, // 공격 속도 강화 복사
                source.RangeUpgrades // 사거리 강화 복사
            ); // 앞선 항목 종료
        }

        private static SnakeWeaponSlot FindSlot( // 포탑·슬롯 호출 시작
            List<SnakeWeaponSlot> source, // 목록·포탑 항목 전달
            SnakeSegment owner // 몸통·소유자 처리
        ) // 앞선 항목 종료
        {
            for (int index = 0; // 순서 반복 처리
                index < source.Count; // 순서·원본 처리
                index++) // 순서 처리
            {
                SnakeWeaponSlot slot = // 포탑·슬롯 값 설정
                    source[index]; // 원본·순서 처리

                if (slot != null && // 슬롯 조건 확인
                    slot.Owner == owner) // 슬롯·소유자 처리
                {
                    return slot; // 슬롯 결과 반환
                }
            }

            return null; // 빈 결과 반환
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || // 대상 조건 확인
                bodyManager == null) // 몸통·관리자 처리
            {
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged += // 몸통·관리자 값 설정
                HandleBodyCountChanged; // 몸통·개수 처리

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || // 대상 조건 확인
                bodyManager == null) // 몸통·관리자 처리
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged -= // 몸통·관리자 값 설정
                HandleBodyCountChanged; // 몸통·개수 처리

            subscribed = false; // 대상 값 설정
        }

        private readonly struct WeaponLoadoutEntry // 포탑 자료 구조
        {
            public WeaponData Weapon { get; } // 포탑·데이터 구성 선언
            public int Grade { get; } // 등급·조회 구성 선언
            public int DamageUpgrades { get; } // 공격력 강화 횟수
            public int SpeedUpgrades { get; } // 공격 속도 강화 횟수
            public int RangeUpgrades { get; } // 사거리 강화 횟수

            public WeaponLoadoutEntry( // 포탑 호출 시작
                WeaponData weapon, // 포탑·데이터 항목 전달
                int grade, // 등급 항목 전달
                int damage, // 공격력 강화 횟수
                int speed, // 공격 속도 강화 횟수
                int range // 사거리 강화 횟수
            ) // 앞선 항목 종료
            {
                Weapon = weapon; // 포탑 값 설정
                Grade = grade; // 등급 값 설정
                DamageUpgrades = damage; // 공격력 강화 저장
                SpeedUpgrades = speed; // 공격 속도 강화 저장
                RangeUpgrades = range; // 사거리 강화 저장
            }
        }
    }
}
