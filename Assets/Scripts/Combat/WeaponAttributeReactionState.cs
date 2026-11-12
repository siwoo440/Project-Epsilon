using System; // 숫자 범위 검사
using ProjectEpsilon.Data; // 포탑 속성 종류

namespace ProjectEpsilon.Combat // 전투 규칙 영역
{
    public sealed class WeaponAttributeReactionState // 대상별 반응 재료 상태
    {
        private readonly int[] firstOrders = new int[WeaponAttributeReactionRules.AttributeCount]; // 속성별 최초 적용 순서
        private readonly float[] expirationTimes = new float[WeaponAttributeReactionRules.AttributeCount]; // 속성별 만료 시각

        public WeaponAttributeReactionState() // 빈 반응 재료 초기화
        {
            for (int index = 0; index < firstOrders.Length; index++) // 전체 속성 순회
            {
                firstOrders[index] = -1; // 미적용 표식 저장
            }
        }

        public bool Apply(WeaponAttribute incoming, int appliedOrder, float now, float duration, out WeaponAttributeReaction reaction) // 재료 적용과 반응 판정
        {
            reaction = WeaponAttributeReaction.None; // 반응 결과 초기화
            int index = (int)incoming; // 속성 인덱스
            if (index < 0 || index >= firstOrders.Length || appliedOrder < 0 || float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f) // 입력 유효성 확인
            {
                return false; // 잘못된 재료 거부
            }
            Expire(now); // 만료 재료 정리
            if (WeaponAttributeReactionRules.TrySelectPartner(incoming, firstOrders, out WeaponAttribute partner, out reaction)) // 반응 가능한 기존 재료 확인
            {
                Clear(partner); // 기존 반응 재료 소비
                Clear(incoming); // 신규 속성의 기존 재료 소비
                return true; // 반응 발생 알림
            }
            Record(incoming, appliedOrder, now, duration); // 반응하지 않은 재료 기록
            return false; // 대기 중인 재료 저장
        }

        public bool Record(WeaponAttribute attribute, int appliedOrder, float now, float duration) // 별도 경로로 적용된 재료 기록
        {
            int index = (int)attribute; // 속성 인덱스
            if (index < 0 || index >= firstOrders.Length || appliedOrder < 0 || float.IsNaN(now) || float.IsInfinity(now) || float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f) // 입력 유효성 확인
            {
                return false; // 잘못된 재료 거부
            }
            Expire(now); // 만료 재료 정리
            if (firstOrders[index] < 0) // 최초 적용 여부 확인
            {
                firstOrders[index] = appliedOrder; // 최초 적용 순서 보존
            }
            expirationTimes[index] = now + duration; // 재료의 만료 시각 갱신
            return true; // 재료 기록 완료
        }

        public bool IsActive(WeaponAttribute attribute, float now) // 재료 활성 상태 조회
        {
            int index = (int)attribute; // 속성 인덱스
            return index >= 0 && index < firstOrders.Length && firstOrders[index] >= 0 && expirationTimes[index] > now; // 만료 전 재료 여부
        }

        public void Clear(WeaponAttribute attribute) // 특정 반응 재료 제거
        {
            int index = (int)attribute; // 속성 인덱스
            if (index < 0 || index >= firstOrders.Length) // 유효한 속성 확인
            {
                return; // 잘못된 속성 무시
            }
            firstOrders[index] = -1; // 최초 적용 순서 해제
            expirationTimes[index] = 0f; // 만료 시각 해제
        }

        private void Expire(float now) // 만료된 재료 일괄 제거
        {
            for (int index = 0; index < firstOrders.Length; index++) // 전체 재료 순회
            {
                if (firstOrders[index] >= 0 && expirationTimes[index] <= now) // 만료된 재료 확인
                {
                    Clear((WeaponAttribute)index); // 만료 재료 해제
                }
            }
        }
    }
}
