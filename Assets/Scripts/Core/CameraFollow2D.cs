using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Core // 프로젝트 기능 영역
{
    public sealed class CameraFollow2D : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private Transform target; // 필드·위치 설정
        [SerializeField] private float smoothTime = 0.15f; // 필드·시간 설정

        private Vector3 velocity; // 벡터 구성 선언

        public Transform Target => target; // 위치·대상 값 반환

        public void SetTarget(Transform nextTarget) // 설정·대상 기능 선언
        {
            target = nextTarget; // 대상·다음 값 값 설정
        }

        private void LateUpdate() // 갱신 기능 선언
        {
            if (target == null) // 대상 조건 확인
            {
                return; // 현재 기능 종료
            }

            Vector3 targetPosition = new Vector3( // 벡터·대상 호출 시작
                target.position.x, // 대상·위치 항목 전달
                target.position.y, // 대상·위치 항목 전달
                transform.position.z // 위치 처리
            ); // 앞선 항목 종료

            transform.position = Vector3.SmoothDamp( // 위치·벡터 호출 시작
                transform.position, // 위치 항목 전달
                targetPosition, // 대상·위치 항목 전달
                ref velocity, // 대상 항목 전달
                smoothTime // 시간 처리
            ); // 앞선 항목 종료
        }
    }
}
