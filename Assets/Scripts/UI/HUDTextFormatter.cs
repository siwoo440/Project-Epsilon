using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{ // 이름 공간 시작
    public static class HUDTextFormatter // 전투 화면 글자 구성
    { // 형식 시작
        public static string FormatHealth(int current, int maximum) // 체력·현재 기능 선언
        { // 함수 시작
            return $"체력 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 체력·최대 결과 반환
        } // 함수 종료

        public static string FormatHealth(int current, int maximum, int shield) // 보호막 포함 체력 문구 생성
        { // 함수 시작
            return $"체력 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}  보호막 {Mathf.Max(0, shield)}"; // 체력과 보호막 반환
        } // 함수 종료

        public static string FormatBodyCount(int current, int maximum) // 몸통·개수 기능 선언
        { // 함수 시작
            return $"몸통 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 몸통·최대 결과 반환
        } // 함수 종료

        public static string FormatBodyCount(int current, int earned, int maximum) // 몸통 현재 수와 두 상한 표시
        { // 함수 시작
            return $"몸통 {Mathf.Max(0, current)} / {Mathf.Max(0, earned)} (최대 {Mathf.Max(0, maximum)})"; // 현재 수와 상한 문구
        } // 함수 종료

        public static string FormatExperience(int current, int required) // 경험치·현재 기능 선언
        { // 함수 시작
            return $"경험치 {Mathf.Max(0, current)} / {Mathf.Max(0, required)}"; // 경험치·최대 결과 반환
        } // 함수 종료

        public static string FormatLevel(int level) // 레벨 기능 선언
        { // 함수 시작
            return $"레벨 {Mathf.Max(1, level)}"; // 최대·레벨 결과 반환
        } // 함수 종료

        public static string FormatStamina(int current, int maximum) // 스태미나·현재 기능 선언
        { // 함수 시작
            return $"스태미나 {Mathf.Max(0, current)} / {Mathf.Max(0, maximum)}"; // 스태미나·최대 결과 반환
        } // 함수 종료

        public static string FormatTime(float elapsedSeconds) // 경과 시간을 분과 초로 변환
        { // 함수 시작
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds)); // 음수를 제외한 정수 초
            int minutes = totalSeconds / 60; // 경과한 전체 분
            int seconds = totalSeconds % 60; // 남은 초

            return $"{minutes:00}:{seconds:00}"; // 두 자리 분과 초 표시
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
