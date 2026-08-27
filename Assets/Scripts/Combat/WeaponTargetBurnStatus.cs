using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Combat // 전투 영역
{ // 네임스페이스 시작
    public sealed class WeaponTargetBurnStatus : MonoBehaviour // 적 화상 상태
    { // 클래스 시작
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
        { // 메서드 시작
            ResolveTarget(); // 대상 참조 확보
            ResolveRenderer(); // 표시기 참조 확보
        } // 메서드 끝

        private void Update() // 화상 지속 처리
        { // 메서드 시작
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임 정지 여부 확인
            { // 조건 시작
                return; // 갱신 중단
            } // 조건 끝

            if (target == null || !target.IsAlive || remainingDuration <= 0f) // 종료 조건 확인
            { // 조건 시작
                Destroy(this); // 화상 제거
                return; // 갱신 중단
            } // 조건 끝

            float delta = Mathf.Min(Time.deltaTime, remainingDuration); // 실제 적용 시간 계산
            remainingDuration = Mathf.Max(0f, remainingDuration - delta); // 남은 시간 감소
            target.TakeDamage(damagePerSecond * delta); // 프레임 비례 피해 적용
            UpdateVisual(); // 화상 색상 갱신

            if (remainingDuration <= 0f || target == null || !target.IsAlive) // 적용 후 종료 확인
            { // 조건 시작
                Destroy(this); // 화상 제거
            } // 조건 끝
        } // 메서드 끝

        private void OnDisable() // 비활성화 처리
        { // 메서드 시작
            RestoreVisual(); // 원래 색상 복구
        } // 메서드 끝

        public bool Apply(WeaponTarget damageTarget, int incomingStage, float duration, float incomingDamagePerSecond) // 화상 적용 요청
        { // 메서드 시작
            if (damageTarget == null || duration <= 0f || incomingDamagePerSecond <= 0f) // 입력 유효성 확인
            { // 조건 시작
                return false; // 적용 실패
            } // 조건 끝

            if (!WeaponAttributeDamageRules.ShouldReplaceBurn(synergyStage, incomingStage)) // 교체 규칙 확인
            { // 조건 시작
                return false; // 낮은 단계 거부
            } // 조건 끝

            target = damageTarget; // 대상 저장
            synergyStage = WeaponAttributeSynergyRules.ResolveStage(incomingStage); // 단계 저장
            damagePerSecond = Mathf.Max(0f, incomingDamagePerSecond); // 초당 피해 저장
            remainingDuration = Mathf.Max(0f, duration); // 지속 시간 갱신
            ResolveRenderer(); // 표시기 확보
            return true; // 적용 성공
        } // 메서드 끝

        private void ResolveTarget() // 대상 참조 확보
        { // 메서드 시작
            if (target == null) // 대상 없음 확인
            { // 조건 시작
                target = GetComponent<WeaponTarget>(); // 같은 오브젝트 대상 조회
            } // 조건 끝
        } // 메서드 끝

        private void ResolveRenderer() // 표시기 참조 확보
        { // 메서드 시작
            if (targetRenderer != null) // 기존 표시기 확인
            { // 조건 시작
                return; // 재조회 생략
            } // 조건 끝

            targetRenderer = GetComponentInChildren<SpriteRenderer>(); // 하위 표시기 조회

            if (targetRenderer != null) // 표시기 존재 확인
            { // 조건 시작
                originalColor = targetRenderer.color; // 원래 색상 저장
            } // 조건 끝
        } // 메서드 끝

        private void UpdateVisual() // 화상 시각 갱신
        { // 메서드 시작
            if (targetRenderer == null) // 표시기 없음 확인
            { // 조건 시작
                return; // 갱신 생략
            } // 조건 끝

            float pulse = 0.35f + Mathf.PingPong(Time.time * 4f, 0.35f); // 점멸 비율 계산
            targetRenderer.color = Color.Lerp(originalColor, new Color(1f, 0.2f, 0.05f, originalColor.a), pulse); // 화상 색상 적용
        } // 메서드 끝

        private void RestoreVisual() // 원래 색상 복구
        { // 메서드 시작
            if (targetRenderer != null) // 표시기 존재 확인
            { // 조건 시작
                targetRenderer.color = originalColor; // 원래 색상 적용
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
