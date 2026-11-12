using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    public static class HUDTextFormatter // 대상 자료 구조
    {
        public static string FormatHealth(int current, int maximum) // 체력·현재 기능 선언
        {
            return $"체력 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 체력·최대 결과 반환
        }

        public static string FormatHealth(int current, int maximum, int shield) // 보호막 포함 체력 문구 생성
        {
            return $"체력 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}  보호막 {Mathf.Max(0, shield)}"; // 체력과 보호막 반환
        }

        public static string FormatBodyCount(int current, int maximum) // 몸통·개수 기능 선언
        {
            return $"몸통 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 몸통·최대 결과 반환
        }

        public static string FormatBodyCount(int current, int earned, int maximum) // 몸통 현재 수와 두 상한 표시
        {
            return $"몸통 {Mathf.Max(0, current)} / {Mathf.Max(0, earned)} (최대 {Mathf.Max(0, maximum)})"; // 현재 수와 상한 문구
        }

        public static string FormatExperience(int current, int required) // 경험치·현재 기능 선언
        {
            return $"경험치 {Mathf.Max(0, current)} / {Mathf.Max(0, required)}"; // 경험치·최대 결과 반환
        }

        public static string FormatLevel(int level) // 레벨 기능 선언
        {
            return $"레벨 {Mathf.Max(1, level)}"; // 최대·레벨 결과 반환
        }

        public static string FormatStamina(int current, int maximum) // 스태미나·현재 기능 선언
        {
            return $"스태미나 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 스태미나·최대 결과 반환
        }

        public static string FormatTime(float elapsedSeconds) // 시간 기능 선언
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds)); // 최대 값 설정
            int minutes = totalSeconds / 60; // 대상 값 설정
            int seconds = totalSeconds % 60; // 대상 값 설정

            return $"{minutes:00}:{seconds:00}"; // 대상 결과 반환
        }
    }
}
