using System; // 실제 예외 처리
using System.Collections; // 프레임 진행 대기
using System.Collections.Generic; // 실제 UI 입력 결과 목록
using System.Reflection; // 실제 저장 서비스 연결
using ProjectEpsilon.Progression; // 영구 성장 기능
using ProjectEpsilon.UI; // 실제 메뉴 화면
using UnityEditor; // 유니티 배치 제어
using UnityEditor.SceneManagement; // 실제 시작 씬 열기
using UnityEngine; // 실제 화면과 입력 기능
using UnityEngine.UI; // 실제 글자와 버튼
using UnityEngine.EventSystems; // 실제 마우스 입력 판정

namespace ProjectEpsilon.Editor // 배치 시험 영역
{ // 영역 시작
    [InitializeOnLoad] // 플레이 상태 재연결
    public static class AutomatedDay144GrowthPanelVerification // 실제 영구 성장 화면 시험
    { // 시험 시작
        private const string Key = "EpsilonDay144GrowthPanel"; // 배치 실행 상태
        private static bool started; // 중복 실행 방지
        static AutomatedDay144GrowthPanelVerification() // 도메인 재시작 처리
        { // 생성자 시작
            if (SessionState.GetBool(Key, false)) // 실제 시험 상태 확인
            { // 조건 시작
                EditorApplication.update += Tick; // 시작 화면 감시
            } // 조건 종료
        } // 생성자 종료
        public static void Run() // 배치 실행 진입점
        { // 함수 시작
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); // 실제 메인 메뉴 씬 열기
            SessionState.SetBool(Key, true); // 배치 시험 상태 저장
            EditorApplication.update += Tick; // 시작 화면 감시
            EditorApplication.isPlaying = true; // 실제 플레이 시작
        } // 함수 종료
        private static void Tick() // 실제 시작 화면 감시
        { // 함수 시작
            if (!EditorApplication.isPlaying || started || RunFrontendController.Instance == null || !RunFrontendController.Instance.IsMainMenu || MetaProgressionController.Instance == null) // 실제 시작 상태 확인
            { // 조건 시작
                return; // 시작 화면 대기
            } // 조건 종료
            started = true; // 중복 시작 방지
            new GameObject("GrowthPanelVerificationRunner").AddComponent<GrowthPanelVerificationRunner>(); // 실제 화면 시험 시작
        } // 함수 종료
        public static void Finish(bool passed, string message) // 실제 시험 결과 기록
        { // 함수 시작
            SessionState.SetBool(Key, false); // 시험 상태 제거
            EditorApplication.update -= Tick; // 시작 감시 해제
            Debug.Log("[Day144Growth] " + (passed ? "PASS: " : "FAIL: ") + message); // 실제 시험 결과
            EditorApplication.Exit(passed ? 0 : 1); // 실제 배치 종료
        } // 함수 종료
    } // 시험 종료
    public sealed class GrowthPanelVerificationRunner : MonoBehaviour // 실제 화면 검증 실행기
    { // 실행기 시작
        private IEnumerator Start() // 실제 영구 성장 화면 검증
        { // 함수 시작
            yield return null; // 실제 메뉴 초기화 대기
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 성장 관리자
            GrowthPanelMemoryStorage storage = new GrowthPanelMemoryStorage(); // 사용자 저장과 분리한 시험 저장소
            MetaProgressionService service = new MetaProgressionService(storage); // 실제 성장 서비스
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, service); // 실제 메뉴의 시험 서비스 연결
            meta.Menu.Open(); // 첫 런 이전 성장 메뉴 시도
            Require(!meta.Menu.IsOpen, "첫 런 이전 성장 메뉴 열림"); // 기존 해금 규칙 확인
            Require(service.TryCompleteRun("growth-ui-test", 12000d, 0, 0, false, 0), "시험용 코어 지급 실패"); // 실제 보상 서비스로 시험 자원 준비
            int initialCores = service.Cores; // 실제 시작 코어 보존
            typeof(RunFrontendController).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(RunFrontendController.Instance, new object[] { FrontendStage.Main }); // 새 해금 상태의 실제 메인 메뉴
            Require(GameObject.Find("FrontendCoreStatus") == null, "메인 메뉴의 코어 표시가 남음"); // 메인 메뉴의 재화 표시 제거 확인
            Require(GameObject.Find("FrontendGrowthError") == null, "정상 저장에서 오류 표시"); // 정상 저장 상태 확인
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) // 그래픽 장치 확인
            { // 조건 시작
                Camera camera = Camera.main; // 실제 메뉴 카메라
                int previousMask = camera.cullingMask; // 기존 표시 범위
                camera.cullingMask = ~0; // 캡처에 메뉴 포함
                typeof(AutomatedDay139ScreenCapture).GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "day145-menu.png" }); // 실제 메뉴 화면 저장
                camera.cullingMask = previousMask; // 기존 표시 범위 복구
            } // 조건 종료
            Button open = FindButton("FrontendGrowthButton"); // 실제 메인 메뉴의 성장 버튼
            Require(open != null && open.interactable, "해금된 영구 성장 진입 버튼 오류"); // 실제 메인 메뉴 진입 확인
            open.onClick.Invoke(); // 실제 성장 버튼으로 메뉴 열기
            yield return null; // 실제 UI 배치 대기
            Require(meta.Menu.IsOpen && GameObject.Find("GrowthPanel") != null, "새 성장 패널 누락"); // 실제 개선된 성장 화면 확인
            Require(GameObject.Find("GrowthRemainingLabel") == null, "전체 강화까지 비용 문구가 남음"); // 불필요한 전체 비용 표시 제거 확인
            Require(GameObject.Find("GrowthBackdrop") != null && GameObject.Find("GrowthPanel").GetComponent<Image>().sprite != null, "성장 배경 또는 공통 프레임 누락"); // 실제 배경과 기존 스킨 확인
            for (int group = 0; group < 4; group++) // 실제 성장 계열 순회
            { // 순회 시작
                Require(GameObject.Find("GrowthCategory" + group) != null, "성장 계열 누락: " + group); // 실제 네 계열 확인
            } // 순회 종료
            for (int index = 0; index < 12; index++) // 실제 성장 노드 순회
            { // 순회 시작
                Button button = FindButton("GrowthNodeButton" + index); // 실제 성장 버튼
                Require(button != null && button.interactable == service.CanPurchase((MetaNode)index), "성장 버튼 상태 오류: " + index); // 실제 구매 가능 상태 확인
            } // 순회 종료
            Require(!FindButton("GrowthNodeButton1").interactable && !FindButton("GrowthNodeButton2").interactable, "선행 조건이 없는 성장 버튼 허용"); // 실제 선행 잠금 확인
            Canvas.ForceUpdateCanvases(); // 실제 입력 영역 갱신
            Button firstButton = FindButton("GrowthNodeButton0"); // 실제 첫 성장 버튼
            PointerEventData pointer = new PointerEventData(EventSystem.current); // 실제 마우스 포인터 입력
            pointer.position = RectTransformUtility.WorldToScreenPoint(null, firstButton.transform.position); // 실제 버튼 가운데 화면 위치
            var hits = new List<RaycastResult>(); // 실제 입력 대상 목록
            EventSystem.current.RaycastAll(pointer, hits); // 실제 UI 입력 순서 검사
            Require(hits.Count > 0 && (hits[0].gameObject == firstButton.gameObject || hits[0].gameObject.transform.IsChildOf(firstButton.transform)), "성장 버튼 위에 다른 화면 입력이 겹침"); // 실제 마우스 클릭 대상 확인
            FindButton("GrowthNodeButton0").onClick.Invoke(); // 실제 첫 성장 구매
            yield return null; // 실제 성장 화면 갱신 대기
            Require(service.GetLevel(MetaNode.Health) == 1 && service.Cores == initialCores - 3 && FindButton("GrowthNodeButton1").interactable, "실제 성장 구매 또는 선행 해금 오류"); // 기존 구매와 계열 잠금 확인
            Text panelCore = GameObject.Find("GrowthCoreLabel")?.GetComponent<Text>(); // 영구 성장 화면의 재화 글자
            Require(panelCore != null && panelCore.text.Contains(service.Cores.ToString("N0")), "구매 후 영구 성장 코어 미갱신"); // 실제 남은 재화 표시 확인
            Text lockedCost = GameObject.Find("GrowthNodeButton2Label")?.GetComponent<Text>(); // 실제 잠긴 노드 비용 글자
            Require(lockedCost != null && lockedCost.color.r < 0.8f, "잠긴 성장 버튼의 글자 구분 누락"); // 실제 잠금 글자 색상 확인
            int afterPurchase = service.Cores; // 정상 구매 후 실제 코어
            FindButton("GrowthNodeButton2").onClick.Invoke(); // 잠긴 최종 성장의 직접 호출 시험
            Require(service.Cores == afterPurchase && service.GetLevel(MetaNode.LastBodySpeed) == 0, "잠긴 성장 구매 허용"); // 실제 선행 조건 재확인
            storage.FailWrite = true; // 실제 저장 실패 재현
            FindButton("GrowthNodeButton0").onClick.Invoke(); // 저장 실패 중 실제 성장 구매
            Require(service.Cores == afterPurchase && service.GetLevel(MetaNode.Health) == 1, "저장 실패에 재화 또는 단계 변경"); // 저장 실패 시 기존 성장 보존
            storage.FailWrite = false; // 정상 저장 복구
            Canvas.ForceUpdateCanvases(); // 실제 UI 크기와 위치 갱신
            RectTransform panel = (RectTransform)GameObject.Find("GrowthPanel").transform; // 실제 메인 패널
            RectTransform first = (RectTransform)GameObject.Find("GrowthCategory0").transform; // 실제 첫 계열 영역
            RectTransform second = (RectTransform)GameObject.Find("GrowthCategory1").transform; // 실제 두 번째 계열 영역
            RectTransform third = (RectTransform)GameObject.Find("GrowthCategory2").transform; // 실제 세 번째 계열 영역
            Vector3[] a = new Vector3[4]; // 첫 영역 모서리
            Vector3[] b = new Vector3[4]; // 두 번째 영역 모서리
            Vector3[] c = new Vector3[4]; // 세 번째 영역 모서리
            first.GetWorldCorners(a); // 실제 첫 영역 위치
            second.GetWorldCorners(b); // 실제 두 번째 영역 위치
            third.GetWorldCorners(c); // 실제 세 번째 영역 위치
            Require(a[2].x <= b[0].x + 0.01f && c[1].y <= a[0].y + 0.01f, "성장 계열 패널 겹침"); // 실제 가로와 세로 영역 분리 확인
            Require(panel.rect.width > 0f && panel.rect.height > 0f, "성장 화면 크기 오류"); // 실제 패널 유효성 확인
            if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null) // 그래픽 장치 사용 가능 확인
            { // 조건 시작
                GameObject underlying = GameObject.Find("RunFrontendCanvas"); // 실제 뒤쪽 메인 메뉴
                if (underlying != null) // 실제 뒤쪽 화면 확인
                { // 조건 시작
                    underlying.SetActive(false); // 캡처용 화면 겹침 방지
                } // 조건 종료
                Camera camera = Camera.main; // 실제 메뉴 표시 카메라
                int previousMask = camera.cullingMask; // 기존 카메라 표시 범위
                camera.cullingMask = ~0; // 캡처에 실제 UI 포함
                typeof(AutomatedDay139ScreenCapture).GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "day144-growth.png" }); // 실제 영구 성장 화면 캡처
                camera.cullingMask = previousMask; // 기존 카메라 표시 범위 복구
                if (underlying != null) // 실제 뒤쪽 화면 확인
                { // 조건 시작
                    underlying.SetActive(true); // 실제 메인 메뉴 표시 복구
                } // 조건 종료
            } // 조건 종료
            FindButton("GrowthRefundButton").onClick.Invoke(); // 실제 성장 무료 초기화
            yield return null; // 실제 초기화 갱신 대기
            Require(service.GetLevel(MetaNode.Health) == 0 && service.Cores == initialCores && !FindButton("GrowthNodeButton1").interactable, "성장 초기화 또는 재화 반환 오류"); // 실제 초기화 규칙 확인
            Require(panelCore.text.Contains(initialCores.ToString("N0")), "초기화 후 영구 성장 코어 미갱신"); // 실제 반환된 재화 표시 확인
            FindButton("GrowthCloseButton").onClick.Invoke(); // 실제 성장 화면 닫기
            Require(!meta.Menu.IsOpen && GameObject.Find("GrowthPanel") == null, "성장 화면 닫기 실패"); // 실제 화면 닫기 확인
            meta.Menu.Open(); // 실제 성장 화면 재진입
            Require(meta.Menu.IsOpen && FindButton("GrowthNodeButton0") != null, "성장 화면 재진입 실패"); // 기존 UI 재사용 확인
            FindButton("GrowthCloseButton").onClick.Invoke(); // 실제 재진입 화면 닫기
            storage.FailRead = true; // 저장 읽기 실패 재현
            typeof(MetaProgressionController).GetProperty("Service").SetValue(meta, new MetaProgressionService(storage)); // 오류 저장 서비스 연결
            typeof(RunFrontendController).GetMethod("Show", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(RunFrontendController.Instance, new object[] { FrontendStage.Main }); // 오류 상태의 메인 메뉴 갱신
            Require(GameObject.Find("FrontendGrowthError") != null && !FindButton("FrontendGrowthButton").interactable, "성장 저장 오류 안내 또는 차단 누락"); // 실제 저장 오류 안내 확인
            AutomatedDay144GrowthPanelVerification.Finish(true, "Growth panel, purchase, prerequisite, refund and close passed."); // 실제 성공 결과
        } // 함수 종료
        private static Button FindButton(string name) // 실제 활성 버튼 조회
        { // 함수 시작
            GameObject item = GameObject.Find(name); // 실제 버튼 객체
            return item == null ? null : item.GetComponent<Button>(); // 실제 버튼 반환
        } // 함수 종료
        private static void Require(bool value, string message) // 실제 시험 조건 확인
        { // 함수 시작
            if (!value) // 실제 실패 조건 확인
            { // 조건 시작
                AutomatedDay144GrowthPanelVerification.Finish(false, message); // 실제 실패 기록
                throw new InvalidOperationException(message); // 후속 시험 중단
            } // 조건 종료
        } // 함수 종료
    } // 실행기 종료
    public sealed class GrowthPanelMemoryStorage : IMetaStorage // 실제 사용자 저장과 분리한 시험 저장소
    { // 저장소 시작
        private string content; // 실제 시험 저장 내용
        public bool FailWrite { get; set; } // 시험용 저장 실패 상태
        public bool FailRead { get; set; } // 시험용 읽기 실패 상태
        public bool TryRead(out string json) // 실제 시험 저장 읽기
        { // 함수 시작
            json = content; // 실제 저장 내용 전달
            return !FailRead; // 읽기 성공 또는 시험 오류
        } // 함수 종료
        public bool TryWrite(string json) // 실제 시험 저장 기록
        { // 함수 시작
            if (FailWrite) // 실제 시험 저장 실패 확인
            { // 조건 시작
                return false; // 기존 저장 내용 보존
            } // 조건 종료
            content = json; // 실제 시험 내용 보존
            return true; // 기록 성공
        } // 함수 종료
    } // 저장소 종료
} // 영역 종료
