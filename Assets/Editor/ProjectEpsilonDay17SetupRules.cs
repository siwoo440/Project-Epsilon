namespace ProjectEpsilon.Editor // 편집기 영역
{
    public static class ProjectEpsilonDay17SetupRules // Day17 Setup 규칙
    {
        public static bool CanCleanupLegacySetup(bool sceneSaved, bool baselineValid, bool day17Valid, int actualTargetCount, int expectedTargetCount) // 이전 Setup 삭제 가능 여부
        {
            return sceneSaved && baselineValid && day17Valid && expectedTargetCount > 0 && actualTargetCount == expectedTargetCount; // 저장과 전체 검증 동시 성공
        }

        public static bool AreSameReference<T>(T expected, T actual) where T : class // 참조 동일성 검사
        {
            return expected != null && object.ReferenceEquals(expected, actual); // 동일 참조 반환
        }
    }
}
