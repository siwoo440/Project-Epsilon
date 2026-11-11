namespace ProjectEpsilon.Core // 프로젝트 기능 영역
{
    public enum GameState // 게임·상태 자료 구조
    {
        Starting, // 대상 항목 전달
        Playing, // 대상 항목 전달
        Paused, // 일시정지 상태
        Defeated, // 몸통 소진 패배 상태
        Victory // 최종 보스 처치 승리 상태
    }
}
