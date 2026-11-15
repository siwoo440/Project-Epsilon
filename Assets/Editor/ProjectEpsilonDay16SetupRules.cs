namespace ProjectEpsilon.Editor // 편집기 영역
{ // 이름 공간 시작
    public static class ProjectEpsilonDay16SetupRules // Day16 Setup 규칙
    { // 형식 시작
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool configurationValid, int actualTargetCount, int expectedTargetCount) // 기존 호출 호환 규칙
        { // 함수 시작
            return CanCleanupLegacySetup(sceneSaved, configurationValid, configurationValid, actualTargetCount, expectedTargetCount); // 기존 검증을 양쪽 검증으로 전달
        } // 함수 종료

        public static bool CanCleanupLegacySetup(bool sceneSaved, bool day15Valid, bool day16Valid, int actualTargetCount, int expectedTargetCount) // 확장 삭제 가능 여부
        { // 함수 시작
            return sceneSaved && day15Valid && day16Valid && expectedTargetCount > 0 && actualTargetCount == expectedTargetCount; // 전체 검증 성공 여부 반환
        } // 함수 종료

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        { // 코드 블록 시작
            return expected != null && object.ReferenceEquals(expected, actual); // 비어 있지 않은 동일 참조 반환
        } // 코드 블록 종료
    } // 형식 종료
} // 이름 공간 종료
