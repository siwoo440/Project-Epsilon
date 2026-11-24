using System.Collections.Generic; // 효과 문구 목록
using ProjectEpsilon.Data; // 포탑과 속성 데이터

namespace ProjectEpsilon.UI // 도감 표시 영역
{ // 이름 공간 시작
    public static class TurretCatalogText // 포탑 데이터 기반 등급 효과 표시
    { // 형식 시작
        public static string Describe(WeaponData weapon, int grade) // 실제 등급 효과 목록
        { // 함수 시작
            if (weapon == null || grade > weapon.MaxGrade) // 해당 등급 지원 확인
            { // 조건 시작
                return "도달할 수 없는 등급"; // 등급 상한 안내
            } // 조건 종료
            var effects = new List<string>(); // 표시할 추가 효과
            switch (weapon.AttackType) // 실제 공격 방식별 효과 확인
            { // 조건 시작
                case WeaponAttackType.Melee: // 근접 공격
                    AddBonus(effects, "최대 타격 대상", weapon.GetMeleeTargetLimit(grade) - 1); // 근접 추가 타격 수
                    if (grade >= 5 && weapon.GradeFiveWideStrikeInterval > 0 && weapon.GradeFiveWideStrikeRadius > 0f) // 정해진 주기 강타 확인
                    { // 조건 시작
                        effects.Add($"{weapon.GradeFiveWideStrikeInterval}타마다 반경 {weapon.GradeFiveWideStrikeRadius:0.##} 강타, 피해 ×{weapon.GradeFiveWideStrikeDamageMultiplier:0.##}"); // 강타 수치 표시
                    } // 조건 종료
                    break; // 근접 효과 완료
                case WeaponAttackType.StraightProjectile: // 직선 투사체
                    AddBonus(effects, "추가 관통", weapon.GetProjectileHitLimit(grade) - weapon.ProjectileHitLimit); // 기본값 대비 관통 증가
                    if (grade >= 3 && weapon.GradeThreeTransferRange > 0f) // 전달 반경 확인
                    { // 조건 시작
                        effects.Add($"반경 {weapon.GradeThreeTransferRange:0.##} 내 피해 전달 ×{weapon.GetTransferDamageMultiplier(grade):0.##}"); // 전달 반경과 배율
                    } // 조건 종료
                    break; // 직선 투사체 효과 완료
                case WeaponAttackType.Area: // 범위 공격
                    AddMultiplier(effects, "공격 범위", weapon.GetAreaRangeMultiplier(grade)); // 등급별 공격 범위
                    if (weapon.GetAreaKnockbackDistance(grade) > 0f) // 범위 밀어내기 확인
                    { // 조건 시작
                        effects.Add($"밀어내기 거리 {weapon.GetAreaKnockbackDistance(grade):0.##}, {weapon.GradeFiveAreaKnockbackDuration:0.##}초"); // 밀어내기 거리와 지속
                    } // 조건 종료
                    break; // 범위 효과 완료
            } // 조건 종료
            AddMultiplier(effects, "공격 속도", weapon.GetGradeAttackSpeedMultiplier(grade)); // 등급별 공격 속도
            int period = weapon.GetPeriodicInterval(grade); // 강화 공격 주기
            if (period > 0) // 주기 공격 설정 확인
            { // 조건 시작
                effects.Add($"{period}타마다 피해 ×{weapon.GetPeriodicDamageMultiplier(grade):0.##}"); // 주기 공격 배율
            } // 조건 종료
            AddAttributeEffects(effects, weapon, grade); // 속성별 실제 추가 효과 표시
            return effects.Count == 0 ? "추가 효과 없음" : string.Join(" · ", effects); // 등급 효과 문구 반환
        } // 함수 종료

