namespace ProjectEpsilon.Editor // 편집기 영역
{ // 이름 공간 시작
    public static class ProjectEpsilonDay15SetupRules // Setup 완료 규칙
    { // 형식 시작
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool configurationValid) // 이전 Setup 삭제 가능 여부
        { // 함수 시작
            return sceneSaved && configurationValid; // 저장과 검증 동시 성공
        } // 함수 종료

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        { // 코드 블록 시작
            return expected != null && object.ReferenceEquals(expected, actual); // 비어 있지 않은 정확한 인스턴스 비교
        } // 코드 블록 종료
    } // 형식 종료
} // 이름 공간 종료
