using ProjectEpsilon.Core; // 대상 기능 연결
using ProjectEpsilon.Enemies; // 실제 전투 시간 참조
using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.UI; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    [DefaultExecutionOrder(200)] // 출현기 시간 갱신 이후 표시
    public sealed class GameTimerUI : MonoBehaviour // 게임·UI 자료 구조
    {
        [SerializeField] private Text timerText; // 필드·문구 설정

        private float elapsedSeconds; // 대상 구성 선언
        private float displayOffset; // 표시 전용 초기화 기준
        private EarlyEnemyWaveSpawner spawner; // 실제 전투 시계

        private void Start() // 시작 기능 선언
        {
            spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기 조회
            Refresh(); // 갱신 기능 실행
        }

        private void LateUpdate() // 실제 전투 시간 표시
        {
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            {
                elapsedSeconds = Mathf.Max(0f, spawner.ElapsedSeconds - displayOffset); // 표시 전용 초기화 반영
                Refresh(); // 실제 시간 표시 갱신
                return; // 독립 시계 증가 생략
            }
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            {
                return; // 현재 기능 종료
            }

            elapsedSeconds += Time.deltaTime; // 시간·변화량 값 설정
            Refresh(); // 갱신 기능 실행
        }

        public void Bind(Text nextTimerText) // 연결·문구 기능 선언
        {
            timerText = nextTimerText; // 문구·다음 값 값 설정
            Refresh(); // 갱신 기능 실행
        }

        public void ResetTimer() // 초기화 기능 선언
        {
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            {
                displayOffset = spawner.ElapsedSeconds; // 표시만 초기화할 기준 저장
            }
            elapsedSeconds = 0f; // 대상 값 설정
            Refresh(); // 갱신 기능 실행
        }

        private void Refresh() // 갱신 기능 선언
        {
            if (timerText != null) // 문구 조건 확인
            {
                timerText.text = HUDTextFormatter.FormatTime(elapsedSeconds); // 문구·시간 값 설정
            }
        }
    }
}
