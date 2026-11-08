using UnityEngine;
using UnityEngine.UI;

namespace ProjectEpsilon.UI
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text bodyCountText;
        [SerializeField] private Text experienceText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text staminaText;
        private RectTransform healthFill; // 체력 막대 채움 영역
        private RectTransform experienceFill; // 경험치 막대 채움 영역
        private RectTransform staminaFill; // 스태미나 막대 채움 영역

        private void Start()
        {
            ShowDebugDefaults();
        }

        public void Bind(
            Text health,
            Text bodyCount,
            Text experience,
            Text level,
            Text stamina
        )
        {
            healthText = health;
            bodyCountText = bodyCount;
            experienceText = experience;
            levelText = level;
            staminaText = stamina;

            ShowDebugDefaults();
        }

        public void SetHealth(int current, int maximum)
        {
            SetBar(healthText, ref healthFill, "HealthBar", current, maximum, new Color(0.85f, 0.16f, 0.16f, 1f)); // 실제 체력 막대 갱신
            if (healthText != null)
            {
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum);
            }
        }

        public void SetHealth(int current, int maximum, int shield) // 보호막 포함 체력 표시
        { // 메서드 시작
            SetBar(healthText, ref healthFill, "HealthBar", current, maximum, new Color(0.85f, 0.16f, 0.16f, 1f)); // 실제 체력 막대 갱신
            if (healthText != null) // 체력 Text 확인
            { // 조건 시작
                healthText.text = HUDTextFormatter.FormatHealth(current, maximum, shield); // 체력과 보호막 갱신
            } // 조건 끝
        } // 메서드 끝

        public void SetBodyCount(int current, int maximum)
        {
            if (bodyCountText != null)
            {
                bodyCountText.text = HUDTextFormatter.FormatBodyCount(current, maximum);
            }
        }

        public void SetBodyCount(int current, int earned, int maximum) // 몸통 현재 수와 두 상한 갱신
        { // 메서드 시작
            if (bodyCountText != null) // 몸통 텍스트 연결 확인
            { // 조건 시작
                bodyCountText.text = HUDTextFormatter.FormatBodyCount(current, earned, maximum); // 몸통 상태 표시
            } // 조건 끝
        } // 메서드 끝

        public void SetExperience(int current, int required)
        {
            SetBar(experienceText, ref experienceFill, "ExperienceBar", current, required, new Color(0.2f, 0.65f, 1f, 1f)); // 실제 경험치 막대 갱신
            if (experienceText != null)
            {
                experienceText.text = HUDTextFormatter.FormatExperience(current, required);
            }
        }

        public void SetLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = HUDTextFormatter.FormatLevel(level);
            }
        }

        public void SetStamina(int current, int maximum)
        {
            SetBar(staminaText, ref staminaFill, "StaminaBar", current, maximum, new Color(0.25f, 0.85f, 0.35f, 1f)); // 실제 스태미나 막대 갱신
            if (staminaText != null)
            {
                staminaText.text = HUDTextFormatter.FormatStamina(current, maximum);
            }
        }

        private static void SetBar(Text label, ref RectTransform fill, string name, int current, int maximum, Color color) // 실제 HUD 막대 구성과 비율 갱신
        { // 메서드 시작
            if (!Application.isPlaying || label == null) // 실제 실행과 글자 연결 확인
            { // 조건 시작
                return; // 편집 모드와 미설정 HUD 무시
            } // 조건 끝
            if (fill != null && (fill.parent == null || fill.parent.parent != label.transform)) // 이전 글자에 연결된 막대 확인
            { // 조건 시작
                if (fill.parent != null) // 실제 이전 막대 확인
                { // 조건 시작
                    Destroy(fill.parent.gameObject); // 이전 막대 제거
                } // 조건 끝
                fill = null; // 이전 채움 참조 초기화
            } // 조건 끝
            if (fill == null) // 실제 막대 생성 여부 확인
            { // 조건 시작
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
            } // 조건 끝
            float ratio = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f; // 안전한 현재 비율
            fill.anchorMax = new Vector2(ratio, 1f); // 실제 채움 너비 갱신
            fill.gameObject.SetActive(ratio > 0f); // 빈 막대 채움 숨김
        } // 메서드 끝

        private void ShowDebugDefaults()
        {
            SetHealth(100, 100);
            SetBodyCount(3, 20);
            SetExperience(0, 10);
            SetLevel(1);
            SetStamina(100, 100);
        }
    }
}
