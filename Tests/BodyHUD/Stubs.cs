using System; // 기본 자료형

namespace UnityEngine // 유니티 대체 형식
{
    public class MonoBehaviour // 기본 컴포넌트
    {
    }
    public sealed class SerializeField : Attribute // 직렬화 표시
    {
    }
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    {
        public DefaultExecutionOrder(int order) // 실행 순서 지정
        {
        }
    }
    public static class Application // 실행 환경
    {
        public static bool isPlaying = true; // 플레이 상태
    }
    public static class Mathf // 수학 함수
    {
        public static int Max(int first, int second) => Math.Max(first, second); // 정수 최댓값
        public static int FloorToInt(float value) => (int)Math.Floor(value); // 정수 내림
    }
}

namespace UnityEngine.UI // UI 대체 형식
{
    public sealed class Text // 문구 표시
    {
        public string text; // 현재 문구
    }
}

namespace ProjectEpsilon.Player // 몸통 관리자 대체
{
    public sealed class SnakeBodyManager // 몸통 상태
    {
        public event Action<int, int> BodyCountChanged; // 몸통 변경 이벤트
        public int CurrentBodyCount = 3; // 현재 몸통 수
        public int EarnedBodyCount = 3; // 성장 최대 몸통 수
        public int MaximumBodyCount = 20; // 절대 최대 몸통 수
        public void Change(int current, int earned) // 몸통 상태 변경
        {
            CurrentBodyCount = current; // 현재 수 저장
            EarnedBodyCount = earned; // 성장 최대 수 저장
            BodyCountChanged?.Invoke(current, MaximumBodyCount); // 기존 이벤트 전달
        }
    }
}
