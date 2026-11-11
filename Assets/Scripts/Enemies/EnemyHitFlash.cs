using ProjectEpsilon.Combat; // 실제 적 피해 대상
using UnityEngine; // 유니티 기능

namespace ProjectEpsilon.Enemies // 적 영역
{
    [RequireComponent(typeof(WeaponTarget), typeof(SpriteRenderer))] // 실제 피해 대상과 표시기 필요
    public sealed class EnemyHitFlash : MonoBehaviour // 적 피격 색상 표시
    {
        [SerializeField] private float duration = 0.18f; // 피격 색상 유지 시간
        private WeaponTarget target; // 실제 적 피해 대상
        private SpriteRenderer visual; // 실제 적 표시기
        private Color originalColor; // 피격 전 적 색상
        private float endTime; // 피격 색상 종료 시각
        private bool flashing; // 피격 표시 상태
        private void OnEnable() // 실제 피해 이벤트 연결
        {
            target = GetComponent<WeaponTarget>(); // 실제 적 대상 조회
            visual = GetComponent<SpriteRenderer>(); // 실제 적 표시기 조회
            target.Damaged += OnDamaged; // 실제 피해 구독
        }
        private void OnDisable() // 실제 피해 이벤트 해제
        {
            if (target != null) // 실제 대상 확인
            {
                target.Damaged -= OnDamaged; // 피해 구독 해제
            }
            Restore(); // 비활성 전 기존 색상 복구
        }
        private void OnDamaged(float damage) // 실제 피해 표시
        {
            if (damage <= 0f || visual == null) // 실제 피해와 표시기 확인
            {
                return; // 무효 피해 무시
            }
            if (!flashing) // 기존 피격 표시 확인
            {
                originalColor = visual.color; // 현재 적 색상 보관
            }
            flashing = true; // 피격 표시 시작
            endTime = Time.time + Mathf.Max(0f, duration); // 실제 종료 시각 갱신
            visual.color = new Color(1f, 0.05f, 0.05f, originalColor.a); // 빨간 피격 색상 표시
        }
        private void Update() // 실제 피격 표시 갱신
        {
            if (flashing && Time.time >= endTime) // 피격 시간 종료 확인
            {
                Restore(); // 기존 적 색상 복구
            }
        }
        private void Restore() // 피격 전 적 색상 복구
        {
            if (flashing && visual != null) // 실제 표시 상태 확인
            {
                visual.color = originalColor; // 기존 적 색상 복구
            }
            flashing = false; // 피격 표시 종료
        }
    }
}
