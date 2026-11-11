using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(310)] // 대상 설정
    public sealed class SnakeHealthHUDPresenter : MonoBehaviour // 체력·행동 자료 구조
    {
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private HUDController hudController; // 필드·제어기 설정
        [SerializeField] private SnakeShieldController shieldController; // 공유 보호막 참조

        private bool subscribed; // 대상 구성 선언

        public SnakeHealth Health => health; // 공유 체력 반환
        public SnakeShieldController ShieldController => shieldController; // 공유 보호막 반환
        public HUDController HUDController => hudController; // HUD 반환

        private void OnEnable() // 활성 기능 선언
        {
            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }
        }

        private void Start() // 시작 기능 선언
        {
            Subscribe(); // 구독 기능 실행
            Refresh(); // 갱신 기능 실행
        }

        private void OnDisable() // 비활성 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
        }

        public void Bind(SnakeHealth snakeHealth, HUDController hud) // 연결·체력 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
            health = snakeHealth; // 체력 값 설정
            hudController = hud; // 제어기 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }

            Refresh(); // 갱신 기능 실행
        }

        public void Bind(SnakeHealth snakeHealth, SnakeShieldController shield, HUDController hud) // 보호막 포함 HUD 연결
        {
            Unsubscribe(); // 기존 이벤트 해제
            health = snakeHealth; // 체력 저장
            shieldController = shield; // 보호막 저장
            hudController = hud; // HUD 저장

            if (Application.isPlaying) // 실행 중 확인
            {
                Subscribe(); // 새 이벤트 구독
            }

            Refresh(); // 즉시 표시 갱신
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || health == null) // 체력 조건 확인
            {
                return; // 현재 기능 종료
            }

            health.HealthChanged += HandleHealthChanged; // 체력 값 설정
            if (shieldController != null) // 보호막 연결 확인
            {
                shieldController.ShieldChanged += HandleShieldChanged; // 보호막 변경 구독
            }
            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || health == null) // 체력 조건 확인
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            health.HealthChanged -= HandleHealthChanged; // 체력 값 설정
            if (shieldController != null) // 보호막 연결 확인
            {
                shieldController.ShieldChanged -= HandleShieldChanged; // 보호막 변경 해제
            }
            subscribed = false; // 대상 값 설정
        }

        private void Refresh() // 갱신 기능 선언
        {
            if (health == null || hudController == null) // 체력·제어기 조건 확인
            {
                return; // 현재 기능 종료
            }

            int shield = shieldController == null ? 0 : shieldController.CurrentShield; // 현재 보호막 조회
            hudController.SetHealth(health.CurrentHealth, health.MaximumHealth, shield); // 체력과 보호막 표시
        }

        private void HandleHealthChanged(int current, int maximum) // 체력·현재 기능 선언
        {
            if (hudController != null) // 제어기 조건 확인
            {
                int shield = shieldController == null ? 0 : shieldController.CurrentShield; // 현재 보호막 조회
                hudController.SetHealth(current, maximum, shield); // 체력과 보호막 표시
            }
        }

        private void HandleShieldChanged(int shield) // 보호막 변경 처리
        {
            Refresh(); // HUD 전체 갱신
        }
    }
}
