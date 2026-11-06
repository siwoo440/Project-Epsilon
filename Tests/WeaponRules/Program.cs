using System; // 기본 출력
using ProjectEpsilon.Combat; // 무기 규칙 사용

internal static class Program // 무기 규칙 시험
{ // 클래스 시작
    private static int failures; // 실패 횟수

    private static int Main() // 시험 시작
    { // 메서드 시작
        Check("기본 화상 지속", WeaponAttributeDamageRules.GetFireDuration(0), 3f); // 기본 화상 시간 검사
        Check("화상 지속 증가", WeaponAttributeDamageRules.GetFireDuration(4), 3.9f); // 화상 시너지 시간 검사
        Check("기본 화상 피해", WeaponAttributeDamageRules.CalculateFireDamagePerSecond(1f, 0), 1f); // 기본 화상 피해 검사
        Check("화상 피해 증가", WeaponAttributeDamageRules.CalculateFireDamagePerSecond(1f, 2), 1.2f); // 화상 시너지 피해 검사
        Check("화상 재적중", WeaponAttributeDamageRules.ShouldReplaceBurn(0, 0) ? 1f : 0f, 1f); // 기본 화상 갱신 검사
        Check("물리 4단계 피해", WeaponAttributeDamageRules.GetPhysicalMultiplier(4), 1.1f); // 물리 4단계 배율 검사
        Check("물리 8단계 피해", WeaponAttributeDamageRules.GetPhysicalMultiplier(8), 1.1f); // 물리 8단계 배율 검사
        Check("치명타 미활성", WeaponAttributeDamageRules.CalculatePhysicalCriticalDamage(10f, 2, 0f), 10f); // 치명타 비활성 검사
        Check("치명타 성공", WeaponAttributeDamageRules.CalculatePhysicalCriticalDamage(10f, 4, 0.09f), 17.5f); // 치명타 성공 검사
        Check("치명타 경계", WeaponAttributeDamageRules.CalculatePhysicalCriticalDamage(10f, 4, 0.1f), 10f); // 치명타 확률 경계 검사
        Check("치명타 8단계", WeaponAttributeDamageRules.CalculatePhysicalCriticalDamage(10f, 8, 0f), 25f); // 치명타 피해 증가 검사
        Console.WriteLine(failures == 0 ? "PASS" : $"FAIL: {failures}"); // 최종 결과 출력
        return failures == 0 ? 0 : 1; // 종료 코드 반환
    } // 메서드 끝

    private static void Check(string name, float actual, float expected) // 숫자 결과 검사
    { // 메서드 시작
        if (Math.Abs(actual - expected) < 0.0001f) // 허용 오차 확인
        { // 조건 시작
            return; // 성공 처리
        } // 조건 끝
        failures++; // 실패 횟수 증가
        Console.WriteLine($"FAIL: {name}: expected {expected}, actual {actual}"); // 실패 내역 출력
    } // 메서드 끝
} // 클래스 끝
