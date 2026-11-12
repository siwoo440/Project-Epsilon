using System; // 기본 출력
using ProjectEpsilon.Combat; // 실제 반응 규칙
using ProjectEpsilon.Data; // 실제 속성 종류

internal static class Program // 속성 반응 규칙 시험
{
    private static int failures; // 실패 횟수
    private static int Main() // 시험 시작
    {
        Pair(WeaponAttribute.Fire, WeaponAttribute.Poison, WeaponAttributeReaction.CorrosiveBurn); // 부식성 연소 양방향
        Pair(WeaponAttribute.Cold, WeaponAttribute.Electric, WeaponAttributeReaction.CoolingDischarge); // 냉각 방전 양방향
        Pair(WeaponAttribute.Physical, WeaponAttribute.Explosion, WeaponAttributeReaction.ShatteringImpact); // 파쇄 충격 양방향
        Pair(WeaponAttribute.Holy, WeaponAttribute.Fire, WeaponAttributeReaction.SacredExplosion); // 성화 폭발 양방향
        Pair(WeaponAttribute.Dark, WeaponAttribute.Poison, WeaponAttributeReaction.CorrosiveCurse); // 부식 저주 양방향
        Pair(WeaponAttribute.Holy, WeaponAttribute.Dark, WeaponAttributeReaction.LightDarkCancel); // 명암 상쇄 양방향
        Check(WeaponAttributeReactionRules.ResolvePair(WeaponAttribute.Fire, WeaponAttribute.Cold) == WeaponAttributeReaction.None, "미정의 조합"); // 미정의 조합 유지
        int[] orders = { -1, -1, -1, -1, 3, -1, 5, -1 }; // 독과 신성의 최초 적용 순서
        Check(WeaponAttributeReactionRules.TrySelectPartner(WeaponAttribute.Fire, orders, out WeaponAttribute partner, out WeaponAttributeReaction reaction) && partner == WeaponAttribute.Poison && reaction == WeaponAttributeReaction.CorrosiveBurn, "오래된 효과 우선"); // 오래된 독 우선
        orders[4] = 5; // 두 효과의 같은 최초 적용 시각
        Check(WeaponAttributeReactionRules.TrySelectPartner(WeaponAttribute.Fire, orders, out partner, out reaction) && partner == WeaponAttribute.Poison, "동률 속성 순서"); // 동률 시 독 우선
        orders[4] = -1; // 독 효과 소비
        Check(WeaponAttributeReactionRules.TrySelectPartner(WeaponAttribute.Fire, orders, out partner, out reaction) && partner == WeaponAttribute.Holy && reaction == WeaponAttributeReaction.SacredExplosion, "소비된 효과 제외"); // 남은 신성 효과 선택
        Check(!WeaponAttributeReactionRules.TrySelectPartner((WeaponAttribute)8, orders, out partner, out reaction), "잘못된 속성"); // 잘못된 속성 거부
        Check(!WeaponAttributeReactionRules.TrySelectPartner(WeaponAttribute.Fire, null, out partner, out reaction), "누락된 효과 목록"); // 누락 목록 거부
        Check(!WeaponAttributeReactionRules.TrySelectPartner(WeaponAttribute.Fire, new int[7], out partner, out reaction), "잘못된 목록 길이"); // 잘못된 길이 거부
        WeaponAttributeReactionState state = new WeaponAttributeReactionState(); // 빈 대상별 재료 상태
        Check(!state.Apply(WeaponAttribute.Poison, 1, 0f, 4f, out reaction), "첫 재료 적용"); // 독 재료 저장
        Check(!state.Apply(WeaponAttribute.Holy, 2, 1f, 4f, out reaction), "두 번째 재료 적용"); // 독과 반응하지 않는 신성 저장
        Check(!state.Apply(WeaponAttribute.Poison, 3, 2f, 4f, out reaction), "기존 재료 지속시간 갱신"); // 최초 순서 유지 갱신
        Check(state.Apply(WeaponAttribute.Fire, 4, 3f, 4f, out reaction) && reaction == WeaponAttributeReaction.CorrosiveBurn, "갱신 이후 최초 순서 보존"); // 갱신된 독 우선 반응
        Check(!state.IsActive(WeaponAttribute.Poison, 3f) && !state.IsActive(WeaponAttribute.Fire, 3f) && state.IsActive(WeaponAttribute.Holy, 3f), "반응한 두 재료만 소비"); // 관련 없는 신성 유지
        Check(!state.Apply(WeaponAttribute.Fire, 5, 6f, 4f, out reaction) && state.IsActive(WeaponAttribute.Fire, 6f), "만료 재료 반응 방지"); // 만료된 신성 제외
        Check(state.Apply(WeaponAttribute.Holy, 6, 7f, 4f, out reaction) && reaction == WeaponAttributeReaction.SacredExplosion, "소비 후 재적용"); // 새 재료의 후속 반응
        Check(!state.Apply(WeaponAttribute.Fire, 7, 8f, 0f, out reaction) && !state.IsActive(WeaponAttribute.Fire, 8f), "지속시간 없는 재료 거부"); // 무효 재료 차단
        Check(!state.Apply((WeaponAttribute)8, 8, 8f, 1f, out reaction), "범위 밖 재료 거부"); // 잘못된 속성 차단
        WeaponAttributeReactionState spreadState = new WeaponAttributeReactionState(); // 별도 경로 재료 상태
        Check(spreadState.Record(WeaponAttribute.Dark, 1, 0f, 5f), "전파된 저주 기록"); // 기존 전파 경로 재료 기록
        Check(!spreadState.Apply(WeaponAttribute.Fire, 2, 1f, 5f, out reaction), "겹치지 않는 화염 기록"); // 별도 화염 보존
        Check(spreadState.Record(WeaponAttribute.Dark, 3, 2f, 5f), "전파 저주 시간 갱신"); // 재료 시간만 갱신
        Check(spreadState.Apply(WeaponAttribute.Holy, 4, 3f, 5f, out reaction) && reaction == WeaponAttributeReaction.LightDarkCancel && spreadState.IsActive(WeaponAttribute.Fire, 3f), "전파 갱신 후 최초 적용 우선"); // 최초 전파 순서와 다른 상태 보존
        Console.WriteLine(failures == 0 ? "PASS: 속성 반응 조합과 우선순위" : "FAIL: " + failures); // 최종 시험 결과
        return failures == 0 ? 0 : 1; // 실제 종료 코드
    }

    private static void Pair(WeaponAttribute first, WeaponAttribute second, WeaponAttributeReaction expected) // 조합 양방향 검사
    {
        Check(WeaponAttributeReactionRules.ResolvePair(first, second) == expected && WeaponAttributeReactionRules.ResolvePair(second, first) == expected, "양방향 " + expected); // 적용 순서 독립 확인
    }

    private static void Check(bool condition, string name) // 시험 조건 확인
    {
        if (condition) // 성공 조건 확인
        {
            return; // 성공 처리
        }
        failures++; // 실패 수 증가
        Console.WriteLine("FAIL: " + name); // 실패 원인 기록
    }
}
