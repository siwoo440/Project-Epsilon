using System; // 기본 형식
using ProjectEpsilon.Progression; // 실제 경험치 구현

namespace UnityEngine // 유니티 대체 형식
{ // 영역 시작
    public class MonoBehaviour // 기본 컴포넌트
    { // 클래스 시작
    } // 클래스 끝
    public sealed class SerializeField : Attribute // 직렬화 표시
    { // 클래스 시작
    } // 클래스 끝
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    { // 클래스 시작
        public DefaultExecutionOrder(int order) // 실행 순서 지정
        { // 생성자 시작
        } // 생성자 끝
    } // 클래스 끝
    public static class Application // 실행 상태
    { // 클래스 시작
        public static bool isPlaying => false; // 편집 상태
    } // 클래스 끝
    public static class Mathf // 수학 함수
    { // 클래스 시작
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
        public static int RoundToInt(float value) => (int)Math.Round(value, MidpointRounding.ToEven); // 반올림
    } // 클래스 끝
} // 영역 끝

public static class Program // 검증 실행기
{ // 클래스 시작
    public static int Main() // 진입점
    { // 메서드 시작
        int[] expected = { 10, 11, 13, 14, 16 }; // 기획 경험치
        for (int index = 0; index < expected.Length; index++) // 레벨별 확인
        { // 반복 시작
            int actual = SnakeExperience.CalculateRequiredExperience(10, 1.12f, index + 1); // 실제 계산
            if (actual != expected[index]) // 불일치 확인
            { // 조건 시작
                Console.Error.WriteLine($"Level {index + 1}: expected {expected[index]}, got {actual}"); // 실패 기록
                return 1; // 실패 종료
            } // 조건 끝
        } // 반복 끝
        SnakeExperience experience = new SnakeExperience(); // 경험치 객체
        int requests = 0; // 선택 요청 횟수
        experience.LevelUpRequested += level => requests++; // 요청 횟수 기록
        experience.AddExperience(36); // 여러 레벨 분량 획득
        if (requests != 1 || experience.CurrentLevel != 2 || experience.CurrentExperience != 26) // 첫 선택 대기 확인
        { // 조건 시작
            Console.Error.WriteLine("First level-up did not preserve experience."); // 실패 기록
            return 1; // 실패 종료
        } // 조건 끝
        experience.CompletePendingLevelUp(); // 첫 선택 완료
        experience.CompletePendingLevelUp(); // 둘째 선택 완료
        if (requests != 3 || experience.CurrentLevel != 4 || experience.CurrentExperience != 2 || !experience.IsLevelUpPending) // 순차 요청과 잔여량 확인
        { // 조건 시작
            Console.Error.WriteLine("Sequential level-ups lost experience or requests."); // 실패 기록
            return 1; // 실패 종료
        } // 조건 끝
        experience.CompletePendingLevelUp(); // 마지막 선택 완료
        if (experience.IsLevelUpPending) // 선택 대기 종료 확인
        { // 조건 시작
            Console.Error.WriteLine("Final level-up remained pending."); // 실패 기록
            return 1; // 실패 종료
        } // 조건 끝
        Console.WriteLine("Experience thresholds and sequential level-ups passed."); // 성공 기록
        return 0; // 성공 종료
    } // 메서드 끝
} // 클래스 끝
