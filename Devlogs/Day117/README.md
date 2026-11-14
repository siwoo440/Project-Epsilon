# 117일차: 안전 낙석 재시도

- 안전한 착탄 후보가 없는 경우 같은 위험을 조정 가능한 3초 시험 간격으로 재시도한다. 위험이 실제 시작된 뒤에는 기존 90초 간격과 황충·낙석 교대를 유지한다.
- 세 맵 데이터에 재시도 시험값을 추가했으며 화면 안 착탄, 안전 통로, 장애물·몸통 보호 및 보스전 위험 중단은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay117RockfallRetryVerification`: 수정 전 후보 부재 후 전체 출현 간격이 적용되는 문제가 재현됐다. 수정 후 세 맵의 데이터, 안전 후보 부재 대기, 후보 복구 후 낙석 예고와 정상 간격 복귀가 통과했다.
- 고정 시드 116의 `AutomatedDay86EventScheduleVerification`, `AutomatedDay86WorldEventsPlayVerification`: 기존 황충·낙석 교대 일정, 경고·피해·무적·안전 잔해와 보스 전환이 통과했다.
- `AutomatedDay116BossSafeSpawnVerification.RunBlocked`: 보스 안전 후보 재시도가 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 3초 재시도는 임시값이다. 실제 혼전과 서로 다른 화면 크기에서 착탄 위치의 빈도와 경고 가독성은 확인하지 못했다.
