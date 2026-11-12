using System; // C# 기본 기능 연결
using ProjectEpsilon.Core; // 실제 런 시작 상태
using ProjectEpsilon.Recording; // 실제 피해 원인 기록
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakeHealth : MonoBehaviour // 체력·행동 자료 구조
    {
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private SnakeInvulnerability invulnerability; // 필드·무적 설정
        [SerializeField] private SnakeShieldController shieldController; // 공유 보호막 참조
        [SerializeField] private int maximumHealth = 100; // 필드·최대 설정
        [SerializeField] private int currentHealth = 100; // 필드·현재 설정

        private bool subscribed; // 대상 구성 선언
        private float healingRemainder; // 캐릭터 회복 배율 소수 누적

        public event Action<int, int> HealthChanged; // 이벤트·동작 구성 선언
        public event Action BodyDepleted; // 이벤트·동작 구성 선언

        public int CurrentHealth => currentHealth; // 현재·체력 값 반환
        public int MaximumHealth => maximumHealth; // 최대·체력 값 반환
        public SnakeShieldController ShieldController => shieldController; // 공유 보호막 반환

        public bool IsInvulnerable => // 무적 값 반환
            invulnerability != null && // 무적 처리
            invulnerability.IsInvulnerable; // 무적 처리

        private void OnEnable() // 활성 기능 선언
        {
            Subscribe(); // 구독 기능 실행
        }

        private void Start() // 시작 기능 선언
        {
            NormalizeHealth(); // 체력 기능 실행
            Subscribe(); // 구독 기능 실행
            NotifyHealthChanged(); // 알림·체력 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
        }

        public void Configure( // 구성 호출 시작
            SnakeBodyManager manager, // 몸통·관리자 항목 전달
            SnakeInvulnerability invulnerabilityController, // 무적·제어기 항목 전달
            int healthPerBody // 체력·몸통 처리
        ) // 앞선 항목 종료
        {
            Unsubscribe(); // 구독 해제 기능 실행

            bodyManager = manager; // 몸통·관리자 값 설정
            invulnerability = // 무적 값 설정
                invulnerabilityController; // 무적·제어기 처리

            maximumHealth = // 최대·체력 값 설정
                Mathf.Max( // 최대 호출 시작
                    1, // 대상 항목 전달
                    healthPerBody // 체력·몸통 처리
                ); // 앞선 항목 종료

            currentHealth = // 현재·체력 값 설정
                maximumHealth; // 최대·체력 처리

            Subscribe(); // 구독 기능 실행
            NotifyHealthChanged(); // 알림·체력 기능 실행
        }

        public bool SetStartingMaximumHealth(int maximum) // 런 시작 공용 체력 설정
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Starting) // 실제 시작 상태 확인
            {
                return false; // 진행 중 체력 초기화 방지
            }
            maximumHealth = Mathf.Max(1, maximum); // 안전한 최대 체력 저장
            currentHealth = maximumHealth; // 시작 체력 완전 회복
            healingRemainder = 0f; // 이전 회복 소수 초기화
            NotifyHealthChanged(); // 실제 HUD 체력 갱신
            return true; // 시작 체력 설정 완료
        }

        public bool TakeDamage(int damage, DamageOrigin origin = DamageOrigin.Unknown) // 받기·피해 기능 선언
        {
            GameManager game = GameManager.Instance; // 실제 게임 종료 상태 조회
            if (game != null && (game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated)) // 이미 확정된 승패 확인
            {
                return false; // 종료된 전투의 직접 피해 차단
            }
            int remainingDamage = // 남은 값·피해 값 설정
                Mathf.Max( // 최대 호출 시작
                    0, // 대상 항목 전달
                    damage // 피해 처리
                ); // 앞선 항목 종료

            remainingDamage = Mathf.CeilToInt(remainingDamage * CharacterRunInitializer.DamageReceivedMultiplier); // 중장형 실제 피해 경감

            if (remainingDamage <= 0 || // 남은 값·피해 조건 확인
                IsInvulnerable) // 무적 처리
            {
                return false; // 실패 결과 반환
            }

            int absorbedDamage = 0; // 실제 보호막 흡수 피해
            if (shieldController != null) // 보호막 연결 확인
            {
                int beforeShield = remainingDamage; // 실제 보호막 적용 전 피해
                remainingDamage = shieldController.Absorb(remainingDamage); // 보호막 우선 흡수
                absorbedDamage = beforeShield - remainingDamage; // 실제 보호막 흡수량

                if (remainingDamage <= 0) // 완전 흡수 확인
                {
                    RunTelemetry.RecordDamage(origin, 0, 0, absorbedDamage); // 실제 보호막 전량 흡수 기록
                    shieldController.CompleteDamageResolution(); // 보호막 종료 효과 완료
                    return true; // 피해 처리 성공
                }
            }

            if (bodyManager == null || // 몸통·관리자 조건 확인
                bodyManager.CurrentBodyCount <= 0) // 몸통·관리자 처리
            {
                SetHealth(0); // 설정·체력 기능 실행
                shieldController?.CompleteDamageResolution(); // 체력 처리 뒤 보호막 종료 완료
                return false; // 실패 결과 반환
            }

            long healthCapacity = currentHealth + (long)(bodyManager.CurrentBodyCount - 1) * maximumHealth; // 실제 남은 전체 몸통 체력
            int appliedDamage = (int)Math.Min(remainingDamage, healthCapacity); // 초과 피해를 제외한 실제 체력 손실
            RunTelemetry.RecordDamage(origin, appliedDamage, 0, absorbedDamage); // 보호막 이후 실제 체력 피해 기록

            while (remainingDamage > 0 && // 남은 값·피해 반복 조건
                bodyManager.CurrentBodyCount > 0) // 몸통·관리자 처리
            {
                if (remainingDamage < // 남은 값·피해 조건 확인
                    currentHealth) // 현재·체력 처리
                {
                    SetHealth( // 설정·체력 호출 시작
                        currentHealth - // 현재·체력 처리
                        remainingDamage // 남은 값·피해 처리
                    ); // 앞선 항목 종료

                    remainingDamage = 0; // 남은 값·피해 값 설정
                    break; // 반복 또는 분기 종료
                }

                remainingDamage -= // 남은 값·피해 값 설정
                    currentHealth; // 현재·체력 처리

                SetHealth(0); // 설정·체력 기능 실행

                int removed = // 대상 값 설정
                    bodyManager.RemoveBodies(1); // 몸통·관리자 기능 실행

                if (removed <= 0 || // 대상 조건 확인
                    bodyManager.CurrentBodyCount <= 0) // 몸통·관리자 처리
                {
                    SetHealth(0); // 설정·체력 기능 실행
                    BodyDepleted?.Invoke(); // 몸통 기능 실행
                    break; // 반복 또는 분기 종료
                }

                SetHealth( // 설정·체력 호출 시작
                    maximumHealth // 최대·체력 처리
                ); // 앞선 항목 종료
            }

            shieldController?.CompleteDamageResolution(); // 체력 처리 뒤 보호막 종료 완료
            return true; // 성공 결과 반환
        }

        public void BindShield(SnakeShieldController shield) // 보호막 참조 연결
        {
            shieldController = shield; // 보호막 저장
        }

        public bool Heal(int amount) // 회복·수량 기능 선언
        {
            int safeAmount = // 수량 값 설정
                Mathf.Max( // 최대 호출 시작
                    0, // 대상 항목 전달
                    amount // 수량 처리
                ); // 앞선 항목 종료

            if (safeAmount <= 0 || // 수량 조건 확인
                currentHealth >= maximumHealth || // 현재·체력 처리
                bodyManager == null || // 몸통·관리자 처리
                bodyManager.CurrentBodyCount <= 0) // 몸통·관리자 처리
            {
                return false; // 실패 결과 반환
            }

            float modifiedHealing = safeAmount * CharacterRunInitializer.HealingMultiplier * ProjectEpsilon.Progression.MetaModifiers.HealingMultiplier + healingRemainder; // 캐릭터와 영구 성장 회복 배율
            int actualHealing = Mathf.FloorToInt(modifiedHealing); // 실제 정수 회복량
            healingRemainder = modifiedHealing - actualHealing; // 다음 회복을 위한 소수 보존
            SetHealth( // 설정·체력 호출 시작
                currentHealth + // 현재·체력 처리
                actualHealing // 캐릭터 회복 배율 반영
            ); // 앞선 항목 종료

            return true; // 성공 결과 반환
        }

        public void ResetHealth() // 초기화·체력 기능 선언
        {
            healingRemainder = 0f; // 이전 회복 소수 초기화
            SetHealth( // 설정·체력 호출 시작
                bodyManager != null && // 몸통·관리자 처리
                bodyManager.CurrentBodyCount <= 0 // 몸통·관리자 처리
                    ? 0 // 대상 처리
                    : maximumHealth // 최대·체력 처리
            ); // 앞선 항목 종료
        }

        private void HandleBodyCountChanged( // 몸통·개수 호출 시작
            int current, // 현재 항목 전달
            int maximum // 최대 처리
        ) // 앞선 항목 종료
        {
            if (current > 0 || // 현재 조건 확인
                currentHealth == 0) // 현재·체력 처리
            {
                return; // 현재 기능 종료
            }

            SetHealth(0); // 설정·체력 기능 실행
            BodyDepleted?.Invoke(); // 몸통 기능 실행
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || // 대상 조건 확인
                bodyManager == null) // 몸통·관리자 처리
            {
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged += // 몸통·관리자 값 설정
                HandleBodyCountChanged; // 몸통·개수 처리

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || // 대상 조건 확인
                bodyManager == null) // 몸통·관리자 처리
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged -= // 몸통·관리자 값 설정
                HandleBodyCountChanged; // 몸통·개수 처리

            subscribed = false; // 대상 값 설정
        }

        private void NormalizeHealth() // 체력 기능 선언
        {
            maximumHealth = // 최대·체력 값 설정
                Mathf.Max( // 최대 호출 시작
                    1, // 대상 항목 전달
                    maximumHealth // 최대·체력 처리
                ); // 앞선 항목 종료

            currentHealth = // 현재·체력 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    currentHealth, // 현재·체력 항목 전달
                    0, // 대상 항목 전달
                    maximumHealth // 최대·체력 처리
                ); // 앞선 항목 종료
        }

        private void SetHealth(int value) // 설정·체력 기능 선언
        {
            int nextHealth = // 다음 값·체력 값 설정
                Mathf.Clamp( // 대상 호출 시작
                    value, // 값 항목 전달
                    0, // 대상 항목 전달
                    maximumHealth // 최대·체력 처리
                ); // 앞선 항목 종료

            if (currentHealth == // 현재·체력 조건 확인
                nextHealth) // 다음 값·체력 처리
            {
                return; // 현재 기능 종료
            }

            currentHealth = // 현재·체력 값 설정
                nextHealth; // 다음 값·체력 처리

            NotifyHealthChanged(); // 알림·체력 기능 실행
        }

        private void NotifyHealthChanged() // 알림·체력 기능 선언
        {
            HealthChanged?.Invoke( // 체력 호출 시작
                currentHealth, // 현재·체력 항목 전달
                maximumHealth // 최대·체력 처리
            ); // 앞선 항목 종료
        }
    }
}
