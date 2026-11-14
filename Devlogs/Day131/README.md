# 131일차: 보상 카드 포탑 이미지 배치

- 보상 카드 포탑 이미지의 기본 크기를 96에서 192로 늘리고 카드 중앙 쪽으로 옮겼다.
- 이미지 아래에 문구를 배치한다. 작은 카드에서는 이미지가 카드와 문구를 침범하지 않도록 크기를 줄이고, 카드 크기가 바뀌면 다시 계산한다.
- 이미지가 없는 보상 화면으로 전환하면 원래 문구 영역을 복구한다. 기존 카드 선택과 정예 보상 처리도 유지한다.

## Unity 비대화형 배치

- `AutomatedRewardLayoutVerification`: 변경 전 기존 이미지 크기에서 실패했고, 변경 후 세 카드의 크기·위치·문구 분리, 작은 카드와 크기 변경, 문구 복구가 통과했다.
- `AutomatedCoreArtVerification`, `AutomatedEliteRewardVerification`, `AutomatedDay90MapStartingSelectionVerification`: 기존 무기 이미지, 정예 보상 선택, 맵 진입·시작 선택·재도전이 통과했다.
- 실제 Unity 화면에서 다른 해상도의 시각적 크기와 문구 가독성은 확인하지 못했다.
