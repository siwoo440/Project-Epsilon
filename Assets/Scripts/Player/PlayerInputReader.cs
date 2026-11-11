using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class PlayerInputReader : MonoBehaviour // 플레이어·입력 자료 구조
    {
        public float TurnInput // 입력 구성 선언
        {
            get // 조회 처리
            {
                InputBindingManager bindingManager = InputBindingManager.Instance; // 입력·관리자 값 설정

                if (bindingManager != null) // 관리자 조건 확인
                {
                    float left = bindingManager.TurnLeftPressed ? -1f : 0f; // 관리자·입력 값 설정
                    float right = bindingManager.TurnRightPressed ? 1f : 0f; // 관리자·입력 값 설정

                    return Mathf.Clamp(left + right, -1f, 1f); // 대상 결과 반환
                }

                return GetFallbackTurnInput(); // 조회·입력 결과 반환
            }
        }

        public bool BoostPressed // 입력 구성 선언
        {
            get // 조회 처리
            {
                InputBindingManager bindingManager = InputBindingManager.Instance; // 입력·관리자 값 설정

                if (bindingManager != null) // 관리자 조건 확인
                {
                    return bindingManager.BoostPressed; // 관리자·입력 결과 반환
                }

                Keyboard keyboard = Keyboard.current; // 키보드·현재 값 설정

                return keyboard != null && // 키보드 결과 반환
                    keyboard.spaceKey.isPressed; // 기본 가속 키
            }
        }

        private static float GetFallbackTurnInput() // 조회·입력 기능 선언
        {
            Keyboard keyboard = Keyboard.current; // 키보드·현재 값 설정

            if (keyboard == null) // 키보드 조건 확인
            {
                return 0f; // 대상 결과 반환
            }

            float left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f; // 키보드·키 값 설정
            float right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f; // 키보드·키 값 설정

            return Mathf.Clamp(left + right, -1f, 1f); // 대상 결과 반환
        }
    }
}
