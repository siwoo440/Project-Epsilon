# 113일차: 적 행동별 임시 모션

- 기존 스프라이트와 충돌체에서 분리된 표시 객체를 사용해 비행·장갑·돌진·사격·포식·지대 생성·엘리트·보스의 이동 또는 공격 움직임을 구분했다.
- 움직임의 속도·기울기·크기는 `EnemyVisualMotion`의 행동별 시험값이며, 게임 속도·피해·충돌·예고를 바꾸지 않는다. 별도 프레임 애니메이션은 아니다.

## Unity 비대화형 배치

- `AutomatedDay113EnemyMotionProfileVerification`: 수정 전 유형별 움직임 차이 누락이 재현됐다. 수정 후 실제 적 생성의 비행·추적·장갑 모션 차이, 충돌체 보존 및 실제 보스 전용 설정이 통과했다.
- `AutomatedDay82MotionVerification`, `AutomatedDay94BurnFlashVerification`: 기존 적·보스 공격과 사망 표시, 피격·화상 색상 및 충돌체 보존이 통과했다.
- `AutomatedDay79EnemyVerification`, `AutomatedDay81EliteVerification`, `AutomatedBossOutcomeRaceVerification.RunVictory`: 일반 적 행동, 엘리트 압박, 보스 승리 경계가 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 화면에서 적별 가독성과 보스 공격 예고가 모션에 가려지지 않는지 확인하지 못했다. 적별 전용 프레임 애니메이션과 최종 연출값은 미완료다.
