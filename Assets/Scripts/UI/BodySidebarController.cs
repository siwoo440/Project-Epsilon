using ProjectEpsilon.Combat; // 실제 포탑과 속성 상태
using ProjectEpsilon.Data; // 실제 포탑 속성 이름
using ProjectEpsilon.Player; // 실제 몸통 순서
using UnityEngine; // 실제 화면과 카메라
using UnityEngine.UI; // 실제 패널과 이미지

namespace ProjectEpsilon.UI // 실제 전투 화면 영역
{
    [DefaultExecutionOrder(500)] // 실제 포탑과 속성 초기화 이후 실행
    public sealed class BodySidebarController : MonoBehaviour // 실제 몸통 순서와 포탑 정보 패널
    {
        private const float GameplayWidth = 0.76f; // 실제 왼쪽 전투 화면 비율
        private const int MaximumRows = 20; // 실제 절대 몸통 상한
        private readonly GameObject[] rows = new GameObject[MaximumRows]; // 실제 몸통 행 목록
        private readonly Image[] icons = new Image[MaximumRows]; // 실제 포탑 이미지 목록
        private readonly Text[] labels = new Text[MaximumRows]; // 실제 포탑 정보 목록
        private readonly Text[] attributes = new Text[WeaponAttributeSynergyRules.AttributeCount]; // 실제 속성 단계 목록
        private readonly Text[] stageLabels = new Text[4]; // 실제 속성 단계별 효과 문구
        private readonly Text[] reactionLabels = new Text[6]; // 실제 여섯 속성 조합 문구
        private readonly Text[] reactionOperators = new Text[12]; // 실제 조합 더하기와 등호 문구
        private readonly Image[] reactionIcons = new Image[12]; // 실제 조합 속성 아이콘
        private static readonly WeaponAttribute[] reactionLeft = { WeaponAttribute.Fire, WeaponAttribute.Cold, WeaponAttribute.Physical, WeaponAttribute.Holy, WeaponAttribute.Dark, WeaponAttribute.Holy }; // 실제 조합의 첫 속성
        private static readonly WeaponAttribute[] reactionRight = { WeaponAttribute.Poison, WeaponAttribute.Electric, WeaponAttribute.Explosion, WeaponAttribute.Fire, WeaponAttribute.Poison, WeaponAttribute.Dark }; // 실제 조합의 둘째 속성
        private static readonly Color inactiveColor = new Color(0.55f, 0.58f, 0.62f, 1f); // 미활성 정보 글자 색상
        private GameObject infoPanel; // 실제 상세 정보 영역
        private GameObject synergyDetail; // 실제 단일 속성 상세 영역
        private GameObject reactionPanel; // 실제 속성 조합 영역
        private Text infoTitle; // 실제 상세 화면 제목
        private Text infoStatus; // 실제 상세 화면 상태
        private int selectedView = -1; // 몸통 목록 또는 선택한 정보 화면
        private SnakeBodyManager body; // 실제 현재 몸통 관리자
        private SnakeWeaponManager weapons; // 실제 몸통별 포탑 관리자
        private WeaponAttributeSynergyManager synergy; // 실제 속성 집계 관리자
        private Camera gameCamera; // 실제 탑뷰 카메라
        private Rect previousCameraRect; // 실제 이전 카메라 화면 영역
        private bool changedCamera; // 실제 카메라 영역 변경 여부
        private Font font; // 실제 한글 표시 글꼴
        private bool ownsFont; // 실제 생성 글꼴 소유 여부
        private void Start() // 실제 전투 UI 분리와 구독
        {
            Canvas canvas = GetComponent<Canvas>(); // 실제 전투 HUD 캔버스
            body = FindFirstObjectByType<SnakeBodyManager>(); // 실제 현재 몸통 조회
            weapons = FindFirstObjectByType<SnakeWeaponManager>(); // 실제 포탑 슬롯 조회
            synergy = FindFirstObjectByType<WeaponAttributeSynergyManager>(); // 실제 속성 집계 조회
            gameCamera = Camera.main; // 실제 전투 탑뷰 카메라
            if (canvas == null || body == null || weapons == null || synergy == null || gameCamera == null) // 실제 필수 전투 구성 확인
            {
                enabled = false; // 구성 없는 테스트와 씬 변경 방지
                return; // 실제 패널 생성 생략
            }
            string[] fonts = // 실제 한글 우선 글꼴 후보
            {
                "Malgun Gothic", "Noto Sans CJK KR", "Arial" // 실제 운영체제 글꼴 이름
            };
            font = Font.CreateDynamicFontFromOSFont(fonts, 24); // 실제 한글 우선 글꼴
            ownsFont = font != null; // 실제 생성 글꼴 소유 기록
            if (font == null) // 실제 시스템 글꼴 확인
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Unity 기본 글꼴 대체
            }
            previousCameraRect = gameCamera.rect; // 실제 이전 카메라 영역 보관
            gameCamera.rect = new Rect(0f, 0f, GameplayWidth, 1f); // 실제 전투 카메라 화면 분리
            changedCamera = true; // 실제 카메라 변경 기록
            BuildLayout(canvas.transform); // 실제 전투 영역과 우측 패널 생성
            weapons.SlotsChanged += Refresh; // 실제 포탑 변경 구독
            synergy.SynergyChanged += Refresh; // 실제 속성 단계 변경 구독
            body.BodyCountChanged += OnBodyCountChanged; // 실제 몸통 변화 구독
            Refresh(); // 실제 초기 몸통과 장착 표시
        }
        private void OnDestroy() // 실제 화면과 이벤트 정리
        {
            if (weapons != null) // 실제 포탑 관리자 확인
            {
                weapons.SlotsChanged -= Refresh; // 실제 포탑 변경 구독 해제
            }
            if (synergy != null) // 실제 속성 관리자 확인
            {
                synergy.SynergyChanged -= Refresh; // 실제 속성 변경 구독 해제
            }
            if (body != null) // 실제 몸통 관리자 확인
            {
                body.BodyCountChanged -= OnBodyCountChanged; // 실제 몸통 변경 구독 해제
            }
            if (changedCamera && gameCamera != null) // 실제 변경된 전투 카메라 확인
            {
                gameCamera.rect = previousCameraRect; // 실제 이전 카메라 영역 복원
            }
            if (ownsFont && font != null) // 실제 생성 글꼴 확인
            {
                Destroy(font); // 실제 글꼴 자원 정리
            }
        }
        private void BuildLayout(Transform root) // 실제 전투 화면과 패널 구성
        {
            RectTransform gameplay = CreateRect(root, "GameplayViewport", Vector2.zero, new Vector2(GameplayWidth, 1f)); // 실제 왼쪽 전투 HUD 영역
            for (int index = root.childCount - 1; index >= 0; index--) // 실제 기존 HUD 자식 역순 확인
            {
                Transform child = root.GetChild(index); // 실제 기존 UI 객체
                if (child != gameplay) // 실제 새 영역 제외
                {
                    child.SetParent(gameplay, false); // 실제 기존 UI를 전투 영역에 연결
                }
            }
            LevelUpPanelController rewards = gameplay.GetComponentInChildren<LevelUpPanelController>(true); // 실제 비활성 보상 패널 조회
            rewards?.ConfigureGameplayViewport(); // 실제 전체 화면 기준 카드 크기 유지
            SettingsMenuController settings = GetComponent<SettingsMenuController>(); // 실제 기존 입력 설정 제어기
            settings?.RemoveHudOpenButton(); // 실제 중복 전투 설정 버튼 제거
            WeaponAttributeHUDPresenter oldAttributes = gameplay.GetComponentInChildren<WeaponAttributeHUDPresenter>(true); // 실제 기존 속성 목록
            if (oldAttributes != null) // 실제 중복 속성 목록 확인
            {
                oldAttributes.enabled = false; // 실제 기존 속성 갱신 중단
                Text oldLabel = oldAttributes.AttributeText; // 실제 기존 속성 글자
                if (oldLabel != null && oldLabel.gameObject != root.gameObject && oldLabel.gameObject != gameplay.gameObject) // 실제 독립 속성 객체 확인
                {
                    oldLabel.gameObject.SetActive(false); // 실제 기존 속성 목록 즉시 숨김
                    Destroy(oldLabel.gameObject); // 실제 중복 속성 객체 제거
                }
                else if (oldLabel != null) // 실제 공용 UI 객체 확인
                {
                    oldLabel.enabled = false; // 실제 공용 UI 보호와 글자 숨김
                }
            }
            RectTransform panel = CreateRect(root, "BodySidebar", new Vector2(GameplayWidth, 0f), Vector2.one); // 실제 오른쪽 정보 패널
            Image background = panel.gameObject.AddComponent<Image>(); // 실제 우측 배경 이미지
            background.color = new Color(0.025f, 0.06f, 0.09f, 0.97f); // 실제 어두운 패널 배경
            ProjectUiSkin.ApplySidebar(background, true); // 공통 우측 패널 프레임
            background.raycastTarget = false; // 실제 전투 입력 방해 방지
            CreateText(panel, "SidebarTitle", "속성 · 몸통 포탑", new Vector2(0.02f, 0.95f), new Vector2(0.72f, 0.995f), 27, TextAnchor.MiddleCenter); // 실제 패널 제목
            CreateButton(panel, "SidebarReactionsButton", "속성 조합", new Vector2(0.73f, 0.953f), new Vector2(0.98f, 0.992f), () => ShowInfo(8), true); // 실제 조합 정보 열기
            for (int index = 0; index < attributes.Length; index++) // 실제 여덟 속성 순회
            {
                int column = index % 2; // 실제 속성 열 번호
                int row = index / 2; // 실제 속성 행 번호
                float top = 0.944f - row * 0.036f; // 실제 속성 행 위쪽
                float left = column == 0 ? 0.04f : 0.51f; // 실제 속성 열 왼쪽
                float right = column == 0 ? 0.49f : 0.96f; // 실제 속성 열 오른쪽
                int selected = index; // 클릭별 속성 번호 보존
                CreateButton(panel, "SidebarAttributeButton_" + index, "", new Vector2(left, top - 0.034f), new Vector2(right, top), () => ShowInfo(selected), false); // 실제 속성 선택 영역
                RectTransform attributeIcon = CreateRect(panel, "SidebarAttributeIcon_" + index, new Vector2(left, top - 0.034f), new Vector2(left + 0.055f, top)); // 실제 속성 아이콘 영역
                Image attributeImage = attributeIcon.gameObject.AddComponent<Image>(); // 실제 속성 아이콘 표시
                attributeImage.sprite = ProjectUiSkin.AttributeIcon(index); // 속성 순서에 맞는 이미지
                attributeImage.enabled = attributeImage.sprite != null; // 누락 이미지 숨김
                attributeImage.preserveAspect = true; // 실제 아이콘 비율 유지
                attributeImage.raycastTarget = false; // 실제 전투 입력 보존
                attributes[index] = CreateText(panel, "SidebarAttribute_" + index, "", new Vector2(left + 0.06f, top - 0.034f), new Vector2(right, top), 19, TextAnchor.MiddleLeft); // 실제 속성 개수와 단계
            }
            for (int index = 0; index < MaximumRows; index++) // 실제 최대 몸통 슬롯 순회
            {
                float top = 0.79f - index * 0.0385f; // 실제 몸통 행 위쪽
                RectTransform row = CreateRect(panel, "SidebarRow_" + (index + 1), new Vector2(0.035f, top - 0.036f), new Vector2(0.965f, top)); // 실제 몸통 순서별 영역
                rows[index] = row.gameObject; // 실제 몸통 행 저장
                Image stripe = row.gameObject.AddComponent<Image>(); // 실제 몸통 행 배경
                stripe.color = index % 2 == 0 ? new Color(0.13f, 0.22f, 0.25f, 0.8f) : new Color(0.09f, 0.16f, 0.20f, 0.8f); // 실제 순서 구분 색상
                ProjectUiSkin.ApplySidebar(stripe, false); // 공통 몸통 행 프레임
                stripe.raycastTarget = false; // 실제 전투 입력 차단 방지
                CreateText(row, "SidebarNumber_" + (index + 1), (index + 1).ToString(), new Vector2(0f, 0f), new Vector2(0.1f, 1f), 20, TextAnchor.MiddleCenter); // 실제 머리 뒤 몸통 번호
                RectTransform iconRect = CreateRect(row, "SidebarWeapon_" + (index + 1), new Vector2(0.11f, 0.05f), new Vector2(0.24f, 0.95f)); // 실제 포탑 이미지 영역
                icons[index] = iconRect.gameObject.AddComponent<Image>(); // 실제 포탑 이미지
                icons[index].preserveAspect = true; // 실제 포탑 비율 보존
                icons[index].raycastTarget = false; // 실제 전투 입력 차단 방지
                labels[index] = CreateText(row, "SidebarLabel_" + (index + 1), "", new Vector2(0.26f, 0f), new Vector2(0.99f, 1f), 22, TextAnchor.MiddleCenter); // 실제 가운데 정렬된 포탑 이름
            }
            BuildInfoPanel(panel); // 실제 속성 상세와 조합 화면 구성
        }
        private Button CreateButton(Transform parent, string name, string caption, Vector2 minimum, Vector2 maximum, UnityEngine.Events.UnityAction action, bool decorated) // 실제 패널 버튼 생성
        { // 함수 시작
            RectTransform rect = CreateRect(parent, name, minimum, maximum); // 실제 버튼 영역
            Image image = rect.gameObject.AddComponent<Image>(); // 실제 마우스 입력 영역
            image.color = decorated ? new Color(0.12f, 0.24f, 0.29f, 1f) : new Color(1f, 1f, 1f, 0.01f); // 버튼 배경과 투명 선택 영역
            image.raycastTarget = true; // 실제 마우스 클릭 허용
            if (decorated) // 실제 메뉴 버튼 확인
            { // 조건 시작
                ProjectUiSkin.ApplySidebar(image, false); // 기존 공용 버튼 프레임
            } // 조건 종료
            Button button = rect.gameObject.AddComponent<Button>(); // 실제 클릭 버튼
            button.targetGraphic = image; // 실제 버튼 입력 배경
            button.onClick.AddListener(action); // 실제 선택 동작 연결
            if (!string.IsNullOrEmpty(caption)) // 실제 버튼 글자 확인
            { // 조건 시작
                CreateText(rect, name + "Label", caption, Vector2.zero, Vector2.one, 19, TextAnchor.MiddleCenter); // 실제 버튼 글자
            } // 조건 종료
            return button; // 실제 생성 버튼 반환
        } // 함수 종료
        private void BuildInfoPanel(Transform parent) // 실제 시너지와 조합 상세 구성
        { // 함수 시작
            RectTransform info = CreateRect(parent, "SidebarInfoPanel", new Vector2(0.025f, 0.015f), new Vector2(0.975f, 0.79f)); // 몸통 목록과 같은 정보 영역
            infoPanel = info.gameObject; // 실제 상세 화면 저장
            Image background = info.gameObject.AddComponent<Image>(); // 실제 상세 배경
            background.color = new Color(0.025f, 0.06f, 0.09f, 0.98f); // 실제 불투명 상세 배경
            background.raycastTarget = true; // 상세 화면 뒤쪽 입력 차단
            infoTitle = CreateText(info, "SidebarInfoTitle", "", new Vector2(0.02f, 0.92f), new Vector2(0.74f, 0.99f), 25, TextAnchor.MiddleCenter); // 실제 선택 정보 제목
            CreateButton(info, "SidebarInfoCloseButton", "몸통 목록", new Vector2(0.75f, 0.92f), new Vector2(0.99f, 0.99f), CloseInfo, true); // 실제 몸통 목록 복귀
            infoStatus = CreateText(info, "SidebarInfoStatus", "", new Vector2(0.025f, 0.82f), new Vector2(0.975f, 0.91f), 18, TextAnchor.MiddleCenter); // 실제 단계 또는 조합 조건 안내
            RectTransform details = CreateRect(info, "SidebarSynergyDetail", new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.81f)); // 실제 속성 단계 영역
            synergyDetail = details.gameObject; // 실제 속성 상세 저장
            for (int index = 0; index < stageLabels.Length; index++) // 실제 네 단계 순회
            { // 반복 시작
                float top = 1f - index * 0.25f; // 실제 단계별 위쪽 위치
                stageLabels[index] = CreateText(details, "SidebarSynergyStage_" + ((index + 1) * 2), "", new Vector2(0.02f, top - 0.24f), new Vector2(0.98f, top), 22, TextAnchor.MiddleLeft); // 실제 단계 효과와 상태
            } // 반복 종료
            RectTransform reactions = CreateRect(info, "SidebarReactionPanel", new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.81f)); // 실제 여섯 조합 영역
            reactionPanel = reactions.gameObject; // 실제 조합 패널 저장
            for (int index = 0; index < reactionLabels.Length; index++) // 실제 여섯 조합 순회
            { // 반복 시작
                float top = 1f - index / 6f; // 실제 조합 행 위쪽 위치
                RectTransform row = CreateRect(reactions, "SidebarReactionRow_" + index, new Vector2(0f, top - 1f / 6f), new Vector2(1f, top)); // 실제 속성 조합 한 줄
                reactionIcons[index * 2] = CreateReactionIcon(row, "SidebarReactionLeft_" + index, reactionLeft[index], 0.01f, 0.14f); // 실제 첫 속성 아이콘
                reactionOperators[index * 2] = CreateText(row, "SidebarReactionPlus_" + index, "+", new Vector2(0.14f, 0f), new Vector2(0.20f, 1f), 22, TextAnchor.MiddleCenter); // 실제 두 속성 사이 더하기
                reactionIcons[index * 2 + 1] = CreateReactionIcon(row, "SidebarReactionRight_" + index, reactionRight[index], 0.20f, 0.33f); // 실제 둘째 속성 아이콘
                reactionOperators[index * 2 + 1] = CreateText(row, "SidebarReactionEquals_" + index, "=", new Vector2(0.33f, 0f), new Vector2(0.39f, 1f), 22, TextAnchor.MiddleCenter); // 실제 조합 결과 등호
                reactionLabels[index] = CreateText(row, "SidebarReactionText_" + index, "", new Vector2(0.40f, 0f), new Vector2(0.99f, 1f), 20, TextAnchor.MiddleLeft); // 실제 조합 수치 설명
            } // 반복 종료
            infoPanel.SetActive(false); // 실제 몸통 목록 초기 표시
        } // 함수 종료
        private static Image CreateReactionIcon(Transform parent, string name, WeaponAttribute attribute, float left, float right) // 실제 조합 속성 이미지 생성
        { // 함수 시작
            RectTransform rect = CreateRect(parent, name, new Vector2(left, 0.12f), new Vector2(right, 0.88f)); // 실제 속성 이미지 영역
            Image image = rect.gameObject.AddComponent<Image>(); // 실제 속성 이미지
            image.sprite = ProjectUiSkin.AttributeIcon((int)attribute); // 기존 속성 아이콘 재사용
            image.enabled = image.sprite != null; // 누락 아이콘 숨김
            image.preserveAspect = true; // 기존 이미지 비율 유지
            image.raycastTarget = false; // 실제 패널 클릭 방해 방지
            return image; // 실제 속성 이미지 반환
        } // 함수 종료
        private void ShowInfo(int view) // 실제 선택한 속성 또는 조합 열기
        { // 함수 시작
            selectedView = view; // 실제 선택 화면 저장
            infoPanel.SetActive(true); // 실제 상세 영역 표시
            synergyDetail.SetActive(view >= 0 && view < WeaponAttributeSynergyRules.AttributeCount); // 실제 단일 속성 상세 표시
            reactionPanel.SetActive(view == WeaponAttributeSynergyRules.AttributeCount); // 실제 속성 조합 표시
            Refresh(); // 실제 포탑 수와 상세 정보 갱신
        } // 함수 종료
        private void CloseInfo() // 실제 몸통 목록 복귀
        { // 함수 시작
            selectedView = -1; // 실제 몸통 목록 선택
            infoPanel.SetActive(false); // 실제 상세 영역 숨김
            Refresh(); // 실제 몸통 목록 복구
        } // 함수 종료
        private static RectTransform CreateRect(Transform parent, string name, Vector2 minimum, Vector2 maximum) // 실제 정규화 영역 생성
        {
            GameObject item = new GameObject(name, typeof(RectTransform)); // 실제 UI 영역 객체
            RectTransform rect = (RectTransform)item.transform; // 실제 UI 위치
            rect.SetParent(parent, false); // 실제 부모 화면 연결
            rect.anchorMin = minimum; // 실제 왼쪽 아래 기준
            rect.anchorMax = maximum; // 실제 오른쪽 위 기준
            rect.offsetMin = Vector2.zero; // 실제 왼쪽 아래 여백 제거
            rect.offsetMax = Vector2.zero; // 실제 오른쪽 위 여백 제거
            return rect; // 실제 생성 영역 반환
        }
        private Text CreateText(Transform parent, string name, string value, Vector2 minimum, Vector2 maximum, int size, TextAnchor alignment) // 실제 한국어 UI 글자 생성
        {
            RectTransform rect = CreateRect(parent, name, minimum, maximum); // 실제 글자 영역
            Text text = rect.gameObject.AddComponent<Text>(); // 실제 UI 글자
            text.font = font; // 실제 한국어 글꼴 연결
            text.fontSize = size; // 실제 기본 글자 크기
            text.resizeTextForBestFit = true; // 실제 화면 크기별 글자 맞춤
            text.resizeTextMinSize = 9; // 실제 최소 글자 크기
            text.resizeTextMaxSize = size; // 실제 최대 글자 크기
            text.alignment = alignment; // 실제 글자 정렬
            text.color = Color.white; // 실제 글자 색상
            text.raycastTarget = false; // 실제 전투 입력 차단 방지
            text.text = value; // 실제 표시 문구
            return text; // 실제 글자 반환
        }
        private void OnBodyCountChanged(int current, int earned) // 실제 몸통 변화 알림
        {
            Refresh(); // 실제 현재 몸통 행 갱신
        }
        private void Refresh() // 실제 속성과 몸통별 포탑 갱신
        {
            if (body == null || weapons == null || synergy == null) // 실제 구성 요소 확인
            {
                return; // 실제 구성 이전 갱신 방지
            }
            for (int index = 0; index < attributes.Length; index++) // 실제 여덟 속성 순회
            {
                WeaponAttribute attribute = (WeaponAttribute)index; // 실제 속성 번호
                int count = synergy.GetCount(attribute); // 실제 장착 포탑 개수
                int stage = synergy.GetStage(attribute); // 실제 속성 단계
                string status = stage == 0 ? "미활성" : stage + "단계"; // 실제 활성 단계 안내
                attributes[index].text = WeaponDisplayNames.Attribute(attribute) + " " + count + "개 · " + status; // 실제 속성 상태 표시
                attributes[index].color = stage > 0 ? Color.white : inactiveColor; // 실제 활성과 미활성 글자 색상
            }
            int bodyCount = Mathf.Clamp(body.CurrentBodyCount, 0, MaximumRows); // 실제 현재 몸통 수
            for (int index = 0; index < MaximumRows; index++) // 실제 각 몸통 행 순회
            {
                bool visible = index < bodyCount; // 실제 존재하는 몸통 확인
                rows[index].SetActive(visible && selectedView < 0); // 실제 목록 화면의 현재 몸통만 표시
                SnakeWeaponSlot slot = visible && index < weapons.Slots.Count ? weapons.Slots[index] : null; // 실제 해당 몸통 슬롯
                WeaponData weapon = slot == null || slot.IsEmpty ? null : slot.Weapon; // 실제 장착 포탑
                icons[index].sprite = weapon == null ? null : weapon.Icon; // 실제 해당 포탑 아이콘
                icons[index].enabled = weapon != null && weapon.Icon != null; // 실제 아이콘 존재 여부
                icons[index].color = weapon == null ? Color.white : weapon.ArtworkDisplayTint; // 원본 포탑 그림 색상 유지
                labels[index].text = weapon == null ? "-" : WeaponDisplayNames.Attribute(weapon.Attribute) + " · " + weapon.DisplayName + " ★" + slot.Grade; // 실제 속성과 포탑 이름·등급
            }
            RefreshInfo(); // 실제 선택 상세의 효과와 조합 갱신
        }
        private void RefreshInfo() // 실제 선택 상세의 속성 상태 갱신
        { // 함수 시작
            if (selectedView < 0 || infoPanel == null) // 실제 상세 선택 상태 확인
            { // 조건 시작
                return; // 몸통 목록 표시 유지
            } // 조건 종료
            if (selectedView < WeaponAttributeSynergyRules.AttributeCount) // 실제 단일 속성 선택 확인
            { // 조건 시작
                WeaponAttribute attribute = (WeaponAttribute)selectedView; // 실제 선택 속성
                int count = synergy.GetCount(attribute); // 실제 장착 속성 수
                int stage = synergy.GetStage(attribute); // 실제 활성 시너지 단계
                infoTitle.text = WeaponDisplayNames.Attribute(attribute) + " 시너지"; // 실제 선택 속성 제목
                infoStatus.text = count + "개 장착 · " + (stage == 0 ? "미활성" : stage + "단계 활성"); // 실제 속성 수와 활성 상태
                infoStatus.color = stage > 0 ? Color.white : inactiveColor; // 실제 현재 단계 색상
                for (int index = 0; index < stageLabels.Length; index++) // 실제 네 단계 순회
                { // 반복 시작
                    int threshold = (index + 1) * 2; // 실제 단계 활성 조건
                    string effect = StageEffect(attribute, threshold); // 실제 구현된 단계별 효과
                    stageLabels[index].text = threshold + "개  " + (string.IsNullOrEmpty(effect) ? "추가 효과 미구현" : effect); // 실제 단계별 효과 설명
                    stageLabels[index].color = stage >= threshold && !string.IsNullOrEmpty(effect) ? Color.white : inactiveColor; // 실제 활성 효과만 흰색 표시
                } // 반복 종료
                return; // 조합 화면 갱신 생략
            } // 조건 종료
            infoTitle.text = "속성 조합"; // 실제 조합 화면 제목
            infoStatus.text = "두 속성 보유 기준 · 발동은 적 상태와 명중 순서에 따라 달라짐"; // 실제 조합 표시 기준
            infoStatus.color = Color.white; // 실제 조합 안내 글자
            for (int index = 0; index < reactionLabels.Length; index++) // 실제 여섯 조합 순회
            { // 반복 시작
                bool available = synergy.GetCount(reactionLeft[index]) > 0 && synergy.GetCount(reactionRight[index]) > 0 && WeaponAttributeReactionRules.ResolvePair(reactionLeft[index], reactionRight[index]) != WeaponAttributeReaction.None; // 실제 두 속성 보유와 구현된 조합 확인
                Color color = available ? Color.white : inactiveColor; // 실제 보유 조합과 미보유 조합 색상
                reactionLabels[index].text = ReactionEffect(index); // 실제 전투 설정의 조합 효과와 수치
                reactionLabels[index].color = color; // 실제 효과 설명 색상
                reactionOperators[index * 2].color = color; // 실제 더하기 색상
                reactionOperators[index * 2 + 1].color = color; // 실제 등호 색상
                reactionIcons[index * 2].color = color; // 실제 첫 속성 이미지 색상
                reactionIcons[index * 2 + 1].color = color; // 실제 둘째 속성 이미지 색상
            } // 반복 종료
        } // 함수 종료
        private string ReactionEffect(int index) // 실제 설정에 따른 속성 조합 효과 설명
        { // 함수 시작
            WeaponAttributeCombatEffects effects = weapons.AttributeCombatEffects; // 실제 포탑 전투 효과 설정
            if (effects == null) // 실제 전투 설정 확인
            { // 조건 시작
                return "효과 수치 확인 불가"; // 설정 누락 시 잘못된 수치 방지
            } // 조건 종료
            switch (index) // 실제 조합 순서 확인
            { // 분기 시작
                case 0: // 화염과 독 조합
                    return Number(effects.ReactionBurnDuration) + "초 동안\n초당 피해 " + Number(effects.ReactionBurnDamagePerSecond); // 실제 지속 피해와 시간
                case 1: // 냉기와 전기 조합
                    return "피해 " + Number(effects.ReactionDischargeDamage) + "\n" + Number(effects.ReactionDischargeSlowDuration) + "초 이동속도 -" + Number((1f - effects.ReactionDischargeSlowMultiplier) * 100f) + "%"; // 실제 피해와 감속 수치
                case 2: // 물리와 폭발 조합
                    return "반경 " + Number(effects.ReactionImpactRadius) + " 피해 " + Number(effects.ReactionImpactDamage) + "\n" + Number(effects.ReactionImpactKnockbackDuration) + "초 동안 " + Number(effects.ReactionImpactKnockbackDistance) + " 밀침"; // 실제 반경과 피해 및 밀치기 수치
                case 3: // 신성과 화염 조합
                    return "반경 " + Number(effects.ReactionHolyFireRadius) + "\n피해 " + Number(effects.ReactionHolyFireDamage); // 실제 범위 피해 수치
                case 4: // 암흑과 독 조합
                    return Number(effects.ReactionDarkPoisonDuration) + "초 주는 피해 -" + Number((1f - effects.ReactionDarkPoisonOutgoingMultiplier) * 100f) + "%\n받는 피해 +" + Number((effects.ReactionDarkPoisonIncomingMultiplier - 1f) * 100f) + "%"; // 실제 피해 약화와 증폭 수치
                case 5: // 신성과 암흑 조합
                    return "피해 " + Number(effects.ReactionHolyDarkDamage) + "\n공용 체력 " + effects.ReactionHolyDarkHealing + " 회복"; // 실제 피해와 체력 회복 수치
                default: // 잘못된 조합 순서
                    return ""; // 유효하지 않은 설명 생략
            } // 분기 종료
        } // 함수 종료
        private static string Number(float value) // 실제 설정 수치의 간결한 표시
        { // 함수 시작
            return value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture); // 소수 두 자리 이하 표시
        } // 함수 종료
        private static string StageEffect(WeaponAttribute attribute, int stage) // 실제 구현된 속성 시너지 설명
        { // 함수 시작
            switch (attribute) // 실제 선택 속성 확인
            { // 분기 시작
                case WeaponAttribute.Physical: // 실제 물리 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "직접 피해 +10%"; // 물리 피해 증가
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "치명타 확률 10% · 피해 1.75배"; // 물리 치명타 활성
                    } // 조건 종료
                    if (stage == 8) // 실제 단계 조건
                    { // 조건 시작
                        return "치명타 피해 2.5배"; // 물리 치명타 강화
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Fire: // 실제 화염 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "화상 피해 +20%"; // 화상 피해 증가
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "화상 지속 시간 +30%"; // 화상 지속 증가
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Cold: // 실제 냉기 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "냉기 감속 효과 +15%"; // 냉기 감속 강화
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "냉기 지속 시간 +25%"; // 냉기 지속 증가
                    } // 조건 종료
                    if (stage == 6) // 실제 단계 조건
                    { // 조건 시작
                        return "최대 냉기 누적 뒤 추가 명중 시 빙결"; // 냉기 빙결 조건
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Electric: // 실제 전기 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "연쇄 범위 +10% · 추가 대상 1명"; // 전기 연쇄 활성
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "연쇄 추가 대상 2명"; // 전기 연쇄 대상 증가
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Poison: // 실제 독 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "독 대상 공격 -5% · 받는 피해 +5%"; // 실제 독 약화 효과
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Explosion: // 실제 폭발 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "폭발 범위 +10%"; // 실제 폭발 범위 증가
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "폭발 밀치기 거리 +20%"; // 실제 밀치기 증가
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Holy: // 실제 신성 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "신성 직접 피해 +8%"; // 신성 직접 피해 증가
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "명중 시 15% 확률로 체력 1 회복"; // 신성 명중 회복
                    } // 조건 종료
                    if (stage == 6) // 실제 단계 조건
                    { // 조건 시작
                        return "5회 명중마다 기본 보호막 15 · 3초"; // 신성 보호막 기본값
                    } // 조건 종료
                    if (stage == 8) // 실제 단계 조건
                    { // 조건 시작
                        return "보호막 종료 시 회복과 주변 피해"; // 신성 보호막 종료 효과
                    } // 조건 종료
                    break; // 추가 효과 부재
                case WeaponAttribute.Dark: // 실제 암흑 시너지
                    if (stage == 2) // 실제 단계 조건
                    { // 조건 시작
                        return "저주 적용 · 피해의 4% 흡수"; // 암흑 저주와 흡수
                    } // 조건 종료
                    if (stage == 4) // 실제 단계 조건
                    { // 조건 시작
                        return "저주 중첩당 직접 피해 +6%"; // 암흑 저주 피해 증가
                    } // 조건 종료
                    if (stage == 6) // 실제 단계 조건
                    { // 조건 시작
                        return "저주 대상 처치 시 15% 확률로 체력 1 회복"; // 암흑 처치 회복
                    } // 조건 종료
                    if (stage == 8) // 실제 단계 조건
                    { // 조건 시작
                        return "피해 흡수 8% · 최대 저주 전파"; // 암흑 흡수와 저주 전파
                    } // 조건 종료
                    break; // 추가 효과 부재
            } // 분기 종료
            return ""; // 해당 단계의 별도 효과 부재
        } // 함수 종료
    }
}
