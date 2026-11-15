namespace ProjectEpsilon.Editor // 편집기 영역
{ // 이름 공간 시작
    public static class ProjectEpsilonDay18SetupRules // Day18 Setup 규칙
    { // 형식 시작
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool baselineValid, bool day18Valid) // 이전 Setup 삭제 가능 여부
        { // 함수 시작
            return sceneSaved && baselineValid && day18Valid; // 전체 검증 성공 반환
        } // 함수 종료

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        { // 코드 블록 시작
            return expected != null && object.ReferenceEquals(expected, actual); // 동일 참조 반환
        } // 코드 블록 종료
    } // 형식 종료
} // 이름 공간 종료
