using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectEpsilon.Combat
{
    public sealed class WeaponTarget : MonoBehaviour
    {
        private static readonly List<WeaponTarget> ActiveTargets =
            new List<WeaponTarget>();

        [SerializeField] private float maximumHealth = 30f;
        [SerializeField] private float currentHealth = 30f;

        private bool deathHandled;

        public event Action<WeaponTarget> Died;

        public float CurrentHealth => currentHealth;
        public float MaximumHealth => maximumHealth;
        public bool IsAlive => !deathHandled && currentHealth > 0f;

        private void OnEnable()
        {
            if (!ActiveTargets.Contains(this))
            {
                ActiveTargets.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveTargets.Remove(this);
        }

        private void Awake()
        {
            maximumHealth = Mathf.Max(1f, maximumHealth);

            if (currentHealth <= 0f || currentHealth > maximumHealth)
            {
                currentHealth = maximumHealth;
            }

            deathHandled = false;
        }

        public void Configure(float health)
        {
            maximumHealth = Mathf.Max(1f, health);
            currentHealth = maximumHealth;
            deathHandled = false;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }

            float safeDamage = Mathf.Max(0f, damage);

            if (safeDamage <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - safeDamage);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (deathHandled)
            {
                return;
            }

            deathHandled = true;
            currentHealth = 0f;

            Died?.Invoke(this);
            Destroy(gameObject);
        }

        public static WeaponTarget FindClosest(
            Vector3 origin,
            float maximumRange
        )
        {
            float safeRange = Mathf.Max(0f, maximumRange);
            float bestDistanceSquared = safeRange * safeRange;
            WeaponTarget closest = null;

            for (int index = ActiveTargets.Count - 1;
                index >= 0;
                index--)
            {
                WeaponTarget target = ActiveTargets[index];

                if (target == null)
                {
                    ActiveTargets.RemoveAt(index);
                    continue;
                }

                if (!target.isActiveAndEnabled || !target.IsAlive)
                {
                    continue;
                }

                float distanceSquared =
                    (target.transform.position - origin).sqrMagnitude;

                if (distanceSquared > bestDistanceSquared)
                {
                    continue;
                }

                bestDistanceSquared = distanceSquared;
                closest = target;
            }

            return closest;
        }

        public static int DamageAllInRange(
            Vector3 origin,
            float maximumRange,
            float damage
        )
        {
            float safeRange = Mathf.Max(0f, maximumRange);
            float safeDamage = Mathf.Max(0f, damage);

            if (safeRange <= 0f || safeDamage <= 0f)
            {
                return 0;
            }

            float rangeSquared = safeRange * safeRange;
            int hitCount = 0;

            for (int index = ActiveTargets.Count - 1;
                index >= 0;
                index--)
            {
                WeaponTarget target = ActiveTargets[index];

                if (target == null)
                {
                    ActiveTargets.RemoveAt(index);
                    continue;
                }

                if (!target.isActiveAndEnabled || !target.IsAlive)
                {
                    continue;
                }

                float distanceSquared =
                    (target.transform.position - origin).sqrMagnitude;

                if (distanceSquared > rangeSquared)
                {
                    continue;
                }

                target.TakeDamage(safeDamage);
                hitCount++;
            }

            return hitCount;
        }

        public static int VisitAllInRange( // 범위 대상 방문
            Vector3 origin, // 중심 위치
            float maximumRange, // 최대 범위
            Action<WeaponTarget> visitor // 대상 처리 함수
        ) // 매개변수 끝
        { // 메서드 시작
            float safeRange = Mathf.Max(0f, maximumRange); // 안전 범위 계산

            if (safeRange <= 0f || visitor == null) // 실행 가능 여부 확인
            { // 조건 시작
                return 0; // 방문 없음 반환
            } // 조건 끝

            float rangeSquared = safeRange * safeRange; // 거리 제곱 계산
            int visitedCount = 0; // 방문 수 초기화

            for (int index = ActiveTargets.Count - 1; index >= 0; index--) // 활성 대상 역순 순회
            { // 반복 시작
                WeaponTarget target = ActiveTargets[index]; // 현재 대상 조회

                if (target == null) // 사라진 대상 확인
                { // 조건 시작
                    ActiveTargets.RemoveAt(index); // 빈 항목 제거
                    continue; // 다음 대상 이동
                } // 조건 끝

                if (!target.isActiveAndEnabled || !target.IsAlive) // 공격 가능 여부 확인
                { // 조건 시작
                    continue; // 대상 제외
                } // 조건 끝

                float distanceSquared = (target.transform.position - origin).sqrMagnitude; // 중심 거리 계산

                if (distanceSquared > rangeSquared) // 범위 밖 확인
                { // 조건 시작
                    continue; // 대상 제외
                } // 조건 끝

                visitor(target); // 대상 처리 실행
                visitedCount++; // 방문 수 증가
            } // 반복 끝

            return visitedCount; // 방문 수 반환
        } // 메서드 끝
    }
}
