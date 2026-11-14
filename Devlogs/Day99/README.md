# 99일차: 개미굴 소환의 꼬리 보호

- 개미굴이 소환 후보를 검사할 때 활성 꼬리에도 기존 몸통과 같은 1.5 거리의 보호 범위를 적용한다. 머리·몸통·장애물 확인과 일반 적 수 상한은 유지한다.
- 여덟 후보가 모두 안전하지 않으면 기존처럼 해당 소환 기회를 소비한다. 꼬리에서 벗어난 후보에는 기존 추적자와 경험치 보상 경로를 사용한다.

## Unity 비대화형 배치

- `AutomatedDay99AntHillTailVerification`: 실제 소환 단계의 후보 일곱 개를 장애물로 막고 마지막 후보에 꼬리를 둔 통제 시험에서 수정 전 소환을 재현했다. 수정 후 꼬리 보호, 같은 후보의 안전 소환과 일반 적 상한을 통과했다. 자연 예고부터의 장시간 플레이는 실행하지 않았다.
- `AutomatedDay98CollapseTailVerification`, `AutomatedMapFeatureGenerationVerification`, `AutomatedDay86WorldEventsPlayVerification`: 붕괴 잔해 꼬리 보호, 세 맵 32개 시드, 황충·낙석과 보스 전환 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 플레이에서 긴 몸통과 다수 적이 섞인 개미굴 주변의 소환 빈도, 화면 가독성과 회피 가능성은 확인하지 못했다.
