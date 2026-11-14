using System; // 실제 시간과 동작
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Player; // 실제 캐릭터 선택
using ProjectEpsilon.Progression; // 실제 진행과 포탑 선택
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEngine; // Unity 화면 기능
using UnityEngine.SceneManagement; // 실제 화면 씬 전환
using UnityEngine.InputSystem; // 안내 취소 입력
using UnityEngine.UI; // 실제 화면과 버튼

namespace ProjectEpsilon.UI // 실제 메뉴 영역
{
    public enum FrontendStage // 실제 화면 단계
    {
        Main, Characters, Maps, Weapons, Playing, Paused, Settings, Results, Tutorial // 실제 메뉴 흐름
    }
    [DefaultExecutionOrder(300)] // 기존 시작 데이터 이후 화면 구성
    public sealed class RunFrontendController : MonoBehaviour // 실제 메뉴와 런 흐름
    {
        private static bool retryNext; // 실제 재도전 진입 예약
        private static int retryMap; // 실제 재도전 맵
        private Canvas canvas; // 실제 화면 캔버스
        private CanvasGroup group; // 실제 입력 우선권
        private RectTransform panel; // 실제 메뉴 배경
        private Font font; // 실제 기본 화면 글꼴
        private bool ownsFont; // 실제 생성 글꼴 소유 여부
        private SettingsMenuController inputSettings; // 기존 입력 설정 화면
        private CharacterRunInitializer characters; // 실제 캐릭터 선택기
        private PrototypeMapSelection maps; // 실제 맵 선택기
        private StartingWeaponSelection startingWeapons; // 실제 포탑 선택기
        private FrontendOptions options; // 실제 화면과 음량 설정
        private FrontendStage settingsReturn; // 설정 닫기 복귀 화면
        private SceneFrontendContext sceneContext; // 실제 분리 화면 데이터
        private const string TutorialCompletedKey = "ProjectEpsilon.TutorialCompleted.v1"; // 첫 안내 완료 저장 키
        private const int TutorialPageCount = 8; // 기획서 안내 단계 수
        private bool tutorialAutoChecked; // 첫 안내 자동 확인 여부
        private bool tutorialOwnsPause; // 안내가 시작한 일시정지 여부
        private int tutorialPage; // 현재 안내 단계
        private FrontendStage tutorialReturn; // 안내 종료 후 복귀 화면
        public static RunFrontendController Instance { get; private set; } // 실제 메뉴 관리자
        public FrontendStage Stage { get; private set; } // 실제 현재 단계
        public Font DisplayFont => font; // 실제 화면의 한글 우선 글꼴
        public bool IsMainMenu => Stage == FrontendStage.Main; // 실제 주 메뉴 여부
        public bool AllowsStartingWeaponSelection => Stage == FrontendStage.Weapons; // 실제 포탑 선택 허용
        private void Awake() // 실제 관리자 등록
        {
            Instance = this; // 실제 현재 화면 등록
            sceneContext = GetComponent<SceneFrontendContext>(); // 실제 독립 씬 구성 조회
            options = new FrontendOptions(); // 실제 저장된 화면 설정
        }
        private void Start() // 실제 시작 화면 구성
        {
            inputSettings = FindFirstObjectByType<SettingsMenuController>(); // 기존 입력 설정 조회
            characters = FindFirstObjectByType<CharacterRunInitializer>(); // 실제 캐릭터 선택 조회
            maps = FindFirstObjectByType<PrototypeMapSelection>(); // 실제 맵 선택 조회
            startingWeapons = FindFirstObjectByType<StartingWeaponSelection>(); // 실제 시작 포탑 조회
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Noto Sans CJK KR", "Arial" }, 32); // 한글 우선 시스템 글꼴
            ownsFont = font != null; // 실제 생성 글꼴 소유 기록
            if (font == null) // 실제 시스템 글꼴 확인
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Unity 기본 화면 글꼴
            }
            LevelUpPanelController rewardPanel = FindFirstObjectByType<LevelUpPanelController>(FindObjectsInactive.Include); // 실제 포탑과 아이템 보상 화면
            WeaponMergePanelController mergePanel = FindFirstObjectByType<WeaponMergePanelController>(FindObjectsInactive.Include); // 실제 포탑 합성 화면
            rewardPanel?.ApplyDisplayFont(font); // 실제 보상 화면 한글 글꼴 적용
            mergePanel?.ApplyDisplayFont(font); // 실제 합성 화면 한글 글꼴 적용
            HUDController hud = FindFirstObjectByType<HUDController>(FindObjectsInactive.Include); // 실제 기존 전투 HUD
            if (hud != null) // 실제 전투 HUD 확인
            {
                HudTextLocalizer.Apply(hud.transform, font); // 실제 기존 문구와 한글 글꼴 적용
            }
            GameObject canvasObject = new GameObject("RunFrontendCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup)); // 실제 메뉴 캔버스 생성
            canvasObject.transform.SetParent(transform, false); // 실제 관리자에 화면 연결
            canvas = canvasObject.GetComponent<Canvas>(); // 실제 캔버스 조회
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 실제 화면 위 표시
            canvas.sortingOrder = 30; // 기존 선택 화면 위 표시
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); // 실제 크기 조정기
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 해상도별 크기 조정
            scaler.referenceResolution = new Vector2(1920f, 1080f); // 실제 기준 해상도
            scaler.matchWidthOrHeight = 0.5f; // 가로와 세로 균형
            group = canvasObject.GetComponent<CanvasGroup>(); // 실제 입력 차단기
            GameObject panelObject = new GameObject("FrontendPanel", typeof(RectTransform), typeof(Image)); // 실제 단계 배경
            panelObject.transform.SetParent(canvasObject.transform, false); // 실제 캔버스에 배경 연결
            panel = panelObject.GetComponent<RectTransform>(); // 실제 배경 위치
            panel.anchorMin = Vector2.zero; // 실제 화면 왼쪽 아래
            panel.anchorMax = Vector2.one; // 실제 화면 오른쪽 위
            panel.offsetMin = Vector2.zero; // 실제 화면 여백 제거
            panel.offsetMax = Vector2.zero; // 실제 화면 여백 제거
            panelObject.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.085f, 0.97f); // 실제 어두운 배경
            if (sceneContext != null) // 실제 분리 화면 확인
            {
                if (sceneContext.Kind == SceneFrontendKind.MainMenu) // 실제 메인 메뉴 씬 확인
                {
                    SceneFrontendContext.ResetFlow(); // 이전 런의 화면 흐름 정리
                    CreateStandaloneInputSettings(); // 실제 메뉴의 키 설정 구성
                    Show(FrontendStage.Main); // 실제 첫 메뉴 표시
                }
                else if (sceneContext.Kind == SceneFrontendKind.Preparation) // 실제 출격 준비 씬 확인
                {
                    Show(FrontendStage.Characters); // 실제 새 출격 캐릭터 화면
                }
                else // 실제 결과 씬 확인
                {
                    Show(FrontendStage.Results); // 실제 런 결과 표시
                }
                return; // 기존 단일 씬 시작 처리 생략
            }
            if (SceneFrontendContext.TakeStart(out int selectedMap)) // 분리 준비 씬의 출격 확인
            {
                Show(FrontendStage.Weapons); // 실제 시작 포탑 선택 허용
                bool mapApplied = maps != null && maps.SelectMap(selectedMap); // 청크 생성 전 실제 선택 맵 적용
                startingWeapons?.RefreshStartingDisplay(); // 실제 캐릭터의 시작 후보 갱신
                if (!mapApplied || startingWeapons == null || !startingWeapons.IsSelecting) // 실제 맵과 시작 카드 확인
                { // 조건 시작
                    Debug.LogError("[Project Epsilon] 선택한 맵 또는 시작 포탑 카드를 준비하지 못했습니다."); // 실제 출격 오류 표시
                } // 조건 종료
                return; // 실제 분리 씬 출격 처리 완료
            }
            if (retryNext) // 실제 재도전 예약 확인
            {
                retryNext = false; // 실제 중복 예약 제거
                maps?.SelectMap(retryMap); // 이전 맵 복원
                startingWeapons?.RefreshStartingDisplay(); // 실제 맵 안내 복원
                Show(FrontendStage.Weapons); // 실제 포탑 선택 복귀
            }
            else // 일반 시작 확인
            {
                Show(GameManager.Instance != null && GameManager.Instance.IsPlaying ? FrontendStage.Playing : FrontendStage.Main); // 실제 게임 상태에 맞는 화면
            }
        }
        private void Update() // 실제 화면 상태 동기화
        {
            if (canvas == null) // 실제 초기화 확인
            {
                return; // 화면 구성 대기
            }
            bool inputOpen = inputSettings != null && inputSettings.IsOpen; // 기존 입력 설정 상태
            bool growthOpen = MetaProgressionController.Instance != null && MetaProgressionController.Instance.Menu != null && MetaProgressionController.Instance.Menu.IsOpen; // 실제 성장 화면 상태
            canvas.enabled = !inputOpen; // 기존 설정 화면 표시 우선
            group.blocksRaycasts = !inputOpen && !growthOpen; // 실제 화면 입력 우선권
            if (Stage == FrontendStage.Tutorial && !inputOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) // 안내 취소 입력 확인
            { // 조건 시작
                FinishTutorial(); // 안내 종료와 이전 화면 복귀
                return; // 같은 프레임의 다른 화면 처리 방지
            } // 조건 종료
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            if (game == null) // 실제 관리자 확인
            {
                return; // 추가 화면 처리 차단
            }
            if ((game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated) && Stage != FrontendStage.Results) // 실제 종료 확인
            {
                if (sceneContext == null && SceneFrontendContext.UsesSeparateScenes) // 실제 분리 씬 런 확인
                {
                    canvas.gameObject.SetActive(false); // 기존 전투 화면 입력 차단
                    Destroy(canvas.gameObject); // 이전 씬의 메뉴 화면 제거
                    Destroy(this); // 이전 화면 제어기 제거
                    SceneManager.LoadScene(SceneFrontendContext.ResultsPath); // 실제 결과 씬 이동
                    return; // 기존 단일 씬 결과 처리 생략
                }
                Show(FrontendStage.Results); // 실제 결과 표시
            }
            else if (game.IsPlaying && Stage != FrontendStage.Playing && Stage != FrontendStage.Settings) // 실제 출격과 외부 재개 확인
            {
                Show(FrontendStage.Playing); // 실제 전투 화면 표시
            }
            if (sceneContext == null && game.IsPlaying && Stage == FrontendStage.Playing && !tutorialAutoChecked) // 첫 실제 전투 확인
            { // 조건 시작
                tutorialAutoChecked = true; // 같은 런의 중복 안내 방지
                if (NeedsFirstTutorial()) // 첫 플레이 안내 확인
                { // 조건 시작
                    BeginTutorial(false); // 첫 전투 안내 시작
                } // 조건 종료
            } // 조건 종료
        }
        private void OnDestroy() // 실제 관리자 정리
        {
            if (ownsFont && font != null) // 실제 생성 글꼴 확인
            {
                Destroy(font); // 실제 글꼴 자원 정리
            }
            if (Instance == this) // 실제 현재 관리자 확인
            {
                Instance = null; // 실제 전역 참조 정리
            }
        }
        public void ShowManualPause() // 실제 수동 일시정지 표시
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused && (Stage == FrontendStage.Playing || Stage == FrontendStage.Weapons)) // 실제 수동 정지 확인
            {
                Show(FrontendStage.Paused); // 실제 정지 화면 표시
            }
        }
        public void HideManualPause() // 실제 수동 일시정지 닫기
        {
            if (Stage == FrontendStage.Paused) // 실제 정지 화면 확인
            {
                Show(FrontendStage.Playing); // 실제 전투 복귀
            }
        }
        private void Show(FrontendStage next) // 실제 단계 전환
        {
            Stage = next; // 실제 현재 단계 기록
            bool showPanel = next != FrontendStage.Playing && (next != FrontendStage.Weapons || sceneContext != null && sceneContext.Kind == SceneFrontendKind.Preparation); // 실제 전체 화면 필요 여부
            bool opaque = next == FrontendStage.Main || next == FrontendStage.Characters || next == FrontendStage.Maps || next == FrontendStage.Results || next == FrontendStage.Weapons && sceneContext != null; // 실제 전투와 분리할 전체 화면 확인
            panel.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.085f, opaque ? 1f : 0.97f); // 실제 선택·결과 화면 불투명 배경
            panel.gameObject.SetActive(showPanel); // 실제 메뉴 배경 표시
            if (!showPanel) // 실제 전투나 포탑 화면 확인
            {
                return; // 전체 메뉴 생성 생략
            }
            foreach (Transform child in panel) // 이전 화면 요소 순회
            {
                child.gameObject.SetActive(false); // 실제 이전 입력 즉시 차단
                Destroy(child.gameObject); // 실제 이전 화면 제거
            }
            if (next == FrontendStage.Characters || next == FrontendStage.Maps || next == FrontendStage.Weapons) // 실제 출격 전 선택 화면 확인
            {
                CreateSelectionBackground(); // 승인된 이미지의 불투명 선택 배경
            }
            if (next == FrontendStage.Paused || next == FrontendStage.Settings || next == FrontendStage.Results || next == FrontendStage.Tutorial) // 실제 공통 정보 화면 확인
            { // 조건 시작
                CreateContentFrame(panel, new Vector2(1600f, 1020f)); // 실제 정보 화면 프레임
            } // 조건 종료
            switch (next) // 실제 화면 종류 확인
            {
                case FrontendStage.Main: BuildMain(); break; // 실제 주 메뉴 구성
                case FrontendStage.Characters: BuildCharacters(); break; // 실제 캐릭터 구성
                case FrontendStage.Maps: BuildMaps(); break; // 실제 맵 구성
                case FrontendStage.Paused: BuildPause(); break; // 실제 일시정지 구성
                case FrontendStage.Settings: BuildSettings(); break; // 실제 설정 구성
                case FrontendStage.Results: BuildResults(); break; // 실제 결과 구성
                case FrontendStage.Tutorial: BuildTutorial(); break; // 첫 플레이 안내 구성
            }
        }
        private static void CreateContentFrame(Transform parent, Vector2 size) // 공통 정보 화면 배경
        { // 함수 시작
            GameObject frame = new GameObject("FrontendContentFrame", typeof(RectTransform), typeof(Image)); // 실제 공통 패널
            RectTransform rect = (RectTransform)frame.transform; // 실제 패널 위치
            rect.SetParent(parent, false); // 실제 화면에 연결
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 중앙 기준
            rect.anchoredPosition = Vector2.zero; // 실제 중앙 배치
            rect.sizeDelta = size; // 실제 패널 크기
            Image image = frame.GetComponent<Image>(); // 실제 배경 이미지
            image.color = new Color(0.035f, 0.06f, 0.12f, 0.94f); // 이미지 부재 시 기본 배경
            ProjectUiSkin.ApplySidebar(image, true); // 공통 정보 화면 프레임
            image.raycastTarget = false; // 실제 버튼 입력 보존
        } // 함수 종료
        private void CreateSelectionBackground() // 실제 출격 선택 배경 구성
        {
            Texture2D background = Resources.Load<Texture2D>("MenuBackground_Casual"); // 실제 승인된 메뉴 배경 이미지
            if (background == null) // 실제 이미지 존재 확인
            {
                return; // 기존 불투명 패널 유지
            }
            GameObject artwork = new GameObject("FrontendBackground", typeof(RectTransform), typeof(RawImage)); // 실제 선택 배경 객체
            artwork.transform.SetParent(panel, false); // 실제 선택 패널에 배경 연결
            RectTransform rect = artwork.GetComponent<RectTransform>(); // 실제 전체 화면 영역
            rect.anchorMin = Vector2.zero; // 실제 왼쪽 아래 기준
            rect.anchorMax = Vector2.one; // 실제 오른쪽 위 기준
            rect.offsetMin = Vector2.zero; // 실제 왼쪽 아래 여백 제거
            rect.offsetMax = Vector2.zero; // 실제 오른쪽 위 여백 제거
            RawImage image = artwork.GetComponent<RawImage>(); // 실제 배경 이미지 표시기
            image.texture = background; // 실제 승인 배경 적용
            image.color = new Color(0.28f, 0.28f, 0.28f, 1f); // 실제 선택 글자의 대비 확보
            image.raycastTarget = false; // 실제 선택 버튼 입력 보존
        }
        private void BuildMain() // 실제 주 메뉴 구성
        {
            Texture2D background = Resources.Load<Texture2D>("MenuBackground_LeftFacing"); // 머리 방향을 수정한 메뉴 이미지
            if (background == null) // 수정 이미지 누락 확인
            {
                background = Resources.Load<Texture2D>("MenuBackground_Casual"); // 기존 배경 대체 사용
            }
            if (background != null) // 실제 메뉴 이미지 확인
            {
                GameObject artwork = new GameObject("FrontendBackground", typeof(RectTransform), typeof(RawImage)); // 실제 메뉴 배경 객체
                artwork.transform.SetParent(panel, false); // 실제 화면 배경 연결
                RectTransform artRect = artwork.GetComponent<RectTransform>(); // 실제 이미지 영역
                artRect.anchorMin = Vector2.zero; // 실제 화면 왼쪽 아래
                artRect.anchorMax = Vector2.one; // 실제 화면 오른쪽 위
                artRect.offsetMin = Vector2.zero; // 실제 화면 여백 제거
                artRect.offsetMax = Vector2.zero; // 실제 화면 여백 제거
                RawImage image = artwork.GetComponent<RawImage>(); // 실제 배경 이미지
                image.texture = background; // 실제 생성 이미지 적용
                image.raycastTarget = false; // 실제 버튼 입력 방해 방지
            }
            Label(panel, "FrontendTitle", "프로젝트 엡실론", new Vector2(500f, 425f), new Vector2(820f, 100f), 56); // 오른쪽 주 메뉴 제목
            Label(panel, "FrontendSubtitle", "생존  /  성장  /  진화", new Vector2(500f, 355f), new Vector2(820f, 55f), 26); // 오른쪽 주 메뉴 안내
            CreateButton(panel, "FrontendStartButton", "시작", new Vector2(500f, 140f), new Vector2(460f, 86f), GoToPreparation); // 실제 캐릭터 선택 진입
            Button growth = CreateButton(panel, "FrontendGrowthButton", "영구 성장", new Vector2(500f, 35f), new Vector2(460f, 86f), () => MetaProgressionController.Instance?.Menu?.Open()); // 실제 성장 메뉴 진입
            growth.interactable = MetaProgressionController.Instance != null && MetaProgressionController.Instance.Service != null && MetaProgressionController.Instance.Service.FinishedFirstRun && MetaProgressionController.Instance.Service.IsAvailable; // 실제 첫 런 해금 확인
            CreateButton(panel, "FrontendSettingsButton", "설정", new Vector2(500f, -70f), new Vector2(460f, 86f), () => OpenSettings(FrontendStage.Main)); // 실제 설정 진입
            CreateButton(panel, "FrontendQuitButton", "종료", new Vector2(500f, -175f), new Vector2(460f, 86f), Application.Quit); // 실제 게임 종료
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 코어 관리자
            Label(panel, "FrontendCoreStatus", meta != null && meta.Service != null && meta.Service.IsAvailable ? $"코어  {meta.Service.Cores}" : "성장 저장을 사용할 수 없습니다", new Vector2(500f, -300f), new Vector2(800f, 55f), 26); // 오른쪽 저장 상태 표시
        }
        private void GoToPreparation() // 실제 출격 준비 화면 이동
        {
            if (sceneContext != null && sceneContext.Kind == SceneFrontendKind.MainMenu) // 실제 독립 메인 메뉴 확인
            {
                SceneManager.LoadScene(SceneFrontendContext.PreparationPath); // 실제 출격 준비 씬 이동
                return; // 기존 단일 씬 전환 생략
            }
            Show(FrontendStage.Characters); // 기존 단일 씬의 캐릭터 화면
        }
        private void BuildCharacters() // 실제 캐릭터 선택 구성
        {
            Title("캐릭터 선택", "해금된 캐릭터를 선택하세요"); // 실제 캐릭터 제목
            if (characters == null && sceneContext == null) // 실제 선택 데이터 확인
            {
                Label(panel, "FrontendCharacterError", "캐릭터 데이터를 찾을 수 없습니다", Vector2.zero, new Vector2(900f, 70f), 28); // 실제 구성 오류 표시
                return; // 잘못된 선택 차단
            }
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            int characterCount = sceneContext == null ? characters.CharacterCount : sceneContext.CharacterCount; // 실제 캐릭터 후보 수
            for (int index = 0; index < characterCount; index++) // 실제 여섯 캐릭터 순회
            {
                int selected = index; // 실제 버튼별 캐릭터 번호
                CharacterData character = sceneContext == null ? characters.GetCharacter(index) : sceneContext.GetCharacter(index); // 실제 캐릭터 후보
                if (character == null) // 실제 누락 후보 확인
                {
                    continue; // 잘못된 후보 생략
                }
                bool unlocked = meta == null || meta.IsCharacterUnlocked(character.Id); // 실제 해금 상태
                string label = character.DisplayName + (unlocked ? "" : "  |  잠김: 개방 평원 보스 처치"); // 실제 이름과 해금 조건
                Button button = CreateButton(panel, "FrontendCharacterButton" + index, label, new Vector2(-520f + index % 2 * 450f, 240f - index / 2 * 105f), new Vector2(420f, 82f), () => SelectCharacter(selected)); // 실제 캐릭터 선택 버튼
                button.interactable = unlocked; // 잠긴 캐릭터 선택 차단
            }
            CharacterData active = sceneContext == null ? CharacterRunInitializer.ActiveCharacter : sceneContext.SelectedCharacter; // 실제 선택 캐릭터
            if (active != null) // 실제 캐릭터 확인
            {
                string weapons = active.StartingWeapons == null || active.StartingWeapons.Weapons == null ? "-" : string.Join("  /  ", Array.ConvertAll(active.StartingWeapons.Weapons, item => item == null ? "-" : item.DisplayName)); // 실제 세 포탑 후보
                string info = $"{active.DisplayName}\n몸통 {active.StartingBodyCount}   체력 {active.BodyMaxHealth:0}\n이동 ×{active.MoveSpeedMultiplier:0.00}   회전 ×{active.TurnPowerMultiplier:0.00}\n특성: {DescribePassive(active)}\n시작 포탑: {weapons}"; // 실제 캐릭터 능력과 후보
                Label(panel, "FrontendCharacterDetails", info, new Vector2(535f, 110f), new Vector2(700f, 400f), 25); // 실제 선택 캐릭터 정보
            }
            if (active != null && active.Portrait != null) // 실제 선택 캐릭터 초상화 확인
            { // 조건 시작
                Preview(panel, "FrontendCharacterPortrait", active.Portrait, new Vector2(535f, -165f), new Vector2(220f, 220f)); // 선택 캐릭터 초상화 위치 상향
            } // 조건 종료
            else // 초상화가 없는 기존 데이터 확인
            { // 대체 시작
                SnakeMovement movement = FindFirstObjectByType<SnakeMovement>(); // 실제 뱀 머리 조회
                SpriteRenderer head = movement == null ? null : movement.GetComponentInChildren<SpriteRenderer>(); // 실제 머리 스프라이트
                SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
                SpriteRenderer segment = body == null || body.BodySegments.Count == 0 ? null : body.BodySegments[0].GetComponent<SpriteRenderer>(); // 실제 몸통 스프라이트
                Sprite headImage = sceneContext == null ? head == null ? null : head.sprite : sceneContext.HeadSprite; // 실제 머리 미리보기
                Sprite bodyImage = sceneContext == null ? segment == null ? null : segment.sprite : sceneContext.BodySprite; // 실제 몸통 미리보기
                Preview(panel, "FrontendCharacterHead", headImage, new Vector2(365f, -210f), new Vector2(150f, 150f)); // 실제 머리 미리보기
                Preview(panel, "FrontendCharacterBody1", bodyImage, new Vector2(515f, -210f), new Vector2(150f, 150f)); // 실제 첫 몸통 미리보기
                Preview(panel, "FrontendCharacterBody2", bodyImage, new Vector2(665f, -210f), new Vector2(150f, 150f)); // 실제 둘째 몸통 미리보기
            } // 대체 종료
            CreateButton(panel, "FrontendCharactersBackButton", "뒤로", new Vector2(-520f, -350f), new Vector2(260f, 75f), BackFromCharacters); // 실제 주 메뉴 복귀
            CreateButton(panel, "FrontendCharactersNextButton", "다음: 맵", new Vector2(520f, -350f), new Vector2(360f, 75f), () => Show(FrontendStage.Maps)); // 실제 맵 선택 진입
        }
        private void BackFromCharacters() // 실제 캐릭터 선택 이전 화면
        {
            if (sceneContext != null && sceneContext.Kind == SceneFrontendKind.Preparation) // 실제 독립 준비 씬 확인
            {
                SceneManager.LoadScene(SceneFrontendContext.MainMenuPath); // 실제 메인 메뉴 씬 복귀
                return; // 기존 화면 전환 생략
            }
            Show(FrontendStage.Main); // 기존 단일 씬의 주 메뉴
        }
        private static string DescribePassive(CharacterData character) // 실제 캐릭터 패시브 설명
        {
            switch (character.Passive) // 실제 패시브 종류 확인
            {
                case CharacterPassive.BalancedRecovery: // 균형형 회복
                case CharacterPassive.MobileRecovery: return $"스태미나 회복 ×{character.PassiveMultiplier:0.00}"; // 실제 스태미나 회복
                case CharacterPassive.HeavyDefense: return $"받는 피해 ×{character.PassiveMultiplier:0.00}"; // 실제 받는 피해
                case CharacterPassive.FusionCandidates: return $"보유 포탑 후보 확률 ×{character.PreferredCandidateWeight:0.00}"; // 실제 합성 후보 가중치
                case CharacterPassive.HolySpecialization: return $"회복 ×{character.PassiveMultiplier:0.00} / 신성 피해 ×{character.SecondaryMultiplier:0.00} / 신성 후보 ×{character.PreferredCandidateWeight:0.00}"; // 실제 신성 특화
                case CharacterPassive.DarkSpecialization: return $"저주 처치 회복 ×{character.PassiveMultiplier:0.00} / 암흑 후보 ×{character.PreferredCandidateWeight:0.00}"; // 실제 암흑 특화
                default: return "-"; // 알 수 없는 패시브
            }
        }
        private void SelectCharacter(int index) // 실제 캐릭터 확정
        {
            if (sceneContext != null) // 실제 독립 준비 씬 확인
            {
                if (sceneContext.SelectCharacter(index)) // 실제 캐릭터 해금과 선택 확인
                {
                    Show(FrontendStage.Characters); // 실제 선택 정보 갱신
                }
                return; // 기존 게임 씬 적용 생략
            }
            CharacterData selected = characters == null ? null : characters.GetCharacter(index); // 실제 캐릭터 후보
            if (selected != null && characters.TryApply(selected)) // 실제 해금과 선택 확인
            {
                CharacterRunInitializer.SelectForNextRun(selected); // 실제 다음 런 캐릭터 기록
                Show(FrontendStage.Characters); // 실제 정보와 후보 갱신
            }
        }
        private void BuildMaps() // 실제 맵 선택 구성
        {
            Title("맵 선택", "잠긴 맵의 해금 조건을 확인하세요"); // 실제 맵 제목
            if (maps == null && sceneContext == null) // 실제 맵 선택 데이터 확인
            {
                Label(panel, "FrontendMapError", "맵 데이터를 찾을 수 없습니다", Vector2.zero, new Vector2(900f, 70f), 28); // 실제 구성 오류 표시
                return; // 잘못된 선택 차단
            }
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            int mapCount = sceneContext == null ? maps.ProfileCount : sceneContext.MapCount; // 실제 맵 후보 수
            int selectedMap = sceneContext == null ? maps.SelectedIndex : sceneContext.SelectedMap; // 실제 선택 맵 번호
            for (int index = 0; index < mapCount; index++) // 실제 세 맵 순회
            {
                int selected = index; // 실제 버튼별 맵 번호
                PrototypeMapProfile profile = sceneContext == null ? maps.GetProfile(index) : sceneContext.GetMap(index); // 실제 맵 데이터
                if (profile == null) // 실제 맵 누락 확인
                {
                    continue; // 잘못된 맵 생략
                }
                bool unlocked = meta == null || meta.IsMapUnlocked(index); // 실제 맵 해금 상태
                string locked = index == 1 ? "개방 평원 보스 처치" : "폐허 회랑 보스 처치"; // 실제 맵 해금 조건
                string label = profile.DisplayName + (unlocked ? index == selectedMap ? "  [선택됨]" : "" : "\n잠김: " + locked); // 실제 맵과 잠금 안내
                GameObject card = new GameObject("FrontendMapCard" + index, typeof(RectTransform), typeof(Image), typeof(Outline)); // 맵 카드 배경 생성
                card.transform.SetParent(panel, false); // 선택 화면에 카드 연결
                RectTransform cardRect = card.GetComponent<RectTransform>(); // 카드 위치 영역
                cardRect.anchoredPosition = new Vector2(-500f + index * 500f, 0f); // 맵별 카드 배치
                cardRect.sizeDelta = new Vector2(460f, 550f); // 카드 크기 설정
                Image cardImage = card.GetComponent<Image>(); // 카드 배경 표시기
                cardImage.color = new Color(0.025f, 0.09f, 0.14f, 0.92f); // 어두운 카드 배경
                ProjectUiSkin.ApplySidebar(cardImage, true); // 공통 맵 카드 프레임
                cardImage.raycastTarget = false; // 버튼 입력 보존
                Outline outline = card.GetComponent<Outline>(); // 카드 테두리
                outline.effectColor = index == selectedMap ? new Color(0.2f, 0.8f, 0.9f, 1f) : new Color(0.25f, 0.4f, 0.48f, 0.8f); // 선택 상태 구분
                outline.effectDistance = new Vector2(2f, -2f); // 테두리 두께
                Preview(card.transform, "FrontendMapPreview" + index, profile.SelectionPreview, new Vector2(0f, 90f), new Vector2(420f, 320f)); // 카드 위쪽 넓은 맵 이미지
                Button button = CreateButton(card.transform, "FrontendMapButton" + index, label, new Vector2(0f, -150f), new Vector2(400f, 90f), () => SelectMap(selected)); // 카드 아래쪽 맵 선택 버튼
                button.interactable = unlocked; // 실제 잠긴 맵 선택 차단
            }
            PrototypeMapProfile current = sceneContext == null ? maps.SelectedProfile : sceneContext.GetMap(selectedMap); // 실제 선택된 맵
            if (current != null) // 실제 맵 정보 확인
            {
                Label(panel, "FrontendMapDetails", $"{current.DisplayName}\n보스 체력 {current.BossHealth:0}   장애물 {current.ObstacleChance:P0}\n특수 지역 {current.SpecialZoneChance:P0}", new Vector2(0f, -330f), new Vector2(1100f, 75f), 22); // 카드 아래 맵과 위험 정보
            }
            CreateButton(panel, "FrontendMapsBackButton", "뒤로", new Vector2(-520f, -445f), new Vector2(260f, 75f), () => Show(FrontendStage.Characters)); // 실제 캐릭터 선택 복귀
            CreateButton(panel, "FrontendMapsNextButton", "출격", new Vector2(520f, -445f), new Vector2(360f, 75f), BeginSelectedMap); // 선택 맵에 출격
        }
        private void BeginSelectedMap() // 실제 선택 맵에 출격
        { // 함수 시작
            if (sceneContext != null) // 실제 분리 준비 씬 확인
            { // 조건 시작
                if (!sceneContext.BeginRun()) // 실제 선택 맵 게임 씬 이동 확인
                { // 조건 시작
                    Text error = GameObject.Find("FrontendMapStartError")?.GetComponent<Text>(); // 기존 출격 오류 안내
                    if (error == null) // 실제 안내 부재 확인
                    { // 조건 시작
                        Label(panel, "FrontendMapStartError", "캐릭터와 시작 포탑 설정을 확인하세요", new Vector2(0f, -390f), new Vector2(1000f, 45f), 22); // 실제 출격 오류 안내
                    } // 조건 종료
                } // 조건 종료
                return; // 기존 단일 씬 처리 생략
            } // 조건 종료
            Show(FrontendStage.Weapons); // 기존 단일 씬 시작 카드 표시
        } // 함수 종료
        private void SelectMap(int index) // 실제 맵 선택
        {
            if (sceneContext != null) // 실제 독립 준비 씬 확인
            {
                if (sceneContext.SelectMap(index)) // 실제 맵 해금과 선택 확인
                {
                    Show(FrontendStage.Maps); // 실제 선택 맵 표시 갱신
                }
                return; // 기존 게임 씬 적용 생략
            }
            if (maps != null && maps.SelectMap(index)) // 실제 해금과 선택 확인
            {
                startingWeapons?.RefreshStartingDisplay(); // 실제 기존 포탑 화면 갱신
                Show(FrontendStage.Maps); // 실제 맵 표시 갱신
            }
        }
        private void CreateStandaloneInputSettings() // 독립 메뉴의 기존 키 설정 화면
        {
            if (inputSettings != null) // 실제 기존 설정 화면 확인
            {
                return; // 중복 키 설정 방지
            }
            GameObject settingsRoot = new GameObject("StandaloneInputCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // 실제 별도 키 설정 캔버스
            settingsRoot.transform.SetParent(transform, false); // 실제 씬 화면에 연결
            Canvas settingsCanvas = settingsRoot.GetComponent<Canvas>(); // 실제 키 설정 캔버스
            settingsCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 실제 화면 위 표시
            settingsCanvas.sortingOrder = 40; // 실제 메뉴 위 표시
            CanvasScaler settingsScaler = settingsRoot.GetComponent<CanvasScaler>(); // 실제 해상도 조정기
            settingsScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 실제 화면 크기 대응
            settingsScaler.referenceResolution = new Vector2(1920f, 1080f); // 실제 기준 해상도
            GameObject background = new GameObject("StandaloneInputPanel", typeof(RectTransform), typeof(Image)); // 실제 키 설정 배경
            background.transform.SetParent(settingsRoot.transform, false); // 실제 캔버스 연결
            RectTransform rect = background.GetComponent<RectTransform>(); // 실제 키 설정 영역
            rect.anchorMin = Vector2.zero; // 실제 화면 왼쪽 아래
            rect.anchorMax = Vector2.one; // 실제 화면 오른쪽 위
            rect.offsetMin = Vector2.zero; // 실제 화면 여백 제거
            rect.offsetMax = Vector2.zero; // 실제 화면 여백 제거
            background.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.085f, 0.98f); // 실제 설정 배경색
            CreateContentFrame(background.transform, new Vector2(1500f, 940f)); // 실제 키 설정 공통 프레임
            Label(background.transform, "InputTitle", "키 설정", new Vector2(0f, 350f), new Vector2(900f, 90f), 48); // 실제 키 설정 제목
            Button left = CreateButton(background.transform, "InputTurnLeft", "왼쪽 회전", new Vector2(-300f, 170f), new Vector2(380f, 75f), null); // 실제 왼쪽 키 변경
            Button right = CreateButton(background.transform, "InputTurnRight", "오른쪽 회전", new Vector2(-300f, 70f), new Vector2(380f, 75f), null); // 실제 오른쪽 키 변경
            Button boost = CreateButton(background.transform, "InputBoost", "가속", new Vector2(-300f, -30f), new Vector2(380f, 75f), null); // 실제 가속 키 변경
            Text leftText = Label(background.transform, "InputTurnLeftValue", "-", new Vector2(240f, 170f), new Vector2(500f, 70f), 28); // 실제 왼쪽 키 표시
            Text rightText = Label(background.transform, "InputTurnRightValue", "-", new Vector2(240f, 70f), new Vector2(500f, 70f), 28); // 실제 오른쪽 키 표시
            Text boostText = Label(background.transform, "InputBoostValue", "-", new Vector2(240f, -30f), new Vector2(500f, 70f), 28); // 실제 가속 키 표시
            Text status = Label(background.transform, "InputStatus", "", new Vector2(0f, -135f), new Vector2(1000f, 60f), 26); // 실제 설정 결과 안내
            Button reset = CreateButton(background.transform, "InputReset", "기본값 복원", new Vector2(-220f, -245f), new Vector2(380f, 75f), null); // 실제 기본 키 복원
            Button close = CreateButton(background.transform, "InputClose", "닫기", new Vector2(220f, -245f), new Vector2(380f, 75f), null); // 실제 키 설정 닫기
            inputSettings = gameObject.AddComponent<SettingsMenuController>(); // 실제 기존 키 설정 제어기
            inputSettings.Bind(background, null, close, reset, left, right, boost, leftText, rightText, boostText, status); // 실제 기존 키 설정 연결
            background.SetActive(false); // 실제 키 설정 초기 숨김
        }
        private void BuildPause() // 실제 일시정지 화면 구성
        {
            Title("일시정지", "게임 시간이 멈췄습니다"); // 실제 일시정지 안내
            CreateButton(panel, "FrontendResumeButton", "계속하기", new Vector2(0f, 140f), new Vector2(460f, 86f), Resume); // 실제 게임 재개
            CreateButton(panel, "FrontendPauseSettingsButton", "설정", new Vector2(0f, 35f), new Vector2(460f, 86f), () => OpenSettings(FrontendStage.Paused)); // 실제 정지 중 설정
            CreateButton(panel, "FrontendAbandonButton", "전투 포기", new Vector2(0f, -70f), new Vector2(460f, 86f), () => GameManager.Instance?.AbandonRun()); // 실제 런 종료와 보상
            CreateButton(panel, "FrontendPauseMainButton", "메인 메뉴", new Vector2(0f, -175f), new Vector2(460f, 86f), ReturnToMain); // 실제 성과 저장과 주 메뉴
        }
        private void Resume() // 실제 수동 정지 해제
        {
            GameManager.Instance?.ResumeGame(); // 실제 게임 시간 재개
            FindFirstObjectByType<SnakeWeaponMergeController>()?.ClearManualPause(); // 실제 ESC 정지 상태 정리
            Show(FrontendStage.Playing); // 실제 전투 화면 복귀
        }
        private void OpenSettings(FrontendStage from) // 실제 설정 화면 진입
        {
            settingsReturn = from; // 실제 복귀 화면 저장
            Show(FrontendStage.Settings); // 실제 설정 화면 표시
        }
        private void BeginTutorial(bool fromSettings) // 첫 안내 또는 다시 보기 시작
        { // 함수 시작
            tutorialPage = 0; // 첫 안내 단계 설정
            tutorialReturn = fromSettings ? FrontendStage.Settings : FrontendStage.Playing; // 종료 후 복귀 화면
            tutorialOwnsPause = false; // 기존 정지 소유 초기화
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (!fromSettings && game != null && game.IsPlaying) // 자동 안내의 진행 상태 확인
            { // 조건 시작
                game.PauseGame(); // 안내 중 전투 일시정지
                tutorialOwnsPause = game.IsPaused; // 안내가 시작한 정지 기록
            } // 조건 종료
            Show(FrontendStage.Tutorial); // 실제 안내 화면 표시
        } // 함수 종료
        private static bool NeedsFirstTutorial() // 첫 안내 저장 상태 조회
        { // 함수 시작
            try // 저장 상태 읽기
            { // 조회 시작
                return PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 0; // 미완료 여부 반환
            } // 조회 종료
            catch (Exception error) // 저장 상태 오류 확인
            { // 실패 시작
                Debug.LogWarning("[Project Epsilon] 안내 상태를 읽지 못했습니다: " + error.Message); // 저장 오류 기록
                return false; // 저장 오류 시 전투 차단 방지
            } // 실패 종료
        } // 함수 종료
        private void BuildTutorial() // 현재 안내 단계 구성
        { // 함수 시작
            string left = InputBindingManager.Instance == null ? "A" : InputBindingManager.Instance.GetBindingDisplayName(RebindableInput.TurnLeft); // 현재 왼쪽 회전 키
            string right = InputBindingManager.Instance == null ? "D" : InputBindingManager.Instance.GetBindingDisplayName(RebindableInput.TurnRight); // 현재 오른쪽 회전 키
            string boost = InputBindingManager.Instance == null ? "스페이스" : InputBindingManager.Instance.GetBindingDisplayName(RebindableInput.Boost); // 현재 가속 키
            string[] titles = { "자동 전진과 회전", "자기 몸 피하기", "몸통과 공용 체력", "자동 공격", "몸통의 포탑", "포탑 합성", "속성 시너지", "가속" }; // 기획서 안내 단계
            string[] pages = // 실제 조작과 성장 안내
            { // 목록 시작
                $"뱀은 자동으로 앞으로 이동합니다.\n{left} 또는 왼쪽 방향키와 {right} 또는 오른쪽 방향키로 회전하세요.", // 이동과 회전 안내
                "머리가 자기 몸에 부딪히지 않도록 넓게 회전하세요.\n자기 충돌로 몸통을 잃을 수 있습니다.", // 자기 충돌 안내
                "몸통은 생명과 포탑 슬롯을 함께 담당합니다.\n공용 체력이 소진되면 몸통을 잃고, 몸통이 모두 사라지면 패배합니다.", // 공용 체력 안내
                "포탑은 자동으로 공격합니다.\n각 포탑은 장착된 몸통 위치에서 가장 가까운 적을 조준합니다.", // 자동 공격 안내
                "오른쪽 패널에서 머리 뒤 몸통 순서와 장착 포탑을 확인하세요.\n적을 처치해 경험치를 얻으면 몸통과 포탑을 성장시킬 수 있습니다.", // 몸통과 성장 안내
                "같은 포탑과 같은 등급이 두 개 있으면 E 키로 즉시 합성합니다.\n앞쪽 몸통부터 한 쌍이 합쳐지고 등급이 올라갑니다.", // 즉시 합성 안내
                "같은 속성의 포탑 수에 따라 시너지가 활성화됩니다.\n합성으로 포탑 수가 줄면 시너지 단계도 바로 바뀝니다.", // 속성 안내
                $"{boost} 키를 누르면 가속하며 스태미나가 줄어듭니다.\n20분을 생존한 뒤 최종 보스를 처치하면 승리합니다." // 가속과 승리 안내
            }; // 목록 종료
            int page = Mathf.Clamp(tutorialPage, 0, TutorialPageCount - 1); // 유효한 안내 단계
            Title(titles[page], $"첫 플레이 안내  {page + 1} / {TutorialPageCount}"); // 실제 안내 제목
            Label(panel, "FrontendTutorialBody", pages[page], new Vector2(0f, 70f), new Vector2(1450f, 330f), 38); // 실제 안내 내용
            Button previous = CreateButton(panel, "FrontendTutorialPreviousButton", "이전", new Vector2(-350f, -280f), new Vector2(300f, 80f), PreviousTutorialPage); // 이전 단계 버튼
            previous.interactable = page > 0; // 첫 단계 이전 이동 차단
            CreateButton(panel, "FrontendTutorialNextButton", page == TutorialPageCount - 1 ? "완료" : "다음", new Vector2(0f, -280f), new Vector2(300f, 80f), NextTutorialPage); // 다음 단계 또는 완료 버튼
            CreateButton(panel, "FrontendTutorialSkipButton", "건너뛰기", new Vector2(350f, -280f), new Vector2(300f, 80f), FinishTutorial); // 안내 건너뛰기 버튼
            Label(panel, "FrontendTutorialEscapeHelp", "ESC: 안내 닫기", new Vector2(0f, -390f), new Vector2(900f, 55f), 23); // 안내 취소 입력
        } // 함수 종료
        private void PreviousTutorialPage() // 이전 안내 단계
        { // 함수 시작
            tutorialPage = Mathf.Max(0, tutorialPage - 1); // 첫 단계 아래 이동 방지
            Show(FrontendStage.Tutorial); // 이전 안내 표시
        } // 함수 종료
        private void NextTutorialPage() // 다음 안내 단계
        { // 함수 시작
            if (tutorialPage >= TutorialPageCount - 1) // 마지막 단계 확인
            { // 조건 시작
                FinishTutorial(); // 안내 완료와 복귀
                return; // 추가 단계 이동 방지
            } // 조건 종료
            tutorialPage++; // 다음 안내 단계
            Show(FrontendStage.Tutorial); // 다음 안내 표시
        } // 함수 종료
        private void FinishTutorial() // 안내 완료와 이전 화면 복귀
        { // 함수 시작
            try // 안내 완료 저장
            { // 저장 시작
                PlayerPrefs.SetInt(TutorialCompletedKey, 1); // 완료 표식 저장
                PlayerPrefs.Save(); // 완료 표식 반영
            } // 저장 종료
            catch (Exception error) // 저장 실패 확인
            { // 실패 시작
                Debug.LogWarning("[Project Epsilon] 안내 완료를 저장하지 못했습니다: " + error.Message); // 저장 오류 기록
            } // 실패 종료
            GameManager game = GameManager.Instance; // 현재 게임 관리자
            if (tutorialOwnsPause && game != null && game.IsPaused) // 안내가 시작한 정지 확인
            { // 조건 시작
                game.ResumeGame(); // 안내가 정지한 전투만 재개
            } // 조건 종료
            tutorialOwnsPause = false; // 정지 소유 해제
            Show(tutorialReturn); // 기존 전투 또는 설정 복귀
        } // 함수 종료
        private void BuildSettings() // 실제 음량과 화면 설정 구성
        {
            Title("설정", "화면, 소리와 조작"); // 실제 설정 제목
            Label(panel, "FrontendOptionsStatus", options.IsAvailable ? $"음량 {options.Volume}%   /   화면 흔들림 {options.Shake}%\n{options.ResolutionLabel}   /   {(options.Fullscreen ? "전체 화면" : "창 모드")}" : "설정을 불러오지 못해 원본을 보존했습니다", new Vector2(0f, 210f), new Vector2(1200f, 120f), 30); // 실제 저장 설정 표시
            Button volume = CreateButton(panel, "FrontendVolumeButton", "음량 +10%", new Vector2(-300f, 75f), new Vector2(480f, 80f), () => ChangeOptions(options.Volume >= 100 ? -100 : 10, 0, 0, false)); // 실제 음량 순환
            Button shake = CreateButton(panel, "FrontendShakeButton", "화면 흔들림 +10%", new Vector2(300f, 75f), new Vector2(480f, 80f), () => ChangeOptions(0, options.Shake >= 100 ? -100 : 10, 0, false)); // 실제 흔들림 설정 순환
            Button resolution = CreateButton(panel, "FrontendResolutionButton", "해상도 변경", new Vector2(-300f, -35f), new Vector2(480f, 80f), () => ChangeOptions(0, 0, 1, false)); // 실제 해상도 순환
            Button mode = CreateButton(panel, "FrontendScreenModeButton", "화면 모드 변경", new Vector2(300f, -35f), new Vector2(480f, 80f), () => ChangeOptions(0, 0, 0, true)); // 실제 화면 모드 변경
            volume.interactable = shake.interactable = resolution.interactable = mode.interactable = options.IsAvailable; // 실제 저장 오류 시 변경 차단
            CreateButton(panel, "FrontendInputSettingsButton", "키 설정", new Vector2(0f, -150f), new Vector2(500f, 80f), () => inputSettings?.OpenSettings()); // 실제 기존 키 설정 열기
            CreateButton(panel, "FrontendTutorialReplayButton", "조작 안내 다시 보기", new Vector2(500f, -150f), new Vector2(420f, 80f), () => BeginTutorial(true)); // 첫 플레이 안내 다시 보기
            CreateButton(panel, "FrontendSettingsBackButton", "뒤로", new Vector2(0f, -280f), new Vector2(320f, 80f), () => Show(settingsReturn)); // 실제 이전 메뉴 복귀
            Label(panel, "FrontendShakeNote", "흔들림 설정은 저장되며 화면 효과는 아직 적용되지 않았습니다.", new Vector2(0f, -350f), new Vector2(1200f, 40f), 20); // 실제 흔들림 구현 상태
            Label(panel, "FrontendDebugHelp", "전투 중 시험 조작\nN: 합성 재료  |  E: 합성  |  P: 피해  |  [: 몸통 손실\nH: 회복 보석  |  J: 복구 보석  |  Shift+3 / Shift+5: 포탑 등급", new Vector2(0f, -445f), new Vector2(1500f, 105f), 22); // 설정 화면의 실제 시험 조작 안내
        }
        private void ChangeOptions(int volume, int shake, int resolution, bool fullscreen) // 실제 화면 설정 변경
        {
            options.Change(volume, shake, resolution, fullscreen); // 실제 저장과 설정 적용
            Show(FrontendStage.Settings); // 실제 설정값 다시 표시
        }
        private void BuildResults() // 실제 결과 화면 구성
        {
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 진행 관리자
            Title(game != null && game.CurrentState == GameState.Victory ? "승리" : "패배", "전투 결과"); // 실제 승패 제목
            string unlocks = meta == null || string.IsNullOrEmpty(meta.LastUnlockNotice) ? "없음" : meta.LastUnlockNotice; // 실제 새 해금 내역
            string save = meta != null && meta.HasPendingSave ? "저장 실패 - 다시 시도하세요" : "저장됨"; // 실제 저장 상태
            double seconds = meta == null ? 0d : meta.LastRunSeconds; // 실제 생존 시간
            string details = $"생존 시간  {(int)(seconds / 60d):00}:{(int)(seconds % 60d):00}\n처치  {(meta == null ? 0 : meta.Kills)}    정예  {(meta == null ? 0 : meta.Elites)}\n코어  +{(meta == null ? 0 : meta.LastAward)}\n해금  {unlocks}\n{save}"; // 실제 런 성과
            Label(panel, "FrontendResultDetails", details, new Vector2(0f, 100f), new Vector2(1100f, 380f), 34); // 실제 성과 표시
            CreateButton(panel, "FrontendRetryButton", "재도전", new Vector2(-260f, -300f), new Vector2(430f, 90f), Retry); // 실제 재도전
            CreateButton(panel, "FrontendResultMainButton", "메인 메뉴", new Vector2(260f, -300f), new Vector2(430f, 90f), ReturnToMain); // 실제 주 메뉴 복귀
        }
        private void Retry() // 실제 동일 캐릭터와 맵 재도전
        {
            if (sceneContext != null && sceneContext.Kind == SceneFrontendKind.Results) // 실제 분리 결과 씬 확인
            {
                SceneFrontendContext.PrepareRetry(); // 실제 포탑 재선택 예약
                if (GameManager.Instance == null || !GameManager.Instance.RestartToScene(SceneFrontendContext.GamePath)) // 실제 저장과 같은 맵 재도전 확인
                {
                    SceneFrontendContext.CancelPendingStart(); // 실패한 재도전 예약 취소
                    Show(FrontendStage.Results); // 실제 저장 실패 상태 유지
                }
                return; // 기존 단일 씬 재시작 생략
            }
            retryMap = maps == null ? 0 : maps.SelectedIndex; // 실제 선택 맵 보존
            retryNext = true; // 실제 포탑 선택 재진입 예약
            if (GameManager.Instance == null || !GameManager.Instance.RestartRun()) // 실제 저장과 재시작 확인
            {
                retryNext = false; // 실제 실패 예약 취소
                Show(FrontendStage.Results); // 실제 저장 실패 표시 갱신
            }
        }
        private void ReturnToMain() // 실제 보상 저장 후 주 메뉴 복귀
        {
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            if (game == null) // 실제 관리자 확인
            {
                return; // 실제 전환 차단
            }
            if (game.IsPlaying || game.IsPaused) // 실제 진행 중 런 확인
            {
                game.AbandonRun(); // 실제 포기 성과 저장
            }
            retryNext = false; // 실제 주 메뉴 진입 예약
            bool restarted = SceneFrontendContext.UsesSeparateScenes ? game.RestartToScene(SceneFrontendContext.MainMenuPath) : game.RestartRun(); // 실제 화면 방식별 저장과 복귀
            if (!restarted) // 실제 저장과 재시작 확인
            {
                Show(FrontendStage.Results); // 실제 실패 상태 표시
            }
        }
        private void Title(string title, string subtitle) // 실제 화면 제목 구성
        {
            Label(panel, "FrontendTitle", title, new Vector2(0f, 425f), new Vector2(1600f, 100f), 56); // 실제 화면 제목
            Label(panel, "FrontendSubtitle", subtitle, new Vector2(0f, 355f), new Vector2(1600f, 55f), 26); // 실제 화면 안내
        }
        private Text Label(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize) // 실제 글자 생성
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Text)); // 실제 글자 객체
            item.transform.SetParent(parent, false); // 실제 부모 화면 연결
            RectTransform rect = item.GetComponent<RectTransform>(); // 실제 글자 영역
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 화면 중앙 기준
            rect.anchoredPosition = position; // 실제 화면 위치
            rect.sizeDelta = size; // 실제 글자 크기
            Text text = item.GetComponent<Text>(); // 실제 글자 컴포넌트
            text.font = font; // 실제 화면 글꼴
            text.fontSize = fontSize; // 실제 글자 크기
            text.alignment = TextAnchor.MiddleCenter; // 실제 가운데 정렬
            text.color = Color.white; // 실제 글자 색상
            text.text = value; // 실제 표시 문구
            text.raycastTarget = false; // 실제 아래 버튼 입력 허용
            return text; // 실제 글자 반환
        }
        private void Preview(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size) // 실제 선택 대상 미리보기
        {
            if (sprite == null) // 실제 이미지 연결 확인
            {
                return; // 이미지 없는 미리보기 생략
            }
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image)); // 실제 미리보기 객체
            item.transform.SetParent(parent, false); // 실제 부모 화면 연결
            RectTransform rect = item.GetComponent<RectTransform>(); // 실제 이미지 영역
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 화면 중앙 기준
            rect.anchoredPosition = position; // 실제 미리보기 위치
            rect.sizeDelta = size; // 실제 미리보기 크기
            Image image = item.GetComponent<Image>(); // 실제 이미지 컴포넌트
            image.sprite = sprite; // 실제 게임 스프라이트 적용
            image.preserveAspect = true; // 실제 이미지 비율 유지
            image.raycastTarget = false; // 실제 버튼 입력 방해 방지
        }
        private Button CreateButton(Transform parent, string name, string value, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action) // 실제 버튼 생성
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); // 실제 버튼 객체
            item.transform.SetParent(parent, false); // 실제 부모 화면 연결
            RectTransform rect = item.GetComponent<RectTransform>(); // 실제 버튼 영역
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); // 실제 화면 중앙 기준
            rect.anchoredPosition = position; // 실제 버튼 위치
            rect.sizeDelta = size; // 실제 버튼 크기
            item.GetComponent<Image>().color = new Color(0.12f, 0.34f, 0.43f, 1f); // 실제 버튼 배경색
            Button button = item.GetComponent<Button>(); // 실제 버튼 컴포넌트
            ProjectUiSkin.ApplyButton(button); // 공통 버튼 프레임 적용
            if (action != null) // 실제 버튼 동작 확인
            {
                button.onClick.AddListener(action); // 실제 사용자 동작 연결
            }
            Label(item.transform, name + "Label", value, Vector2.zero, size - new Vector2(20f, 10f), 25); // 실제 버튼 문구
            return button; // 실제 버튼 반환
        }
    }
}
