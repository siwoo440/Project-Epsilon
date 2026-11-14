# 101일차: 자동 전진 중 보석 통과 획득

- 실제 자동 전진의 시작점과 종료점을 현재 프레임에 기록하고, 보석이 그 구간의 획득 반경 안에 있으면 기존 경험치 지급 경로로 수집한다.
- 보석 갱신을 실제 자동 전진 뒤에 실행한다. 강제 위치 변경 경로와 지난 프레임의 이동은 획득 구간으로 사용하지 않는다.
- 일반 적·엘리트의 분 단위 소급 능력치는 기존 구현과 시험을 확인하고 재사용했다.

## Unity 비대화형 배치

- `AutomatedDay101ExperienceTraversalVerification`: 시험용 고속 자동 전진에서 수정 전 보석 통과 획득 누락을 재현했다. 수정 후 실제 통과 보석의 단일 지급, 경로 밖 보석과 강제 위치 변경 제외를 통과했다.
- `AutomatedEnemyWaveStagesVerification`: 실제 분 경계의 일반 적과 엘리트 능력치·생존 체력 비율 및 신규 적 배율을 통과했다.
- `AutomatedDay45PickupVerification`, `AutomatedExperienceMergeVerification`, `AutomatedExperienceMergePartialVerification`, `AutomatedGemAccountingVerification`: 기존 이동 중 흡수·보석 병합·경험치 집계를 통과했다. 경험치 기록 시험은 기록 옵션을 넣어 재실행했다.
- `AutomatedCombatProgressionVerification`, `AutomatedBodyGrowthVerification`, `AutomatedMapMovementEffectsVerification`: 자동 공격부터 경험치·레벨업·재개, 연속 레벨업과 기존 맵 이동 효과를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 플레이의 일반 속도·낮은 프레임과 다수 보석 환경에서 획득 표현 및 성능은 확인하지 못했다. 시험의 고속 이동은 재현용 값이다.
