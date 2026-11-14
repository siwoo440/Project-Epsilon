# 92일차: 직선 포탑 탄의 이동 경로 충돌

- 직선 탄이 한 프레임에 지나간 경로의 적과 엄폐를 충돌 거리순으로 확인한다. 기존 명중 횟수, 속성 효과, 등급별 전달, 중복 명중 방지와 엄폐 처리를 사용한다.
- 실제 충돌 반경과 물리 계층을 적용하며 충돌 결과 목록은 투사체별로 재사용한다. 시험 속도는 재현을 위한 극단적인 값이며 평상시 발생 빈도는 확인되지 않았다.

## Unity 비대화형 배치

- `AutomatedDay92ProjectileSweepVerification`: 수정 전 고속 탄의 적 통과 피해 누락을 재현했다. 수정 후 단일 명중, 엄폐 뒤 적 보호, 거리순 두 대상 관통, 일시정지와 재개를 통과했다.
- `AutomatedMapCoverVerification`, `AutomatedFireballGradeVerification`: 실제 생성 엄폐물과 화염탄 등급별 전달·속성·관통 회귀를 통과했다.
- `AutomatedBossOutcomeRaceVerification.RunVictory`, `.RunDefeat`: 보스 승리와 플레이어 패배 경합 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 플레이에서 낮은 프레임, 빠르게 움직이는 적과 다수 포탑의 시각적 명중 및 성능을 확인한다. 이 배치는 움직이는 표적의 모든 경로와 실제 표시 품질을 검증하지 않았다.
