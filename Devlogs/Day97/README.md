# 97일차: 중첩 상태 표시 정돈

- 실제 활성 상태만 순서대로 모아 한 줄에 최대 세 개씩, 최대 두 줄로 가운데 정렬한다. 상태가 해제되면 남은 표시를 자동으로 재배치한다.
- 기존 이미지, 적 추적, 48마리 표시 상한과 위험 예고보다 낮은 표시 순서는 유지한다. 표시 간격은 시험값이다.

## Unity 비대화형 배치

- `AutomatedDay97StatusLayoutVerification`: 변경 전 단일 표시의 가운데 정렬 부재를 확인했다. 변경 후 한 개·두 개·다섯 개의 줄별 정렬과 상태 해제 후 재배치를 통과했다.
- `AutomatedDay93StatusIndicatorVerification`, `AutomatedDay95CurseIndicatorVerification`, `AutomatedDay96CorrosiveBurnIndicatorVerification`: 기존 상태 표시 상한과 상태별 반응·만료 회귀를 통과했다.
- `AutomatedDay83VfxVerification`: 속성·반응과 플레이어 전투 효과 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 화면에서 상태 표시의 간격·크기, 위험 예고와의 가독성 및 적이 많은 상황의 성능은 확인하지 못했다.
