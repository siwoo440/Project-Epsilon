using System; // 실제 시간과 동작
using ProjectEpsilon.Core; // 실제 게임 상태
using ProjectEpsilon.Data; // 실제 캐릭터 데이터
using ProjectEpsilon.Player; // 실제 캐릭터 선택
using ProjectEpsilon.Progression; // 실제 진행과 포탑 선택
using ProjectEpsilon.World; // 실제 맵 선택
using UnityEngine; // Unity 화면 기능
using UnityEngine.UI; // 실제 화면과 버튼

namespace ProjectEpsilon.UI // 실제 메뉴 영역
{
    public enum FrontendStage // 실제 화면 단계
    {
        Main, Characters, Maps, Weapons, Playing, Paused, Settings, Results // 실제 메뉴 흐름
    }
    [DefaultExecutionOrder(300)] // 기존 시작 데이터 이후 화면 구성
    public sealed class RunFrontendController : MonoBehaviour // 실제 메뉴와 런 흐름
    {
        private static bool retryNext; // 실제 재도전 진입 예약
        private static int retryMap; // 실제 재도전 맵
        private Canvas canvas; // 실제 화면 캔버스
        private CanvasGroup group; // 실제 입력 우선권
        private RectTransform panel; // 실제 메뉴 배경
        private GameObject backButton; // 실제 포탑 선택 뒤로 버튼
        private Font font; // 실제 기본 화면 글꼴
        private bool ownsFont; // 실제 생성 글꼴 소유 여부
        private SettingsMenuController inputSettings; // 기존 입력 설정 화면
        private CharacterRunInitializer characters; // 실제 캐릭터 선택기
        private PrototypeMapSelection maps; // 실제 맵 선택기
        private StartingWeaponSelection startingWeapons; // 실제 포탑 선택기
        private FrontendOptions options; // 실제 화면과 음량 설정
        private FrontendStage settingsReturn; // 설정 닫기 복귀 화면
        public static RunFrontendController Instance { get; private set; } // 실제 메뉴 관리자
        public FrontendStage Stage { get; private set; } // 실제 현재 단계
        public bool IsMainMenu => Stage == FrontendStage.Main; // 실제 주 메뉴 여부
        public bool AllowsStartingWeaponSelection => Stage == FrontendStage.Weapons; // 실제 포탑 선택 허용
        private void Awake() // 실제 관리자 등록
        {
            Instance = this; // 실제 현재 화면 등록
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
            backButton = CreateButton(canvasObject.transform, "FrontendWeaponBackButton", "BACK TO MAP", new Vector2(-790f, 460f), new Vector2(250f, 60f), () => Show(FrontendStage.Maps)).gameObject; // 실제 포탑 선택 뒤로
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
            GameManager game = GameManager.Instance; // 실제 게임 관리자
            if (game == null) // 실제 관리자 확인
            {
                return; // 추가 화면 처리 차단
            }
            if ((game.CurrentState == GameState.Victory || game.CurrentState == GameState.Defeated) && Stage != FrontendStage.Results) // 실제 종료 확인
            {
                Show(FrontendStage.Results); // 실제 결과 표시
            }
            else if (game.IsPlaying && Stage != FrontendStage.Playing && Stage != FrontendStage.Settings) // 실제 출격과 외부 재개 확인
            {
                Show(FrontendStage.Playing); // 실제 전투 화면 표시
            }
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
            bool showPanel = next != FrontendStage.Weapons && next != FrontendStage.Playing; // 실제 전체 화면 필요 여부
            panel.gameObject.SetActive(showPanel); // 실제 메뉴 배경 표시
            backButton.SetActive(next == FrontendStage.Weapons); // 실제 포탑 뒤로 버튼 표시
            if (!showPanel) // 실제 전투나 포탑 화면 확인
            {
                return; // 전체 메뉴 생성 생략
            }
            foreach (Transform child in panel) // 이전 화면 요소 순회
            {
                child.gameObject.SetActive(false); // 실제 이전 입력 즉시 차단
                Destroy(child.gameObject); // 실제 이전 화면 제거
            }
            switch (next) // 실제 화면 종류 확인
            {
                case FrontendStage.Main: BuildMain(); break; // 실제 주 메뉴 구성
                case FrontendStage.Characters: BuildCharacters(); break; // 실제 캐릭터 구성
                case FrontendStage.Maps: BuildMaps(); break; // 실제 맵 구성
                case FrontendStage.Paused: BuildPause(); break; // 실제 일시정지 구성
                case FrontendStage.Settings: BuildSettings(); break; // 실제 설정 구성
                case FrontendStage.Results: BuildResults(); break; // 실제 결과 구성
            }
        }
        private void BuildMain() // 실제 주 메뉴 구성
        {
            Texture2D background = Resources.Load<Texture2D>("MenuBackground_Casual"); // 실제 승인 화풍 메뉴 이미지
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
            Title("PROJECT EPSILON", "SURVIVE  /  GROW  /  EVOLVE"); // 실제 주 메뉴 제목
            CreateButton(panel, "FrontendStartButton", "START", new Vector2(0f, 140f), new Vector2(460f, 86f), () => Show(FrontendStage.Characters)); // 실제 캐릭터 선택 진입
            Button growth = CreateButton(panel, "FrontendGrowthButton", "PERMANENT GROWTH", new Vector2(0f, 35f), new Vector2(460f, 86f), () => MetaProgressionController.Instance?.Menu?.Open()); // 실제 성장 메뉴 진입
            growth.interactable = MetaProgressionController.Instance != null && MetaProgressionController.Instance.Service != null && MetaProgressionController.Instance.Service.FinishedFirstRun && MetaProgressionController.Instance.Service.IsAvailable; // 실제 첫 런 해금 확인
            CreateButton(panel, "FrontendSettingsButton", "SETTINGS", new Vector2(0f, -70f), new Vector2(460f, 86f), () => OpenSettings(FrontendStage.Main)); // 실제 설정 진입
            CreateButton(panel, "FrontendQuitButton", "QUIT", new Vector2(0f, -175f), new Vector2(460f, 86f), Application.Quit); // 실제 게임 종료
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 코어 관리자
            Label(panel, "FrontendCoreStatus", meta != null && meta.Service != null && meta.Service.IsAvailable ? $"CORES  {meta.Service.Cores}" : "META SAVE UNAVAILABLE", new Vector2(0f, -300f), new Vector2(800f, 55f), 26); // 실제 저장 상태 표시
        }
        private void BuildCharacters() // 실제 캐릭터 선택 구성
        {
            Title("SELECT CHARACTER", "Choose an unlocked character"); // 실제 캐릭터 제목
            if (characters == null) // 실제 선택기 확인
            {
                Label(panel, "FrontendCharacterError", "CHARACTER DATA UNAVAILABLE", Vector2.zero, new Vector2(900f, 70f), 28); // 실제 구성 오류 표시
                return; // 잘못된 선택 차단
            }
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            for (int index = 0; index < characters.CharacterCount; index++) // 실제 여섯 캐릭터 순회
            {
                int selected = index; // 실제 버튼별 캐릭터 번호
                CharacterData character = characters.GetCharacter(index); // 실제 캐릭터 후보
                if (character == null) // 실제 누락 후보 확인
                {
                    continue; // 잘못된 후보 생략
                }
                bool unlocked = meta == null || meta.IsCharacterUnlocked(character.Id); // 실제 해금 상태
                string label = character.DisplayName + (unlocked ? "" : "  |  LOCKED: CLEAR PLAINS"); // 실제 이름과 해금 조건
                Button button = CreateButton(panel, "FrontendCharacterButton" + index, label, new Vector2(-520f + index % 2 * 450f, 240f - index / 2 * 105f), new Vector2(420f, 82f), () => SelectCharacter(selected)); // 실제 캐릭터 선택 버튼
                button.interactable = unlocked; // 잠긴 캐릭터 선택 차단
            }
            CharacterData active = CharacterRunInitializer.ActiveCharacter; // 실제 선택 캐릭터
            if (active != null) // 실제 캐릭터 확인
            {
                string weapons = active.StartingWeapons == null || active.StartingWeapons.Weapons == null ? "-" : string.Join("  /  ", Array.ConvertAll(active.StartingWeapons.Weapons, item => item == null ? "-" : item.DisplayName)); // 실제 세 포탑 후보
                string info = $"{active.DisplayName}\nBODY {active.StartingBodyCount}   HP {active.BodyMaxHealth:0}\nMOVE x{active.MoveSpeedMultiplier:0.00}   TURN x{active.TurnPowerMultiplier:0.00}\nPASSIVE: {DescribePassive(active)}\nSTART: {weapons}"; // 실제 캐릭터 능력과 후보
                Label(panel, "FrontendCharacterDetails", info, new Vector2(535f, 110f), new Vector2(700f, 400f), 25); // 실제 선택 캐릭터 정보
            }
            SnakeMovement movement = FindFirstObjectByType<SnakeMovement>(); // 실제 뱀 머리 조회
            SpriteRenderer head = movement == null ? null : movement.GetComponentInChildren<SpriteRenderer>(); // 실제 머리 스프라이트
            SnakeBodyManager body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 몸통 관리자
            SpriteRenderer segment = body == null || body.BodySegments.Count == 0 ? null : body.BodySegments[0].GetComponent<SpriteRenderer>(); // 실제 몸통 스프라이트
            Preview(panel, "FrontendCharacterHead", head == null ? null : head.sprite, new Vector2(365f, -210f), new Vector2(150f, 150f)); // 실제 머리 미리보기
            Preview(panel, "FrontendCharacterBody1", segment == null ? null : segment.sprite, new Vector2(515f, -210f), new Vector2(150f, 150f)); // 실제 첫 몸통 미리보기
            Preview(panel, "FrontendCharacterBody2", segment == null ? null : segment.sprite, new Vector2(665f, -210f), new Vector2(150f, 150f)); // 실제 둘째 몸통 미리보기
            CreateButton(panel, "FrontendCharactersBackButton", "BACK", new Vector2(-520f, -350f), new Vector2(260f, 75f), () => Show(FrontendStage.Main)); // 실제 주 메뉴 복귀
            CreateButton(panel, "FrontendCharactersNextButton", "NEXT: MAP", new Vector2(520f, -350f), new Vector2(360f, 75f), () => Show(FrontendStage.Maps)); // 실제 맵 선택 진입
        }
        private static string DescribePassive(CharacterData character) // 실제 캐릭터 패시브 설명
        {
            switch (character.Passive) // 실제 패시브 종류 확인
            {
                case CharacterPassive.BalancedRecovery: // 균형형 회복
                case CharacterPassive.MobileRecovery: return $"STAMINA RECOVERY x{character.PassiveMultiplier:0.00}"; // 실제 스태미나 회복
                case CharacterPassive.HeavyDefense: return $"DAMAGE TAKEN x{character.PassiveMultiplier:0.00}"; // 실제 받는 피해
                case CharacterPassive.FusionCandidates: return $"OWNED TURRET CHANCE x{character.PreferredCandidateWeight:0.00}"; // 실제 합성 후보 가중치
                case CharacterPassive.HolySpecialization: return $"HEAL x{character.PassiveMultiplier:0.00} / HOLY DAMAGE x{character.SecondaryMultiplier:0.00} / HOLY CHANCE x{character.PreferredCandidateWeight:0.00}"; // 실제 신성 특화
                case CharacterPassive.DarkSpecialization: return $"CURSED KILL HEAL x{character.PassiveMultiplier:0.00} / DARK CHANCE x{character.PreferredCandidateWeight:0.00}"; // 실제 암흑 특화
                default: return "-"; // 알 수 없는 패시브
            }
        }
        private void SelectCharacter(int index) // 실제 캐릭터 확정
        {
            CharacterData selected = characters == null ? null : characters.GetCharacter(index); // 실제 캐릭터 후보
            if (selected != null && characters.TryApply(selected)) // 실제 해금과 선택 확인
            {
                CharacterRunInitializer.SelectForNextRun(selected); // 실제 다음 런 캐릭터 기록
                Show(FrontendStage.Characters); // 실제 정보와 후보 갱신
            }
        }
        private void BuildMaps() // 실제 맵 선택 구성
        {
            Title("SELECT MAP", "Locked maps show their unlock condition"); // 실제 맵 제목
            if (maps == null) // 실제 맵 선택기 확인
            {
                Label(panel, "FrontendMapError", "MAP DATA UNAVAILABLE", Vector2.zero, new Vector2(900f, 70f), 28); // 실제 구성 오류 표시
                return; // 잘못된 선택 차단
            }
            MetaProgressionController meta = MetaProgressionController.Instance; // 실제 해금 관리자
            for (int index = 0; index < maps.ProfileCount; index++) // 실제 세 맵 순회
            {
                int selected = index; // 실제 버튼별 맵 번호
                PrototypeMapProfile profile = maps.GetProfile(index); // 실제 맵 데이터
                if (profile == null) // 실제 맵 누락 확인
                {
                    continue; // 잘못된 맵 생략
                }
                bool unlocked = meta == null || meta.IsMapUnlocked(index); // 실제 맵 해금 상태
                string locked = index == 1 ? "CLEAR PLAINS" : "CLEAR RUINS"; // 실제 맵 해금 조건
                string label = profile.DisplayName + (unlocked ? index == maps.SelectedIndex ? "  [SELECTED]" : "" : "\nLOCKED: " + locked); // 실제 맵과 잠금 안내
                Button button = CreateButton(panel, "FrontendMapButton" + index, label, new Vector2(-500f + index * 500f, 160f), new Vector2(450f, 150f), () => SelectMap(selected)); // 실제 맵 선택 버튼
                button.interactable = unlocked; // 실제 잠긴 맵 선택 차단
                Preview(panel, "FrontendMapPreview" + index, profile.Ground, new Vector2(-500f + index * 500f, -220f), new Vector2(110f, 110f)); // 실제 맵 바닥 미리보기
            }
            PrototypeMapProfile current = maps.SelectedProfile; // 실제 선택된 맵
            if (current != null) // 실제 맵 정보 확인
            {
                Label(panel, "FrontendMapDetails", $"{current.DisplayName}\nBOSS HP {current.BossHealth:0}   OBSTACLES {current.ObstacleChance:P0}\nSPECIAL ZONES {current.SpecialZoneChance:P0}", new Vector2(0f, -60f), new Vector2(1100f, 180f), 28); // 실제 맵과 위험 정보
            }
            CreateButton(panel, "FrontendMapsBackButton", "BACK", new Vector2(-520f, -350f), new Vector2(260f, 75f), () => Show(FrontendStage.Characters)); // 실제 캐릭터 선택 복귀
            CreateButton(panel, "FrontendMapsNextButton", "NEXT: TURRET", new Vector2(520f, -350f), new Vector2(360f, 75f), () => Show(FrontendStage.Weapons)); // 실제 포탑 선택 진입
        }
        private void SelectMap(int index) // 실제 맵 선택
        {
            if (maps != null && maps.SelectMap(index)) // 실제 해금과 선택 확인
            {
                startingWeapons?.RefreshStartingDisplay(); // 실제 기존 포탑 화면 갱신
                Show(FrontendStage.Maps); // 실제 맵 표시 갱신
            }
        }
        private void BuildPause() // 실제 일시정지 화면 구성
        {
            Title("PAUSED", "Game time is stopped"); // 실제 일시정지 안내
            CreateButton(panel, "FrontendResumeButton", "RESUME", new Vector2(0f, 140f), new Vector2(460f, 86f), Resume); // 실제 게임 재개
            CreateButton(panel, "FrontendPauseSettingsButton", "SETTINGS", new Vector2(0f, 35f), new Vector2(460f, 86f), () => OpenSettings(FrontendStage.Paused)); // 실제 정지 중 설정
            CreateButton(panel, "FrontendAbandonButton", "ABANDON RUN", new Vector2(0f, -70f), new Vector2(460f, 86f), () => GameManager.Instance?.AbandonRun()); // 실제 런 종료와 보상
            CreateButton(panel, "FrontendPauseMainButton", "MAIN MENU", new Vector2(0f, -175f), new Vector2(460f, 86f), ReturnToMain); // 실제 성과 저장과 주 메뉴
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
        private void BuildSettings() // 실제 음량과 화면 설정 구성
        {
            Title("SETTINGS", "Display, audio and input"); // 실제 설정 제목
            Label(panel, "FrontendOptionsStatus", options.IsAvailable ? $"VOLUME {options.Volume}%   /   SHAKE {options.Shake}%\n{options.ResolutionLabel}   /   {(options.Fullscreen ? "FULLSCREEN" : "WINDOWED")}" : "SETTINGS SAVE ERROR: ORIGINAL PRESERVED", new Vector2(0f, 210f), new Vector2(1200f, 120f), 30); // 실제 저장 설정 표시
            Button volume = CreateButton(panel, "FrontendVolumeButton", "VOLUME +10%", new Vector2(-300f, 75f), new Vector2(480f, 80f), () => ChangeOptions(options.Volume >= 100 ? -100 : 10, 0, 0, false)); // 실제 음량 순환
            Button shake = CreateButton(panel, "FrontendShakeButton", "SHAKE +10%", new Vector2(300f, 75f), new Vector2(480f, 80f), () => ChangeOptions(0, options.Shake >= 100 ? -100 : 10, 0, false)); // 실제 흔들림 설정 순환
            Button resolution = CreateButton(panel, "FrontendResolutionButton", "NEXT RESOLUTION", new Vector2(-300f, -35f), new Vector2(480f, 80f), () => ChangeOptions(0, 0, 1, false)); // 실제 해상도 순환
            Button mode = CreateButton(panel, "FrontendScreenModeButton", "TOGGLE SCREEN MODE", new Vector2(300f, -35f), new Vector2(480f, 80f), () => ChangeOptions(0, 0, 0, true)); // 실제 화면 모드 변경
            volume.interactable = shake.interactable = resolution.interactable = mode.interactable = options.IsAvailable; // 실제 저장 오류 시 변경 차단
            CreateButton(panel, "FrontendInputSettingsButton", "INPUT BINDINGS", new Vector2(0f, -150f), new Vector2(500f, 80f), () => inputSettings?.OpenSettings()); // 실제 기존 키 설정 열기
            CreateButton(panel, "FrontendSettingsBackButton", "BACK", new Vector2(0f, -280f), new Vector2(320f, 80f), () => Show(settingsReturn)); // 실제 이전 메뉴 복귀
            Label(panel, "FrontendShakeNote", "Shake amount is saved; camera shake is not implemented yet.", new Vector2(0f, -390f), new Vector2(1200f, 50f), 21); // 실제 흔들림 구현 상태
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
            Title(game != null && game.CurrentState == GameState.Victory ? "VICTORY" : "GAME OVER", "RUN RESULTS"); // 실제 승패 제목
            string unlocks = meta == null || string.IsNullOrEmpty(meta.LastUnlockNotice) ? "NONE" : meta.LastUnlockNotice; // 실제 새 해금 내역
            string save = meta != null && meta.HasPendingSave ? "SAVE FAILED - RETRY REQUIRED" : "SAVED"; // 실제 저장 상태
            double seconds = meta == null ? 0d : meta.LastRunSeconds; // 실제 생존 시간
            string details = $"TIME  {(int)(seconds / 60d):00}:{(int)(seconds % 60d):00}\nKILLS  {(meta == null ? 0 : meta.Kills)}    ELITES  {(meta == null ? 0 : meta.Elites)}\nCORES  +{(meta == null ? 0 : meta.LastAward)}\nUNLOCKED  {unlocks}\n{save}"; // 실제 런 성과
            Label(panel, "FrontendResultDetails", details, new Vector2(0f, 100f), new Vector2(1100f, 380f), 34); // 실제 성과 표시
            CreateButton(panel, "FrontendRetryButton", "RETRY", new Vector2(-260f, -300f), new Vector2(430f, 90f), Retry); // 실제 재도전
            CreateButton(panel, "FrontendResultMainButton", "MAIN MENU", new Vector2(260f, -300f), new Vector2(430f, 90f), ReturnToMain); // 실제 주 메뉴 복귀
        }
        private void Retry() // 실제 동일 캐릭터와 맵 재도전
        {
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
            if (!game.RestartRun()) // 실제 저장과 재시작 확인
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
            button.onClick.AddListener(action); // 실제 사용자 동작 연결
            Label(item.transform, name + "Label", value, Vector2.zero, size - new Vector2(20f, 10f), 25); // 실제 버튼 문구
            return button; // 실제 버튼 반환
        }
    }
}
