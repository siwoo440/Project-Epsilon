using ProjectEpsilon.Data; // 무기 속성 사용
using ProjectEpsilon.Player; // 플레이어 체력 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public sealed class WeaponAttributePlayerEffects : MonoBehaviour // 플레이어 속성 효과 관리자
    { // 클래스 시작
        [SerializeField] private SnakeHealth snakeHealth; // 공유 체력 참조
        [SerializeField] private SnakeShieldController shieldController; // 공유 보호막 참조
        [SerializeField] private WeaponAttributeEffectHooks effectHooks; // 명중 Hook 참조
        [SerializeField] private Transform effectOrigin; // 플레이어 효과 중심

        private int holyHitCount; // Holy 누적 명중 수
        private float darkAbsorptionRemainder; // Dark 소수 흡수 누적
        private WeaponAttributeAttackSnapshot lastHolyAttack; // 마지막 Holy 공격 정보
        private bool subscribed; // 보호막 구독 상태
        private int lastHolyStage; // 마지막 Holy 단계

        public SnakeHealth SnakeHealth => snakeHealth; // 공유 체력 반환
        public SnakeShieldController ShieldController => shieldController; // 보호막 반환
        public WeaponAttributeEffectHooks EffectHooks => effectHooks; // Hook 반환
        public Transform EffectOrigin => effectOrigin; // 플레이어 효과 중심 반환
        public bool IsConfigured => snakeHealth != null && shieldController != null && effectHooks != null && effectOrigin != null; // 연결 상태 반환

        private void OnEnable() // 활성화 처리
        { // 메서드 시작
            Subscribe(); // 보호막 종료 구독
        } // 메서드 끝

        private void OnDisable() // 비활성화 처리
        { // 메서드 시작
            Unsubscribe(); // 보호막 종료 구독 해제
        } // 메서드 끝

        public void Configure(SnakeHealth health, SnakeShieldController shield, WeaponAttributeEffectHooks hooks, Transform origin) // 참조 구성
        { // 메서드 시작
            Unsubscribe(); // 기존 구독 해제
            snakeHealth = health; // 체력 저장
            shieldController = shield; // 보호막 저장
            effectHooks = hooks; // Hook 저장
            effectOrigin = origin; // 효과 중심 저장
            Subscribe(); // 새 구독 연결
        } // 메서드 끝

        public void HandleHolyHit(WeaponAttributeAttackSnapshot attack) // Holy 명중 처리
        { // 메서드 시작
            lastHolyAttack = attack; // 마지막 공격 저장

            if (lastHolyStage != attack.SynergyStage) // 단계 변경 확인
            { // 조건 시작
                holyHitCount = 0; // 누적 명중 초기화
                lastHolyStage = attack.SynergyStage; // 새 단계 저장
            } // 조건 끝

            holyHitCount = attack.SynergyStage >= 6 ? holyHitCount + 1 : 0; // 보호막 단계 명중 누적

            if (WeaponAttributeHolyRules.ShouldHeal(attack.SynergyStage, Random.value)) // 확률 회복 확인
            { // 조건 시작
                snakeHealth?.Heal(WeaponAttributeHolyRules.HealAmount); // 공유 체력 회복
            } // 조건 끝

            if (shieldController != null && WeaponAttributeHolyRules.ShouldGrantShield(attack.SynergyStage, holyHitCount)) // 보호막 발동 확인
            { // 조건 시작
                shieldController.Apply(WeaponAttributeHolyRules.ShieldAmount, WeaponAttributeHolyRules.ShieldDuration, attack.SynergyStage, attack.DirectDamage); // 보호막 적용
            } // 조건 끝
        } // 메서드 끝

        public void HandleDarkDamage(int synergyStage, float dealtDamage) // Dark 피해 흡수 처리
        { // 메서드 시작
            darkAbsorptionRemainder += WeaponAttributeDarkRules.CalculateAbsorption(dealtDamage, synergyStage); // 소수 흡수 누적
            int wholeHealing = Mathf.FloorToInt(darkAbsorptionRemainder); // 정수 회복량 계산

            if (wholeHealing <= 0) // 회복 가능량 확인
            { // 조건 시작
                return; // 회복 생략
            } // 조건 끝

            darkAbsorptionRemainder -= wholeHealing; // 정수 누적량 차감
            snakeHealth?.Heal(wholeHealing); // 공유 체력 회복 시도
        } // 메서드 끝

        public void HandleDarkKill(int synergyStage, int curseStacks, Vector3 deathPosition) // Dark 처치 처리
        { // 메서드 시작
            if (WeaponAttributeDarkRules.ShouldHealOnKill(synergyStage, curseStacks, Random.value)) // 저주 대상 처치 회복 확인
            { // 조건 시작
                snakeHealth?.Heal(WeaponAttributeDarkRules.KillHealAmount); // 공유 체력 회복
            } // 조건 끝

            if (!WeaponAttributeDarkRules.ShouldSpreadCurse(synergyStage, curseStacks)) // 저주 전파 조건 확인
            { // 조건 시작
                return; // 전파 생략
            } // 조건 끝

            WeaponTarget.VisitAllInRange(deathPosition, WeaponAttributeDarkRules.CurseSpreadRadius, target => ApplySpreadCurse(target, synergyStage)); // 주변 대상 저주 전파
        } // 메서드 끝

        private static void ApplySpreadCurse(WeaponTarget target, int synergyStage) // 단일 대상 저주 전파
        { // 메서드 시작
            if (target == null || !target.IsAlive) // 대상 유효성 확인
            { // 조건 시작
                return; // 전파 생략
            } // 조건 끝

            WeaponTargetDarkCurseStatus curse = target.GetComponent<WeaponTargetDarkCurseStatus>(); // 기존 저주 조회

            if (curse == null) // 저주 없음 확인
            { // 조건 시작
                curse = target.gameObject.AddComponent<WeaponTargetDarkCurseStatus>(); // 저주 컴포넌트 추가
            } // 조건 끝

            curse.Apply(synergyStage, WeaponAttributeDarkRules.CurseSpreadStacks); // 규칙 기반 저주 중첩 적용
        } // 메서드 끝

        private void HandleShieldEnded(SnakeShieldEndContext context) // 보호막 종료 효과 처리
        { // 메서드 시작
            if (!WeaponAttributeHolyRules.CanBurstOnShieldEnd(context.SynergyStage)) // 8단계 종료 확인
            { // 조건 시작
                return; // 종료 효과 생략
            } // 조건 끝

            snakeHealth?.Heal(WeaponAttributeHolyRules.HealAmount); // 소량 공유 체력 회복
            float burstDamage = WeaponAttributeHolyRules.CalculateShieldBurstDamage(context.SourceDamage, context.SynergyStage); // 폭발 피해 계산

            if (burstDamage <= 0f) // 유효 폭발 확인
            { // 조건 시작
                return; // 폭발 생략
            } // 조건 끝

            Vector3 center = effectOrigin == null ? transform.position : effectOrigin.position; // 폭발 중심 계산
            WeaponTarget.VisitAllInRange(center, WeaponAttributeHolyRules.ShieldBurstRadius, target => ApplyHolyBurstTarget(target, center, burstDamage)); // 주변 대상 폭발 적용
        } // 메서드 끝

        private void ApplyHolyBurstTarget(WeaponTarget target, Vector3 center, float burstDamage) // 단일 Holy 폭발 처리
        { // 메서드 시작
            if (target == null || !target.IsAlive) // 대상 유효성 확인
            { // 조건 시작
                return; // 폭발 생략
            } // 조건 끝

            Vector3 hitPosition = target.transform.position; // 명중 위치 저장
            target.TakeDamage(burstDamage); // 폭발 피해 적용
            effectHooks?.NotifyHit(new WeaponAttributeHitContext(lastHolyAttack, target, hitPosition)); // Holy 명중 Hook 전달
        } // 메서드 끝

        private void Subscribe() // 보호막 종료 구독
        { // 메서드 시작
            if (subscribed || shieldController == null) // 구독 가능 여부 확인
            { // 조건 시작
                return; // 구독 생략
            } // 조건 끝

            shieldController.ShieldEnded += HandleShieldEnded; // 종료 이벤트 연결
            subscribed = true; // 구독 상태 저장
        } // 메서드 끝

        private void Unsubscribe() // 보호막 종료 구독 해제
        { // 메서드 시작
            if (!subscribed || shieldController == null) // 해제 가능 여부 확인
            { // 조건 시작
                subscribed = false; // 상태 초기화
                return; // 해제 생략
            } // 조건 끝

            shieldController.ShieldEnded -= HandleShieldEnded; // 종료 이벤트 해제
            subscribed = false; // 구독 상태 초기화
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
