using System; // 이벤트 형식 사용
using ProjectEpsilon.Core; // 게임 상태 사용
using UnityEngine; // Unity 기능 사용

namespace ProjectEpsilon.Player // 플레이어 영역
{ // 네임스페이스 시작
    public readonly struct SnakeShieldEndContext // 보호막 종료 정보
    { // 구조체 시작
        public int SynergyStage { get; } // 적용 시너지 단계
        public float SourceDamage { get; } // 적용 공격 피해
        public bool WasBroken { get; } // 피해 파괴 여부

        public SnakeShieldEndContext(int synergyStage, float sourceDamage, bool wasBroken) // 종료 정보 생성
        { // 생성자 시작
            SynergyStage = synergyStage; // 단계 저장
            SourceDamage = Mathf.Max(0f, sourceDamage); // 피해 저장
            WasBroken = wasBroken; // 파괴 여부 저장
        } // 생성자 끝
    } // 구조체 끝

    public sealed class SnakeShieldController : MonoBehaviour // 공유 보호막 관리자
    { // 클래스 시작
        [SerializeField] private int currentShield; // 현재 보호막
        [SerializeField] private float remainingDuration; // 남은 지속 시간
        [SerializeField] private int sourceStage; // 적용 시너지 단계
        [SerializeField] private float sourceDamage; // 적용 공격 피해

        private bool pendingBrokenEnd; // 지연 파괴 종료 여부
        private SnakeShieldEndContext pendingBrokenContext; // 지연 파괴 종료 정보

        public event Action<int> ShieldChanged; // 보호막 변경 알림
        public event Action<SnakeShieldEndContext> ShieldEnded; // 보호막 종료 알림

        public int CurrentShield => currentShield; // 현재 보호막 반환
        public float RemainingDuration => remainingDuration; // 남은 시간 반환
        public bool IsActive => currentShield > 0 && remainingDuration > 0f; // 활성 상태 반환

        private void Update() // 보호막 시간 갱신
        { // 메서드 시작
            if (!IsActive || GameManager.Instance == null || !GameManager.Instance.IsPlaying) // 갱신 가능 여부 확인
            { // 조건 시작
                return; // 시간 갱신 중단
            } // 조건 끝

            remainingDuration = Mathf.Max(0f, remainingDuration - Time.deltaTime); // 남은 시간 감소

            if (remainingDuration <= 0f) // 시간 종료 확인
            { // 조건 시작
                EndShield(false); // 자연 종료 처리
            } // 조건 끝
        } // 메서드 끝

        public bool Apply(int amount, float duration, int synergyStage, float attackDamage) // 보호막 적용
        { // 메서드 시작
            int nextShield = SnakeShieldRules.ResolveAppliedShield(currentShield, amount); // 적용 보호막 계산
            float safeDuration = Mathf.Max(0f, duration); // 지속 시간 보정

            if (nextShield <= 0 || safeDuration <= 0f) // 유효 보호막 확인
            { // 조건 시작
                return false; // 적용 실패
            } // 조건 끝

            currentShield = nextShield; // 보호막 저장
            remainingDuration = Mathf.Max(remainingDuration, safeDuration); // 긴 지속 시간 저장
            sourceStage = Mathf.Max(sourceStage, synergyStage); // 높은 단계 저장
            sourceDamage = Mathf.Max(sourceDamage, attackDamage); // 높은 공격 피해 저장
            ShieldChanged?.Invoke(currentShield); // 변경 알림 전달
            return true; // 적용 성공
        } // 메서드 끝

        public int Absorb(int incomingDamage) // 피해 흡수
        { // 메서드 시작
            int safeDamage = Mathf.Max(0, incomingDamage); // 피해량 보정

            if (!IsActive || safeDamage <= 0) // 흡수 가능 여부 확인
            { // 조건 시작
                return safeDamage; // 원래 피해 반환
            } // 조건 끝

            int healthDamage = SnakeShieldRules.ResolveHealthDamage(currentShield, safeDamage); // 체력 피해 계산
            int nextShield = SnakeShieldRules.ResolveShieldAfterDamage(currentShield, safeDamage); // 잔여 보호막 계산

            if (nextShield != currentShield) // 보호막 변경 확인
            { // 조건 시작
                currentShield = nextShield; // 잔여 보호막 저장
                ShieldChanged?.Invoke(currentShield); // 변경 알림 전달
            } // 조건 끝

            if (currentShield <= 0) // 보호막 파괴 확인
            { // 조건 시작
                QueueBrokenShieldEnd(); // 체력 피해 뒤 종료 예약
            } // 조건 끝

            return healthDamage; // 잔여 체력 피해 반환
        } // 메서드 끝

        public void CompleteDamageResolution() // 체력 피해 이후 종료 완료
        { // 메서드 시작
            if (!pendingBrokenEnd) // 예약 여부 확인
            { // 조건 시작
                return; // 완료 생략
            } // 조건 끝

            SnakeShieldEndContext context = pendingBrokenContext; // 예약 정보 복사
            pendingBrokenEnd = false; // 예약 상태 초기화
            pendingBrokenContext = default; // 예약 정보 초기화
            ShieldEnded?.Invoke(context); // 파괴 종료 알림 전달
        } // 메서드 끝

        public void Clear() // 보호막 초기화
        { // 메서드 시작
            if (currentShield <= 0 && remainingDuration <= 0f) // 초기 상태 확인
            { // 조건 시작
                return; // 초기화 생략
            } // 조건 끝

            currentShield = 0; // 보호막 초기화
            remainingDuration = 0f; // 지속 시간 초기화
            sourceStage = 0; // 단계 초기화
            sourceDamage = 0f; // 피해 초기화
            pendingBrokenEnd = false; // 지연 종료 초기화
            pendingBrokenContext = default; // 지연 정보 초기화
            ShieldChanged?.Invoke(currentShield); // 변경 알림 전달
        } // 메서드 끝

        private void QueueBrokenShieldEnd() // 보호막 파괴 종료 예약
        { // 메서드 시작
            pendingBrokenContext = new SnakeShieldEndContext(sourceStage, sourceDamage, true); // 파괴 정보 저장
            pendingBrokenEnd = true; // 지연 종료 예약
            remainingDuration = 0f; // 지속 시간 초기화
            sourceStage = 0; // 단계 초기화
            sourceDamage = 0f; // 피해 초기화
        } // 메서드 끝

        private void EndShield(bool wasBroken) // 보호막 종료 처리
        { // 메서드 시작
            SnakeShieldEndContext context = new SnakeShieldEndContext(sourceStage, sourceDamage, wasBroken); // 종료 정보 생성
            currentShield = 0; // 보호막 초기화
            remainingDuration = 0f; // 지속 시간 초기화
            sourceStage = 0; // 단계 초기화
            sourceDamage = 0f; // 피해 초기화
            ShieldChanged?.Invoke(currentShield); // 변경 알림 전달
            ShieldEnded?.Invoke(context); // 종료 알림 전달
        } // 메서드 끝
    } // 클래스 끝
} // 네임스페이스 끝
