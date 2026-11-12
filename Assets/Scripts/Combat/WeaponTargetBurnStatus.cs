using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{
    public sealed class WeaponTargetBurnStatus : MonoBehaviour // 적 화상 상태
    {
        [SerializeField] private WeaponTarget target; // 피해 대상
        [SerializeField] private int synergyStage; // 현재 화상 단계
        [SerializeField] private float damagePerSecond; // 초당 화상 피해
        [SerializeField] private float remainingDuration; // 남은 화상 시간

        private SpriteRenderer targetRenderer; // 대상 표시기
        private Color originalColor = Color.white; // 원래 색상

        public int SynergyStage => synergyStage; // 현재 단계 반환
        public float DamagePerSecond => damagePerSecond; // 초당 피해 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환

        private void Awake() // 초기 참조 구성
        {
            ResolveTarget(); // 대상 참조 확보
            ResolveRenderer(); // 표시기 참조 확보
        }

        private void Update() // 화상 지속 처리
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임 정지 여부 확인
            {
                return; // 갱신 중단
            }

            if (target == null || !target.IsAlive || remainingDuration <= 0f) // 종료 조건 확인
            {
                Destroy(this); // 화상 제거
                return; // 갱신 중단
            }

            float delta = Mathf.Min(Time.deltaTime, remainingDuration); // 실제 적용 시간 계산
            target.TakeDamage(damagePerSecond * delta); // 프레임 비례 피해 적용
            remainingDuration = Mathf.Max(0f, remainingDuration - delta); // 피해 적용 뒤 남은 시간 감소
            UpdateVisual(); // 화상 색상 갱신

            if (remainingDuration <= 0f || target == null || !target.IsAlive) // 적용 후 종료 확인
            {
                Destroy(this); // 화상 제거
            }
        }

        private void OnDisable() // 비활성화 처리
        {
            RestoreVisual(); // 원래 색상 복구
        }

        public bool Apply(WeaponTarget damageTarget, int incomingStage, float duration, float incomingDamagePerSecond) // 화상 적용 요청
        {
            if (damageTarget == null || duration <= 0f || incomingDamagePerSecond <= 0f) // 입력 유효성 확인
            {
                return false; // 적용 실패
            }

            if (!WeaponAttributeDamageRules.ShouldReplaceBurn(synergyStage, incomingStage)) // 교체 규칙 확인
            {
                return false; // 낮은 단계 거부
            }

            target = damageTarget; // 대상 저장
            synergyStage = WeaponAttributeSynergyRules.ResolveStage(incomingStage); // 단계 저장
            damagePerSecond = Mathf.Max(0f, incomingDamagePerSecond); // 초당 피해 저장
            remainingDuration = Mathf.Max(0f, duration); // 지속 시간 갱신
            ResolveRenderer(); // 표시기 확보
            return true; // 적용 성공
        }

        public void Clear() // 화상 재료 즉시 해제
        {
            synergyStage = 0; // 화상 단계 초기화
            damagePerSecond = 0f; // 화상 피해 초기화
            remainingDuration = 0f; // 화상 시간 초기화
            RestoreVisual(); // 기존 색상 복구
        }

        private void ResolveTarget() // 대상 참조 확보
        {
            if (target == null) // 대상 없음 확인
            {
                target = GetComponent<WeaponTarget>(); // 같은 오브젝트 대상 조회
            }
        }

        private void ResolveRenderer() // 표시기 참조 확보
        {
            if (targetRenderer != null) // 기존 표시기 확인
            {
                return; // 재조회 생략
            }

            targetRenderer = GetComponentInChildren<SpriteRenderer>(); // 하위 표시기 조회

            if (targetRenderer != null) // 표시기 존재 확인
            {
                originalColor = targetRenderer.color; // 원래 색상 저장
            }
        }

        private void UpdateVisual() // 화상 시각 갱신
        {
            if (targetRenderer == null) // 표시기 없음 확인
            {
                return; // 갱신 생략
            }

            float pulse = 0.35f + Mathf.PingPong(Time.time * 4f, 0.35f); // 점멸 비율 계산
            targetRenderer.color = Color.Lerp(originalColor, new Color(1f, 0.2f, 0.05f, originalColor.a), pulse); // 화상 색상 적용
        }

        private void RestoreVisual() // 원래 색상 복구
        {
            if (targetRenderer != null) // 표시기 존재 확인
            {
                targetRenderer.color = originalColor; // 원래 색상 적용
            }
        }
    }
}
