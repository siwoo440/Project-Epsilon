using System; // C# 기본 기능 연결
using ProjectEpsilon.Data; // 데이터 기능 연결
using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Combat // 프로젝트 기능 영역
{
    public enum WeaponMergeUpgrade // 합성 공통 강화 종류
    {
        None, // 강화 없음
        Damage, // 공격력 강화
        AttackSpeed, // 공격 속도 강화
        Range // 사거리 및 범위 강화
    }

    [Serializable] // 대상 설정
    public sealed class SnakeWeaponSlot // 포탑·슬롯 자료 구조
    {
        [SerializeField] private SnakeSegment owner; // 필드·몸통 설정
        [SerializeField] private WeaponData weapon; // 필드·포탑 설정
        [Range(1, 5)] [SerializeField] private int grade = 1; // 범위·필드 설정

        [SerializeField] private int damageUpgrades; // 공격력 강화 횟수
        [SerializeField] private int speedUpgrades; // 공격 속도 강화 횟수
        [SerializeField] private int rangeUpgrades; // 사거리 강화 횟수
        private const float UpgradeBonus = 0.05f; // 강화 1회당 임시 증가율
        private float nextAttackTime; // 다음 값·공격 구성 선언
        private int meleeAttackCount; // 장착 후 근접 공격 횟수
        private int periodicAttackCount; // 장착 후 주기 공격 횟수
        private float lastPeriodicMultiplier = 1f; // 실제 최근 공격의 주기 피해 배율

        public SnakeSegment Owner => owner; // 몸통·소유자 값 반환
        public Transform Origin => owner == null ? null : owner.transform; // 위치·발사 위치 값 반환
        public WeaponData Weapon => weapon; // 포탑·데이터 값 반환
        public int Grade => grade; // 등급 값 반환
        public bool IsEmpty => weapon == null; // 빈 상태·포탑 값 반환
        public int DamageUpgrades => damageUpgrades; // 공격력 강화 횟수 조회
        public int SpeedUpgrades => speedUpgrades; // 공격 속도 강화 횟수 조회
        public int RangeUpgrades => rangeUpgrades; // 사거리 강화 횟수 조회
        public float LastPeriodicMultiplier => lastPeriodicMultiplier; // 실제 최근 주기 피해 배율
        public float DamageMultiplier => 1f + damageUpgrades * UpgradeBonus; // 공격력 배율
        public float SpeedMultiplier => 1f + speedUpgrades * UpgradeBonus; // 공격 속도 배율
        public float EffectiveRange => weapon == null ? 0f : weapon.Range * (1f + rangeUpgrades * UpgradeBonus); // 강화 사거리

        public SnakeWeaponSlot(SnakeSegment segment) // 포탑·슬롯 기능 선언
        {
            owner = segment; // 소유자·몸통 값 설정
            grade = 1; // 등급 값 설정
            nextAttackTime = 0f; // 다음 값·공격 값 설정
            meleeAttackCount = 0; // 새 포탑의 강타 횟수 초기화
        }

        public void SetOwner(SnakeSegment segment) // 설정·소유자 기능 선언
        {
            owner = segment; // 소유자·몸통 값 설정
        }

        public void Equip(WeaponData data, int weaponGrade = 1, int damageBonus = 0, int speedBonus = 0, int rangeBonus = 0) // 무기 및 강화 장착
        {
            weapon = data; // 포탑·데이터 값 설정
            grade = Mathf.Clamp(weaponGrade, 1, 5); // 등급·포탑 값 설정
            damageUpgrades = Mathf.Max(0, damageBonus); // 공격력 강화 저장
            speedUpgrades = Mathf.Max(0, speedBonus); // 공격 속도 강화 저장
            rangeUpgrades = Mathf.Max(0, rangeBonus); // 사거리 강화 저장
            nextAttackTime = 0f; // 다음 값·공격 값 설정
            meleeAttackCount = 0; // 새 장착의 강타 횟수 초기화
            periodicAttackCount = 0; // 새 장착의 주기 공격 횟수 초기화
            lastPeriodicMultiplier = 1f; // 새 장착의 최근 피해 배율 초기화
        }

        public void Clear() // 대상 기능 선언
        {
            weapon = null; // 포탑 값 설정
            meleeAttackCount = 0; // 제거한 포탑의 강타 횟수 초기화
            periodicAttackCount = 0; // 제거한 포탑의 주기 공격 초기화
            lastPeriodicMultiplier = 1f; // 제거한 포탑의 최근 피해 배율 초기화
            grade = 1; // 등급 값 설정
            damageUpgrades = 0; // 공격력 강화 초기화
            speedUpgrades = 0; // 공격 속도 강화 초기화
            rangeUpgrades = 0; // 사거리 강화 초기화
            nextAttackTime = 0f; // 다음 값·공격 값 설정
        }

        public void ApplyUpgrade(WeaponMergeUpgrade upgrade) // 선택 강화 적용
        {
            switch (upgrade) // 강화 종류 분기
            {
                case WeaponMergeUpgrade.Damage: // 공격력 선택
                    damageUpgrades++; // 공격력 강화 증가
                    break; // 분기 종료
                case WeaponMergeUpgrade.AttackSpeed: // 공격 속도 선택
                    speedUpgrades++; // 공격 속도 강화 증가
                    break; // 분기 종료
                case WeaponMergeUpgrade.Range: // 사거리 선택
                    rangeUpgrades++; // 사거리 강화 증가
                    break; // 분기 종료
            }
        }

        public bool IsReady(float currentTime) // 현재·시간 기능 선언
        {
            return weapon != null && currentTime >= nextAttackTime; // 포탑·현재 결과 반환
        }

        public bool CountWideStrike(int interval) // 실제 공격의 주기 강타 확인
        {
            if (interval <= 0) // 비활성 주기 확인
            {
                return false; // 일반 공격 유지
            }
            meleeAttackCount = (meleeAttackCount + 1) % interval; // 주기 내 공격 횟수 갱신
            return meleeAttackCount == 0; // 강타 시점 반환
        }

        public float NextPeriodicMultiplier() // 실제 공격의 주기 피해 배율
        { // 함수 시작
            int interval = weapon == null ? 0 : weapon.GetPeriodicInterval(grade); // 실제 등급의 주기
            if (interval <= 0) // 비활성 주기 확인
            { // 조건 시작
                lastPeriodicMultiplier = 1f; // 기본 공격 배율 복구
                return lastPeriodicMultiplier; // 주기 없는 기본 피해
            } // 조건 종료
            periodicAttackCount = (periodicAttackCount + 1) % interval; // 실제 공격 횟수 갱신
            lastPeriodicMultiplier = periodicAttackCount == 0 ? weapon.GetPeriodicDamageMultiplier(grade) : 1f; // 해당 주기의 실제 피해 배율
            return lastPeriodicMultiplier; // 실제 발사 피해 배율 반환
        } // 함수 종료

        public void StartCooldown(float currentTime) // 시작·현재 기능 선언
        {
            if (weapon == null) // 포탑 조건 확인
            {
                nextAttackTime = currentTime; // 다음 값·공격 값 설정
                return; // 현재 기능 종료
            }

            nextAttackTime = currentTime + Mathf.Max(0.01f, weapon.AttackInterval / (SpeedMultiplier * weapon.GetGradeAttackSpeedMultiplier(grade) * ProjectEpsilon.Progression.MetaModifiers.AttackSpeedMultiplier)); // 합성·등급·영구 성장 공격 주기
        }
    }
}
