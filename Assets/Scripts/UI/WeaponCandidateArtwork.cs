using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 버튼과 이미지

namespace ProjectEpsilon.UI // 화면 영역
{
    public sealed class WeaponCandidateArtwork : MonoBehaviour // 무기 후보 버튼 이미지
    {
        private Image image; // 실제 후보 이미지
        private Text currentLabel; // 실제 후보 문구
        private Vector2 originalOffsetMax; // 기존 문구 위쪽 여백
        private bool hasOriginalOffset; // 기존 문구 크기 저장 여부

        public static void Set(Button button, Text label, Sprite sprite, Color? tint = null) // 실제 후보 이미지와 속성 색상 설정
        {
            if (button == null) // 실제 버튼 확인
            {
                return; // 버튼 없는 표시 생략
            }
            WeaponCandidateArtwork artwork = button.GetComponent<WeaponCandidateArtwork>(); // 기존 후보 표시 조회
            if (artwork == null && sprite == null) // 표시기와 이미지 부재 확인
            {
                return; // 불필요한 표시기 생성 방지
            }
            if (artwork == null) // 실제 표시기 부재 확인
            {
                artwork = button.gameObject.AddComponent<WeaponCandidateArtwork>(); // 실제 후보 표시기 생성
            }
            artwork.Apply(label, sprite, tint ?? Color.white); // 실제 후보와 속성 색상 반영
        }

        private void Apply(Text label, Sprite sprite, Color tint) // 실제 이미지와 문구 영역 갱신
        {
            if (currentLabel != label) // 실제 문구 연결 변경 확인
            {
                RestoreLabel(); // 이전 문구 영역 복구
                currentLabel = label; // 현재 문구 저장
                hasOriginalOffset = currentLabel != null; // 실제 문구 존재 저장
                if (hasOriginalOffset) // 실제 문구 확인
                {
                    originalOffsetMax = currentLabel.rectTransform.offsetMax; // 기존 위쪽 여백 보존
                }
            }
            if (sprite != null && image == null) // 실제 이미지 표시기 부재 확인
            {
                GameObject item = new GameObject("WeaponArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // 실제 UI 이미지 생성
                item.transform.SetParent(transform, false); // 실제 후보 버튼 아래 연결
                RectTransform area = item.GetComponent<RectTransform>(); // 실제 이미지 영역
                area.anchorMin = new Vector2(0.5f, 0.62f); // 카드 중앙 쪽 기준점
                area.anchorMax = new Vector2(0.5f, 0.62f); // 카드 중앙 쪽 끝점
                area.pivot = new Vector2(0.5f, 0.5f); // 이미지 가운데 기준
                area.anchoredPosition = Vector2.zero; // 실제 카드 기준 위치
                area.sizeDelta = new Vector2(192f, 192f); // 기존 이미지의 두 배 크기
                image = item.GetComponent<Image>(); // 실제 UI 이미지 조회
                image.raycastTarget = false; // 버튼 입력 가림 방지
                image.preserveAspect = true; // 원본 이미지 비율 유지
            }
            if (image != null) // 실제 이미지 표시기 확인
            {
                image.sprite = sprite; // 실제 무기 이미지 설정
                image.color = tint; // 무기 속성별 포탑 색상 설정
                image.gameObject.SetActive(sprite != null); // 이미지 존재 시 표시
            }
            if (sprite != null) // 실제 이미지 표시 확인
            { // 조건 시작
                UpdateLayout(); // 카드 크기와 문구 영역 갱신
            } // 조건 종료
            else // 이미지 없는 화면 확인
            { // 대체 시작
                RestoreLabel(); // 기존 문구 영역 복구
            } // 대체 종료
        }
        private void OnRectTransformDimensionsChange() // 실제 카드 크기 변경
        { // 함수 시작
            UpdateLayout(); // 크기별 이미지와 문구 갱신
        } // 함수 종료
        private void UpdateLayout() // 카드 안쪽 이미지와 문구 배치
        { // 함수 시작
            if (image == null || image.sprite == null || !image.gameObject.activeSelf) // 실제 표시 이미지 확인
            { // 조건 시작
                return; // 이미지 없는 화면 유지
            } // 조건 종료
            RectTransform card = transform as RectTransform; // 실제 카드 영역
            if (card == null || card.rect.width <= 0f || card.rect.height <= 0f) // 유효한 카드 크기 확인
            { // 조건 시작
                return; // 카드 배치 전 갱신 생략
            } // 조건 종료
            float height = card.rect.height; // 실제 카드 높이
            float verticalOffset = Mathf.Min(30f, Mathf.Max(0f, height * 0.38f - 8f)); // 카드 밖을 피하는 최대 삼십 상향
            image.rectTransform.anchoredPosition = new Vector2(0f, verticalOffset); // 실제 포탑 이미지 상향 위치
            float imageCenter = height * 0.62f + verticalOffset; // 카드 아래 기준 이미지 중심
            float availableHalf = Mathf.Min(height - imageCenter - 8f, imageCenter - 96f); // 이미지와 문구의 최소 여백
            float size = Mathf.Min(192f, Mathf.Max(0f, card.rect.width - 16f), Mathf.Max(0f, availableHalf * 2f)); // 카드 안쪽의 최대 이미지 크기
            image.rectTransform.sizeDelta = new Vector2(size, size); // 실제 비율별 이미지 크기
            if (currentLabel != null && hasOriginalOffset) // 실제 문구 영역 확인
            { // 조건 시작
                float imageBottom = imageCenter - size * 0.5f; // 카드 아래 기준 이미지 경계
                float labelTop = Mathf.Min(originalOffsetMax.y, imageBottom - height - 12f); // 이미지 아래 문구 위쪽 경계
                currentLabel.rectTransform.offsetMax = new Vector2(originalOffsetMax.x, labelTop); // 실제 문구와 이미지 분리
            } // 조건 종료
        } // 함수 종료
        private void RestoreLabel() // 이전 문구 영역 복구
        {
            if (currentLabel != null && hasOriginalOffset) // 실제 저장 문구 확인
            {
                currentLabel.rectTransform.offsetMax = originalOffsetMax; // 기존 문구 위쪽 여백 복구
            }
        }
    }
}
