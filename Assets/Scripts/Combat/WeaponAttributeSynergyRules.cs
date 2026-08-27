using ProjectEpsilon.Data; // 무기 속성 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public static class WeaponAttributeSynergyRules // 속성 시너지 규칙
    { // 클래스 시작
        public const int AttributeCount = 8; // 전체 속성 개수

        public static int NormalizeCount(int count) // 속성 개수 정규화
        { // 메서드 시작
            return count < 0 ? 0 : count; // 음수 제거
        } // 메서드 끝

        public static int ResolveStage(int count) // 시너지 단계 계산
        { // 메서드 시작
            int safeCount = NormalizeCount(count); // 안전 개수 계산

            if (safeCount >= 8) // 8개 이상 확인
            { // 조건 시작
                return 8; // 8단계 반환
            } // 조건 끝

            if (safeCount >= 6) // 6개 이상 확인
            { // 조건 시작
                return 6; // 6단계 반환
            } // 조건 끝

            if (safeCount >= 4) // 4개 이상 확인
            { // 조건 시작
                return 4; // 4단계 반환
            } // 조건 끝

            if (safeCount >= 2) // 2개 이상 확인
            { // 조건 시작
                return 2; // 2단계 반환
            } // 조건 끝

            return 0; // 비활성 단계 반환
        } // 메서드 끝

        public static int GetAttributeIndex(WeaponAttribute attribute) // 속성 인덱스 계산
        { // 메서드 시작
            int index = (int)attribute; // 열거형 숫자 변환

            if (index < 0 || index >= AttributeCount) // 범위 검사
            { // 조건 시작
                return -1; // 잘못된 인덱스 반환
            } // 조건 끝

            return index; // 정상 인덱스 반환
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
