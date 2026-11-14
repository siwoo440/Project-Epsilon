# 103일차: 레벨업과 같은 전진의 아이템 획득

- 첫 경험치 보석이 레벨업 선택을 열어 전투를 정지해도, 같은 프레임의 실제 자동 전진 경로에서 이미 지나간 다른 보석과 회복 아이템은 기존 획득 규칙으로 처리한다.
- 레벨업 정지의 이후 프레임, 일반 일시정지와 게임 종료 상태에서는 새 아이템을 획득하지 않는다. 경험치와 회복 가능 조건은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay103LevelUpTraversalVerification`: 수정 전 첫 보석의 레벨업 정지로 같은 전진 경로의 다른 아이템이 누락되는 현상을 재현했다. 수정 후 두 보석·회복 아이템, 실제 보상 선택과 재개, 이후 정지 및 일반 일시정지의 획득 차단을 통과했다.
- `AutomatedDay101ExperienceTraversalVerification`, `AutomatedDay102RecoveryTraversalVerification`: 기존 고속 보석·회복 통과와 강제 위치 변경 제외 회귀를 통과했다.
- `AutomatedBodyGrowthVerification`, `AutomatedBossRepairVerification`, `AutomatedGemAccountingVerification`: 연속 레벨업·몸통 규칙·보스전 복구 제한·경험치 집계 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 자연 플레이에서 여러 아이템의 동시 획득 연출과 다수 아이템 상황의 성능은 확인하지 못했다. 빠른 이동은 시험용 값이다.
