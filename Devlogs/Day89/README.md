# 89일차: 황충떼 연속 이동 충돌

- 황충떼의 이전 물리 판정 위치부터 현재 위치까지 실제 이동 구간을 검사한다. 마지막 이동 구간도 판정한 뒤 표시를 정리한다.
- 기존 공용 HP 피해 예약, 보호막·무적, 피해 폭과 단일 피해 규칙을 유지한다. 이동 속도와 피해 데이터는 바꾸지 않았다.

## Unity 비대화형 배치

- `AutomatedDay89LocustSweepVerification`: 시험용 초고속 황충이 한 프레임에 머리 또는 몸통을 통과해도 한 번 피해를 주고, 무적과 범위 밖 회피를 지키는 것을 확인했다. 수정 전에는 빠른 통과 피해가 누락됐다.
- `AutomatedDay86WorldEventsPlayVerification`, `AutomatedDay86EventScheduleVerification`, `AutomatedDay88LocustWarningVerification`: 기존 황충·낙석 동작, 시간 단축 출현 일정 및 방향 경고 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 화면의 황충 연출과 여러 적이 겹치는 자연 전투 상황은 별도 확인이 필요하다.
- 다음 작업: 맵 진입 후 기존 레벨업 포탑 선택창을 사용한 시작 포탑 선택과 이전 화면 복귀 차단.
