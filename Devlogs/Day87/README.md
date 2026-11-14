# 87일차: 화면 밖 낙석 방향 안내

- 낙석 예고 중 착탄 중심이 전투 카메라 밖으로 이동하면 기존 경고판에 왼쪽·오른쪽·위·아래와 대각선 방향을 표시한다. 화면에 다시 들어오거나 카메라가 없으면 기본 낙석 경고를 표시한다.
- 기존 예고 게이지와 위험 시간, 피해, 착탄 위치, 잔해 생성 및 황충 경고는 유지한다. 착탄 경계선 전체가 언제나 화면 안에 표시되는 기능은 아니다.

## Unity 비대화형 배치

- `AutomatedDay87RockfallWarningVerification`: 실제 게임 씬에서 카메라 이동에 따른 화면 안과 화면 밖 8방향, 화면 복귀, 일시정지, 황충 경고를 통과했다. 수정 전에는 화면 밖에서도 방향이 표시되지 않아 실패했다.
- `AutomatedDay86WorldEventsPlayVerification`: 평원에서 황충·낙석의 경고, 피해, 무적, 안전한 잔해 및 보스 전환 회귀를 통과했다.
- `AutomatedDay86EventScheduleVerification`, `AutomatedCollisionPriorityVerification`: 시간 단축 자동 출현과 실제 충돌 우선순위 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 화면에서 한글 방향 글자의 크기와 겹침, 다양한 해상도 및 이동 중 경고 가독성을 확인한다. 배치의 UI 상태 검증은 실제 화면 확인이 아니다.
