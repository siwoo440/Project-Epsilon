using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(320)] // 대상 설정
    public sealed class SnakeStaminaHUDPresenter : MonoBehaviour // 스태미나·행동 자료 구조
    {
        [SerializeField] private SnakeStamina stamina; // 필드·스태미나 설정
        [SerializeField] private HUDController hudController; // 필드·제어기 설정

        private bool subscribed; // 대상 구성 선언

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

        public void Bind(SnakeStamina snakeStamina, HUDController hud) // 연결·스태미나 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
            stamina = snakeStamina; // 스태미나 값 설정
            hudController = hud; // 제어기 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }

            Refresh(); // 갱신 기능 실행
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || stamina == null) // 스태미나 조건 확인
            {
                return; // 현재 기능 종료
            }

            stamina.StaminaChanged += HandleStaminaChanged; // 스태미나 값 설정
            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || stamina == null) // 스태미나 조건 확인
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            stamina.StaminaChanged -= HandleStaminaChanged; // 스태미나 값 설정
            subscribed = false; // 대상 값 설정
        }

        private void Refresh() // 갱신 기능 선언
        {
            if (stamina == null || hudController == null) // 스태미나·제어기 조건 확인
            {
                return; // 현재 기능 종료
            }

            hudController.SetStamina( // 제어기·설정 호출 시작
                Mathf.RoundToInt(stamina.CurrentStamina), // 스태미나·현재 항목 전달
                Mathf.RoundToInt(stamina.MaximumStamina) // 스태미나·최대 기능 실행
            ); // 앞선 항목 종료
        }

        private void HandleStaminaChanged(int current, int maximum) // 스태미나·현재 기능 선언
        {
            if (hudController != null) // 제어기 조건 확인
            {
                hudController.SetStamina(current, maximum); // 제어기·설정 기능 실행
            }
        }
    }
}
