using ProjectEpsilon.Combat; // 무기 관리자 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using ProjectEpsilon.Player; // Body 관리자 사용
using UnityEngine; // Unity 기능 사용
using UnityEngine.InputSystem; // 키보드 입력 사용

namespace ProjectEpsilon.Debugging // 디버그 영역
{
    public sealed class WeaponAttributeDebugControls : MonoBehaviour // 속성 단축키 제어기
    {
        [SerializeField] private SnakeBodyManager bodyManager; // Body 관리자 참조
        [SerializeField] private SnakeWeaponManager weaponManager; // 무기 관리자 참조
        [SerializeField] private WeaponData fireWeapon; // 화염 테스트 무기
        [SerializeField] private WeaponData fallbackWeapon; // 물리 채움 무기
        [SerializeField] private WeaponData coldWeapon; // 냉기 테스트 무기
        [SerializeField] private WeaponData electricWeapon; // 전기 테스트 무기
        [SerializeField] private WeaponData poisonWeapon; // 독 테스트 무기
        [SerializeField] private WeaponData explosionWeapon; // 폭발 테스트 무기
        [SerializeField] private WeaponData holyWeapon; // 신성 테스트 무기
        [SerializeField] private WeaponData darkWeapon; // 암흑 테스트 무기

        public bool IsConfigured => bodyManager != null && weaponManager != null && fireWeapon != null && fallbackWeapon != null; // Day15 필수 연결 상태
        public bool IsDay16Configured => IsConfigured && coldWeapon != null && electricWeapon != null; // Day16 추가 연결 상태
        public bool IsDay17Configured => IsDay16Configured && poisonWeapon != null && explosionWeapon != null; // Day17 추가 연결 상태
        public bool IsDay18Configured => IsDay17Configured && holyWeapon != null && darkWeapon != null; // Day18 추가 연결 상태
        public SnakeBodyManager BodyManager => bodyManager; // Body 관리자 반환
        public SnakeWeaponManager WeaponManager => weaponManager; // 무기 관리자 반환
        public WeaponData FireWeapon => fireWeapon; // 화염 무기 반환
        public WeaponData FallbackWeapon => fallbackWeapon; // 물리 무기 반환
        public WeaponData ColdWeapon => coldWeapon; // 냉기 무기 반환
        public WeaponData ElectricWeapon => electricWeapon; // 전기 무기 반환
        public WeaponData PoisonWeapon => poisonWeapon; // 독 무기 반환
        public WeaponData ExplosionWeapon => explosionWeapon; // 폭발 무기 반환
        public WeaponData HolyWeapon => holyWeapon; // 신성 무기 반환
        public WeaponData DarkWeapon => darkWeapon; // 암흑 무기 반환

        public void Configure(SnakeBodyManager body, SnakeWeaponManager weapons, WeaponData fire, WeaponData fallback) // Day15 연결 구성
        {
            bodyManager = body; // Body 관리자 저장
            weaponManager = weapons; // 무기 관리자 저장
            fireWeapon = fire; // 화염 무기 저장
            fallbackWeapon = fallback; // 물리 무기 저장
        }

        public void Configure(SnakeBodyManager body, SnakeWeaponManager weapons, WeaponData fire, WeaponData fallback, WeaponData cold, WeaponData electric) // Day16 연결 구성
        {
            Configure(body, weapons, fire, fallback); // 기존 Day15 연결 유지
            coldWeapon = cold; // 냉기 무기 저장
            electricWeapon = electric; // 전기 무기 저장
        }

        public void Configure(SnakeBodyManager body, SnakeWeaponManager weapons, WeaponData fire, WeaponData fallback, WeaponData cold, WeaponData electric, WeaponData poison, WeaponData explosion) // Day17 연결 구성
        {
            Configure(body, weapons, fire, fallback, cold, electric); // 기존 Day16 연결 유지
            poisonWeapon = poison; // 독 무기 저장
            explosionWeapon = explosion; // 폭발 무기 저장
        }

        public void Configure(SnakeBodyManager body, SnakeWeaponManager weapons, WeaponData fire, WeaponData fallback, WeaponData cold, WeaponData electric, WeaponData poison, WeaponData explosion, WeaponData holy, WeaponData dark) // Day18 연결 구성
        { // 메서드 시작
            Configure(body, weapons, fire, fallback, cold, electric, poison, explosion); // 기존 Day17 연결 유지
            holyWeapon = holy; // 신성 무기 저장
            darkWeapon = dark; // 암흑 무기 저장
        } // 메서드 끝

        private void Update() // 매 프레임 입력 처리
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임 진행 여부 확인
            {
                return; // 디버그 입력 중단
            }

            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null) // 키보드 존재 확인
            {
                return; // 입력 처리 생략
            }

