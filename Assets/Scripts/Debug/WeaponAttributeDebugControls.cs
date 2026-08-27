using ProjectEpsilon.Combat; // 무기 관리자 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using ProjectEpsilon.Player; // Body 관리자 사용
using UnityEngine; // Unity 기능 사용
using UnityEngine.InputSystem; // 키보드 입력 사용

namespace ProjectEpsilon.Debugging // 디버그 영역
{ // 네임스페이스 시작
    public sealed class WeaponAttributeDebugControls : MonoBehaviour // 속성 단축키 제어기
    { // 클래스 시작
        [SerializeField] private SnakeBodyManager bodyManager; // Body 관리자 참조
        [SerializeField] private SnakeWeaponManager weaponManager; // 무기 관리자 참조
        [SerializeField] private WeaponData fireWeapon; // 화염 테스트 무기
        [SerializeField] private WeaponData fallbackWeapon; // 비화염 채움 무기

        public bool IsConfigured // 연결 완료 상태
        { // 속성 시작
            get // 상태 조회
            { // 접근자 시작
                return bodyManager != null && weaponManager != null && fireWeapon != null && fallbackWeapon != null; // 필수 연결 여부 반환
            } // 접근자 끝
        } // 속성 끝

        public SnakeBodyManager BodyManager // 연결된 Body 관리자
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return bodyManager; // Body 관리자 반환
            } // 접근자 끝
        } // 속성 끝

        public SnakeWeaponManager WeaponManager // 연결된 무기 관리자
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return weaponManager; // 무기 관리자 반환
            } // 접근자 끝
        } // 속성 끝

        public WeaponData FireWeapon // 연결된 화염 무기
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return fireWeapon; // 화염 무기 반환
            } // 접근자 끝
        } // 속성 끝

        public WeaponData FallbackWeapon // 연결된 대체 무기
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return fallbackWeapon; // 대체 무기 반환
            } // 접근자 끝
        } // 속성 끝

        public void Configure(SnakeBodyManager body, SnakeWeaponManager weapons, WeaponData fire, WeaponData fallback) // 디버그 연결
        { // 메서드 시작
            bodyManager = body; // Body 관리자 저장
            weaponManager = weapons; // 무기 관리자 저장
            fireWeapon = fire; // 화염 무기 저장
            fallbackWeapon = fallback; // 대체 무기 저장
        } // 메서드 끝

        private void Update() // 매 프레임 입력 처리
        { // 메서드 시작
            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null) // 키보드 존재 확인
            { // 조건 시작
                return; // 입력 처리 생략
            } // 조건 끝

            if (keyboard.f1Key.wasPressedThisFrame) // F1 입력 확인
            { // 조건 시작
                ApplyFireCount(2); // Fire 2개 구성
            } // 조건 끝
            else if (keyboard.f2Key.wasPressedThisFrame) // F2 입력 확인
            { // 조건 시작
                ApplyFireCount(4); // Fire 4개 구성
            } // 조건 끝
            else if (keyboard.f3Key.wasPressedThisFrame) // F3 입력 확인
            { // 조건 시작
                ApplyFireCount(6); // Fire 6개 구성
            } // 조건 끝
            else if (keyboard.f4Key.wasPressedThisFrame) // F4 입력 확인
            { // 조건 시작
                ApplyFireCount(8); // Fire 8개 구성
            } // 조건 끝
        } // 메서드 끝

        private void ApplyFireCount(int requestedCount) // 화염 개수 구성
        { // 메서드 시작
            if (bodyManager == null || weaponManager == null || fireWeapon == null || fallbackWeapon == null) // 필수 참조 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] Day14 attribute debug references are missing."); // 누락 경고 출력
                return; // 구성 중단
            } // 조건 끝

            int targetCount = Mathf.Clamp(requestedCount, 0, 8); // 요청 개수 제한

            while (bodyManager.CurrentBodyCount < targetCount) // 필요한 Body 확보
            { // 반복 시작
                if (!bodyManager.TryAddBody()) // Body 추가 시도
                { // 조건 시작
                    break; // 추가 실패 시 종료
                } // 조건 끝
            } // 반복 끝

            weaponManager.SynchronizeSlots(); // 슬롯과 Body 동기화
            int availableFireCount = Mathf.Min(targetCount, weaponManager.SlotCount); // 실제 화염 개수 계산

            for (int index = 0; index < weaponManager.SlotCount; index++) // 전체 슬롯 순회
            { // 반복 시작
                WeaponData selectedWeapon = index < availableFireCount ? fireWeapon : fallbackWeapon; // 슬롯 무기 선택
                weaponManager.TryEquipAt(index, selectedWeapon, 1); // ★1 무기 장착
            } // 반복 끝

            if (availableFireCount != targetCount) // 요청 개수 충족 여부 확인
            { // 조건 시작
                Debug.LogWarning("[Project Epsilon] Fire synergy debug requested ×" + targetCount + ", applied ×" + availableFireCount + "."); // 부분 적용 경고 출력
                return; // 처리 종료
            } // 조건 끝

            Debug.Log("[Project Epsilon] Fire synergy debug ×" + availableFireCount + " applied."); // 실제 적용 결과 출력
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
