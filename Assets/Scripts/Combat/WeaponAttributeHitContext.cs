using ProjectEpsilon.Data; // 무기 데이터 사용
using UnityEngine; // 위치 형식 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public readonly struct WeaponAttributeAttackSnapshot // 공격 시점 속성 정보
    { // 구조체 시작
        public WeaponData Weapon { get; } // 공격 무기
        public WeaponAttribute Attribute { get; } // 공격 속성
        public int AttributeCount { get; } // 속성 개수
        public int SynergyStage { get; } // 시너지 단계
        public int Grade { get; } // 무기 등급
        public Vector3 Origin { get; } // 공격 시작 위치
        public float DirectDamage { get; } // 최종 직접 피해

        public WeaponAttributeAttackSnapshot(WeaponData weapon, WeaponAttribute attribute, int attributeCount, int synergyStage, int grade, Vector3 origin, float directDamage) // 공격 정보 생성
        { // 생성자 시작
            Weapon = weapon; // 무기 저장
            Attribute = attribute; // 속성 저장
            AttributeCount = WeaponAttributeSynergyRules.NormalizeCount(attributeCount); // 개수 저장
            SynergyStage = WeaponAttributeSynergyRules.ResolveStage(synergyStage); // 단계 저장
            Grade = Mathf.Clamp(grade, 1, 5); // 등급 저장
            Origin = origin; // 시작 위치 저장
            DirectDamage = Mathf.Max(0f, directDamage); // 직접 피해 저장
        } // 생성자 끝
    } // 구조체 끝

    public readonly struct WeaponAttributeHitContext // 실제 명중 정보
    { // 구조체 시작
        public WeaponAttributeAttackSnapshot Attack { get; } // 공격 정보
        public WeaponTarget Target { get; } // 명중 대상
        public Vector3 HitPosition { get; } // 명중 위치

        public WeaponAttributeHitContext(WeaponAttributeAttackSnapshot attack, WeaponTarget target, Vector3 hitPosition) // 명중 정보 생성
        { // 생성자 시작
            Attack = attack; // 공격 정보 저장
            Target = target; // 대상 저장
            HitPosition = hitPosition; // 위치 저장
        } // 생성자 끝
    } // 구조체 끝
} // 네임스페이스 끝
