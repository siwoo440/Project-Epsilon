using ProjectEpsilon.Player; // 플레이어 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(300)] // 대상 설정
    public sealed class SnakeBodyHUDPresenter : MonoBehaviour // 몸통·행동 자료 구조
    {
        [SerializeField] private SnakeBodyManager bodyManager; // 필드·몸통 설정
        [SerializeField] private HUDController hudController; // 필드·제어기 설정

        private bool subscribed; // 대상 구성 선언

        private void OnEnable() // 활성 기능 선언
        {
            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
                Refresh(); // 재활성화 시 현재 몸통 반영
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

        public void Bind(SnakeBodyManager manager, HUDController hud) // 연결·몸통 기능 선언
        {
            Unsubscribe(); // 구독 해제 기능 실행
            bodyManager = manager; // 몸통·관리자 값 설정
            hudController = hud; // 제어기 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }

            Refresh(); // 갱신 기능 실행
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || bodyManager == null) // 몸통·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged += HandleBodyCountChanged; // 몸통·관리자 값 설정
            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || bodyManager == null) // 몸통·관리자 조건 확인
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            bodyManager.BodyCountChanged -= HandleBodyCountChanged; // 몸통·관리자 값 설정
            subscribed = false; // 대상 값 설정
        }

        private void Refresh() // 갱신 기능 선언
        {
            if (bodyManager == null || hudController == null) // 몸통·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            hudController.SetBodyCount( // 제어기·설정 호출 시작
                bodyManager.CurrentBodyCount, // 몸통·관리자 항목 전달
                bodyManager.EarnedBodyCount, // 성장 최대 몸통 수
                bodyManager.MaximumBodyCount // 몸통·관리자 처리
            ); // 앞선 항목 종료
        }

        private void HandleBodyCountChanged(int current, int maximum) // 몸통·개수 기능 선언
        {
            if (hudController != null && bodyManager != null) // 표시 및 몸통 연결 확인
            {
                hudController.SetBodyCount(current, bodyManager.EarnedBodyCount, maximum); // 현재 수와 두 상한 표시
            }
        }
    }
}
