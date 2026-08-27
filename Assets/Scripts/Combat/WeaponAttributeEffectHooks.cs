using System; // 이벤트 형식 사용
using ProjectEpsilon.Data; // 무기 데이터 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public readonly struct WeaponAttributeEffectContext // 속성 효과 정보
    { // 구조체 시작
        public WeaponData Weapon { get; } // 공격 무기
        public WeaponAttribute Attribute { get; } // 공격 속성
        public int AttributeCount { get; } // 현재 속성 개수
        public int SynergyStage { get; } // 현재 시너지 단계
        public int Grade { get; } // 무기 등급
        public Vector3 Origin { get; } // 공격 위치
        public float Damage { get; } // 계산 피해량

        public WeaponAttributeEffectContext(WeaponData weapon, WeaponAttribute attribute, int attributeCount, int synergyStage, int grade, Vector3 origin, float damage) // 정보 생성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            Attribute = attribute; // 속성 저장
            AttributeCount = WeaponAttributeSynergyRules.NormalizeCount(attributeCount); // 개수 저장
            SynergyStage = WeaponAttributeSynergyRules.ResolveStage(synergyStage); // 단계 저장
            Grade = Mathf.Clamp(grade, 1, 5); // 등급 저장
            Origin = origin; // 위치 저장
            Damage = Mathf.Max(0f, damage); // 피해량 저장
        } // 생성자 끝
    } // 구조체 끝

    public sealed class WeaponAttributeEffectHooks : MonoBehaviour // 속성 효과 연결점
    { // 클래스 시작
        [SerializeField] private WeaponGradeEffectHooks gradeEffectHooks; // 기존 공격 알림 참조
        [SerializeField] private WeaponAttributeSynergyManager synergyManager; // 시너지 관리자 참조

        private bool subscribed; // 구독 상태

        public event Action<WeaponAttributeEffectContext> AttackTriggered; // 속성 공격 알림

        public bool IsConfigured // 연결 완료 상태
        { // 속성 시작
            get // 상태 조회
            { // 접근자 시작
                return gradeEffectHooks != null && synergyManager != null; // 필수 연결 여부 반환
            } // 접근자 끝
        } // 속성 끝

        public WeaponGradeEffectHooks GradeEffectHooks // 연결된 등급 Hook
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return gradeEffectHooks; // 등급 Hook 반환
            } // 접근자 끝
        } // 속성 끝

        public WeaponAttributeSynergyManager SynergyManager // 연결된 시너지 관리자
        { // 속성 시작
            get // 참조 조회
            { // 접근자 시작
                return synergyManager; // 시너지 관리자 반환
            } // 접근자 끝
        } // 속성 끝

        private void OnEnable() // 활성화 처리
        { // 메서드 시작
            Subscribe(); // 이벤트 구독
        } // 메서드 끝

        private void OnDisable() // 비활성화 처리
        { // 메서드 시작
            Unsubscribe(); // 이벤트 구독 해제
        } // 메서드 끝

        public void Configure(WeaponGradeEffectHooks gradeHooks, WeaponAttributeSynergyManager manager) // 연결 구성
        { // 메서드 시작
            Unsubscribe(); // 기존 연결 해제
            gradeEffectHooks = gradeHooks; // 등급 Hook 저장
            synergyManager = manager; // 시너지 관리자 저장
            Subscribe(); // 새 연결 구독
        } // 메서드 끝

        private void HandleEffectTriggered(WeaponGradeEffectContext context) // 기존 효과 알림 처리
        { // 메서드 시작
            if (context.Trigger != WeaponGradeEffectTrigger.Attack || context.Weapon == null) // 공격 알림 여부 확인
            { // 조건 시작
                return; // 비공격 알림 제외
            } // 조건 끝

            WeaponAttribute attribute = context.Weapon.Attribute; // 공격 속성 조회
            int count = synergyManager == null ? 0 : synergyManager.GetCount(attribute); // 현재 속성 개수 조회
            int stage = synergyManager == null ? 0 : synergyManager.GetStage(attribute); // 현재 단계 조회

            AttackTriggered?.Invoke(new WeaponAttributeEffectContext(context.Weapon, attribute, count, stage, context.Grade, context.Origin, context.Damage)); // 속성 공격 정보 전달
        } // 메서드 끝

        private void Subscribe() // 이벤트 구독
        { // 메서드 시작
            if (subscribed || gradeEffectHooks == null) // 구독 가능 여부 확인
            { // 조건 시작
                return; // 구독 생략
            } // 조건 끝

            gradeEffectHooks.EffectTriggered += HandleEffectTriggered; // 공통 효과 알림 연결
            subscribed = true; // 구독 상태 저장
        } // 메서드 끝

        private void Unsubscribe() // 이벤트 구독 해제
        { // 메서드 시작
            if (!subscribed || gradeEffectHooks == null) // 해제 가능 여부 확인
            { // 조건 시작
                subscribed = false; // 상태 초기화
                return; // 해제 생략
            } // 조건 끝

            gradeEffectHooks.EffectTriggered -= HandleEffectTriggered; // 공통 효과 알림 해제
            subscribed = false; // 구독 상태 초기화
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
