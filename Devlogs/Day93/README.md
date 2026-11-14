# 93일차: 냉기·독 지속 상태 표시

- 실제 적에 냉기 또는 독 상태가 유지되는 동안 83일차 속성 이미지로 작은 표시를 띄운다. 두 상태가 겹치면 함께 표시하고 적을 따라 움직인다. 기존 상태 수치·피해·적 이미지 색상은 변경하지 않는다.
- 한 번에 최대 48마리의 표시를 허용하는 시험값을 사용한다. 자리가 생기면 대기 중인 적도 표시하며 상태 해제·만료·적 비활성화·사망 시 표시와 자리를 회수한다. 공격 예고보다 낮은 순서에 표시한다.

## Unity 비대화형 배치

- `AutomatedDay93StatusIndicatorVerification`: 변경 전 동시 표시 누락을 확인했다. 변경 후 냉기·독 동시 표시, 위치 추적, 정지, 개별 해제, 지속 시간 만료, 48마리 상한과 자리 재사용을 통과했다.
- `AutomatedFirePoisonReactionVerification`, `AutomatedRemainingReactionVerification`, `AutomatedColdFreezeVerification`: 기존 상태 반응과 빙결·보스 저항 회귀를 통과했다.
- `AutomatedDay83VfxVerification`, `AutomatedDay82MotionVerification`: 기존 전투 효과와 적·보스 모션 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 해상도에서 표시 크기·위치·경고선 가독성과 적 다수의 성능은 확인하지 못했다. 화상은 기존 색상 점멸을 유지하며 그 밖의 지속 상태 전용 표시는 아직 추가하지 않았다.
