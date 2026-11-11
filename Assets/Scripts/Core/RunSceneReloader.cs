using System.Collections; // 비동기 프레임 대기
using UnityEngine; // Unity 기능
using UnityEngine.SceneManagement; // 실제 씬 재시작

namespace ProjectEpsilon.Core // 게임 공통 영역
{
    public sealed class RunSceneReloader : MonoBehaviour // 기존 관리자 제거 후 씬 재시작
    {
        public void Begin(GameObject oldManager, string scenePath) // 실제 재시작 예약
        {
            DontDestroyOnLoad(gameObject); // 씬 변경 중 재시작 객체 유지
            StartCoroutine(Reload(oldManager, scenePath)); // 실제 비동기 재시작
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
