using System; // 이벤트 형식 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public readonly struct WeaponAttributeEffectContext // 속성 효과 정보
    {
        public WeaponData Weapon { get; } // 공격 무기
        public WeaponAttribute Attribute { get; } // 공격 속성
        public int AttributeCount { get; } // 현재 속성 개수
        public int SynergyStage { get; } // 현재 시너지 단계
        public int Grade { get; } // 무기 등급
        public Vector3 Origin { get; } // 공격 위치
        public float Damage { get; } // 계산 피해량

        public WeaponAttributeEffectContext(WeaponData weapon, WeaponAttribute attribute, int attributeCount, int synergyStage, int grade, Vector3 origin, float damage) // 정보 생성
        {
            Weapon = weapon; // 무기 저장
            Attribute = attribute; // 속성 저장
            AttributeCount = WeaponAttributeSynergyRules.NormalizeCount(attributeCount); // 개수 저장
            SynergyStage = WeaponAttributeSynergyRules.ResolveStage(synergyStage); // 단계 저장
            Grade = Mathf.Clamp(grade, 1, 5); // 등급 저장
            Origin = origin; // 위치 저장
            Damage = Mathf.Max(0f, damage); // 피해량 저장
        }
    }

    public sealed class WeaponAttributeEffectHooks : MonoBehaviour // 속성 효과 연결점
    {
        [SerializeField] private WeaponGradeEffectHooks gradeEffectHooks; // 기존 공격 알림 참조
        [SerializeField] private WeaponAttributeSynergyManager synergyManager; // 시너지 관리자 참조

        private bool subscribed; // 구독 상태

        public event Action<WeaponAttributeEffectContext> AttackTriggered; // 속성 공격 알림
        public event Action<WeaponAttributeHitContext> HitTriggered; // 속성 명중 알림

        public bool IsConfigured // 연결 완료 상태
        {
            get // 상태 조회
            {
                return gradeEffectHooks != null && synergyManager != null; // 필수 연결 여부 반환
            }
        }

        public WeaponGradeEffectHooks GradeEffectHooks // 연결된 등급 Hook
        {
            get // 참조 조회
            {
                return gradeEffectHooks; // 등급 Hook 반환
            }
        }

        public WeaponAttributeSynergyManager SynergyManager // 연결된 시너지 관리자
        {
            get // 참조 조회
            {
                return synergyManager; // 시너지 관리자 반환
            }
        }

        private void OnEnable() // 활성화 처리
        {
            Subscribe(); // 이벤트 구독
        }

        private void OnDisable() // 비활성화 처리
        {
            Unsubscribe(); // 이벤트 구독 해제
        }

        public void Configure(WeaponGradeEffectHooks gradeHooks, WeaponAttributeSynergyManager manager) // 연결 구성
        {
            Unsubscribe(); // 기존 연결 해제
            gradeEffectHooks = gradeHooks; // 등급 Hook 저장
            synergyManager = manager; // 시너지 관리자 저장
            Subscribe(); // 새 연결 구독
        }

        public void NotifyHit(WeaponAttributeHitContext context) // 실제 명중 알림 전달
        {
            if (context.Target == null || context.Attack.Weapon == null) // 명중 정보 유효성 확인
            {
                return; // 알림 생략
            }

            HitTriggered?.Invoke(context); // 명중 이벤트 전달
        }

        private void HandleEffectTriggered(WeaponGradeEffectContext context) // 기존 효과 알림 처리
        {
            if (context.Trigger != WeaponGradeEffectTrigger.Attack || context.Weapon == null) // 공격 알림 여부 확인
            {
                return; // 비공격 알림 제외
            }

            WeaponAttribute attribute = context.Weapon.Attribute; // 공격 속성 조회
            int count = synergyManager == null ? 0 : synergyManager.GetCount(attribute); // 현재 속성 개수 조회
            int stage = synergyManager == null ? 0 : synergyManager.GetStage(attribute); // 현재 단계 조회

            AttackTriggered?.Invoke(new WeaponAttributeEffectContext(context.Weapon, attribute, count, stage, context.Grade, context.Origin, context.Damage)); // 속성 공격 정보 전달
        }

        private void Subscribe() // 이벤트 구독
        {
            if (subscribed || gradeEffectHooks == null) // 구독 가능 여부 확인
            {
                return; // 구독 생략
            }

            gradeEffectHooks.EffectTriggered += HandleEffectTriggered; // 공통 효과 알림 연결
            subscribed = true; // 구독 상태 저장
        }

        private void Unsubscribe() // 이벤트 구독 해제
        {
            if (!subscribed || gradeEffectHooks == null) // 해제 가능 여부 확인
            {
                subscribed = false; // 상태 초기화
                return; // 해제 생략
            }

            gradeEffectHooks.EffectTriggered -= HandleEffectTriggered; // 공통 효과 알림 해제
            subscribed = false; // 구독 상태 초기화
        }
    }
}
