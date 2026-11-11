using ProjectEpsilon.Data; // 데이터 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public readonly struct WeaponMergeCandidate // 포탑·합성 자료 구조
    {
        public WeaponData Weapon { get; } // 포탑·데이터 구성 선언
        public int CurrentGrade { get; } // 현재·등급 구성 선언
        public int ResultGrade { get; } // 결과·등급 구성 선언
        public int FirstSlotIndex { get; } // 슬롯·순서 구성 선언
        public int SecondSlotIndex { get; } // 슬롯·순서 구성 선언

        public bool IsValid => // 대상 값 반환
            Weapon != null && // 포탑 처리
            CurrentGrade >= 1 && // 현재·등급 처리
            CurrentGrade < 5 && // 현재·등급 처리
            ResultGrade == CurrentGrade + 1 && // 결과·등급 처리
            FirstSlotIndex >= 0 && // 슬롯·순서 처리
            SecondSlotIndex >= 0 && // 슬롯·순서 처리
            FirstSlotIndex != SecondSlotIndex; // 슬롯·순서 처리

        public WeaponMergeCandidate( // 포탑·합성 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int currentGrade, // 현재·등급 항목 전달
            int firstSlotIndex, // 슬롯·순서 항목 전달
            int secondSlotIndex // 슬롯·순서 처리
        ) // 앞선 항목 종료
        {
            Weapon = weapon; // 포탑 값 설정
            CurrentGrade = currentGrade; // 현재·등급 값 설정
            ResultGrade = currentGrade + 1; // 결과·등급 값 설정
            FirstSlotIndex = firstSlotIndex; // 슬롯·순서 값 설정
            SecondSlotIndex = secondSlotIndex; // 슬롯·순서 값 설정
        }
    }
}
