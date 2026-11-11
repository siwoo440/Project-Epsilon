using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    [RequireComponent(typeof(SnakeBodyManager))] // 구성 요소·몸통 설정
    public sealed class SnakeBodyDebugControls : MonoBehaviour // 몸통·행동 자료 구조
    {
        [SerializeField] private bool debugControlsEnabled = true; // 필드·활성 설정

        private SnakeBodyManager bodyManager; // 몸통·관리자 구성 선언

        private void Awake() // 초기화 기능 선언
        {
            bodyManager = GetComponent<SnakeBodyManager>(); // 몸통·관리자 값 설정
        }

        private void Update() // 갱신 기능 선언
        {
            if (!debugControlsEnabled) // 활성 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            Keyboard keyboard = Keyboard.current; // 키보드·현재 값 설정

            if (keyboard == null) // 키보드 조건 확인
            {
                return; // 현재 기능 종료
            }

            if (keyboard.rightBracketKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                bodyManager.TryAddBody(); // 몸통·관리자 기능 실행
            }

            if (keyboard.leftBracketKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                bodyManager.TryRemoveBody(); // 몸통·관리자 기능 실행
            }
        }
    }
}
