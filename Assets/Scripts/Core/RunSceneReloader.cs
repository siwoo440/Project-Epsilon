using System.Collections; // 비동기 프레임 대기
using ProjectEpsilon.UI; // 실제 출격 예약
using UnityEngine; // Unity 기능
using UnityEngine.EventSystems; // 실제 선택 해제
using UnityEngine.SceneManagement; // 실제 씬 재시작
using UnityEngine.UI; // 실제 전환 가림 화면

namespace ProjectEpsilon.Core // 게임 공통 영역
{
    public sealed class RunSceneReloader : MonoBehaviour // 기존 관리자 제거 후 씬 재시작
    {
        private Text progressText; // 실제 로딩 진행 글자
        private Image progressFill; // 실제 로딩 진행 막대
        private Font loadingFont; // 실제 로딩 전용 글꼴
        private bool ownsLoadingFont; // 생성한 글꼴 소유 여부
        public void Begin(GameObject oldManager, string scenePath, bool coverScreen = false, bool showLoading = false) // 실제 재시작 예약
        {
            DontDestroyOnLoad(gameObject); // 씬 변경 중 재시작 객체 유지
            if (coverScreen) // 실제 전투 씬 전환 가림 요청 확인
            {
                CreateCover(showLoading); // 실제 메뉴 전환 전 게임 화면 가림
            }
            StartCoroutine(Reload(oldManager, scenePath, showLoading)); // 실제 비동기 재시작
        }
        private void CreateCover(bool showLoading) // 실제 씬 전환 중 불투명 화면 구성
        {
            GameObject cover = new GameObject("SceneTransitionCover", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(Image)); // 실제 전체 화면 가림 객체
            cover.transform.SetParent(transform, false); // 실제 전환 보조 객체에 연결
            Canvas canvas = cover.GetComponent<Canvas>(); // 실제 가림 캔버스
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 실제 전체 화면 위 표시
            canvas.sortingOrder = 32760; // 실제 기존 HUD보다 위에 표시
            RectTransform rect = cover.GetComponent<RectTransform>(); // 실제 가림 영역
            rect.anchorMin = Vector2.zero; // 실제 왼쪽 아래 기준
            rect.anchorMax = Vector2.one; // 실제 오른쪽 위 기준
            rect.offsetMin = Vector2.zero; // 실제 왼쪽 아래 여백 제거
            rect.offsetMax = Vector2.zero; // 실제 오른쪽 위 여백 제거
            cover.GetComponent<Image>().color = new Color(0.01f, 0.04f, 0.08f, 1f); // 실제 불투명 전환 배경
            if (showLoading) // 실제 출격 로딩 표시 요청
            { // 조건 시작
                cover.name = "SceneLoadingOverlay"; // 실제 로딩 화면 이름
                cover.AddComponent<GraphicRaycaster>(); // 뒤쪽 메뉴 입력 차단
                if (EventSystem.current != null) // 실제 선택된 UI 확인
                { // 조건 시작
                    EventSystem.current.SetSelectedGameObject(null); // 뒤쪽 버튼 선택 해제
                } // 조건 종료
                CreateLoadingContent(rect); // 실제 진행 표시 구성
            } // 조건 종료
        }
        private void CreateLoadingContent(RectTransform parent) // 실제 로딩 안내 구성
        { // 함수 시작
            loadingFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 28); // 실제 한국어 글꼴
            ownsLoadingFont = loadingFont != null; // 생성한 글꼴 소유 기록
            if (loadingFont == null) // 시스템 글꼴 확인
            { // 조건 시작
                loadingFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 기본 글꼴 대체
            } // 조건 종료
            GameObject labelObject = new GameObject("SceneLoadingProgress", typeof(RectTransform), typeof(Text)); // 실제 진행 글자
            RectTransform labelRect = labelObject.GetComponent<RectTransform>(); // 실제 글자 영역
            labelRect.SetParent(parent, false); // 실제 로딩 화면에 연결
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 화면 가운데 기준
            labelRect.sizeDelta = new Vector2(600f, 80f); // 실제 글자 영역 크기
            labelRect.anchoredPosition = new Vector2(0f, 50f); // 실제 막대 위 글자 위치
            progressText = labelObject.GetComponent<Text>(); // 실제 글자 구성
            progressText.font = loadingFont; // 실제 표시 글꼴
            progressText.fontSize = 30; // 실제 글자 크기
            progressText.alignment = TextAnchor.MiddleCenter; // 실제 가운데 정렬
            progressText.text = "불러오는 중  0%"; // 실제 초기 진행 안내
            progressText.raycastTarget = false; // 실제 입력 차단 제외
            GameObject track = new GameObject("SceneLoadingTrack", typeof(RectTransform), typeof(Image)); // 실제 진행 막대 배경
            RectTransform trackRect = track.GetComponent<RectTransform>(); // 실제 막대 영역
            trackRect.SetParent(parent, false); // 실제 로딩 화면에 연결
            trackRect.anchorMin = trackRect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 화면 가운데 기준
            trackRect.sizeDelta = new Vector2(500f, 24f); // 실제 막대 크기
            track.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.27f, 1f); // 실제 막대 배경색
            GameObject fill = new GameObject("SceneLoadingFill", typeof(RectTransform), typeof(Image)); // 실제 진행 막대
            RectTransform fillRect = fill.GetComponent<RectTransform>(); // 실제 진행 영역
            fillRect.SetParent(trackRect, false); // 실제 막대 배경에 연결
            fillRect.anchorMin = Vector2.zero; // 실제 왼쪽 아래 기준
            fillRect.anchorMax = Vector2.one; // 실제 오른쪽 위 기준
            fillRect.offsetMin = new Vector2(3f, 3f); // 실제 아래 여백
            fillRect.offsetMax = new Vector2(-3f, -3f); // 실제 위 여백
            progressFill = fill.GetComponent<Image>(); // 실제 진행 이미지
            progressFill.color = new Color(0.16f, 0.84f, 0.88f, 1f); // 실제 진행 색상
            progressFill.type = Image.Type.Filled; // 실제 비율 표시 방식
            progressFill.fillMethod = Image.FillMethod.Horizontal; // 실제 왼쪽부터 채움
            progressFill.fillAmount = 0f; // 실제 초기 진행 비율
        } // 함수 종료
        private void OnDestroy() // 실제 로딩 자원 정리
        { // 함수 시작
            if (ownsLoadingFont && loadingFont != null) // 생성한 글꼴 확인
            { // 조건 시작
                Destroy(loadingFont); // 실제 임시 글꼴 제거
            } // 조건 종료
        } // 함수 종료
        private void SetProgress(float value) // 실제 로딩 진행 갱신
        { // 함수 시작
            float progress = Mathf.Clamp01(value); // 실제 진행 비율 제한
            if (progressText != null) // 실제 진행 글자 확인
            { // 조건 시작
                progressText.text = $"불러오는 중  {Mathf.RoundToInt(progress * 100f)}%"; // 실제 진행률 안내
            } // 조건 종료
            if (progressFill != null) // 실제 진행 막대 확인
            { // 조건 시작
                progressFill.fillAmount = progress; // 실제 진행 막대 채움
            } // 조건 종료
        } // 함수 종료
        private IEnumerator Reload(GameObject oldManager, string scenePath, bool showLoading) // 실제 관리자 교체
        {
            if (oldManager != null) // 이전 관리자 확인
            {
                oldManager.SetActive(false); // 이전 관리자 기능 정지
                Destroy(oldManager); // 이전 전역 관리자 제거 예약
            }
            yield return null; // 실제 전역 관리자 파괴 대기
            if (!showLoading) // 기존 씬 재시작 확인
            { // 조건 시작
                SceneManager.LoadScene(scenePath); // 기존 동기 재시작 유지
                Destroy(gameObject); // 실제 보조 객체 제거
                yield break; // 기존 전환 종료
            } // 조건 종료
            AsyncOperation operation = null; // 실제 비동기 씬 작업
            try // 실제 씬 불러오기 요청
            { // 예외 검사 시작
                operation = SceneManager.LoadSceneAsync(scenePath); // 실제 비동기 씬 이동
            } // 예외 검사 종료
            catch (System.Exception error) // 실제 불러오기 실패 확인
            { // 예외 처리 시작
                Debug.LogError("[Project Epsilon] 씬 로딩 실패: " + error.Message); // 실제 실패 원인 기록
            } // 예외 처리 종료
            if (operation == null) // 실제 씬 작업 생성 확인
            { // 조건 시작
                SceneFrontendContext.CancelPendingStart(); // 실패한 출격 예약 취소
                RunFrontendController frontend = FindFirstObjectByType<RunFrontendController>(); // 실제 준비 화면 조회
                if (frontend != null) // 실제 준비 화면 확인
                { // 조건 시작
                    frontend.ShowSceneLoadError(); // 실제 실패 안내 표시
                } // 조건 종료
                Destroy(gameObject); // 실패한 로딩 화면 제거
                yield break; // 실제 로딩 중단
            } // 조건 종료
            while (!operation.isDone) // 실제 씬 불러오기 완료 대기
            { // 반복 시작
                SetProgress(operation.progress); // 실제 진행률 표시
                yield return null; // 실제 화면 갱신 대기
            } // 반복 종료
            SetProgress(1f); // 실제 완료 상태 표시
            yield return null; // 새 씬 초기화와 완료 화면 대기
            Destroy(gameObject); // 재시작 보조 객체 제거
        }
    }
}
