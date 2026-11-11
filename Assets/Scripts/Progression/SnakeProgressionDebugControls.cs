using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Progression // 프로젝트 기능 영역
{
    public sealed class SnakeProgressionDebugControls : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private SnakeExperience experience; // 필드·경험치 설정
        [SerializeField] private int debugExperienceAmount = 10; // 필드·경험치 설정

        public void Configure( // 구성 호출 시작
            SnakeExperience experienceSource, // 경험치·원본 항목 전달
            int amount // 수량 처리
        ) // 앞선 항목 종료
        {
            experience = experienceSource; // 경험치·원본 값 설정
            debugExperienceAmount = // 경험치·수량 값 설정
                Mathf.Max(1, amount); // 최대·수량 기능 실행
        }

        private void Update() // 갱신 기능 선언
        {
            if (GameManager.Instance != null && // 게임·관리자 조건 확인
                !GameManager.Instance.IsPlaying) // 게임·관리자 처리
            {
                return; // 현재 기능 종료
            }

            Keyboard keyboard = Keyboard.current; // 키보드·현재 값 설정

            if (keyboard == null || // 키보드 조건 확인
                !keyboard.lKey.wasPressedThisFrame) // 키보드·키 처리
            {
                return; // 현재 기능 종료
            }

            if (experience == null) // 경험치 조건 확인
            {
                experience = // 경험치 값 설정
                    GetComponent<SnakeExperience>(); // 조회·구성 요소 기능 실행
            }

            experience?.AddExperience( // 경험치·추가 호출 시작
                Mathf.Max(1, debugExperienceAmount) // 최대·경험치 기능 실행
            ); // 앞선 항목 종료
        }
    }
}
