using System; // 이벤트 형식 사용
using ProjectEpsilon.Data; // 무기 속성 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 이름 공간 시작
    [DefaultExecutionOrder(90)] // 무기 관리자 이후 실행
    public sealed class WeaponAttributeSynergyManager : MonoBehaviour // 속성 시너지 관리자
    { // 형식 시작
        [SerializeField] private SnakeWeaponManager weaponManager; // 무기 관리자 참조
        [SerializeField] private int[] attributeCounts = new int[WeaponAttributeSynergyRules.AttributeCount]; // 속성별 개수

        private bool subscribed; // 구독 상태

        public event Action SynergyChanged; // 시너지 변경 알림

        public bool IsConfigured // 연결 완료 상태
        { // 코드 블록 시작
            get // 상태 조회
            { // 코드 블록 시작
                return weaponManager != null; // 관리자 연결 여부 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        public SnakeWeaponManager WeaponManager // 연결된 무기 관리자
        { // 코드 블록 시작
            get // 참조 조회
            { // 코드 블록 시작
                return weaponManager; // 무기 관리자 반환
            } // 코드 블록 종료
        } // 코드 블록 종료

        private void OnEnable() // 활성화 처리
        { // 함수 시작
            Subscribe(); // 이벤트 구독
            Recalculate(); // 초기 집계
        } // 함수 종료

        private void Start() // 시작 처리
        { // 함수 시작
            Subscribe(); // 누락 구독 보완
            Recalculate(); // 시작 집계
        } // 함수 종료

        private void OnDisable() // 비활성화 처리
        { // 함수 시작
            Unsubscribe(); // 이벤트 구독 해제
        } // 함수 종료

        public void Configure(SnakeWeaponManager manager) // 관리자 연결
        { // 함수 시작
            Unsubscribe(); // 기존 구독 해제
            weaponManager = manager; // 관리자 저장
            Subscribe(); // 새 관리자 구독
            Recalculate(); // 즉시 재집계
        } // 함수 종료

        public int GetCount(WeaponAttribute attribute) // 속성 개수 조회
        { // 함수 시작
            EnsureCountArray(); // 배열 상태 보정
            int index = WeaponAttributeSynergyRules.GetAttributeIndex(attribute); // 속성 인덱스 계산

            if (index < 0) // 잘못된 속성 확인
            { // 조건 시작
                return 0; // 빈 개수 반환
            } // 조건 종료

            return WeaponAttributeSynergyRules.NormalizeCount(attributeCounts[index]); // 안전 개수 반환
        } // 함수 종료

        public int GetStage(WeaponAttribute attribute) // 시너지 단계 조회
        { // 함수 시작
            return WeaponAttributeSynergyRules.ResolveStage(GetCount(attribute)); // 현재 단계 반환
        } // 함수 종료

        public void Recalculate() // 전체 속성 재집계
        { // 함수 시작
            EnsureCountArray(); // 배열 상태 보정
            int[] nextCounts = new int[WeaponAttributeSynergyRules.AttributeCount]; // 새 집계 배열

            if (weaponManager != null) // 관리자 존재 확인
            { // 조건 시작
                for (int index = 0; index < weaponManager.Slots.Count; index++) // 모든 슬롯 순회
                { // 반복 시작
                    SnakeWeaponSlot slot = weaponManager.Slots[index]; // 현재 슬롯 조회

                    if (slot == null || slot.IsEmpty || slot.Weapon == null) // 빈 슬롯 확인
                    { // 조건 시작
                        continue; // 다음 슬롯 이동
                    } // 조건 종료

                    int attributeIndex = WeaponAttributeSynergyRules.GetAttributeIndex(slot.Weapon.Attribute); // 무기 속성 인덱스

                    if (attributeIndex >= 0) // 정상 속성 확인
                    { // 조건 시작
                        nextCounts[attributeIndex]++; // 무기 하나 집계
                    } // 조건 종료
                } // 반복 종료
            } // 조건 종료

            if (!HasChanged(nextCounts)) // 변경 여부 확인
            { // 조건 시작
                return; // 중복 알림 방지
            } // 조건 종료

            attributeCounts = nextCounts; // 새 집계 저장
            SynergyChanged?.Invoke(); // 변경 알림 전달
        } // 함수 종료

        private bool HasChanged(int[] nextCounts) // 집계 변경 검사
        { // 함수 시작
            for (int index = 0; index < WeaponAttributeSynergyRules.AttributeCount; index++) // 속성별 비교
            { // 반복 시작
                if (attributeCounts[index] != nextCounts[index]) // 값 차이 확인
                { // 조건 시작
                    return true; // 변경 있음 반환
                } // 조건 종료
            } // 반복 종료

            return false; // 변경 없음 반환
        } // 함수 종료

        private void EnsureCountArray() // 집계 배열 보정
        { // 함수 시작
            if (attributeCounts == null || attributeCounts.Length != WeaponAttributeSynergyRules.AttributeCount) // 배열 상태 확인
            { // 조건 시작
                attributeCounts = new int[WeaponAttributeSynergyRules.AttributeCount]; // 정상 배열 생성
            } // 조건 종료
        } // 함수 종료

        private void Subscribe() // 이벤트 구독
        { // 함수 시작
            if (subscribed || weaponManager == null) // 구독 가능 여부 확인
            { // 조건 시작
                return; // 구독 생략
            } // 조건 종료

            weaponManager.SlotsChanged += HandleSlotsChanged; // 슬롯 변경 연결
            subscribed = true; // 구독 상태 저장
        } // 함수 종료

        private void Unsubscribe() // 이벤트 구독 해제
        { // 함수 시작
            if (!subscribed || weaponManager == null) // 해제 가능 여부 확인
            { // 조건 시작
                subscribed = false; // 상태 초기화
                return; // 해제 생략
            } // 조건 종료

            weaponManager.SlotsChanged -= HandleSlotsChanged; // 슬롯 변경 연결 해제
            subscribed = false; // 구독 상태 초기화
        } // 함수 종료

        private void HandleSlotsChanged() // 슬롯 변경 처리
        { // 함수 시작
            Recalculate(); // 속성 재집계
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
