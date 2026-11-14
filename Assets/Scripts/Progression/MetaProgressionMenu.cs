using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Player; // 실제 시작 능력치
using ProjectEpsilon.UI; // 기존 화면과 공통 UI 스킨
using UnityEngine; // 유니티 화면 기능
using UnityEngine.UI; // 실제 성장 버튼과 글자

namespace ProjectEpsilon.Progression // 영구 성장 영역
{ // 영역 시작
    public sealed class MetaProgressionMenu : MonoBehaviour // 실제 영구 성장 화면
    { // 클래스 시작
        private static readonly string[] Labels = // 실제 열두 성장 노드 안내
        { // 목록 시작
            "체력 +4%", "회복 +5%", "마지막 몸통 속도", // 생존 계열 효과
            "회전 +3%", "스태미나 +5%", "저스태미나 효과", // 기동 계열 효과
            "공격력 +2%", "공격 속도 +2%", "3등급 이상 피해", // 전투 계열 효과
            "경험치 +3%", "획득 범위 +5%", "재선택 1회" // 성장 계열 효과
        }; // 목록 종료
        private static readonly string[] Categories = { "생존", "기동", "전투", "성장" }; // 실제 네 성장 계열
        private static readonly Color[] CategoryColors = // 계열별 제목 색상
        { // 색상 시작
            new Color(0.42f, 0.95f, 0.75f), new Color(0.38f, 0.84f, 1f), // 생존과 기동 색상
            new Color(1f, 0.75f, 0.38f), new Color(0.82f, 0.62f, 1f) // 전투와 성장 색상
        }; // 색상 종료
        private readonly Button[] nodeButtons = new Button[12]; // 실제 열두 성장 버튼
        private readonly Text[] nodeLabels = new Text[12]; // 실제 성장 효과와 단계 글자
        private readonly Text[] buttonLabels = new Text[12]; // 실제 성장 비용 글자
        private MetaProgressionController controller; // 실제 성장 관리자
        private GameObject canvasRoot; // 실제 독립 성장 화면
        private Font displayFont; // 실제 한글 화면 글꼴
        private Text coreLabel; // 실제 보유 코어 글자
        private Text remainingLabel; // 실제 남은 강화 비용 글자
        public bool IsOpen { get; private set; } // 실제 성장 화면 표시 상태
        public void Configure(MetaProgressionController source) // 실제 관리자 연결
        { // 함수 시작
            controller = source; // 실제 성장 관리자 저장
        } // 함수 종료
        public void Open() // 실제 영구 성장 화면 열기
        { // 함수 시작
            if (!CanShow()) // 실제 시작 화면과 성장 해금 확인
            { // 조건 시작
                return; // 열 수 없는 상태 차단
            } // 조건 종료
            if (canvasRoot == null) // 실제 화면 생성 여부 확인
            { // 조건 시작
                BuildPanel(); // 실제 성장 UI 생성
            } // 조건 종료
            IsOpen = true; // 실제 성장 화면 표시 상태
            canvasRoot.SetActive(true); // 실제 성장 화면 활성화
            RefreshPanel(); // 실제 보유 재화와 성장 상태 반영
        } // 함수 종료
        private bool CanShow() // 성장 화면 표시 가능 상태 확인
        { // 함수 시작
            if (controller == null || controller.Service == null || !controller.Service.IsAvailable || !controller.Service.FinishedFirstRun) // 실제 저장과 첫 전투 확인
            { // 조건 시작
                return false; // 기존 해금 조건 유지
            } // 조건 종료
            RunFrontendController frontend = RunFrontendController.Instance; // 실제 메뉴 관리자
            bool mainMenu = frontend != null && frontend.IsMainMenu; // 실제 메인 메뉴 상태
            GameManager game = GameManager.Instance; // 실제 전투 관리자
            bool starting = game == null ? mainMenu : game.CurrentState == GameState.Starting; // 기존 시작 상태 판정
            return starting && (frontend == null || mainMenu); // 메인 메뉴 또는 기존 시작 상태만 허용
        } // 함수 종료
        private void Update() // 실제 메뉴 상태 유지
        { // 함수 시작
            if (IsOpen && !CanShow()) // 메뉴 이동 또는 저장 상태 변경 확인
            { // 조건 시작
                Close(); // 다른 화면에서 성장 메뉴 닫기
            } // 조건 종료
        } // 함수 종료
        private void OnGUI() // 기존 단일 씬의 성장 메뉴 진입
        { // 함수 시작
            if (RunFrontendController.Instance != null || IsOpen || !CanShow()) // 독립 메뉴 또는 유효한 시작 상태 확인
            { // 조건 시작
                return; // 기존 단일 씬 외 버튼 생략
            } // 조건 종료
            if (GUI.Button(new Rect(16f, 16f, 180f, 36f), "영구 성장")) // 기존 단일 씬 진입 버튼
            { // 조건 시작
                Open(); // 실제 성장 화면 열기
            } // 조건 종료
        } // 함수 종료
        private void BuildPanel() // 실제 영구 성장 UI 구성
        { // 함수 시작
            displayFont = RunFrontendController.Instance == null ? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") : RunFrontendController.Instance.DisplayFont; // 기존 한글 글꼴 재사용
            canvasRoot = new GameObject("GrowthCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // 별도 입력 계층의 성장 화면
            canvasRoot.transform.SetParent(transform, false); // 실제 성장 관리자 아래 연결
            Canvas canvas = canvasRoot.GetComponent<Canvas>(); // 실제 성장 캔버스
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 실제 전체 화면 UI
            canvas.sortingOrder = 50; // 메인 메뉴와 설정 화면 위 표시
            CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>(); // 실제 화면 크기 조정기
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 화면 크기별 비율 적용
            scaler.referenceResolution = new Vector2(1920f, 1080f); // 기존 메뉴 기준 해상도
            scaler.matchWidthOrHeight = 0.5f; // 가로와 세로 균형
            GameObject backdrop = new GameObject("GrowthBackdrop", typeof(RectTransform), typeof(Image)); // 뒷화면을 가리는 배경
            RectTransform backRect = backdrop.GetComponent<RectTransform>(); // 실제 배경 영역
            backRect.SetParent(canvasRoot.transform, false); // 실제 성장 화면에 연결
            backRect.anchorMin = Vector2.zero; // 전체 화면 시작 기준
            backRect.anchorMax = Vector2.one; // 전체 화면 끝 기준
            backRect.offsetMin = backRect.offsetMax = Vector2.zero; // 전체 화면 여백 제거
            backdrop.GetComponent<Image>().color = new Color(0.01f, 0.025f, 0.055f, 0.96f); // 기존 배경과 구분되는 어두운 막
            Image frame = CreateImage(canvasRoot.transform, "GrowthPanel", Vector2.zero, new Vector2(1540f, 940f), Color.white); // 실제 성장 패널
            ProjectUiSkin.ApplySidebar(frame, true); // 기존 공통 패널 스킨 적용
            CreateText(frame.transform, "GrowthTitle", "영구 성장", new Vector2(0f, 405f), new Vector2(1200f, 70f), 46, TextAnchor.MiddleCenter, Color.white); // 실제 화면 제목
            coreLabel = CreateText(frame.transform, "GrowthCoreLabel", string.Empty, new Vector2(-280f, 350f), new Vector2(500f, 42f), 26, TextAnchor.MiddleCenter, new Color(0.46f, 0.95f, 1f)); // 실제 보유 재화 표시
            remainingLabel = CreateText(frame.transform, "GrowthRemainingLabel", string.Empty, new Vector2(290f, 350f), new Vector2(600f, 42f), 24, TextAnchor.MiddleCenter, new Color(0.76f, 0.84f, 0.93f)); // 실제 전체 강화 비용 표시
            for (int group = 0; group < Categories.Length; group++) // 실제 성장 계열 생성
            { // 순회 시작
                int currentGroup = group; // 현재 계열 번호
                Vector2 center = new Vector2(group % 2 == 0 ? -370f : 370f, group < 2 ? 155f : -170f); // 두 열과 두 행의 계열 위치
                Image category = CreateImage(frame.transform, "GrowthCategory" + group, center, new Vector2(700f, 300f), Color.white); // 실제 계열 패널
                ProjectUiSkin.ApplySidebar(category, true); // 기존 공통 테두리 재사용
                CreateText(category.transform, "GrowthCategoryTitle" + group, Categories[group], new Vector2(0f, 116f), new Vector2(630f, 42f), 26, TextAnchor.MiddleLeft, CategoryColors[group]); // 실제 계열 제목
                for (int row = 0; row < 3; row++) // 실제 계열의 세 노드 순회
                { // 순회 시작
                    int index = currentGroup * 3 + row; // 실제 성장 노드 번호
                    Image item = CreateImage(category.transform, "GrowthNode" + index, new Vector2(0f, 56f - row * 74f), new Vector2(670f, 66f), new Color(0.08f, 0.14f, 0.21f, 0.9f)); // 실제 노드 행 배경
                    nodeLabels[index] = CreateText(item.transform, "GrowthNodeLabel" + index, string.Empty, new Vector2(-90f, 0f), new Vector2(450f, 60f), 24, TextAnchor.MiddleLeft, Color.white); // 실제 효과와 단계 표시
                    nodeLabels[index].resizeTextForBestFit = true; // 긴 효과 이름 자동 맞춤
                    nodeLabels[index].resizeTextMinSize = 17; // 노드 글자의 최소 크기
                    nodeLabels[index].resizeTextMaxSize = 24; // 노드 글자의 최대 크기
                    nodeButtons[index] = CreateButton(item.transform, "GrowthNodeButton" + index, new Vector2(245f, 0f), new Vector2(165f, 54f), out buttonLabels[index]); // 실제 구매 버튼
                    nodeButtons[index].onClick.AddListener(() => Purchase(index)); // 실제 노드 구매 연결
                } // 순회 종료
            } // 순회 종료
            Button refund = CreateButton(frame.transform, "GrowthRefundButton", new Vector2(-190f, -405f), new Vector2(300f, 64f), out Text refundText); // 실제 무료 초기화 버튼
            refundText.text = "무료 초기화"; // 실제 초기화 안내
            refund.onClick.AddListener(Refund); // 실제 코어 반환 연결
            Button close = CreateButton(frame.transform, "GrowthCloseButton", new Vector2(190f, -405f), new Vector2(300f, 64f), out Text closeText); // 실제 닫기 버튼
            closeText.text = "닫기"; // 실제 닫기 안내
            close.onClick.AddListener(Close); // 실제 메뉴 닫기 연결
        } // 함수 종료
        private void RefreshPanel() // 실제 성장 정보와 버튼 갱신
        { // 함수 시작
            MetaProgressionService service = controller.Service; // 실제 저장 서비스
            coreLabel.text = $"보유 코어  {service.Cores:N0}"; // 실제 보유 재화
            remainingLabel.text = $"전체 강화까지  {service.RemainingUpgradeCost:N0} 코어"; // 실제 남은 강화 비용
            for (int index = 0; index < nodeButtons.Length; index++) // 실제 열두 노드 순회
            { // 순회 시작
                MetaNode node = (MetaNode)index; // 실제 성장 노드
                int level = service.GetLevel(node); // 실제 성장 단계
                int maximum = index % 3 == 2 ? 1 : 5; // 실제 노드 최대 단계
                int cost = service.GetNextCost(node); // 실제 다음 성장 비용
                bool unlocked = index % 3 == 0 || service.GetLevel((MetaNode)(index - index % 3)) >= 1 && (index % 3 == 1 || service.GetLevel((MetaNode)(index - 1)) >= 1); // 기존 선행 조건 표시
                nodeLabels[index].text = $"{Labels[index]}   {level}/{maximum}" + (cost >= 0 && !unlocked ? "   선행 필요" : string.Empty); // 실제 효과와 단계 및 잠금 표시
                nodeLabels[index].color = unlocked || cost < 0 ? Color.white : new Color(0.6f, 0.65f, 0.72f); // 실제 해금 상태 글자 색상
                buttonLabels[index].text = cost < 0 ? "최대" : $"강화  {cost} 코어"; // 실제 다음 비용 또는 최대 단계
                nodeButtons[index].interactable = service.CanPurchase(node); // 기존 구매 가능 조건 재사용
                buttonLabels[index].color = nodeButtons[index].interactable ? Color.white : new Color(0.55f, 0.6f, 0.67f); // 구매 불가 버튼의 글자 색상
            } // 순회 종료
        } // 함수 종료
        private void Purchase(int index) // 실제 성장 구매
        { // 함수 시작
            if (controller != null && controller.Service != null && controller.Service.TryPurchase((MetaNode)index)) // 기존 구매와 저장 확인
            { // 조건 시작
                RefreshStartingStats(); // 실제 시작 능력치 갱신
            } // 조건 종료
            if (IsOpen && controller != null && controller.Service != null) // 실제 성장 화면 확인
            { // 조건 시작
                RefreshPanel(); // 실제 성공 또는 실패 상태 갱신
            } // 조건 종료
        } // 함수 종료
        private void Refund() // 실제 성장 무료 초기화
        { // 함수 시작
            if (controller != null && controller.Service != null && controller.Service.TryRefundAll()) // 기존 재화 반환과 저장 확인
            { // 조건 시작
                RefreshStartingStats(); // 실제 시작 능력치 복구
            } // 조건 종료
            if (IsOpen && controller != null && controller.Service != null) // 실제 성장 화면 확인
            { // 조건 시작
                RefreshPanel(); // 실제 초기화 결과 표시
            } // 조건 종료
        } // 함수 종료
        private void Close() // 실제 성장 화면 닫기
        { // 함수 시작
            IsOpen = false; // 실제 표시 상태 해제
            if (canvasRoot != null) // 실제 화면 객체 확인
            { // 조건 시작
                canvasRoot.SetActive(false); // 실제 화면과 입력 비활성화
            } // 조건 종료
        } // 함수 종료
        private static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color) // 실제 UI 배경 생성
        { // 함수 시작
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image)); // 실제 이미지 객체
            RectTransform rect = item.GetComponent<RectTransform>(); // 실제 배치 영역
            rect.SetParent(parent, false); // 실제 부모에 연결
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 중앙 기준
            rect.anchoredPosition = position; // 실제 상대 위치
            rect.sizeDelta = size; // 실제 이미지 크기
            Image image = item.GetComponent<Image>(); // 실제 이미지 구성 요소
            image.color = color; // 실제 배경 색상
            return image; // 실제 이미지 반환
        } // 함수 종료
        private Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color) // 실제 UI 글자 생성
        { // 함수 시작
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Text)); // 실제 글자 객체
            RectTransform rect = item.GetComponent<RectTransform>(); // 실제 글자 영역
            rect.SetParent(parent, false); // 실제 부모에 연결
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 중앙 기준
            rect.anchoredPosition = position; // 실제 상대 위치
            rect.sizeDelta = size; // 실제 글자 크기
            Text text = item.GetComponent<Text>(); // 실제 글자 구성 요소
            text.font = displayFont; // 기존 한글 화면 글꼴
            text.fontSize = fontSize; // 실제 글자 크기
            text.alignment = alignment; // 실제 글자 정렬
            text.color = color; // 실제 글자 색상
            text.text = value; // 실제 표시 내용
            text.raycastTarget = false; // 아래 버튼 입력 보존
            return text; // 실제 글자 반환
        } // 함수 종료
        private Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, out Text label) // 실제 스킨 버튼 생성
        { // 함수 시작
            Image image = CreateImage(parent, name, position, size, Color.white); // 실제 버튼 배경
            Button button = image.gameObject.AddComponent<Button>(); // 실제 클릭 처리
            button.targetGraphic = image; // 실제 버튼 색상 대상
            ProjectUiSkin.ApplyButton(button); // 기존 공통 버튼 스킨 적용
            label = CreateText(image.transform, name + "Label", string.Empty, Vector2.zero, size - new Vector2(8f, 4f), 21, TextAnchor.MiddleCenter, Color.white); // 실제 버튼 글자
            label.resizeTextForBestFit = true; // 긴 버튼 문구 자동 맞춤
            label.resizeTextMinSize = 14; // 버튼 글자의 최소 크기
            label.resizeTextMaxSize = 21; // 버튼 글자의 최대 크기
            return button; // 실제 버튼 반환
        } // 함수 종료
        private static void RefreshStartingStats() // 실제 시작 능력치 갱신
        { // 함수 시작
            CharacterRunInitializer initializer = FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 초기화기
            if (initializer != null && CharacterRunInitializer.ActiveCharacter != null) // 실제 시작 캐릭터 확인
            { // 조건 시작
                initializer.TryApply(CharacterRunInitializer.ActiveCharacter); // 실제 시작 체력 재적용
            } // 조건 종료
            SnakeStamina stamina = FindFirstObjectByType<SnakeStamina>(); // 실제 스태미나 조회
            stamina?.ResetStamina(); // 실제 새 최대치 회복
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
