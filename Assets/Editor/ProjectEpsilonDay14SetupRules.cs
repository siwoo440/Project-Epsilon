namespace ProjectEpsilon.Editor // 편집기 영역
{ // 네임스페이스 시작
    public static class ProjectEpsilonDay14SetupRules // Setup 완료 규칙
    { // 클래스 시작
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool configurationValid) // 이전 Setup 삭제 가능 여부
        { // 메서드 시작
            return sceneSaved && configurationValid; // 저장과 검증 동시 성공
        } // 메서드 끝

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        { // 메서드 시작
            return expected != null && object.ReferenceEquals(expected, actual); // 비어 있지 않은 정확한 인스턴스 비교
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
