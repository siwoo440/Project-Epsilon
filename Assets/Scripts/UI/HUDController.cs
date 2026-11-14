using UnityEngine; // Unity 엔진 기능 연결
using UnityEngine.UI; // Unity 엔진 기능 연결

namespace ProjectEpsilon.UI // 프로젝트 기능 영역
{
    public sealed class HUDController : MonoBehaviour // 행동 자료 구조
    {
        [SerializeField] private Text healthText; // 필드·문구 설정
        [SerializeField] private Text bodyCountText; // 필드·문구 설정
        [SerializeField] private Text experienceText; // 필드·문구 설정
        [SerializeField] private Text levelText; // 필드·문구 설정
        [SerializeField] private Text staminaText; // 필드·문구 설정
        private RectTransform healthFill; // 체력 막대 채움 영역
        private RectTransform experienceFill; // 경험치 막대 채움 영역
        private RectTransform staminaFill; // 스태미나 막대 채움 영역
        private Text sizedLevelText; // 글자 크기 기준 레벨 표시
        private Text sizedExperienceText; // 글자 크기 기준 경험치 표시
        private int baseLevelFontSize; // 원래 레벨 글자 크기
        private int baseExperienceFontSize; // 원래 경험치 글자 크기
        private GameObject worldWarning; // 실제 맵 위험 예고판
        private Text worldWarningText; // 실제 위험 이름
        private Image worldWarningFill; // 실제 위험 예고 게이지
        private static Sprite worldWarningSprite; // 공용 게이지 흰색 이미지

        public void SetWorldWarning(string message, float progress) // 맵 위험 예고와 게이지 갱신
        { // 함수 시작
            if (string.IsNullOrEmpty(message)) // 실제 예고 종료 확인
            { // 조건 시작
                if (worldWarning != null) // 기존 예고판 확인
                { // 조건 시작
                    worldWarning.SetActive(false); // 종료된 예고 숨김
                } // 조건 종료
                return; // 표시 갱신 종료
            } // 조건 종료
            if (worldWarning == null) // 실제 예고판 생성 확인
            { // 조건 시작
                if (healthText == null || healthText.canvas == null || healthText.font == null) // 기존 HUD 구성 확인
                { // 조건 시작
                    return; // HUD 준비 전 표시 생략
                } // 조건 종료
                worldWarning = new GameObject("WorldHazardWarning", typeof(RectTransform), typeof(Image)); // 실제 위험 예고판
                RectTransform frame = (RectTransform)worldWarning.transform; // 실제 예고판 영역
                frame.SetParent(healthText.canvas.transform, false); // 기존 전투 화면에 연결
                frame.anchorMin = frame.anchorMax = new Vector2(0.5f, 1f); // 화면 위 중앙 기준
                frame.pivot = new Vector2(0.5f, 1f); // 예고판 위쪽 기준
                frame.anchoredPosition = new Vector2(0f, -125f); // 타이머 아래 예고 위치
                frame.sizeDelta = new Vector2(300f, 58f); // 예고판 크기
                Image background = worldWarning.GetComponent<Image>(); // 실제 예고 배경
                background.color = new Color(0.08f, 0.06f, 0.04f, 0.9f); // 어두운 예고 배경
                background.raycastTarget = false; // 전투 입력 차단 방지
                GameObject label = new GameObject("WarningText", typeof(RectTransform), typeof(Text)); // 실제 예고 문구
                label.transform.SetParent(frame, false); // 예고판에 문구 연결
                RectTransform labelRect = (RectTransform)label.transform; // 실제 문구 영역
                labelRect.anchorMin = new Vector2(0f, 0.3f); // 문구 아래쪽 기준
                labelRect.anchorMax = Vector2.one; // 문구 위쪽 기준
                labelRect.offsetMin = Vector2.zero; // 문구 아래 여백
                labelRect.offsetMax = Vector2.zero; // 문구 위 여백
                worldWarningText = label.GetComponent<Text>(); // 실제 예고 문구
                worldWarningText.font = healthText.font; // 기존 한국어 글꼴 재사용
                worldWarningText.fontSize = 22; // 실제 예고 글자 크기
                worldWarningText.resizeTextForBestFit = true; // 긴 방향 문구 자동 크기 조정
                worldWarningText.resizeTextMinSize = 14; // 경고 문구 최소 글자 크기
                worldWarningText.resizeTextMaxSize = 22; // 경고 문구 최대 글자 크기
                worldWarningText.alignment = TextAnchor.MiddleCenter; // 예고 문구 가운데 정렬
                worldWarningText.color = Color.white; // 예고 문구 색상
                worldWarningText.raycastTarget = false; // 전투 입력 차단 방지
                GameObject fill = new GameObject("WarningGauge", typeof(RectTransform), typeof(Image)); // 실제 예고 게이지
                fill.transform.SetParent(frame, false); // 예고판에 게이지 연결
                RectTransform fillRect = (RectTransform)fill.transform; // 실제 게이지 영역
                fillRect.anchorMin = new Vector2(0f, 0f); // 게이지 왼쪽 아래 기준
                fillRect.anchorMax = new Vector2(1f, 0.25f); // 게이지 가로 크기 기준
                fillRect.offsetMin = new Vector2(4f, 4f); // 게이지 안쪽 여백
                fillRect.offsetMax = new Vector2(-4f, 0f); // 게이지 오른쪽 여백
                worldWarningFill = fill.GetComponent<Image>(); // 실제 게이지 이미지
                if (worldWarningSprite == null) // 공용 게이지 이미지 확인
                { // 조건 시작
                    worldWarningSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width); // 실제 채우기용 흰색 이미지
                } // 조건 종료
                worldWarningFill.sprite = worldWarningSprite; // 실제 게이지 이미지 연결
                worldWarningFill.type = Image.Type.Filled; // 채워지는 막대 설정
                worldWarningFill.fillMethod = Image.FillMethod.Horizontal; // 왼쪽에서 오른쪽 충전
                worldWarningFill.color = new Color(1f, 0.55f, 0.1f); // 위험 게이지 색상
                worldWarningFill.raycastTarget = false; // 전투 입력 차단 방지
            } // 조건 종료
            worldWarning.SetActive(true); // 실제 예고 표시
            worldWarningText.text = message; // 현재 위험 이름 표시
            worldWarningFill.fillAmount = Mathf.Clamp01(progress); // 실제 예고 진행도 표시
        } // 함수 종료

