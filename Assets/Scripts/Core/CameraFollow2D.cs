using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Core // 프로젝트 기능 영역
{ // 이름 공간 시작
    public sealed class CameraFollow2D : MonoBehaviour // 행동 자료 구조
    { // 형식 시작
        [SerializeField] private Transform target; // 필드·위치 설정
        [SerializeField] private float smoothTime = 0.15f; // 필드·시간 설정

        private Vector3 velocity; // 벡터 구성 선언
        private Vector3 shakeOffset; // 직전 프레임의 임시 흔들림
        private float shakeDuration; // 흔들림 전체 시간
        private float shakeElapsed; // 흔들림 경과 시간
        private float shakeMagnitude; // 흔들림 최대 이동량

        public Transform Target => target; // 위치·대상 값 반환

        public void SetTarget(Transform nextTarget) // 카메라 추적 대상 변경
        { // 함수 시작
            target = nextTarget; // 대상·다음 값 값 설정
        } // 함수 종료

        public void PlayShake(float duration, float magnitude) // 전투 피드백의 흔들림 요청
        { // 함수 시작
            shakeDuration = Mathf.Max(0f, duration); // 안전한 지속 시간
            shakeElapsed = 0f; // 새 흔들림 시작
            shakeMagnitude = Mathf.Max(0f, magnitude); // 안전한 최대 이동량
        } // 함수 종료

        private void OnDisable() // 카메라 추적 종료 시 위치 복구
        { // 함수 시작
            transform.position -= shakeOffset; // 적용 중이던 진동 제거
            shakeOffset = Vector3.zero; // 이전 위치 보정 초기화
            shakeDuration = 0f; // 이전 흔들림 종료
        } // 함수 종료

        private void LateUpdate() // 갱신 기능 선언
        { // 함수 시작
            if (target == null) // 대상 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            Vector3 targetPosition = new Vector3( // 벡터·대상 호출 시작
                target.position.x, // 대상·위치 항목 전달
                target.position.y, // 대상·위치 항목 전달
                transform.position.z // 위치 처리
            ); // 앞선 항목 종료

            Vector3 followed = Vector3.SmoothDamp( // 흔들림과 독립된 카메라 추적
                transform.position - shakeOffset, // 직전 효과를 제거한 추적 기준
                targetPosition, // 대상·위치 항목 전달
                ref velocity, // 대상 항목 전달
                smoothTime // 시간 처리
            ); // 앞선 항목 종료
            shakeElapsed += Time.deltaTime; // 게임 시간만큼 흔들림 진행
            shakeOffset = Vector3.zero; // 종료 또는 정지 시 기본 위치
            if (shakeElapsed < shakeDuration && Time.deltaTime > 0f) // 남아 있는 전투 효과 확인
            { // 조건 시작
                float strength = ProjectEpsilon.UI.RunFrontendController.Instance?.ScreenShakeStrength ?? 0f; // 저장된 흔들림 강도 조회
                float magnitude = shakeMagnitude * strength * (1f - shakeElapsed / shakeDuration); // 시간이 지날수록 줄어드는 이동량
                shakeOffset = new Vector3(Mathf.Sin(shakeElapsed * 95f), Mathf.Sin(shakeElapsed * 137f), 0f) * magnitude; // 난수에 영향 없는 화면 진동
            } // 조건 종료
            transform.position = followed + shakeOffset; // 카메라 위치에 시각 효과 적용
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
