using System; // 피해 콜백 예외 처리
using ProjectEpsilon.Core; // 실제 게임 진행 상태
using ProjectEpsilon.Data; // 속성 데이터
using UnityEngine; // Unity 기능

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponBurnDeathExplosion : MonoBehaviour // 화상 적 처치의 시험 폭발
    {
        private static bool resolvingExplosion; // 중첩 연쇄 폭발 방지
        private WeaponTarget target; // 실제 화상 대상
        private float radius; // 명중 시점 폭발 반경
        private float damage; // 명중 시점 폭발 피해
        private Sprite visual; // 기존 폭발 표시 이미지
        private bool triggered; // 실제 폭발 중복 방지
        public static void Attach(WeaponTarget victim, WeaponAttributeAttackSnapshot attack, Sprite pulse) // 오 등급 화염탄 효과 연결
        {
            if (victim == null || attack.Weapon == null || attack.Grade < 5 || attack.Weapon.GradeFiveBurnExplosionRadius <= 0f) // 실제 대상과 무기 설정 확인
            {
                return; // 무효 효과 차단
            }
            WeaponBurnDeathExplosion effect = victim.GetComponent<WeaponBurnDeathExplosion>(); // 기존 효과 조회
            if (effect == null) // 기존 효과 부재 확인
            {
                effect = victim.gameObject.AddComponent<WeaponBurnDeathExplosion>(); // 대상별 단일 효과 생성
            }
            effect.Configure(victim, attack, pulse); // 최신 화염탄 공격 정보 저장
        }
        private void Configure(WeaponTarget victim, WeaponAttributeAttackSnapshot attack, Sprite pulse) // 실제 폭발 설정
        {
            if (target != null) // 이전 이벤트 연결 확인
            {
                target.Died -= HandleDeath; // 중복 사망 구독 방지
            }
            target = victim; // 실제 대상 저장
            radius = attack.Weapon.GradeFiveBurnExplosionRadius; // 명중 시점 반경 저장
            damage = attack.DirectDamage * attack.Weapon.GradeFiveBurnExplosionDamageMultiplier; // 실제 공격 기준 폭발 피해
            visual = pulse; // 기존 표시 이미지 저장
            triggered = false; // 새 명중의 폭발 상태 초기화
            target.Died += HandleDeath; // 실제 사망 이벤트 연결
        }
        private void OnDisable() // 실제 대상 비활성 처리
        {
            if (target != null) // 실제 연결 대상 확인
            {
                target.Died -= HandleDeath; // 사망 이벤트 해제
            }
        }
        private void OnEnable() // 실제 대상 재활성 처리
        {
            if (target != null) // 기존 대상 확인
            {
                target.Died -= HandleDeath; // 중복 이벤트 제거
                target.Died += HandleDeath; // 사망 이벤트 복구
            }
        }
        private void HandleDeath(WeaponTarget victim) // 실제 화상 적 사망 처리
        {
            WeaponTargetBurnStatus burn = victim == null ? null : victim.GetComponent<WeaponTargetBurnStatus>(); // 실제 화상 상태 조회
            if (!CanContinueCombat() || triggered || resolvingExplosion || (victim != null && victim.IsReactionDamage) || burn == null || !burn.isActiveAndEnabled || burn.RemainingDuration <= 0f || radius <= 0f || damage <= 0f) // 실제 전투와 화상 폭발 유효성 확인
            {
                return; // 무효 또는 연쇄 폭발 차단
            }
            triggered = true; // 같은 대상 중복 폭발 방지
            resolvingExplosion = true; // 폭발 피해의 연쇄 차단
            try // 실제 폭발 피해 처리
            {
                Vector3 center = victim.transform.position; // 실제 처치 위치
                WeaponTarget[] targets = UnityEngine.Object.FindObjectsByType<WeaponTarget>(FindObjectsSortMode.None); // 폭발 전 실제 대상 목록
                foreach (WeaponTarget candidate in targets) // 실제 폭발 대상 순회
                {
                    if (!CanContinueCombat()) // 앞선 대상의 승리 또는 정지 확인
                    { // 조건 시작
                        break; // 이후 폭발 대상 피해 차단
                    } // 조건 종료
                    if (candidate != null && candidate != victim && candidate.isActiveAndEnabled && candidate.IsAlive && (candidate.transform.position - center).sqrMagnitude <= radius * radius) // 범위 안 생존 대상 확인
                    {
                        candidate.TakeDamage(damage); // 실제 폭발 피해 적용
                    }
                }
                GameObject pulseObject = new GameObject("Fireball_DeathExplosion"); // 실제 폭발 표시 객체
                pulseObject.transform.position = center; // 실제 폭발 표시 위치
                pulseObject.AddComponent<WeaponAttackPulse>().Configure(visual, radius, new Color(1f, 0.35f, 0.1f, 0.8f), 0.2f); // 시험용 화염 폭발 표시
            }
            finally // 예외 발생 후 상태 복구
            {
                resolvingExplosion = false; // 후속 독립 폭발 허용
            }
        }

        private static bool CanContinueCombat() // 실제 화상 폭발 진행 여부
        { // 함수 시작
            return GameManager.Instance == null || GameManager.Instance.IsPlaying; // 게임 관리자 부재 또는 전투 진행 확인
        } // 함수 종료
    }
}
