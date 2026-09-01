namespace ProjectEpsilon.Editor // 편집기 영역
{ // 네임스페이스 시작
    public static class ProjectEpsilonDay18SetupRules // Day18 Setup 규칙
    { // 클래스 시작
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool baselineValid, bool day18Valid) // 이전 Setup 삭제 가능 여부
        { // 메서드 시작
            return sceneSaved && baselineValid && day18Valid; // 전체 검증 성공 반환
        } // 메서드 끝

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        { // 메서드 시작
            return expected != null && object.ReferenceEquals(expected, actual); // 동일 참조 반환
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
