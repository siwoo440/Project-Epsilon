using System.Collections; // 비동기 프레임 대기
using UnityEngine; // Unity 기능
using UnityEngine.SceneManagement; // 실제 씬 재시작
using UnityEngine.UI; // 실제 전환 가림 화면

namespace ProjectEpsilon.Core // 게임 공통 영역
{
    public sealed class RunSceneReloader : MonoBehaviour // 기존 관리자 제거 후 씬 재시작
    {
        public void Begin(GameObject oldManager, string scenePath, bool coverScreen = false) // 실제 재시작 예약
        {
            DontDestroyOnLoad(gameObject); // 씬 변경 중 재시작 객체 유지
            if (coverScreen) // 실제 전투 씬 전환 가림 요청 확인
            {
                CreateCover(); // 실제 메뉴 전환 전 게임 화면 가림
            }
            StartCoroutine(Reload(oldManager, scenePath)); // 실제 비동기 재시작
        }
        private void CreateCover() // 실제 씬 전환 중 불투명 화면 구성
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
            cover.GetComponent<Image>().color = Color.black; // 실제 불투명 전환 배경
        }
        private IEnumerator Reload(GameObject oldManager, string scenePath) // 실제 관리자 교체
        {
            if (oldManager != null) // 이전 관리자 확인
            {
                oldManager.SetActive(false); // 이전 관리자 기능 정지
                Destroy(oldManager); // 이전 전역 관리자 제거 예약
            }
            yield return null; // 실제 전역 관리자 파괴 대기
            SceneManager.LoadScene(scenePath); // 실제 게임 씬 재시작
            Destroy(gameObject); // 재시작 보조 객체 제거
        }
    }
}
