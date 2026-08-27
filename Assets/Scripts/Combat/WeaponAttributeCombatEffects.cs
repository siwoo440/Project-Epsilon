using ProjectEpsilon.Data; // 무기 데이터 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public sealed class WeaponAttributeCombatEffects : MonoBehaviour // 속성 전투 효과 관리자
    { // 클래스 시작
        [SerializeField] private WeaponAttributeSynergyManager synergyManager; // 시너지 관리자 참조
        [SerializeField] private WeaponAttributeEffectHooks effectHooks; // 속성 Hook 참조
        [SerializeField] private Sprite pulseSprite; // 명중 Pulse 이미지

        public WeaponAttributeSynergyManager SynergyManager => synergyManager; // 시너지 관리자 반환
        public WeaponAttributeEffectHooks EffectHooks => effectHooks; // 속성 Hook 반환

        public void Configure(WeaponAttributeSynergyManager manager, WeaponAttributeEffectHooks hooks, Sprite visual) // 참조 구성
        { // 메서드 시작
            synergyManager = manager; // 시너지 관리자 저장
            effectHooks = hooks; // Hook 저장
            pulseSprite = visual; // Pulse 이미지 저장
        } // 메서드 끝

        public WeaponAttributeAttackSnapshot CreateAttackSnapshot(WeaponData weapon, int grade, Vector3 origin, float gradeDamage) // 공격 정보 생성
        { // 메서드 시작
            WeaponAttribute attribute = weapon == null ? WeaponAttribute.Physical : weapon.Attribute; // 공격 속성 조회
            int count = synergyManager == null ? 0 : synergyManager.GetCount(attribute); // 속성 개수 조회
            int stage = synergyManager == null ? 0 : synergyManager.GetStage(attribute); // 시너지 단계 조회
            float directDamage = WeaponAttributeDamageRules.CalculateDirectDamage(gradeDamage, attribute, stage); // 직접 피해 계산
            return new WeaponAttributeAttackSnapshot(weapon, attribute, count, stage, grade, origin, directDamage); // 공격 정보 반환
        } // 메서드 끝

        public void ApplyHit(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 hitPosition) // 실제 명중 적용
        { // 메서드 시작
            if (target == null || !target.IsAlive) // 대상 유효성 확인
            { // 조건 시작
                return; // 적용 중단
            } // 조건 끝

            target.TakeDamage(attack.DirectDamage); // 직접 피해 적용

            if (attack.Attribute == WeaponAttribute.Fire && target.IsAlive) // 생존 화염 명중 확인
            { // 조건 시작
                ApplyFire(attack, target); // 화상 적용
            } // 조건 끝

            WeaponAttributeHitContext hit = new WeaponAttributeHitContext(attack, target, hitPosition); // 명중 정보 생성
            effectHooks?.NotifyHit(hit); // 명중 Hook 전달
            SpawnHitPulse(attack.Attribute, attack.SynergyStage, hitPosition); // 속성 명중 표시
        } // 메서드 끝

        private static void ApplyFire(WeaponAttributeAttackSnapshot attack, WeaponTarget target) // 화상 적용
        { // 메서드 시작
            float duration = WeaponAttributeDamageRules.GetFireDuration(attack.SynergyStage); // 화상 지속 시간 계산
            float damagePerSecond = WeaponAttributeDamageRules.CalculateFireDamagePerSecond(attack.DirectDamage, attack.SynergyStage); // 화상 초당 피해 계산

            if (duration <= 0f || damagePerSecond <= 0f) // 활성 화상 여부 확인
            { // 조건 시작
                return; // 적용 생략
            } // 조건 끝

            WeaponTargetBurnStatus burn = target.GetComponent<WeaponTargetBurnStatus>(); // 기존 화상 조회

            if (burn == null) // 화상 없음 확인
            { // 조건 시작
                burn = target.gameObject.AddComponent<WeaponTargetBurnStatus>(); // 화상 컴포넌트 생성
            } // 조건 끝

            burn.Apply(target, attack.SynergyStage, duration, damagePerSecond); // 화상 상태 갱신
        } // 메서드 끝

        private void SpawnHitPulse(WeaponAttribute attribute, int stage, Vector3 position) // 명중 Pulse 생성
        { // 메서드 시작
            if (pulseSprite == null || stage <= 0) // 표시 가능 여부 확인
            { // 조건 시작
                return; // 표시 생략
            } // 조건 끝

            if (attribute != WeaponAttribute.Physical && attribute != WeaponAttribute.Fire) // Day15 속성 여부 확인
            { // 조건 시작
                return; // 표시 생략
            } // 조건 끝

            GameObject pulseObject = new GameObject($"{attribute}_HitPulse"); // Pulse 오브젝트 생성
            pulseObject.transform.position = position; // 명중 위치 배치
            WeaponAttackPulse pulse = pulseObject.AddComponent<WeaponAttackPulse>(); // Pulse 컴포넌트 추가
            Color color = attribute == WeaponAttribute.Fire ? new Color(1f, 0.3f, 0.05f, 0.8f) : new Color(0.95f, 0.95f, 1f, 0.8f); // 속성 색상 선택
            float radius = 0.35f + stage * 0.035f; // 단계별 크기 계산
            pulse.Configure(pulseSprite, radius, color, 0.18f); // Pulse 표시 구성
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