            if (keyboard.f1Key.wasPressedThisFrame) // F1 입력 확인
            {
                ApplyAttributeCount(fireWeapon, "Fire", 2); // Fire 2개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f2Key.wasPressedThisFrame) // F2 입력 확인
            {
                ApplyAttributeCount(fireWeapon, "Fire", 4); // Fire 4개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f3Key.wasPressedThisFrame) // F3 입력 확인
            {
                ApplyAttributeCount(fireWeapon, "Fire", 6); // Fire 6개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f4Key.wasPressedThisFrame) // F4 입력 확인
            {
                ApplyAttributeCount(fireWeapon, "Fire", 8); // Fire 8개 구성
                return; // 중복 입력 방지
            }

            bool shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed; // Shift 입력 상태 계산
            bool controlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed; // Ctrl 입력 상태 계산
            bool altPressed = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed; // Alt 입력 상태 계산

            if (keyboard.f8Key.wasPressedThisFrame) // F8 입력 확인
            {
                ApplyExtendedAttributeCount(controlPressed, shiftPressed, altPressed, 2); // Day16부터 Day18 속성 2개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f9Key.wasPressedThisFrame) // F9 입력 확인
            {
                ApplyExtendedAttributeCount(controlPressed, shiftPressed, altPressed, 4); // Day16부터 Day18 속성 4개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f10Key.wasPressedThisFrame) // F10 입력 확인
            {
                ApplyExtendedAttributeCount(controlPressed, shiftPressed, altPressed, 6); // Day16부터 Day18 속성 6개 구성
                return; // 중복 입력 방지
            }

            if (keyboard.f11Key.wasPressedThisFrame) // F11 입력 확인
            {
                ApplyExtendedAttributeCount(controlPressed, shiftPressed, altPressed, 8); // Day16부터 Day18 속성 8개 구성
            }
        }

        private void ApplyExtendedAttributeCount(bool controlPressed, bool shiftPressed, bool altPressed, int requestedCount) // Day16부터 Day18 속성 선택
        {
            if (altPressed) // Day18 조합 확인
            { // 조건 시작
                WeaponData selectedWeapon = shiftPressed ? darkWeapon : holyWeapon; // 신성 또는 암흑 선택
                string label = shiftPressed ? "Dark" : "Holy"; // Day18 로그 이름 선택
                ApplyAttributeCount(selectedWeapon, label, requestedCount); // Day18 속성 개수 구성
                return; // 이전 일차 처리 방지
            } // 조건 끝

            if (controlPressed) // Day17 조합 확인
            {
                WeaponData selectedWeapon = shiftPressed ? explosionWeapon : poisonWeapon; // 독 또는 폭발 선택
                string label = shiftPressed ? "Explosion" : "Poison"; // Day17 로그 이름 선택
                ApplyAttributeCount(selectedWeapon, label, requestedCount); // Day17 속성 개수 구성
                return; // Day16 처리 방지
            }

            WeaponData day16Weapon = shiftPressed ? electricWeapon : coldWeapon; // 냉기 또는 전기 선택
            string day16Label = shiftPressed ? "Electric" : "Cold"; // Day16 로그 이름 선택
            ApplyAttributeCount(day16Weapon, day16Label, requestedCount); // Day16 속성 개수 구성
        }

        private void ApplyAttributeCount(WeaponData selectedWeapon, string label, int requestedCount) // 속성 무기 개수 구성
        {
            if (bodyManager == null || weaponManager == null || selectedWeapon == null || fallbackWeapon == null) // 필수 참조 확인
            {
                Debug.LogWarning("[Project Epsilon] Attribute debug references are missing for " + label + "."); // 누락 경고 출력
                return; // 구성 중단
            }

            int targetCount = Mathf.Clamp(requestedCount, 0, 8); // 요청 개수 제한

            while (bodyManager.CurrentBodyCount < targetCount) // 필요한 Body 확보
            {
                if (!bodyManager.TryAddBody()) // Body 추가 시도
                {
                    break; // 추가 실패 시 종료
                }
            }

            weaponManager.SynchronizeSlots(); // 슬롯과 Body 동기화
            int availableAttributeCount = Mathf.Min(targetCount, weaponManager.SlotCount); // 실제 속성 개수 계산

            for (int index = 0; index < weaponManager.SlotCount; index++) // 전체 슬롯 순회
            {
                WeaponData weapon = index < availableAttributeCount ? selectedWeapon : fallbackWeapon; // 슬롯 무기 선택
                weaponManager.TryEquipAt(index, weapon, 1); // ★1 무기 장착
            }

            if (availableAttributeCount != targetCount) // 요청 개수 충족 여부 확인
            {
                Debug.LogWarning("[Project Epsilon] " + label + " synergy debug requested ×" + targetCount + ", applied ×" + availableAttributeCount + "."); // 부분 적용 경고 출력
                return; // 처리 종료
            }

            Debug.Log("[Project Epsilon] " + label + " synergy debug ×" + availableAttributeCount + " applied."); // 실제 적용 결과 출력
        }
    }
}