        private void Start() // 시작 기능 선언
        {
            HideLegacyControlHints(); // 기존 화면의 시험 조작 안내 숨김
            ShowDebugDefaults(); // 표시 기능 실행
        }

        private void HideLegacyControlHints() // 기존 HUD 시험 조작 안내 제거
        { // 영역 시작
            foreach (Transform hint in GetComponentsInChildren<Transform>(true)) // 전투 영역을 포함한 자식 검색
            { // 영역 시작
                if (hint.name == "MergeHintText" || hint.name == "PrototypeHintText") // 기존 안내 이름 확인
                { // 영역 시작
                    hint.gameObject.SetActive(false); // 기존 화면의 안내 표시 제거
                } // 영역 종료
            } // 영역 종료
        } // 영역 종료

        public void Bind( // 연결 호출 시작
            Text health, // 문구·체력 항목 전달
            Text bodyCount, // 문구·몸통 항목 전달
            Text experience, // 문구·경험치 항목 전달
            Text level, // 문구·레벨 항목 전달
            Text stamina // 문구·스태미나 처리
        ) // 앞선 항목 종료
        {
            healthText = health; // 체력·문구 값 설정
            bodyCountText = bodyCount; // 몸통·개수 값 설정
            experienceText = experience; // 경험치·문구 값 설정
            levelText = level; // 레벨·문구 값 설정
            staminaText = stamina; // 스태미나·문구 값 설정

            ShowDebugDefaults(); // 표시 기능 실행
        }

