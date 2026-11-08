namespace ProjectEpsilon.Core
{
    public enum GameState
    {
        Starting,
        Playing,
        Paused, // 일시정지 상태
        Defeated, // 몸통 소진 패배 상태
        Victory // 최종 보스 처치 승리 상태
    }
}
