using ProjectEpsilon.Core; // 대상 기능 연결
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.InputSystem; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{
    public sealed class SnakeDamageDebugControls : MonoBehaviour // 피해·행동 자료 구조
    {
        [SerializeField] private SnakeHealth health; // 필드·체력 설정
        [SerializeField] private SnakeSelfCollision selfCollision; // 필드·충돌 설정
        [SerializeField] private bool debugControlsEnabled = true; // 필드·활성 설정

        public void Bind(SnakeHealth snakeHealth, SnakeSelfCollision collision) // 연결·체력 기능 선언
        {
            health = snakeHealth; // 체력 값 설정
            selfCollision = collision; // 충돌 값 설정
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

            if (keyboard.pKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                health?.TakeDamage(25); // 체력·받기 기능 실행
            }

            if (keyboard.oKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                health?.TakeDamage(120); // 체력·받기 기능 실행
            }

            if (keyboard.kKey.wasPressedThisFrame) // 키보드·키 조건 확인
            {
                selfCollision?.TryApplySelfCollisionPenalty(); // 충돌·시도 기능 실행
            }
        }
    }
}
