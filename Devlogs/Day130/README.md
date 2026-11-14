# 130일차: 단계별 조작 안내 그림

- 이동·자기 충돌·몸통 체력·자동 공격·몸통 포탑·합성·속성 시너지·가속의 여덟 안내 그림을 투명 아틀라스로 생성했다.
- 기존 8단계 안내의 문구와 현재 키 설정을 유지하면서 단계에 맞는 이미지를 표시한다. 이미지가 없으면 기존 글자 배치를 사용하며 그림은 입력을 가리지 않는다.

## Unity 비대화형 배치

- `AutomatedDay126UiSkinVerification`: 변경 전 안내 그림이 없어 실패했고, 변경 후 8단계별 이미지 순서·입력 보존과 공통 UI가 통과했다.
- `AutomatedDay75TutorialVerification`: 별도 테스트 저장 공간에서 첫 안내 8단계·완료·저장·다시 보기·ESC·일시정지 소유가 통과했다.
- `AutomatedEliteRewardVerification`, `AutomatedDay90MapStartingSelectionVerification`: 정예 보상 선택과 메뉴·맵·시작 선택·결과·재도전 흐름이 통과했다.
- 실제 화면에서 이미지·글자 간격과 서로 다른 해상도의 가독성은 확인하지 못했다.