        private static void AddAttributeEffects(List<string> effects, WeaponData weapon, int grade) // 적용 속성의 등급 수치 추가
        { // 함수 시작
            switch (weapon.Attribute) // 포탑 속성 확인
            { // 조건 시작
                case WeaponAttribute.Fire: // 화염 속성
                    AddMultiplier(effects, "화상 대상 피해", weapon.GetBurnedDamageMultiplier(grade)); // 화상 적의 직접 피해
                    AddMultiplier(effects, "화상 지속", weapon.GetBurnDurationMultiplier(grade)); // 화상 시간 증가
                    AddMultiplier(effects, "화상 피해", weapon.GetBurnDamageMultiplier(grade)); // 화상 틱 피해 증가
                    if (grade >= 5 && weapon.GradeFiveBurnExplosionRadius > 0f) // 처치 폭발 범위 확인
                    { // 조건 시작
                        effects.Add($"화상 적 처치 시 반경 {weapon.GradeFiveBurnExplosionRadius:0.##} 폭발, 피해 ×{weapon.GradeFiveBurnExplosionDamageMultiplier:0.##}"); // 처치 폭발 수치
                    } // 조건 종료
                    break; // 화염 효과 완료
                case WeaponAttribute.Cold: // 냉기 속성
                    AddMultiplier(effects, "냉기 대상 피해", weapon.GetChilledDamageMultiplier(grade)); // 냉기 적의 직접 피해
                    AddMultiplier(effects, "냉기 지속", weapon.GetColdDurationMultiplier(grade)); // 냉기 시간 증가
                    break; // 냉기 효과 완료
                case WeaponAttribute.Electric: // 전기 속성
                    AddBonus(effects, "연쇄 대상", weapon.GetExtraElectricTargets(grade)); // 연쇄 추가 대상 수
                    AddMultiplier(effects, "연쇄 범위", weapon.GetElectricRangeMultiplier(grade)); // 연쇄 범위 증가
                    break; // 전기 효과 완료
                case WeaponAttribute.Poison: // 독 속성
                    AddMultiplier(effects, "독 대상 피해", weapon.GetPoisonedDamageMultiplier(grade)); // 중독 적의 직접 피해
                    AddMultiplier(effects, "독 지속", weapon.GetPoisonDurationMultiplier(grade)); // 중독 시간 증가
                    break; // 독 효과 완료
                case WeaponAttribute.Explosion: // 폭발 속성
                    AddMultiplier(effects, "폭발 범위", weapon.GetExplosionRangeMultiplier(grade)); // 폭발 반경 증가
                    AddMultiplier(effects, "폭발 중심 피해", weapon.GetExplosionCenterMultiplier(grade)); // 중심 피해 증가
                    break; // 폭발 효과 완료
                case WeaponAttribute.Holy: // 신성 속성
                    AddBonus(effects, "회복", weapon.GetHolyHealBonus(grade)); // 발동 시 추가 회복
                    AddBonus(effects, "보호막", weapon.GetHolyShieldBonus(grade)); // 발동 시 추가 보호막
                    break; // 신성 효과 완료
                case WeaponAttribute.Dark: // 암흑 속성
                    AddMultiplier(effects, "저주 대상 피해", weapon.GetCursedDamageMultiplier(grade)); // 저주 적의 직접 피해
                    AddMultiplier(effects, "저주 지속", weapon.GetCurseDurationMultiplier(grade)); // 저주 시간 증가
                    break; // 암흑 효과 완료
            } // 조건 종료
        } // 함수 종료

        private static void AddBonus(List<string> effects, string name, int value) // 양수 추가량 표시
        { // 함수 시작
            if (value > 0) // 효과 증가 확인
            { // 조건 시작
                effects.Add($"{name} +{value}"); // 추가량 문구 기록
            } // 조건 종료
        } // 함수 종료

        private static void AddMultiplier(List<string> effects, string name, float value) // 기본값을 넘는 배율 표시
        { // 함수 시작
            if (value > 1f) // 효과 증가 확인
            { // 조건 시작
                effects.Add($"{name} ×{value:0.##}"); // 배율 문구 기록
            } // 조건 종료
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
