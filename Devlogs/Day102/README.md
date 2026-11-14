# 102일차: 자동 전진 중 회복 아이템 통과 획득

- 체력 회복과 몸통 복구 아이템에 101일차의 실제 자동 전진 구간 검사를 연결한다. 이동 경로 밖과 강제 위치 변경 경로는 제외한다.
- 완전 회복 상태, 복구할 몸통이 없는 상태와 보스전 복구 제한 등 기존 적용 가능 조건은 그대로 유지한다.

## Unity 비대화형 배치

- `AutomatedDay102RecoveryTraversalVerification`: 수정 전 시험용 고속 자동 전진에서 회복·복구 아이템 획득 누락을 재현했다. 수정 후 실제 회복과 몸통 복구, 경로 밖 아이템, 적용 불가 상태와 강제 위치 변경 제외를 통과했다.
- `AutomatedBossRepairVerification`, `AutomatedBodyGrowthVerification`: 기존 보스전 몸통 복구 제한과 연속 레벨업·몸통 규칙 회귀를 통과했다.
- `AutomatedDay101ExperienceTraversalVerification`, `AutomatedMapMovementEffectsVerification`: 경험치 보석 통과 획득과 바람·진흙 등 맵 이동 효과 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 플레이의 일반 속도·낮은 프레임, 다수 아이템 환경에서 획득 표현과 성능은 확인하지 못했다. 시험의 고속 이동은 재현용 값이다.
