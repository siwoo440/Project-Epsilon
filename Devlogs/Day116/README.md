# 116일차: 안전한 보스 출현

- 최종 보스의 기존 위쪽 출현 지점이 장애물이나 뱀에 겹치면 기존 거리의 주변 후보를 차례로 확인한다.
- 보스 충돌 범위와 실제 머리·몸통·꼬리 위치를 확인한다. 모든 후보가 막히면 겹쳐 생성하지 않고 전투가 진행되는 동안 0.25초마다 재시도한다.
- 이는 출현 시점의 겹침 방지이며 모든 경로의 탈출이나 실제 화면 가독성 보장은 아니다.

## Unity 비대화형 배치

- `AutomatedDay116BossSafeSpawnVerification.RunAlternate`·`.RunBlocked`: 수정 전 장애물 위 출현이 재현됐고, 수정 후 대체 후보와 전체 차단 뒤 재시도가 통과했다.
- `AutomatedDay116BossSafeSpawnVerification.RunTail`: 첫 시험은 몸통 관리자를 잘못 조회해 실패했다. 조회를 수정한 뒤 충돌체 없는 꼬리 위 출현을 재현했고, 위치 검사 보완 후 통과했다.
- `AutomatedDay113EnemyMotionProfileVerification`, `AutomatedDay82MotionVerification`, `AutomatedBossOutcomeRaceVerification.RunVictory`, `AutomatedDay77MusicVerification`: 기존 적·보스 모션, 보스 승리 경계와 음악 전환이 통과했다.
- `AutomatedDay86WorldEventsPlayVerification`: 무작위 실행 한 번은 낙석의 안전 후보를 찾지 못해 실패했다. 다른 무작위 실행과 고정 시드 116은 통과했다. 무작위 배치에 의존하는 시험의 재현성 개선은 별도로 필요하다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 세 맵에서 긴 몸통과 혼전 상태의 직접 조작, 보스 등장 위치와 경고 가독성은 확인하지 못했다.
