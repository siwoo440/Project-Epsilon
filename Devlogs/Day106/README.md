# 106일차: 화상 처치 폭발의 승리 경계

- 화상 상태의 최종 보스 사망으로 이미 승리가 확정된 경우, 이후 사망 이벤트에서 처치 폭발을 시작하지 않는다.
- 일반 적의 처치 폭발이 보스를 처치하면 그 전까지 처리한 피해는 유지하고 이후 폭발 대상의 피해를 중단한다. 일반 전투 중 폭발과 중첩 폭발 방지 규칙은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay106BurnDeathVictoryVerification.RunBossBurn`, `.RunBossInBlast`: 수정 전 실제 사망 이벤트에서 두 종류의 승리 후 폭발 피해를 재현했다. 수정 후 두 경계가 통과했다.
- `AutomatedDay106BurnDeathVictoryVerification.RunNormal`: 일반 전투 중 실제 화상 처치 폭발 피해를 통과했다.
- `AutomatedFireballGradeVerification`, `AutomatedFirePoisonReactionVerification`, `AutomatedDay94BurnFlashVerification`: 기존 화염탄 등급·화염 반응·피격 표시 회귀를 통과했다.
- `AutomatedDay105VictoryProjectileVerification.RunChainSecondary`, `AutomatedBossOutcomeRaceVerification.RunVictory`: 전기 연쇄와 승리 우선 경합 회귀를 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 자연 플레이에서 화상 폭발의 승리 직후 화면 연출과 다른 처치 연쇄 효과는 확인하지 못했다.
