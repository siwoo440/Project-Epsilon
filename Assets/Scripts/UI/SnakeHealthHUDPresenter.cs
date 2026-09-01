using ProjectEpsilon.Player;
using UnityEngine;

namespace ProjectEpsilon.UI
{
    [DefaultExecutionOrder(310)]
    public sealed class SnakeHealthHUDPresenter : MonoBehaviour
    {
        [SerializeField] private SnakeHealth health;
        [SerializeField] private HUDController hudController;
        [SerializeField] private SnakeShieldController shieldController; // 공유 보호막 참조

        private bool subscribed;

        public SnakeHealth Health => health; // 공유 체력 반환
        public SnakeShieldController ShieldController => shieldController; // 공유 보호막 반환
        public HUDController HUDController => hudController; // HUD 반환

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Subscribe();
            }
        }

        private void Start()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Bind(SnakeHealth snakeHealth, HUDController hud)
        {
            Unsubscribe();
            health = snakeHealth;
            hudController = hud;

            if (Application.isPlaying)
            {
                Subscribe();
            }

            Refresh();
        }

        public void Bind(SnakeHealth snakeHealth, SnakeShieldController shield, HUDController hud) // 보호막 포함 HUD 연결
        { // 메서드 시작
            Unsubscribe(); // 기존 이벤트 해제
            health = snakeHealth; // 체력 저장
            shieldController = shield; // 보호막 저장
            hudController = hud; // HUD 저장

            if (Application.isPlaying) // 실행 중 확인
            { // 조건 시작
                Subscribe(); // 새 이벤트 구독
            } // 조건 끝

            Refresh(); // 즉시 표시 갱신
        } // 메서드 끝

        private void Subscribe()
        {
            if (subscribed || health == null)
            {
                return;
            }

            health.HealthChanged += HandleHealthChanged;
            if (shieldController != null) // 보호막 연결 확인
            { // 조건 시작
                shieldController.ShieldChanged += HandleShieldChanged; // 보호막 변경 구독
            } // 조건 끝
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || health == null)
            {
                subscribed = false;
                return;
            }

            health.HealthChanged -= HandleHealthChanged;
            if (shieldController != null) // 보호막 연결 확인
            { // 조건 시작
                shieldController.ShieldChanged -= HandleShieldChanged; // 보호막 변경 해제
            } // 조건 끝
            subscribed = false;
        }

        private void Refresh()
        {
            if (health == null || hudController == null)
            {
                return;
            }

            int shield = shieldController == null ? 0 : shieldController.CurrentShield; // 현재 보호막 조회
            hudController.SetHealth(health.CurrentHealth, health.MaximumHealth, shield); // 체력과 보호막 표시
        }

        private void HandleHealthChanged(int current, int maximum)
        {
            if (hudController != null)
            {
                int shield = shieldController == null ? 0 : shieldController.CurrentShield; // 현재 보호막 조회
                hudController.SetHealth(current, maximum, shield); // 체력과 보호막 표시
            }
        }

        private void HandleShieldChanged(int shield) // 보호막 변경 처리
        { // 메서드 시작
            Refresh(); // HUD 전체 갱신
        } // 메서드 끝
    }
}