        public void SetHealth(int current, int maximum) // 설정·체력 기능 선언
        {
            SetBar(healthText, ref healthFill, "HealthBar", current, maximum, new Color(0.85f, 0.16f, 0.16f, 1f)); // 실제 체력 막대 갱신
            if (healthText != null) // 체력·문구 조건 확인
            {
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum); // 체력·문구 값 설정
            }
        }

        public void SetHealth(int current, int maximum, int shield) // 보호막 포함 체력 표시
        {
            SetBar(healthText, ref healthFill, "HealthBar", current, maximum, new Color(0.85f, 0.16f, 0.16f, 1f)); // 실제 체력 막대 갱신
            if (healthText != null) // 체력 Text 확인
            {
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum, shield); // 체력과 보호막 갱신
            }
        }

        public void SetBodyCount(int current, int maximum) // 설정·몸통 기능 선언
        {
            if (bodyCountText != null) // 몸통·개수 조건 확인
            {
                bodyCountText.text = HUDTextFormatter.FormatBodyCount(current, maximum); // 몸통·개수 값 설정
            }
        }

        public void SetBodyCount(int current, int earned, int maximum) // 몸통 현재 수와 두 상한 갱신
        {
            if (bodyCountText != null) // 몸통 텍스트 연결 확인
            {
                bodyCountText.text = HUDTextFormatter.FormatBodyCount(current, earned, maximum); // 몸통 상태 표시
            }
        }

        public void SetExperience(int current, int required) // 설정·경험치 기능 선언
        {
            SetBar(experienceText, ref experienceFill, "ExperienceBar", current, required, new Color(0.2f, 0.65f, 1f, 1f)); // 실제 경험치 막대 갱신
            if (experienceText != null) // 경험치·문구 조건 확인
            {
                experienceText.text = HUDTextFormatter.FormatExperience(current, required); // 경험치·문구 값 설정
                FitLevelExperienceLabels(); // 겹치는 레벨과 경험치 글자 조정
            }
        }

        public void SetLevel(int level) // 설정·레벨 기능 선언
        {
            if (levelText != null) // 레벨·문구 조건 확인
            {
                levelText.text = HUDTextFormatter.FormatLevel(level); // 레벨·문구 값 설정
                FitLevelExperienceLabels(); // 겹치는 레벨과 경험치 글자 조정
            }
        }

        public void SetStamina(int current, int maximum) // 설정·스태미나 기능 선언
        {
            SetBar(staminaText, ref staminaFill, "StaminaBar", current, maximum, new Color(0.25f, 0.85f, 0.35f, 1f)); // 실제 스태미나 막대 갱신
            if (staminaText != null) // 스태미나·문구 조건 확인
            {
                staminaText.text = HUDTextFormatter.FormatStamina(current, maximum); // 스태미나·문구 값 설정
            }
        }

        private void FitLevelExperienceLabels() // 공통 영역의 글자 겹침 방지
        { // 함수 시작
            if (levelText == null || experienceText == null || levelText.font == null || experienceText.font == null) // 글자와 글꼴 확인
            { // 조건 시작
                return; // 표시 준비 전 조정 방지
            } // 조건 종료
            if (sizedLevelText != levelText || sizedExperienceText != experienceText) // 새 글자 연결 확인
            { // 조건 시작
                if (sizedLevelText != null) // 이전 레벨 글자 확인
                { // 조건 시작
                    sizedLevelText.fontSize = baseLevelFontSize; // 이전 글자 크기 복원
                } // 조건 종료
                if (sizedExperienceText != null) // 이전 경험치 글자 확인
                { // 조건 시작
                    sizedExperienceText.fontSize = baseExperienceFontSize; // 이전 글자 크기 복원
                } // 조건 종료
                sizedLevelText = levelText; // 현재 레벨 글자 저장
                sizedExperienceText = experienceText; // 현재 경험치 글자 저장
                baseLevelFontSize = levelText.fontSize; // 원래 레벨 크기 저장
                baseExperienceFontSize = experienceText.fontSize; // 원래 경험치 크기 저장
            } // 조건 종료
            RectTransform levelRect = levelText.rectTransform; // 레벨 표시 영역
            RectTransform experienceRect = experienceText.rectTransform; // 경험치 표시 영역
            if (levelRect.parent != experienceRect.parent || levelRect.anchorMin != experienceRect.anchorMin || levelRect.anchorMax != experienceRect.anchorMax || levelRect.anchoredPosition != experienceRect.anchoredPosition || levelRect.sizeDelta != experienceRect.sizeDelta) // 같은 표시 영역 확인
            { // 조건 시작
                return; // 별도 영역의 글자 크기 유지
            } // 조건 종료
            levelText.fontSize = baseLevelFontSize; // 기본 레벨 글자 크기 복원
            experienceText.fontSize = baseExperienceFontSize; // 기본 경험치 글자 크기 복원
            float available = Mathf.Min(levelRect.rect.width, experienceRect.rect.width); // 실제 공통 영역 너비
            if (available <= 0f) // 유효한 표시 너비 확인
            { // 조건 시작
                return; // 계산 전 크기 조정 방지
            } // 조건 종료
            int minimumExperience = Mathf.Min(14, baseExperienceFontSize); // 경험치 최소 글자 크기
            int minimumLevel = Mathf.Min(14, baseLevelFontSize); // 레벨 최소 글자 크기
            while (levelText.preferredWidth + experienceText.preferredWidth + 8f > available) // 글자와 여백이 겹치는 동안 반복
            { // 반복 시작
                if (experienceText.fontSize > minimumExperience) // 경험치 크기 조정 가능 확인
                { // 조건 시작
                    experienceText.fontSize--; // 경험치 글자 축소
                } // 조건 종료
                else if (levelText.fontSize > minimumLevel) // 레벨 크기 조정 가능 확인
                { // 조건 시작
                    levelText.fontSize--; // 레벨 글자 축소
                } // 조건 종료
                else // 최소 글자 크기 도달
                { // 조건 시작
                    break; // 추가 축소 방지
                } // 조건 종료
            } // 반복 종료
        } // 함수 종료

        private static void SetBar(Text label, ref RectTransform fill, string name, int current, int maximum, Color color) // 실제 HUD 막대 구성과 비율 갱신
        {
            if (!Application.isPlaying || label == null) // 실제 실행과 글자 연결 확인
            {
                return; // 편집 모드와 미설정 HUD 무시
            }
            if (fill != null && (fill.parent == null || fill.parent.parent != label.transform)) // 이전 글자에 연결된 막대 확인
            {
                if (fill.parent != null) // 실제 이전 막대 확인
                {
                    Destroy(fill.parent.gameObject); // 이전 막대 제거
                }
                fill = null; // 이전 채움 참조 초기화
            }
            if (fill == null) // 실제 막대 생성 여부 확인
            {
                GameObject background = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // 실제 막대 배경 생성
                RectTransform frame = (RectTransform)background.transform; // 실제 막대 배경 영역
                frame.SetParent(label.transform, false); // 실제 글자 아래 연결
                frame.anchorMin = Vector2.zero; // 배경 왼쪽 아래 기준
                frame.anchorMax = new Vector2(1f, 0f); // 배경 가로 늘림 기준
                frame.pivot = Vector2.zero; // 배경 왼쪽 아래 정렬
                frame.anchoredPosition = Vector2.zero; // 글자 아래쪽 위치
                frame.sizeDelta = new Vector2(0f, 10f); // 실제 막대 높이
                Image backgroundImage = background.GetComponent<Image>(); // 실제 배경 이미지
                backgroundImage.color = new Color(0.05f, 0.06f, 0.09f, 0.85f); // 어두운 막대 배경
                ProjectUiSkin.ApplyBar(backgroundImage); // 공통 체력과 경험치 막대 프레임
                backgroundImage.raycastTarget = false; // UI 입력 차단 방지
                GameObject foreground = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // 실제 채움 이미지 생성
                fill = (RectTransform)foreground.transform; // 실제 채움 영역 저장
                fill.SetParent(frame, false); // 실제 배경 아래 연결
                fill.anchorMin = Vector2.zero; // 채움 왼쪽 아래 기준
                fill.anchorMax = Vector2.one; // 채움 높이 기준
                fill.offsetMin = new Vector2(1f, 1f); // 배경 안쪽 여백
                fill.offsetMax = new Vector2(-1f, -1f); // 배경 안쪽 여백
                Image fillImage = foreground.GetComponent<Image>(); // 실제 채움 이미지
                fillImage.color = color; // 체력 또는 경험치 색상
                fillImage.raycastTarget = false; // UI 입력 차단 방지
            }
            float ratio = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f; // 안전한 현재 비율
            fill.anchorMax = new Vector2(ratio, 1f); // 실제 채움 너비 갱신
            fill.gameObject.SetActive(ratio > 0f); // 빈 막대 채움 숨김
        }

        private void ShowDebugDefaults() // 표시 기능 선언
        {
            SetHealth(100, 100); // 설정·체력 기능 실행
            SetBodyCount(3, 20); // 설정·몸통 기능 실행
            SetExperience(0, 10); // 설정·경험치 기능 실행
            SetLevel(1); // 설정·레벨 기능 실행
            SetStamina(100, 100); // 설정·스태미나 기능 실행
        }
    }
}
