using ProjectEpsilon.Progression; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(300)] // 대상 설정
    public sealed class SnakeExperienceHUDPresenter : MonoBehaviour // 경험치·행동 자료 구조
    {
        [SerializeField] private SnakeExperience experience; // 필드·경험치 설정
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

        public void Bind( // 연결 호출 시작
            SnakeExperience source, // 경험치·원본 항목 전달
            HUDController hud // 대상 처리
        ) // 앞선 항목 종료
        {
            Unsubscribe(); // 구독 해제 기능 실행

            experience = source; // 경험치·원본 값 설정
            hudController = hud; // 제어기 값 설정

            if (Application.isPlaying) // 대상 조건 확인
            {
                Subscribe(); // 구독 기능 실행
            }

            Refresh(); // 갱신 기능 실행
        }

        private void Subscribe() // 구독 기능 선언
        {
            if (subscribed || experience == null) // 경험치 조건 확인
            {
                return; // 현재 기능 종료
            }

            experience.ExperienceChanged += // 경험치 값 설정
                HandleExperienceChanged; // 경험치 처리

            experience.LevelChanged += // 경험치·레벨 값 설정
                HandleLevelChanged; // 레벨 처리

            subscribed = true; // 대상 값 설정
        }

        private void Unsubscribe() // 구독 해제 기능 선언
        {
            if (!subscribed || experience == null) // 경험치 조건 확인
            {
                subscribed = false; // 대상 값 설정
                return; // 현재 기능 종료
            }

            experience.ExperienceChanged -= // 경험치 값 설정
                HandleExperienceChanged; // 경험치 처리

            experience.LevelChanged -= // 경험치·레벨 값 설정
                HandleLevelChanged; // 레벨 처리

            subscribed = false; // 대상 값 설정
        }

        private void Refresh() // 갱신 기능 선언
        {
            if (experience == null || hudController == null) // 경험치·제어기 조건 확인
            {
                return; // 현재 기능 종료
            }

            hudController.SetExperience( // 제어기·설정 호출 시작
                experience.CurrentExperience, // 경험치·현재 항목 전달
                experience.RequiredExperience // 경험치 처리
            ); // 앞선 항목 종료

            hudController.SetLevel( // 제어기·설정 호출 시작
                experience.CurrentLevel // 경험치·현재 처리
            ); // 앞선 항목 종료
        }

        private void HandleExperienceChanged( // 경험치 호출 시작
            int current, // 현재 항목 전달
            int required // 대상 처리
        ) // 앞선 항목 종료
        {
            if (hudController != null) // 제어기 조건 확인
            {
                hudController.SetExperience( // 제어기·설정 호출 시작
                    current, // 현재 항목 전달
                    required // 대상 처리
                ); // 앞선 항목 종료
            }
        }

        private void HandleLevelChanged(int level) // 레벨 기능 선언
        {
            if (hudController != null) // 제어기 조건 확인
            {
                hudController.SetLevel(level); // 제어기·설정 기능 실행
            }
        }
    }
}
