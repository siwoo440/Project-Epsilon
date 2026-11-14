# 111일차: 명암 상쇄의 승리 경계

- 신성·암흑 반응 피해로 최종 보스를 처치한 뒤 공용 체력이 회복되지 않도록 했다.
- 일반 전투의 명암 반응 피해와 회복은 유지한다.

## Unity 비대화형 배치

- `AutomatedDay111LightDarkOutcomeVerification.RunVictory`: 수정 전 승리 확정 후 명암 회복이 재현됐고 수정 후 통과했다.
- `AutomatedDay111LightDarkOutcomeVerification.RunNormal`: 일반 적 처치 시 명암 반응의 회복이 정상 적용됐다.
- `AutomatedRemainingReactionVerification`: 명암·성화·부식 반응의 양방향 조합이 통과했다.
- `AutomatedDay110HolyHitOutcomeVerification.RunVictory`, `AutomatedDay109DarkKillVictoryVerification.RunNormal`, `AutomatedDay107ReactionVictoryVerification.RunSacred`·`.RunShattering`, `AutomatedBossOutcomeRaceVerification.RunVictory`: 기존 신성·암흑·범위 반응과 보스 승리 경계가 통과했다.
- 회귀 실행 중 존재하지 않는 `AutomatedDay107ReactionVictoryVerification.Run`을 한 차례 호출해 실패했다. 실제 진입점 `RunSacred`와 `RunShattering`으로 다시 실행해 통과했다.
- Unity 시작 시 기존 검색 데이터베이스 `ArgumentOutOfRangeException`이 기록됐다. 원인은 별도 확인이 필요하다.

## 남은 사항

- 실제 자연 플레이의 보스 처치와 결과 화면은 확인하지 못했다.
