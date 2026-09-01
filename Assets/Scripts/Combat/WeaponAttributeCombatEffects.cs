using System.Collections.Generic; // 대상 집합 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponAttributeCombatEffects : MonoBehaviour // 속성 전투 효과 관리자
    {
        [SerializeField] private WeaponAttributeSynergyManager synergyManager; // 시너지 관리자 참조
        [SerializeField] private WeaponAttributeEffectHooks effectHooks; // 속성 Hook 참조
        [SerializeField] private Sprite pulseSprite; // 명중 Pulse 이미지

        public WeaponAttributeSynergyManager SynergyManager => synergyManager; // 시너지 관리자 반환
        public WeaponAttributeEffectHooks EffectHooks => effectHooks; // 속성 Hook 반환

        public void Configure(WeaponAttributeSynergyManager manager, WeaponAttributeEffectHooks hooks, Sprite visual) // 참조 구성
        {
            synergyManager = manager; // 시너지 관리자 저장
            effectHooks = hooks; // Hook 저장
            pulseSprite = visual; // Pulse 이미지 저장
        }

        public WeaponAttributeAttackSnapshot CreateAttackSnapshot(WeaponData weapon, int grade, Vector3 origin, float gradeDamage) // 공격 정보 생성
        {
            WeaponAttribute attribute = weapon == null ? WeaponAttribute.Physical : weapon.Attribute; // 공격 속성 조회
            int count = synergyManager == null ? 0 : synergyManager.GetCount(attribute); // 속성 개수 조회
            int stage = synergyManager == null ? 0 : synergyManager.GetStage(attribute); // 시너지 단계 조회
            float directDamage = WeaponAttributeDamageRules.CalculateDirectDamage(gradeDamage, attribute, stage); // 직접 피해 계산
            return new WeaponAttributeAttackSnapshot(weapon, attribute, count, stage, grade, origin, directDamage); // 공격 정보 반환
        }

        public void ApplyHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 hitPosition) // 실제 명중 적용
        {
            if (target == null || !target.IsAlive) // 대상 유효성 확인
            {
                return; // 적용 중단
            }

            target.TakeDamage(attack.DirectDamage); // 직접 피해 적용

            if (attack.Attribute == WeaponAttribute.Fire && target.IsAlive) // 생존 화염 명중 확인
            {
                ApplyFire(attack, target); // 화상 적용
            }

            if (attack.Attribute == WeaponAttribute.Cold && target.IsAlive) // 생존 냉기 명중 확인
            {
                ApplyCold(attack, target); // 냉기 적용
            }

            WeaponAttributeHitContext hit = new WeaponAttributeHitContext(attack, target, hitPosition); // 주 대상 명중 정보 생성
            effectHooks?.NotifyHit(hit); // 주 대상 명중 Hook 전달
            SpawnHitPulse(attack.Attribute, attack.SynergyStage, hitPosition); // 주 대상 속성 명중 표시

            if (attack.Attribute == WeaponAttribute.Electric) // 전기 명중 확인
            {
                ApplyElectricChain(attack, target, hitPosition); // 전기 연쇄 공격 적용
            }
        }

        private static void ApplyFire(WeaponAttributeAttackSnapshot attack, WeaponTarget target) // 화상 적용
        {
            float duration = WeaponAttributeDamageRules.GetFireDuration(attack.SynergyStage); // 화상 지속 시간 계산
            float damagePerSecond = WeaponAttributeDamageRules.CalculateFireDamagePerSecond(attack.DirectDamage, attack.SynergyStage); // 화상 초당 피해 계산

            if (duration <= 0f || damagePerSecond <= 0f) // 활성 화상 여부 확인
            {
                return; // 적용 생략
            }

            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 기존 화상 조회

            if (burn == null) // 화상 없음 확인
            {
                burn = target.gameObject.AddComponent<WeaponTargetBurnStatus>(); // 화상 컴포넌트 생성
            }

            burn.Apply(target, attack.SynergyStage, duration, damagePerSecond); // 화상 상태 갱신
        }

        private static void ApplyCold(WeaponAttributeAttackSnapshot attack, WeaponTarget target) // 냉기 적용
        {
            if (!WeaponAttributeControlRules.IsActiveSynergy(attack.SynergyStage)) // 냉기 시너지 활성 확인
            {
                return; // 냉기 적용 생략
            }

            WeaponTargetColdStatus cold = target.GetComponent<WeaponTargetColdStatus>(); // 기존 냉기 상태 조회

            if (cold == null) // 냉기 상태 없음 확인
            {
                cold = target.gameObject.AddComponent<WeaponTargetColdStatus>(); // 냉기 상태 추가
            }

            cold.Apply(target, attack.SynergyStage); // 냉기 누적과 감속 적용
        }

        private void ApplyElectricChain(WeaponAttributeAttackSnapshot attack, WeaponTarget firstTarget, Vector3 firstHitPosition) // 전기 연쇄 적용
        {
            if (attack.Weapon == null || !WeaponAttributeControlRules.IsActiveSynergy(attack.SynergyStage)) // 전기 연쇄 가능 여부 확인
            {
                return; // 연쇄 공격 생략
            }

            int remainingTargets = WeaponAttributeControlRules.GetElectricSecondaryTargetCount(attack.SynergyStage); // 추가 대상 수 계산
            float rangeMultiplier = WeaponAttributeControlRules.GetElectricChainRangeMultiplier(attack.SynergyStage); // 연쇄 범위 배율 계산
            float chainRange = Mathf.Max(0f, attack.Weapon.Range * rangeMultiplier); // 실제 연쇄 범위 계산

            if (remainingTargets <= 0 || chainRange <= 0f) // 연쇄 조건 확인
            {
                return; // 연쇄 공격 생략
            }

            HashSet<WeaponTarget> visitedTargets = new HashSet<WeaponTarget>(); // 이미 맞은 대상 집합
            visitedTargets.Add(firstTarget); // 첫 대상 제외 등록
            Vector3 searchOrigin = firstHitPosition; // 첫 연쇄 중심 저장

            for (int index = 0; index < remainingTargets; index++) // 연쇄 대상 수만큼 반복
            {
                WeaponTarget nextTarget = FindClosestUnvisitedTarget(searchOrigin, chainRange, visitedTargets); // 다음 대상 탐색

                if (nextTarget == null) // 다음 대상 없음 확인
                {
                    break; // 연쇄 종료
                }

                Vector3 nextHitPosition = nextTarget.transform.position; // 다음 명중 위치 저장
                visitedTargets.Add(nextTarget); // 다음 대상 방문 등록
                nextTarget.TakeDamage(attack.DirectDamage); // 스냅샷 직접 피해 적용
                WeaponAttributeHitContext chainHit = new WeaponAttributeHitContext(attack, nextTarget, nextHitPosition); // 연쇄 명중 정보 생성
                effectHooks?.NotifyHit(chainHit); // 연쇄 명중 Hook 전달
                SpawnHitPulse(WeaponAttribute.Electric, attack.SynergyStage, nextHitPosition); // 전기 연쇄 Pulse 표시
                searchOrigin = nextHitPosition; // 다음 연쇄 중심 갱신
            }
        }

        private static WeaponTarget FindClosestUnvisitedTarget(Vector3 origin, float range, HashSet<WeaponTarget> visitedTargets) // 미방문 최근접 대상 탐색
        {
            WeaponTarget closest = null; // 최근접 대상 초기화
            float bestDistanceSquared = range * range; // 최대 거리 제곱 저장

            WeaponTarget.VisitAllInRange(origin, range, candidate => // 범위 대상 순회
            {
                if (candidate == null || !candidate.IsAlive || visitedTargets.Contains(candidate)) // 후보 유효성 확인
                {
                    return; // 후보 제외
                }

                float distanceSquared = (candidate.transform.position - origin).sqrMagnitude; // 후보 거리 계산

                if (distanceSquared > bestDistanceSquared) // 기존 후보보다 먼지 확인
                {
                    return; // 후보 유지
                }

                bestDistanceSquared = distanceSquared; // 최근접 거리 갱신
                closest = candidate; // 최근접 대상 갱신
            });

            return closest; // 최근접 대상 반환
        }

        private void SpawnHitPulse(WeaponAttribute attribute, int stage, Vector3 position) // 명중 Pulse 생성
        {
            if (pulseSprite == null || stage <= 0) // 표시 가능 여부 확인
            {
                return; // 표시 생략
            }

            if (attribute != WeaponAttribute.Physical && attribute != WeaponAttribute.Fire && attribute != WeaponAttribute.Cold && attribute != WeaponAttribute.Electric) // Day16 지원 속성 확인
            {
                return; // 미지원 속성 표시 생략
            }

            GameObject pulseObject = new GameObject($"{attribute}_HitPulse"); // Pulse 오브젝트 생성
            pulseObject.transform.position = position; // 명중 위치 배치
            WeaponAttackPulse pulse = pulseObject.AddComponent<WeaponAttackPulse>(); // Pulse 컴포넌트 추가
            Color color = ResolvePulseColor(attribute); // 속성 색상 계산
            float radius = 0.35f + stage * 0.035f; // 단계별 크기 계산
            pulse.Configure(pulseSprite, radius, color, 0.18f); // Pulse 표시 구성
        }

        private static Color ResolvePulseColor(WeaponAttribute attribute) // 속성 Pulse 색상 계산
        {
            if (attribute == WeaponAttribute.Fire) // 화염 속성 확인
            {
                return new Color(1f, 0.3f, 0.05f, 0.8f); // 화염 주황색 반환
            }

            if (attribute == WeaponAttribute.Cold) // 냉기 속성 확인
            {
                return new Color(0.35f, 0.8f, 1f, 0.8f); // 냉기 하늘색 반환
            }

            if (attribute == WeaponAttribute.Electric) // 전기 속성 확인
            {
                return new Color(0.85f, 0.9f, 0.2f, 0.8f); // 전기 황록색 반환
            }

            return new Color(0.95f, 0.95f, 1f, 0.8f); // 물리 밝은 회색 반환
        }
    }
}
