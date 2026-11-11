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

        private void Start() // 시작 기능 선언
        {
            ShowDebugDefaults(); // 표시 기능 실행
        }

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
            }
        }

        public void SetLevel(int level) // 설정·레벨 기능 선언
        {
            if (levelText != null) // 레벨·문구 조건 확인
            {
                levelText.text = HUDTextFormatter.FormatLevel(level); // 레벨·문구 값 설정
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
