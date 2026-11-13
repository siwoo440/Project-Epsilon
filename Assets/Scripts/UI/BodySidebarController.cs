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
            background.raycastTarget = false; // 실제 전투 입력 방해 방지
            CreateText(panel, "SidebarTitle", "속성 · 몸통 포탑", new Vector2(0.04f, 0.95f), new Vector2(0.96f, 0.995f), 29, TextAnchor.MiddleCenter); // 실제 패널 제목
            for (int index = 0; index < attributes.Length; index++) // 실제 여덟 속성 순회
            {
                int column = index % 2; // 실제 속성 열 번호
                int row = index / 2; // 실제 속성 행 번호
                float top = 0.944f - row * 0.036f; // 실제 속성 행 위쪽
                float left = column == 0 ? 0.04f : 0.51f; // 실제 속성 열 왼쪽
                float right = column == 0 ? 0.49f : 0.96f; // 실제 속성 열 오른쪽
                attributes[index] = CreateText(panel, "SidebarAttribute_" + index, "", new Vector2(left, top - 0.034f), new Vector2(right, top), 19, TextAnchor.MiddleLeft); // 실제 속성 개수와 단계
            }
            for (int index = 0; index < MaximumRows; index++) // 실제 최대 몸통 슬롯 순회
            {
                float top = 0.79f - index * 0.0385f; // 실제 몸통 행 위쪽
                RectTransform row = CreateRect(panel, "SidebarRow_" + (index + 1), new Vector2(0.035f, top - 0.036f), new Vector2(0.965f, top)); // 실제 몸통 순서별 영역
                rows[index] = row.gameObject; // 실제 몸통 행 저장
                Image stripe = row.gameObject.AddComponent<Image>(); // 실제 몸통 행 배경
                stripe.color = index % 2 == 0 ? new Color(0.13f, 0.22f, 0.25f, 0.8f) : new Color(0.09f, 0.16f, 0.20f, 0.8f); // 실제 순서 구분 색상
                stripe.raycastTarget = false; // 실제 전투 입력 차단 방지
                CreateText(row, "SidebarNumber_" + (index + 1), (index + 1).ToString(), new Vector2(0f, 0f), new Vector2(0.1f, 1f), 20, TextAnchor.MiddleCenter); // 실제 머리 뒤 몸통 번호
                RectTransform iconRect = CreateRect(row, "SidebarWeapon_" + (index + 1), new Vector2(0.11f, 0.05f), new Vector2(0.24f, 0.95f)); // 실제 포탑 이미지 영역
                icons[index] = iconRect.gameObject.AddComponent<Image>(); // 실제 포탑 이미지
                icons[index].preserveAspect = true; // 실제 포탑 비율 보존
                icons[index].raycastTarget = false; // 실제 전투 입력 차단 방지
                labels[index] = CreateText(row, "SidebarLabel_" + (index + 1), "", new Vector2(0.26f, 0f), new Vector2(0.99f, 1f), 22, TextAnchor.MiddleCenter); // 실제 가운데 정렬된 포탑 이름
            }
        }
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
            }
            int bodyCount = Mathf.Clamp(body.CurrentBodyCount, 0, MaximumRows); // 실제 현재 몸통 수
            for (int index = 0; index < MaximumRows; index++) // 실제 각 몸통 행 순회
            {
                bool visible = index < bodyCount; // 실제 존재하는 몸통 확인
                rows[index].SetActive(visible); // 실제 현재 몸통만 표시
                SnakeWeaponSlot slot = visible && index < weapons.Slots.Count ? weapons.Slots[index] : null; // 실제 해당 몸통 슬롯
                WeaponData weapon = slot == null || slot.IsEmpty ? null : slot.Weapon; // 실제 장착 포탑
                icons[index].sprite = weapon == null ? null : weapon.Icon; // 실제 해당 포탑 아이콘
                icons[index].enabled = weapon != null && weapon.Icon != null; // 실제 아이콘 존재 여부
                icons[index].color = weapon == null ? Color.white : weapon.ArtworkDisplayTint; // 원본 포탑 그림 색상 유지
                labels[index].text = weapon == null ? "-" : WeaponDisplayNames.Attribute(weapon.Attribute) + " · " + weapon.DisplayName + " ★" + slot.Grade; // 실제 속성과 포탑 이름·등급
            }
        }
    }
}
