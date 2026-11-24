using System; // 기본 형식
using System.Reflection; // Unity 직렬화 값 대입
using ProjectEpsilon.Progression; // 실제 경험치 구현

namespace UnityEngine // 유니티 대체 형식
{
    public class MonoBehaviour // 기본 컴포넌트
    {
        protected Transform transform { get; } = new Transform(); // 경험치 지역 판정에 쓰이는 위치 참조
    }
    public sealed class Transform // Unity 이동 정보의 시험 대체 형식
    { // 형식 시작
        public Vector3 position => default; // 통제된 시험 위치
    } // 형식 종료
    public struct Vector3 // Unity 3차원 위치의 시험 대체 형식
    { // 형식 시작
    } // 형식 종료
    public sealed class SerializeField : Attribute // 직렬화 표시
    {
    }
    public sealed class DefaultExecutionOrder : Attribute // 실행 순서 표시
    {
        public DefaultExecutionOrder(int order) // 실행 순서 지정
        {
        }
    }
    public static class Application // 실행 상태
    {
        public static bool isPlaying => false; // 편집 상태
    }
    public static class Mathf // 수학 함수
    {
        public static int Max(int a, int b) => Math.Max(a, b); // 정수 최댓값
        public static float Max(float a, float b) => Math.Max(a, b); // 실수 최댓값
        public static int RoundToInt(float value) => (int)Math.Round(value, MidpointRounding.ToEven); // 반올림
    }
}

namespace ProjectEpsilon.Progression // 경험치 시험의 Unity 에셋 대체 영역
{ // 이름 공간 시작
    public sealed class ExperiencePickupSettings // 씬의 보석 흡수 에셋 자리
    { // 형식 시작
    } // 형식 종료
    public static class MetaModifiers // 기본 영구 성장 배율
    { // 형식 시작
        public static float ExperienceMultiplier => 1f; // 고정 경험치 배율
    } // 형식 종료
} // 이름 공간 종료

namespace ProjectEpsilon.World // 지도 경험치 효과 대체 영역
{ // 이름 공간 시작
    public static class PrototypeSpecialZone // 고정 지역 경험치 효과
    { // 형식 시작
        public static float ExperienceMultiplierAt(UnityEngine.Vector3 position) => 1f; // 지도 효과가 없는 통제 환경
    } // 형식 종료
} // 이름 공간 종료

public static class Program // 검증 실행기
{
    public static int Main() // 진입점
    {
        int[] expected = { 10, 11, 13, 14, 16 }; // 기획 경험치
        for (int index = 0; index < expected.Length; index++) // 레벨별 확인
        {
            int actual = SnakeExperience.CalculateRequiredExperience(10, 1.12f, index + 1); // 실제 계산
            if (actual != expected[index]) // 불일치 확인
            {
                Console.Error.WriteLine($"Level {index + 1}: expected {expected[index]}, got {actual}"); // 실패 기록
                return 1; // 실패 종료
            }
        }
        SnakeExperience experience = new SnakeExperience(); // 경험치 객체
        int requests = 0; // 선택 요청 횟수
        experience.LevelUpRequested += level => requests++; // 요청 횟수 기록
        experience.AddExperience(36); // 여러 레벨 분량 획득
        if (requests != 1 || experience.CurrentLevel != 2 || experience.CurrentExperience != 26) // 첫 선택 대기 확인
        {
            Console.Error.WriteLine("First level-up did not preserve experience."); // 실패 기록
            return 1; // 실패 종료
        }
        experience.CompletePendingLevelUp(); // 첫 선택 완료
        experience.CompletePendingLevelUp(); // 둘째 선택 완료
        if (requests != 3 || experience.CurrentLevel != 4 || experience.CurrentExperience != 2 || !experience.IsLevelUpPending) // 순차 요청과 잔여량 확인
        {
            Console.Error.WriteLine("Sequential level-ups lost experience or requests."); // 실패 기록
            return 1; // 실패 종료
        }
        experience.CompletePendingLevelUp(); // 마지막 선택 완료
        if (experience.IsLevelUpPending) // 선택 대기 종료 확인
        {
            Console.Error.WriteLine("Final level-up remained pending."); // 실패 기록
            return 1; // 실패 종료
        }
        var releaseCurve = new SnakeExperience(); // 출시 시험용 성장 객체
        releaseCurve.Configure(52, 1.062f); // 도입부 이후 기본 곡선 설정
        FieldInfo introductory = typeof(SnakeExperience).GetField("introductoryRequirements", BindingFlags.NonPublic | BindingFlags.Instance); // Unity 씬이 채우는 필드
        introductory.SetValue(releaseCurve, new[] { 10, 14, 24 }); // 씬의 도입부 요구량 재현
        releaseCurve.CopyIntroductoryRequirements()[0] = 1; // 기록용 사본 변경
        int[] releaseRequirements = { 10, 14, 24, 52, 55 }; // 도입부에서 증가 곡선으로 넘어가는 요구량
        foreach (int expectedRequirement in releaseRequirements) // 순서대로 모든 경계 확인
        { // 요구량 반복 시작
            if (releaseCurve.RequiredExperience != expectedRequirement) // 현재 레벨의 요구량 검증
            { // 실패 조건 시작
                Console.Error.WriteLine($"Release level {releaseCurve.CurrentLevel}: expected {expectedRequirement}, got {releaseCurve.RequiredExperience}"); // 도입부 경계 불일치 기록
                return 1; // 실패 종료
            } // 실패 조건 종료
            releaseCurve.AddExperience(expectedRequirement); // 현재 레벨에 필요한 경험치 획득
            releaseCurve.CompletePendingLevelUp(); // 해당 레벨의 선택 완료
        } // 요구량 반복 종료
        releaseCurve.Configure(7, 1f); // 명시 재설정 경로 검증
        if (releaseCurve.RequiredExperience != 7 || releaseCurve.CopyIntroductoryRequirements().Length != 0) // 기존 호출의 곡선 복구 확인
        { // 실패 조건 시작
            Console.Error.WriteLine("Explicit configuration retained scene introductory thresholds."); // 재설정 실패 기록
            return 1; // 실패 종료
        } // 실패 조건 종료
        Console.WriteLine("Experience thresholds and sequential level-ups passed."); // 성공 기록
        return 0; // 성공 종료
    }
}
