# 94일차: 화상과 직접 피격 표시 공존

- 실제 화상 지속 피해가 직접 피격의 빨간 표시를 덮고, 화상 해제 시 색상이 잘못 복원될 수 있던 문제를 수정했다.
- 화상과 부식 연소의 지속 피해는 기존 피해·반응·사망 이벤트를 유지하되 피격 점멸을 매 프레임 다시 시작하지 않는다. 직접 피해가 들어오면 빨간색을 우선 표시하고 종료 후 화상 또는 원래 색상을 복원한다.
- 지속 피해 알림 중 다른 직접 피해가 중첩되어도 각각의 피해 종류와 반응 상태가 복원된다.

## Unity 비대화형 배치

- `AutomatedDay94BurnFlashVerification`: 수정 전 화상이 빨간 피격색을 덮는 실패를 재현했다. 수정 후 직접 피격, 화상 유지, 피격 중 해제와 원래 색 복원 및 중첩 피해 상태를 통과했다.
- `AutomatedFirePoisonReactionVerification`, `AutomatedFireballGradeVerification`, `AutomatedRemainingReactionVerification`: 화상·화염탄·속성 반응 회귀를 통과했다.
- `AutomatedDay82MotionVerification`, `AutomatedDay93StatusIndicatorVerification`, `AutomatedBossOutcomeRaceVerification.RunVictory`: 적 모션, 냉기·독 표시, 보스 승리 경합 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 플레이 화면에서 화상과 빨간 피격색의 시인성, 동시에 적이 많은 경우의 표현은 확인하지 못했다.
