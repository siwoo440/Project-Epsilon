namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDay15SetupRules // Setup 완료 규칙
    {
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool configurationValid) // 이전 Setup 삭제 가능 여부
        {
            return sceneSaved && configurationValid; // 저장과 검증 동시 성공
        }

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        {
            return expected != null && object.ReferenceEquals(expected, actual); // 비어 있지 않은 정확한 인스턴스 비교
        }
    }
}
