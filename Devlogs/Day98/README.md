# 98일차: 붕괴 잔해의 꼬리 보호

- 폐허 붕괴 예고가 끝날 때 머리와 몸통에 더해 활성 꼬리가 구역 안에 있는지도 확인한다. 뱀 일부가 남아 있으면 일시 장애물 생성을 생략한다.
- 기존 지형 피해의 접촉 판정은 변경하지 않았으며 뱀이 구역에서 빠져나온 안전한 경우의 장애물 생성은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay98CollapseTailVerification`: 변경 전 꼬리 위 붕괴 장애물 생성을 재현했다. 변경 후 꼬리 보호와 안전 위치의 정상 생성을 통과했다.
- `AutomatedMapTimedHazardsVerification`, `AutomatedMapFeatureGenerationVerification`: 방전·스위치·포자·개미굴·기존 붕괴 보호와 세 맵 32개 시드의 배치 회귀를 통과했다.
- `AutomatedDay86WorldEventsPlayVerification`, `AutomatedCollisionPriorityVerification`: 황충·낙석·보스 전환과 실제 충돌 우선순위 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 긴 몸통으로 폐허를 자연 이동할 때의 회전 공간과 실제 화면은 확인하지 못했다. 개미굴 소환 후보의 꼬리 겹침 처리는 별도 확인이 필요하다.
