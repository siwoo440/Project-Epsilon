using System; // 이벤트 형식 사용
using System.Collections.Generic; // 대상 목록 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTarget : MonoBehaviour // 공격 가능 대상
    {
        private static readonly List<WeaponTarget> ActiveTargets = new List<WeaponTarget>(); // 활성 대상 목록

        [SerializeField] private float maximumHealth = 30f; // 최대 체력
        [SerializeField] private float currentHealth = 30f; // 현재 체력

        private bool deathHandled; // 사망 처리 여부

        public event Action<WeaponTarget> Died; // 사망 이벤트

        public float CurrentHealth => currentHealth; // 현재 체력 반환
        public float MaximumHealth => maximumHealth; // 최대 체력 반환
        public bool IsAlive => !deathHandled && currentHealth > 0f; // 생존 여부 반환
        public float IncomingDamageMultiplier => ResolvePoisonIncomingDamageMultiplier(); // 받는 피해 배율 반환
        public float OutgoingDamageMultiplier => ResolvePoisonOutgoingDamageMultiplier(); // 주는 피해 배율 반환

        private void OnEnable() // 활성 대상 등록
        {
            if (!ActiveTargets.Contains(this)) // 중복 등록 확인
            {
                ActiveTargets.Add(this); // 활성 목록 추가
            }
        }

        private void OnDisable() // 활성 대상 해제
        {
            ActiveTargets.Remove(this); // 활성 목록 제거
        }

        private void Awake() // 체력 초기화
        {
            maximumHealth = Mathf.Max(1f, maximumHealth); // 최대 체력 보정

            if (currentHealth <= 0f || currentHealth > maximumHealth) // 현재 체력 범위 확인
            {
                currentHealth = maximumHealth; // 최대 체력으로 초기화
            }

            deathHandled = false; // 사망 처리 초기화
        }

        public void Configure(float health) // 체력 구성
        {
            maximumHealth = Mathf.Max(1f, health); // 최대 체력 저장
            currentHealth = maximumHealth; // 현재 체력 회복
            deathHandled = false; // 사망 처리 초기화
        }

        public void TakeDamage(float damage) // 피해 적용
        {
            if (!IsAlive) // 생존 상태 확인
            {
                return; // 피해 생략
            }

            float safeDamage = Mathf.Max(0f, damage); // 음수 피해 제거
            float finalDamage = safeDamage * IncomingDamageMultiplier; // 상태 약화 배율 적용

            if (finalDamage <= 0f) // 유효 피해 확인
            {
                return; // 피해 생략
            }

            currentHealth = Mathf.Max(0f, currentHealth - finalDamage); // 체력 감소

            if (currentHealth <= 0f) // 사망 체력 확인
            {
                Die(); // 사망 처리
            }
        }

        private void Die() // 사망 처리
        {
            if (deathHandled) // 중복 사망 확인
            {
                return; // 중복 처리 방지
            }

            deathHandled = true; // 사망 처리 저장
            currentHealth = 0f; // 체력 0 고정
            Died?.Invoke(this); // 사망 이벤트 전달
            Destroy(gameObject); // 대상 제거
        }

        public static WeaponTarget FindClosest(Vector3 origin, float maximumRange) // 최근접 대상 탐색
        {
            float safeRange = Mathf.Max(0f, maximumRange); // 탐색 범위 정규화
            float bestDistanceSquared = safeRange * safeRange; // 최대 거리 제곱 저장
            WeaponTarget closest = null; // 최근접 대상 초기화

            for (int index = ActiveTargets.Count - 1; index >= 0; index--) // 활성 대상 역순 순회
            {
                WeaponTarget target = ActiveTargets[index]; // 현재 대상 조회

                if (target == null) // 사라진 대상 확인
                {
                    ActiveTargets.RemoveAt(index); // 빈 항목 제거
                    continue; // 다음 대상 이동
                }

                if (!target.isActiveAndEnabled || !target.IsAlive) // 공격 가능 여부 확인
                {
                    continue; // 대상 제외
                }

                float distanceSquared = (target.transform.position - origin).sqrMagnitude; // 중심 거리 계산

                if (distanceSquared > bestDistanceSquared) // 현재 후보보다 먼지 확인
                {
                    continue; // 대상 제외
                }

                bestDistanceSquared = distanceSquared; // 최근접 거리 갱신
                closest = target; // 최근접 대상 저장
            }

            return closest; // 최근접 대상 반환
        }

        public static int DamageAllInRange(Vector3 origin, float maximumRange, float damage) // 범위 피해 적용
        {
            float safeRange = Mathf.Max(0f, maximumRange); // 범위 정규화
            float safeDamage = Mathf.Max(0f, damage); // 피해 정규화

            if (safeRange <= 0f || safeDamage <= 0f) // 적용 가능 여부 확인
            {
                return 0; // 피해 대상 없음 반환
            }

            float rangeSquared = safeRange * safeRange; // 범위 제곱 계산
            int hitCount = 0; // 명중 수 초기화

            for (int index = ActiveTargets.Count - 1; index >= 0; index--) // 활성 대상 역순 순회
            {
                WeaponTarget target = ActiveTargets[index]; // 현재 대상 조회

                if (target == null) // 사라진 대상 확인
                {
                    ActiveTargets.RemoveAt(index); // 빈 항목 제거
                    continue; // 다음 대상 이동
                }

                if (!target.isActiveAndEnabled || !target.IsAlive) // 공격 가능 여부 확인
                {
                    continue; // 대상 제외
                }

                float distanceSquared = (target.transform.position - origin).sqrMagnitude; // 중심 거리 계산

                if (distanceSquared > rangeSquared) // 범위 밖 확인
                {
                    continue; // 대상 제외
                }

                target.TakeDamage(safeDamage); // 대상 피해 적용
                hitCount++; // 명중 수 증가
            }

            return hitCount; // 명중 수 반환
        }

        public static int VisitAllInRange(Vector3 origin, float maximumRange, Action<WeaponTarget> visitor) // 범위 대상 방문
        {
            float safeRange = Mathf.Max(0f, maximumRange); // 안전 범위 계산

            if (safeRange <= 0f || visitor == null) // 실행 가능 여부 확인
            {
                return 0; // 방문 없음 반환
            }

            float rangeSquared = safeRange * safeRange; // 거리 제곱 계산
            int visitedCount = 0; // 방문 수 초기화

            for (int index = ActiveTargets.Count - 1; index >= 0; index--) // 활성 대상 역순 순회
            {
                WeaponTarget target = ActiveTargets[index]; // 현재 대상 조회

                if (target == null) // 사라진 대상 확인
                {
                    ActiveTargets.RemoveAt(index); // 빈 항목 제거
                    continue; // 다음 대상 이동
                }

                if (!target.isActiveAndEnabled || !target.IsAlive) // 공격 가능 여부 확인
                {
                    continue; // 대상 제외
                }

                float distanceSquared = (target.transform.position - origin).sqrMagnitude; // 중심 거리 계산

                if (distanceSquared > rangeSquared) // 범위 밖 확인
                {
                    continue; // 대상 제외
                }

                visitor(target); // 대상 처리 실행
                visitedCount++; // 방문 수 증가
            }

            return visitedCount; // 방문 수 반환
        }

        private float ResolvePoisonIncomingDamageMultiplier() // 독 방어 약화 배율 조회
        {
            WeaponTargetPoisonStatus poison = GetComponent<WeaponTargetPoisonStatus>(); // 독 상태 조회
            return poison == null ? 1f : poison.IncomingDamageMultiplier; // 받는 피해 배율 반환
        }

        private float ResolvePoisonOutgoingDamageMultiplier() // 독 공격 약화 배율 조회
        {
            WeaponTargetPoisonStatus poison = GetComponent<WeaponTargetPoisonStatus>(); // 독 상태 조회
            return poison == null ? 1f : poison.OutgoingDamageMultiplier; // 주는 피해 배율 반환
        }
    }
}
