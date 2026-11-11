using System; // 실제 예외와 형식
using System.Reflection; // 실제 글꼴 참조 조회
using ProjectEpsilon.UI; // 실제 포탑과 합성 화면
using UnityEditor; // 실제 배치 제어
using UnityEditor.SceneManagement; // 실제 씬 열기
using UnityEngine; // 실제 Unity 객체
using UnityEngine.UI; // 실제 글자 표시

namespace ProjectEpsilon.Editor // 실제 시험 영역
{
    [InitializeOnLoad] // 실제 도메인 갱신
    public static class AutomatedDay50FontVerification // 실제 보상 화면 한글 글꼴 검증
    {
        private const string Key = "EpsilonDay50Font"; // 실제 시험 상태 키
        private static double started; // 실제 시험 시작 시각
        static AutomatedDay50FontVerification() // 실제 도메인 재시작
        {
            if (SessionState.GetBool(Key, false)) // 실제 시험 상태 확인
            {
                EditorApplication.update += Tick; // 실제 시험 감시 등록
            }
        }
        public static void Run() // 실제 배치 진입
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Game.unity"); // 실제 게임 씬 열기
            SessionState.SetBool(Key, true); // 실제 시험 상태 등록
            EditorApplication.update -= Tick; // 기존 감시 제거
            EditorApplication.update += Tick; // 실제 시험 감시 등록
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        }
        private static void Tick() // 실제 씬 글꼴 감시
        {
            if (!EditorApplication.isPlaying) // 실제 플레이 상태 확인
            {
                return; // 실제 씬 시작 대기
            }
            if (started <= 0d) // 실제 시작 시각 확인
            {
                started = EditorApplication.timeSinceStartup; // 실제 시작 시각 저장
            }
            if (EditorApplication.timeSinceStartup - started > 60d) // 실제 제한 시간 확인
            {
                Finish(false, "Timed out."); // 실제 시간 초과
                return; // 실제 시험 중단
            }
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 메뉴 화면
            if (frontend == null || GameObject.Find("FrontendStartButton") == null) // 실제 화면 초기화 확인
            {
                return; // 실제 화면 구성 대기
            }
            Font font = (Font)typeof(RunFrontendController).GetField("font", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(frontend); // 실제 한글 우선 글꼴
            LevelUpPanelController level = UnityEngine.Object.FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 포탑 보상 화면
            WeaponMergePanelController merge = UnityEngine.Object.FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 실제 포탑 합성 화면
            bool valid = font != null && level != null && merge != null && AllTextUses(level.gameObject, font) && AllTextUses(merge.gameObject, font); // 실제 두 화면 글꼴 확인
            Finish(valid, valid ? "Weapon and reward text uses the Korean-capable menu font." : "Weapon or reward text still uses another font."); // 실제 글꼴 결과
        }
        private static bool AllTextUses(GameObject panel, Font expected) // 실제 화면 글꼴 확인
        {
            Text[] texts = panel.GetComponentsInChildren<Text>(true); // 실제 모든 화면 글자
            if (texts.Length == 0) // 실제 글자 존재 확인
            {
                return false; // 누락 화면 차단
            }
            foreach (Text item in texts) // 실제 글자 순회
            {
                if (item.font != expected) // 실제 글꼴 연결 확인
                {
                    return false; // 다른 글꼴 사용 차단
                }
            }
            return true; // 실제 모든 글꼴 연결 완료
        }
        private static void Finish(bool success, string message) // 실제 시험 결과 처리
        {
            SessionState.SetBool(Key, false); // 실제 시험 상태 해제
            EditorApplication.update -= Tick; // 실제 시험 감시 해제
            Debug.Log($"[Day50FontVerification] {(success ? "PASS" : "FAIL")}: {message}"); // 실제 시험 결과
            EditorApplication.Exit(success ? 0 : 1); // 실제 종료 코드
        }
    }
}
