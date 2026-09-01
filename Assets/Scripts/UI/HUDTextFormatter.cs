using UnityEngine;

namespace ProjectEpsilon.UI
{
    public static class HUDTextFormatter
    {
        public static string FormatHealth(int current, int maximum)
        {
            return $"HP {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}";
        }

        public static string FormatHealth(int current, int maximum, int shield) // 보호막 포함 체력 문구 생성
        { // 메서드 시작
            return $"HP {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}  Shield {Mathf.Max(0, shield)}"; // 체력과 보호막 반환
        } // 메서드 끝

        public static string FormatBodyCount(int current, int maximum)
        {
            return $"Body {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}";
        }

        public static string FormatExperience(int current, int required)
        {
            return $"XP {Mathf.Max(0, current)} / {Mathf.Max(0, required)}";
        }

        public static string FormatLevel(int level)
        {
            return $"Lv. {Mathf.Max(1, level)}";
        }

        public static string FormatStamina(int current, int maximum)
        {
            return $"Stamina {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}";
        }

        public static string FormatTime(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes:00}:{seconds:00}";
        }
    }
}
