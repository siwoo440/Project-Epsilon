using UnityEngine; // UI 이미지와 색상
using UnityEngine.UI; // 버튼과 이미지

namespace ProjectEpsilon.UI // 게임 UI 영역
{ // 영역 시작
    public static class ProjectUiSkin // 공통 UI 디자인
    { // 클래스 시작
        private static Sprite buttonSprite; // 버튼 이미지 저장
        private static Sprite cardSprite; // 카드 이미지 저장
        private static Sprite barSprite; // 막대 이미지 저장
        private static readonly Sprite[] attributeSprites = new Sprite[8]; // 실제 여덟 속성 이미지 저장
        private static readonly Sprite[] eliteRewardSprites = new Sprite[4]; // 실제 네 가지 정예 보상 이미지
        private static Sprite Load(ref Sprite cached, string path, Rect normalized, Vector4 border) // 이미지 불러오기와 테두리 구성
        { // 함수 시작
            if (cached != null) // 기존 이미지 확인
            { // 조건 시작
                return cached; // 기존 이미지 재사용
            } // 조건 종료
            Texture2D texture = Resources.Load<Texture2D>(path); // 실제 생성 이미지 조회
            if (texture == null) // 이미지 누락 확인
            { // 조건 시작
                return null; // 기존 UI 유지
            } // 조건 종료
            Rect rect = new Rect(texture.width * normalized.x, texture.height * normalized.y, texture.width * normalized.width, texture.height * normalized.height); // 투명 여백을 제외한 이미지 영역
            Vector4 pixels = new Vector4(rect.width * border.x, rect.height * border.y, rect.width * border.z, rect.height * border.w); // 화면 크기별 테두리 영역
            cached = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, pixels); // 크기 변경 가능한 이미지 생성
            return cached; // 실제 이미지 반환
        } // 함수 종료
        public static void ApplyButton(Button button, bool card = false) // 기존 버튼 또는 보상 카드 꾸미기
        { // 함수 시작
            if (button == null) // 버튼 누락 확인
            { // 조건 시작
                return; // 기존 화면 유지
            } // 조건 종료
            Image image = button.GetComponent<Image>(); // 실제 버튼 배경
            if (image == null) // 이미지 누락 확인
            { // 조건 시작
                return; // 기존 동작 유지
            } // 조건 종료
            Sprite sprite = card ? Load(ref cardSprite, "UI/RewardCardFrame", new Rect(0.17f, 0.07f, 0.66f, 0.86f), new Vector4(0.1f, 0.06f, 0.1f, 0.06f)) : Load(ref buttonSprite, "UI/ButtonFrame", new Rect(0.065f, 0.26f, 0.87f, 0.48f), new Vector4(0.06f, 0.18f, 0.06f, 0.18f)); // 실제 생성 프레임 선택
            if (!Apply(image, sprite, card ? 2f : 3f)) // 실제 이미지 적용 확인
            { // 조건 시작
                return; // 누락 에셋의 기존 화면 유지
            } // 조건 종료
            ColorBlock colors = button.colors; // 기존 버튼 색상 상태
            colors.normalColor = Color.white; // 기본 이미지 색상
            colors.highlightedColor = new Color(0.76f, 1f, 1f, 1f); // 마우스 강조 색상
            colors.selectedColor = new Color(0.76f, 1f, 1f, 1f); // 선택 강조 색상
            colors.pressedColor = new Color(0.58f, 0.8f, 0.95f, 1f); // 누른 상태 색상
            colors.disabledColor = new Color(0.4f, 0.45f, 0.52f, 0.7f); // 비활성 상태 색상
            button.colors = colors; // 실제 버튼 색상 적용
        } // 함수 종료
        public static void ApplySidebar(Image image, bool panel) // 우측 패널과 몸통 행 꾸미기
        { // 함수 시작
            Sprite sprite = panel ? Load(ref cardSprite, "UI/RewardCardFrame", new Rect(0.17f, 0.07f, 0.66f, 0.86f), new Vector4(0.1f, 0.06f, 0.1f, 0.06f)) : Load(ref buttonSprite, "UI/ButtonFrame", new Rect(0.065f, 0.26f, 0.87f, 0.48f), new Vector4(0.06f, 0.18f, 0.06f, 0.18f)); // 패널 또는 행 프레임
            Apply(image, sprite, panel ? 2f : 5f); // 실제 크기 대응 이미지 적용
        } // 함수 종료
        public static void ApplyBar(Image image) // 기존 공용 막대 배경 꾸미기
        { // 함수 시작
            Sprite sprite = Load(ref barSprite, "UI/BarFrame", new Rect(0.03f, 0.39f, 0.94f, 0.22f), new Vector4(0.04f, 0.25f, 0.04f, 0.25f)); // 실제 막대 프레임
            Apply(image, sprite, 4f); // 기존 채움 아래 프레임 적용
        } // 함수 종료
        public static Sprite AttributeIcon(int index) // 실제 속성 순서의 아이콘
        { // 함수 시작
            if (index < 0 || index >= attributeSprites.Length) // 유효한 속성 번호 확인
            { // 조건 시작
                return null; // 잘못된 번호 무시
            } // 조건 종료
            if (attributeSprites[index] != null) // 이미 생성한 아이콘 확인
            { // 조건 시작
                return attributeSprites[index]; // 기존 아이콘 재사용
            } // 조건 종료
            Texture2D atlas = Resources.Load<Texture2D>("UI/AttributeIcons"); // 실제 여덟 속성 이미지
            if (atlas == null) // 이미지 누락 확인
            { // 조건 시작
                return null; // 기존 글자 표시 유지
            } // 조건 종료
            int column = index % 4; // 실제 아이콘 열 번호
            int row = index / 4; // 실제 아이콘 행 번호
            int left = column * atlas.width / 4; // 정수 기준 왼쪽 경계
            int right = (column + 1) * atlas.width / 4; // 정수 기준 오른쪽 경계
            int bottom = (1 - row) * atlas.height / 2; // 정수 기준 아래쪽 경계
            int top = (2 - row) * atlas.height / 2; // 정수 기준 위쪽 경계
            Rect rect = new Rect(left, bottom, right - left, top - bottom); // 속성 순서별 이미지 영역
            attributeSprites[index] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect); // 실제 속성 아이콘 생성
            return attributeSprites[index]; // 실제 아이콘 반환
        } // 함수 종료
        public static Sprite EliteRewardIcon(int kind) // 실제 정예 보상 종류별 아이콘
        { // 함수 시작
            if (kind < 0 || kind >= eliteRewardSprites.Length) // 유효한 보상 종류 확인
            { // 조건 시작
                return null; // 잘못된 보상 표시 생략
            } // 조건 종료
            if (eliteRewardSprites[kind] != null) // 기존 아이콘 확인
            { // 조건 시작
                return eliteRewardSprites[kind]; // 실제 이미지 재사용
            } // 조건 종료
            Texture2D atlas = Resources.Load<Texture2D>("UI/EliteRewardIcons"); // 실제 네 보상 이미지
            if (atlas == null) // 실제 이미지 누락 확인
            { // 조건 시작
                return null; // 기존 보상 글자 유지
            } // 조건 종료
            int left = kind * atlas.width / 4; // 보상별 왼쪽 정수 경계
            int right = (kind + 1) * atlas.width / 4; // 보상별 오른쪽 정수 경계
            Rect rect = new Rect(left, 0, right - left, atlas.height); // 실제 보상 이미지 영역
            eliteRewardSprites[kind] = Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect); // 실제 보상 아이콘 생성
            return eliteRewardSprites[kind]; // 실제 보상 아이콘 반환
        } // 함수 종료
        private static bool Apply(Image image, Sprite sprite, float pixelMultiplier) // 실제 UI 프레임 적용
        { // 함수 시작
            if (image == null || sprite == null) // 이미지와 에셋 확인
            { // 조건 시작
                return false; // 누락 시 기존 화면 유지
            } // 조건 종료
            image.sprite = sprite; // 실제 프레임 연결
            image.type = Image.Type.Sliced; // 화면 크기별 테두리 유지
            image.pixelsPerUnitMultiplier = pixelMultiplier; // 테두리 표시 크기 조정
            image.color = Color.white; // 생성 이미지 원래 색상
            return true; // 실제 적용 성공
        } // 함수 종료
    } // 클래스 종료
} // 영역 종료
