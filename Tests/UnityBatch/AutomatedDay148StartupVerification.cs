using System; // 격리 설정과 예외 처리
using System.Collections; // 프레임 대기
using ProjectEpsilon.Progression; // 실제 영구 성장
using ProjectEpsilon.UI; // 실제 메인 메뉴
using UnityEditor; // Unity 배치 실행
using UnityEditor.SceneManagement; // 실제 시작 씬 열기
using UnityEngine; // Unity 객체
using UnityEngine.SceneManagement; // 실제 활성 씬
using UnityEngine.UI; // 실제 메뉴 버튼

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 모드 재연결
    public static class AutomatedDay148StartupVerification // 실제 영구 성장 시작 검증
    { // 시험 시작
        private const string Key = "EpsilonDay148Startup"; // 배치 실행 상태
        private const string SaveKey = "ProjectEpsilon.MetaProgress.v1"; // 실제 영구 성장 저장 키
        private static bool started; // 시험 중복 방지
        static AutomatedDay148StartupVerification() // 도메인 재시작 복구
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 실제 시험 진행 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 실제 메뉴 시작 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 실제 배치 실행 진입점
        { // 함수 시작
            if (!Application.productName.EndsWith("-Isolated", StringComparison.Ordinal)) // 사용자 저장 격리 확인
            { // 조건 시작
                Finish(false, "격리된 시험 저장 공간이 필요합니다"); // 실제 저장 보호
                return; // 실제 사용자 저장 변경 차단
            } // 조건 종료
            bool returning = Array.IndexOf(Environment.GetCommandLineArgs(), "-epsilonReturningGrowth") >= 0; // 기존 저장 시험 여부
            bool directGame = Array.IndexOf(Environment.GetCommandLineArgs(), "-epsilonStartGameScene") >= 0; // 게임 씬 직접 시작 시험
            PlayerPrefs.DeleteKey(SaveKey); // 격리된 이전 시험 성장 저장 제거
            PlayerPrefs.Save(); // 격리된 저장 초기화
            if (returning) // 기존 전투 완료 상태 확인
            { // 조건 시작
                MetaProgressionService service = new MetaProgressionService(new PlayerPrefsMetaStorage(SaveKey)); // 격리된 실제 성장 저장
                if (!service.TryCompleteRun("day148-returning", 12000d, 0, 0, false, 0)) // 실제 첫 전투 완료 저장
                { // 조건 시작
                    Finish(false, "기존 전투 완료 상태 생성 실패"); // 실제 준비 실패 기록
                    return; // 잘못된 시험 중단
                } // 조건 종료
            } // 조건 종료
            SessionState.SetBool(Key + "Returning", returning); // 시험 저장 상태 보존
            EditorSceneManager.OpenScene(directGame ? SceneFrontendContext.GamePath : SceneFrontendContext.MainMenuPath); // 실제 시작 씬 선택
            SessionState.SetBool(Key, true); // 배치 시험 상태 저장
            EditorApplication.update += Tick; // 실제 메뉴 시작 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 함수 종료
        private static void Tick() // 실제 메뉴 시작 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || SceneManager.GetActiveScene().path != SceneFrontendContext.MainMenuPath || RunFrontendController.Instance == null || !RunFrontendController.Instance.IsMainMenu || MetaProgressionController.Instance == null || GameObject.Find("FrontendGrowthButton") == null) // 실제 메인 메뉴 준비 확인
            { // 조건 시작
                return; // 실제 시작 화면 대기
            } // 조건 종료
            started = true; // 시험 중복 방지
            new GameObject("Day148StartupRunner").AddComponent<Day148StartupRunner>(); // 실제 화면 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 시험 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 상태 제거
            EditorApplication.update -= Tick; // 메뉴 감시 해제
            Debug.Log("[Day148Startup] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과
            EditorApplication.Exit(passed ? 0 : 1); // 실제 배치 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class Day148StartupRunner : MonoBehaviour // 실제 시작 화면 시험 실행기
    { // 실행기 시작
        private IEnumerator Start() // 실제 시작과 사용자 선택 확인
        { // 함수 시작
            bool returning = SessionState.GetBool("EpsilonDay148StartupReturning", false); // 실제 시험 저장 상태
            for (int frame = 0; frame < 30; frame++) // 실제 시작 후 여러 프레임 순회
            { // 반복 시작
                MetaProgressionMenu menu = MetaProgressionController.Instance == null ? null : MetaProgressionController.Instance.Menu; // 실제 영구 성장 화면
                Require(menu != null && !menu.IsOpen && GameObject.Find("GrowthCanvas") == null && GameObject.Find("GrowthPanel") == null, "시작 직후 영구 성장 창이 자동으로 열림"); // 시작 화면의 성장 창 미표시 확인
                yield return null; // 실제 다음 프레임 확인
            } // 반복 종료
            Button growth = GameObject.Find("FrontendGrowthButton").GetComponent<Button>(); // 실제 영구 성장 메뉴 버튼
            Require(growth.interactable == returning, "첫 전투 완료에 따른 성장 해금 오류"); // 기존 해금 조건 확인
            if (returning) // 기존 전투 완료 상태 확인
            { // 조건 시작
                growth.onClick.Invoke(); // 실제 메뉴 버튼 클릭
                Require(MetaProgressionController.Instance.Menu.IsOpen && GameObject.Find("GrowthPanel") != null, "직접 선택한 영구 성장 화면이 열리지 않음"); // 기존 직접 진입 확인
                MetaProgressionController.Instance.Menu.enabled = false; // 성장 화면 제어기의 비활성 상황 재현
                Require(!MetaProgressionController.Instance.Menu.IsOpen && GameObject.Find("GrowthPanel") == null, "비활성 성장 화면이 남아 있음"); // 씬 전환 중 남은 창 차단 확인
                MetaProgressionController.Instance.Menu.enabled = true; // 실제 성장 화면 제어기 복구
                growth.onClick.Invoke(); // 실제 메뉴 버튼으로 다시 열기
                Require(MetaProgressionController.Instance.Menu.IsOpen && GameObject.Find("GrowthPanel") != null, "성장 화면 재활성화 후 직접 진입 실패"); // 기존 재진입 유지 확인
                GameObject.Find("GrowthCloseButton").GetComponent<Button>().onClick.Invoke(); // 실제 성장 화면 닫기
                Require(!MetaProgressionController.Instance.Menu.IsOpen && GameObject.Find("GrowthPanel") == null, "성장 화면 닫기 실패"); // 기존 닫기 동작 확인
            } // 조건 종료
            AutomatedDay148StartupVerification.Finish(true, "Startup stays closed and explicit growth selection remains available."); // 실제 성공 결과
        } // 함수 종료
        private static void Require(bool condition, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!condition) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedDay148StartupVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 실행기 종료
} // 영역 종료
