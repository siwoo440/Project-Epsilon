using ProjectEpsilon.Core; // 게임 진행 상태
using ProjectEpsilon.Enemies; // 실제 전투 시간 참조
using ProjectEpsilon.Progression; // 실제 전투 점수
using UnityEngine; // Unity 시간과 객체
using UnityEngine.UI; // 타이머와 점수 글자

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{ // 이름 공간 시작
    [DefaultExecutionOrder(200)] // 출현기 시간 갱신 이후 표시
    public sealed class GameTimerUI : MonoBehaviour // 게임·UI 자료 구조
    { // 형식 시작
        [SerializeField] private Text timerText; // 시간 표시 글자

        private float elapsedSeconds; // 표시할 경과 시간
        private int renderedSeconds = -1; // 마지막으로 표시한 초
        private float displayOffset; // 표시 전용 초기화 기준
        private EarlyEnemyWaveSpawner spawner; // 실제 전투 시계
        private MetaProgressionController scoreSource; // 실제 점수 집계 관리자
        private Text scoreText; // 현재 점수 문구
        private Text scoreGainText; // 실제 점수 증가량 문구
        private float scoreGainUntil; // 증가량 표시 종료 시각

        private void Start() // 타이머와 점수 연결
        { // 함수 시작
            spawner = FindFirstObjectByType<EarlyEnemyWaveSpawner>(); // 실제 출현기 조회
            CreateScoreLabels(); // 타이머 오른쪽 점수 표시 생성
            BindScore(); // 실제 점수 관리자 연결
            Refresh(); // 현재 시간 표시
        } // 함수 종료

        private void LateUpdate() // 실제 전투 시간 표시
        { // 함수 시작
            if (scoreSource == null) // 점수 관리자 초기화 대기
            { // 조건 시작
                BindScore(); // 실제 점수 연결 재시도
            } // 조건 종료
            if (scoreGainText != null && Time.unscaledTime >= scoreGainUntil) // 증가량 표시 시간 확인
            { // 조건 시작
                scoreGainText.text = string.Empty; // 지난 증가량 숨김
            } // 조건 종료
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            { // 조건 시작
                elapsedSeconds = Mathf.Max(0f, spawner.ElapsedSeconds - displayOffset); // 표시 전용 초기화 반영
                Refresh(); // 실제 시간 표시 갱신
                return; // 독립 시계 증가 생략
            } // 조건 종료
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) // 게임·관리자 조건 확인
            { // 조건 시작
                return; // 정지 중 시간 증가 생략
            } // 조건 종료

            elapsedSeconds += Time.deltaTime; // 시간·변화량 값 설정
            Refresh(); // 현재 시간 표시
        } // 함수 종료

        private void CreateScoreLabels() // 현재 점수와 증가량 배치
        { // 함수 시작
            if (timerText == null || timerText.transform.parent == null) // 타이머 연결 확인
            { // 조건 시작
                return; // 배치 기준 부재 시 생략
            } // 조건 종료
            scoreText = CreateScoreLabel("CurrentScoreText", new Vector2(105f, -30f), new Vector2(270f, 44f)); // 타이머 오른쪽 현재 점수
            scoreGainText = CreateScoreLabel("ScoreGainText", new Vector2(375f, -30f), new Vector2(160f, 44f)); // 점수 오른쪽 증가량
            scoreGainText.color = new Color(1f, 0.89f, 0.34f, 1f); // 증가량 강조 색상
            scoreText.text = "현재 점수 : 0"; // 초기 점수 문구
        } // 함수 종료
        private Text CreateScoreLabel(string name, Vector2 position, Vector2 size) // 타이머 기준 점수 글자 생성
        { // 함수 시작
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); // 실제 HUD 글자 객체
            RectTransform area = item.GetComponent<RectTransform>(); // 실제 글자 영역
            area.SetParent(timerText.transform.parent, false); // 타이머와 같은 화면 영역 연결
            area.anchorMin = area.anchorMax = new Vector2(0.5f, 1f); // 상단 중앙 기준
            area.pivot = new Vector2(0f, 0.5f); // 왼쪽 가운데 정렬 기준
            area.anchoredPosition = position; // 타이머 오른쪽 위치
            area.sizeDelta = size; // 글자 영역 크기
            Text label = item.GetComponent<Text>(); // 실제 글자 구성
            label.font = timerText.font; // 기존 HUD 글꼴 사용
            label.fontSize = 30; // 읽기 쉬운 기본 크기
            label.resizeTextForBestFit = true; // 긴 점수의 자동 크기 맞춤
            label.resizeTextMinSize = 15; // 자동 맞춤 최소 크기
            label.resizeTextMaxSize = 30; // 자동 맞춤 최대 크기
            label.alignment = TextAnchor.MiddleLeft; // 왼쪽 기준 정렬
            label.color = Color.white; // 기본 글자 색상
            label.raycastTarget = false; // 게임 입력 차단 방지
            return label; // 실제 글자 반환
        } // 함수 종료
        private void BindScore() // 실제 점수 변경 연결
        { // 함수 시작
            scoreSource = MetaProgressionController.Instance; // 현재 전투 관리자 조회
            if (scoreSource != null) // 실제 관리자 확인
            { // 조건 시작
                scoreSource.ScoreChanged += HandleScoreChanged; // 실제 점수 이벤트 구독
                HandleScoreChanged(scoreSource.CurrentScore, 0); // 기존 점수 표시 복구
            } // 조건 종료
        } // 함수 종료
        private void HandleScoreChanged(int score, int gained) // 실제 점수와 증가량 표시
        { // 함수 시작
            if (scoreText != null) // 현재 점수 문구 확인
            { // 조건 시작
                scoreText.text = $"현재 점수 : {score:N0}"; // 실제 누적 점수 표시
            } // 조건 종료
            if (scoreGainText != null) // 증가량 문구 확인
            { // 조건 시작
                scoreGainText.text = gained > 0 ? $"+{gained:N0}" : string.Empty; // 실제 증가량만 표시
                scoreGainUntil = Time.unscaledTime + 1.5f; // 일시정지와 무관한 표시 시간
            } // 조건 종료
        } // 함수 종료
        private void OnDestroy() // 실제 점수 구독 정리
        { // 함수 시작
            if (scoreSource != null) // 연결된 점수 관리자 확인
            { // 조건 시작
                scoreSource.ScoreChanged -= HandleScoreChanged; // 이전 HUD 이벤트 해제
            } // 조건 종료
        } // 함수 종료
        public void Bind(Text nextTimerText) // 타이머 표시 대상 변경
        { // 함수 시작
            timerText = nextTimerText; // 새 타이머 글자 연결
            renderedSeconds = -1; // 새 글자의 표시 갱신 예약
            Refresh(); // 현재 시간 표시
        } // 함수 종료

        public void ResetTimer() // 화면의 표시 시간 초기화
        { // 함수 시작
            if (spawner != null && spawner.isActiveAndEnabled && GameManager.Instance != null) // 실제 전투 시계 확인
            { // 조건 시작
                displayOffset = spawner.ElapsedSeconds; // 표시만 초기화할 기준 저장
            } // 조건 종료
            elapsedSeconds = 0f; // 표시 경과 시간 초기화
            Refresh(); // 현재 시간 표시
        } // 함수 종료

        private void Refresh() // 변경된 시간 글자 갱신
        { // 함수 시작
            if (timerText == null) // 타이머 글자 연결 확인
            { // 조건 시작
                return; // 표시 대상이 없으면 갱신 생략
            } // 조건 종료
            int currentSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds)); // 표시할 정수 초
            if (currentSeconds == renderedSeconds) // 표시 중인 초와 비교
            { // 조건 시작
                return; // 같은 초의 문자열 생성 생략
            } // 조건 종료
            renderedSeconds = currentSeconds; // 표시한 초 저장
            timerText.text = HUDTextFormatter.FormatTime(elapsedSeconds); // 바뀐 시간 표시
        } // 함수 종료
    } // 형식 종료
} // 이름 공간 종료
