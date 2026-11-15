using ProjectEpsilon.Core; // 게임 상태와 공통 기능
using UnityEngine; // Unity 엔진 기능 연결

namespace ProjectEpsilon.Player // 프로젝트 기능 영역
{ // 이름 공간 시작
    [DefaultExecutionOrder(40)] // 몸통 생성 전 경로 준비
    public sealed class SnakePathRecorder : MonoBehaviour // 경로·행동 자료 구조
    { // 형식 시작
        [SerializeField] private float minimumPointDistance = 0.04f; // 필드·최소 설정
        [SerializeField] private float maximumPathLength = 18f; // 필드·최대 설정
        [SerializeField] private float initialPathLength = 14f; // 필드·경로 설정

        private SnakePathHistory pathHistory; // 경로 구성 선언

        public bool IsReady => pathHistory != null && pathHistory.PointCount > 1; // 경로·지점 값 반환

        private void Awake() // 초기화 기능 선언
        { // 함수 시작
            ResetHistory(); // 초기화 기능 실행
        } // 함수 종료

        private void OnEnable() // 활성 기능 선언
        { // 함수 시작
            if (pathHistory == null) // 경로 조건 확인
            { // 조건 시작
                ResetHistory(); // 초기화 기능 실행
            } // 조건 종료
        } // 함수 종료

        private void LateUpdate() // 갱신 기능 선언
        { // 함수 시작
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            { // 조건 시작
                return; // 현재 기능 종료
            } // 조건 종료

            pathHistory?.Record(transform.position); // 경로·위치 기능 실행
        } // 함수 종료

        public void ResetHistory() // 초기화 기능 선언
        { // 함수 시작
            pathHistory = new SnakePathHistory(minimumPointDistance, maximumPathLength); // 경로·최소 값 설정
            pathHistory.Reset(transform.position, transform.up, initialPathLength); // 경로·초기화 기능 실행
        } // 함수 종료

        public Vector3 GetPositionAtDistance(float distanceBehindHead) // 벡터·조회 기능 선언
        { // 함수 시작
            if (pathHistory == null) // 경로 조건 확인
            { // 조건 시작
                return transform.position; // 위치 결과 반환
            } // 조건 종료

            return pathHistory.SampleDistanceBehind(Mathf.Max(0f, distanceBehindHead)); // 경로·거리 결과 반환
        } // 함수 종료

        public Vector3 GetForwardAtDistance(float distanceBehindHead) // 벡터·조회 기능 선언
        { // 함수 시작
            Vector3 ahead = GetPositionAtDistance(Mathf.Max(0f, distanceBehindHead - 0.08f)); // 벡터·조회 값 설정
            Vector3 behind = GetPositionAtDistance(distanceBehindHead + 0.08f); // 벡터·조회 값 설정
            Vector3 direction = ahead - behind; // 벡터·방향 값 설정

            if (direction.sqrMagnitude < 0.0001f) // 방향 조건 확인
            { // 조건 시작
                return transform.up; // 위치 결과 반환
            } // 조건 종료

            return direction.normalized; // 방향 결과 반환
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
