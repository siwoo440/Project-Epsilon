using ProjectEpsilon.Data; // 데이터 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{ // 이름 공간 시작
    public readonly struct WeaponRewardCandidate // 포탑·보상 자료 구조
    { // 형식 시작
        public WeaponData Weapon { get; } // 포탑·데이터 구성 선언
        public int Grade { get; } // 등급·조회 구성 선언

        public bool IsValid => Weapon != null; // 포탑 값 반환

        public WeaponRewardCandidate( // 포탑·보상 호출 시작
            WeaponData weapon, // 포탑·데이터 항목 전달
            int grade // 등급 처리
        ) // 앞선 항목 종료
        { // 함수 시작
            Weapon = weapon; // 포탑 값 설정
            Grade = grade; // 등급 값 설정
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
