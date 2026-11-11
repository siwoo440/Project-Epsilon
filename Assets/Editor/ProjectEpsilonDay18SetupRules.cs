namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDay18SetupRules // Day18 Setup 규칙
    {
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool baselineValid, bool day18Valid) // 이전 Setup 삭제 가능 여부
        {
            return sceneSaved && baselineValid && day18Valid; // 전체 검증 성공 반환
        }

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        {
            return expected != null && object.ReferenceEquals(expected, actual); // 동일 참조 반환
        }
    }
}
