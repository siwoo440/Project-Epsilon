using UnityEngine; // 유니티 기능
using UnityEngine.UI; // 실제 버튼과 이미지

namespace ProjectEpsilon.UI // 화면 영역
{ // 영역 시작
    public sealed class WeaponCandidateArtwork : MonoBehaviour // 무기 후보 버튼 이미지
    { // 클래스 시작
        private Image image; // 실제 후보 이미지
        private Text currentLabel; // 실제 후보 문구
        private Vector2 originalOffsetMax; // 기존 문구 위쪽 여백
        private bool hasOriginalOffset; // 기존 문구 크기 저장 여부

        public static void Set(Button button, Text label, Sprite sprite) // 실제 후보 이미지 설정
        { // 메서드 시작
            if (button == null) // 실제 버튼 확인
            { // 조건 시작
                return; // 버튼 없는 표시 생략
            } // 조건 끝
            WeaponCandidateArtwork artwork = button.GetComponent<WeaponCandidateArtwork>(); // 기존 후보 표시 조회
            if (artwork == null && sprite == null) // 표시기와 이미지 부재 확인
            { // 조건 시작
                return; // 불필요한 표시기 생성 방지
            } // 조건 끝
            if (artwork == null) // 실제 표시기 부재 확인
            { // 조건 시작
                artwork = button.gameObject.AddComponent<WeaponCandidateArtwork>(); // 실제 후보 표시기 생성
            } // 조건 끝
            artwork.Apply(label, sprite); // 실제 후보와 문구 반영
        } // 메서드 끝

        private void Apply(Text label, Sprite sprite) // 실제 이미지와 문구 영역 갱신
        { // 메서드 시작
            if (currentLabel != label) // 실제 문구 연결 변경 확인
            { // 조건 시작
                RestoreLabel(); // 이전 문구 영역 복구
                currentLabel = label; // 현재 문구 저장
                hasOriginalOffset = currentLabel != null; // 실제 문구 존재 저장
                if (hasOriginalOffset) // 실제 문구 확인
                { // 조건 시작
                    originalOffsetMax = currentLabel.rectTransform.offsetMax; // 기존 위쪽 여백 보존
                } // 조건 끝
            } // 조건 끝
            if (sprite != null && image == null) // 실제 이미지 표시기 부재 확인
            { // 조건 시작
                GameObject item = new GameObject("WeaponArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // 실제 UI 이미지 생성
                item.transform.SetParent(transform, false); // 실제 후보 버튼 아래 연결
                RectTransform area = item.GetComponent<RectTransform>(); // 실제 이미지 영역
                area.anchorMin = new Vector2(0.5f, 1f); // 버튼 위 중앙 기준점
                area.anchorMax = new Vector2(0.5f, 1f); // 버튼 위 중앙 끝점
                area.pivot = new Vector2(0.5f, 1f); // 이미지 위 중앙 기준
                area.anchoredPosition = new Vector2(0f, -4f); // 버튼 위쪽 안쪽 위치
                area.sizeDelta = new Vector2(96f, 96f); // 기존 두 배 후보 이미지 크기
                image = item.GetComponent<Image>(); // 실제 UI 이미지 조회
                image.raycastTarget = false; // 버튼 입력 가림 방지
                image.preserveAspect = true; // 원본 이미지 비율 유지
            } // 조건 끝
            if (image != null) // 실제 이미지 표시기 확인
            { // 조건 시작
                image.sprite = sprite; // 실제 무기 이미지 설정
                image.gameObject.SetActive(sprite != null); // 이미지 존재 시 표시
            } // 조건 끝
            if (currentLabel != null && hasOriginalOffset) // 실제 문구 영역 확인
            { // 조건 시작
                currentLabel.rectTransform.offsetMax = sprite != null ? originalOffsetMax - new Vector2(0f, 100f) : originalOffsetMax; // 두 배 이미지 공간 확보 또는 원래 영역 복구
            } // 조건 끝
        } // 메서드 끝
        private void RestoreLabel() // 이전 문구 영역 복구
        { // 메서드 시작
            if (currentLabel != null && hasOriginalOffset) // 실제 저장 문구 확인
            { // 조건 시작
                currentLabel.rectTransform.offsetMax = originalOffsetMax; // 기존 문구 위쪽 여백 복구
            } // 조건 끝
        } // 메서드 끝
    } // 클래스 끝
} // 영역 끝
